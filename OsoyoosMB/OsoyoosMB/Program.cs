using System;
using System.Collections.Generic;
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
                Console.WriteLine("Do not run this manually, it is a helper executable for Osoyoos. This is not a standalone application");
            }
            else
            {
                if (args[0] == "getbitmapdata" && args.Length >= 4)
                {
                    GetBitmapData(args[1], args[2], args[3]);
                }
                else
                {
                    Console.WriteLine("Insufficient arguments");
                }
            }
        }
        

        /* Use this if you need to debug this code, can't debug when run from the main Osoyoos solution
        public static void Main()
        {
            GetBitmapData(@"C:\Program Files (x86)\Steam\steamapps\common\H3EK", @"objects\scenery\minecraft_door\bitmaps", "Uncompressed");
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

            // Initialize ManagedBlam
            ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, ek_path);

            foreach (string bitmap_full in all_bitmaps)
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
        }
    }
}
