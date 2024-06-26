using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Palit.TLSHSharp;
using System.Runtime.CompilerServices;
using ToolkitLauncher.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace ToolkitLauncher
{
    public enum build_type
    {
        release_standalone,
        release_mcc
    }

    /// <summary>
    /// Interaction logic for PathSettings.xaml
    /// </summary>
    public partial class PathSettings : Window
    {
        private bool IsFirstInit;
        private bool startup_finished = false;
        private bool setting_profile = false;

        public int profile_index
        {
            get
            {
                Debug.Assert(profile_select.SelectedIndex >= 0);
                int profile_index = profile_select.SelectedIndex;

                return profile_index;
            }
        }

        public ObservableCollection<ComboBoxItem> BuiltinProfiesCombo { get; init; } = new();

        public PathSettings(bool isFirstInit = false)
        {
            foreach (BuiltinProfiles.Profile profile in  BuiltinProfiles.Profiles)
            {
                BuiltinProfiesCombo.Add(new ComboBoxItem { Content = profile.ToolkitName });
            }
            InitializeComponent();
            IsFirstInit = isFirstInit;
            first_launch.Visibility = IsFirstInit ? Visibility.Visible : Visibility.Collapsed;
            if (ToolkitProfiles.SettingsList.Count == 0)
                ToolkitProfiles.AddProfile();
            CreateItems();
            UpdateUI();
            startup_finished = true;
        }

        private void CreateItems()
        {
            profile_select.Items.Clear();
            foreach (var settings in ToolkitProfiles.SettingsList)
            {
                profile_select.Items.Add(new ComboBoxItem { Content = settings.ProfileName });
            }
            profile_select.SelectedIndex = profile_index;
        }

        private void UpdateUI()
        {
            bool has_profiles = profile_select.Items.Count > 0;

            delete_button.IsEnabled = has_profiles;
            duplicate.IsEnabled = has_profiles;
            shift_up.IsEnabled = has_profiles;
            shift_down.IsEnabled = has_profiles;
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

        private void browse_tool_fast_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FilePicker(tool_fast_path, null, toolExeOptions, null);
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
            string ProfileName = ToolkitProfiles.GetProfile(new_profile).ProfileName;
            profile_select.Items.Add(new ComboBoxItem { Content = ProfileName });
            profile_select.SelectedIndex = new_profile;
            setting_profile = false;
            UpdateUI();
        }

        private void delete_button_Click(object sender, RoutedEventArgs e)
        {
            if (profile_select.Items.Count > 0)
            {
                int new_index = profile_index - 1;
                ToolkitProfiles.SettingsList.RemoveAt(profile_index);
                profile_select.Items.Remove(profile_select.SelectedItem);
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

        private void shift_item(int direction)
        {
            int new_index = profile_index + direction;
            if (new_index >= 0 && new_index < profile_select.Items.Count)
            {
                int current_index = profile_index;

                ToolkitProfiles.SwitchProfileIndex(new_index, current_index);

                ComboBoxItem current_profile_name = (ComboBoxItem)profile_select.SelectedItem;
                profile_select.Items.Remove(profile_select.SelectedItem);
                profile_select.Items.Insert(new_index, current_profile_name);
                profile_select.SelectedIndex = new_index;
            }
        }

        private void shift_up_Click(object sender, RoutedEventArgs e)
        {
            shift_item(-1);
        }

        private void shift_down_Click(object sender, RoutedEventArgs e)
        {
            shift_item(1);
        }

        private void duplicate_Click(object sender, RoutedEventArgs e)
        {
            int new_profile = ToolkitProfiles.AddProfile();
            string ProfileName = ToolkitProfiles.GetProfile(new_profile).ProfileName;
            profile_select.Items.Add(new ComboBoxItem { Content = ProfileName });
            ToolkitProfiles.SetProfile(profile_index, new_profile);
            profile_select.SelectedIndex = new_profile;
            var profile_item = profile_select.Items[profile_index] as ComboBoxItem;
            profile_item.Content = profile_name.Text;
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
            tool_fast_path.Text = "";
            sapien_path.Text = "";
            guerilla_path.Text = "";
            game_path.Text = "";
            game_exe_path.Text = "";
            data_path.Text = "";
            tag_path.Text = "";
            gen_type.SelectedIndex = 0;
            community_tools.IsChecked = false;
            is_mcc.IsChecked = false;
            verbose.IsChecked = false;
            expert_mode.IsChecked = false;
            batch.IsChecked = false;
            h2codez_update_groupbox.Visibility = Visibility.Collapsed;
            if (profile_select != null && profile_select.SelectedItem != null && ToolkitProfiles.SettingsList.Count > profile_index && profile_index >= 0)
            {
                profile_name.Text = ToolkitProfiles.SettingsList[profile_index].ProfileName;
                tool_path.Text = ToolkitProfiles.SettingsList[profile_index].ToolPath;
                tool_fast_path.Text = ToolkitProfiles.SettingsList[profile_index].ToolFastPath;
                sapien_path.Text = ToolkitProfiles.SettingsList[profile_index].SapienPath;
                guerilla_path.Text = ToolkitProfiles.SettingsList[profile_index].GuerillaPath;
                game_path.Text = ToolkitProfiles.SettingsList[profile_index].GamePath;
                game_exe_path.Text = ToolkitProfiles.SettingsList[profile_index].GameExePath;
                data_path.Text = ToolkitProfiles.SettingsList[profile_index].DataPath;
                tag_path.Text = ToolkitProfiles.SettingsList[profile_index].TagPath;
                gen_type.SelectedIndex = (int)ToolkitProfiles.SettingsList[profile_index].Generation - 1;
                is_mcc.IsChecked = ToolkitProfiles.SettingsList[profile_index].IsAlternativeBuild;
                community_tools.IsChecked = ToolkitProfiles.SettingsList[profile_index].CommunityTools;
                verbose.IsChecked = ToolkitProfiles.SettingsList[profile_index].Verbose;
                expert_mode.IsChecked = ToolkitProfiles.SettingsList[profile_index].ExpertMode;
                batch.IsChecked = ToolkitProfiles.SettingsList[profile_index].Batch;

                h2codez_update_groupbox.Visibility = ToolkitProfiles.SettingsList[profile_index].IsH2Codez() ?
                    Visibility.Visible : Visibility.Collapsed;
            }
            setting_profile = false;
        }

        private void profile_dataChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            if (startup_finished && !setting_profile)
            {
                var profile_item = profile_select.Items[profile_index] as ComboBoxItem;
                profile_item.Content = profile_name.Text;
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
                ToolFastPath = tool_fast_path.Text,
                SapienPath = sapien_path.Text,
                GuerillaPath = guerilla_path.Text,
                GamePath = game_path.Text,
                GameExePath = game_exe_path.Text,
                DataPath = data_path.Text,
                TagPath = tag_path.Text,
                Generation = (ToolkitProfiles.GameGen)(gen_type.SelectedIndex + 1),
                IsAlternativeBuild = (bool)is_mcc.IsChecked,
                CommunityTools = (bool)community_tools.IsChecked,
                Verbose = (bool)verbose.IsChecked,
                ExpertMode = (bool)expert_mode.IsChecked,
                Batch = (bool)batch.IsChecked,
            };

            // get new and old base directory
            string? old_base_directory = Path.GetDirectoryName(ToolkitProfiles.SettingsList[profile_index].ToolPath);
            string? new_base_directory = Path.GetDirectoryName(tool_path.Text);

            int? prt_version_info = ToolkitProfiles.SettingsList[profile_index].LatestPRTToolVersion;
            

            Debug.Assert(profile_index >= 0 && ToolkitProfiles.SettingsList.Count > profile_index);
            ToolkitProfiles.SettingsList[profile_index] = settings;

            // if the base path was not changed, the prt install information can be carried over
            if (old_base_directory is not null && old_base_directory == new_base_directory)
            {
                ToolkitProfiles.SettingsList[profile_index].LatestPRTToolVersion = prt_version_info;
            }

            h2codez_update_groupbox.Visibility = settings.IsH2Codez() ? Visibility.Visible : Visibility.Collapsed;
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
                string toolFastPath = null;

                bool AllHashesFound()
                {
                    if (toolPath is null && profile.Tool is not null)
                        return false;
                    if (toolFastPath is null && profile.ToolFast is not null)
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
                foreach ((string name, string hash) file in HashHelpers.GetExecutableMD5Hashes(root_directory_path))
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
                    CheckHash(profile.ToolFast, ref toolFastPath);
                    CheckHash(profile.Sapien, ref sapienPath);
                    CheckHash(profile.Guerilla, ref guerillaPath);
                    CheckHash(profile.Standalone, ref gamePath);
                }

                // check fuzzy hashes if needed
                if (!AllHashesFound())
                {

                    // should be initialized to 100 as this has a good FP rate of only 6.43%
                    // todo: sometimes the same file will be matched for multiple tools, figure out how to handle this

                    int last_standalone_diff = 100;
                    int last_tool_diff = 100;
                    int last_tool_fast_diff = 100;
                    int last_sapien_diff = 100;
                    int last_guerilla_diff = 100;

                    foreach ((string name, TlshHash hash) file in HashHelpers.GetExecutableTLSHashes(root_directory_path))
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
                        CheckHash(profile.ToolFast, ref toolFastPath, ref last_tool_fast_diff);
                        CheckHash(profile.Sapien, ref sapienPath, ref last_sapien_diff);
                        CheckHash(profile.Guerilla, ref guerillaPath, ref last_guerilla_diff);
                        CheckHash(profile.Standalone, ref gamePath, ref last_standalone_diff);
                    }
                }

                profile_name.Text = profile.ToolkitName;
                tool_path.Text = toolPath;
                tool_fast_path.Text = toolFastPath;
                sapien_path.Text = sapienPath;
                guerilla_path.Text = guerillaPath;
                game_exe_path.Text = gamePath;
                gen_type.SelectedIndex = profile.GameGeneration - 1;
                is_mcc.IsChecked = profile.IsMCCBuild;
                community_tools.IsChecked = profile.Community;
                ProfileSave();
            }
        }

        private void update_h2codez_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("H2Codez is obsolete, please update to MCC", "H2Codez Updater", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No)
                MessageBox.Show("What do you mean no? I told you it's obsolete", "Listen to me", MessageBoxButton.OK);
            return;
            /*
            CancelableProgressBarWindow<long> progress = new();
            progress.Status = "Fetching latest update";
            progress.Title = "Getting H2Codez update";

            GitHubReleases gitHubReleases = new();
            IReadOnlyList<GitHubReleases.Release> list = await gitHubReleases.GetReleasesForRepo("Project-Cartographer", "H2Codez");
            Debug.Print(list.ToString());
            Debug.Print(list[0].ToString());

            GitHubReleases.Release latestRelease = list[0];
            async Task<byte[]> GetAsset(string name)
            {
                progress.Status = $"Downloading {name}";
                progress.MaxValue = 0;
                progress.CurrentProgress = 0;
                return await gitHubReleases.DownloadReleaseAsset(
latestRelease.Assets.First(assert => assert.Name == name),
progress, progress.GetCancellationToken());
            }


            byte[] latestHash = await GetAsset("hash");
            byte[] latestDLL = await GetAsset("H2Codez.dll");

            progress.Complete = true;
                        */
        }


        private void isMCC_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
