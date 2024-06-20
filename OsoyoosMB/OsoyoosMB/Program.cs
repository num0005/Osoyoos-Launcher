/*
BUILD INFORMATION

This project uses ManagedBlam.dll, but that can't be added to the GitHub repo.
In order to still be built, the project uses a Reference Assembly version
of the ManagedBlam DLL. This is generated using the NetBrains tool Refasmer.

In theory it already contains all namespaces/methods etc present in the full DLL.
In case it needs to be regenerated in future howerver:

Refasmer can be installed from the terminal with "dotnet tool install -g JetBrains.Refasmer.CliTool"
Once installed, run with "refasmer -v -O ref -c ManagedBlam.dll"
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Bungie;
using Bungie.Tags;

namespace OsoyoosMB
{
    internal class MBHandler
    {
        
        public static void Main(String[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Do not run this manually, it is a helper executable for Osoyoos. This is not a standalone application.\nPress Enter to exit.");
                Console.ReadLine();

            }
            else
            {
                if (args[0] == "getbitmapdata" && args.Length >= 4)
                {
                    Console.WriteLine("Running GetBitmapData");
                    GetBitmapData(args[1], args[2], args[3]);
                }
                else if (args[0] == "generateshaders" && args.Length >= 3)
                {
                    Console.WriteLine("Running ShaderGenerator");
                    ShaderGenerator(args[1], args[2]);
                }
                else
                {
                    Console.WriteLine("Insufficient arguments");
                }
            }
        }
        

        /*
        //Use this if you need to debug GetBitmapData(), can't debug when run from the main Osoyoos solution
        public static void Main()
        {
            GetBitmapData(@"C:\Program Files (x86)\Steam\steamapps\common\H3EK", @"objects\scenery\minecraft_door\bitmaps", "Uncompressed");
        }
        */

        /*
        //Use this if you need to debug this GenerateShaders, can't debug when run from the main Osoyoos solution
        public static void Main()
        {
            string[] shaders = { "diffuse", "minecraft_stone" };
            ShaderHandler(@"C:\Program Files (x86)\Steam\steamapps\common\H3EK", @"C:\Program Files (x86)\Steam\steamapps\common\H3EK\tags\objects\scenery\minecraft_door\shaders");
        }
        */

        public static void GetBitmapData(string ek_path, string tag_folder, string compression_type)
        {
            // Convert compression type string to correct Enum setting
            Dictionary<string, int> comp_mapping = new Dictionary<string, int>
            {
                { "Default", 0 },
                { "Uncompressed", 2 },
                { "Best Compressed Color", 3 },
                { "DXT1", 13 },
                { "DXT5", 15 },
                { "24-bit Color + 8-bit Alpha", 16 }
            };

            int compress_value = comp_mapping[compression_type];

            // Get all bitmaps
            string tag_folder_full = Path.Combine(ek_path, "tags", tag_folder);
            string[] all_bitmaps = Directory.GetFiles(tag_folder_full, "*.bitmap");

            // Ignore bitmaps that aren't diffuse textures
            string[] non_diffuse_suffixes = {
                "_3d.bitmap",
                "_blend.bitmap",
                "_bump.bitmap",
                "_bmp.bitmap",
                "_bp.bitmap",
                "_b.bitmap",
                "_cube.bitmap",
                "_detailbump.bitmap",
                "_dsprite.bitmap",
                "_float.bitmap",
                "_height.bitmap",
                "_lactxl.bitmap",
                "_ladxn.bitmap",
                "_material.bitmap",
                "_materialmap.bitmap",
                "_mat.bitmap",
                "_m.bitmap",
                "_orm.bitmap",
                "_ormh.bitmap",
                "_rmo.bitmap",
                "_rmoh.bitmap",
                "_mro.bitmap",
                "_mroh.bitmap",
                "_msprite.bitmap",
                "_normal.bitmap",
                "_normalmap.bitmap",
                "_nm.bitmap",
                "_n.bitmap",
                "_sprite.bitmap",
                "_ui.bitmap",
                "_vec.bitmap",
                "_warp.bitmap",
                "_zbump.bitmap"
            };

            string[] normal_suffixes =
            {
                "_normal.bitmap",
                "_normalmap.bitmap",
                "_nm.bitmap",
                "_n.bitmap",
                "_zbump.bitmap"
            };

            string[] bump_suffixes =
            {
                "_bump.bitmap",
                "_bmp.bitmap",
                "_bp.bitmap",
                "_b.bitmap"
            };

            string[] material_suffixes =
            {
                "_material.bitmap",
                "_materialmap.bitmap",
                "_mat.bitmap",
                "_m.bitmap",
                "_orm.bitmap",
                "_ormh.bitmap",
                "_rmo.bitmap",
                "_rmoh.bitmap",
                "_mro.bitmap",
                "_mroh.bitmap"
            };

            List<string> diffuse_bitmaps = new List<string>();
            List<string> normal_bitmaps = new List<string>();
            List<string> bump_bitmaps = new List<string>();
            List<string> material_bitmaps = new List<string>();

            foreach (string bitmap in all_bitmaps)
            {
                if (normal_suffixes.Any(suffix => bitmap.EndsWith(suffix)))
                {
                    // Bitmap file is a normal map
                    normal_bitmaps.Add(bitmap);
                }
                else if (bump_suffixes.Any(suffix => bitmap.EndsWith(suffix)))
                {
                    // Bitmap file is a bump map
                    bump_bitmaps.Add(bitmap);
                }
                else if (material_suffixes.Any(suffix => bitmap.EndsWith(suffix)))
                {
                    // Bitmap file is a material map
                    material_bitmaps.Add(bitmap);
                }
                else
                {
                    // Treat bitmap as diffuse
                    diffuse_bitmaps.Add(bitmap);
                }
            }

            // Initialize ManagedBlam
            ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, ek_path);

            foreach (string bitmap_full in diffuse_bitmaps)
            {
                // Get correctly formatted path by only taking tags-relative path and removing extension
                int startIndex = bitmap_full.IndexOf("tags\\");
                string bitmap_path = bitmap_full.Substring(startIndex + 5).Replace(".bitmap", "");

                var tag_path = TagPath.FromPathAndType(bitmap_path, "bitm*");

                using (var tagFile = new TagFile(tag_path))
                {
                    // Set curve mode to pretty
                    var curve = (TagFieldEnum)tagFile.SelectField("CharEnum:curve mode");
                    curve.Value = 2;

                    // Set compression to UI-selected value
                    var compression = (TagFieldEnum)tagFile.SelectField("ShortEnum:force bitmap format");
                    compression.Value = compress_value;

                    // Set max mipmap to -1
                    var mip_limit = (TagFieldElementInteger)tagFile.SelectField("CharInteger:max mipmap level");
                    mip_limit.Data = -1;

                    // Check if bitmap already has overrides entry, if so remove it
                    int override_count = ((TagFieldBlock)tagFile.SelectField("Block:usage override")).Elements.Count();
                    if (override_count > 0)
                    {
                        ((TagFieldBlock)tagFile.SelectField("Block:usage override")).RemoveAllElements();
                    }

                    // Add override entry
                    ((TagFieldBlock)tagFile.SelectField("Block:usage override")).AddElement();

                    // Set gamma
                    var gamma = (TagFieldElementSingle)tagFile.SelectField("Block:usage override[0]/Real:source gamma");
                    gamma.Data = 2.2f;

                    // Set bitmap curve to sRGB
                    var bitmap_curve = (TagFieldEnum)tagFile.SelectField("Block:usage override[0]/LongEnum:bitmap curve");
                    bitmap_curve.Value = 5;

                    // Set ignore curve override flag
                    var flags = (TagFieldFlags)tagFile.SelectField("Block:usage override[0]/ByteFlags:flags");
                    flags.RawValue = 1;

                    // Set mipmap limit
                    var mip_limit_override = (TagFieldElementInteger)tagFile.SelectField("Block:usage override[0]/ShortInteger:mipmap limit");
                    mip_limit_override.Data = -1;

                    // Set compression to UI-selected value
                    var override_compression = (TagFieldEnum)tagFile.SelectField("Block:usage override[0]/LongEnum:bitmap format");
                    override_compression.Value = compress_value;

                    tagFile.Save();
                }
            }

            foreach (string bitmap_full in normal_bitmaps)
            {
                // Get correctly formatted path by only taking tags-relative path and removing extension
                int startIndex = bitmap_full.IndexOf("tags\\");
                string bitmap_path = bitmap_full.Substring(startIndex + 5).Replace(".bitmap", "");

                var tag_path = TagPath.FromPathAndType(bitmap_path, "bitm*");

                using (var tagFile = new TagFile(tag_path))
                {
                    // Set usage to zbump
                    var usage = (TagFieldEnum)tagFile.SelectField("LongEnum:Usage");
                    usage.Value = 17;

                    // Set curve mode to pretty
                    var curve = (TagFieldEnum)tagFile.SelectField("CharEnum:curve mode");
                    curve.Value = 2;

                    // Set compression to DXN
                    var compression = (TagFieldEnum)tagFile.SelectField("ShortEnum:force bitmap format");
                    compression.Value = 49;

                    // Set max mipmap to -1
                    var mip_limit = (TagFieldElementInteger)tagFile.SelectField("CharInteger:max mipmap level");
                    mip_limit.Data = -1;

                    // Check if bitmap already has overrides entry, if so remove it
                    int override_count = ((TagFieldBlock)tagFile.SelectField("Block:usage override")).Elements.Count();
                    if (override_count > 0)
                    {
                        ((TagFieldBlock)tagFile.SelectField("Block:usage override")).RemoveAllElements();
                    }

                    // Add override entry
                    ((TagFieldBlock)tagFile.SelectField("Block:usage override")).AddElement();

                    // Set gamma
                    var gamma = (TagFieldElementSingle)tagFile.SelectField("Block:usage override[0]/Real:source gamma");
                    gamma.Data = 1.0f;

                    // Set bitmap curve to linear
                    var bitmap_curve = (TagFieldEnum)tagFile.SelectField("Block:usage override[0]/LongEnum:bitmap curve");
                    bitmap_curve.Value = 3;

                    // Set ignore curve override flag
                    var flags = (TagFieldFlags)tagFile.SelectField("Block:usage override[0]/ByteFlags:flags");
                    flags.RawValue = 1;

                    // Set Unsigned flag
                    var dicer_flags = (TagFieldFlags)tagFile.SelectField("Block:usage override[0]/WordFlags:dicer flags");
                    dicer_flags.RawValue = 16;

                    // Set mipmap limit
                    var mip_limit_override = (TagFieldElementInteger)tagFile.SelectField("Block:usage override[0]/ShortInteger:mipmap limit");
                    mip_limit_override.Data = -1;

                    // Set compression to DXN
                    var override_compression = (TagFieldEnum)tagFile.SelectField("Block:usage override[0]/LongEnum:bitmap format");
                    override_compression.Value = 49;

                    tagFile.Save();
                }
            }

            foreach (string bitmap_full in bump_bitmaps)
            {
                // Get correctly formatted path by only taking tags-relative path and removing extension
                int startIndex = bitmap_full.IndexOf("tags\\");
                string bitmap_path = bitmap_full.Substring(startIndex + 5).Replace(".bitmap", "");

                var tag_path = TagPath.FromPathAndType(bitmap_path, "bitm*");

                using (var tagFile = new TagFile(tag_path))
                {
                    // Set usage to bump
                    var usage = (TagFieldEnum)tagFile.SelectField("LongEnum:Usage");
                    usage.Value = 2;

                    // Set curve mode to pretty
                    var curve = (TagFieldEnum)tagFile.SelectField("CharEnum:curve mode");
                    curve.Value = 2;

                    // Set compression to best compressed bump
                    var compression = (TagFieldEnum)tagFile.SelectField("ShortEnum:force bitmap format");
                    compression.Value = 3;

                    // Set max mipmap to -1
                    var mip_limit = (TagFieldElementInteger)tagFile.SelectField("CharInteger:max mipmap level");
                    mip_limit.Data = -1;

                    // Check if bitmap already has overrides entry, if so remove it
                    int override_count = ((TagFieldBlock)tagFile.SelectField("Block:usage override")).Elements.Count();
                    if (override_count > 0)
                    {
                        ((TagFieldBlock)tagFile.SelectField("Block:usage override")).RemoveAllElements();
                    }

                    tagFile.Save();
                }
            }

            foreach (string bitmap_full in material_bitmaps)
            {
                // Get correctly formatted path by only taking tags-relative path and removing extension
                int startIndex = bitmap_full.IndexOf("tags\\");
                string bitmap_path = bitmap_full.Substring(startIndex + 5).Replace(".bitmap", "");

                var tag_path = TagPath.FromPathAndType(bitmap_path, "bitm*");

                using (var tagFile = new TagFile(tag_path))
                {
                    // Set curve mode to pretty
                    var curve = (TagFieldEnum)tagFile.SelectField("CharEnum:curve mode");
                    curve.Value = 2;

                    // Set compression to UI-selected value
                    var compression = (TagFieldEnum)tagFile.SelectField("ShortEnum:force bitmap format");
                    compression.Value = compress_value;

                    // Set max mipmap to -1
                    var mip_limit = (TagFieldElementInteger)tagFile.SelectField("CharInteger:max mipmap level");
                    mip_limit.Data = -1;

                    // Check if bitmap already has overrides entry, if so remove it
                    int override_count = ((TagFieldBlock)tagFile.SelectField("Block:usage override")).Elements.Count();
                    if (override_count > 0)
                    {
                        ((TagFieldBlock)tagFile.SelectField("Block:usage override")).RemoveAllElements();
                    }

                    // Add override entry
                    ((TagFieldBlock)tagFile.SelectField("Block:usage override")).AddElement();

                    // Set gamma
                    var gamma = (TagFieldElementSingle)tagFile.SelectField("Block:usage override[0]/Real:source gamma");
                    gamma.Data = 1.0f;

                    // Set bitmap curve to linear
                    var bitmap_curve = (TagFieldEnum)tagFile.SelectField("Block:usage override[0]/LongEnum:bitmap curve");
                    bitmap_curve.Value = 3;

                    // Set slicer to no slicing
                    var slicer = (TagFieldEnum)tagFile.SelectField("Block:usage override[0]/CharEnum:slicer");
                    slicer.Value = 1;

                    // Set mipmap limit
                    var mip_limit_override = (TagFieldElementInteger)tagFile.SelectField("Block:usage override[0]/ShortInteger:mipmap limit");
                    mip_limit_override.Data = -1;

                    // Set compression to UI-selected value
                    var override_compression = (TagFieldEnum)tagFile.SelectField("Block:usage override[0]/LongEnum:bitmap format");
                    override_compression.Value = compress_value;

                    tagFile.Save();
                }
            }
        }
    
        public static void ShaderGenerator(string ek_path, string shaders_folder)
        {
            // Create shader directory
            Directory.CreateDirectory(shaders_folder);

            string[] shader_names = File.ReadAllLines(Path.Combine(ek_path, @"bin\shader_names.txt"));

            foreach (string shader in shader_names)
            {
                string shader_name = shaders_folder.Split(new string[] { "\\H3EK\\tags\\" }, StringSplitOptions.None)[1] + "\\" + shader;

                if (!File.Exists(Path.Combine(shaders_folder, shader_name)))
                {
                    // Initialize ManagedBlam
                    ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, ek_path);

                    TagPath tagPath = TagPath.FromPathAndType(shader_name, "rmsh*");
                    TagFile tagFile = new TagFile();
                    tagFile.New(tagPath);
                    tagFile.Save();
                }
            }
        }
    }
}
