using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolkitLauncher.ToolkitInterface;
using UkooLabs.FbxSharpie;

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

        public static async Task Model(ToolkitBase tk, string path, ModelCompile importType, bool isHalo1)
        {
            /* Check for x.all.fbx in root directory. */
            List<FileInfo> alls = new List<FileInfo>();
            string rootDir = tk.GetDataDirectory() + "\\" + path;

            if (!isHalo1) // Don't bother doing this for Halo 1 as the fbx-to-jms tool works differntly for that one
            {
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

                    try
                    {
                        string stripInPath = StripFBXCollision(all.FullName);
                        string stripOutPath = outPath.Replace(".jms", "_render.jms");
                        await tk.RunTool(ToolType.Tool, new() { "fbx-to-jms", "render", stripInPath, stripOutPath });
                        if (File.Exists(stripInPath)) { File.Delete(stripInPath); } // Done with this file so delete
                    }
                    catch(Exception ex)
                    {
                        // Not sure how you want to handle errors of this type.
                        // If my FBX parse/strip/write/convert fails for whatever reason this catch (should) get it
                    }
                }
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

                            if (File.Exists(outPath)) { File.Delete(outPath); } // Delete file at outpath because overwrite throws an exception

                            if (File.Exists(inPath)) { File.Move(inPath, outPath); } // Move file as long as it exists, a messed up fbx won't output a jms so we check before moving
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

        /* Returns path to fbx file with stripped collision and physics data */
        public static string StripFBXCollision(string path)
        {
            FbxDocument fbx = FbxIO.Read(path);

            string GetTokenValueAsString(UkooLabs.FbxSharpie.Tokens.Token token)
            {
                if(token is UkooLabs.FbxSharpie.Tokens.StringToken)
                {
                    return ((UkooLabs.FbxSharpie.Tokens.StringToken)token).Value;
                }
                if (token is UkooLabs.FbxSharpie.Tokens.Value.IntegerToken)
                {
                    return " " + ((UkooLabs.FbxSharpie.Tokens.Value.IntegerToken)token).Value;
                }
                if (token is UkooLabs.FbxSharpie.Tokens.Value.LongToken)
                {
                    return " " + ((UkooLabs.FbxSharpie.Tokens.Value.LongToken)token).Value;
                }
                return "{value}";
            }

            long GetTokenValueAsLong(UkooLabs.FbxSharpie.Tokens.Token token)
            {
                if (token is UkooLabs.FbxSharpie.Tokens.Value.IntegerToken)
                {
                    return ((UkooLabs.FbxSharpie.Tokens.Value.IntegerToken)token).Value;
                }
                if (token is UkooLabs.FbxSharpie.Tokens.Value.LongToken)
                {
                    return ((UkooLabs.FbxSharpie.Tokens.Value.LongToken)token).Value;
                }
                return 0;
            }

            FbxNode GetNodeByIdentifierValue(FbxDocument f, string identifier)
            {
                foreach (FbxNode n in f.Nodes)
                {
                    if(n.Identifier.Value.Equals(identifier))
                    {
                        return n;
                    }
                }
                return null; // Doooooom
            }

            /* Step #1 - Search "Objects" for any Models with a name starting with '@' or '$' and then grab the id values for it */
            List<FbxNode> StNodes5a = new List<FbxNode>();
            List<long> ModelIDs = new List<long>();
            foreach(FbxNode node in GetNodeByIdentifierValue(fbx, "Objects").Nodes)
            {
                if(node != null && node.Properties != null && node.Properties.Length > 2)
                {
                    string name = GetTokenValueAsString(node.Properties[1]);
                    long id = GetTokenValueAsLong(node.Properties[0]);
                    if (name.StartsWith("Model::@") || name.StartsWith("Model::$"))
                    {
                        ModelIDs.Add(id);
                        continue;
                    }
                }
                StNodes5a.Add(node);
            }

            /* Step #2 - Search "Connections" for any entries containing the any ModelIDs we collected in property slot #2, store the value from slot #1 as that is the child id for the mesh */
            List<FbxNode> StNodes6a = new List<FbxNode>();
            List<long> MeshIDs = new List<long>();
            foreach (FbxNode node in GetNodeByIdentifierValue(fbx, "Connections").Nodes)
            {
                if (node != null && node.Properties != null && node.Properties.Length > 2)
                {
                    long id1 = GetTokenValueAsLong(node.Properties[1]);
                    long id2 = GetTokenValueAsLong(node.Properties[2]);
                    if (ModelIDs.Contains(id2))
                    {
                        MeshIDs.Add(id1);
                        continue;
                    }
                }
                StNodes6a.Add(node);
            }

            /* Step #3 - Search "Object Definitions" for any entries with the strings 'Geometry' and 'Mesh' in property slots #1 and #2 */
            List<FbxNode> StNodes5b = new List<FbxNode>();
            foreach (FbxNode node in StNodes5a)
            {
                if (node != null && node.Properties != null && node.Properties.Length > 2)
                {
                    long id = GetTokenValueAsLong(node.Properties[0]);
                    string slot1 = GetTokenValueAsString(node.Properties[1]);
                    string slot2 = GetTokenValueAsString(node.Properties[2]);
                    if (slot1.ToLower().Contains("geometry") && slot2.ToLower().Contains("mesh") && MeshIDs.Contains(id))
                    {
                        continue;
                    }
                }
                StNodes5b.Add(node);
            }

            /* Step #4 - Build a new FBX file with the modified nodelist we generated in StNodes */
            FbxDocument nufbx = new FbxDocument();
            foreach(FbxNode node in fbx.Nodes)
            {
                if (node.Identifier.Value.Equals("Objects"))
                {
                    FbxNode nunode = new FbxNode(node.Identifier);
                    foreach(FbxNode n in StNodes5b) { nunode.AddNode(n); }
                    foreach (UkooLabs.FbxSharpie.Tokens.Token property in node.Properties) { nunode.AddProperty(property); }
                    nunode.Value = node.Value;
                    nufbx.AddNode(nunode);
                }
                else if (node.Identifier.Value.Equals("Connections"))
                {
                    FbxNode nunode = new FbxNode(node.Identifier);
                    foreach (FbxNode n in StNodes6a) { nunode.AddNode(n); }
                    foreach (UkooLabs.FbxSharpie.Tokens.Token property in node.Properties) { nunode.AddProperty(property); }
                    nunode.Value = node.Value;
                    nufbx.AddNode(nunode);
                }
                else
                {
                    nufbx.AddNode(node);
                }
            }

            string outPath = path.Replace(".fbx", ".stripped.fbx");
            FbxIO.WriteAscii(nufbx, outPath);
            return outPath;
        }
    }
}
