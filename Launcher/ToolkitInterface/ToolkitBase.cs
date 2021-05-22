using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Management;

namespace ToolkitLauncher.ToolkitInterface
{
    public enum ToolType
    {
        Tool,
        Sapien,
        Guerilla
    }

    public enum BitmapType
    {
        image_2d,
        image_3d,
        image_cubemaps,
        image_sprites,
        image_interface
    }
    abstract public class ToolkitBase
    {


        /// <summary>
        /// Base class for exceptions relating to toolkit commands
        /// </summary>
        public class ToolkitException : Exception { }

        /// <summary>
        /// Thrown when there is a file missing from the base directory
        /// </summary>
        public class MissingFile : ToolkitException
        {
            /// <summary>
            ///
            /// </summary>
            /// <param name="filename">Path to the file relative to the base directory</param>
            public MissingFile(string filename)
            {
                FileName = filename;
            }

            public string FileName
            {
                get;
            }
        }

        public class LightmapArgs
        {
            public enum Level_Quality
            {
                Checkerboard,
                Draft_Low,
                Draft_Medium,
                Draft_High,
                Draft_Super,
                Direct_Only,
                Low,
                Medium,
                High,
                Super,
                Custom
            }

            public LightmapArgs(Level_Quality level_combobox, float level_slider, bool radiosity_quality, float customRgb)
            {
                this.level_combobox = level_combobox;
                this.level_slider = level_slider;
                this.radiosity_quality = radiosity_quality;
                this.customRgb = customRgb;
            }

            public Level_Quality level_combobox;
            public float level_slider;
            public bool radiosity_quality;
            public float customRgb;
        }
        /// <summary>
        /// Build a lightmap for a given scenario and BSP
        /// </summary>
        /// <param name="scenario">Path to the scenario</param>
        /// <param name="bsp">Name of the BSP</param>
        /// <param name="args">Lightmap settings</param>
        /// <returns></returns>
        public abstract Task BuildLightmap(string scenario, string bsp, LightmapArgs args);

        /// <summary>
        /// Imports bitmaps from a directory
        /// </summary>
        /// <param name="path">Directory containing the bitmaps</param>
        /// <param name="type">The type of bitmap to import it by</param>
        public abstract Task ImportBitmaps(string path, string type);

        /// <summary>
        /// Build a cache file from a scenario
        /// </summary>
        /// <param name="scenario">The scenario to be compiled into a cache file</param>
        public abstract Task BuildCache(string scenario);

        /// <summary>
        /// Import a structure into a BSP tag
        /// </summary>
        /// <param name="data_file">The file to import</param>
        /// <param name="release">H2</param>
        /// <returns></returns>
        public abstract Task ImportStructure(string data_file, bool release = true);

        /// <summary>
        /// Import geometry to generate various types of model related tags
        /// </summary>
        /// <param name="path"></param>
        /// <param name="import_type"></param>
        /// <returns></returns>
        public abstract Task ImportModel(string path, string import_type);

        /// <summary>
        /// Import a WAV file to generate a sound tag
        /// </summary>
        /// <param name="path"></param>
        /// <param name="platform"></param>
        /// <param name="bitrate"></param>
        /// <param name="ltf_path"></param>
        /// <returns></returns>
        public abstract Task ImportSound(string path, string platform, string bitrate, string ltf_path);

        /// <summary>
        /// Get the file name of the executable
        /// </summary>
        /// <param name="tool">The tool we want</param>
        /// <returns>A string containing the executable file name including the extension</returns>
        public virtual string GetToolExecutable(ToolType tool)
        {
            string tool_path = MainWindow.toolkit_profile.tool_path;
            string sapien_path = MainWindow.toolkit_profile.sapien_path;
            string guerilla_path = MainWindow.toolkit_profile.guerilla_path;

            if (tool == ToolType.Sapien)
                return Path.GetFileName(sapien_path);
            else if (tool == ToolType.Guerilla)
                return Path.GetFileName(guerilla_path);
            else
                return Path.GetFileName(tool_path);
        }

        /// <summary>
        /// Get the base directory for a given toolkit
        /// </summary>
        /// <returns>Returns the path</returns>
        public string GetBaseDirectory()
        {
            string tool_path = MainWindow.toolkit_profile.tool_path;
            string base_directory = "";
            if (!String.IsNullOrWhiteSpace(tool_path))
                base_directory = Directory.GetParent(tool_path).FullName;

            return base_directory;
        }

        /// <summary>
        /// Get the tag directory, use this instead of manipulating the paths yourself
        /// </summary>
        /// <returns></returns>
        public string GetTagDirectory()
        {
            return Path.Combine(GetBaseDirectory(), "tags");
        }

