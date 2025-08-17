using System.Collections.Generic;
using System.Threading.Tasks;
using static ToolkitLauncher.ToolkitProfiles;

namespace ToolkitLauncher.ToolkitInterface
{
    public class H4Toolkit : HRToolkit, IToolkitFBX2GR2
    {

        public H4Toolkit(ProfileSettingsLauncher profile, string baseDirectory, Dictionary<ToolType, string> toolPaths) : base(profile, baseDirectory, toolPaths) { }

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

            await RunTool(tool, args);
        }

        public override async Task BuildCache(string scenario, CacheType cacheType, ResourceMapUsage resourceUsage, bool logTags, string cachePlatform, bool cacheCompress, bool cacheResourceSharing, bool cacheMultilingualSounds, bool cacheRemasteredSupport, bool cacheMPTagSharinge)
        {
            await RunTool(ToolType.Tool, new List<string>() { "build-cache-file", scenario.Replace(".scenario", ""), "pc" });
        }

        public override async Task BuildLightmap(string scenario, string bsp, LightmapArgs args, ICancellableProgress<int>? progress)
        {
            if (progress is not null)
            {
                progress.DisableCancellation();
                progress.MaxValue += 1;
            }

            if (bsp != "")
            {
                if (args.lightmapGlobals != "")
                {
                    await RunTool(args.NoAssert ? ToolType.ToolFast : ToolType.Tool, new List<string>() { "faux_lightmap_with_settings", scenario, bsp, "true", "false", args.lightmapGlobals });
                }
                else
                {
                    await RunTool(args.NoAssert ? ToolType.ToolFast : ToolType.Tool, new List<string>() { "faux_lightmap_farm", scenario, bsp, "false", "false" });
                }
                
            }
            else
            {
                if (args.lightmapGlobals != "")
                {
                    await RunTool(args.NoAssert ? ToolType.ToolFast : ToolType.Tool, new List<string>() { "faux_lightmap_with_settings_for_all", scenario, bsp, "true", "false", args.lightmapGlobals });
                }
                else
                {
                    await RunTool(args.NoAssert ? ToolType.ToolFast : ToolType.Tool, new List<string>() { "faux_lightmap_farm_for_all", scenario, bsp, "false", "false" });
                }
            }
            
            if (progress is not null)
                progress.Report(1);
        }

        public override string GetDocumentationName()
        {
            return "HR";
        }
    }
}