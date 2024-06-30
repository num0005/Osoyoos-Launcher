using OsoyoosMB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using static ToolkitLauncher.ToolkitProfiles;

#nullable enable

namespace ToolkitLauncher.ToolkitInterface
{
    public enum ToolType
    {
        Tool,
        ToolFast,
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

    public enum StructureType
    {
        structure,
        structure_design,
        structure_seams
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

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum OutputMode
    {
        [Description("Keep shell open")]
        keepOpen,
        [Description("Close shell")]
        closeShell,
        [Description("Slient (log to disk)")]
        logToDisk,
        [Description("Slient (no log)")]
        slient
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

        public record LightmapArgs(
            string level_combobox,
            float Threshold,
            bool radiosity_quality,
            bool NoAssert,
            string lightmapGroup,
            int instanceCount,
            OutputMode outputSetting,
            string lightmapGlobals);

        public record ImportArgs(
            bool import_check,
            bool import_force,
            bool import_verbose,
            bool import_repro,
            bool import_draft,
            bool import_seam_debug,
            bool import_skip_instances,
            bool import_local,
            bool import_farm_seams,
            bool import_farm_bsp,
            bool import_decompose_instances,
            bool import_supress_errors_to_vrml);

        /// <summary>
        /// Build a lightmap for a given scenario and BSP
        /// </summary>
        /// <param name="scenario">Path to the scenario</param>
        /// <param name="bsp">Name of the BSP</param>
        /// <param name="args">Lightmap settings</param>
        /// <exception cref="OperationCanceledException">Thrown if the lightmap is canceled</exception>
        /// <returns></returns>
        public abstract Task BuildLightmap(string scenario, string bsp, LightmapArgs args, ICancellableProgress<int>? progress = null);

        /// <summary>
        /// Imports bitmaps from a directory
        /// </summary>
        /// <param name="path">Directory containing the bitmaps</param>
        /// <param name="type">The type of bitmap to import it by</param>
        /// <param name="debug_plate">MCC H1A: Whether or not we dump plate data to the data folder for inspection</param>
        public abstract Task ImportBitmaps(string path, string type, string compression, bool debug_plate = false);

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
        /// <param name="logTags">CE: Log tags that tool has to load for the scenario</param>
        /// <param name="cacheType">CE: What graphics engine does the cache support</param>
        /// <param name="cachePlatform">H2-H3: What platform is the cahce file intended for</param>
        /// <param name="cacheCompress">H2: Is the cache compressed</param>
        /// <param name="cacheResourceSharing">H2: Does the cache support raw data sharing</param>
        /// <param name="cacheMultilingualSounds">H2: Does the cache support multiple languages for sounds</param>
        /// <param name="cacheRemasteredSupport">H2: Does the cache support Saber3D</param>
        /// <param name="cacheMPTagSharing">H2: Does the cache support tag sharing</param>
        public abstract Task BuildCache(string scenario, CacheType cacheType, ResourceMapUsage resourceUsage, bool logTags, string cachePlatform, bool cacheCompress, bool cacheResourceSharing, bool cacheMultilingualSounds, bool cacheRemasteredSupport, bool cacheMPTagSharing);

        /// <summary>
        /// Import a structure into a BSP tag
        /// </summary>
        /// <param name="structure_command">H3: The command to use for level importing</param>
        /// <param name="data_file">The file to import</param>
        /// <param name="release">H2V: An unused bool. Always set to true</param>
        /// <param name="useFast">H2-H3: Run a play build of tool if the toolset has one</param>
        /// <param name="phantom_fix">CE: Whatever to apply the phantom fix</param>
        /// <param name="autoFBX"></param>
        /// <param name="import_info">HR-H4-H2A: Settings for the import command</param>
        /// <returns></returns>
        public abstract Task ImportStructure(StructureType structure_command, string data_file, bool phantom_fix, bool release, bool useFast, bool autoFBX, ImportArgs import_info);

        /// <summary>
        /// Import geometry to generate various types of model related tags
        /// </summary>
        /// <param name="path"></param>
        /// <param name="importType"></param>
        /// <param name="renderPRT"></param>
        /// <param name="FPAnim"></param>
        /// <param name="characterFPPath"></param>
        /// <param name="weaponFPPath"></param>
        /// <param name="accurateRender"></param>
        /// <param name="verboseAnim"></param>
        /// <param name="uncompressedAnim"></param>
        /// <param name="skyRender"></param>
        /// <param name="PDARender"></param>
        /// <param name="resetCompression"></param>
        /// <param name="autoFBX"></param>
        /// <param name="model_generate_shaders"></param>
        /// <returns></returns>
        public abstract Task ImportModel(string path, ModelCompile importType, bool phantomFix, bool h2SelectionLogic, bool renderPRT, bool FPAnim, string characterFPPath, string weaponFPPath, bool accurateRender, bool verboseAnim, bool uncompressedAnim, bool skyRender, bool PDARender, bool resetCompression, bool autoFBX, bool genShaders);

        /// <summary>
        /// Import a WAV file to generate a sound tag
        /// </summary>
        /// <param name="path"></param>
        /// <param name="platform"></param>
        /// <param name="bitrate"></param>
        /// <param name="ltf_path"></param>
        /// <param name="sound_command"></param>
        /// <param name="class_type"></param>
        /// <param name="compression_type"></param>
        /// <returns></returns>
        public abstract Task ImportSound(string path, string platform, string bitrate, string ltf_path, string sound_command, string class_type, string compression_type, string custom_extension);

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
            // bit of a hack but what can you do
            // return a hardcoded tool_fast path if not set.
            // todo(numoo5): replace this with a proper defaults system
            if (tool == ToolType.ToolFast && !ToolPaths.ContainsKey(ToolType.ToolFast))
                return "tool_fast"; // hack hack hack
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
            return Path.Join(BaseDirectory, "tags");
        }

