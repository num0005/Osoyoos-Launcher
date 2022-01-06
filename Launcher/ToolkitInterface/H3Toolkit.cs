using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static ToolkitLauncher.ToolkitProfiles;

namespace ToolkitLauncher.ToolkitInterface
{
    public class H3Toolkit : ToolkitBase, IToolkitFBX2Jointed, IToolkitFBX2ASS, IToolkitFBX2JMI
    {

        public H3Toolkit(ProfileSettingsLauncher profile, string baseDirectory, Dictionary<ToolType, string> toolPaths) : base(profile, baseDirectory, toolPaths) { }

        protected string sapienWindowClass
        {
            get => "Sapien";
        }

        override public async Task ImportBitmaps(string path, string type, bool debug_plate)
        {
            // todo(num0005): is this required? Might be able to just use bitmaps-with-type for both
            if (type != "2d")
                _ = await RunTool(ToolType.Tool, new() { "bitmaps-with-type", path, type });
            else
                _ = await RunTool(ToolType.Tool, new() { debug_plate ? "bitmaps-debug" : "bitmaps", path });
        }

        override public async Task ImportUnicodeStrings(string path)
        {
            await RunTool(ToolType.Tool, new() { "strings", path });
        }

        private async Task StructureAutoFBX(string data_file)
        {
            /* Convert Structure */
            string inPath = BaseDirectory + "\\data\\" + data_file.ToLower().Replace(".ass", ".fbx"); // Jank code, should probably make this safer
            if (File.Exists(inPath))
            {
                string outPath = BaseDirectory + "\\data\\" + data_file;
                await RunTool(ToolType.Tool, new() { "fbx-to-ass", inPath, outPath });
            }
        }

        override public async Task ImportStructure(StructureType structure_command, string data_file, bool phantom_fix, bool release, bool useFast, bool autoFBX)
        {
            if(autoFBX) { await StructureAutoFBX(data_file); }

            ToolType tool = useFast ? ToolType.ToolFast : ToolType.Tool;
            string tool_command = structure_command.ToString().Replace("_", "-");
            string data_path = data_file;
            if (structure_command == StructureType.structure_seams)
                data_path = Path.GetDirectoryName(Path.GetDirectoryName(data_file));

            await RunTool(tool, new() { tool_command, data_path }, true);
        }

        public override async Task BuildCache(string scenario, CacheType cacheType, ResourceMapUsage resourceUsage, bool logTags, string cachePlatform, bool cacheCompress, bool cacheResourceSharing, bool cacheMultilingualSounds, bool cacheRemasteredSupport, bool cacheMPTagSharinge)
        {
            string path = scenario.Replace(".scenario", "");
            string audio_configuration = "";
            string target_language = "";
            string dedicated_server = "";
            string compression_type = "";
            string use_fmod_data = "";

            await RunTool(ToolType.Tool, new List<string>() { "build-cache-file", path, cachePlatform, audio_configuration, target_language, dedicated_server, compression_type, use_fmod_data });
        }

        private static string GetLightmapQuality(LightmapArgs lightmapArgs)
        {
            return lightmapArgs.level_combobox.ToLower();
        }

        public async Task FauxSync(string scenario, string bsp, bool instanceOutput, bool useFast)
        {
            await RunTool(useFast ? ToolType.ToolFast : ToolType.Tool, new() { "faux_data_sync", scenario, bsp }, instanceOutput);
        }

        private static int FauxCalculateJobID(string scenario, string bsp)
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

