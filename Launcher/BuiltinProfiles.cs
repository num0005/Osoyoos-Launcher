using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Reflection;

namespace ToolkitLauncher
{
    public static class BuiltinProfiles
    {
        public class Profile
        {
            public class Executable
            {
                public string TLSH { get; set; }
                public string[] MD5 { get; set; }
            }

            public string ToolkitName { get; set; }
            public string Description { get; set; }

            public int GameGeneration { get; set; }
            public bool IsMCCBuild { get; set; }

            public bool Community { get; set; }

            public Executable Tool { get; set; }
            public Executable Guerilla { get; set; }
            public Executable Sapien { get; set; }
            public Executable Standalone { get; set; }
        }

        static BuiltinProfiles()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using Stream stream = assembly.GetManifestResourceStream("ToolkitLauncher.BultinProfiles.json");
            using StreamReader reader = new(stream);
            _profiles = JsonSerializer.Deserialize<List<Profile>>(reader.ReadToEnd());
        }

        public static IReadOnlyList<Profile> Profiles
        {
            get
            {
                return _profiles;
            }
        }

        private static readonly List<Profile> _profiles;
    }
}
