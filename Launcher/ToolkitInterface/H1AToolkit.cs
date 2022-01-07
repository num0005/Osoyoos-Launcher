using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ToolkitLauncher.Utility;
using static ToolkitLauncher.ToolkitProfiles;

namespace ToolkitLauncher.ToolkitInterface
{
    public class H1AToolkit : H1Toolkit, IToolkitFBX2Jointed
    {

        public H1AToolkit(ProfileSettingsLauncher profle, string baseDirectory, Dictionary<ToolType, string> toolPaths) : base(profle, baseDirectory, toolPaths) { }

        private string tagPath
        {
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
        public override async Task ImportModel(string path, ModelCompile importType, bool phantomFix, bool h2SelectionLogic, bool renderPRT, bool FPAnim, string characterFPPath, string weaponFPPath, bool accurateRender, bool verboseAnim, bool uncompressedAnim, bool skyRender, bool resetCompression, bool autoFBX)
        {
            if (autoFBX) { await AutoFBX.Model(this, path, importType, true); }

            // todo(num0005): detect when the command is done running even when using -pause (and remove the forced shell usage)
            if (importType.HasFlag(ModelCompile.render))
                await RunTool(ToolType.Tool, new() { "model", path, h2SelectionLogic.ToString() }, true);
            if (importType.HasFlag(ModelCompile.collision))
                await RunTool(ToolType.Tool, new() { "collision-geometry", path, phantomFix.ToString() }, true);
            if (importType.HasFlag(ModelCompile.physics))
                await RunTool(ToolType.Tool, new() { "physics", path }, true);
            if (importType.HasFlag(ModelCompile.animations))
                await RunTool(ToolType.Tool, new() { "animations", path }, true);
        }

        public override async Task ImportStructure(StructureType structure_command, string data_file, bool phantom_fix, bool release, bool useFast, bool autoFBX)
        {
            if (autoFBX) { await AutoFBX.Structure(this, data_file, true); }

            // todo(num0005): detect when the command is done running even when using -pause (and remove the forced shell usage)
            var info = SplitStructureFilename(data_file);
            await RunTool(ToolType.Tool, new () { "structure", info.ScenarioPath, info.BspName, phantom_fix.ToString() }, true);
        }

        public override async Task BuildCache(string scenario, CacheType cacheType, ResourceMapUsage resourceUsage, bool logTags, string cachePlatform, bool cacheCompress, bool cacheResourceSharing, bool cacheMultilingualSounds, bool cacheRemasteredSupport, bool cacheMPTagSharing)
        {
            string path = scenario.Replace(".scenario", "");
            string resourceUsageString = resourceUsage switch
            {
                ResourceMapUsage.None => "none",
                ResourceMapUsage.Read => "read",
                ResourceMapUsage.ReadWrite => "read_write",
                _ => throw new InvalidDataException("Invalid ResourceMapUsage value!")
            };
            await RunTool(ToolType.Tool, new() { "build-cache-file", path, cacheType.ToString(), resourceUsageString, logTags.ToString() });
        }

        public override async Task BuildLightmap(string scenario, string bsp, LightmapArgs args, ICancellableProgress<int>? progress)
        {
            if (progress is not null)
            {
                progress.DisableCancellation();
                progress.MaxValue += 1;
            }
            var cmd_args = new List<string>()
                {
                    "lightmaps",
                    scenario,
                    bsp,
                    Convert.ToInt32(args.radiosity_quality).ToString(),
                    args.Threshold.ToString()
                };
            if (args.NoAssert)
            {
                cmd_args = new List<string>()
                    {
                    "-noassert",
                    "lightmaps",
                    scenario,
                    bsp,
                    Convert.ToInt32(args.radiosity_quality).ToString(),
                    args.Threshold.ToString()
                    };
            }
            await RunTool(ToolType.Tool, cmd_args);
            if (progress is not null)
                progress.Report(1);
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
        public async Task JMSFromFBX(string fbxPath, string jmsPath, string geo_class)
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

        public override string GetDocumentationName()
        {
            return "H1MCC";
        }
    }
}
