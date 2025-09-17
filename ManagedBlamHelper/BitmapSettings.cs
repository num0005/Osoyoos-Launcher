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

            var mainSuffixRules = new Dictionary<string, Action<TagFileBitmap>>(StringComparer.OrdinalIgnoreCase)
            {
                // Normal maps
                { "_normal", ApplySettingsNormals },
                { "_normalmap", ApplySettingsNormals },
                { "_nm", ApplySettingsNormals },
                { "_n", ApplySettingsNormals },
                { "_zbump", ApplySettingsNormals },

                // Bump maps
                { "_bump", ApplySettingsBumps },
                { "_bmp", ApplySettingsBumps },
                { "_bp", ApplySettingsBumps },
                { "_b", ApplySettingsBumps },

                // Material maps
                { "_material", ApplySettingsMaterials },
                { "_materialmap", ApplySettingsMaterials },
                { "_mat", ApplySettingsMaterials },
                { "_m", ApplySettingsMaterials },
                { "_orm", ApplySettingsMaterials },
                { "_ormh", ApplySettingsMaterials },
                { "_rmo", ApplySettingsMaterials },
                { "_rmoh", ApplySettingsMaterials },
                { "_mro", ApplySettingsMaterials },
                { "_mroh", ApplySettingsMaterials },

                // Terrain blend maps
                { "_blend", ApplySettingsBlends },
                { "_terrainblend", ApplySettingsBlends },    
            };

            var miscSuffixRules = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                //{ "_3d", "3D Texture" }, - crashes tool, disabling
                { "_cc", "Change Color Map" },
                { "_change", "Change Color Map" },
                { "_changecolor", "Change Color Map" },
                { "_cube", "Cube Map (Reflection Map)" },
                { "_cubemap", "Cube Map (Reflection Map)" },
                { "_detail", "Detail Map" },
                { "_detailbump", "Detail Bump Map (from Height Map - fades out)" },
                { "_dsprite", "Sprite (Double Multiply, Gray Background)" },
                { "_float", "Float Map (WARNING" },
                { "_height", "Height Map (for Parallax)" },
                { "_heightmap", "Height Map (for Parallax)" },
                { "_parallax", "Height Map (for Parallax)" },
                { "_illum", "Self-Illum Map" },
                { "_illumination", "Self-Illum Map" },
                { "_selfillum", "Self-Illum Map" },
                //{ "_lactxl", ApplySettingsMisc }, - unimplemented?
                //{ "_ladxn", ApplySettingsMisc }, - unimplemented?
                { "_msprite", "Sprite (Blend, White Background)" },
                { "_spec", "Specular Map" },
                { "_specular", "Specular Map" },
                { "_sprite", "Sprite (Additive, Black Background)" },
                { "_ui", "Interface Bitmap" },
                { "_vec", "Vector Map" },
                { "_vector", "Vector Map" },
                { "_warp", "Warp Map (EMBM)" },
            };

            void ApplySettingsDiffuse(TagFileBitmap bitmapFile)
            {
                bitmapFile.ResetUsageOverride();
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
                bitmapFile.ResetUsageOverride();

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
                bitmapFile.RemoveUsageOverride();

                bitmapFile.UsageValue = "Bump Map (from Height Map)"; // 2
                bitmapFile.BumpHeight.Data = 5; // use 5 as the default value
                bitmapFile.CurveValue = "force PRETTY";
                bitmapFile.BitmapFormatValue = "Best Compressed Bump Format";
                bitmapFile.MipMapLevel.Data = -1;
            }

            void ApplySettingsMaterials(TagFileBitmap bitmapFile)
            {
                bitmapFile.ResetUsageOverride();

                bitmapFile.CurveValue = "force PRETTY";
                bitmapFile.BitmapFormatValue = compress_value;
                bitmapFile.MipMapLevel.Data = -1;
                bitmapFile.Gamma.Data = 1.0f;
                bitmapFile.BitmapCurveValue = "linear";
                bitmapFile.SlicerValue = "No Slicing (each source bitmap generates one element)";
                bitmapFile.MipLimit.Data = -1;
                bitmapFile.UsageFormatValue = compress_value;
            }

            void ApplySettingsBlends(TagFileBitmap bitmapFile)
            {
                bitmapFile.ResetUsageOverride();
                bitmapFile.UsageValue = "Blend Map (linear for terrains)";
                bitmapFile.CurveValue = "force PRETTY";
                bitmapFile.BitmapFormatValue = compress_value;
                bitmapFile.BitmapCurveValue = "linear";
                bitmapFile.SlicerValue = "No Slicing (each source bitmap generates one element)";
                bitmapFile.MipLimit.Data = -1;
                bitmapFile.MipMapLevel.Data = -1;
                bitmapFile.Gamma.Data = 1.0f;
            }

            void ApplySettingsMisc(TagFileBitmap bitmapFile, string usage)
            {
                bitmapFile.RemoveUsageOverride();
                bitmapFile.UsageValue = usage;
                bitmapFile.CurveValue = "force PRETTY";
                bitmapFile.BitmapFormatValue = compress_value;
            }

            foreach (string bitmap in bitmaps_to_import)
            {
                /*
                 * Figure out of the tag exists, and if it does, if we should modify it
                 */

                TagPath tag_path = TagPath.FromPathAndType(bitmap, "bitm*");
                using TagFile tagFile = new();

                if (File.Exists(tag_path.Filename))
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

                TagFileBitmap tagBitmap = new(tag_path, tagFile);

                /*
                 * Apply custom settings depending on the bitmap usage (based on filename)
                 */

                bool applied = false;
                // Check main suffix dictionary
                foreach (var rule in mainSuffixRules)
                {
                    if (bitmap.EndsWith(rule.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        rule.Value(tagBitmap);
                        applied = true;
                        break;
                    }
                }

                if (!applied)
                {
                    // Check misc suffix dictionary
                    foreach (var rule in miscSuffixRules)
                    {
                        if (bitmap.EndsWith(rule.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            ApplySettingsMisc(tagBitmap, rule.Value);
                            applied = true;
                            break;
                        }
                    }
                }

                // Default to diffuse if no valid suffix
                if (!applied)
                {
                    ApplySettingsDiffuse(tagBitmap);
                }

                tagBitmap.Save();
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

            public void ResetUsageOverride()
            {
                // clear any old data
                UsageOverridesBlock.RemoveAllElements();

                // ensure there is exactly one element
                UsageOverridesBlock.AddElement();
            }

            public void RemoveUsageOverride()
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
