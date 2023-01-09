﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ToolkitLauncher.Utility;
using static ToolkitLauncher.ToolkitProfiles;

namespace ToolkitLauncher.ToolkitInterface
{
    public class H2AToolkit : H2Toolkit, IToolkitFBX2Jointed, IToolkitFBX2ASS, IToolkitFBX2JMI
    {

        public H2AToolkit(ProfileSettingsLauncher profle, string baseDirectory, Dictionary<ToolType, string> toolPaths) : base(profle, baseDirectory, toolPaths) { }

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
            get => "Sapien";
        }

        override public async Task ImportBitmaps(string path, string type, bool debug_plate)
        {
            await RunTool(ToolType.Tool, new() { "bitmaps", path, type, debug_plate.ToString() });
        }

        override public async Task ImportStructure(StructureType structure_command, string data_file, bool phantom_fix, bool release, bool useFast, bool autoFBX, ImportArgs import_args)
        {
            if (autoFBX) { await AutoFBX.Structure(this, data_file, false); }

            ToolType tool = useFast ? ToolType.ToolFast : ToolType.Tool;
            bool is_ass_file = data_file.ToLowerInvariant().EndsWith("ass");
            string command = is_ass_file ? "structure-new-from-ass" : "structure-from-jms";
            await RunTool(tool, new List<string>() { command, data_file }, true);
        }

        private string set_flags(string flag_string, string flag_name)
        {
            if (!String.IsNullOrEmpty(flag_string))
            {
                flag_string += "|";
            }
            flag_string += flag_name;

            return flag_string;
        }

        public override async Task ImportModel(string path, ModelCompile importType, bool phantomFix, bool h2SelectionLogic, bool renderPRT, bool FPAnim, string characterFPPath, string weaponFPPath, bool accurateRender, bool verboseAnim, bool uncompressedAnim, bool skyRender, bool PDARender, bool resetCompression, bool autoFBX, bool genShaders)
        {
            string flags = "";
            if (verboseAnim)
            {
                flags = set_flags(flags, "verbose");
            }
            if (uncompressedAnim)
            {
                flags = set_flags(flags, "uncompressed");
            }
            if (resetCompression)
            {
                flags = set_flags(flags, "reset_compression");
            }

            if (autoFBX) { await AutoFBX.Model(this, path, importType); }

            List<string> args = new List<string>();
            if (importType.HasFlag(ModelCompile.render))
            {
                // Generate shaders if requested
                if (genShaders) { if (!AutoShaders.CreateEmptyShaders(BaseDirectory, path, "H2")) { return; }; }
                args.Add("render");
                args.Add(path);
                if (accurateRender)
                {
                    args.Add("true");
                }
                if (renderPRT)
                {
                    args.Add("true");
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
                    args.Add("fp-model-animations");
                    args.Add(path);
                    args.Add(characterFPPath);
                    args.Add(weaponFPPath);
                    args.Add(flags);
                }
                else
                {
                    args.Add("model-animations");
                    args.Add(path);
                    args.Add(flags);
                }
            }

            await RunTool(ToolType.Tool, args, true);
        }

        public override async Task ImportSound(string path, string platform, string bitrate, string ltf_path, string sound_command, string class_type, string compression_type)
        {
            await RunTool(ToolType.Tool, new List<string>() { sound_command.Replace("_", "-"), path, class_type, compression_type });
        }

        public override async Task BuildCache(string scenario, CacheType cacheType, ResourceMapUsage resourceUsage, bool logTags, string cachePlatform, bool cacheCompress, bool cacheResourceSharing, bool cacheMultilingualSounds, bool cacheRemasteredSupport, bool cacheMPTagSharing)
        {
            string flags = "";
            if (cacheCompress)
            {
                flags = set_flags(flags, "compress");
            }
            if (cacheResourceSharing)
            {
                flags = set_flags(flags, "resource_sharing");
            }
            if (cacheMultilingualSounds)
            {
                flags = set_flags(flags, "multilingual_sounds");
            }
            if (cacheRemasteredSupport)
            {
                flags = set_flags(flags, "remastered_support");
            }
            if (cacheMPTagSharing)
            {
                flags = set_flags(flags, "mp_tag_sharing");
            }

            List<string> args = new List<string>();
            args.Add("build-cache-file");
            args.Add(scenario.Replace(".scenario", ""));
            args.Add(cachePlatform);
            if (flags != "")
                args.Add(flags);

            await RunTool(ToolType.Tool, args, true);
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
