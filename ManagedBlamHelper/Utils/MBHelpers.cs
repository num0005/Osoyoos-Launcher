using Bungie.Tags;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace OsoyoosMB.Utils
{
    internal class MBHelpers
    {
        public static string GetBitmapRelativePath(string base_path, string full_path)
        {
            return Path.ChangeExtension(Path.GetRelativePath(base_path, full_path), null);
        }

        public static void CreateDummyBitmaps(string ek_path, string files_path, string ek_tags_folder_path)
        {
            // Get all tiffs in data folder
            string[] extensions = new[] { "*.tif", "*.tiff", "*.dds" };
            List<string> all_textures = new List<string>();
            string data_folder_full = Path.Join(ek_path, "data", files_path);
            string base_path = Path.Join(ek_path, "data");

            // exit early if our directories don't exist
            if (!Directory.Exists(data_folder_full) || !Directory.Exists(base_path))
            {
                return;
            }

            foreach (string extension in extensions)
            {
                all_textures.AddRange(Directory.GetFiles(data_folder_full, extension));
            }

            foreach (string full_texture_path in all_textures)
            {
                string relative_texture_path = GetBitmapRelativePath(base_path, full_texture_path);

                // Only create bitmap tag if it doesn't already exist
                if (!File.Exists(Path.ChangeExtension(Path.Join(ek_tags_folder_path, relative_texture_path), ".bitmap")))
                {
                    TagPath tag_path = TagPath.FromPathAndType(relative_texture_path, "bitm*");
                    TagFile tagFile = new TagFile();
                    tagFile.New(tag_path);
                    tagFile.Save();

                    Debug.WriteLine("Created bitmap " + relative_texture_path);
                }
                else 
                {
                    Debug.WriteLine("Bitmap for texture: " + full_texture_path + " already exists");
                }
            }
        }
    }
}
