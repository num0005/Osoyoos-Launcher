using OsoyoosMB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ToolkitLauncher.Utility;
using static ToolkitLauncher.ToolkitProfiles;
using Path = System.IO.Path;

namespace ToolkitLauncher.ToolkitInterface
{
    public class H3Toolkit : ToolkitBase, IToolkitFBX2Jointed, IToolkitFBX2ASS, IToolkitFBX2JMI
    {

        public H3Toolkit(ProfileSettingsLauncher profile, string baseDirectory, Dictionary<ToolType, string> toolPaths) : base(profile, baseDirectory, toolPaths) { }

        protected string sapienWindowClass
        {
            get => "Sapien";
        }

        override public async Task ImportBitmaps(string path, string type, string compression, bool debug_plate)
        {
            // Call managedblam import
            await RunManagedBlamCommand(new() { MBHandler.setup_bitmaps_command, path, compression });
            

            // Import bitmaps - run this regardless of managedblam success
            await RunTool(ToolType.Tool, new() { debug_plate ? "bitmaps-debug" : "bitmaps", path });
        }

        override public async Task ImportUnicodeStrings(string path)
        {
            await RunTool(ToolType.Tool, new List<string>() { "strings", path });
        }

        override public async Task ImportStructure(StructureType structure_command, string data_file, bool phantom_fix, bool release, bool useFast, bool autoFBX, ImportArgs import_args)
        {
            if (autoFBX) { await AutoFBX.Structure(this, data_file, false); }

            ToolType tool = useFast ? ToolType.ToolFast : ToolType.Tool;
            string tool_command = structure_command.ToString().Replace("_", "-");
            string data_path = data_file;
            if (structure_command == StructureType.structure_seams)
                data_path = Path.GetDirectoryName(Path.GetDirectoryName(data_file));

            await RunTool(tool, new List<string>() { tool_command, data_path });
        }

        public override async Task BuildCache(string scenario, CacheType cacheType, ResourceMapUsage resourceUsage, bool logTags, string cachePlatform, bool cacheCompress, bool cacheResourceSharing, bool cacheMultilingualSounds, bool cacheRemasteredSupport, bool cacheMPTagSharinge)
        {
            await RunTool(ToolType.Tool, new List<string>() { "build-cache-file", scenario.Replace(".scenario", "") });
        }

        public static string GetLightmapQuality(LightmapArgs lightmapArgs)
        {
            return lightmapArgs.level_combobox.ToLower();
        }

        public async Task FauxSync(string scenario, string bsp, OutputMode mode, bool useFast, CancellationToken cancellationToken = default)
        {
            await RunTool(useFast ? ToolType.ToolFast : ToolType.Tool, new List<string>() { "faux_data_sync", scenario, bsp }, mode, cancellationToken: cancellationToken);
        }

        public static int FauxCalculateJobID(string scenario, string bsp)
        {
            int hash = 0x117;
            foreach (char @char in scenario)
                hash ^= (hash << 11) ^ @char;
            foreach (char @char in bsp)
                hash ^= (hash << 7) ^ @char;
            return Math.Abs(hash);
        }

        public enum StageResult
        {
            Sucesss,
            ClientFail,
            MergeFail
        };

