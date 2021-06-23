using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static ToolkitLauncher.ToolkitProfiles;

namespace ToolkitLauncher.ToolkitInterface
{
    public class H1Toolkit: ToolkitBase
    {
        public H1Toolkit(ProfileSettingsLauncher profile, string baseDirectory, Dictionary<ToolType, string> toolPaths) : base(profile, baseDirectory, toolPaths) { }

        override public async Task ImportStructure(string data_file, bool phantom_fix, bool release)
        {
            var info = SplitStructureFilename(data_file);
            await RunTool(ToolType.Tool, new() { "structure" , info.ScenarioPath, info.BspName });
        }

        public override async Task BuildCache(string scenario, CacheType cache_type, ResourceMapUsage resourceUsage, bool log_tags)
        {
            string path = scenario.Replace(".scenario", "");
            await RunTool(ToolType.Tool, new() { "build-cache-file", path });
        }

        public override async Task BuildLightmap(string scenario, string bsp, LightmapArgs args, bool noassert)
        {
            await RunTool(ToolType.Tool, new() { "lightmaps", scenario, bsp, Convert.ToInt32(args.radiosity_quality).ToString(), args.level_slider.ToString() });
        }

        override public async Task ImportUnicodeStrings(string path)
        {
            await RunTool(ToolType.Tool, new() { "unicode-strings", path });
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
        public override async Task ImportModel(string path, ModelCompile importType)
        {
            if (importType.HasFlag(ModelCompile.render))
                await RunTool(ToolType.Tool, new() { "model", path });
            if (importType.HasFlag(ModelCompile.collision))
                await RunTool(ToolType.Tool, new() { "collision-geometry", path });
            if (importType.HasFlag(ModelCompile.physics))
                await RunTool(ToolType.Tool, new() { "physics", path });
            if (importType.HasFlag(ModelCompile.animations))
                await RunTool(ToolType.Tool, new() { "animations", path });
        }

        /// <summary>
        /// Import a sound file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public override async Task ImportSound(string path, string platform, string bitrate, string ltf_path)
        {
            await RunTool(ToolType.Tool, new List<string>() { "sounds", path, platform, bitrate});
        }

        public async Task ImportAnimations(string path)
        {
            await RunTool(ToolType.Tool, new List<string>() { "animations", path });
        }

        public async Task ImportPhysics(string path)
        {
            await RunTool(ToolType.Tool, new List<string>() { "physics", path });
        }

        override public async Task ImportBitmaps(string path, string type, bool debug_plate)
        {
            await RunTool(ToolType.Tool, new List<string>() { "bitmaps", path });
        }

        protected virtual string sapienWindowClass
        {
            get => "halo";
        }

        public override bool IsMutexLocked(ToolType tool)
        {
            if (!OperatingSystem.IsWindows()) {
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
                } catch (UnauthorizedAccessException)
                {
                    // The mutex exists so treat it as locked
                    return true;
                } catch (WaitHandleCannotBeOpenedException)
                {
                    // very weird, shouldn't happen
                    return false;
                }
                catch (Exception ex)
                {
                    // maybe it's bad to catch everything but WinAPI can be a crapshoot, better to not crash the whole launcher
                    Debug.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