        /// <summary>
        /// Get the data directory, use this instead of manipulating the paths yourself
        /// </summary>
        /// <returns></returns>
        public string GetDataDirectory()
        {
            return Path.Combine(GetBaseDirectory(), "data");
        }

        /// <summary>
        /// Should the toolkit be enabled
        /// </summary>
        /// <returns></returns>
        public bool IsEnabled()
        {
            bool base_exists = Directory.Exists(GetBaseDirectory());
            bool tag_data_exists = Directory.Exists(GetTagDirectory()) || Directory.Exists(GetDataDirectory());
            return base_exists && tag_data_exists;
        }

        /// <summary>
        /// Helper function for converting a path to a local path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>A path relative to the toolkit base directory</returns>
        protected virtual string ToLocalPath(string path)
        {
            return Path.GetRelativePath(GetBaseDirectory(), path);
        }

        /// <summary>
        /// Import Unicode strings into a tag
        /// </summary>
        /// <param name="path">File to import</param>
        public abstract Task ImportUnicodeStrings(string path);

        public async Task RunCustomToolCommand(string command)
        {
            await StartProcessWithShell(GetToolExecutable(ToolType.Tool), command);
        }

        /// <summary>
        /// Run a tool from the toolkit with arguments
        /// </summary>
        /// <param name="tool">Tool to run</param>
        /// <param name="args">Arguments to pass to the tool</param>
        /// <returns></returns>
        public async Task RunTool(ToolType tool, List<string> args = null)
        {
            if (args is null)
                args = new List<string>();
            string tool_path = GetToolExecutable(tool);
            if (tool == ToolType.Tool)
                await StartProcessWithShell(tool_path, args);
            else
                await StartProcess(tool_path, args);
        }

        private static string escape_arg(string arg)
        {
            return " \"" + Regex.Replace(arg, @"(\\+)$", @"$1$1") + "\"";
        }

        /// <summary>
        /// Split an import file path in data into the directory containing the scenario, scenario name, and BSP name
        /// </summary>
        /// <param name="data_file"></param>
        /// <returns>(scenario_path, bsp_name)</returns>
        public static (string ScenarioPath, string ScenarioName, string BspName) SplitStructureFilename(string data_file)
        {
            string scenario_path = Path.GetDirectoryName(Path.GetDirectoryName(data_file).ToLower());
            string scenario_name = Path.GetFileNameWithoutExtension(scenario_path).ToLower();
            string bsp_name = Path.GetFileNameWithoutExtension(data_file).ToLower();
            return (scenario_path, scenario_name, bsp_name);
        }

        private async Task StartProcessWithShell(string executable, List<string> args)
        {
            string commnad_line = "";
            foreach (string arg in args)
                commnad_line += escape_arg(arg);
            await StartProcessWithShell(executable, commnad_line);
        }

        /// <summary>
        /// Run a executable in a cmd.exe shell that pauses after the executable returns
        /// </summary>
        /// <param name="executable">unescaped name</param>
        /// <param name="args">escaped arguments</param>
        /// <returns></returns>
        private async Task StartProcessWithShell(string executable, string args)
        {
            // build command line
            string commnad_line = "/c \"" + escape_arg(executable) + " " + args + " & pause\"";

            // run shell process
            ProcessStartInfo info = new ProcessStartInfo("cmd", commnad_line);
            info.WorkingDirectory = GetBaseDirectory();
            Process proc = Process.Start(info);

            // TODO: find a way to do this without System.Management or P/invoke
            ManagementObjectSearcher mos = new ManagementObjectSearcher(
                String.Format("Select * From Win32_Process Where ParentProcessID={0} And Caption=\"{1}\"",
                proc.Id, executable));

            // wait a bit so cmd has a chance to start the process
            await Task.Delay(2000);

            foreach (ManagementObject obj in mos.Get())
            {
                var child_process = Process.GetProcessById(Convert.ToInt32(obj["ProcessID"]));
                try
                {
                    await child_process.WaitForExitAsync();
                } catch {} // will get arg error if the process exits
                return; // there shouldn't be more than one query result
            }
        }
        private async Task StartProcess(string executable, List<string> args)
        {
            try
            {
                string executable_path = Path.Combine(GetBaseDirectory(), executable);
                ProcessStartInfo info = new ProcessStartInfo(executable_path);
                info.WorkingDirectory = GetBaseDirectory();
                foreach (string arg in args)
                    info.ArgumentList.Add(arg);

                Process proc = Process.Start(info);
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
                    throw new MissingFile(executable);
                }
                else
                {
                    throw;
                }
            }

        }
    }
}
