using Bungie;
using Bungie.Tags;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OsoyoosMB
{
    internal class BitmapSettings
    {
        public static void GetBitmapData(string ek_path, string tag_folder, string compress_value)
        {
            // Get all bitmaps
            string tag_folder_full = Path.Combine(ek_path, "tags", tag_folder);
            string[] all_bitmaps = Directory.GetFiles(tag_folder_full, "*.bitmap");

            // Define bitmap name suffixes for anything non-diffuse
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

            ApplyBitmSettings(diffuse_bitmaps.ToArray(), normal_bitmaps.ToArray(), bump_bitmaps.ToArray(), material_bitmaps.ToArray(), ek_path, int.Parse(compress_value));
        }

        public static void ApplyBitmSettings(string[] diffuses, string[] normals, string[] bumps, string[] materials, string ek_path, int compress_value)
        {
            // EK "tags" folder location
            string base_path = Path.Combine(ek_path, "tags");

            // Initialize ManagedBlam
            ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, ek_path);

            foreach (string bitmap_full in diffuses)
            {
                // Get correctly formatted path by only taking tags-relative path and removing extension
                string bitmap_path = Path.ChangeExtension(PathNetCore.GetRelativePath(base_path, bitmap_full), null);

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

            foreach (string bitmap_full in normals)
            {
                // Get correctly formatted path by only taking tags-relative path and removing extension
                string bitmap_path = Path.ChangeExtension(PathNetCore.GetRelativePath(base_path, bitmap_full), null);

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

            foreach (string bitmap_full in bumps)
            {
                // Get correctly formatted path by only taking tags-relative path and removing extension
                string bitmap_path = Path.ChangeExtension(PathNetCore.GetRelativePath(base_path, bitmap_full), null);

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

            foreach (string bitmap_full in materials)
            {
                // Get correctly formatted path by only taking tags-relative path and removing extension
                string bitmap_path = Path.ChangeExtension(PathNetCore.GetRelativePath(base_path, bitmap_full), null);

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
    }
}
