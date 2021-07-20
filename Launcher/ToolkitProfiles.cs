﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ToolkitLauncher
{
    public class ToolkitProfiles
    {
        private readonly static string appdata_path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private const string save_folder = "Osoyoos";
        private const string settings_file = "Settings.JSON";

        private static List<ProfileSettingsLauncher> _SettingsList = new();
        public static List<ProfileSettingsLauncher> SettingsList => _SettingsList;

        private class BuildTypeJsonConverter : JsonConverter<build_type>
        {
            public override build_type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                build_type parsed;
                if (Enum.TryParse(reader.GetString(), out parsed))
                    return parsed;
                return build_type.release_standalone;
            }

            public override void Write(Utf8JsonWriter writer, build_type value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

#nullable enable
        public class ProfileSettingsLauncher
        {
            [JsonPropertyName("profile_name")]
            public string ProfileName { get; set; } = "unnamed";

            [JsonPropertyName("tool_path")]
            public string ToolPath { get; set; } = "";

            [JsonPropertyName("sapien_path")]
            public string SapienPath { get; set; } = "";

            [JsonPropertyName("guerilla_path")]
            public string GuerillaPath { get; set; } = "";

            [JsonPropertyName("game_gen")]
            public int GameGen { get; set; }

            [JsonPropertyName("build_type")]
            [JsonConverter(typeof(BuildTypeJsonConverter))]
            public build_type BuildType { get; set; }

            [JsonPropertyName("community_tools")]
            public bool CommunityTools { get; set; }

            [JsonPropertyName("data_path")]
            public string DataPath { get; set; } = "";

            [JsonPropertyName("tag_path")]
            public string TagPath { get; set; } = "";

            [JsonPropertyName("verbose")]
            public bool Verbose { get; set; }

            [JsonPropertyName("game_path")]
            public string GamePath { get; set; } = "";

            [JsonPropertyName("game_exe_path")]
            public string GameExePath { get; set; } = "";
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
                    _SettingsList = JsonSerializer.Deserialize<List<ProfileSettingsLauncher>>(jsonString);
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
                ProfileName = String.Format("Profile {0}", count),
                BuildType = build_type.release_standalone
            };
            _SettingsList.Add(profile);
            return count;
        }

        private static void WriteJSONFile()
        {
            JsonSerializerOptions options = new()
            {
                WriteIndented = true
            };
            string json_string = JsonSerializer.Serialize(_SettingsList, options);
            string file_path = Path.Combine(appdata_path + "\\" + save_folder, settings_file);

            Directory.CreateDirectory(Path.Combine(appdata_path, save_folder));

            File.WriteAllText(file_path, json_string);
        }
    }
}