using Bungie.Tags;
using ManagedBlamHelper;
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
        public static void ConfigureCompression(EditingKitInfo editingKit, string tag_folder, string compress_value, bool override_existing)
        {
            // num0005 (2024), I don't think we need to get a list of all the already existing bitmaps if we are just reimporting files

            /*
            // Get all bitmap tags
            string tag_folder_full = Path.Join(editingKit.TagDirectory, tag_folder);
            string[] all_bitmaps = Directory.GetFiles(tag_folder_full, "*.bitmap");
            */

            var bitmaps_to_import = GetBitmapsToImport(editingKit, tag_folder);

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

            void ApplySettingsDiffuse(TagFileBitmap bitmapFile)
            {
                bitmapFile.ResetUsageOverrides();
                bitmapFile.CurveValue = "force PRETTY";
                bitmapFile.BitmapFormatValue = compress_value;
                bitmapFile.MipMapLevel.Data = -1;

                // 2.2 gamma is fairly standard sRGB curve
                bitmapFile.Gamma.Data = 2.2f;
                bitmapFile.BitmapCurveValue = "sRGB";

                // Set ignore curve override flag
                bitmapFile.UsageOverrideFlags.RawValue = 1;

                bitmapFile.MipLimit.Data = -1;
                bitmapFile.UsageFormatValue = compress_value;
            }

            void ApplySettingsNormals(TagFileBitmap bitmapFile)
            {
                bitmapFile.ResetUsageOverrides();

                bitmapFile.UsageValue = "ZBrush Bump Map (from Bump Map)"; // 17

                bitmapFile.BumpHeight.Data = 5; // use a height of 5 as a default

                bitmapFile.CurveValue = "force PRETTY";

                bitmapFile.BitmapFormatValue = "DXN Compressed Normals (better)";
                bitmapFile.UsageFormatValue = "DXN Compressed Normals (better)";

                bitmapFile.MipMapLevel.Data = -1;

                // Setup linear gamma
                bitmapFile.Gamma.Data = 1.0f;
                bitmapFile.BitmapCurveValue = "linear";

                // Set ignore curve override flag
                bitmapFile.UsageOverrideFlags.RawValue = 1;

                // Set Unsigned flag
                bitmapFile.DicerFlags.RawValue = 16;
                bitmapFile.MipLimit.Data = -1;
            }

            void ApplySettingsBumps(TagFileBitmap bitmapFile)
            {
                bitmapFile.ClearUsageOverrides();

                bitmapFile.UsageValue = "Bump Map (from Height Map)"; // 2
                bitmapFile.BumpHeight.Data = 5; // use 5 as the default value
                bitmapFile.CurveValue = "force PRETTY";
                bitmapFile.BitmapFormatValue = "Best Compressed Bump Format";
                bitmapFile.MipMapLevel.Data = -1;
            }

            void ApplySettingsMaterials(TagFileBitmap bitmapFile)
            {
                bitmapFile.ResetUsageOverrides();

                bitmapFile.CurveValue = "force PRETTY";
                bitmapFile.BitmapFormatValue = compress_value;
                bitmapFile.MipMapLevel.Data = -1;
                bitmapFile.Gamma.Data = 1.0f;
                bitmapFile.BitmapCurveValue = "linear";
                bitmapFile.SlicerValue = "No Slicing (each source bitmap generates one element)";
                bitmapFile.MipLimit.Data = -1;
                bitmapFile.UsageFormatValue = compress_value;
            }

            foreach (string bitmap in bitmaps_to_import)
            {
                /*
                 * Figure otu of the tag exists, and if it does, if we should modify it
                 */

                string tag_file_on_disk_path = Path.Combine(editingKit.TagDirectory, bitmap) + ".bitmap";
                TagPath tag_path = TagPath.FromPathAndType(bitmap, "bitm*");
                using TagFile tagFile = new();

                if (File.Exists(tag_file_on_disk_path))
                {
                    if (!override_existing)
                    {
                        Trace.WriteLine($"Skipping {bitmap} as it already exists, and override_existing is false");
                        continue;
                    }

                    tagFile.Load(tag_path);
                }
                else
                {
                    tagFile.New(tag_path);
                }

                using TagFileBitmap tagBitmap = new(tag_path, tagFile);

                /*
                 * Apply custom settings depending on the bitmap usage (based on filename)
                 */

                if (normal_suffixes.Any(suffix => bitmap.EndsWith(suffix)))
                {
                    ApplySettingsNormals(tagBitmap);
                }
                else if (bump_suffixes.Any(suffix => bitmap.EndsWith(suffix)))
                {
                    ApplySettingsBumps(tagBitmap);
                }
                else if (material_suffixes.Any(suffix => bitmap.EndsWith(suffix)))
                {
                    ApplySettingsMaterials(tagBitmap);
                }
                else
                {
                    // default to treating bitmaps as diffuse
                    ApplySettingsDiffuse(tagBitmap);
                }
            }
        }

        public static IEnumerable<string> GetBitmapsToImport(EditingKitInfo editingKit, string files_path)
        {
            // Get all tiffs in data folder
            string[] extensions = new[] { "*.tif", "*.tiff", "*.dds" };
            List<string> all_textures = new();
            string data_folder_full = Path.Join(editingKit.DataDirectory, files_path);

            // exit early if our directories don't exist
            if (!Directory.Exists(data_folder_full))
            {
                return null;
            }

            foreach (string extension in extensions)
            {
                all_textures.AddRange(Directory.GetFiles(data_folder_full, extension));
            }

            return all_textures.Select(path => Path.GetRelativePath(editingKit.DataDirectory, Path.ChangeExtension(path, null)));
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
            public const string MipLimitHR = "Block:usage override[0]/CharInteger:mipmap limit";
            public const string UsageFormat = "Block:usage override[0]/LongEnum:bitmap format";
            public const string DicerFlags = "Block:usage override[0]/WordFlags:dicer flags";
            public const string DicerFlagsHr = "Block:usage override[0]/ByteFlags:dicer flags";
            public const string Slicer = "Block:usage override[0]/CharEnum:slicer";
            public const string BumpHeight = "Real:bump map height";
        }

        internal sealed class TagFileBitmap : IDisposable
        {
            public readonly TagPath tag_path;
            public readonly TagFile tag = null;

            public TagFileBitmap(TagPath path, TagFile tagFile)
            {
                tag_path = path;
                tag = tagFile;
            }

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

            private static string GetEnumValue(TagFieldEnum @enum)
            {
                return @enum.Items[@enum.Value].EnumName;
            }

            private static void SetEnumValue(TagFieldEnum @enum, string value)
            {
                var index = Array.FindIndex(@enum.Items, e => e.EnumName.StartsWith(value));

                if (index == -1)
                {
                    throw new InvalidDataException($"Unknown enum value {value}");
                }

                @enum.Value = index;
            }

            public TagFieldBlock UsageOverridesBlock => (TagFieldBlock)tag.SelectField(TagFieldConstants.UsageOverride);

            public TagFieldEnum Usage => (TagFieldEnum)tag.SelectField(TagFieldConstants.Usage);

            public TagFieldElementSingle BumpHeight => (TagFieldElementSingle)tag.SelectField(TagFieldConstants.BumpHeight);

            public TagFieldEnum Curve => (TagFieldEnum)tag.SelectField(TagFieldConstants.CurveMode);

            public TagFieldEnum BitmapFormat => (TagFieldEnum)tag.SelectField(TagFieldConstants.BitmapFormat);

            public TagFieldElementInteger MipMapLevel => (TagFieldElementInteger)tag.SelectField(TagFieldConstants.MipMapLevel);
            public string UsageValue
            {
                get
                {
                    return GetEnumValue(Usage);
                }

                set
                {
                    SetEnumValue(Usage, value);
                }
            }

            public string CurveValue
            {
                get
                {
                    return GetEnumValue(Curve);
                }

                set
                {
                    SetEnumValue(Curve, value);
                }
            }

            public string BitmapFormatValue
            {
                get
                {
                    return GetEnumValue(BitmapFormat);
                }

                set
                {
                    SetEnumValue(BitmapFormat, value);
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

            public string BitmapCurveValue
            {
                get
                {
                    return GetEnumValue(BitmapCurve);
                }

                set
                {
                    SetEnumValue(BitmapCurve, value);
                }
            }

            public TagFieldElementInteger MipLimit
            {
                get
                {
                    _checkIsUsageValid();
                    return (TagFieldElementInteger)tag.SelectField(ManagedBlamInterface.IsGen4 ? TagFieldConstants.MipLimitHR : TagFieldConstants.MipLimit);
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
                    return (TagFieldFlags)tag.SelectField(ManagedBlamInterface.IsGen4 ? TagFieldConstants.DicerFlagsHr : TagFieldConstants.DicerFlags);
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

            public string UsageFormatValue
            {
                get
                {
                    return GetEnumValue(UsageFormat);
                }

                set
                {
                    SetEnumValue(UsageFormat, value);
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

            public string SlicerValue
            {
                get
                {
                    return GetEnumValue(Slicer);
                }

                set
                {
                    SetEnumValue(Slicer, value);
                }
            }

        }
    }
}