        public virtual async Task FauxLocalFarm(string scenario, string bsp, string lightmapGroup, string quality, int clientCount, bool useFast, OutputMode mode, ICancellableProgress<int> progress)
        {
            progress.MaxValue += 1 + 1 + 5 * (clientCount + 1) + 1 + 3;

            // first sync
            progress.Status = "Syncing faux (this might take a while)...";
            await FauxSync(scenario, bsp, mode, useFast, progress.GetCancellationToken());
            progress.Report(1);

            int jobID = FauxCalculateJobID(scenario, bsp);
            string blobDirectory = $"faux\\{jobID}";

            string clientCountStr = clientCount.ToString(); // cache
            ToolType tool = useFast ? ToolType.ToolFast : ToolType.Tool;

            async Task<Utility.Process.Result?> RunFastool(List<string> arguments, OutputMode mode)
            {
                Utility.Process.Result? result = await RunTool(tool, arguments, outputMode: mode, cancellationToken: progress.GetCancellationToken());
                progress.Report(1);
                if (result is not null && result.HasErrorOccured)
                {
                    Trace.WriteLine($"A lightmap command ({arguments}) has crashed, aborting");
                    progress.Cancel("Tool has crashed, canceling lightmaps...");
                }
                return result;
            }

            async Task<StageResult> RunStage(string stage)
            {
                progress.Status = $"Running stage: \"{stage}\" client count: {clientCount}";
                var instances = new List<Task<Utility.Process.Result?>>();
                for (int clientIdx = 0; clientIdx < clientCount; clientIdx++)
                    instances.Add(RunFastool(new() { $"faux_farm_{stage}", blobDirectory, clientIdx.ToString(), clientCountStr }, GetMoreSilentMode(mode)));
                await Task.WhenAll(instances); // wait till workers exit

                bool worked = instances.TrueForAll(result => result.Result is not null && result.Result.Success);
                if (!worked)
                {
                    Trace.WriteLine("Some instance crashed todo (numm005): do something here");
                    //return StageResult.ClientFail;
                }

                progress.Status = $"Merging results from stage: \"{stage}\"";
                // todo(num005): handle workers crashing in a better way than just aborting
                // merge results from workers
                await RunFastool(new List<string>() { $"faux_farm_{stage}_merge", blobDirectory, clientCountStr }, mode);
                return StageResult.Sucesss;

            }

            // start farm
            progress.Status = "Initializing lightmap farm...";
            await RunFastool(new List<string>() { "faux_farm_begin", scenario, bsp, lightmapGroup, quality, jobID.ToString() }, mode);

            // run farm

            await RunStage("dillum");
            await RunStage("pcast");
            await RunStage("radest");
            await RunStage("extillum");
            await RunStage("fgather");

            // end farm
            progress.Status = "Ending lightmap farm...";
            await RunFastool(new List<string>() { "faux_farm_finish", blobDirectory }, mode);

            // todo(num0005): are all these strictly required?
            progress.Status = "A few final steps...";
            await RunFastool(new List<string>() { "faux-build-linear-textures-with-intensity-from-quadratic", scenario, bsp }, mode);
            await RunFastool(new List<string>() { "faux-compress-scenario-bitmaps-dxt5", scenario, bsp }, mode);
            await RunFastool(new List<string>() { "faux-farm-compression-merge", scenario, bsp }, mode);
        }

        public override async Task BuildLightmap(string scenario, string bsp, LightmapArgs args, ICancellableProgress<int>? progress)
        {
            Debug.Assert(progress is not null);
            string quality = GetLightmapQuality(args);

            // default to all
            string lightmap_group = args.lightmapGroup;
            if (string.IsNullOrWhiteSpace(args.lightmapGroup))
                lightmap_group = "all";

            try
            {
                await FauxLocalFarm(scenario, bsp, lightmap_group, quality, args.instanceCount, args.NoAssert, args.outputSetting, progress);
            }
            catch (OperationCanceledException)
            {
            }

        }

        /// <summary>
        /// Import a model
        /// </summary>
        /// <param name="path"></param>
        /// <param name="importType"></param>
        /// <returns></returns>
        public override async Task ImportModel(string path, ModelCompile importType, bool phantomFix, bool h2SelectionLogic, bool renderPRT, bool FPAnim, string characterFPPath, string weaponFPPath, bool accurateRender, bool verboseAnim, bool uncompressedAnim, bool skyRender, bool PDARender, bool resetCompression, bool autoFBX, bool genShaders)
        {
            if (autoFBX) { await AutoFBX.Model(this, path, importType); }

            if (importType.HasFlag(ModelCompile.render))
            {
                await RunTool(ToolType.Tool, await ImportRenderArgs(genShaders, skyRender, path, accurateRender, renderPRT));
            }
            if (importType.HasFlag(ModelCompile.collision))
            {
                await RunTool(ToolType.Tool, new List<string> { "collision", path });
            }
            if (importType.HasFlag(ModelCompile.physics))
            {
                await RunTool(ToolType.Tool, new List<string> { "physics", path });
            }
            if (importType.HasFlag(ModelCompile.animations))
            {
                await RunTool(ToolType.Tool, ImportAnimationsArgs(FPAnim, verboseAnim, uncompressedAnim, resetCompression, path, characterFPPath, weaponFPPath));
            }
        }

