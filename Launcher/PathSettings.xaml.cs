using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ToolkitLauncher
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum build_type
    {
        [Description("MCC")]
        release_mcc,
        [Description("Standalone")]
        release_standalone,
        [Description("Internal")]
        release_internal,
    }

    /// <summary>
    /// Interaction logic for PathSettings.xaml
    /// </summary>
    public partial class PathSettings : Window
    {
        private bool IsFirstInit;
        private bool startup_finished = false;
        private bool setting_profile = false;

        public PathSettings(bool isFirstInit = false)
        {
            InitializeComponent();
            IsFirstInit = isFirstInit;
            first_launch.Visibility = IsFirstInit ? Visibility.Visible : Visibility.Collapsed;
            if (ToolkitProfiles.SettingsList.Count == 0)
                ToolkitProfiles.AddProfile();
            foreach (var settings in ToolkitProfiles.SettingsList)
            {
                profile_select.Items.Add(settings.profile_name);
            }
            UpdateUI();
            startup_finished = true;
        }

        private void UpdateUI()
        {
            bool has_profiles = profile_select.Items.Count != 0;

            profile_wizard.IsEnabled = has_profiles;
            profile_name_box.IsEnabled = has_profiles;
            hek_box.IsEnabled = has_profiles;
            build_type_box.IsEnabled = has_profiles;
            gen_type_box.IsEnabled = has_profiles;
            community_tools.IsEnabled = has_profiles;
        }

        readonly FilePicker.Options toolExeOptions = FilePicker.Options.FileSelect(
            "Select app",
            "Toolkit Files|*.exe",
            FilePicker.Options.PathRoot.FileSystem,
            parent: false,
            strip_extension: false
        );

        private void browse_tool_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FilePicker(tool_path, null, toolExeOptions, null);
            picker.Prompt();
        }

        private void browse_sapien_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FilePicker(sapien_path, null, toolExeOptions, null);
            picker.Prompt();
        }

        private void browse_guerilla_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FilePicker(guerilla_path, null, toolExeOptions, null);
            picker.Prompt();
        }

        private void save_button_Click(object sender, RoutedEventArgs e)
        {
            ToolkitProfiles.Save();
            this.Close();
        }

        private void cancel_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void add_button_Click(object sender, RoutedEventArgs e)
        {
            int new_profile = ToolkitProfiles.AddProfile();
            profile_select.Items.Add(ToolkitProfiles.GetProfile(new_profile).profile_name);
            profile_select.SelectedIndex = new_profile;
            setting_profile = false;
            UpdateUI();
        }

        private void delete_button_Click(object sender, RoutedEventArgs e)
        {
            if (profile_select.Items.Count > 0)
            {
                object profile_item = profile_select.SelectedItem;

                int new_index = profile_select.SelectedIndex - 1;
                ToolkitProfiles.SettingsList.RemoveAt(profile_select.SelectedIndex);
                profile_select.Items.Remove(profile_item);
                profile_select.SelectedIndex = 0;
                if (new_index > 0)
                {
                    profile_select.SelectedIndex = new_index;
                }
            }
            UpdateUI();
        }

        private void profile_wizard_Click(object sender, RoutedEventArgs e)
        {
            profile_wizard_menu.Visibility = Visibility.Visible;
        }

        private void profile_wizard_cancel_Click(object sender, RoutedEventArgs e)
        {
            profile_wizard_menu.Visibility = Visibility.Collapsed;
        }

        private void profile_wizard_ok_Click(object sender, RoutedEventArgs e)
        {
            string root_directory_path = root_directory.Text;
            int profile_template_index = profile_type.SelectedIndex;
            profile_wizard_menu.Visibility = Visibility.Collapsed;
            CreateProfileTemplate(root_directory_path, profile_template_index);
        }

        private void browse_root_directory_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog root_folder = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = root_folder.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                root_directory.Text = root_folder.SelectedPath;
            }
        }

        private void profile_selection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            setting_profile = true;
            profile_name.Text = "";
            tool_path.Text = "";
            sapien_path.Text = "";
            guerilla_path.Text = "";
            gen_type.SelectedIndex = 0;
            build_type.SelectedIndex = 0;
            community_tools.IsChecked = false;
            if (profile_select != null && profile_select.SelectedItem != null && ToolkitProfiles.SettingsList.Count > profile_select.SelectedIndex && profile_select.SelectedIndex >= 0)
            {
                build_type build_type_enum = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].build_type;

                profile_name.Text = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].profile_name;
                tool_path.Text = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].tool_path;
                sapien_path.Text = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].sapien_path;
                guerilla_path.Text = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].guerilla_path;
                gen_type.SelectedIndex = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].game_gen;
                build_type.SelectedIndex = (int)build_type_enum;
                community_tools.IsChecked = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].community_tools;
            }
            setting_profile = false;
        }

        private void profile_dataChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            if (startup_finished && !setting_profile)
            {
                ProfileSave();
            }
        }

        private void profile_data_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (startup_finished && !setting_profile)
            {
                ProfileSave();
            }
        }

        private void profile_data_Click(object sender, RoutedEventArgs e)
        {
            if (startup_finished && !setting_profile)
            {
                ProfileSave();
            }
        }

        void ProfileSave()
        {
            ToolkitProfiles.ProfileSettingsLauncher settings = new ToolkitProfiles.ProfileSettingsLauncher
            {
                profile_name = profile_name.Text,
                tool_path = tool_path.Text,
                sapien_path = sapien_path.Text,
                guerilla_path = guerilla_path.Text,
                game_gen = gen_type.SelectedIndex,
                build_type = (build_type)build_type.SelectedIndex,
                community_tools = (bool)community_tools.IsChecked,
            };
            Debug.Assert(profile_select.SelectedIndex >= 0 && ToolkitProfiles.SettingsList.Count > profile_select.SelectedIndex);
            ToolkitProfiles.SettingsList[profile_select.SelectedIndex] = settings;
        }

        private void profile_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selected_profile_index = profile_type.SelectedIndex;
            switch (selected_profile_index)
            {
                case 0:
                    profile_description.Text = "The original HEK release for Halo Custom Edition on PC.";
                    break;
                case 1:
                    profile_description.Text = "The original HEK release for Halo Custom Edition on PC with Open Sauce extensions.";
                    break;
                case 4:
                    profile_description.Text = "The original HEK release for Halo 2 Vista on PC.";
                    break;
                case 5:
                    profile_description.Text = "The original HEK release for Halo 2 Vista on PC with H2Codez extensions.";
                    break;
                default:
                    throw new Exception();
            }
        }

        public void CreateProfileTemplate(string root_directory_path, int profile_template_index)
        {
            if (Directory.Exists(root_directory_path))
            {
                string[] h1tool_gearbox_md5_list = new string[1] { "1F18ECB6F0ACDCD0B0455AA4F7E06B73" };
                string[] h1guerilla_gearbox_md5_list = new string[1] { "FD86057ECDC707D9659BF683DB2FC8DF" };
                string[] h1sapien_gearbox_md5_list = new string[1] { "2A8529486E223DF039AE7464D94C39AC" };

                string[] h1tool_gearbox_os_md5_list = new string[1] { "F7474F0FBAFFB217BDD9B4790D31C255" };
                string[] h1guerilla_gearbox_os_md5_list = new string[1] { "350900E1163FAFDF70428850AA7478E5" };
                string[] h1sapien_gearbox_os_md5_list = new string[1] { "969F8F4D143FEA89488044802F156EF1" };

                string[] h2vtool_md5_list = new string[3] { "DC221CA8C917A1975D6B3DD035D2F862", "3F58C70BBD47C64C8903033A7E3CA1CB", "4EE1F890E3B85163642A4B18DE1EC00D" };
                string[] h2vguerilla_md5_list = new string[3] { "CE3803CC90E260B3DC59854D89B3EA88", "B95E4D600CFF0D3F3E4F790D54FAE23B", "C54FAC6F99D8C37C71C0D8407B9029C9" };
                string[] h2vsapien_md5_list = new string[3] { "D86C488B7C8F64B86F90C732AF01BF50", "FD6B5727EC66124E8F5E9CEDA3880AC8", "B81F92A73496139F6A5BF72FF8221477" };

                string[] h2vtool_h2codez_md5_list = new string[1] { "F81C24DA93CE8D114CAA8BA0A21C7A63" };
                string[] h2vguerilla_h2codez_md5_list = new string[1] { "55B09D5A6C8ECD86988A5C0F4D59D7EA" };
                string[] h2vsapien_h2codez_md5_list = new string[1] { "975C0D0AD45C1687D11D7D3FDFB778B8" };

                string[] hek_tool_md5_array = new string[0];
                string[] hek_guerilla_md5_array = new string[0];
                string[] hek_sapien_md5_array = new string[0];

                string profile_name_template = "";
                string hek_tool_path = "";
                string hek_guerilla_path = "";
                string hek_sapien_path = "";
                int gen_type_template = 0;
                int build_type_template = 0;
                bool community_tools_template = false;

                switch (profile_template_index)
                {
                    case 0:
                        profile_name_template = "Gearbox H1EK";
                        hek_tool_md5_array = h1tool_gearbox_md5_list;
                        hek_guerilla_md5_array = h1guerilla_gearbox_md5_list;
                        hek_sapien_md5_array = h1sapien_gearbox_md5_list;
                        gen_type_template = 0;
                        build_type_template = (int)Enum.Parse(typeof(build_type), "release_standalone");
                        community_tools_template = false;
                        break;
                    case 1:
                        profile_name_template = "Gearbox H1EK OS";
                        hek_tool_md5_array = h1tool_gearbox_os_md5_list;
                        hek_guerilla_md5_array = h1guerilla_gearbox_os_md5_list;
                        hek_sapien_md5_array = h1sapien_gearbox_os_md5_list;
                        gen_type_template = 0;
                        build_type_template = (int)Enum.Parse(typeof(build_type), "release_standalone");
                        community_tools_template = true;
                        break;
                    case 4:
                        profile_name_template = "H2VEK";
                        hek_tool_md5_array = h2vtool_md5_list;
                        hek_guerilla_md5_array = h2vguerilla_md5_list;
                        hek_sapien_md5_array = h2vsapien_md5_list;
                        gen_type_template = 1;
                        build_type_template = (int)Enum.Parse(typeof(build_type), "release_standalone");
                        community_tools_template = false;
                        break;
                    case 5:
                        profile_name_template = "H2VEK H2Codez";
                        hek_tool_md5_array = h2vtool_h2codez_md5_list;
                        hek_guerilla_md5_array = h2vguerilla_h2codez_md5_list;
                        hek_sapien_md5_array = h2vsapien_h2codez_md5_list;
                        gen_type_template = 1;
                        build_type_template = (int)Enum.Parse(typeof(build_type), "release_standalone");
                        community_tools_template = true;
                        break;
                }

                string[] fileEntries = Directory.GetFiles(root_directory_path);
                bool tool_hash_matched = false;
                bool guerilla_hash_matched = false;
                bool sapien_hash_matched = false;
                foreach (string fileName in fileEntries)
                    if (Path.GetExtension(fileName) == ".exe")
                    {
                        var md5 = System.Security.Cryptography.MD5.Create();
                        var stream = File.OpenRead(fileName);
                        string hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "");

                        if (!tool_hash_matched)
                        {
                            foreach (string hek_tool_md5 in hek_tool_md5_array)
                            {
                                if (hek_tool_md5.Contains(hash))
                                {
                                    hek_tool_path = fileName;
                                    tool_hash_matched = true;
                                    break;
                                }
                            }
                        }
                        if (!guerilla_hash_matched)
                        {
                            foreach (string hek_guerilla_md5 in hek_guerilla_md5_array)
                            {

                                if (hek_guerilla_md5.Contains(hash))
                                {
                                    hek_guerilla_path = fileName;
                                    guerilla_hash_matched = true;
                                    break;
                                }
                            }
                        }
                        if (!sapien_hash_matched)
                        {
                            foreach (string hek_sapien_md5 in hek_sapien_md5_array)
                            {
                                if (hek_sapien_md5.Contains(hash))
                                {
                                    hek_sapien_path = fileName;
                                    sapien_hash_matched = true;
                                    break;
                                }
                            }
                        }
                    }

                profile_name.Text = profile_name_template;
                tool_path.Text = hek_tool_path;
                sapien_path.Text = hek_sapien_path;
                guerilla_path.Text = hek_guerilla_path;
                gen_type.SelectedIndex = gen_type_template;
                build_type.SelectedIndex = build_type_template;
                community_tools.IsChecked = community_tools_template;
                ProfileSave();
            }
        }
    }

    public class ToolkitProfiles
    {
        private static string appdata_path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private const string save_folder = "Osoyoos";
        private const string settings_file = "Settings.JSON";

        private static List<ProfileSettingsLauncher> _SettingsList = new List<ProfileSettingsLauncher>();
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
                        build_type platform = (build_type)0;
                        if (Enum.TryParse<build_type>(JSON.build_type, out platform))
                        {
                            platform = (build_type)Enum.Parse(typeof(build_type), JSON.build_type);
                        }
                        ToolkitProfiles.ProfileSettingsLauncher settings = new ToolkitProfiles.ProfileSettingsLauncher
                        {
                            profile_name = JSON.profile_name,
                            tool_path = JSON.tool_path,
                            sapien_path = JSON.sapien_path,
                            guerilla_path = JSON.guerilla_path,
                            game_gen = JSON.game_gen,
                            build_type = platform,
                            community_tools = JSON.community_tools,
                        };
                        _SettingsList.Add(settings);
                    }
                }
                catch (System.Text.Json.JsonException)
                {
                    // delete the borked settings
                    File.Delete(file_path);
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
            var profile = new ToolkitProfiles.ProfileSettingsLauncher();
            profile.profile_name = String.Format("Profile {0}", count);
            profile.build_type = build_type.release_standalone; // default to this for now
            _SettingsList.Add(profile);
            return count;
        }

        private static void WriteJSONFile()
        {   
            List<ProfileSettingsJSON> JSONSettingsList = new List<ProfileSettingsJSON>();
            JsonSerializerOptions options = new()
            {
                WriteIndented = true
            };

            foreach (ProfileSettingsLauncher launcher_settings in _SettingsList)
            {
                ToolkitProfiles.ProfileSettingsJSON settings = new ToolkitProfiles.ProfileSettingsJSON
                {
                    profile_name = launcher_settings.profile_name,
                    tool_path = launcher_settings.tool_path,
                    sapien_path = launcher_settings.sapien_path,
                    guerilla_path = launcher_settings.guerilla_path,
                    game_gen = launcher_settings.game_gen,
                    build_type = launcher_settings.build_type.ToString(),
                    community_tools = launcher_settings.community_tools,
                };
                JSONSettingsList.Add(settings);
            }
            string json_string = JsonSerializer.Serialize(JSONSettingsList, options);
            string file_path = Path.Combine(appdata_path + "\\" + save_folder, settings_file);

            Directory.CreateDirectory(Path.Combine(appdata_path, save_folder));

            File.WriteAllText(file_path, json_string);
        }
    }

    public class BuildTypeToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var vis = Visibility.Collapsed;
            string build_type = "release_mcc";
            if (ToolkitProfiles.SettingsList != null && (int)value >= 0) //Not sure what to do here. Crashes designer otherwise cause the list or value is empty
            {
                build_type build_type_item = (build_type)value;
                build_type = Enum.GetName(typeof(build_type), build_type_item);
            }
            else
            {
                vis = Visibility.Visible;
            }
            if (build_type == "release_mcc")
                vis = Visibility.Visible;
            return vis;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
