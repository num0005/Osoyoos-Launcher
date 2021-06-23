using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ToolkitLauncher
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum build_type
    {
        [Description("MCC")]
        release_mcc,
        [Description("Standalone")]
        release_standalone
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
            asset_dir.IsEnabled = has_profiles;
            build_type_box.IsEnabled = has_profiles;
            gen_type_box.IsEnabled = has_profiles;
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
            System.Windows.Forms.FolderBrowserDialog root_folder = new();
            System.Windows.Forms.DialogResult result = root_folder.ShowDialog();
            root_folder.Description = "Select the root folder";
            root_folder.UseDescriptionForTitle = true;
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                root_directory.Text = root_folder.SelectedPath;
            }
        }

        private void browse_data_dir_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderDlg = new();
            folderDlg.Description = "Select the data folder";
            folderDlg.UseDescriptionForTitle = true;
            System.Windows.Forms.DialogResult result = folderDlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                data_path.Text = folderDlg.SelectedPath;
            }
        }

        private void browse_tag_dir_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderDlg = new();
            folderDlg.Description = "Select the tags folder";
            folderDlg.UseDescriptionForTitle = true;
            System.Windows.Forms.DialogResult result = folderDlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                tag_path.Text = folderDlg.SelectedPath;
            }
        }

        private void browse_game_dir_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog root_folder = new();
            System.Windows.Forms.DialogResult result = root_folder.ShowDialog();
            root_folder.Description = "Select the root folder";
            root_folder.UseDescriptionForTitle = true;
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                game_path.Text = root_folder.SelectedPath;
            }
        }

        private void browse_game_exe_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FilePicker(game_exe_path, null, toolExeOptions, null);
            picker.Prompt();
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
            data_path.Text = "";
            tag_path.Text = "";
            verbose.IsChecked = false;
            game_path.Text = "";
            game_exe_path.Text = "";
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
                data_path.Text = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].data_path;
                tag_path.Text = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].tag_path;
                verbose.IsChecked = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].verbose;
                game_path.Text = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].game_path;
                game_exe_path.Text = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].game_exe_path;
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
            ToolkitProfiles.ProfileSettingsLauncher settings = new()
            {
                profile_name = profile_name.Text,
                tool_path = tool_path.Text,
                sapien_path = sapien_path.Text,
                guerilla_path = guerilla_path.Text,
                game_gen = gen_type.SelectedIndex,
                build_type = (build_type)build_type.SelectedIndex,
                community_tools = (bool)community_tools.IsChecked,
                data_path = data_path.Text,
                tag_path = tag_path.Text,
                verbose = (bool)verbose.IsChecked,
                game_path = game_path.Text,
                game_exe_path = game_exe_path.Text,
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
                case 2:
                    profile_description.Text = "The MCC Halo Combat Evolved Anniversary Toolset.";
                    break;
                case 3:
                    profile_description.Text = "The original HEK release for Halo 2 Vista on PC.";
                    break;
                case 4:
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

                string[] h1tool_343_md5_list = new string[1] { "6B1638B44039189C593FC8E6A813E0FC" };
                string[] h1guerilla_343_md5_list = new string[1] { "CD73707BA9DADA257150E96E318568F3" };
                string[] h1sapien_343_md5_list = new string[1] { "32618717C45928D1D64EE21F91F26336" };
                string[] h1game_343_md5_list = new string[1] { "EAEE4DBF92F5C6947C12F039F7D894E6" };

                string[] h2vtool_md5_list = new string[3] { "DC221CA8C917A1975D6B3DD035D2F862", "3F58C70BBD47C64C8903033A7E3CA1CB", "4EE1F890E3B85163642A4B18DE1EC00D" };
                string[] h2vguerilla_md5_list = new string[3] { "CE3803CC90E260B3DC59854D89B3EA88", "B95E4D600CFF0D3F3E4F790D54FAE23B", "C54FAC6F99D8C37C71C0D8407B9029C9" };
                string[] h2vsapien_md5_list = new string[3] { "D86C488B7C8F64B86F90C732AF01BF50", "FD6B5727EC66124E8F5E9CEDA3880AC8", "B81F92A73496139F6A5BF72FF8221477" };

                string[] h2vtool_h2codez_md5_list = new string[1] { "F81C24DA93CE8D114CAA8BA0A21C7A63" };
                string[] h2vguerilla_h2codez_md5_list = new string[1] { "55B09D5A6C8ECD86988A5C0F4D59D7EA" };
                string[] h2vsapien_h2codez_md5_list = new string[1] { "975C0D0AD45C1687D11D7D3FDFB778B8" };

                string[] hek_tool_md5_array = new string[0];
                string[] hek_guerilla_md5_array = new string[0];
                string[] hek_sapien_md5_array = new string[0];
                string[] game_md5_array = new string[0];

                string profile_name_template = "";
                string hek_tool_path = "";
                string hek_guerilla_path = "";
                string hek_sapien_path = "";
                string game_exe_path = "";
                int gen_type_template = 0;
                int build_type_template = 0;
                bool community_tools_template = false;

                switch (profile_template_index)
                {
                    case 0:
                        profile_name_template = "Gearbox HEK";
                        hek_tool_md5_array = h1tool_gearbox_md5_list;
                        hek_guerilla_md5_array = h1guerilla_gearbox_md5_list;
                        hek_sapien_md5_array = h1sapien_gearbox_md5_list;
                        gen_type_template = 0;
                        build_type_template = (int)Enum.Parse(typeof(build_type), "release_standalone");
                        community_tools_template = false;
                        break;
                    case 1:
                        profile_name_template = "Gearbox HEK OS";
                        hek_tool_md5_array = h1tool_gearbox_os_md5_list;
                        hek_guerilla_md5_array = h1guerilla_gearbox_os_md5_list;
                        hek_sapien_md5_array = h1sapien_gearbox_os_md5_list;
                        gen_type_template = 0;
                        build_type_template = (int)Enum.Parse(typeof(build_type), "release_standalone");
                        community_tools_template = true;
                        break;
                    case 2:
                        profile_name_template = "343 H1A HEK";
                        hek_tool_md5_array = h1tool_343_md5_list;
                        hek_guerilla_md5_array = h1guerilla_343_md5_list;
                        hek_sapien_md5_array = h1sapien_343_md5_list;
                        game_md5_array = h1game_343_md5_list;
                        gen_type_template = 0;
                        build_type_template = (int)Enum.Parse(typeof(build_type), "release_mcc");
                        community_tools_template = false;
                        break;
                    case 3:
                        profile_name_template = "H2VEK";
                        hek_tool_md5_array = h2vtool_md5_list;
                        hek_guerilla_md5_array = h2vguerilla_md5_list;
                        hek_sapien_md5_array = h2vsapien_md5_list;
                        gen_type_template = 1;
                        build_type_template = (int)Enum.Parse(typeof(build_type), "release_standalone");
                        community_tools_template = false;
                        break;
                    case 4:
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
                bool game_hash_matched = false;
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
                        if (!game_hash_matched)
                        {
                            foreach (string game_md5 in game_md5_array)
                            {
                                if (game_md5.Contains(hash))
                                {
                                    game_exe_path = fileName;
                                    game_hash_matched = true;
                                    break;
                                }
                            }
                        }
                    }

                profile_name.Text = profile_name_template;
                tool_path.Text = hek_tool_path;
                sapien_path.Text = hek_sapien_path;
                guerilla_path.Text = hek_guerilla_path;
                game_path.Text = game_exe_path;
                gen_type.SelectedIndex = gen_type_template;
                build_type.SelectedIndex = build_type_template;
                community_tools.IsChecked = community_tools_template;
                ProfileSave();
            }
        }
    }
}
