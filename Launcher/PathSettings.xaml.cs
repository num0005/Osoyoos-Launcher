using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Palit.TLSHSharp;
using System.Runtime.CompilerServices;

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
                profile_select.Items.Add(settings.ProfileName);
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
            profile_select.Items.Add(ToolkitProfiles.GetProfile(new_profile).ProfileName);
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
                build_type build_type_enum = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].BuildType;

                profile_name.Text = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].ProfileName;
                tool_path.Text = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].ToolPath;
                sapien_path.Text = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].SapienPath;
                guerilla_path.Text = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].GuerillaPath;
                gen_type.SelectedIndex = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].GameGen;
                build_type.SelectedIndex = (int)build_type_enum;
                community_tools.IsChecked = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].CommunityTools;
                data_path.Text = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].DataPath;
                tag_path.Text = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].TagPath;
                verbose.IsChecked = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].Verbose;
                game_path.Text = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].GamePath;
                game_exe_path.Text = ToolkitProfiles.SettingsList[profile_select.SelectedIndex].GameExePath;
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
                ProfileName = profile_name.Text,
                ToolPath = tool_path.Text,
                SapienPath = sapien_path.Text,
                GuerillaPath = guerilla_path.Text,
                GameGen = gen_type.SelectedIndex,
                BuildType = (build_type)build_type.SelectedIndex,
                CommunityTools = (bool)community_tools.IsChecked,
                DataPath = data_path.Text,
                TagPath = tag_path.Text,
                Verbose = (bool)verbose.IsChecked,
                GamePath = game_path.Text,
                GameExePath = game_exe_path.Text,
            };
            Debug.Assert(profile_select.SelectedIndex >= 0 && ToolkitProfiles.SettingsList.Count > profile_select.SelectedIndex);
            ToolkitProfiles.SettingsList[profile_select.SelectedIndex] = settings;
        }

        private void profile_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            profile_description.Text = BuiltinProfiles.Profiles[profile_type.SelectedIndex].Description;
        }

        public void CreateProfileTemplate(string root_directory_path, int profile_template_index)
        {
            if (Directory.Exists(root_directory_path))
            {
                BuiltinProfiles.Profile profile = BuiltinProfiles.Profiles[profile_type.SelectedIndex];

                string[] fileEntries = Directory.GetFiles(root_directory_path);
                string toolPath = null;
                string guerillaPath = null;
                string sapienPath = null;
                string gamePath = null;

                bool AllHashesFound()
                {
                    if (toolPath is null && profile.Tool is not null)
                        return false;
                    if (sapienPath is null && profile.Sapien is not null)
                        return false;
                    if (guerillaPath is null && profile.Guerilla is not null)
                        return false;
                    if (gamePath is null && profile.Standalone is not null)
                        return false;

                    return true;
                }

                // check exact MD5 hashes
                foreach((string name, string hash) file in HashHelpers.GetExecutableMD5Hashes(root_directory_path))
                {
                    Debug.WriteLine($"MD5 \"{file.hash}\" for \"{file.name}\"");

                    void CheckHash(BuiltinProfiles.Profile.Executable executable, ref string foundPath)
                    {
                        if (foundPath is null && executable is not null && executable.MD5 is not null)
                        {
                            if (Array.IndexOf(executable.MD5, file.hash) != -1)
                                foundPath = file.name;
                        }
                    }


                    CheckHash(profile.Tool, ref toolPath);
                    CheckHash(profile.Sapien, ref sapienPath);
                    CheckHash(profile.Guerilla, ref guerillaPath);
                    CheckHash(profile.Standalone, ref gamePath);
                }

                // check fuzzy hashes if needed
                if (!AllHashesFound()) {
                    
                    // should be initialized to 100 as this has a good FP rate of only 6.43%, but using 50 for now as it will detect the same file for multiple tools.
                    int last_standalone_diff = 50;
                    int last_tool_diff = 50;
                    int last_sapien_diff = 50;
                    int last_guerilla_diff = 50;

                    foreach((string name, TlshHash hash) file in HashHelpers.GetExecutableTLSHashes(root_directory_path))
                    {
                        Debug.WriteLine($"TLSH \"{file.hash}\" for \"{file.name}\"");

                        void CheckHash(BuiltinProfiles.Profile.Executable executable, ref string foundPath, ref int lastDiff, [CallerFilePath] string callerFIle = "", [CallerLineNumber] int callerLine = 0)
                        {
                            if (executable is not null && executable.TLSH is not null)
                            {
                                int newDiff = file.hash.TotalDiff(executable.TLSH, true);
                                if (newDiff < lastDiff)
                                {
                                    foundPath = file.name;
                                    lastDiff = newDiff;
                                    Debug.WriteLine($"{callerFIle}:{callerLine} TLSH: {newDiff}");
                                }
                            }
                        }


                        CheckHash(profile.Tool, ref toolPath, ref last_tool_diff);
                        CheckHash(profile.Sapien, ref sapienPath, ref last_sapien_diff);
                        CheckHash(profile.Guerilla, ref guerillaPath, ref last_guerilla_diff);
                        CheckHash(profile.Standalone, ref gamePath, ref last_standalone_diff);
                    }
                }

                profile_name.Text = profile.ToolkitName;
                tool_path.Text = toolPath;
                sapien_path.Text = sapienPath;
                guerilla_path.Text = guerillaPath;
                game_exe_path.Text = gamePath;
                gen_type.SelectedIndex = profile.GameGeneration - 1;
                build_type.SelectedIndex = profile.IsMCCBuild ? 0 : 1;
                community_tools.IsChecked = profile.Community;
                ProfileSave();
            }
        }
    }
}
