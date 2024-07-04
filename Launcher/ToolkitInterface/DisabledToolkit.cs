using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using static ToolkitLauncher.ToolkitProfiles;

namespace ToolkitLauncher.ToolkitInterface
{
    public class DisabledToolkit : ToolkitBase
    {
        public DisabledToolkit(ProfileSettingsLauncher profile, string baseDirectory, Dictionary<ToolType, string> toolPaths) : base(profile, baseDirectory, toolPaths) { }
        #region stubbs
        #pragma warning disable 1998
        override public async Task ImportStructure(StructureType structure_command, string data_file, bool phantom_fix, bool release, bool useFast, bool autoFBX, ImportArgs import_args)
        {
        }

        public override async Task BuildCache(string scenario, CacheType cacheType, ResourceMapUsage resourceUsage, bool logTags, string cachePlatform, bool cacheCompress, bool cacheResourceSharing, bool cacheMultilingualSounds, bool cacheRemasteredSupport, bool cacheMPTagSharing)
        {
        }

        public override async Task BuildLightmap(string scenario, string bsp, LightmapArgs args, ICancellableProgress<int>? progress)
        {
        }

        override public async Task ImportUnicodeStrings(string path)
        {
        }

        public async Task ImportHUDStrings(string path, string scenario_name)
        {
        }

        public override async Task ImportModel(string path, ModelCompile importType, bool phantomFix, bool h2SelectionLogic, bool renderPRT, bool FPAnim, string characterFPPath, string weaponFPPath, bool accurateRender, bool verboseAnim, bool uncompressedAnim, bool skyRender, bool PDARender, bool resetCompression, bool autoFBX, bool genShaders)
        {
        }

        public override async Task ImportSound(string path, string platform, string bitrate, string ltf_path, string sound_command, string class_type, string compression_type, string custom_extension)
        {
        }

        override public async Task ImportBitmaps(string path, string type, string compression, bool should_clear_old_usage, bool debug_plate)
        {
        }

        public override async Task ExtractTags(string path)
        {
        }

        public override bool IsMutexLocked(ToolType tool)
        {
            return false;
        }

        public override string GetDocumentationName()
        {
            return "H1CE";
        }
        #pragma warning restore 1998
        #endregion

        // always disabled
        public override bool IsEnabled()
        {
            return false;
        }
    }
}
