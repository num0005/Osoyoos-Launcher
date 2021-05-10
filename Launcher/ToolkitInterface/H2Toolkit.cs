using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.IO;

namespace ToolkitLauncher.ToolkitInterface
{
    class H2Toolkit: ToolkitBase
    {
        override public string GetToolExecutable(ToolType tool)
        {
            string name = base.GetToolExecutable(tool);
            return name;
        }

        override public async Task ImportBitmaps(string path, string type)
        {
            var profile = ToolkitProfiles.SettingsList[MainWindow.profile_index];
            string bitmaps_command = "bitmaps";
            if (profile.community_tools && profile.build_type == build_type.release_standalone 
                || profile.build_type == build_type.release_mcc  
                || profile.build_type == build_type.release_internal )
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

        public override async Task BuildCache(string scenario, int cache_type, bool update_resource)
        {
            string path = scenario.Replace(".scenario", "");
            await RunTool(ToolType.Tool, new List<string>() { "build-cache-file", path });
        }

        private static string GetLightmapQuality(LightmapArgs lightmapArgs)
        {
            return lightmapArgs.level_combobox.ToString().ToLower();
        }
    
        public override async Task BuildLightmap(string scenario, string bsp, LightmapArgs args)
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
            var profile = ToolkitProfiles.SettingsList[MainWindow.profile_index];
            var args = new List<string>()
            {
                "lightmap_merge",
                scenario,
                bsp,
                slave_count.ToString()
            };
            if (profile.build_type == build_type.release_standalone && profile.community_tools)
            {
                args = new List<string>()
                {
                    "lightmaps-master",
                    scenario,
                    bsp,
                    quality,
                    slave_count.ToString(),
                };
            }
            await RunTool(ToolType.Tool, args);
        }

        private async Task RunSlaveLightmap(string scenario, string bsp, string quality, int slave_count, int index)
        {
            var profile = ToolkitProfiles.SettingsList[MainWindow.profile_index];
            var args = new List<string>()
                {
                    "lightmap_slave",
                    scenario,
                    bsp,
                    quality,
                    index.ToString(),
                    slave_count.ToString()
                };
            if (profile.build_type == build_type.release_standalone && profile.community_tools)
            {
                args = new List<string>()
                {
                    "lightmaps-slave",
                    scenario,
                    bsp,
                    quality,
                    slave_count.ToString(),
                    index.ToString()
                };
            }
            await RunTool(ToolType.Tool, args);
        }

        /// <summary>
        /// Import a model
        /// </summary>
        /// <param name="path"></param>
        /// <param name="import_type"></param>
        /// <param name="render_prt"></param>
        /// <returns></returns>
        public override async Task ImportModel(string path, string import_type, bool render_prt)
        {
            var profile = ToolkitProfiles.SettingsList[MainWindow.profile_index];
            string command_string;
            string render_command = "render";
            string collision_command = "collision";
            string physics_command = "physics";
            string animations_command = "model-animations";
            if (profile.build_type == build_type.release_standalone && profile.community_tools)
            {
                render_command = "model-render";
                collision_command = "model-collision";
                physics_command = "model-physics";
                animations_command = "append-animations";
            }

            if (render_prt && profile.build_type == build_type.release_mcc 
                || render_prt && profile.build_type == build_type.release_internal)
                render_command = "render-prt";

            switch (import_type)
            {
                case "render":
                    command_string = render_command;
                    break;
                case "collision":
                    command_string = collision_command;
                    break;
                case "physics":
                    command_string = physics_command;
                    break;
                case "animations":
                    command_string = animations_command;
                    break;
                case "all":
                    command_string = "all";
                    break;
                default:
                    throw new Exception();
            }
            if (command_string == "all")
            {
                await RunTool(ToolType.Tool, new List<string>() { render_command, path });
                await RunTool(ToolType.Tool, new List<string>() { collision_command, path });
                await RunTool(ToolType.Tool, new List<string>() { physics_command, path });
                await RunTool(ToolType.Tool, new List<string>() { animations_command, path });
            }
            else
            {
                await RunTool(ToolType.Tool, new List<string>() { command_string, path });
            }
        }

        public override async Task ImportSound(string sound_command, string path, string platform, string class_type, string bitrate, string ltf_path)
        {
            var profile = ToolkitProfiles.SettingsList[MainWindow.profile_index];
            if (profile.community_tools && profile.build_type == build_type.release_standalone)
                await RunTool(ToolType.Tool, new List<string>() { "import-lipsync", path, ltf_path });
            else
                await RunTool(ToolType.Tool, new List<string>() { sound_command.Replace("_", "-"), path, class_type });
        }
    }
}