        private async Task<List<string>> ImportRenderArgs(bool genShaders, bool skyRender, string path, bool accurateRender, bool renderPRT)
        {
            List<string> args = new();

            // Generate shaders if requested
            if (genShaders) { if (! await AutoShaders.CreateEmptyShaders(GetTagDirectory(), GetDataDirectory(), path, "H3")) { }; }
            if (skyRender)
            {
                args.Add("render-sky");
                args.Add(path);
            }
            else if (accurateRender)
            {
                args.Add("render-accurate");
                args.Add(path);
                args.Add(renderPRT ? "final" : "draft");
            }
            else
            {
                args.Add("render");
                args.Add(path);
                args.Add(renderPRT ? "final" : "draft");
            }

            return args;
        }

        private static List<string> ImportAnimationsArgs(bool FPAnim, bool verboseAnim, bool uncompressedAnim, bool resetCompression, string path, string characterFPPath, string weaponFPPath)
        {
            List<string> args = new();

            if (FPAnim)
            {
                if (verboseAnim)
                    args.Add("fp-model-animations-verbose");
                else if (uncompressedAnim)
                    args.Add("fp-model-animations-uncompressed");
                else if (resetCompression)
                    args.Add("fp-model-animations-reset");
                else
                    args.Add("fp-model-animations");
                args.Add(path);
                args.Add(characterFPPath);
                args.Add(weaponFPPath);
            }
            else
            {
                if (verboseAnim)
                    args.Add("model-animations-verbose");
                else if (uncompressedAnim)
                    args.Add("model-animations-uncompressed");
                else if (resetCompression)
                    args.Add("model-animations-reset");
                else
                    args.Add("model-animations");
                args.Add(path);
            }
            return args;
        }

        public override async Task ImportSound(string path, string platform, string bitrate, string ltf_path, string sound_command, string class_type, string compression_type, string custom_extension)
        {
            if (string.IsNullOrWhiteSpace(custom_extension))
            {
                await RunTool(ToolType.Tool, new List<string>() { sound_command.Replace("_", "-"), path, class_type });
            }
            else
            {
                await RunTool(ToolType.Tool, new List<string>() { sound_command.Replace("_", "-"), path, class_type, "-bank:" + custom_extension.ToLower() });
            }
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
                await RunTool(ToolType.Tool, new List<string>() { "fbx-to-jma", fbxPath, jmaPath, startIndex.ToString(), endIndex.ToString() });
            else
                await RunTool(ToolType.Tool, new List<string>() { "fbx-to-jma", fbxPath, jmaPath, startIndex.ToString() });
        }

        /// <summary>
        /// Create an JMS from an FBX file
        /// </summary>
        /// <param name="fbxPath"></param>
        /// <param name="jmsPath"></param>
        /// <param name="geoClass"></param>
        /// <returns></returns>
        public async Task JMSFromFBX(string fbxPath, string jmsPath, string geoClass)
        {
            await RunTool(ToolType.Tool, new List<string>() { "fbx-to-jms", geoClass, fbxPath, jmsPath });
        }

        /// <summary>
        /// Create an JMI from an FBX file
        /// </summary>
        /// <param name="fbxPath"></param>
        /// <param name="jmiPath"></param>
        /// <returns></returns>
        public async Task JMIFromFBX(string fbxPath, string jmiPath)
        {
            await RunTool(ToolType.Tool, new List<string>() { "fbx-to-jmi", fbxPath, jmiPath });
        }

        /// <summary>
        /// Create an ASS from an FBX file
        /// </summary>
        /// <param name="fbxPath"></param>
        /// <param name="assPath"></param>
        /// <returns></returns>
        public async Task ASSFromFBX(string fbxPath, string assPath)
        {
            await RunTool(ToolType.Tool, new List<string>() { "fbx-to-ass", fbxPath, assPath });
        }

        public override bool IsMutexLocked(ToolType tool)
        {
            // todo(num0005) implement this
            return false;
        }

        public override string GetDocumentationName()
        {
            return "H3";
        }
    }
}