        public async Task FauxLocalFarm(string scenario, string bsp, string lightmapGroup, string quality, int clientCount, bool useFast, bool instanceOutput, ICancellableProgress<int> progress)
        {
            progress.MaxValue += 1 + 1 + 5 * (clientCount + 1) + 1 + 3;

            // first sync
            progress.Status = "Syncing faux (this might take a while)...";
            await FauxSync(scenario, bsp, instanceOutput, useFast);
            progress.Report(1);

            int jobID = FauxCalculateJobID(scenario, bsp);
            string blobDirectory = $"faux\\{jobID}";

            string clientCountStr = clientCount.ToString(); // cache
            ToolType tool = useFast ? ToolType.ToolFast : ToolType.Tool;

            async Task<Utility.Process.Result?> RunFastool(List<string> arguments, bool useShell)
            {
                Utility.Process.Result? result = await RunTool(tool, arguments, useShell, progress.GetCancellationToken());
                progress.Report(1);
                if (result is not null && result.HasErrorOccured)
                {
                    Debug.Print($"A lightmap command ({arguments}) has crashed, aborting");
                    progress.Cancel("Tool has crashed, canceling lightmaps...");
                }
                return result;
            }

            async Task<StageResult> RunStage(string stage)
            {
                progress.Status = $"Running stage: \"{stage}\" client count: {clientCount}";
                var instances = new List<Task<Utility.Process.Result?>>();
                for (int clientIdx = 0; clientIdx < clientCount; clientIdx++)
                    instances.Add(RunFastool(new() { $"faux_farm_{stage}", blobDirectory, clientIdx.ToString(), clientCountStr }, false));
                await Task.WhenAll(instances); // wait till workers exit

                bool worked = instances.TrueForAll(result => result.Result is not null && result.Result.Success);
                if (!worked)
                {
                    Debug.Print("Some instance crashed todo (numm005): do something here");
                    //return StageResult.ClientFail;
                }

                progress.Status = $"Merging results from stage: \"{stage}\"";
                // todo(num005): handle workers crashing in a better way than just aborting
                // merge results from workers
                await RunFastool(new() { $"faux_farm_{stage}_merge", blobDirectory, clientCountStr }, instanceOutput);
                return StageResult.Sucesss;

            }

            // start farm
            progress.Status = "Initializing lightmap farm...";
            await RunFastool(new() { "faux_farm_begin", scenario, bsp, lightmapGroup, quality, jobID.ToString() }, instanceOutput);

            // run farm

            await RunStage("dillum");
            await RunStage("pcast");
            await RunStage("radest");
            await RunStage("extillum");
            await RunStage("fgather");

            // end farm
            progress.Status = "Ending lightmap farm...";
            await RunFastool(new() { "faux_farm_finish", blobDirectory }, instanceOutput);

            // todo(num0005): are all these strictly required?
            progress.Status = "A few final steps...";
            await RunFastool(new() { "faux-build-linear-textures-with-intensity-from-quadratic", scenario, bsp }, instanceOutput);
            await RunFastool(new() { "faux-compress-scenario-bitmaps-dxt5", scenario, bsp }, instanceOutput);
            await RunFastool(new() { "faux-farm-compression-merge", scenario, bsp }, instanceOutput);
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
                await FauxLocalFarm(scenario, bsp, lightmap_group, quality, args.instanceCount, args.NoAssert, args.instanceOutput, progress);
            } catch (OperationCanceledException)
            {
            }

        }

        // Some nonsense, probably should be an enum actually
        private class ImportTypeInfo
        {
            public ModelCompile importType;
            public string folderName, commandOption;
            public ImportTypeInfo(ModelCompile a, string b, string c)
            {
                importType = a;
                folderName = b;
                commandOption = c;
            }
        }

        private static List<ImportTypeInfo> IMPORT_TYPE_INFOS = new List<ImportTypeInfo>()
        {
            new ImportTypeInfo(ModelCompile.render, "render", "render"),
            new ImportTypeInfo(ModelCompile.collision, "collision", "collision"),
            new ImportTypeInfo(ModelCompile.physics, "physics", "physics")
        };

        private async Task ModelAutoFBX(string path, ModelCompile importType)
        {
            /* Check for x.all.fbx in root directory. */
            List<FileInfo> alls = new List<FileInfo>();
            string rootDir = BaseDirectory + "\\data\\" + path;

            string[] rootFilePaths = Directory.GetFiles(rootDir);
            foreach (string f in rootFilePaths)
            {
                FileInfo fi = new FileInfo(f);
                if (fi.Name.ToLower().EndsWith(".all.fbx"))
                {
                    alls.Add(fi);
                }
            }

            /* Convert x.all.fbx */
            if (alls.Count > 0)
            {
                foreach (FileInfo all in alls)
                {
                    string outPath = rootDir + "\\" + (all.Name.ToLower().Replace(".all.fbx", ".jms"));
                    await RunTool(ToolType.Tool, new() { "fbx-to-jms", "all", all.FullName, outPath });
                }
            }

            /* Convert render/collision/physics */
            foreach (ImportTypeInfo importTypeInfo in IMPORT_TYPE_INFOS)
            {
                if (importType.HasFlag(importTypeInfo.importType))
                {
                    string typeDir = rootDir + "\\" + importTypeInfo.folderName;

                    /* Copy x.all.fbx output if it exists */
                    if (alls.Count > 0)
                    {
                        if (!Directory.Exists(typeDir)) { Directory.CreateDirectory(typeDir); }

                        foreach (FileInfo all in alls)
                        {
                            string inPath = rootDir + "\\" + (all.Name.ToLower().Replace(".all.fbx", "_" + importTypeInfo.folderName + ".jms"));
                            string outPath = typeDir + "\\" + (all.Name.ToLower().Replace(".all.fbx", ".jms"));

                            string checkPath = typeDir + "\\" + (all.Name.ToLower().Replace(".all.fbx", ".fbx"));
                            if (File.Exists(checkPath)) { File.Delete(inPath); continue; } // Skip x.all.fbx if there is an x.fbx with same name in the relevant subfolder

                            if(File.Exists(outPath)) { File.Delete(outPath); }

                            File.Copy(inPath, outPath);
                            File.Delete(inPath);
                        }
                    }
                    /* Convert fbx in relevant sub folders */
                    if (Directory.Exists(typeDir))
                    {
                        List<FileInfo> fbxs = new List<FileInfo>();

                        string[] filePaths = Directory.GetFiles(typeDir);
                        foreach (string f in filePaths)
                        {
                            FileInfo fi = new FileInfo(f);
                            if (fi.Name.ToLower().EndsWith(".fbx"))
                            {
                                fbxs.Add(fi);
                            }
                        }

                        foreach (FileInfo fbx in fbxs)
                        {
                            string outPath = typeDir + "\\" + (fbx.Name.ToLower().Replace(".fbx", ".jms"));
                            await RunTool(ToolType.Tool, new() { "fbx-to-jms", importTypeInfo.commandOption, fbx.FullName, outPath });
                        }
                    }
                }
            }

            /* Convert animations */
            string animDir = rootDir + "\\animations";
            if (Directory.Exists(animDir) && importType.HasFlag(ModelCompile.animations))
            {
                List<FileInfo> fbxs = new List<FileInfo>();

                string[] filePaths = Directory.GetFiles(animDir);
                foreach (string f in filePaths)
                {
                    FileInfo fi = new FileInfo(f);
                    if (fi.Name.ToLower().EndsWith(".fbx"))
                    {
                        fbxs.Add(fi);
                    }
                }

                foreach (FileInfo fbx in fbxs)
                {
                    string outPath = animDir + "\\" + (fbx.Name.ToLower().Replace(".fbx", ".jma"));
                    await RunTool(ToolType.Tool, new() { "fbx-to-jma", fbx.FullName, outPath });
                }
            }
        }

