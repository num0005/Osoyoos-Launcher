using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using ToolkitLauncher.ToolkitInterface;
using Windows.Win32;
using Microsoft.Win32.SafeHandles;
using Microsoft.Win32;
using Windows.Win32.System.Console;
using Windows.Win32.Security;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;
using System.ComponentModel;
using System.Security.Permissions;
using System.Runtime.Versioning;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;

namespace ToolkitLauncher.Utility
{
	public static partial class Process
    {
		#region code_copied_from_dotnet_runtime
		private static readonly object s_createProcessLock = new();

		private static int GetShowWindowFromWindowStyle(ProcessWindowStyle windowStyle) => windowStyle switch
		{
			ProcessWindowStyle.Hidden => 0, // SW_HIDE
			ProcessWindowStyle.Minimized => 2, // SW_SHOWMINIMIZED
			ProcessWindowStyle.Maximized => 3, // SW_SHOWMAXIMIZED
			_ => 1, // SW_SHOWNORMAL
		};

		private static Encoding GetEncoding(int code_page)
		{
			if (code_page == 1200) // utf-16
				return Encoding.Unicode;
			if (code_page == 1201) // utf-16 BE
				return Encoding.BigEndianUnicode;
			return Encoding.UTF8;
		}

		// Using synchronous Anonymous pipes for process input/output redirection means we would end up
		// wasting a worker threadpool thread per pipe instance. Overlapped pipe IO is desirable, since
		// it will take advantage of the NT IO completion port infrastructure. But we can't really use
		// Overlapped I/O for process input/output as it would break Console apps (managed Console class
		// methods such as WriteLine as well as native CRT functions like printf) which are making an
		// assumption that the console standard handles (obtained via GetStdHandle()) are opened
		// for synchronous I/O and hence they can work fine with ReadFile/WriteFile synchronously!
		[SupportedOSPlatform("windows5.1.2600")]
		private static void CreatePipe(out SafeFileHandle parentHandle, out SafeFileHandle childHandle, bool parentInputs)
		{
			SECURITY_ATTRIBUTES securityAttributesParent = default;
			securityAttributesParent.bInheritHandle = true;

			SafeFileHandle? hTmp = null;
			try
			{
				if (parentInputs)
				{
					PInvoke.CreatePipe(out childHandle, out hTmp, securityAttributesParent, 0);
				}
				else
				{
					PInvoke.CreatePipe(out hTmp, out childHandle, securityAttributesParent, 0);
				}
				// Duplicate the parent handle to be non-inheritable so that the child process
				// doesn't have access. This is done for correctness sake, exact reason is unclear.
				// One potential theory is that child process can do something brain dead like
				// closing the parent end of the pipe and there by getting into a blocking situation
				// as parent will not be draining the pipe at the other end anymore.
				SafeFileHandle currentProcHandle = PInvoke.GetCurrentProcess_SafeHandle();
				if (!PInvoke.DuplicateHandle(currentProcHandle,
													 hTmp,
													 currentProcHandle,
													 out parentHandle,
													 0,
													 false,
													 DUPLICATE_HANDLE_OPTIONS.DUPLICATE_SAME_ACCESS))
				{
					throw new Win32Exception();
				}
			}
			finally
			{
				if (hTmp != null && !hTmp.IsInvalid)
				{
					hTmp.Dispose();
				}
			}
		}

		private static string GetEnvironmentVariablesBlock(IDictionary<string, string?> sd)
		{
			// https://learn.microsoft.com/windows/win32/procthread/changing-environment-variables
			// "All strings in the environment block must be sorted alphabetically by name. The sort is
			//  case-insensitive, Unicode order, without regard to locale. Because the equal sign is a
			//  separator, it must not be used in the name of an environment variable."

			var keys = new string[sd.Count];
			sd.Keys.CopyTo(keys, 0);
			Array.Sort(keys, StringComparer.OrdinalIgnoreCase);

			// Join the null-terminated "key=val\0" strings
			var result = new StringBuilder(8 * keys.Length);
			foreach (string key in keys)
			{
				string? value = sd[key];

				// Ignore null values for consistency with Environment.SetEnvironmentVariable
				if (value != null)
				{
					result.Append(key).Append('=').Append(value).Append('\0');
				}
			}

			return result.ToString();
		}

