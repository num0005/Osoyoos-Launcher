using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using static ToolkitLauncher.ToolkitInterface.ToolkitBase;
using static ToolkitLauncher.ToolkitProfiles;
using System.Windows;

namespace ToolkitLauncher.ToolkitInterface
{
    public class HRToolkit : H3Toolkit, IToolkitFBX2GR2
    {

        public HRToolkit(ProfileSettingsLauncher profile, string baseDirectory, Dictionary<ToolType, string> toolPaths) : base(profile, baseDirectory, toolPaths) { }

        override public async Task ImportStructure(StructureType structure_command, string data_file, bool phantom_fix, bool release, bool useFast, bool autoFBX, ImportArgs import_args)
        {
            ToolType tool = useFast ? ToolType.ToolFast : ToolType.Tool;

            List<string> args = new List<string>();
            args.Add("import");
            args.Add(data_file);
            if (import_args.import_check)
                args.Add("check");
            if (import_args.import_force)
                args.Add("force");
            if (import_args.import_repro)
                args.Add("repro");
            if (import_args.import_draft)
                args.Add("draft");
            if (import_args.import_seam_debug)
                args.Add("seam_debug");
            if (import_args.import_skip_instances)
                args.Add("skip_instances");
            if (import_args.import_local)
                args.Add("local");
            if (import_args.import_farm_seams)
                args.Add("farm_seams");
            if (import_args.import_farm_bsp)
                args.Add("farm_bsp");
            if (import_args.import_decompose_instances)
                args.Add("decompose_instances");
            if (import_args.import_suppress_errors_to_vrml)
                args.Add("supress_errors_to_vrml");

            await RunTool(tool, args);
        }

        // Disables "color->red>=0" type assertion failures during lightmapping
        private void PatchLightmapColorAssert(ToolType tool)
        {
            byte[] newBytes = { 0xEB, 0x3D };
            string exePath = Path.Join(BaseDirectory, GetToolExecutable(tool));

            // Expected hashes
            string originalHashNormal = "7c6b868b5f5b9303aa9210f31a7cc3221a6fa419a6ab3f323c8e0831dfd0afcd"; // Reach tool.exe 18/07/2023 unmodified
            string patchedHashNormal = "7b44c891aaf9687ac5e2848f82f1ddcf386ed928c81b9ee05043ffec83a178d0"; // Reach tool.exe 18/07/2023 WITH color patch
            string originalHashFast = "cc94ebb74f45fb60c1bbefb81c4533320398e100882bf958bff653ac48571365"; // Reach tool_fast.exe 18/07/2023 unmodified
            string patchedHashFast = "592410ad79ee701a0ba2ce96d7a72a5a34f127c42a7a96508983c57a5960237e";// Reach tool_fast.exe 18/07/2023 WITH color patch

            // Verify hash before patching
            string fileHash = ComputeFileHash(exePath);

            if (tool == ToolType.ToolFast)
            {
                if (ShouldPatch(fileHash, originalHashFast, patchedHashFast, "tool_fast.exe"))
                {
                    // Patch for assert for Reach tool_fast.exe ---- 0xF2A7F 73 0C -> EB 3D ---- 0xF29F9 73 11 -> EB 3D (Thanks Krevil)
                    ToolPatcher(exePath, 0xF2A7F, 0xF29F9, newBytes);
                }
            }
            else
            {
                if (ShouldPatch(fileHash, originalHashNormal, patchedHashNormal, "tool.exe"))
                {
                    // Patch for assert for Reach tool.exe ---- 0x170956 73 0C -> EB 3D ---- 0x17157F 73 0C -> EB 3D (Thanks Krevil)
                    ToolPatcher(exePath, 0x170956, 0x17157F, newBytes);
                }
            }
        
            static void ToolPatcher(string exePath, long offset1, long offset2, byte[] newBytes)
            {
                using var fs = new FileStream(exePath, FileMode.Open, FileAccess.ReadWrite);

                // Apply patch
                fs.Seek(offset1, SeekOrigin.Begin);
                fs.Write(newBytes, 0, 2);
                fs.Seek(offset2, SeekOrigin.Begin);
                fs.Write(newBytes, 0, 2);
            }

            static string ComputeFileHash(string exePath)
            {
                using var sha256 = SHA256.Create();
                using var stream = File.OpenRead(exePath);
                byte[] hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "");
            }

            bool ShouldPatch(string fileHash, string originalHash, string patchedHash, string exeName)
            {
                if (fileHash.Equals(originalHash, StringComparison.OrdinalIgnoreCase))
                {
                    return true; // Safe to patch original file
                }

                if (fileHash.Equals(patchedHash, StringComparison.OrdinalIgnoreCase))
                {
                    return false; // Already patched, skip
                }

                // Unknown file
                Trace.WriteLine($"Hash mismatch for {exeName} -- aborting patch");
                MessageBox.Show($"Hash mismatch for {exeName} -- aborting \"color assert\" patch.\nIs your file original?\nLightmapping will continue but you may experience a crash.",
                "Patcher Error", MessageBoxButton.OK, MessageBoxImage.Error
                );
                return false;
            }
        }