        /// <summary>
        /// Import a model
        /// </summary>
        /// <param name="path"></param>
        /// <param name="importType"></param>
        /// <returns></returns>
        public override async Task ImportModel(string path, ModelCompile importType, bool phantomFix, bool h2SelectionLogic, bool renderPRT, bool FPAnim, string characterFPPath, string weaponFPPath, bool accurateRender, bool verboseAnim, bool uncompressedAnim, bool skyRender, bool resetCompression, bool autoFBX)
        {
            string type = "";
            if (verboseAnim)
            {
                type = "-verbose";
            }
            else if (uncompressedAnim)
            {
                type = "-uncompressed";
            }
            else if (resetCompression)
            {
                type = "-reset";
            }

            if(autoFBX) { await ModelAutoFBX(path, importType); }

            if (importType.HasFlag(ModelCompile.render))
                if (skyRender)
                    await RunTool(ToolType.Tool, new() { "render-sky", path });
                else if (accurateRender)
                    await RunTool(ToolType.Tool, new() { "render-accurate", path, renderPRT ? "final" : "draft" });
                else
                    await RunTool(ToolType.Tool, new() { "render", path, renderPRT ? "final" : "draft" });
            if (importType.HasFlag(ModelCompile.collision))
                await RunTool(ToolType.Tool, new() { "collision", path });
            if (importType.HasFlag(ModelCompile.physics))
                await RunTool(ToolType.Tool, new() { "physics", path });
            if (importType.HasFlag(ModelCompile.animations))
                if (FPAnim)
                    await RunTool(ToolType.Tool, new() { "fp-model-animations" + type, path, characterFPPath, weaponFPPath });
                else
                    await RunTool(ToolType.Tool, new() { "model-animations" + type, path });
        }

        public override async Task ImportSound(string path, string platform, string bitrate, string ltf_path, string sound_command, string class_type, string compression_type)
        {
            await RunTool(ToolType.Tool, new List<string>() { sound_command.Replace("_", "-"), path, class_type });
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
        /// <param name="geoClass"></param>
        /// <returns></returns>
        public async Task JMSFromFBX(string fbxPath, string jmsPath, string geoClass)
        {
            await RunTool(ToolType.Tool, new() { "fbx-to-jms", geoClass, fbxPath, jmsPath });
        }

        /// <summary>
        /// Create an JMI from an FBX file
        /// </summary>
        /// <param name="fbxPath"></param>
        /// <param name="jmiPath"></param>
        /// <returns></returns>
        public async Task JMIFromFBX(string fbxPath, string jmiPath)
        {
            await RunTool(ToolType.Tool, new() { "fbx-to-jmi", fbxPath, jmiPath });
        }

        /// <summary>
        /// Create an ASS from an FBX file
        /// </summary>
        /// <param name="fbxPath"></param>
        /// <param name="assPath"></param>
        /// <returns></returns>
        public async Task ASSFromFBX(string fbxPath, string assPath)
        {
            await RunTool(ToolType.Tool, new() { "fbx-to-ass", fbxPath, assPath });
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