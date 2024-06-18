using System;
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
                if (args[0] == "getbitmapdata")
                {
                    Console.WriteLine(GetBitmapData());
                }
            }
        }

        public static String GetBitmapData()
        {
            string h3ek = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\H3EK";
            ManagedBlamSystem.InitializeProject(InitializationType.TagsOnly, h3ek);

            var tag_path = Bungie.Tags.TagPath.FromPathAndType(@"objects\scenery\minecraft_door\bitmaps\diffuse", "bitm*");

            using (var tagFile = new Bungie.Tags.TagFile(tag_path))
            {
                // Access the curve mode value of a bitmap tag
                var curve = (TagFieldEnum)tagFile.SelectField("CharEnum:curve mode");
                curve.Value = 2;

                // Access the fade factor value of a bitmap tag
                var fade = (TagFieldElementSingle)tagFile.SelectField("RealFraction:fade factor");
                fade.Data = 0.123456f;

                tagFile.Save();

                return($"This bitmap's curve value is {curve.Value} and the fade is {fade.Data.ToString()}.");
            }
        }
    }
}
