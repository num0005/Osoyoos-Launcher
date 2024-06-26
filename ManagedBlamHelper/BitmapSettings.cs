using Bungie;
using Bungie.Tags;
using OsoyoosMB.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OsoyoosMB
{
    internal class BitmapSettings
    {
        public static void ConfigureCompression(string ek_path, string tag_folder, string ek_tags_folder_path, int compress_value)
        {
            // Initialize ManagedBlam
            ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, ek_path);

            // Makes "empty" bitmap tags
            MBHelpers.CreateDummyBitmaps(ek_path, tag_folder, ek_tags_folder_path);

            // Get all bitmap tags
            string tag_folder_full = Path.Join(ek_path, "tags", tag_folder);
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

            ApplyBitmSettings(diffuse_bitmaps.ToArray(), normal_bitmaps.ToArray(), bump_bitmaps.ToArray(), material_bitmaps.ToArray(), ek_path, compress_value);
        }

        // These are the block/field names within the tag file
        public static class TagFieldConstants
        {
            public const string Usage = "LongEnum:Usage";
            public const string CurveMode = "CharEnum:curve mode";
            public const string BitmapFormat = "ShortEnum:force bitmap format";
            public const string MipMapLevel = "CharInteger:max mipmap level";
            public const string UsageOverride = "Block:usage override";
            public const string Gamma = "Block:usage override[0]/Real:source gamma";
            public const string BitmapCurve = "Block:usage override[0]/LongEnum:bitmap curve";
            public const string Flags = "Block:usage override[0]/ByteFlags:flags";
            public const string MipLimit = "Block:usage override[0]/ShortInteger:mipmap limit";
            public const string UsageFormat = "Block:usage override[0]/LongEnum:bitmap format";
            public const string DicerFlags = "Block:usage override[0]/WordFlags:dicer flags";
            public const string Slicer = "Block:usage override[0]/CharEnum:slicer";
            public const string BumpHeight = "Real:bump map height";
        }

        public static void ApplyBitmSettings(string[] diffuses, string[] normals, string[] bumps, string[] materials, string ek_path, int compress_value)
        {
            // EK "tags" folder location
            string base_path = Path.Join(ek_path, "tags");

            foreach (string bitmap_full in diffuses)
            {
                // Get correctly formatted path by only taking tags-relative path and removing extension
                string bitmap_path = MBHelpers.GetBitmapRelativePath(base_path, bitmap_full);

                var tag_path = TagPath.FromPathAndType(bitmap_path, "bitm*");

                using (var tagFile = new TagFile(tag_path))
                {
                    // Set curve mode to pretty
                    var curve = (TagFieldEnum)tagFile.SelectField(TagFieldConstants.CurveMode);
                    curve.Value = 2;

                    // Set compression to UI-selected value
                    var compression = (TagFieldEnum)tagFile.SelectField(TagFieldConstants.BitmapFormat);
                    compression.Value = compress_value;

                    // Set max mipmap to -1
                    var mip_limit = (TagFieldElementInteger)tagFile.SelectField(TagFieldConstants.MipMapLevel);
                    mip_limit.Data = -1;

                    // Check if bitmap already has overrides entry, if so remove it
                    int override_count = ((TagFieldBlock)tagFile.SelectField(TagFieldConstants.UsageOverride)).Elements.Count();
                    if (override_count > 0)
                    {
                        ((TagFieldBlock)tagFile.SelectField(TagFieldConstants.UsageOverride)).RemoveAllElements();
                    }

                    // Add override entry
                    ((TagFieldBlock)tagFile.SelectField(TagFieldConstants.UsageOverride)).AddElement();

                    // Set gamma
                    var gamma = (TagFieldElementSingle)tagFile.SelectField(TagFieldConstants.Gamma);
                    gamma.Data = 2.2f;

                    // Set bitmap curve to sRGB
                    var bitmap_curve = (TagFieldEnum)tagFile.SelectField(TagFieldConstants.BitmapCurve);
                    bitmap_curve.Value = 5;

                    // Set ignore curve override flag
                    var flags = (TagFieldFlags)tagFile.SelectField(TagFieldConstants.Flags);
                    flags.RawValue = 1;

                    // Set mipmap limit
                    var mip_limit_override = (TagFieldElementInteger)tagFile.SelectField(TagFieldConstants.MipLimit);
                    mip_limit_override.Data = -1;

                    // Set compression to UI-selected value
                    var override_compression = (TagFieldEnum)tagFile.SelectField(TagFieldConstants.UsageFormat);
                    override_compression.Value = compress_value;

                    tagFile.Save();
                }
            }

            foreach (string bitmap_full in normals)
            {
                // Get correctly formatted path by only taking tags-relative path and removing extension
                string bitmap_path = MBHelpers.GetBitmapRelativePath(base_path, bitmap_full);

                var tag_path = TagPath.FromPathAndType(bitmap_path, "bitm*");

                using (var tagFile = new TagFile(tag_path))
                {
                    // Set usage to zbump
                    var usage = (TagFieldEnum)tagFile.SelectField(TagFieldConstants.Usage);
                    usage.Value = 17;

                    // Set bump height to default of 5
                    var bump_height = (TagFieldElementSingle)tagFile.SelectField(TagFieldConstants.BumpHeight);
                    bump_height.Data = 5;

                    // Set curve mode to pretty
                    var curve = (TagFieldEnum)tagFile.SelectField(TagFieldConstants.CurveMode);
                    curve.Value = 2;

                    // Set compression to DXN
                    var compression = (TagFieldEnum)tagFile.SelectField(TagFieldConstants.BitmapFormat);
                    compression.Value = 49;

                    // Set max mipmap to -1
                    var mip_limit = (TagFieldElementInteger)tagFile.SelectField(TagFieldConstants.MipMapLevel);
                    mip_limit.Data = -1;

                    // Check if bitmap already has overrides entry, if so remove it
                    int override_count = ((TagFieldBlock)tagFile.SelectField(TagFieldConstants.UsageOverride)).Elements.Count();
                    if (override_count > 0)
                    {
                        ((TagFieldBlock)tagFile.SelectField(TagFieldConstants.UsageOverride)).RemoveAllElements();
                    }

                    // Add override entry
                    ((TagFieldBlock)tagFile.SelectField(TagFieldConstants.UsageOverride)).AddElement();

                    // Set gamma
                    var gamma = (TagFieldElementSingle)tagFile.SelectField(TagFieldConstants.Gamma);
                    gamma.Data = 1.0f;

                    // Set bitmap curve to linear
                    var bitmap_curve = (TagFieldEnum)tagFile.SelectField(TagFieldConstants.BitmapCurve);
                    bitmap_curve.Value = 3;

                    // Set ignore curve override flag
                    var flags = (TagFieldFlags)tagFile.SelectField(TagFieldConstants.Flags);
                    flags.RawValue = 1;

                    // Set Unsigned flag
                    var dicer_flags = (TagFieldFlags)tagFile.SelectField(TagFieldConstants.DicerFlags);
                    dicer_flags.RawValue = 16;

                    // Set mipmap limit
                    var mip_limit_override = (TagFieldElementInteger)tagFile.SelectField(TagFieldConstants.MipLimit);
                    mip_limit_override.Data = -1;

                    // Set compression to DXN
                    var override_compression = (TagFieldEnum)tagFile.SelectField(TagFieldConstants.UsageFormat);
                    override_compression.Value = 49;

                    tagFile.Save();
                }
            }

            foreach (string bitmap_full in bumps)
            {
                // Get correctly formatted path by only taking tags-relative path and removing extension
                string bitmap_path = MBHelpers.GetBitmapRelativePath(base_path, bitmap_full);

                var tag_path = TagPath.FromPathAndType(bitmap_path, "bitm*");

                using (var tagFile = new TagFile(tag_path))
                {
                    // Set usage to bump
                    var usage = (TagFieldEnum)tagFile.SelectField(TagFieldConstants.Usage);
                    usage.Value = 2;

                    // Set bump height to default of 5
                    var bump_height = (TagFieldElementSingle)tagFile.SelectField(TagFieldConstants.BumpHeight);
                    bump_height.Data = 5;

                    // Set curve mode to pretty
                    var curve = (TagFieldEnum)tagFile.SelectField(TagFieldConstants.CurveMode);
                    curve.Value = 2;

                    // Set compression to best compressed bump
                    var compression = (TagFieldEnum)tagFile.SelectField(TagFieldConstants.BitmapFormat);
                    compression.Value = 3;

                    // Set max mipmap to -1
                    var mip_limit = (TagFieldElementInteger)tagFile.SelectField(TagFieldConstants.MipMapLevel);
                    mip_limit.Data = -1;

                    // Check if bitmap already has overrides entry, if so remove it
                    int override_count = ((TagFieldBlock)tagFile.SelectField(TagFieldConstants.UsageOverride)).Elements.Count();
                    if (override_count > 0)
                    {
                        ((TagFieldBlock)tagFile.SelectField(TagFieldConstants.UsageOverride)).RemoveAllElements();
                    }

                    tagFile.Save();
                }
            }

            foreach (string bitmap_full in materials)
            {
                // Get correctly formatted path by only taking tags-relative path and removing extension
                string bitmap_path = MBHelpers.GetBitmapRelativePath(base_path, bitmap_full);

                var tag_path = TagPath.FromPathAndType(bitmap_path, "bitm*");

                using (var tagFile = new TagFile(tag_path))
                {
                    // Set curve mode to pretty
                    var curve = (TagFieldEnum)tagFile.SelectField(TagFieldConstants.CurveMode);
                    curve.Value = 2;

                    // Set compression to UI-selected value
                    var compression = (TagFieldEnum)tagFile.SelectField(TagFieldConstants.BitmapFormat);
                    compression.Value = compress_value;

                    // Set max mipmap to -1
                    var mip_limit = (TagFieldElementInteger)tagFile.SelectField(TagFieldConstants.MipMapLevel);
                    mip_limit.Data = -1;

                    // Check if bitmap already has overrides entry, if so remove it
                    int override_count = ((TagFieldBlock)tagFile.SelectField(TagFieldConstants.UsageOverride)).Elements.Count();
                    if (override_count > 0)
                    {
                        ((TagFieldBlock)tagFile.SelectField(TagFieldConstants.UsageOverride)).RemoveAllElements();
                    }

                    // Add override entry
                    ((TagFieldBlock)tagFile.SelectField(TagFieldConstants.UsageOverride)).AddElement();

                    // Set gamma
                    var gamma = (TagFieldElementSingle)tagFile.SelectField(TagFieldConstants.Gamma);
                    gamma.Data = 1.0f;

                    // Set bitmap curve to linear
                    var bitmap_curve = (TagFieldEnum)tagFile.SelectField(TagFieldConstants.BitmapCurve);
                    bitmap_curve.Value = 3;

                    // Set slicer to no slicing
                    var slicer = (TagFieldEnum)tagFile.SelectField(TagFieldConstants.Slicer);
                    slicer.Value = 1;

                    // Set mipmap limit
                    var mip_limit_override = (TagFieldElementInteger)tagFile.SelectField(TagFieldConstants.MipLimit);
                    mip_limit_override.Data = -1;

                    // Set compression to UI-selected value
                    var override_compression = (TagFieldEnum)tagFile.SelectField(TagFieldConstants.UsageFormat);
                    override_compression.Value = compress_value;

                    tagFile.Save();
                }
            }
        }
    }
}
