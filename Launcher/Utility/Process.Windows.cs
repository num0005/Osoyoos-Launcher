using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using ToolkitLauncher.ToolkitInterface;

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
            static public async Task<Result> StartProcess(string directory, string executable, List<string> args, bool lowPriority, bool admin, bool noWindow, string? logFileName, CancellationToken cancellationToken)
            {
                try
                {
                    string executable_path = Path.Combine(directory, executable);
                    ProcessStartInfo info = new(executable_path);
                    info.WorkingDirectory = directory;
                    info.CreateNoWindow = noWindow;


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

            static public async Task<Result?> StartProcessWithShell(string directory, string executable, string args, bool lowPriority, CancellationToken cancellationToken)
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
                            Trace.WriteLine($"initial priority: {process.PriorityClass}");
                            if (lowPriority)
                                process.PriorityClass = LowerPriority(process.PriorityClass);
                            Trace.WriteLine($"final priority: {process.PriorityClass}");
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
