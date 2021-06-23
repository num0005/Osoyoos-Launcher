using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading.Tasks;

namespace ToolkitLauncher.Utility
{
    internal static partial class Process
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Process.cs restricts access")]
        private static class Windows
        {
            static public async Task StartProcess(string directory, string executable, List<string> args)
            {
                try
                {
                    string executable_path = Path.Combine(directory, executable);
                    ProcessStartInfo info = new(executable_path);
                    info.WorkingDirectory = directory;
                    foreach (string arg in args)
                        info.ArgumentList.Add(arg);

                    System.Diagnostics.Process proc = System.Diagnostics.Process.Start(info);
                    await proc.WaitForExitAsync();
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
                    else
                    {
                        throw;
                    }
                }
            }

            static public async Task StartProcessWithShell(string directory, string executable, string args)
            {
                // build command line
                string commnad_line = "/c \"" + escape_arg(executable) + " " + args + " & pause\"";

                // run shell process
                ProcessStartInfo info = new("cmd", commnad_line);
                info.WorkingDirectory = directory;
                System.Diagnostics.Process proc = System.Diagnostics.Process.Start(info);

                // TODO: find a way to do this without System.Management or P/invoke
                ManagementObjectSearcher mos = new(
                    String.Format("Select * From Win32_Process Where ParentProcessID={0} And Caption=\"{1}\"",
                    proc.Id, executable));

                // wait a bit so cmd has a chance to start the process
                await Task.Delay(2000);

                foreach (ManagementObject obj in mos.Get())
                {
                    var child_process = System.Diagnostics.Process.GetProcessById(Convert.ToInt32(obj["ProcessID"]));
                    try
                    {
                        await child_process.WaitForExitAsync();
                    }
                    catch { } // will get arg error if the process exits
                    return; // there shouldn't be more than one query result
                }
            }

        }
    }
}
