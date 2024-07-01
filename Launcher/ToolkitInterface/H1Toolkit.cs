using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static ToolkitLauncher.ToolkitProfiles;

namespace ToolkitLauncher.ToolkitInterface
{
    public class H1Toolkit : ToolkitBase
    {
        public H1Toolkit(ProfileSettingsLauncher profile, string baseDirectory, Dictionary<ToolType, string> toolPaths) : base(profile, baseDirectory, toolPaths) { }

        override public async Task ImportStructure(StructureType structure_command, string data_file, bool phantom_fix, bool release, bool useFast, bool autoFBX, ImportArgs import_args)
        {
            var info = SplitStructureFilename(data_file);
            await RunTool(ToolType.Tool, new List<string>() { "structure", info.ScenarioPath, info.BspName });
        }

        public override async Task BuildCache(string scenario, CacheType cacheType, ResourceMapUsage resourceUsage, bool logTags, string cachePlatform, bool cacheCompress, bool cacheResourceSharing, bool cacheMultilingualSounds, bool cacheRemasteredSupport, bool cacheMPTagSharing)
        {
            await RunTool(ToolType.Tool, new List<string>() { "build-cache-file", scenario.Replace(".scenario", "") });
        }

        public override async Task BuildLightmap(string scenario, string bsp, LightmapArgs args, ICancellableProgress<int>? progress)
        {
            if (progress is not null)
            {
                progress.DisableCancellation();
                progress.MaxValue += 1;
            }
            await RunTool(ToolType.Tool, new List<string>() { "lightmaps", scenario, bsp, Convert.ToInt32(args.radiosity_quality).ToString(), args.Threshold.ToString() });
            if (progress is not null)
                progress.Report(1);
        }

        override public async Task ImportUnicodeStrings(string path)
        {
            await RunTool(ToolType.Tool, new List<string>() { "unicode-strings", path });
        }

        /// <summary>
        /// Import an hmt text file into a tag
        /// </summary>
        /// <param name="path">File to import</param>
        /// <param name="path">File to import</param>
        public async Task ImportHUDStrings(string path, string scenario_name)
        {
            await RunTool(ToolType.Tool, new List<string>() { "hud-messages", path, scenario_name });
        }

        /// <summary>
        /// Import a model
        /// </summary>
        /// <param name="path"></param>
        /// <param name="importType"></param>
        /// <returns></returns>
        public override async Task ImportModel(string path, ModelCompile importType, bool phantomFix, bool h2SelectionLogic, bool renderPRT, bool FPAnim, string characterFPPath, string weaponFPPath, bool accurateRender, bool verboseAnim, bool uncompressedAnim, bool skyRender, bool PDARender, bool resetCompression, bool autoFBX, bool genShaders)
        {
            if (importType.HasFlag(ModelCompile.render))
                await RunTool(ToolType.Tool, new List<string>() { "model", path });
            if (importType.HasFlag(ModelCompile.collision))
                await RunTool(ToolType.Tool, new List<string>() { "collision-geometry", path });
            if (importType.HasFlag(ModelCompile.physics))
                await RunTool(ToolType.Tool, new List<string>() { "physics", path });
            if (importType.HasFlag(ModelCompile.animations))
                await RunTool(ToolType.Tool, new List<string>() { "animations", path });
        }

        /// <summary>
        /// Import a sound file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public override async Task ImportSound(string path, string platform, string bitrate, string ltf_path, string sound_command, string class_type, string compression_type, string custom_extension)
        {
            await RunTool(ToolType.Tool, new List<string>() { "sounds", path, platform, bitrate });
        }

        override public async Task ImportBitmaps(string path, string type, string compression, bool should_clear_old_usage, bool debug_plate)
        {
            await RunTool(ToolType.Tool, new List<string>() { "bitmaps", path });
        }

        protected virtual string sapienWindowClass
        {
            get => "halo";
        }

        public override bool IsMutexLocked(ToolType tool)
        {
            if (!OperatingSystem.IsWindows())
            {
                Debug.Fail("Unsupported API!");
                return false;
            }
            if (tool == ToolType.Sapien)
            {
                string mutex_name = sapienWindowClass + " in " + BaseDirectory.Replace('\\', '/');
                bool createdNew;
                try
                {
                    Mutex shellMutex = new(true, mutex_name, out createdNew);
                    shellMutex.Close();
                    return !createdNew;
                }
                catch (UnauthorizedAccessException)
                {
                    // The mutex exists so treat it as locked
                    return true;
                }
                catch (WaitHandleCannotBeOpenedException)
                {
                    // very weird, shouldn't happen
                    return false;
                }
                catch (Exception ex)
                {
                    // maybe it's bad to catch everything but WinAPI can be a crapshoot, better to not crash the whole launcher
                    Trace.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public override string GetDocumentationName()
        {
            return "H1CE";
        }
    }
}
