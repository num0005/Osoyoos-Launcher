using System.Collections.Generic;
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
        public override async Task ImportModel(string path, ModelCompile importType, bool phantomFix, bool h2SelectionLogic, bool renderPRT, bool FPAnim, string characterFPPath, string weaponFPPath, bool accurateRender, bool verboseAnim, bool uncompressedAnim, bool skyRender, bool PDARender, bool resetCompression, bool autoFBX, bool genShaders)
        {
            if (autoFBX) { await AutoFBX.Model(this, path, importType); }

            List<string> args = new List<string>();
            if (importType.HasFlag(ModelCompile.render))
            {
                // Generate shaders if requested
                if (genShaders) { if (!AutoShaders.CreateEmptyShaders(BaseDirectory, path, "H3ODST")) { return; }; }
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
                else if (PDARender)
                {
                    args.Add("render-pda");
                    args.Add(path);
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

            await RunTool(ToolType.Tool, args, true);
        }
    }
}