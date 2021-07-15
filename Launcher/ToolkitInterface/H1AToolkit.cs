using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static ToolkitLauncher.ToolkitProfiles;

namespace ToolkitLauncher.ToolkitInterface
{
    public class H1AToolkit : H1Toolkit
    {

        public H1AToolkit(ProfileSettingsLauncher profle, string baseDirectory, Dictionary<ToolType, string> toolPaths) : base(profle, baseDirectory, toolPaths) {}

        private string tagPath {
            get => Profile.TagPath;
        }
        private string dataPath
        {
            get => Profile.DataPath;
        }

        protected override bool IsDefaultTagDirectory()
        {
            return String.IsNullOrWhiteSpace(tagPath);
        }

        protected override bool IsDefaultDataDirectory()
        {
            return String.IsNullOrWhiteSpace(dataPath);
        }

        public override string GetTagDirectory()
        {
            if (!IsDefaultTagDirectory())
            {
                if (Path.IsPathRooted(tagPath))
                    return tagPath;
                else
                    return Path.Combine(BaseDirectory, tagPath);
            }
            return base.GetTagDirectory();
        }

        public override string GetDataDirectory()
        {
            if (!IsDefaultDataDirectory())
            {
                if (Path.IsPathRooted(dataPath))
                    return dataPath;
                else
                    return Path.Combine(BaseDirectory, dataPath);
            }
            return base.GetDataDirectory();
        }

        protected override string sapienWindowClass
        {
            get => "H1A Sapien";
        }

        /// <summary>
        /// Import a model with special H1A options
        /// </summary>
        /// <param name="path"></param>
        /// <param name="importType"></param>
        /// <param name="phantomFix"></param>
        /// <param name="h2SelectionLogic"></param>
        /// <returns></returns>
        public async Task ImportModel(string path, ModelCompile importType, bool phantomFix, bool h2SelectionLogic)
        {
            if (importType.HasFlag(ModelCompile.render))
                await RunTool(ToolType.Tool, new() { "model", path, h2SelectionLogic.ToString() });
            if (importType.HasFlag(ModelCompile.collision))
                await RunTool(ToolType.Tool, new() { "collision-geometry", path, phantomFix.ToString()});
            if (importType.HasFlag(ModelCompile.physics))
                await RunTool(ToolType.Tool, new() { "physics", path });
            if (importType.HasFlag(ModelCompile.animations))
                await RunTool(ToolType.Tool, new() { "animations", path });
        }

        public override async Task ImportStructure(string data_file, bool phantom_fix, bool release)
        {
            var info = SplitStructureFilename(data_file);
            await RunTool(ToolType.Tool, new List<string>() { "structure", info.ScenarioPath, info.BspName, phantom_fix.ToString() });
        }

        public override async Task BuildCache(string scenario, CacheType cache_type, ResourceMapUsage resourceUsage, bool log_tags)
        {
            string path = scenario.Replace(".scenario", "");
            string resourceUsageString = resourceUsage switch
            {
                ResourceMapUsage.None => "none",
                ResourceMapUsage.Read => "read",
                ResourceMapUsage.ReadWrite => "read_write",
                _ => throw new InvalidDataException("Invalid ResourceMapUsage value!")
            };
            await RunTool(ToolType.Tool, new() { "build-cache-file", path, cache_type.ToString(), resourceUsageString, log_tags.ToString() });
        }

        public override async Task BuildLightmap(string scenario, string bsp, LightmapArgs args, bool noassert)
        {
            var cmd_args = new List<string>()
                {
                    "lightmaps",
                    scenario,
                    bsp,
                    Convert.ToInt32(args.radiosity_quality).ToString(),
                    args.level_slider.ToString()
                };
            if (noassert)
            {
                cmd_args = new List<string>()
                    {
                    "-noassert",
                    "lightmaps",
                    scenario,
                    bsp,
                    Convert.ToInt32(args.radiosity_quality).ToString(),
                    args.level_slider.ToString()
                    };
            }
            await RunTool(ToolType.Tool, cmd_args);
        }

        /// <summary>
        /// Create a JMA from an FBX file
        /// </summary>
        /// <param name="fbxPath">Path to the FBX file</param>
        /// <param name="jmaPath">Path to save the JMA at</param>
        /// <param name="startIndex">First keyframe index to include</param>
        /// <param name="startIndex">Last keyframe index to include</param>
        /// <returns></returns>
        public async Task JMAFromFBX(string fbxPath, string jmaPath, int startIndex = 0, int? endIndex = null)
        {
            if (endIndex is not null)
                await RunTool(ToolType.Tool, new() { "fbx-to-jma", fbxPath, jmaPath, startIndex.ToString(), endIndex.ToString() });
            else
                await RunTool(ToolType.Tool, new() { "fbx-to-jma", fbxPath, jmaPath, startIndex.ToString() });
        }

        /// <summary>
        /// Create an JMS from an FBX file
        /// </summary>
        /// <param name="fbxPath"></param>
        /// <param name="jmsPath"></param>
        /// <returns></returns>
        public async Task JMSFromFBX(string fbxPath, string jmsPath)
        {
            await RunTool(ToolType.Tool, new() { "fbx-to-jms", fbxPath, jmsPath });
        }

        /// <summary>
        /// Creates bitmap tags from TIF/TIFF image files
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        override public async Task ImportBitmaps(string path, string type, bool debug_plate)
        {
            await RunTool(ToolType.Tool, new() { "bitmaps", path, type, debug_plate.ToString() });
        }

    }
}
