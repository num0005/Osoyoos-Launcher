using Bungie.Tags;
using Bungie;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace OsoyoosMB.Utils
{
    internal class MBHelpers
    {
        public static string GetBitmapRelativePath(string base_path, string full_path)
        {
            return Path.ChangeExtension(PathNetCore.GetRelativePath(base_path, full_path), null);
        }

        public static void CreateDummyBitmaps(string ek_path, string files_path)
        {
            // Get all tiffs in data folder
            string[] extensions = new[] { "*.tif", "*.tiff", "*.dds" };
            List<string> all_textures = new List<string>();
            string data_folder_full = Path.Combine(ek_path, "data", files_path);
            string base_path = Path.Combine(ek_path, "data");

            foreach (string extension in extensions)
            {
                all_textures.AddRange(Directory.GetFiles(data_folder_full, extension));
            }

            foreach (string full_texture_path in all_textures)
            {
                // Only create bitmap tag if it doesn't already exist
                if (!File.Exists(Path.ChangeExtension(full_texture_path.Replace("H3EK\\data", "H3EK\\tags"), ".bitmap")))
                {
                    string relative_texture_path = GetBitmapRelativePath(base_path, full_texture_path);
                    TagPath tag_path = TagPath.FromPathAndType(relative_texture_path, "bitm*");
                    TagFile tagFile = new TagFile();
                    tagFile.New(tag_path);
                    tagFile.Save();

                    Debug.WriteLine("Created bitmap " + relative_texture_path);
                }
                Debug.WriteLine("Bitmap for texture: " + full_texture_path + " already exists");
            }
        }
    }
}
