using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using static ToolkitLauncher.ToolkitProfiles;

#nullable enable

namespace ToolkitLauncher.ToolkitInterface
{
    public enum ToolType
    {
        Tool,
        Sapien,
        Guerilla,
        Game
    }

    public enum BitmapType
    {
        image_2d,
        image_3d,
        image_cubemaps,
        image_sprites,
        image_interface
    }

    [Flags]
    public enum ModelCompile
    {
        none = 0,
        collision = 2,
        physics = 4,
        render = 8,
        animations = 16,
        all = collision | physics | render | animations,
    }
    abstract public class ToolkitBase
    {

        public ToolkitBase(ProfileSettingsLauncher sourceProfile, string baseDirectory, Dictionary<ToolType, string> toolPaths)
        {
            BaseDirectory = baseDirectory;
            ToolPaths = toolPaths;
            Profile = sourceProfile;
        }

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
        /// <param name="noassert">Set tool args for -verbose and -noassert</param>
        /// <returns></returns>
        public abstract Task BuildLightmap(string scenario, string bsp, LightmapArgs args, bool noassert = false);

        /// <summary>
        /// Imports bitmaps from a directory
        /// </summary>
        /// <param name="path">Directory containing the bitmaps</param>
        /// <param name="type">The type of bitmap to import it by</param>
        /// <param name="debug_plate">MCC H1A: Whether or not we dump plate data to the data folder for inspection</param>
        public abstract Task ImportBitmaps(string path, string type, bool debug_plate = false);

        /// <summary>
        /// How build cache will handle resources
        /// </summary>
        public enum ResourceMapUsage
        {
            /// <summary>
            /// **All** resources will be internal
            /// </summary>
            None,
            /// <summary>
            /// Read existing resource files and use resources from those but don't create new ones
            /// </summary>
            Read,
            /// <summary>
            /// Read existing resource file and update them with any new resources
            /// </summary>
            ReadWrite
        }

        /// <summary>
        /// Build a cache file from a scenario
        /// </summary>
        /// <param name="scenario">The scenario to be compiled into a cache file</param>
        /// <param name="resourceUsage">CE: Whatever the resource maps (loc, bitmap, sound) should be updated or used</param>
        /// <param name="log_tags">CE: Log tags that tool has to load for the scenario</param>
        /// <param name="cache_type">CE: What platform is the cahce file intended for</param>
        public abstract Task BuildCache(string scenario, CacheType cache_type, ResourceMapUsage resourceUsage, bool log_tags = false);

        /// <summary>
        /// Import a structure into a BSP tag
        /// </summary>
        /// <param name="data_file">The file to import</param>
        /// <param name="release">H2</param>
        /// <param name="phantom_fix">CE: Whatever to apply the phantom fix</param>
        /// <returns></returns>
        public abstract Task ImportStructure(string data_file, bool phantom_fix = false, bool release = true);

        /// <summary>
        /// Import geometry to generate various types of model related tags
        /// </summary>
        /// <param name="path"></param>
        /// <param name="importType"></param>
        /// <returns></returns>
        public abstract Task ImportModel(string path, ModelCompile importType);

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
        /// Check whatever there is mutex preventing another instance of a tool from starting
        /// </summary>
        /// <param name="tool">Tool to check</param>
        /// <returns>True if another instance can't be launched, otherwise false</returns>
        public abstract bool IsMutexLocked(ToolType tool);

        /// <summary>
        /// Get the file name of the executable
        /// </summary>
        /// <param name="tool">The tool we want</param>
        /// <returns>A string containing the executable file name including the extension</returns>
        public virtual string GetToolExecutable(ToolType tool)
        {
            return ToLocalPath(ToolPaths[tool]);
        }

        /// <summary>
        /// Base directory for this toolkit
        /// </summary>
        public readonly string BaseDirectory;

        /// <summary>
        /// The paths for different executables
        /// </summary>
        public readonly Dictionary<ToolType, string> ToolPaths;

        /// <summary>
        /// Get the default tags directory for the current base directory
        /// </summary>
        /// <returns>path</returns>
        protected string GetDefaultTagDirectory()
        {
            return Path.Combine(BaseDirectory, "tags");
        }

        /// <summary>
        /// Get the default data directory for the current base directory
        /// </summary>
        /// <returns>path</returns>
        protected string GetDefaultDataDirectory()
        {
            return Path.Combine(BaseDirectory, "data");
        }

        /// <summary>
        /// Checks whatever the default tag path is being used
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsDefaultTagDirectory()
        {
            return true;
        }

        /// <summary>
        /// Checks whatever the default data path is being used
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsDefaultDataDirectory()
        {
            return true;
        }

