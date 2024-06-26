using System;
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

        public enum GameGen
        {
            Invalid = 0,
            /// <summary>
            /// Halo 1 - h1ce, h1pc and h1mcc
            /// </summary>
            Halo1 = 1,
            /// <summary>
            /// Halo 2 - vista, h2x and h2mcc
            /// </summary>
            Halo2 = 2,
            /// <summary>
            /// Halo 3 and Halo 3 ODST
            /// </summary>
            Halo3 = 3,
            /// <summary>
            // Halo Reach and H4/H2A
            /// </summary>
            Gen4 = 4,
        }


#nullable enable
        public class ProfileSettingsLauncher
        {
            [JsonPropertyName("profile_name")]
            public string ProfileName { get; set; } = "unnamed";

            [JsonPropertyName("tool_path")]
            public string ToolPath { get; set; } = "";

            [JsonPropertyName("tool_fast_path")]
            public string ToolFastPath { get; set; } = "";

            [JsonPropertyName("sapien_path")]
            public string SapienPath { get; set; } = "";

            [JsonPropertyName("guerilla_path")]
            public string GuerillaPath { get; set; } = "";

            [JsonPropertyName("game_path")]
            public string GamePath { get; set; } = "";

            [JsonPropertyName("game_exe_path")]
            public string GameExePath { get; set; } = "";

            [JsonPropertyName("data_path")]
            public string DataPath { get; set; } = "";

            [JsonPropertyName("tag_path")]
            public string TagPath { get; set; } = "";

            [JsonPropertyName("game_gen")]
            [JsonInclude]
            [Obsolete("Game generation is now set through the GameGen enum")]
            public int GameGenLegacy { private get; set; } = -1;

            [JsonPropertyName("generation")]
            public GameGen Generation { get; set; } = GameGen.Invalid;

            [JsonPropertyName("build_type")]
            [JsonConverter(typeof(BuildTypeJsonConverter))]
            [Obsolete("Build type property is obsolute, use IsMcc, IsReach, IsODST and IsHalo4 instead")]
            [JsonInclude]
            public build_type BuildType { private get; set; }

            [JsonPropertyName("alt_build")]
            public bool? IsAlternativeBuild { get; set; }

            [JsonPropertyName("community_tools")]
            public bool CommunityTools { get; set; }

            [JsonPropertyName("verbose")]
            public bool Verbose { get; set; }

            [JsonPropertyName("expert_mode")]
            public bool ActualExpertMode { get; set; }

            [JsonPropertyName("batch")]
            public bool Batch { get; set; }

            [JsonPropertyName("prt_tool_version")]
            public int? LatestPRTToolVersion { get; set; }

            /// <summary>
            /// Whatever we should temporarily be experts
            /// </summary>
            [JsonIgnore]
            public bool ElevatedToExpert;

            [JsonIgnore]
            public bool ExpertMode
            {
                get => ActualExpertMode || ElevatedToExpert;
                set
                {
                    ElevatedToExpert = false;
                    ActualExpertMode = value;
                }
            }

            /// <summary>
            /// Is this an MCC build?
            /// </summary>
            [JsonIgnore]
            public bool IsMCC
            {
                get
                {
                    return IsAlternativeBuild ?? false || Generation > GameGen.Halo2;
                }
            }

            /// <summary>
            /// is it ODST?
            /// </summary>
            [JsonIgnore]
            public bool IsODST
            {
                get
                {
                    return Generation == GameGen.Halo3 && (IsAlternativeBuild ?? false);
                }
            }

            /// <summary>
            /// Is it Halo Reach?
            /// </summary>
            [JsonIgnore]
            public bool IsReach
            {
                get
                {
                    return Generation == GameGen.Gen4 && (IsAlternativeBuild ?? false) == false;
                }
            }

            /// <summary>
            /// Is Halo 4 or H2A?
            /// </summary>
            [JsonIgnore]
            public bool IsH4
            {
                get
                {
                    return Generation == GameGen.Gen4 && (IsAlternativeBuild == true);
                }
            }

            private string GetH2CodezPath()
            {
                return Path.Combine(Path.GetDirectoryName(ToolPath) ?? "", "h2codez.dll");
            }

            /// <summary>
            /// Is H2Codez installed?
            /// </summary>
            /// <returns></returns>
            public bool IsH2Codez()
            {
                return CommunityTools && BuildType == build_type.release_standalone && Generation == GameGen.Halo2 && File.Exists(GetH2CodezPath());
            }

            public void Upgrade()
            {
                // disable obsolute warnings in upgrade code (this should be common sense)
#pragma warning disable 612, 618
                // upgrade from old style generation to new
                if (Generation == GameGen.Invalid)
                {
                    Generation = (GameGen)(GameGenLegacy + 1);
                    if (!Enum.IsDefined(typeof(GameGen), Generation))
                    {
                        Generation = GameGen.Invalid;
                    }
                }

                if (IsAlternativeBuild is null)
                {
                    IsAlternativeBuild = BuildType == build_type.release_mcc;
                }
#pragma warning restore 612, 618
            }

            public void PrepareForSave()
            {
#pragma warning disable 612, 618
                // downgrade generation for backwards compat
                GameGenLegacy = (int)Generation - 1;
                if (IsAlternativeBuild is bool altBuild)
                {
                    BuildType = altBuild ? build_type.release_mcc : build_type.release_standalone;
                }
#pragma warning restore 612, 618
            }
        }
#nullable restore

        private readonly static JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
        };

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
                    _SettingsList = JsonSerializer.Deserialize<List<ProfileSettingsLauncher>>(jsonString, options);
                    _SettingsList.ForEach(x => x.Upgrade());

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
            // update backwards compat settings
            _SettingsList.ForEach(x => x.PrepareForSave());
            WriteJSONFile();
            return true;
        }

        public static void SwitchProfileIndex(int new_index, int current_index)
        {
            var new_profile = SettingsList[new_index];
            var current_profile = SettingsList[current_index];

            SettingsList[new_index] = current_profile;
            SettingsList[current_index] = new_profile;
        }

        public static void SetProfile(int index, int duplicate_index)
        {
            SettingsList[duplicate_index] = SettingsList[index];
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
                IsAlternativeBuild = false,
            };
            _SettingsList.Add(profile);
            return count;
        }

        private static void WriteJSONFile()
        {
            string json_string = JsonSerializer.Serialize(_SettingsList, options);
            string file_path = Path.Combine(appdata_path + "\\" + save_folder, settings_file);

            Directory.CreateDirectory(Path.Combine(appdata_path, save_folder));

            File.WriteAllText(file_path, json_string);
        }
    }
}
