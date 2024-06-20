using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace ToolkitLauncher.Utility
{
    public class ManagedBlam
    {
        public static bool RunMBBitmaps(string ek_path, string tag_path, string compression_type)
        {
            string exe_path = Path.Combine(ek_path, @"bin\OsoyoosMB.exe");

            if (File.Exists(exe_path))
            {
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
                        process.WaitForExit();
                        return true;
                    }
                }
                catch
                {
                    // Handle any errors that might occur
                    MessageBox.Show("Unspecified ManagedBlam error.\nBitmaps have still been imported, but settings will not be applied.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            else
            {
                // User likely hasnt put the second exe in the right place
                MessageBox.Show($"Error: Cannot find \"{exe_path}\".\nMake sure the OsoyoosMB.exe is in your editing kit's \"bin\" folder.\nBitmaps have still been imported, but settings will not be applied.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            
        }

        public static bool RunMBShaders(string ek_path, string shaders_folder, string[] shader_names)
        {
            // Can't pass an array to another process, so this is unfortunate
            string txt_path = Path.Combine(ek_path, @"bin\", "shader_names.txt");

            if (File.Exists(txt_path))
            {
                File.Delete(txt_path);
            }

            File.WriteAllLines(txt_path, shader_names);


            // Let's get managedblam going now
            string exe_path = Path.Combine(ek_path, @"bin\OsoyoosMB.exe");
            string full_shaders_folder = Path.Combine(ek_path, @"tags\", shaders_folder, @"shaders");

            if (File.Exists(exe_path))
            {
                Debug.WriteLine("Starting OsoyoosMB.exe now");
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = exe_path,
                    Arguments = $"generateshaders \"{ek_path}\" \"{full_shaders_folder.TrimEnd('\\')}\"",
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
                        process.WaitForExit();
                        Debug.WriteLine("MB ran successfully, reimporting bitmaps");
                        return true;
                    }
                }
                catch
                {
                    Debug.WriteLine("Unspecified ManagedBlam error");
                    MessageBox.Show("Unspecified ManagedBlam error.\nShader tags have not been generated.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            else
            {
                // User likely hasnt put the second exe in the right place
                Debug.WriteLine("Failed to find OsoyoosMB.exe");
                MessageBox.Show($"Error: Cannot find \"{exe_path}\".\nMake sure the OsoyoosMB.exe is in your editing kit's \"bin\" folder.\nShader tags have not been generated.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

        }
    }
}
