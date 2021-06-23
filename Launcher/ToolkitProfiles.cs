using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ToolkitLauncher
{
    public class ToolkitProfiles
    {
        private readonly static string appdata_path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private const string save_folder = "Osoyoos";
        private const string settings_file = "Settings.JSON";

        private readonly static List<ProfileSettingsLauncher> _SettingsList = new();
        public static List<ProfileSettingsLauncher> SettingsList => _SettingsList;

#nullable enable
        public class ProfileSettingsJSON
        {
            public string profile_name { get; set; } = "unnamed";
            public string tool_path { get; set; } = "";
            public string sapien_path { get; set; } = "";
            public string guerilla_path { get; set; } = "";
            public int game_gen { get; set; }
            public string build_type { get; set; }
            public bool community_tools { get; set; }
            public string data_path { get; set; } = "";
            public string tag_path { get; set; } = "";
            public bool verbose { get; set; }
            public string game_path { get; set; } = "";
            public string game_exe_path { get; set; } = "";
        }

        public class ProfileSettingsLauncher
        {
            public string profile_name { get; set; } = "unnamed";
            public string tool_path { get; set; } = "";
            public string sapien_path { get; set; } = "";
            public string guerilla_path { get; set; } = "";
            public int game_gen { get; set; }
            public build_type build_type { get; set; }
            public bool community_tools { get; set; }
            public string data_path { get; set; } = "";
            public string tag_path { get; set; } = "";
            public bool verbose { get; set; }
            public string game_path { get; set; } = "";
            public string game_exe_path { get; set; } = "";
        }
#nullable restore
        /// <summary>
        /// Loads settings from disk if they exist.
        /// </summary>
        /// <returns>Whatever there was an issue parsing the settings</returns>
        public static bool Load()
        {
            string file_path = Path.Combine(appdata_path + "\\" + save_folder, settings_file);

            if (File.Exists(file_path))
            {
                try
                {
                    string jsonString = File.ReadAllText(file_path);
                    List<ProfileSettingsJSON> JSONList = JsonSerializer.Deserialize<List<ProfileSettingsJSON>>(jsonString);
                    foreach (ProfileSettingsJSON JSON in JSONList)
                    {
                        build_type platform;
                        if (!Enum.TryParse(JSON.build_type, out platform))
                        {
                            platform = build_type.release_standalone; // default to standalone
                        }
                        ProfileSettingsLauncher settings = new()
                        {
                            profile_name = JSON.profile_name,
                            tool_path = JSON.tool_path,
                            sapien_path = JSON.sapien_path,
                            guerilla_path = JSON.guerilla_path,
                            game_gen = JSON.game_gen,
                            build_type = platform,
                            community_tools = JSON.community_tools,
                            data_path = JSON.data_path,
                            tag_path = JSON.tag_path,
                            verbose = JSON.verbose,
                            game_path = JSON.game_path,
                            game_exe_path = JSON.game_exe_path,
                        };
                        _SettingsList.Add(settings);
                    }
                }
                catch (JsonException)
                {
#if !DEBUG
                    // delete the borked settings
                    File.Delete(file_path);
#endif
                    return false;
                }
            }
            return true;
        }

        public static bool Save()
        {
            WriteJSONFile();
            return true;
        }

        public static ProfileSettingsLauncher GetProfile(int index)
        {
            return _SettingsList[index];
        }

        public static int AddProfile()
        {
            int count = _SettingsList.Count;
            var profile = new ProfileSettingsLauncher
            {
                profile_name = String.Format("Profile {0}", count),
                build_type = build_type.release_standalone // default to this for now
            };
            _SettingsList.Add(profile);
            return count;
        }

        private static void WriteJSONFile()
        {
            List<ProfileSettingsJSON> JSONSettingsList = new();
            JsonSerializerOptions options = new()
            {
                WriteIndented = true
            };

            foreach (ProfileSettingsLauncher launcher_settings in _SettingsList)
            {
                ProfileSettingsJSON settings = new()
                {
                    profile_name = launcher_settings.profile_name,
                    tool_path = launcher_settings.tool_path,
                    sapien_path = launcher_settings.sapien_path,
                    guerilla_path = launcher_settings.guerilla_path,
                    game_gen = launcher_settings.game_gen,
                    build_type = launcher_settings.build_type.ToString(),
                    community_tools = launcher_settings.community_tools,
                    data_path = launcher_settings.data_path,
                    tag_path = launcher_settings.tag_path,
                    verbose = launcher_settings.verbose,
                    game_path = launcher_settings.game_path,
                    game_exe_path = launcher_settings.game_exe_path,
                };
                JSONSettingsList.Add(settings);
            }
            string json_string = JsonSerializer.Serialize(JSONSettingsList, options);
            string file_path = Path.Combine(appdata_path + "\\" + save_folder, settings_file);

            Directory.CreateDirectory(Path.Combine(appdata_path, save_folder));

            File.WriteAllText(file_path, json_string);
        }
    }
}