        /// <summary>
        /// Get the tag directory, use this instead of manipulating the paths yourself
        /// </summary>
        /// <returns>Tag directory</returns>
        public virtual string GetTagDirectory()
        {
            return GetDefaultTagDirectory();
        }

        /// <summary>
        /// Get the data directory, use this instead of manipulating the paths yourself
        /// </summary>
        /// <returns>Data directory</returns>
        public virtual string GetDataDirectory()
        {
            return GetDefaultDataDirectory();
        }

        /// <summary>
        /// Should the toolkit be enabled
        /// </summary>
        /// <returns></returns>
        public bool IsEnabled()
        {            
            foreach (var exe in ToolPaths)
                if (File.Exists(exe.Value))
                    return true;
            return false;
        }

        /// <summary>
        /// Helper function for converting a path to a local path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>A path relative to the toolkit base directory</returns>
        protected virtual string ToLocalPath(string path)
        {
            return Path.GetRelativePath(BaseDirectory, path);
        }

        /// <summary>
        /// Import Unicode strings into a tag
        /// </summary>
        /// <param name="path">File to import</param>
        public abstract Task ImportUnicodeStrings(string path);

        /// <summary>
        /// Runs a custom tool command in a shell
        /// </summary>
        /// <param name="command">Command to run</param>
        /// <returns></returns>
        public async Task RunCustomToolCommand(string command)
        {
            await Utility.Process.StartProcessWithShell(BaseDirectory, GetToolExecutable(ToolType.Tool), Utility.Process.EscapeArgList(GetArgsToPrepend()) + " " + command);
        }

        /// <summary>
        /// Get the args to prepend to every invokaing of a game tool
        /// </summary>
        /// <returns>List with all the args to add</returns>
        private List<string> GetArgsToPrepend()
        {
            List<string> args = new();

            if (!IsDefaultTagDirectory())
            {
                args.Add("-tags_dir");
                args.Add(GetTagDirectory());
            }

            if (!IsDefaultDataDirectory())
            {
                args.Add("-data_dir");
                args.Add(GetDataDirectory());
            }

            if (Profile.Verbose && Profile.BuildType == build_type.release_mcc)
            {
                args.Add("-verbose");
            }

            if (!string.IsNullOrWhiteSpace(Profile.GamePath) && Directory.Exists(Profile.GamePath) && Profile.BuildType == build_type.release_mcc)
            {
                args.Add("-game_root_dir");
                args.Add(Profile.GamePath);
            }

            return args;
        }

        /// <summary>
        /// Run a tool from the toolkit with arguments
        /// </summary>
        /// <param name="tool">Tool to run</param>
        /// <param name="args">Arguments to pass to the tool</param>
        /// <returns></returns>
        public async Task RunTool(ToolType tool, List<string>? args = null)
        {
            // always include the prepend args
            List<string> full_args = GetArgsToPrepend();
            if (args is not null)
                full_args.AddRange(args);

            string tool_path = GetToolExecutable(tool);

            if (tool == ToolType.Tool)
            {
                await Utility.Process.StartProcessWithShell(BaseDirectory, tool_path, full_args);
            }
            else
            {
                await Utility.Process.StartProcess(BaseDirectory, tool_path, full_args);
            }
        }

        /// <summary>
        /// Split an import file path in data into the directory containing the scenario, scenario name, and BSP name
        /// </summary>
        /// <param name="data_file"></param>
        /// <returns>(scenario_path, bsp_name)</returns>
        public static (string ScenarioPath, string ScenarioName, string BspName) SplitStructureFilename(string data_file, string bsp_data_file = "")
        {
            string scenario_path = "";
            string scenario_name = "";
            string bsp_name = "";
            if (data_file.EndsWith(".scenario"))
            {
                scenario_path = Path.GetDirectoryName(data_file).ToLower() ?? "";
                scenario_name = Path.GetFileNameWithoutExtension(scenario_path).ToLower();
                bsp_name = "all";
                if (!string.IsNullOrEmpty(bsp_data_file))
                {
                    bsp_name = Path.GetFileNameWithoutExtension(bsp_data_file).ToLower();
                }
            }
            else
            {
                scenario_path = Path.GetDirectoryName(Path.GetDirectoryName(data_file).ToLower()) ?? "";
                scenario_name = Path.GetFileNameWithoutExtension(scenario_path).ToLower();
                bsp_name = Path.GetFileNameWithoutExtension(data_file).ToLower();
            }
            Debug.Assert(scenario_path is not null);
            Debug.Assert(scenario_name is not null);
            Debug.Assert(bsp_name is not null);
            return (scenario_path, scenario_name, bsp_name);
        }

        public ProfileSettingsLauncher Profile { get; }
    }
}
