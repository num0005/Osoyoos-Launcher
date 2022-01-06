using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolkitLauncher.ToolkitInterface;

namespace ToolkitLauncher.Utility
{
    class AutoFBX
    {

        public static async Task Structure(ToolkitBase tk, string filePath, Boolean isJMS)
        {
            /* Convert Structure */
            string inPath = tk.GetDataDirectory() + "\\" + filePath.ToLower().Replace(isJMS?".jms":".ass", ".fbx"); // Jank code, should probably make this safer
            if (File.Exists(inPath))
            {
                string outPath = tk.GetDataDirectory() + "\\" + filePath;
                await tk.RunTool(ToolType.Tool, new() { "fbx-to-ass", inPath, outPath });
            }
        }

        public static async Task Model(ToolkitBase tk, string path, ModelCompile importType)
        {
            /* Check for x.all.fbx in root directory. */
            List<FileInfo> alls = new List<FileInfo>();
            string rootDir = tk.GetDataDirectory() + "\\" + path;

            string[] rootFilePaths = Directory.GetFiles(rootDir, "*.all.fbx");
            foreach (string f in rootFilePaths)
            {
                FileInfo fi = new FileInfo(f);
                alls.Add(fi);
            }

            /* Convert x.all.fbx */
            foreach (FileInfo all in alls)
            {
                string outPath = rootDir + "\\" + (all.Name.ToLower().Replace(".all.fbx", ".jms"));
                await tk.RunTool(ToolType.Tool, new() { "fbx-to-jms", "all", all.FullName, outPath });
            }

            /* Convert render/collision/physics */
            foreach (ToolkitBase.ImportTypeInfo importTypeInfo in tk.GetImportTypeInfo())
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
                            string inPath = rootDir + "\\" + (all.Name.ToLower().Replace(".all.fbx", "_" + importTypeInfo.folderName + ".jms")); // Lowercase enforce to avoid .FBX <-> .fbx mismatches
                            string outPath = typeDir + "\\" + (all.Name.ToLower().Replace(".all.fbx", ".jms"));

                            string checkPath = typeDir + "\\" + (all.Name.ToLower().Replace(".all.fbx", ".fbx"));
                            if (File.Exists(checkPath)) { continue; } // Skip x.all.fbx if there is an x.fbx with same name in the relevant subfolder

                            if (File.Exists(outPath)) { File.Delete(outPath); }

                            File.Move(inPath, outPath);
                        }
                    }
                    /* Convert fbx in relevant sub folders */
                    if (Directory.Exists(typeDir))
                    {
                        List<FileInfo> fbxs = new List<FileInfo>();

                        string[] filePaths = Directory.GetFiles(typeDir, "*.fbx");
                        foreach (string f in filePaths)
                        {
                            FileInfo fi = new FileInfo(f);
                            fbxs.Add(fi);
                        }

                        foreach (FileInfo fbx in fbxs)
                        {
                            string outPath = typeDir + "\\" + (fbx.Name.ToLower().Replace(".fbx", ".jms")); // Lowercase enforce to avoid .FBX <-> .fbx mismatches
                            await tk.RunTool(ToolType.Tool, new() { "fbx-to-jms", importTypeInfo.commandOption, fbx.FullName, outPath });
                        }
                    }
                }
            }

            /* Delete any unused .jms files in root directory converted by 'all' */
            foreach (FileInfo all in alls)
            {
                foreach (ToolkitBase.ImportTypeInfo importTypeInfo in tk.GetImportTypeInfo())
                {
                    string fp = rootDir + "\\" + all.Name.ToLower().Replace(".all.fbx", "_" + importTypeInfo.folderName + ".jms"); // Lowercase enforce to avoid .FBX <-> .fbx mismatches
                    if (File.Exists(fp)) { File.Delete(fp); }
                }
            }

            /* Convert animations */
            string animDir = rootDir + "\\animations";
            if (Directory.Exists(animDir) && importType.HasFlag(ModelCompile.animations))
            {
                List<FileInfo> fbxs = new List<FileInfo>();

                string[] filePaths = Directory.GetFiles(animDir, "*.fbx");
                foreach (string f in filePaths)
                {
                    FileInfo fi = new FileInfo(f);
                    fbxs.Add(fi);
                }

                foreach (FileInfo fbx in fbxs)
                {
                    string outPath = animDir + "\\" + (fbx.Name.ToLower().Replace(".fbx", ".jma")); // Lowercase enforce to avoid .FBX <-> .fbx mismatches
                    await tk.RunTool(ToolType.Tool, new() { "fbx-to-jma", fbx.FullName, outPath });
                }
            }
        }
    }
}