        /// <summary>
        /// Get the default data directory for the current base directory
        /// </summary>
        /// <returns>path</returns>
        protected string GetDefaultDataDirectory()
        {
            return Path.Join(BaseDirectory, "data");
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
        public virtual bool IsEnabled()
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

            if ((Profile.Generation == GameGen.Halo1 || Profile.Generation == GameGen.Halo2) && Profile.IsMCC)
            {
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

                if (Profile.Verbose)
                {
                    args.Add("-verbose");
                }

                if (!string.IsNullOrWhiteSpace(Profile.GamePath) && Directory.Exists(Profile.GamePath) && Profile.Generation == GameGen.Halo1)
                {
                    args.Add("-game_root_dir");
                    args.Add(Profile.GamePath);
                }

                if (Profile.Generation == GameGen.Halo2)
                {
                    if (Profile.ExpertMode)
                    {
                        args.Add("-expert_mode");
                    }

                    if (Profile.Batch)
                    {
                        args.Add("-batch");
                    }
                }
            }

            return args;
        }

        public abstract string GetDocumentationName();

        public enum ManagedBlamErrorCode
        {
            Success,
            MissingManagedBlam,
            InternalError,
        }

        protected async Task<ManagedBlamErrorCode> RunManagedBlamCommand(List<string> arguments)
        {
            string? executable_path = Process.GetCurrentProcess().MainModule?.FileName;
            Debug.Print($"our path is {executable_path}");

            if (executable_path is null)
                return ManagedBlamErrorCode.InternalError;

            List<string> actual_arguments = new() { MBHandler.command_id };
            actual_arguments.AddRange(arguments);

            Utility.Process.Result result = await Utility.Process.StartProcess(BaseDirectory, executable_path, actual_arguments);

            Debug.Print($" Managedblam result {result}");

            // -2 is the special code for missing assembly
            if (result.ReturnCode == -2)
                return ManagedBlamErrorCode.MissingManagedBlam;

            // don't bother disambiguating this out unless the UI is updated.
            return result.ReturnCode == 0 ? ManagedBlamErrorCode.Success : ManagedBlamErrorCode.InternalError;
        }

        /// <summary>
        /// Should the shell be used to handle this tool execution request?
        /// </summary>
        /// <param name="tool">The tool that will be run</param>
        /// <param name="args">Arguments pased to the tool</param>
        /// <returns>Whatever shell should be used</returns>
        protected virtual OutputMode GetDefaultOutputMode(ToolType tool, List<string>? args)
        {
            bool isTool = tool == ToolType.Tool || tool == ToolType.ToolFast;
            return isTool ? OutputMode.keepOpen : OutputMode.closeShell;
        }
        /// <summary>
        /// Used for lightmapping, gets you the more silent mode
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        static protected OutputMode GetMoreSilentMode(OutputMode mode)
        {
            switch (mode)
            {
                case OutputMode.logToDisk:
                case OutputMode.slient:
                    return OutputMode.slient;
                case OutputMode.keepOpen:
                case OutputMode.closeShell:
                    return OutputMode.closeShell;
                default:
                    throw new NotImplementedException("Unknown OutputMode");
            }
        }


