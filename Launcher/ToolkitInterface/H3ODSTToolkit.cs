using System;
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
    public class H3ODSTToolkit : H3Toolkit, IToolkitFBX2Jointed, IToolkitFBX2ASS, IToolkitFBX2JMI
    {

        public H3ODSTToolkit(ProfileSettingsLauncher profile, string baseDirectory, Dictionary<ToolType, string> toolPaths) : base(profile, baseDirectory, toolPaths) { }

        /// <summary>
        /// Import a model
        /// </summary>
        /// <param name="path"></param>
        /// <param name="importType"></param>
        /// <returns></returns>
        public override async Task ImportModel(string path, ModelCompile importType, bool phantomFix, bool h2SelectionLogic, bool renderPRT, bool FPAnim, string characterFPPath, string weaponFPPath, bool accurateRender, bool verboseAnim, bool uncompressedAnim, bool skyRender, bool PDARender, bool resetCompression, bool autoFBX)
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

            if(autoFBX) { await AutoFBX.Model(this, path, importType); }

            if (importType.HasFlag(ModelCompile.render))
                if (skyRender)
                    await RunTool(ToolType.Tool, new() { "render-sky", path });
                else if (accurateRender)
                    await RunTool(ToolType.Tool, new() { "render-accurate", path, renderPRT ? "final" : "draft" });
                else if (PDARender)
                    await RunTool(ToolType.Tool, new() { "render-pda", path });
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
    }
}