		/// <summary>Starts the process using the supplied start info.</summary>
		/// <param name="startInfo">The start info with which to start the process.</param>
		[SupportedOSPlatform("windows5.1.2600")]
		static private unsafe System.Diagnostics.Process? StartWithCreateProcess(ProcessStartInfo startInfo, bool launchSuspended = false)
		{
			Trace.WriteLine($"StartWithCreateProcess - forked from dotnet runtime code startInfo: {startInfo} launchSuspended:{launchSuspended}");
			// See knowledge base article Q190351 for an explanation of the following code.  Noteworthy tricky points:
			//    * The handles are duplicated as non-inheritable before they are passed to CreateProcess so
			//      that the child process can not close them
			//    * CreateProcess allows you to redirect all or none of the standard IO handles, so we use
			//      GetStdHandle for the handles that are not being redirected

			string commandLine = escape_arg(startInfo.FileName, is_first: true);
			if (startInfo.ArgumentList.Count > 0)
			{
				commandLine += Process.EscapeArgList(startInfo.ArgumentList);
			}

			STARTUPINFOW startupInfo = default;
			PROCESS_INFORMATION processInfo = default;
			SECURITY_ATTRIBUTES* unused_SecAttrs = null;
			SafeProcessHandle procSH = new SafeProcessHandle();

			// handles used in parent process
			SafeFileHandle? parentInputPipeHandle = null;
			SafeFileHandle? childInputPipeHandle = null;
			SafeFileHandle? parentOutputPipeHandle = null;
			SafeFileHandle? childOutputPipeHandle = null;
			SafeFileHandle? parentErrorPipeHandle = null;
			SafeFileHandle? childErrorPipeHandle = null;

			// Take a global lock to synchronize all redirect pipe handle creations and CreateProcess
			// calls. We do not want one process to inherit the handles created concurrently for another
			// process, as that will impact the ownership and lifetimes of those handles now inherited
			// into multiple child processes.
			lock (s_createProcessLock)
			{
				try
				{
					startupInfo.cb = (uint)sizeof(STARTUPINFOW);

					// set up the streams
					if (startInfo.RedirectStandardInput || startInfo.RedirectStandardOutput || startInfo.RedirectStandardError)
					{
						if (startInfo.RedirectStandardInput)
						{
							CreatePipe(out parentInputPipeHandle, out childInputPipeHandle, true);
						}
						else
						{
							childInputPipeHandle = new SafeFileHandle(PInvoke.GetStdHandle(STD_HANDLE.STD_INPUT_HANDLE), false);
						}

						if (startInfo.RedirectStandardOutput)
						{
							CreatePipe(out parentOutputPipeHandle, out childOutputPipeHandle, false);
						}
						else
						{
							childOutputPipeHandle = new SafeFileHandle(PInvoke.GetStdHandle(STD_HANDLE.STD_OUTPUT_HANDLE), false);
						}

						if (startInfo.RedirectStandardError)
						{
							CreatePipe(out parentErrorPipeHandle, out childErrorPipeHandle, false);
						}
						else
						{
							childErrorPipeHandle = new SafeFileHandle(PInvoke.GetStdHandle(STD_HANDLE.STD_ERROR_HANDLE), false);
						}

						startupInfo.hStdInput = (HANDLE)childInputPipeHandle.DangerousGetHandle();
						startupInfo.hStdOutput = (HANDLE)childOutputPipeHandle.DangerousGetHandle();
						startupInfo.hStdError = (HANDLE)childErrorPipeHandle.DangerousGetHandle();

						startupInfo.dwFlags = STARTUPINFOW_FLAGS.STARTF_USESTDHANDLES;
					}

					if (startInfo.WindowStyle != ProcessWindowStyle.Normal)
					{
						startupInfo.wShowWindow = (ushort)GetShowWindowFromWindowStyle(startInfo.WindowStyle);
						startupInfo.dwFlags |= STARTUPINFOW_FLAGS.STARTF_USESHOWWINDOW;
					}

					// set up the creation flags parameter
					PROCESS_CREATION_FLAGS creationFlags = 0;
					if (startInfo.CreateNoWindow) creationFlags |= PROCESS_CREATION_FLAGS.CREATE_NO_WINDOW;

					creationFlags |= PROCESS_CREATION_FLAGS.CREATE_UNICODE_ENVIRONMENT;

					if (launchSuspended)
					{
						creationFlags |= PROCESS_CREATION_FLAGS.CREATE_SUSPENDED;
					}

					// set up the environment block parameter
					string environmentBlock = GetEnvironmentVariablesBlock(startInfo.Environment);

					string? workingDirectory = startInfo.WorkingDirectory;
					if (workingDirectory.Length == 0)
					{
						workingDirectory = null;
					}

					int errorCode = 0;
					bool retVal;

					char* processName = null;

					fixed (char* workingDirectoryPtr = &workingDirectory.GetPinnableReference())
					fixed (void* environmentBlockPtr = environmentBlock)
					fixed (char* commandLinePtr = commandLine)
					{
						retVal = PInvoke.CreateProcess(
							processName,
							commandLinePtr,
							unused_SecAttrs,
							unused_SecAttrs,
							false,
							creationFlags,
							environmentBlockPtr,
							workingDirectoryPtr,
							&startupInfo,
							&processInfo);
					}

					if (!retVal)
						errorCode = Marshal.GetLastWin32Error();

					if (processInfo.hProcess != IntPtr.Zero && processInfo.hProcess != new IntPtr(-1))
						Marshal.InitHandle(procSH, processInfo.hProcess);
					if (processInfo.hThread != IntPtr.Zero && processInfo.hThread != new IntPtr(-1))
						PInvoke.CloseHandle(processInfo.hThread);

					if (!retVal)
					{
						throw new Win32Exception(errorCode);
					}
				}
				catch
				{
					parentInputPipeHandle?.Dispose();
					parentOutputPipeHandle?.Dispose();
					parentErrorPipeHandle?.Dispose();
					procSH.Dispose();
					throw;
				}
				finally
				{
					childInputPipeHandle?.Dispose();
					childOutputPipeHandle?.Dispose();
					childErrorPipeHandle?.Dispose();
				}
			}

			System.Diagnostics.Process process = new();
			Type processType = typeof(System.Diagnostics.Process);

			if (startInfo.RedirectStandardInput)
			{
				FieldInfo? field = processType.GetField("_standardInput", BindingFlags.NonPublic | BindingFlags.Instance);

				Encoding enc = startInfo.StandardInputEncoding ?? GetEncoding((int)PInvoke.GetConsoleCP());

				StreamWriter standardInput = new StreamWriter(new FileStream(parentInputPipeHandle!, FileAccess.Write, 4096, false), enc, 4096);
				standardInput.AutoFlush = true;

				field.SetValue(process, standardInput);
			}
			if (startInfo.RedirectStandardOutput)
			{
				FieldInfo? field = processType.GetField("_standardOutput", BindingFlags.NonPublic | BindingFlags.Instance);

				Encoding enc = startInfo.StandardOutputEncoding ?? GetEncoding((int)PInvoke.GetConsoleOutputCP());
				StreamReader standardOutput = new StreamReader(new FileStream(parentOutputPipeHandle!, FileAccess.Read, 4096, false), enc, true, 4096);

				field.SetValue(process, standardOutput);
			}
			if (startInfo.RedirectStandardError)
			{
				FieldInfo? field = processType.GetField("_standardError", BindingFlags.NonPublic | BindingFlags.Instance);

				Encoding enc = startInfo.StandardErrorEncoding ?? GetEncoding((int)PInvoke.GetConsoleOutputCP());
				StreamReader standardError = new StreamReader(new FileStream(parentErrorPipeHandle!, FileAccess.Read, 4096, false), enc, true, 4096);

				field.SetValue(process, standardError);
			}

			if (procSH.IsInvalid)
			{
				procSH.Dispose();
				throw new Exception("Failed to launch process!");
			}

			processType.InvokeMember("SetProcessHandle", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic, null, process, new object[] { procSH });
			processType.InvokeMember("SetProcessId", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic, null, process, new object[] { (int)processInfo.dwProcessId });

			return process;
		}
		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Process.cs restricts access")]
        private static class Windows
        {
            private static ProcessPriorityClass LowerPriority(ProcessPriorityClass old)
            {
                switch (old)
                {
                    case ProcessPriorityClass.RealTime:
                        return ProcessPriorityClass.High;
                    case ProcessPriorityClass.High:
                        return ProcessPriorityClass.AboveNormal;
                    case ProcessPriorityClass.AboveNormal:
                        return ProcessPriorityClass.Normal;
                    case ProcessPriorityClass.Normal:
                        return ProcessPriorityClass.BelowNormal;
                    case ProcessPriorityClass.BelowNormal:
                    case ProcessPriorityClass.Idle:
                        return ProcessPriorityClass.Idle;
                    default:
                        Debug.Fail("Unhandled priority class!");
                        return ProcessPriorityClass.Idle;
                }
            }
            static public async Task<Result> StartProcess(string directory, string executable, List<string> args, bool lowPriority, bool admin, bool noWindow, string? logFileName, InjectionConfig? injectionOptions, CancellationToken cancellationToken)
            {
                try
                {
                    string executable_path = Path.Combine(directory, executable);
                    ProcessStartInfo info = new(executable_path);
                    info.WorkingDirectory = directory;
                    info.CreateNoWindow = noWindow;

					bool launchSuspended = false;
                    Guid injector_id = Guid.Empty;
                    if (injectionOptions is not null)
                    {
                        injector_id = injectionOptions.Injector.SetupEnviroment(info);
						launchSuspended = injectionOptions.Injector.ShouldSuspendOnLaunch;

					}

                    bool loggingToDisk = false;
                    if (!String.IsNullOrWhiteSpace(logFileName))
                    {
                        info.RedirectStandardError = true;
                        info.RedirectStandardOutput = true;

                        string log_folder = Path.GetDirectoryName(logFileName);
                        Directory.CreateDirectory(log_folder);
                        loggingToDisk = true;
                        Trace.WriteLine($"log folder for process {log_folder}");
                    }

                    foreach (string arg in args)
                        info.ArgumentList.Add(arg);

                    if (admin)
                    {
						Debug.Assert(!launchSuspended);
                        info.Verb = "runas";
                        info.UseShellExecute = true;
                    }

					System.Diagnostics.Process proc;
					if (!launchSuspended)
					{
						proc = System.Diagnostics.Process.Start(info);
					}
					else
					{
						proc = StartWithCreateProcess(info, launchSuspended: true);
					}

                    if (injectionOptions is not null)
                    {
                        injectionOptions.Success = await injectionOptions.Injector.Inject(injector_id, proc);
                    }

                    if (lowPriority)
                    {
                        try
                        {
                            proc.PriorityClass = LowerPriority(proc.PriorityClass);
                        } catch (Exception ex)
                        {
                            Trace.WriteLine(ex.ToString());
                        }
                    }

                    Task<string> standardOut = null;
                    Task<string> standardError = null;

                    try
                    {
                        if (loggingToDisk)
                        {
                            standardOut = proc.StandardOutput.ReadToEndAsync();
                            standardError = proc.StandardError.ReadToEndAsync();
                        }

                        await proc.WaitForExitAsync(cancellationToken);
                    }
                    catch (OperationCanceledException) { };
                    if (cancellationToken.IsCancellationRequested && !proc.HasExited)
                    {
                        try
                        {
                            proc.Kill();
                        } catch (Exception ex)
                        {
                            Trace.WriteLine($"Error trying to terminate process (\"{executable}\", \"{info}\"):");
                            Trace.WriteLine(ex.ToString());
                        }
                    }
                    
                    Result results = null;

                    if (loggingToDisk)
                    {
                        await Task.WhenAll(standardOut, standardError);

                        using (StreamWriter file = new(logFileName, append: false))
                        {
                            await file.WriteAsync("=== Error log == \r\n\r\n");
                            await file.WriteAsync(standardError.Result);
                            await file.WriteAsync("\r\n\r\n");
                            await file.WriteAsync("=== Output log == \r\n\r\n");
                            await file.WriteAsync(standardOut.Result);
                        }

                        results = new Result(standardOut.Result, standardError.Result, proc.ExitCode);
                    }
                    else
                    {
                        results = new Result("", "", proc.ExitCode);
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    return results;
                    
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    /*
                    https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes
                    */
                    int ErrorCode = ex.NativeErrorCode;
                    if (ErrorCode == 2)
                    {
                        // todo(num0005) refactor this and move the exception into the Process
                        throw new ToolkitBase.MissingFile(executable);
                    }
                    else if (ErrorCode == 1223)
                    {
                        // todo(num0005) refactor result type, for now use a magic value to indicate UAC prompt was rejected
                        return new Result("", "", -451);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            static public async Task<Result?> StartProcessWithShell(string directory, string executable, string args, bool lowPriority, InjectionConfig? injectionOptions, CancellationToken cancellationToken)
            {
                // build command line
                string commnad_line = "/c \"" + escape_arg(executable) + " " + args + " & pause\"";

                // run shell process
                ProcessStartInfo info = new("cmd", commnad_line);
                info.WorkingDirectory = directory;

                Guid injector_id = Guid.Empty;
                if (injectionOptions is not null)
                {
                    injector_id = injectionOptions.Injector.SetupEnviroment(info);
                }

                System.Diagnostics.Process proc = System.Diagnostics.Process.Start(info);

                // TODO: find a way to do this without System.Management or P/invoke
                ManagementObjectSearcher mos = new(
                    $"Select * From Win32_Process Where ParentProcessID={proc.Id} And Caption LIKE \"%{executable}%\"");

                async Task<Result> HandleProcess(System.Diagnostics.Process process)
                {
                    try
                    {
                        try
                        {
                            Trace.WriteLine($"initial priority: {process.PriorityClass}");
                            if (lowPriority)
                                process.PriorityClass = LowerPriority(process.PriorityClass);
                            Trace.WriteLine($"final priority: {process.PriorityClass}");

                            if (injectionOptions is not null)
                            {
                                injectionOptions.Success = await injectionOptions.Injector.Inject(injector_id, process);
                            }

                            await process.WaitForExitAsync(cancellationToken);
                        }
                        catch (OperationCanceledException) { };
                        if (cancellationToken.IsCancellationRequested)
                            process.Kill();
                    }
                    catch { } // will get arg error if the process exits
                    return new Result("", "", process.ExitCode);
                }

                for (int i = 0; i < 300; i++)
                {
                    foreach (ManagementObject obj in mos.Get())
                    {
                        Trace.WriteLine($"Found child process on the {i}th iteration");
                        var child_process = System.Diagnostics.Process.GetProcessById(Convert.ToInt32(obj["ProcessID"]));
                        return await HandleProcess(child_process);
                    }

                    // wait a bit so cmd has a chance to start the process
                    await Task.Delay((int)(Math.Sqrt(i) * 30));
                }
                Trace.WriteLine($"Unable to find child proc");
                return null;
            }

        }
    }
}
