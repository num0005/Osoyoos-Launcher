using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ToolkitLauncher;
using ToolkitLauncher.Utility;
using static ToolkitLauncher.ToolkitProfiles;

namespace ToolkitLauncher.ToolkitInterface
{
    public class H2Toolkit : ToolkitBase
    {

        public H2Toolkit(ProfileSettingsLauncher profile, string baseDirectory, Dictionary<ToolType, string> toolPaths) : base(profile, baseDirectory, toolPaths) { }

        protected virtual string sapienWindowClass
        {
            get => "Sapien";
        }

        override public async Task ImportBitmaps(string path, string type, string compression, bool should_clear_old_usage, bool debug_plate)
        {
            string bitmaps_command = "bitmaps";
            if (Profile.CommunityTools || Profile.IsMCC)
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

        override public async Task ImportStructure(StructureType structure_command, string data_file, bool phantom_fix, bool release, bool useFast, bool autoFBX, ImportArgs import_args)
        {
            bool is_ass_file = data_file.ToLowerInvariant().EndsWith("ass");
            string command = is_ass_file ? "structure-new-from-ass" : "structure-from-jms";
            string use_release = release ? "yes" : "no";
            await RunTool(ToolType.Tool, new List<string>() { command, data_file, use_release });
        }

        public override async Task BuildCache(string scenario, CacheType cacheType, ResourceMapUsage resourceUsage, bool logTags, string cachePlatform, bool cacheCompress, bool cacheResourceSharing, bool cacheMultilingualSounds, bool cacheRemasteredSupport, bool cacheMPTagSharing)
        {
            await RunTool(ToolType.Tool, new List<string>() { "build-cache-file", scenario.Replace(".scenario", "") });
        }

        static private DLLInjector GetLightmapConfigInjector()
        {
			static void ModifyEnviroment(IDictionary<string, string?> Enviroment)
			{
				Enviroment[DLLInjector.GetVariableName("PATCH_QUALITY")] = "1";
			}

            return new(Resources.H2ToolHooks, "h2.patch.lightmap-quality.dll", ModifyEnviroment);
		}

		private record NopFillFormat(uint BaseAddress, List<uint> CallsToPatch);


        readonly static Dictionary<string, NopFillFormat> _calls_to_patch_md5 = new()
        {
            // tool regular, latest MCC build
            { "C2011BB9B07A7325492D7A804BD939EB", 
                new NopFillFormat(0x400000, new() {0x4F833F, 0x4F867B}) }, // tag_save lightmap_tag, tag_save scenario_editable
                        // tool_fast, latest MCC build
            { "3A889D370A7BE537AF47FF8035ACD201",
				new NopFillFormat(0x400000, new() {0x4ADD50, 0x4ADFF5}) }  // tag_save lightmap_tag, tag_save scenario_editable
		};

		private IProcessInjector? GetInjector(LightmapArgs args)
        {
            if (!Profile.IsMCC)
                return null;

			ToolType tool = args.NoAssert ? ToolType.ToolFast: ToolType.Tool;


            string tool_Path = GetToolExecutable(tool);
            if (!Path.IsPathRooted(tool_Path))
            {
				tool_Path = Path.Join(BaseDirectory, tool_Path);
            }

            string tool_hash = HashHelpers.GetMD5Hash(tool_Path).ToUpper();

            DLLInjector? lightmapQualityInjector = null;

			if (args.QualitySetting == "custom")
            {
                lightmapQualityInjector = GetLightmapConfigInjector();

			}

            if (_calls_to_patch_md5.ContainsKey(tool_hash))
            {
                NopFillFormat config = _calls_to_patch_md5[tool_hash];

                IEnumerable<H2ToolLightmapFixInjector.NopFill> nopFills = config.CallsToPatch.Select(offset => new H2ToolLightmapFixInjector.NopFill(offset, 5));

                return new H2ToolLightmapFixInjector(config.BaseAddress, nopFills, daisyChain: lightmapQualityInjector);

			}
            else
            {
                return lightmapQualityInjector;
            }
		}

		public override async Task BuildLightmap(string scenario, string bsp, LightmapArgs args, ICancellableProgress<int>? progress)
		{
			LogFolder = $"lightmaps_{Path.GetFileNameWithoutExtension(scenario)}";
			try
			{
				if (args.instanceCount > 1 && (Profile.IsMCC || Profile.CommunityTools)) // multi instance?
				{
					if (progress is not null)
						progress.Status = $"Running {args.instanceCount} instances";

					if (progress is not null)
						progress.MaxValue += 1 + args.instanceCount;


					IProcessInjector? injector = null;
                    Dictionary<int, Utility.Process.InjectionConfig> injectionState = new();
                    if (Profile.IsMCC)
                        injector = GetInjector(args);

					async Task RunInstance(int index)
					{
						bool delayZerothInstance = true;
						if (index == 0 && !Profile.IsH2Codez()) // not needed for H2Codez
						{
                            Trace.WriteLine("Launcher worker zero, checking patch success, etc");
                            if (injector is not null)
                            {
                                for (int i = 0; i < 20; i++)
                                {
									await Task.Delay(200);
                                    if (injectionState.Values.Any(c => c.Success))
                                    {
                                        Trace.WriteLine("fix injection succeeded for all processes!");
										delayZerothInstance = false;
                                        break;
                                    }
								}

                                if (delayZerothInstance)
                                {
                                    Trace.WriteLine("Failed to inject the fix into some processes");
									Trace.Indent();
									foreach (var entry in injectionState)
                                    {
                                        if (!entry.Value.Success)
											Trace.WriteLine($"{entry.Key} worker injection failed");
									}
                                    Trace.Unindent();
                                }
                                
                            }

                            if (delayZerothInstance)
                            {
                                Trace.WriteLine("Unable to patch workers, worker zero will be delayed to compensate");
                                if (progress is not null)
                                    progress.Status = "Delaying launch of zeroth instance";
                                await Task.Delay(1000 * 70, progress.GetCancellationToken());
                                progress.Status = $"Running {args.instanceCount} instances";
                            }
						}

						Utility.Process.Result result;

						LogFileSuffix = $"-{index}";
						if (Profile.IsMCC)
						{
							bool wereWeExperts = Profile.ElevatedToExpert;
							Profile.ElevatedToExpert = true;

							Utility.Process.InjectionConfig? config = null;
                            if (injector is not null && index != 0)
                            {
                                Trace.WriteLine($"Configuring injector for worker {index}");
                                config = new(injector);
                                injectionState[index] = config;

							}

							try
							{
								result = await RunTool(args.NoAssert ? ToolType.ToolFast : ToolType.Tool,
									new List<string>(){
										"lightmaps-farm-worker",
										scenario,
										bsp,
										args.QualitySetting,
										index.ToString(),
										args.instanceCount.ToString(),
									},
									outputMode: args.outputSetting, 
									lowPriority: index == 0 && delayZerothInstance,
                                    injectionOptions: config,
									cancellationToken: progress.GetCancellationToken());
							}
							finally
							{
								Profile.ElevatedToExpert = wereWeExperts;
							}
						}
						else
						{
							// todo: Remove this code
							result = await RunTool(ToolType.Tool,
							new List<string>(){
								"lightmaps-slave",// the long legacy of h2codez
								scenario,
								bsp,
								args.QualitySetting,
								args.instanceCount.ToString(),
								index.ToString()
							},
							outputMode: args.outputSetting,
							cancellationToken: progress.GetCancellationToken());
						}

						if (result is not null && result.HasErrorOccured)
							progress.Cancel($"Tool worker {index} has failed - exit code {result.ReturnCode}");
						if (progress is not null)
							progress.Report(1);
					}

					var instances = new List<Task>();
					for (int i = args.instanceCount - 1; i >= 0; i--)
					{
						instances.Add(RunInstance(i));
					}
					await Task.WhenAll(instances);
					if (progress is not null)
						progress.Status = "Merging output";

					if (progress.IsCancelled)
						return;

					await RunMergeLightmap(scenario, bsp, args.instanceCount, args.NoAssert);
					if (progress is not null)
						progress.Report(1);
				}
				else
				{
					Debug.Assert(args.instanceCount == 1); // should be one, otherwise we got bad args
					if (progress is not null)
					{
						progress.DisableCancellation();
						progress.MaxValue += 1;
					}
					await RunTool((args.NoAssert && Profile.IsMCC) ? ToolType.ToolFast : ToolType.Tool, new() { "lightmaps", scenario, bsp, args.QualitySetting });
					if (progress is not null)
						progress.Report(1);
				}
			}
			finally
			{
				LogFolder = null;
			}
		}

		private async Task RunMergeLightmap(string scenario, string bsp, int workerCount, bool useFast)
        {

            if (Profile.IsMCC)
            {
                await RunTool(useFast ? ToolType.ToolFast : ToolType.Tool, new List<string>()
                {
                    "lightmaps-farm-merge",
                    scenario,
                    bsp,
                    workerCount.ToString(),
                });
            }
            else // todo: Remove this code
            {
                await RunTool(ToolType.Tool, new List<string>()
                {
                    "lightmaps-master", // beware legacy code
                    scenario,
                    bsp,
                    "super",
                    workerCount.ToString(),
                });
            }
        }

        private async Task<Utility.Process.Result> RunLightmapWorker(string scenario, string bsp, string quality, int workerCount, int index, bool useFast, CancellationToken cancelationToken, OutputMode output)
        {
            this.LogFileSuffix = $"-{index}";
            if (Profile.IsMCC)
            {
                bool wereWeExperts = Profile.ElevatedToExpert;
                Profile.ElevatedToExpert = true;
                try
                {
                    return await RunTool(useFast ? ToolType.ToolFast : ToolType.Tool, new List<string>()
                    {
                        "lightmaps-farm-worker",
                        scenario,
                        bsp,
                        quality,
                        index.ToString(),
                        workerCount.ToString()
                    }, outputMode: output, lowPriority: index == 0, cancellationToken: cancelationToken);
                } finally
                {
                    Profile.ElevatedToExpert = wereWeExperts;
                }
            }
            else // todo: Remove this code
            {
                List<string> args = new List<string>()
                {
                    "lightmaps-slave", // the long legacy of h2codez
                    scenario,
                    bsp,
                    quality,
                    workerCount.ToString(),
                    index.ToString()
                };
                return await RunTool(ToolType.Tool, args, outputMode: output, cancellationToken: cancelationToken);
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
            if (importType.HasFlag(ModelCompile.render))
                await RunTool(ToolType.Tool, new List<string>() { "model-render", path });
            if (importType.HasFlag(ModelCompile.collision))
                await RunTool(ToolType.Tool, new List<string>() { "model-collision", path });
            if (importType.HasFlag(ModelCompile.physics))
                await RunTool(ToolType.Tool, new List<string>() { "model-physics", path });
            if (importType.HasFlag(ModelCompile.animations))
                await RunTool(ToolType.Tool, new List<string>() { "append-animations", path });
        }

        public override async Task ImportSound(string path, string platform, string bitrate, string ltf_path, string sound_command, string class_type, string compression_type, string custom_extension)
        {
            await RunTool(ToolType.Tool, new List<string>() { "import-lipsync", path, ltf_path });
        }

        public override async Task ExtractTags(string path, bool h2MoveDir, bool bitmapsAsTGA)
        {
            await RunTool(ToolType.Tool, new List<string>() { "export-structure-mesh-obj", path });
        }

        public override bool IsMutexLocked(ToolType tool)
        {
            // todo(num0005) implement this
            return false;
        }

        public override string GetDocumentationName()
        {
            return "H2V";
        }
    }
}
