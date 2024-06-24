using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading.Tasks;
using System.Threading;

namespace ToolkitLauncher.Utility
{
    public static partial class Process
    {
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
            static public async Task<Result> StartProcess(string directory, string executable, List<string> args, CancellationToken cancellationToken, bool lowPriority, bool admin)
            {
                try
                {
                    string executable_path = Path.Combine(directory, executable);
                    ProcessStartInfo info = new(executable_path);
                    info.WorkingDirectory = directory;
                    // info.RedirectStandardError = true;
                    // info.RedirectStandardOutput = true;
                    foreach (string arg in args)
                        info.ArgumentList.Add(arg);

                    if (admin)
                    {
                        info.Verb = "runas";
                        info.UseShellExecute = true;
                    }

                    System.Diagnostics.Process proc = System.Diagnostics.Process.Start(info);
                    if (lowPriority)
                    {
                        try
                        {
                            proc.PriorityClass = LowerPriority(proc.PriorityClass);
                        } catch (Exception ex)
                        {
                            Debug.Print(ex.ToString());
                        }
                    }
                    
                    //proc.StandardOutput.

                    //Task<string> standardOut = proc.StandardOutput.ReadToEndAsync();
                    //Task<string> standardError = proc.StandardError.ReadToEndAsync();
                    //await Task.WhenAll(standardOut, standardError, proc.WaitForExitAsync());
                    try
                    {
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
                            Debug.Print($"Error trying to terminate process (\"{executable}\", \"{info}\"):");
                            Debug.Print(ex.ToString());
                        }
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                    //return new Result(standardOut.Result, standardError.Result, proc.ExitCode);
                    return new Result("", "", proc.ExitCode);
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
                        throw new ToolkitInterface.ToolkitBase.MissingFile(executable);
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

            static public async Task<Result?> StartProcessWithShell(string directory, string executable, string args, CancellationToken cancellationToken, bool lowPriority)
            {
                // build command line
                string commnad_line = "/c \"" + escape_arg(executable) + " " + args + " & pause\"";

                // run shell process
                ProcessStartInfo info = new("cmd", commnad_line);
                info.WorkingDirectory = directory;
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
                            Debug.WriteLine($"initial priority: {process.PriorityClass}");
                            if (lowPriority)
                                process.PriorityClass = LowerPriority(process.PriorityClass);
                            Debug.WriteLine($"final priority: {process.PriorityClass}");
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
                        Debug.Print($"Found child process on the {i}th iteration");
                        var child_process = System.Diagnostics.Process.GetProcessById(Convert.ToInt32(obj["ProcessID"]));
                        return await HandleProcess(child_process);
                    }

                    // wait a bit so cmd has a chance to start the process
                    await Task.Delay((int)(Math.Sqrt(i) * 30));
                }
                Debug.Print($"Unable to find child proc");
                return null;
            }

        }
    }
}
