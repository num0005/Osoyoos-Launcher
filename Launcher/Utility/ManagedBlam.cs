using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ToolkitLauncher.Utility
{
    public class ManagedBlam
    {
        public static void RunMB(string ek_path, string tag_path, string compression_type)
        {
            string exe_path = @"I:\Osoyoos\Osoyoos-Launcher\OsoyoosMB\OsoyoosMB\bin\x64\Release\OsoyoosMB.exe";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = exe_path,
                Arguments = $"getbitmapdata \"{ek_path}\" \"{tag_path.TrimEnd('\\')}\" \"{compression_type}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            try
            {
                // Start the process
                using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();
                    Console.WriteLine("Output:");
                    Console.WriteLine(output);
                    Console.WriteLine("Errors:");
                    Console.WriteLine(error);
                }
            }
            catch (Exception ex)
            {
                // Handle any errors that might occur
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }
}