        public Action<Utility.Process.Result>? ToolFailure { get; set; }

        /// <summary>
        /// Run a tool from the toolkit with arguments
        /// </summary>
        /// <param name="tool">Tool to run</param>
        /// <param name="args">Arguments to pass to the tool</param>
        /// <param name="useShell">Force either use shell or not use it</param>
        /// <param name="lowPriority">Lower priority if possible</param>
        /// <param name="cancellationToken">Kill the tool before it exits</param>
        /// <returns>Results of running the tool if possible</returns>
        public async Task<Utility.Process.Result?> RunTool(ToolType tool, List<string>? args = null, OutputMode? outputMode = null, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            Utility.Process.Result? result = await RunToolInternal(tool, args, outputMode, cancellationToken, lowPriority);
            if (result is not null && result.ReturnCode != 0 && ToolFailure is not null)
                ToolFailure(result);
            return result;
        }

        /// <summary>
        /// Implementation of <c>RunTool</c>
        /// </summary>
        private async Task<Utility.Process.Result?> RunToolInternal(ToolType tool, List<string>? args, OutputMode? outputMode, CancellationToken cancellationToken, bool lowPriority)
        {
            // always include the prepend args
            List<string> full_args = GetArgsToPrepend();
            if (args is not null)
                full_args.AddRange(args);

            string tool_path = GetToolExecutable(tool);
            if (outputMode is null)
                outputMode = GetDefaultOutputMode(tool, args);

            if (outputMode == OutputMode.keepOpen)
                return await Utility.Process.StartProcessWithShell(BaseDirectory, tool_path, full_args, cancellationToken, lowPriority);
            else
                return await Utility.Process.StartProcess(BaseDirectory, tool_path, full_args, cancellationToken, lowPriority);
        }

        /// <summary>
        /// Split an import file path in data into the directory containing the scenario, scenario name, and BSP name
        /// </summary>
        /// <param name="data_file"></param>
        /// <returns>(scenario_path, bsp_name)</returns>
        public static (string ScenarioPath, string ScenarioName, string BspName) SplitStructureFilename(string data_file, string bsp_data_file = "", string hrek_path = "")
        {
            string scenario_path = "";
            string scenario_name = "";
            string bsp_name = "";
            if (data_file.EndsWith(".scenario"))
            {
                scenario_path = Path.GetDirectoryName(data_file.ToLower()) ?? "";
                scenario_name = Path.GetFileNameWithoutExtension(data_file.ToLower());
                bsp_name = "all";
                if (!string.IsNullOrEmpty(bsp_data_file))
                {
                    bsp_name = Path.GetFileNameWithoutExtension(bsp_data_file.ToLower());
                }
            }
            else if (data_file.EndsWith(".xml"))
            {
                string full_path = Path.Join(hrek_path, "data", data_file);

                XmlDocument sidecar = new XmlDocument();
                sidecar.Load(full_path);
                XmlNodeList elemList = sidecar.GetElementsByTagName("OutputTag");

                foreach (XmlNode element in elemList)
                {
                    if (element.Attributes != null)
                    {
                        foreach (XmlAttribute attribute in element.Attributes)
                        {
                            switch (attribute.Value)
                            {
                                case "scenario":
                                    scenario_path = Path.GetDirectoryName(element.InnerText.ToLower()) ?? "";
                                    scenario_name = Path.GetFileNameWithoutExtension(scenario_path.ToLower());
                                    break;
                                case "scenario_structure_bsp":
                                    bsp_name = Path.GetFileNameWithoutExtension(element.InnerText.ToLower());
                                    break;
                            }
                        }
                    }
                }
            }
            else
            {
                scenario_path = Path.GetDirectoryName(Path.GetDirectoryName(data_file.ToLower())) ?? "";
                scenario_name = Path.GetFileNameWithoutExtension(scenario_path.ToLower());
                bsp_name = Path.GetFileNameWithoutExtension(data_file.ToLower());
            }
            Debug.Assert(scenario_path is not null);
            Debug.Assert(scenario_name is not null);
            Debug.Assert(bsp_name is not null);
            return (scenario_path, scenario_name, bsp_name);
        }

        public ProfileSettingsLauncher Profile { get; }
    }
}
