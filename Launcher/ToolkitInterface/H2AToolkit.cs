﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToolkitLauncher.Properties;
using ToolkitLauncher.Utility;

namespace ToolkitLauncher.ToolkitInterface
{
    public class H2AToolkit : H2Toolkit, IToolkitFBX2Jointed, IToolkitFBX2ASS, IToolkitFBX2JMI
    {

        public H2AToolkit(ToolkitProfiles.ProfileSettingsLauncher profle, string baseDirectory, Dictionary<ToolType, string> toolPaths) : base(profle, baseDirectory, toolPaths) { }

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
                    return Path.Join(BaseDirectory, tagPath);
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
                    return Path.Join(BaseDirectory, dataPath);
            }
            return base.GetDataDirectory();
        }

        protected override string sapienWindowClass
        {
            get => "Sapien";
        }

        override public async Task ImportBitmaps(string path, string type, string compression, bool should_clear_old_usage, bool debug_plate)
        {
            await RunTool(ToolType.Tool, new() { "bitmaps", path, type, debug_plate.ToString() });
        }

        override public async Task ImportStructure(StructureType structure_command, string data_file, bool phantom_fix, bool release, bool useFast, bool autoFBX, ImportArgs import_args)
        {
            if (autoFBX) { await AutoFBX.Structure(this, data_file, false); }

            ToolType tool = useFast ? ToolType.ToolFast : ToolType.Tool;
            bool is_ass_file = data_file.ToLowerInvariant().EndsWith("ass");
            string command = is_ass_file ? "structure-new-from-ass" : "structure-from-jms";
            await RunTool(tool, new List<string>() { command, data_file });
        }

        static private string SetFlag(string flag_string, string flag_name)
        {
            if (!String.IsNullOrEmpty(flag_string))
            {
                flag_string += "|";
            }
            flag_string += flag_name;

            return flag_string;
        }

        private string _get_prt_tool_path()
        {
            return Path.Join(BaseDirectory, PRTSimInstaller.prt_executable_file_path);
        }

        private async Task CheckPRTToolDeployment()
        {
            string prt_tool_path = _get_prt_tool_path();

            bool should_update_prt = false;
            if (!File.Exists(prt_tool_path))
            {
                // no tool, clear the version data as it's inaccurate
                Profile.LatestPRTToolVersion = null;
                ToolkitProfiles.Save();

                var result = MessageBox.Show(
                    "PRT related operations will fail until it is installed, do you want to do that now?", 
                    "PRT Simulation Tool not installed!",
                    MessageBoxButtons.YesNo);
                should_update_prt = result == DialogResult.Yes;
            }
            else if (!PRTSimInstaller.IsRedistInstalled())
            {
                var result = MessageBox.Show(
                    "PRT installation is incomplete (missing redist), do you want to reinstall it now?",
                    "D3DX Redist Package Missing!",
                    MessageBoxButtons.YesNo);
                should_update_prt = result == DialogResult.Yes;
            }
            else
            {

                if (Profile.LatestPRTToolVersion is not null)
                {
                    if (Profile.LatestPRTToolVersion < (Settings.Default.newest_prt_sim_version ?? 0))
                    {
                        var result = MessageBox.Show(
                            $"A newer version ({Settings.Default.newest_prt_sim_version}) of prt_sim has been installed for other toolkits, do you want to update from {Profile.LatestPRTToolVersion} to latest?",
                            "PRT Simulation Tool Update Available",
                            MessageBoxButtons.YesNo);
                        should_update_prt = result == DialogResult.Yes;
                    }
                }
                else
                {
                    Trace.WriteLine("Running untracked prt_sim, might be outdated, or unoffical build");
                    // untracked PRT tool version, there's nothing we can do now
                }
            }

            if (should_update_prt)
            {
                int? installed_version = await PRTSimInstaller.Install(prt_tool_path);
                if (installed_version is null)
                {
                    MessageBox.Show(
                    "Failed to install PRT simulation tool!",
                    "Error",
                    MessageBoxButtons.OK);
                }
                else
                {
                    Profile.LatestPRTToolVersion = installed_version;
                    ToolkitProfiles.Save();
                }
            }

        }

        public override async Task ImportModel(string path, ModelCompile importType, bool phantomFix, bool h2SelectionLogic, bool renderPRT, bool FPAnim, string characterFPPath, string weaponFPPath, bool accurateRender, bool verboseAnim, bool uncompressedAnim, bool skyRender, bool PDARender, bool resetCompression, bool autoFBX, bool genShaders)
        {
            string flags = "";
            if (verboseAnim)
            {
                flags = SetFlag(flags, "verbose");
            }
            if (uncompressedAnim)
            {
                flags = SetFlag(flags, "uncompressed");
            }
            if (resetCompression)
            {
                flags = SetFlag(flags, "reset_compression");
            }

            if (autoFBX) { await AutoFBX.Model(this, path, importType); }

            if (importType.HasFlag(ModelCompile.render))
            {
                // Generate shaders if requested
                if (genShaders) { if (! await AutoShaders.CreateEmptyShaders(GetTagDirectory(), GetDataDirectory(), path, "H2")) { return; }; }

                // check PRT tool is setup before we use it
                if (renderPRT)
                {
                    await CheckPRTToolDeployment();
                }

                List<string> args = new() {
                    "render",
                    path,
                    accurateRender ? "true" : "false",
                    renderPRT ? "true" : "false" };
                await RunTool(ToolType.Tool, args);

            }
            if (importType.HasFlag(ModelCompile.collision))
            {
                await RunTool(ToolType.Tool, new() { "collision", path });
            }
            if (importType.HasFlag(ModelCompile.physics))
            {
                await RunTool(ToolType.Tool, new() { "physics", path });
            }
            if (importType.HasFlag(ModelCompile.animations))
            {
                if (FPAnim)
                    await RunTool(ToolType.Tool, new() {
                        "fp-model-animations",
                        path,
                        characterFPPath,
                        weaponFPPath,
                        flags });
                else
                    await RunTool(ToolType.Tool, new() {
                        "model-animations",
                        path,
                        flags });
            }
        }

        public override async Task ImportSound(string path, string platform, string bitrate, string ltf_path, string sound_command, string class_type, string compression_type, string custom_extension)
        {
            await RunTool(ToolType.Tool, new List<string>() { sound_command.Replace("_", "-"), path, class_type, compression_type });
        }

        public override async Task BuildCache(string scenario, CacheType cacheType, ResourceMapUsage resourceUsage, bool logTags, string cachePlatform, bool cacheCompress, bool cacheResourceSharing, bool cacheMultilingualSounds, bool cacheRemasteredSupport, bool cacheMPTagSharing)
        {
            string flags = "";
            if (cacheCompress)
            {
                flags = SetFlag(flags, "compress");
            }
            if (cacheResourceSharing)
            {
                flags = SetFlag(flags, "resource_sharing");
            }
            if (cacheMultilingualSounds)
            {
                flags = SetFlag(flags, "multilingual_sounds");
            }
            if (cacheRemasteredSupport)
            {
                flags = SetFlag(flags, "remastered_support");
            }
            if (cacheMPTagSharing)
            {
                flags = SetFlag(flags, "mp_tag_sharing");
            }

            List<string> args = new List<string>();
            args.Add("build-cache-file");
            args.Add(scenario.Replace(".scenario", ""));
            args.Add(cachePlatform);
            if (flags != "")
                args.Add(flags);

            await RunTool(ToolType.Tool, args);
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
        /// <param name="geo_class"></param>
        /// <returns></returns>
        public async Task JMSFromFBX(string fbxPath, string jmsPath, string geo_class)
        {
            await RunTool(ToolType.Tool, new() { "fbx-to-jms", geo_class, fbxPath, jmsPath });
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

        public override async Task ExtractTags(string path)
        {
            string[] pathandExtension = { path.Substring(0, path.LastIndexOf('.')), path.Substring(path.LastIndexOf('.')) };
            switch (pathandExtension[1])
            {
                case ".scenario_structure_bsp":
                    await RunTool(ToolType.Tool, new List<string>() { "extract-structure-data", pathandExtension[0] }, OutputMode.closeShell);
                    break;
                case ".render_model":
                    await RunTool(ToolType.Tool, new List<string>() { "extract-render-data", pathandExtension[0] }, OutputMode.closeShell);
                    break;
                case ".physics_model":
                    await RunTool(ToolType.Tool, new List<string>() { "extract-physics-data", pathandExtension[0] }, OutputMode.closeShell);
                    break;
                case ".collision_model":
                    await RunTool(ToolType.Tool, new List<string>() { "extract-collision-data", pathandExtension[0] }, OutputMode.closeShell);
                    break;
                case ".bitmap":
                    await RunTool(ToolType.Tool, new List<string>() { "extract-bitmap-dds", pathandExtension[0] }, OutputMode.closeShell);
                    break;
                case ".multilingual_unicode_string_list":
                    await RunTool(ToolType.Tool, new List<string>() { "extract-unicode-strings", pathandExtension[0] }, OutputMode.closeShell);
                    break;
            }
        }

        public override bool IsMutexLocked(ToolType tool)
        {
            // todo(num0005) implement this
            return false;
        }

        public override string GetDocumentationName()
        {
            return "H2MCC";
        }
    }
}
