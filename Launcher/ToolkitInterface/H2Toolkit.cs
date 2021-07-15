using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using static ToolkitLauncher.ToolkitProfiles;

namespace ToolkitLauncher.ToolkitInterface
{
    public class H2Toolkit : ToolkitBase
    {

        public H2Toolkit(ProfileSettingsLauncher profile, string baseDirectory, Dictionary<ToolType, string> toolPaths) : base(profile, baseDirectory, toolPaths) { }

        override public async Task ImportBitmaps(string path, string type, bool debug_plate)
        {
            string bitmaps_command = "bitmaps";
            if (Profile.CommunityTools)
            {
                bitmaps_command = "bitmaps-with-type";
                await RunTool(ToolType.Tool, new List<string>() { bitmaps_command, path, type });
            }
            else
            {
                await RunTool(ToolType.Tool, new List<string>() { bitmaps_command, path });
            }
        }

        override public async Task ImportUnicodeStrings(string path)
        {
            await RunTool(ToolType.Tool, new List<string>() { "new-strings", path });
        }

        override public async Task ImportStructure(string data_file, bool phantom_fix, bool release)
        {
            bool is_ass_file = data_file.ToLowerInvariant().EndsWith("ass");
            string command = is_ass_file ? "structure-new-from-ass" : "structure-from-jms";
            string use_release = release ? "yes" : "no";
            await RunTool(ToolType.Tool, new List<string>() { command, data_file, use_release });
        }

        public override async Task BuildCache(string scenario, CacheType cache_type, ResourceMapUsage resourceUsage, bool log_tags)
        {
            string path = scenario.Replace(".scenario", "");
            await RunTool(ToolType.Tool, new List<string>() { "build-cache-file", path });
        }

        private static string GetLightmapQuality(LightmapArgs lightmapArgs)
        {
            return lightmapArgs.level_combobox.ToString().ToLower();
        }

        public override async Task BuildLightmap(string scenario, string bsp, LightmapArgs args, bool noassert)
        {
            string quality = GetLightmapQuality(args);

            await RunTool(ToolType.Tool, new List<string>() { "lightmaps", scenario, bsp, quality });
        }

        public async Task BuildLightmapMultiInstance(string scenario, string bsp, LightmapArgs lightmapArgs, int count)
        {
            string quality = GetLightmapQuality(lightmapArgs);
            var instances = new List<Task>();
            for (int i = 0; i < count; i++)
            {
                instances.Add(RunSlaveLightmap(scenario, bsp, quality, count, i));
            }
            await Task.WhenAll(instances);

            await RunMergeLightmap(scenario, bsp, quality, count);
        }

        private async Task RunMergeLightmap(string scenario, string bsp, string quality, int slave_count)
        {
            var args = new List<string>()
                {
                    "lightmaps-master",
                    scenario,
                    bsp,
                    quality,
                    slave_count.ToString(),
                };

            await RunTool(ToolType.Tool, args);
        }

        private async Task RunSlaveLightmap(string scenario, string bsp, string quality, int slave_count, int index)
        {
            var args = new List<string>()
                {
                    "lightmaps-slave",
                    scenario,
                    bsp,
                    quality,
                    slave_count.ToString(),
                    index.ToString()
                };
            await RunTool(ToolType.Tool, args);
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
                await RunTool(ToolType.Tool, new() { "model-render", path });
            if (importType.HasFlag(ModelCompile.collision))
                await RunTool(ToolType.Tool, new() { "model-collision", path });
            if (importType.HasFlag(ModelCompile.physics))
                await RunTool(ToolType.Tool, new() { "model-physics", path });
            if (importType.HasFlag(ModelCompile.animations))
                await RunTool(ToolType.Tool, new() { "append-animations", path });
        }

        public override async Task ImportSound(string path, string platform, string bitrate, string ltf_path)
        {
            await RunTool(ToolType.Tool, new() { "import-lipsync", path, ltf_path });
        }

        public override bool IsMutexLocked(ToolType tool)
        {
            // todo(num0005) implement this
            return false;
        }
    }
}
