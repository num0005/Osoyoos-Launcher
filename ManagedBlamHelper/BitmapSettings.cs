using Bungie.Tags;
using OsoyoosMB.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static OsoyoosMB.MBHandler;

namespace OsoyoosMB
{
    internal class BitmapSettings
    {
        public static void ConfigureCompression(EditingKitInfo editingKit, string tag_folder, int compress_value)
        {
            // Makes "empty" bitmap tags
            MBHelpers.CreateDummyBitmaps(editingKit, tag_folder);

            // Get all bitmap tags
            string tag_folder_full = Path.Join(editingKit.TagDirectory, tag_folder);
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

            List<string> diffuse_bitmaps = new();
            List<string> normal_bitmaps = new();
            List<string> bump_bitmaps = new();
            List<string> material_bitmaps = new();

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

            ApplyBitmSettings(editingKit, diffuse_bitmaps.ToArray(), normal_bitmaps.ToArray(), bump_bitmaps.ToArray(), material_bitmaps.ToArray(), compress_value);
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

        internal sealed class TagFileBitmap : IDisposable
        {
            public readonly TagPath tag_path;
            public readonly TagFile tag = null;

            public TagFileBitmap(string path)
            {
                tag_path = TagPath.FromPathAndType(path, "bitm*");
                tag = new TagFile(tag_path);
            }

            public static TagFileBitmap FromFullPath(EditingKitInfo editingKit, string fullPath)
            {
                string tag_style_path = Path.ChangeExtension(Path.GetRelativePath(editingKit.TagDirectory, fullPath), null);
                Debug.Assert(tag_style_path is not null);

                return new TagFileBitmap(tag_style_path);
            }

            public void ResetUsageOverrides()
            {
                // clear any old data
                UsageOverridesBlock.RemoveAllElements();

                // ensure there is exactly one element
                UsageOverridesBlock.AddElement();
            }

            public void ClearUsageOverrides()
            {
                UsageOverridesBlock.RemoveAllElements();
            }

            public void Save()
            {
                tag.Save();
            }

            public void Dispose()
            {
                if (tag is not null)
                    tag.Dispose();
            }

            public TagFieldBlock UsageOverridesBlock
            {
                get
                {
                    return (TagFieldBlock)tag.SelectField(TagFieldConstants.UsageOverride);
                }
            }

            public TagFieldEnum Usage
            {
                get
                {
                    return (TagFieldEnum)tag.SelectField(TagFieldConstants.Usage);
                }
            }

            public TagFieldElementSingle BumpHeight
            {
                get
                {
                    return (TagFieldElementSingle)tag.SelectField(TagFieldConstants.BumpHeight);
                }
            }

            public TagFieldEnum Curve
            {
                get
                {
                    return (TagFieldEnum)tag.SelectField(TagFieldConstants.CurveMode);
                }
            }

            public TagFieldEnum BitmapFormat
            {
                get
                {
                    return (TagFieldEnum)tag.SelectField(TagFieldConstants.BitmapFormat);
                }
            }

            public TagFieldElementInteger MipMapLevel
            {
                get
                {
                    return (TagFieldElementInteger)tag.SelectField(TagFieldConstants.MipMapLevel);
                }
            }

            private void _checkIsUsageValid()
            {
                TagFieldBlock usageOverrideBlock = (TagFieldBlock)tag.SelectField(TagFieldConstants.UsageOverride);
                Debug.Assert(usageOverrideBlock.Elements.Count == 1);
            }

            public TagFieldElementSingle Gamma
            {
                get
                {
                    _checkIsUsageValid();
                    return (TagFieldElementSingle)tag.SelectField(TagFieldConstants.Gamma);
                }
            }

            public TagFieldEnum BitmapCurve
            {
                get
                {
                    _checkIsUsageValid();
                    return (TagFieldEnum)tag.SelectField(TagFieldConstants.BitmapCurve);
                }
            }

            public TagFieldElementInteger MipLimit
            {
                get
                {
                    _checkIsUsageValid();
                    return (TagFieldElementInteger)tag.SelectField(TagFieldConstants.MipLimit);
                }
            }

            public TagFieldFlags UsageOverrideFlags
            {
                get
                {
                    _checkIsUsageValid();
                    return (TagFieldFlags)tag.SelectField(TagFieldConstants.Flags);
                }
            }

            public TagFieldFlags DicerFlags
            {
                get
                {
                    _checkIsUsageValid();
                    return (TagFieldFlags)tag.SelectField(TagFieldConstants.DicerFlags);
                }
            }

            public TagFieldEnum UsageFormat
            {
                get
                {
                    _checkIsUsageValid();
                    return (TagFieldEnum)tag.SelectField(TagFieldConstants.UsageFormat);
                }
            }

            public TagFieldEnum Slicer
            {
                get
                {
                    _checkIsUsageValid();
                    return (TagFieldEnum)tag.SelectField(TagFieldConstants.Slicer);
                }
            }

        }

        public static void ApplyBitmSettings(EditingKitInfo editingKit, string[] diffuses, string[] normals, string[] bumps, string[] materials, int compress_value)
        {
            foreach (string bitmap_full in diffuses)
            {
                using (var bitmapFile = TagFileBitmap.FromFullPath(editingKit, bitmap_full))
                {
                    bitmapFile.ResetUsageOverrides();

                    // Set curve mode to pretty
                    bitmapFile.Curve.Value = 2;

                    // Set compression to UI-selected value
                    bitmapFile.BitmapFormat.Value = compress_value;

                    // Set max mipmap to -1
                    bitmapFile.MipMapLevel.Data = -1;

                    // 2.2 gamma is fairly standard
                    bitmapFile.Gamma.Data = 2.2f;

                    // Set bitmap curve to sRGB
                    bitmapFile.BitmapCurve.Value = 5;

                    // Set ignore curve override flag
                    bitmapFile.UsageOverrideFlags.RawValue = 1;

                    // Set mipmap limit
                    bitmapFile.MipLimit.Data = -1;

                    // Set compression to UI-selected value
                    bitmapFile.UsageFormat.Value = compress_value;

                    bitmapFile.Save();
                }
            }

            foreach (string bitmap_full in normals)
            {
                using (var bitmapFile = TagFileBitmap.FromFullPath(editingKit, bitmap_full))
                {
                    bitmapFile.ResetUsageOverrides();

                    // Set usage to zbump
                    bitmapFile.Usage.Value = 17;

                    // Set bump height to default of 5
                    bitmapFile.BumpHeight.Data = 5;

                    // Set curve mode to pretty
                    bitmapFile.Curve.Value = 2;

                    // Set compression to DXN
                    bitmapFile.BitmapFormat.Value = 49;

                    // Set max mipmap to -1
                    bitmapFile.MipMapLevel.Data = -1;

                    // 1.0 works better for normals or something?
                    bitmapFile.Gamma.Data = 1.0f;

                    // Set bitmap curve to linear
                    bitmapFile.BitmapCurve.Value = 3;

                    // Set ignore curve override flag
                    bitmapFile.UsageOverrideFlags.RawValue = 1;

                    // Set Unsigned flag
                    bitmapFile.DicerFlags.RawValue = 16;

                    // Set mipmap limit
                    bitmapFile.MipLimit.Data = -1;

                    // Set compression to DXN
                    bitmapFile.UsageFormat.Value = 49;

                    bitmapFile.Save();
                }
            }

            foreach (string bitmap_full in bumps)
            {

                using (var bitmapFile = TagFileBitmap.FromFullPath(editingKit, bitmap_full))
                {
                    bitmapFile.ClearUsageOverrides();


                    // Set usage to bump
                    bitmapFile.Usage.Value = 2;

                    // Set bump height to default of 5
                    bitmapFile.BumpHeight.Data = 5;

                    // Set curve mode to pretty
                    bitmapFile.Curve.Value = 2;

                    // Set compression to best compressed bump
                    bitmapFile.BitmapFormat.Value = 3;

                    // Set max mipmap to -1
                    bitmapFile.MipMapLevel.Data = -1;

                    bitmapFile.Save();
                }
            }

            foreach (string bitmap_full in materials)
            {
                using (var bitmapFile = TagFileBitmap.FromFullPath(editingKit, bitmap_full))
                {
                    bitmapFile.ResetUsageOverrides();

                    // Set curve mode to pretty
                    bitmapFile.Curve.Value = 2;

                    // Set compression to UI-selected value
                    bitmapFile.BitmapFormat.Value = compress_value;

                    // Set max mipmap to -1
                    bitmapFile.MipMapLevel.Data = -1;

                    // Set gamma
                    bitmapFile.Gamma.Data = 1.0f;

                    // Set bitmap curve to linear
                    bitmapFile.BitmapCurve.Value = 3;

                    // Set slicer to no slicing
                    bitmapFile.Slicer.Value = 1;

                    // Set mipmap limit
                    bitmapFile.MipLimit.Data = -1;

                    // Set compression to UI-selected value
                    bitmapFile.UsageFormat.Value = compress_value;

                    bitmapFile.Save();
                }
            }
        }
    }
}
