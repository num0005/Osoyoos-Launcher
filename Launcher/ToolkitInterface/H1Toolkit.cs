using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ToolkitLauncher.ToolkitInterface
{
    class H1Toolkit: ToolkitBase
    {
        override public async Task ImportStructure(string data_file, bool phantom_fix, bool release)
        {
            var info = SplitStructureFilename(data_file);
            await RunTool(ToolType.Tool, new List<string>() { phantom_fix ? "structure-hack" : "structure" , info.ScenarioPath, info.BspName });
        }

        public override async Task BuildCache(string scenario, int cache_type, bool update_resources)
        {
            string path = scenario.Replace(".scenario", "");
            string build_command = "build-cache-file";
            string platform_string = "pc";
            switch (cache_type)
            {
                case 0:
                    platform_string = "pc";
                    break;
                case 1:
                    platform_string = "mcc";
                    break;
                default:
                    throw new Exception("Unreachable!");
            }

            if (update_resources && MainWindow.halo_ce_mcc)
            {
                build_command = "build-cache-file-nopack";
                await RunTool(ToolType.Tool, new List<string>() { build_command, path, platform_string });
            }
            else
            {
                await RunTool(ToolType.Tool, new List<string>() { build_command, path });
            }
        }

        public override async Task BuildLightmap(string scenario, string bsp, LightmapArgs args)
        {
            await RunTool(ToolType.Tool, new List<string>() { "lightmaps", scenario, bsp, Convert.ToInt32(args.radiosity_quality).ToString(), args.level_slider.ToString() });
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
        /// <param name="import_type"></param>
        /// <param name="render_prt"></param>
        /// <returns></returns>
        public override async Task ImportModel(string path, string import_type, bool render_prt)
        {
            string command_string;
            switch (import_type)
            {
                case "render":
                    command_string = "model";
                    break;
                case "collision":
                    command_string = "collision-geometry";
                    break;
                case "physics":
                    command_string = "physics";
                    break;
                case "animations":
                    command_string = "animations";
                    break;
                case "all":
                    command_string = "all";
                    break;
                default:
                    throw new Exception();
            }
            if (command_string == "all")
            {
                await RunTool(ToolType.Tool, new List<string>() { "model", path });
                await RunTool(ToolType.Tool, new List<string>() { "collision-geometry", path });
                await RunTool(ToolType.Tool, new List<string>() { "physics", path });
                await RunTool(ToolType.Tool, new List<string>() { "animations", path });
            }
            else
            {
                await RunTool(ToolType.Tool, new List<string>() { command_string, path });
            }
        }

        /// <summary>
        /// Import a sound file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public override async Task ImportSound(string sound_command, string path, string platform, string class_type, string bitrate, string ltf_path)
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

        override public async Task ImportBitmaps(string path, string type)
        {
            await RunTool(ToolType.Tool, new List<string>() { "bitmaps", path });
        }
    }
}