        override public async Task FauxLocalFarm(string scenario, string bsp, string lightmapGroup, string quality, int clientCount, bool useFast, OutputMode mode, ICancellableProgress<int> progress)
        {
            ToolType tool = useFast ? ToolType.ToolFast : ToolType.Tool;
            if (Profile.ReachColorAssertFix) { PatchLightmapColorAssert(tool); }

            progress.MaxValue += 1 + 1 + 5 * (clientCount + 1) + 1 + 3;

            // first sync
            progress.Status = "Syncing faux (this might take a while)...";
            await FauxSync(scenario, bsp, mode, useFast, progress.GetCancellationToken());
            progress.Report(1);

            int jobID = FauxCalculateJobID(scenario, bsp);
            string blobDirectory = $"faux\\{jobID}";

            string clientCountStr = clientCount.ToString(); // cache

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
                await RunFastool(new() { $"faux_farm_{stage}_merge", blobDirectory, clientCountStr }, mode);
                return StageResult.Sucesss;

            }

            // start farm
            progress.Status = "Initializing lightmap farm...";
            await RunFastool(new() { "faux_farm_begin", scenario, bsp, lightmapGroup, quality, jobID.ToString(), "true" }, mode);

            // run farm

            await RunStage("dillum");
            await RunStage("pcast");
            await RunStage("radest_extillum");
            await RunStage("fgather");

            // end farm
            progress.Status = "Ending lightmap farm...";
            await RunFastool(new() { "faux_farm_finish", blobDirectory }, mode);

            // todo(num0005): are all these strictly required?
            progress.Status = "A few final steps...";
            await RunFastool(new() { "faux-reorganize-mesh-for-analytical-lights", scenario, bsp }, mode);
            await RunFastool(new() { "faux-build-vmf-textures-from-quadratic", scenario, bsp, "true", "true" }, mode);
        }

        /// <summary>
        /// Import a model
        /// </summary>
        /// <param name="path"></param>
        /// <param name="importType"></param>
        /// <returns></returns>
        public override async Task ImportModel(string path, ModelCompile importType, bool phantomFix, bool h2SelectionLogic, bool renderPRT, bool FPAnim, string characterFPPath, string weaponFPPath, bool accurateRender, bool verboseAnim, bool uncompressedAnim, bool skyRender, bool PDARender, bool resetCompression, bool autoFBX, bool genShaders)
        {
            List<string> args = new();
            if (importType.HasFlag(ModelCompile.render))
            {
                // Generate shaders if requested
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
            }
            if (importType.HasFlag(ModelCompile.collision))
            {
                args.Add("collision");
                args.Add(path);
            }
            if (importType.HasFlag(ModelCompile.physics))
            {
                args.Add("physics");
                args.Add(path);
            }
            if (importType.HasFlag(ModelCompile.animations))
            {
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
            }

            await RunTool(ToolType.Tool, args);
        }

        public async Task GR2FromFBX(string fbxPath, string jsonPath, string gr2Path, bool json_rebuild, bool showOutput)
        {
            var args = new List<string>() { "fbx-to-gr2", fbxPath, jsonPath, gr2Path};

            if (json_rebuild)
                args.Add("recreate_json");

            await RunTool(ToolType.Tool, args, showOutput ? OutputMode.keepOpen : OutputMode.silent);
        }

        public async Task ImportSidecar(string sidecarPath)
        {
            await RunTool(ToolType.Tool, new List<string>() { "import", sidecarPath });
        }

        public async Task GR2FromFBXBatch(string fbx_search_path, bool json_rebuild, bool show_output)
        {
            List<Task> dispatchedTasks = new();

            string getFilepath(string file)
            {
                string[] t = file.Split(".");
                string filepath = "";
                for (int i = 0; i < t.Length - 1; i++)
                    filepath += t[i];

                return filepath;
            }

            void ConvertAllInFolder(string folder)
            {
                try
                {
                    IEnumerable<string> files = Directory.EnumerateFiles(folder, "*.fbx");

                    foreach (var f in files)
                    {
                        Task toolTask = GR2FromFBX(
                            f,
                            getFilepath(f) + ".json",
                            getFilepath(f) + ".gr2",
                            json_rebuild,
                            show_output);
                        dispatchedTasks.Add(toolTask);
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    Trace.WriteLine($"{folder} does not exist");
                }
            }

            foreach (var folder in Directory.EnumerateDirectories(fbx_search_path))
            {
                string folderName = Path.GetFileName(folder);

                string[] assetFolders = new[] { "animations", "collision", "markers", "physics", "render", "skeleton" };

                if (assetFolders.Any(folderName.Contains))
                {
                    if (folderName == "animations")
                    {
                        string[] subfolders = new[] { "JMM", "JMA", "JMT", "JMZ", "JMV", "JMO (Keyframe)", "JMO (Pose)", "JMR (Local)", "JMR (Object)" };
                        foreach (string sub in subfolders)
                            ConvertAllInFolder(Path.Join(fbx_search_path, folderName, sub));
                    }
                    else
                    {
                        ConvertAllInFolder(Path.Join(fbx_search_path, folderName));
                    }
                }
                else
                {
                    string[] subfolders = new[] { "structure", "structure_design" };
                    foreach (string sub in subfolders)
                        ConvertAllInFolder(Path.Join(fbx_search_path, folderName, sub));
                }
            }

            // wait for all the FBX files to get converted
            await Task.WhenAll(dispatchedTasks);
        }

        public override bool IsMutexLocked(ToolType tool)
        {
            // todo(num0005) implement this
            return false;
        }

        public override string GetDocumentationName()
        {
            return "HR";
        }
    }
}