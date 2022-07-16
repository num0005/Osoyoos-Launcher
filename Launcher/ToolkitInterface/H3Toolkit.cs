using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ToolkitLauncher.Utility;
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

        override public async Task ImportStructure(StructureType structure_command, string data_file, bool phantom_fix, bool release, bool useFast, bool autoFBX)
        {
            if (autoFBX) { await AutoFBX.Structure(this, data_file, false); }

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

            // Generate shaders if requested
            if (genShaders)
            {
                // Variables
                string line = "", full_jms_path = "";
                List<string> shaders = new List<string>();
                int counter = 0;

                //Grabbing full path from drive letter to render folder
                string jmsPath = (BaseDirectory + @"\data\" + path + @"\render").Replace("\\\\", "\\");

                // Get all files in render foler
                string[] files = Directory.GetFiles(jmsPath);
                string destinationShadersFolder = BaseDirectory + @"\tags\" + path + @"\shaders";

                // Need to find a better way to do this :/
                try
                {
                    if (Directory.GetFiles(destinationShadersFolder) == Array.Empty<String>())
                    {
                        System.Diagnostics.Debug.WriteLine("Folder exists but contains no shaders, proceeding");
                        shaderGen(files, full_jms_path, line, counter, shaders, destinationShadersFolder, BaseDirectory);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Shaders already exist!");
                        MessageBox.Show("Shaders for this model already exist, skipping shader generation!", "Shader Gen. Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    System.Diagnostics.Debug.WriteLine("No folders exist, proceeding with shader gen");
                    shaderGen(files, full_jms_path, line, counter, shaders, destinationShadersFolder, BaseDirectory);
                }

                static void shaderGen(string[] files, string full_jms_path, string line, int counter, List<string> shaders, string destinationShadersFolder, string BaseDirectory)
                {
                    // Find name of jms file
                    foreach (string file in files)
                    {
                        if (file.Substring(file.Length - 4) == ".JMS" || file.Substring(file.Length - 4) == ".jms")
                        {
                            full_jms_path = file;
                        }
                    }

                    StreamReader sr = new StreamReader(full_jms_path);

                    // Find Materials definition header in JMS file
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Contains(";### MATERIALS ###"))
                        {
                            break;
                        }

                        counter++;
                    }

                    //Grab number of materials from file
                    int numMats = Int32.Parse(File.ReadLines(full_jms_path).Skip(counter + 1).Take(1).First());
                    // Line number of first shader name
                    int currentLine = counter + 7;

                    // Take each material name, strip symbols, add to list
                    string shaderNameStripped;
                    for (int i = 0; i < numMats; i++)
                    {
                        string[] shaderNameSections = File.ReadLines(full_jms_path).Skip(currentLine - 1).Take(1).First().Split(' ');
                        if (shaderNameSections.Count() >= 2)
                        {
                            shaderNameStripped = Regex.Replace(shaderNameSections[1], "[^0-9a-zA-Z_.]", String.Empty);
                        }
                        else
                        {
                            shaderNameStripped = Regex.Replace(shaderNameSections[0], "[^0-9a-zA-Z_.]", String.Empty);
                        }
                        
                        shaders.Add(shaderNameStripped);
                        currentLine += 4;
                    }

                    // Remove duplicate shader names
                    shaders = shaders.Distinct().ToList();

                    // Create directories               

                    Directory.CreateDirectory(destinationShadersFolder);

                    string defaultShader = BaseDirectory + @"\tags\levels\shared\shaders\simple\default.shader";
                    foreach (string shader in shaders)
                    {
                        string shaderName = shader + ".shader";
                        try
                        {
                            File.Copy(defaultShader, Path.Combine(destinationShadersFolder, shaderName));
                        }
                        catch (System.IO.IOException)
                        {
                            Console.WriteLine("Shader tag probably already exists!");
                        }
                    }
                }

            }

            if (autoFBX) { await AutoFBX.Model(this, path, importType); }

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