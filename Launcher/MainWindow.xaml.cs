using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ToolkitLauncher.Properties;
using ToolkitLauncher.ToolkitInterface;
using System.Threading.Tasks;
using System.Windows.Navigation;
using System.Windows.Documents;
using System.Linq;

namespace ToolkitLauncher
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum StringType
    {
        [Description("HUD Messages")]
        hud_strings,
        [Description("Unicode Strings")]
        unicode_strings
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ModelContent
    {
        [Description("GBXModel")]
        gbxmodel,
        [Description("Render")]
        render
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum LightmapContent
    {
        [Description("Light Threshold")]
        light_threshold,
        [Description("Light Quality")]
        light_quality
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum CacheType
    {
        [Description("Classic")]
        classic,
        [Description("Remastered")]
        remastered
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ThemeType
    {
        [Description("Light")]
        light,
        [Description("Dark")]
        dark,
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum h2_quality_settings_stock
    {
        [Description("Checkerboard")]
        checkerboard,
        [Description("Draft Low")]
        draft_low,
        [Description("Draft Medium")]
        draft_medium,
        [Description("Draft High")]
        draft_high,
        [Description("Draft Super")]
        draft_super,
        [Description("Direct Only")]
        direct_only,
        [Description("Low")]
        low,
        [Description("Medium")]
        medium,
        [Description("High")]
        high,
        [Description("Super")]
        super,
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum h2_quality_settings_community
    {
        [Description("Checkerboard")]
        checkerboard,
        [Description("Draft Low")]
        draft_low,
        [Description("Draft Medium")]
        draft_medium,
        [Description("Draft High")]
        draft_high,
        [Description("Draft Super")]
        draft_super,
        [Description("Direct Only")]
        direct_only,
        [Description("Low")]
        low,
        [Description("Medium")]
        medium,
        [Description("High")]
        high,
        [Description("Super")]
        super,
        [Description("Custom")]
        custom,
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum h2_quality_settings_mcc
    {
        [Description("Checkerboard")]
        checkerboard,
        [Description("Cuban")]
        cuban,
        [Description("Draft Low")]
        draft_low,
        [Description("Draft Medium")]
        draft_medium,
        [Description("Draft High")]
        draft_high,
        [Description("Draft Super")]
        draft_super,
        [Description("Direct Only")]
        direct_only,
        [Description("Low")]
        low,
        [Description("Medium")]
        medium,
        [Description("High")]
        high,
        [Description("Super")]
        super,
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum h3_quality_settings_stock
    {
        // doesn't work?
        //[Description("Checkerboard")]
        //checkerboard,
        [Description("Direct Only")]
        direct_only,
        [Description("Draft")]
        draft,
        [Description("Debug")]
        debug,
        [Description("Low")]
        low,
        [Description("Medium")]
        medium,
        [Description("High")]
        high,
        [Description("Super")]
        super_slow,
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<ToolkitBase> toolkits = new();
        ToolkitBase toolkit
        {
            get
            {
                Debug.Assert(toolkit_selection.SelectedIndex != -1);
                return toolkits[toolkit_selection.SelectedIndex];
            }
        }

        [Flags]
        enum level_compile_type : byte
        {
            none = 0,
            compile = 2,
            light = 4,
        }
        level_compile_type levelCompileType;

        ModelCompile model_compile_type;
        enum object_type
        {
            biped,
            vehicle,
            weapon,
            equipment,
            garbage,
            projectile,
            scenery,
            machine,
            control,
            light_fixture,
            sound_scenery,
            crate,
            creature
        }

        enum sound_command_type
        {
            sounds_one_shot,
            sounds_single_layer,
            sounds_single_mixed,
        }

        enum codec_type
        {
            xbox,
            wav,
            ogg
        }

        enum import_type
        {
            projectile_impact,
            projectile_detonation,
            projectile_flyby,
            unused,
            weapon_fire,
            weapon_ready,
            weapon_reload,
            weapon_empty,
            weapon_charge,
            weapon_overheat,
            weapon_idle,
            weapon_melee,
            weapon_animation,
            object_impacts,
            particle_impacts,
            unit_footsteps,
            unit_dialog,
            unit_animation,
            vehicle_collision,
            vehicle_engine,
            vehicle_animation,
            device_door,
            device_machinery,
            device_stationary,
            music,
            ambient_nature,
            ambient_machinery,
            huge_ass,
            object_looping,
            cinematic_music,
            cortana_mission,
            cortana_cinematic,
            mission_dialog,
            cinematic_dialog,
            scripted_cinematic_foley,
            game_event,
            ui,
            test,
            multilingual_test
        }

        List<Object[]> levels_list = new List<Object[]>();

        // todo(num0005) this is ugly, rework it
        public static int profile_index = 0;
        public static List<int> profile_mapping = new();
        public static ToolkitProfiles.ProfileSettingsLauncher toolkit_profile
        {
            get
            {
                if (profile_index < 0 || profile_mapping.Count <= profile_index)
                    return null;
                return ToolkitProfiles.SettingsList[profile_mapping[profile_index]];
            }
        }

        public static bool halo_community
        {
            get
            {
                if (profile_mapping.Count > 0 && profile_index >= 0)
                {
                    return toolkit_profile.CommunityTools;
                }
                return false;
            }
        }

        public static bool halo_mcc
        {
            get
            {
                if (profile_mapping.Count > 0 && profile_index >= 0)
                {
                    return toolkit_profile.BuildType == build_type.release_mcc || toolkit_profile.GameGen > 1;
                }
                return false;
            }
        }

        public static bool halo_ce
        {
            get
            {
                if (profile_mapping.Count > 0 && profile_index >= 0)
                {
                    return toolkit_profile.GameGen == 0;
                }
                return false;
            }
        }

        public static bool halo_ce_standalone
        {
            get
            {
                if (profile_mapping.Count > 0 && profile_index >= 0)
                {
                    return toolkit_profile.GameGen == 0 && toolkit_profile.BuildType == build_type.release_standalone;
                }
                return false;
            }
        }

        public static bool halo_ce_standalone_community
        {
            get
            {
                if (profile_mapping.Count > 0 && profile_index >= 0)
                {
                    return toolkit_profile.GameGen == 0 && toolkit_profile.CommunityTools && toolkit_profile.BuildType == build_type.release_standalone;
                }
                return false;
            }
        }

        public static bool halo_ce_mcc
        {
            get
            {
                if (profile_mapping.Count > 0 && profile_index >= 0)
                {
                    return toolkit_profile.GameGen == 0 && toolkit_profile.BuildType == build_type.release_mcc;
                }
                return false;
            }
        }

        public static bool halo_2
        {
            get
            {
                if (profile_mapping.Count > 0 && profile_index >= 0)
                {
                    return toolkit_profile.GameGen == 1;
                }
                return false;
            }
        }

        public static bool halo_2_standalone
        {
            get
            {
                if (profile_mapping.Count > 0 && profile_index >= 0)
                {
                    return toolkit_profile.GameGen == 1 && toolkit_profile.BuildType == build_type.release_standalone;
                }
                return false;
            }
        }

        public static bool halo_2_standalone_stock
        {
            get
            {
                if (profile_mapping.Count > 0 && profile_index >= 0)
                {
                    return toolkit_profile.GameGen == 1 && !toolkit_profile.CommunityTools && toolkit_profile.BuildType == build_type.release_standalone;
                }
                return false;
            }
        }

        public static bool halo_2_standalone_community
        {
            get
            {
                if (profile_mapping.Count > 0 && profile_index >= 0)
                {
                    return toolkit_profile.GameGen == 1 && toolkit_profile.CommunityTools && toolkit_profile.BuildType == build_type.release_standalone;
                }
                return false;
            }
        }

        public static bool halo_2_mcc
        {
            get
            {
                if (profile_mapping.Count > 0 && profile_index >= 0)
                {
                    return toolkit_profile.GameGen == 1 && toolkit_profile.BuildType == build_type.release_mcc;
                }
                return false;
            }
        }

        public static bool halo_3
        {
            get
            {
                if (profile_mapping.Count > 0 && profile_index >= 0)
                {
                    return toolkit_profile.GameGen == 2;
                }
                return false;
            }
        }

        public static bool halo_3_odst
        {
            get
            {
                if (profile_mapping.Count > 0 && profile_index >= 0)
                {
                    return toolkit_profile.GameGen == 2 && toolkit_profile.BuildType == build_type.release_mcc;
                }
                return false;
            }
        }

        public static int string_encoding_index { get; set; }
        private bool handling_exception = false;
        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // if the code crashed trying to handle it once chances are it will just crash again
            if (handling_exception)
            {
                Environment.FailFast("Exception occured file processing an exception", e.Exception);
            }
            handling_exception = true;
            if (e.Exception is UnauthorizedAccessException)
            {
                e.Handled = true;
                handling_exception = false;
                MessageBox.Show(e.ToString(), "Permission denied!");
            }

            if (e.Exception is ToolkitBase.MissingFile)
            {
                e.Handled = true;
                handling_exception = false;
                var missing_file_excep = e.Exception as ToolkitBase.MissingFile;
                MessageBox.Show("The following executable is missing: " + missing_file_excep.FileName, "Corrupt Install");
            }
            else
            {
                MessageBoxResult result = MessageBox.Show(e.Exception.ToString(), "An unhandled exception has occurred!", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                    e.Handled = true;
            }
            handling_exception = false;
        }

        private string get_default_path(string textbox_string, bool tag_dir, bool is_file)
        {
            string base_path = toolkit.GetDataDirectory();
            string local_path = "";
            if (tag_dir is true)
            {
                base_path = toolkit.GetTagDirectory();
            }

            if (!string.IsNullOrWhiteSpace(textbox_string))
            {
                if (is_file == true)
                {
                    local_path = Path.GetDirectoryName(textbox_string);
                }
                else
                {
                    local_path = textbox_string;
                }
            }

            if (Directory.Exists(Path.Join(base_path, local_path)))
                return Path.Join(base_path, local_path);
            return base_path;
        }

        public MainWindow()
        {
#if !DEBUG
            Application.Current.DispatcherUnhandledException += App_DispatcherUnhandledException;
#endif

            // load profiles
            if (!ToolkitProfiles.Load())
                MessageBox.Show("Settings file was corrupted or had unexpected data. Generating new file.", "Invalid Settings File", MessageBoxButton.OK);

            // upgrade old settings
            if (Settings.Default.settings_update)
            {
                Settings.Default.Upgrade();
                Settings.Default.settings_update = false;
                Settings.Default.Save();
            }

            if (Settings.Default.first_run)
            {
                var dialog = new PathSettings(isFirstInit: true);
                dialog.ShowDialog();
                Settings.Default.first_run = false;
                Settings.Default.Save();
            }

            InitializeComponent();
            UpdateToolkitStatus();
            theme.SelectedIndex = Settings.Default.set_theme;
            instance_value.Text = Environment.ProcessorCount.ToString();
            DataContext = new ProfileIndexViewModel();
            AddHandler(Hyperlink.RequestNavigateEvent,
    new RequestNavigateEventHandler(RequestNavigateHandler));
        }

        private void RequestNavigateHandler(object sender, RequestNavigateEventArgs e)
        {
            Documentation.Data.HelpURL url = Documentation.Data.HelpURL.main;
            Enum.TryParse(e.Uri.ToString(), out url);
            Documentation.Contents.OpenURL(toolkit is not null ? toolkit.GetDocumentationName() : "_base", url);
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            int last_index = 0;
            if (toolkit_selection.SelectedIndex >= 0)
                last_index = toolkit_selection.SelectedIndex;
            Settings.Default.set_profile = last_index;
            Settings.Default.set_theme = theme.SelectedIndex;
            Settings.Default.Save();
        }

        private static ToolkitBase CreateToolkitFromProfile(ToolkitProfiles.ProfileSettingsLauncher profile)
        {
            string base_path = Path.GetDirectoryName(profile.ToolPath);
            Debug.Assert(base_path is not null, "base_path should never be null");

            Dictionary<ToolType, string> tool_paths = new()
            {
                { ToolType.Tool, profile.ToolPath },
                { ToolType.Guerilla, profile.GuerillaPath },
                { ToolType.Sapien, profile.SapienPath },
                { ToolType.Game, profile.GameExePath }
            };

            if (!string.IsNullOrWhiteSpace(profile.ToolFastPath))
                tool_paths.Add(ToolType.ToolFast, profile.ToolFastPath);

            ToolkitBase toolkit = null;

            switch (profile.GameGen)
            {
                case 0:
                    toolkit = profile.BuildType == build_type.release_standalone ?
                        new H1Toolkit(profile, base_path, tool_paths) :
                        new H1AToolkit(profile, base_path, tool_paths);
                    break;
                case 1:
                    toolkit = profile.BuildType == build_type.release_standalone ?
                        new H2Toolkit(profile, base_path, tool_paths) :
                        new H2AToolkit(profile, base_path, tool_paths);
                    break;
                case 2:
                    toolkit = profile.BuildType == build_type.release_standalone ?
                    toolkit = new H3Toolkit(profile, base_path, tool_paths) :
                    toolkit = new H3ODSTToolkit(profile, base_path, tool_paths);
                    break;
                default:
                    Debug.Assert(false, "Profile has a game gen that isn't supported. Defaulting to Halo 1");
                    toolkit = profile.BuildType == build_type.release_standalone ?
                        new H1Toolkit(profile, base_path, tool_paths) :
                        new H1AToolkit(profile, base_path, tool_paths);
                    break;
            }


            toolkit.ToolFailure = (Utility.Process.Result result) =>
            {
                //new ToolErrorDialog(result.Output, result.Error, result.ReturnCode).Show();
            };

            return toolkit;
        }

        private void UpdateToolkitStatus()
        {
            int current_index = toolkit_selection.SelectedIndex;
            int light_quality_level_index = light_quality_level.SelectedIndex;
            toolkits.Clear(); // num0005: actually clear this, don't just think about it!
            toolkit_selection.Items.Clear();
            profile_mapping.Clear();
            bool is_any_toolkit_enabled = false;
            for (int i = 0; i < ToolkitProfiles.SettingsList.Count; i++)
            {
                var current_profile = ToolkitProfiles.SettingsList[i];

                var current_toolkit = CreateToolkitFromProfile(ToolkitProfiles.SettingsList[i]);
                if (current_toolkit.IsEnabled())
                {
                    profile_mapping.Add(i);
                    toolkit_selection.Items.Add(new ComboBoxItem { Content = current_profile.ProfileName });
                    toolkits.Add(current_toolkit);

                    is_any_toolkit_enabled = true;
                }
                else
                {
                    Debug.Print($"Profile '{current_profile.ProfileName}' has been disabled!");
                }
            }
            programs_box.IsEnabled = is_any_toolkit_enabled;
            tasks_box.IsEnabled = is_any_toolkit_enabled;
            if (current_index >= 0)
            {
                //Checking that the last index used isn't a negative value
                if (profile_mapping.Count <= current_index)
                {
                    //Checking that the last index used isn't equal or greater to the list count.
                    //Since Comboboxes are zero indexed the count should always be greater
                    toolkit_selection.SelectedIndex = profile_mapping.Count - 1;
                }
                else
                {
                    //Last index used was still within a valid range
                    //Set the value since comboboxes were cleared
                    toolkit_selection.SelectedIndex = current_index;
                }
            }

            if (light_quality_level_index >= 0)
            {
                if (light_quality_level.Items.Count <= light_quality_level_index)
                {
                    light_quality_level.SelectedIndex = light_quality_level.Items.Count - 1;
                }
                else
                {
                    light_quality_level.SelectedIndex = light_quality_level_index;
                }
            }

            if (!is_any_toolkit_enabled)
                MessageBox.Show("No valid profiles were found, please set one in \"toolkit profiles\" to proceed", "No valid profiles!", MessageBoxButton.OK);
        }

        /// <summary>
        /// Handles checking whatever the multi instance flag needs to be set
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RunSapien(object sender, RoutedEventArgs e)
        {
            if (toolkit.IsMutexLocked(ToolType.Sapien))
            {
                // oh no an instance is already running, lets see what we can do
                if (toolkit is not H1AToolkit)
                {
                    // not H1A, can't do anything but let the user know
                    var result = MessageBox.Show(
                        "An instance of Sapien is already running, close that before trying to open another scenario.",
                        "Sorry about this!",
                        MessageBoxButton.OKCancel
                        );
                    if (result == MessageBoxResult.Cancel)
                        return;
                    // Launch Sapien anyways, no real harm in it
                    await toolkit.RunTool(ToolType.Sapien);
                }
                else
                {
                    var result = MessageBox.Show(
                        "An instance of Sapien is already running, do you wish to enable multi instance? This is experimental and may cause issues.",
                        "Enable Multi-instance?",
                        MessageBoxButton.YesNoCancel
                        );
                    if (result == MessageBoxResult.Cancel)
                        return;
                    if (result == MessageBoxResult.Yes)
                        await toolkit.RunTool(ToolType.Sapien, new() { "-multipleinstance" });
                    else
                        await toolkit.RunTool(ToolType.Sapien); // well have your way user!
                }
            }
            else
            {
                await toolkit.RunTool(ToolType.Sapien);
            }
        }
        private async void RunGuerilla(object sender, RoutedEventArgs e)
        {
            await toolkit.RunTool(ToolType.Guerilla);
        }

        private async void RunGame(object sender, RoutedEventArgs e)
        {
            await toolkit.RunTool(ToolType.Game);
        }

        private void level_compile_Click(object sender, RoutedEventArgs e)
        {
            string light_level = light_quality_level.SelectedItem.ToString();
            // tool doesn't support a value of 0 or 1, the bounds are [0, 1], so we adjust the value a bit to get something reasonable
            float light_level_slider = (float)Math.Max(Math.Min(light_quality_slider.ConvertedValue, 0.999999), 0.000001);
            int instance_count = 1;
            if (!halo_ce && !halo_2_standalone_stock)
            {
                if (instance_value.Text.Length == 0 ? true : Int32.TryParse(instance_value.Text, out instance_count))
                {
                    if (Environment.ProcessorCount < instance_count)
                    {
                        //Prevent people from setting the instance count higher than their PC can realisticly run.
                        MessageBox.Show(string.Format("Instance count exceeded logical processor count of {0}.", Environment.ProcessorCount) + "\n" + "Logical processor count is the cutoff." + "\n" + "This is for your own good.", "Woah there Partner", MessageBoxButton.OK);
                        instance_value.Text = Environment.ProcessorCount.ToString();
                        instance_count = Environment.ProcessorCount;
                    }
                }
                else
                {
                    MessageBox.Show("Invalid instance count!", "Error!");
                }
            }
            CompileLevel(compile_level_path.Text, bsp_path.Text, light_level, light_level_slider, Final.IsChecked is true, instance_count, phantom_hack.IsChecked is true, lightmap_group.Text);
        }

        private async void CompileLevel(string level_path, string bsp_path, string Level_Quality, float level_slider, bool radiosity_quality_toggle, int instance_count, bool phantom_fix, string lightmap_group)
        {
            if (levelCompileType.HasFlag(level_compile_type.compile))
            {
                bool is_release = true;
                StructureType structure_command = (StructureType)structure_import_type.SelectedIndex;

                await toolkit.ImportStructure(structure_command, level_path, phantom_fix, is_release, disable_asserts.IsChecked ?? false, struct_auto_fbx.IsChecked ?? false);
            }
            if (levelCompileType.HasFlag(level_compile_type.light))
            {
                var lightmaps_args = new ToolkitBase.LightmapArgs(
                    Level_Quality,
                    level_slider,
                    radiosity_quality_toggle,
                    disable_asserts.IsChecked ?? false,
                    lightmap_group,
                    instance_count,
                    instance_cmd.IsChecked ?? false
                    );
                var info = ToolkitBase.SplitStructureFilename(level_path, bsp_path);
                var scen_path = Path.Combine(info.ScenarioPath, info.ScenarioName);
                CancelableProgressBarWindow<int> progress = null;
                if (!halo_ce && !halo_2_standalone_stock && lightmaps_args.instanceCount > 1 || !halo_ce && !halo_2)
                {
                    progress = new CancelableProgressBarWindow<int>();
                    progress.Owner = this;
                }
                try
                {
                    await toolkit.BuildLightmap(scen_path, info.BspName, lightmaps_args, progress);
                }
                catch (OperationCanceledException)
                {
                }
                if (progress is not null)
                    progress.Complete = true;
            }
        }

        private async void CompileText(object sender, RoutedEventArgs e)
        {
            StringType string_item = (StringType)string_encoding.SelectedItem;
            H1Toolkit ce_toolkit = toolkit as H1Toolkit;
            var scenario_name = Path.GetFileNameWithoutExtension(compile_text_path.Text);
            if (string_item == StringType.hud_strings && ce_toolkit is not null)
            {
                await ce_toolkit.ImportHUDStrings(compile_text_path.Text, scenario_name);
            }
            else
            {
                await toolkit.ImportUnicodeStrings(compile_text_path.Text);
            }
        }

        class BitmapCompile
        {
            public static List<string> bitmapType = new List<string>()
            {
                "2d",
                "3d",
                "cubemaps",
                "sprites",
                "interface"
            };
        }

        private async void CompileImage(object sender, RoutedEventArgs e)
        {
            string listEntry = BitmapCompile.bitmapType[bitmap_compile_type.SelectedIndex];
            await toolkit.ImportBitmaps(compile_image_path.Text, listEntry, debug_plate.IsChecked is true);
        }

        private async void PackageLevel(object sender, RoutedEventArgs e)
        {
            if (levels_list.Count == 0)
            {
                // Halo 2 uses win64, win32, and xbox1 as platform options. Default if improper arg is given is win64
                string cache_platform = "win64";
                if (halo_3)
                    cache_platform = "pc";

                CacheType cache_type_item = (CacheType)cache_type.SelectedIndex;
                ToolkitBase.ResourceMapUsage usage = (ToolkitBase.ResourceMapUsage)resource_map_usage.SelectedIndex;

                await toolkit.BuildCache(package_level_path.Text, cache_type_item, usage, log_tag_loads.IsChecked ?? false, cache_platform, cache_compress.IsChecked ?? false, cache_resource_sharing.IsChecked ?? false, cache_multilingual_sounds.IsChecked ?? false, cache_remastered_support.IsChecked ?? false, cache_mp_tag_sharing.IsChecked ?? false);
            }
            else
            {
                foreach (Object[] level_to_compile in levels_list)
                {
                    await toolkit.BuildCache(level_to_compile[0].ToString(), (CacheType) level_to_compile[1], (ToolkitBase.ResourceMapUsage) level_to_compile[2], (bool) level_to_compile[3], level_to_compile[4].ToString(), (bool) level_to_compile[5], (bool) level_to_compile[6], (bool) level_to_compile[7], (bool)level_to_compile[8], (bool)level_to_compile[9]);
                }
            }
        }

        private async void AddLevelCompile(object sender, RoutedEventArgs e)
        {
            if (levels_list.Count == 0)
            {
                compile_button.Content = "Package Levels";
            }
            if (halo_3)
            {
                string cache_platform = "win64";
                cache_platform = "pc";
                CacheType cache_type_item = (CacheType)cache_type.SelectedIndex;
                ToolkitBase.ResourceMapUsage usage = (ToolkitBase.ResourceMapUsage)resource_map_usage.SelectedIndex;

                Object[] level_data = new Object[] { package_level_path.Text, cache_type_item, usage, log_tag_loads.IsChecked ?? false, cache_platform, cache_compress.IsChecked ?? false, cache_resource_sharing.IsChecked ?? false, cache_multilingual_sounds.IsChecked ?? false, cache_remastered_support.IsChecked ?? false, cache_mp_tag_sharing.IsChecked ?? false };
                levels_list.Add(level_data);
            }
        }

        private void CompileOnly_Checked(object sender, RoutedEventArgs e)
        {
            levelCompileType = level_compile_type.compile;
        }

        private void LightOnly_Checked(object sender, RoutedEventArgs e)
        {
            levelCompileType = level_compile_type.light;
        }

        private void CompileAndLight_Checked(object sender, RoutedEventArgs e)
        {
            levelCompileType = level_compile_type.compile | level_compile_type.light;
        }

        private void run_cmd_Click(object sender, RoutedEventArgs e)
        {
            var process = new ProcessStartInfo();
            process.FileName = "cmd";
            process.Arguments = "/K \"cd /d \"" + toolkit.BaseDirectory + "\"";
            Process.Start(process);
        }

        private void custom_tool_cmd_Click(object sender, RoutedEventArgs e)
        {
            Custom_Command.Visibility = Visibility.Visible;
        }

        private void custom_cancel_Click(object sender, RoutedEventArgs e)
        {
            Custom_Command.Visibility = Visibility.Collapsed;
            custom_command_text.Text = "";
            recent_cmds.SelectedIndex = -1;
        }

        private void custom_run_Click(object sender, RoutedEventArgs e)
        {
            Custom_Command.Visibility = Visibility.Collapsed;
            var process = new ProcessStartInfo();

            _ = toolkit.RunCustomToolCommand(custom_command_text.Text);

            if (!string.IsNullOrEmpty(custom_command_text.Text) && (recent_cmds.Items.Cast<ComboBoxItem>().Any(someitem => someitem.Content.Equals(custom_command_text.Text))) == false)
            {
                recent_cmds.IsEnabled = true;
                recent_cmds.Items.Add(new ComboBoxItem { Content = custom_command_text.Text });
            }

            custom_command_text.Text = "";
            recent_cmds.SelectedIndex = -1;
        }

        private void recent_cmds_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (recent_cmds.IsDropDownOpen)
            {
                string cmd = (recent_cmds.SelectedItem as ComboBoxItem).Content.ToString();
                Custom_Command.Visibility = Visibility.Visible;
                custom_command_text.Text = cmd;
            }
        }

        private void custom_command_text_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Custom_Command.Visibility = Visibility.Collapsed;
                var process = new ProcessStartInfo();

                _ = toolkit.RunCustomToolCommand(custom_command_text.Text);

                if (!string.IsNullOrEmpty(custom_command_text.Text) && (recent_cmds.Items.Cast<ComboBoxItem>().Any(someitem => someitem.Content.Equals(custom_command_text.Text))) == false)
                {
                    recent_cmds.IsEnabled = true;
                    recent_cmds.Items.Add(new ComboBoxItem { Content = custom_command_text.Text });
                }

                custom_command_text.Text = "";
                recent_cmds.SelectedIndex = -1;
            }
        }

        private void lightmap_config_Click(object sender, RoutedEventArgs e)
        {
            lightmap_config_ui.Visibility = Visibility.Visible;
            LightmapConfigUI();
        }

        private void model_compile_collision_Checked(object sender, RoutedEventArgs e)
        {
            model_compile_type = ModelCompile.collision;
        }

        private void model_compile_physics_Checked(object sender, RoutedEventArgs e)
        {
            model_compile_type = ModelCompile.physics;
        }

        private void model_compile_animations_Checked(object sender, RoutedEventArgs e)
        {
            model_compile_type = ModelCompile.animations;
        }

        private void model_compile_all_Checked(object sender, RoutedEventArgs e)
        {
            model_compile_type = ModelCompile.all;
        }

        private void model_compile_render_Checked(object sender, RoutedEventArgs e)
        {
            model_compile_type = ModelCompile.render;
        }

        private async void compile_model_Click(object sender, RoutedEventArgs e)
        {
            await toolkit.ImportModel(compile_model_path.Text, model_compile_type, phantom_hack_collision.IsChecked ?? false, h2_lod_logic.IsChecked ?? false, prt_enabled.IsChecked ?? false, fp_anim.IsChecked ?? false, character_fp_path.Text, weapon_fp_path.Text, accurate_render.IsChecked ?? false, verbose_anim.IsChecked ?? false, uncompressed_anim.IsChecked ?? false, sky_model.IsChecked ?? false, pda_model.IsChecked ?? false, reset_compression.IsChecked ?? false, model_auto_fbx.IsChecked ?? false, model_generate_shaders.IsChecked ?? false);
        }

        private async void import_sound_Click(object sender, RoutedEventArgs e)
        {
            sound_command_type sound_command = (sound_command_type)sound_import_type.SelectedIndex;
            codec_type platform = (codec_type)platform_type.SelectedIndex;
            import_type class_name = (import_type)class_dropdown.SelectedIndex;

            string sound_path = import_sound_path.Text;
            string bitrate_value = bitrate_slider.Value.ToString();
            string ltf_path = "data\\" + import_ltf_path.Text;

            await toolkit.ImportSound(sound_path, platform.ToString(), bitrate_value, ltf_path, sound_command.ToString(), class_name.ToString(), ((ComboBoxItem)sound_compression_type.SelectedItem).Content.ToString().ToLower());
        }

        private void spaces_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //Set handled to true if the key is space. Stops us from entering spaces in textboxes.
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        private void numbers_only(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = System.Text.RegularExpressions.Regex.IsMatch(e.Text, "[^0-9]+");
        }

        private void decimals_only(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = System.Text.RegularExpressions.Regex.IsMatch(e.Text, "[^0-9.]+");
        }

        readonly FilePicker.Options soundDataOptions = FilePicker.Options.FolderSelect(
           "Select sound folder",
           FilePicker.Options.PathRoot.Data
        );

        readonly FilePicker.Options soundTagOptions = FilePicker.Options.FileSelect(
            "Select sound tag",
            "Select sound tag|*.sound",
            FilePicker.Options.PathRoot.Tag,
            strip_extension: true
        );

        readonly FilePicker.Options LTFOptions = FilePicker.Options.FileSelect(
            "Select LTF",
            "Select LTF file|*.LTF",
            FilePicker.Options.PathRoot.Data,
            strip_extension: true
        );

        private void browse_sound_Click(object sender, RoutedEventArgs e)
        {
            bool tag_dir = false;
            bool is_file = false;
            var soundOptions = soundDataOptions;
            if (halo_2_standalone_community)
            {
                // Switching from sound compiling to LTF importing for H2Codez
                soundOptions = soundTagOptions;
                tag_dir = true;
            }

            string default_path = get_default_path(import_sound_path.Text, tag_dir, is_file);
            var picker = new FilePicker(import_sound_path, toolkit, soundOptions, default_path);
            picker.Prompt();
        }

        private void browse_ltf_Click(object sender, RoutedEventArgs e)
        {
            bool tag_dir = false;
            bool is_file = false;
            string default_path = get_default_path(import_ltf_path.Text, tag_dir, is_file);
            var picker = new FilePicker(import_ltf_path, toolkit, LTFOptions, default_path);
            picker.Prompt();
        }

        readonly FilePicker.Options ASSlevelOptions = FilePicker.Options.FileSelect(
            "Select your level",
            "map data|*.ASS;*.scenario",
            FilePicker.Options.PathRoot.Tag_Data,
            strip_extension: false
            );

        readonly FilePicker.Options JMSlevelOptions = FilePicker.Options.FileSelect(
            "Select your level",
            "map data|*.JMS;*.scenario",
            FilePicker.Options.PathRoot.Tag_Data,
            strip_extension: false
            );

        readonly FilePicker.Options ASSJMSlevelOptions = FilePicker.Options.FileSelect(
            "Select your level",
            "map data|*.ASS;*.JMS;*.scenario",
            FilePicker.Options.PathRoot.Tag_Data,
            strip_extension: false
            );

        private void browse_level_compile_Click(object sender, RoutedEventArgs e)
        {
            bool tag_dir = false;
            if (compile_level_path.Text.EndsWith(".scenario"))
                tag_dir = true;
            bool is_file = true;
            string default_path = get_default_path(compile_level_path.Text, tag_dir, is_file);
            var levelOptions = JMSlevelOptions;
            if (!halo_ce)
            {
                levelOptions = ASSlevelOptions;
                if (halo_2_standalone_community || halo_2_mcc)
                {
                    levelOptions = ASSJMSlevelOptions;
                }
            }
            var picker = new FilePicker(compile_level_path, toolkit, levelOptions, default_path);
            picker.Prompt();
            if (compile_level_path.Text.EndsWith(".scenario"))
                LightOnly.IsChecked = true;
        }

        readonly FilePicker.Options txtOptions = FilePicker.Options.FolderSelect(
           "Select a folder with txt files",
           FilePicker.Options.PathRoot.Data
        );

        private void Browse_text_Click(object sender, RoutedEventArgs e)
        {
            bool tag_dir = false;
            bool is_file = false;
            string default_path = get_default_path(compile_text_path.Text, tag_dir, is_file);
            var picker = new FilePicker(compile_text_path, toolkit, txtOptions, default_path);
            picker.Prompt();
        }

        readonly FilePicker.Options gen1BitmapOptions = FilePicker.Options.FileSelect(
           "Select Image File",
           "Supported image files|*.tif",
           FilePicker.Options.PathRoot.Data,
           parent: true
        );

        readonly FilePicker.Options gen2BitmapOptions = FilePicker.Options.FileSelect(
           "Select Image File",
           "Supported image files|*.tif;*.tiff;*.tga;*.jpg;*.bmp",
           FilePicker.Options.PathRoot.Data,
           parent: true
        );

        readonly FilePicker.Options gen2H2CodezBitmapOptions = FilePicker.Options.FileSelect(
           "Select Image File",
           "Supported image files|*.tif;*.tiff;*.tga;*.jpg;*.bmp;*.png",
           FilePicker.Options.PathRoot.Data,
           parent: true
        );

        readonly FilePicker.Options MCCBitmapOptions = FilePicker.Options.FileSelect(
           "Select Image File",
           "Supported image files|*.tif;*.tiff",
           FilePicker.Options.PathRoot.Data,
           parent: true
        );

        private void browse_bitmap_Click(object sender, RoutedEventArgs e)
        {
            bool tag_dir = false;
            bool is_file = false;
            string default_path = get_default_path(compile_image_path.Text, tag_dir, is_file);
            var bitmapOptions = gen1BitmapOptions;
            if (halo_2_standalone_stock)
            {
                bitmapOptions = gen2BitmapOptions;
            }
            else if (halo_2_standalone_community)
            {
                bitmapOptions = gen2H2CodezBitmapOptions;
            }
            else if (halo_mcc)
            {
                bitmapOptions = MCCBitmapOptions;
            }

            var picker = new FilePicker(compile_image_path, toolkit, bitmapOptions, default_path);
            picker.Prompt();
        }

        readonly FilePicker.Options packageOptions = FilePicker.Options.FileSelect(
           "Select Scenario",
           "Unpackaged Map|*.scenario",
           FilePicker.Options.PathRoot.Tag
        );

        private void browse_package_level_Click(object sender, RoutedEventArgs e)
        {
            string default_path = get_default_path(package_level_path.Text, tag_dir: true, is_file: true);
            var picker = new FilePicker(package_level_path, toolkit, packageOptions, default_path);
            picker.Prompt();
        }

        readonly FilePicker.Options modelOptions = FilePicker.Options.FolderSelect(
           "Select model folder",
           FilePicker.Options.PathRoot.Data
        );

        private void browse_model_Click(object sender, RoutedEventArgs e)
        {
            string default_path = get_default_path(compile_model_path.Text, tag_dir: false, is_file: false);
            var picker = new FilePicker(compile_model_path, toolkit, modelOptions, default_path);
            picker.Prompt();
        }

        private void browse_character_fp_Click(object sender, RoutedEventArgs e)
        {
            string default_path = get_default_path(character_fp_path.Text, tag_dir: false, is_file: false);
            var picker = new FilePicker(character_fp_path, toolkit, modelOptions, default_path);
            picker.Prompt();
        }

        private void browse_weapon_fp_Click(object sender, RoutedEventArgs e)
        {
            string default_path = get_default_path(weapon_fp_path.Text, tag_dir: false, is_file: false);
            var picker = new FilePicker(weapon_fp_path, toolkit, modelOptions, default_path);
            picker.Prompt();
        }

        private void toolkit_profiles_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PathSettings();
            dialog.ShowDialog();
            UpdateToolkitStatus();
        }

        private void about_Click(object sender, RoutedEventArgs e)
        {
            var credits = new Credits();
            credits.ShowDialog();
        }

        readonly FilePicker.Options bspOptions = FilePicker.Options.FileSelect(
           "Select BSP to light",
           "Compiled level geometry|*.scenario_structure_bsp",
           FilePicker.Options.PathRoot.Tag
        );

        private void browse_bsp_Click(object sender, RoutedEventArgs e)
        {
            string default_path = get_default_path(compile_level_path.Text, tag_dir: true, is_file: true);
            var picker = new FilePicker(bsp_path, toolkit, bspOptions, default_path);
            picker.Prompt();
        }

        private void toolkit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            light_quality_level.SelectedIndex = 0;
        }

        private void string_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string_encoding_index = string_encoding.SelectedIndex;
        }

        private void theme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ThemeType theme_type_item = (ThemeType)theme.SelectedIndex;
            var ThemeConfig = new LauncherThemeSettings();
            ThemeConfig.SetLauncherTheme(theme_type_item);
        }

        private LightmapConfigSettings lightmapConfig;
        public void LightmapConfigUI()
        {

            lightmapConfig = new(toolkit.BaseDirectory + "\\" + "custom_lightmap_quality.conf");
            LightmapSetUI();
        }

        private void lightmap_save_Click(object sender, RoutedEventArgs e)
        {
            lightmap_config_ui.Visibility = Visibility.Collapsed;
            SaveConfig();
        }

        private void lightmap_reset_Click(object sender, RoutedEventArgs e)
        {
            lightmapConfig.Reset();
            LightmapSetUI();
        }

        private void LightmapSetUI()
        {
            lightmap_is_checkboard.IsChecked = lightmapConfig.IsCheckerboard;
            lightmap_is_direct_only.IsChecked = lightmapConfig.IsDirectOnly;
            lightmap_is_draft.IsChecked = lightmapConfig.IsDraft;
            lightmap_sample_count.Text = lightmapConfig.SampleCount.ToString();
            lightmap_photon_count.Text = lightmapConfig.PhotonCount.ToString();
            lightmap_AA_sample_count.Text = lightmapConfig.AASampleCount.ToString();
            lightmap_gather_dist.Text = lightmapConfig.GatherDistance.ToString();
        }

        private void SaveConfig()
        {
            lightmapConfig.IsCheckerboard = lightmap_is_checkboard.IsChecked ?? false;
            lightmapConfig.IsDirectOnly = lightmap_is_direct_only.IsChecked ?? false;
            lightmapConfig.IsDraft = lightmap_is_draft.IsChecked ?? false;
            lightmapConfig.SampleCount = int.Parse(lightmap_sample_count.Text);
            lightmapConfig.PhotonCount = int.Parse(lightmap_photon_count.Text);
            lightmapConfig.AASampleCount = int.Parse(lightmap_AA_sample_count.Text);
            lightmapConfig.GatherDistance = float.Parse(lightmap_gather_dist.Text);

            if (!lightmapConfig.Save())
                MessageBox.Show($"Failed to save config to \"{lightmapConfig.Path}\". Check file system permissions!", "Error!");
        }

        private (string ext, string fbxFileName, string outputFileName)? PromptForFBXPaths(string title_string, string filter_string)
        {
            string outputFileName = "";

            var openDialog = new System.Windows.Forms.OpenFileDialog();
            openDialog.Title = "Select FBX (Filmbox)";
            openDialog.Filter = "FBX (Filmbox)|*.fbx";
            openDialog.InitialDirectory = Settings.Default.last_fbx_path;
            if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                // check if we need to update the initial directory
                string? fbxFileDir = Path.GetDirectoryName(openDialog.FileName);
                if (fbxFileDir != Settings.Default.last_fbx_path)
                {
                    Settings.Default.last_fbx_path = fbxFileDir;
                    Settings.Default.Save();
                }

                var saveDialog = new System.Windows.Forms.SaveFileDialog();

                saveDialog.OverwritePrompt = true;
                saveDialog.Title = title_string;
                saveDialog.Filter = filter_string;

                string data_dir = toolkit.GetDataDirectory();
                if (fbxFileDir.StartsWith(data_dir))
                    saveDialog.InitialDirectory = Path.GetDirectoryName(fbxFileDir);

                else
                    saveDialog.InitialDirectory = toolkit.GetDataDirectory();

                if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    outputFileName = saveDialog.FileName;
                    string ext = Path.GetExtension(outputFileName).ToLowerInvariant();
                    return (ext, openDialog.FileName, outputFileName);
                }
            }
            return null;
        }

        private async void convert_level_from_fbx_Click(object sender, RoutedEventArgs e)
        {
            if (halo_ce_standalone || halo_2_standalone)
            {
                Debug.Fail("toolkit is not MCC, FBX not supported!");
                return;
            }

            IToolkitFBX2ASS FBX2ASS = toolkit as IToolkitFBX2ASS;
            IToolkitFBX2Jointed FBX2Jointed = toolkit as IToolkitFBX2Jointed;

            (string ext, string fbxFileName, string outputFileName)? FBXArgs;

            switch (toolkit.Profile.GameGen)
            {
                case 0:
                    FBXArgs = PromptForFBXPaths("Select JMS save location", "Jointed model skeleton|*.JMS");
                    if (FBXArgs is not null)
                        await FBX2Jointed.JMSFromFBX(FBXArgs.Value.fbxFileName, FBXArgs.Value.outputFileName, null);
                    break;

                case 1:
                case 2:
                    FBXArgs = PromptForFBXPaths("Select JMS/ASS save location", "Jointed model skeleton|*.JMS|Amalgam scene specification|*.ASS");
                    if (FBXArgs is not null)
                    {
                        switch (FBXArgs.Value.ext)
                        {
                            case ".ass":
                                {
                                    await FBX2ASS.ASSFromFBX(FBXArgs.Value.fbxFileName, FBXArgs.Value.outputFileName);
                                    break;
                                }
                            case ".jms":
                                {
                                    var dialog = new GeoClassPrompt();
                                    dialog.ShowDialog();
                                    string geo_class = dialog.geo_class.Text.ToString().ToLower();
                                    await FBX2Jointed.JMSFromFBX(FBXArgs.Value.fbxFileName, FBXArgs.Value.outputFileName, geo_class);
                                    break;
                                }
                            default:
                                Debug.Fail($"Unexpected file extension: {FBXArgs.Value.ext}");
                                break;
                        }
                    }
                    break;

            }
        }

        private static async Task CreateJMAFromFBX(IToolkitFBX2Jointed toolkit, string fbxFileName, string outputFileName)
        {
            int? startFrame = null;
            int? endFrame = null;
            AnimLengthPrompt AnimDialog = new AnimLengthPrompt();
            bool? result = AnimDialog.ShowDialog();
            if (result == true)
            {
                int parsed_value;
                if (Int32.TryParse(AnimDialog.start_index.Text, out parsed_value))
                    startFrame = parsed_value;
                if (Int32.TryParse(AnimDialog.last_index.Text, out parsed_value))
                    endFrame = parsed_value;
            }
            await toolkit.JMAFromFBX(fbxFileName, outputFileName, startFrame ?? 0, endFrame);
        }

        private async void convert_model_from_fbx_Click(object sender, RoutedEventArgs e)
        {
            if (halo_ce_standalone || halo_2_standalone)
            {
                Debug.Fail("toolkit is not MCC, FBX not supported!");
                return;
            }

            IToolkitFBX2JMI FBX2JMI = toolkit as IToolkitFBX2JMI;
            IToolkitFBX2Jointed FBX2Jointed = toolkit as IToolkitFBX2Jointed;

            (string ext, string fbxFileName, string outputFileName)? FBXArgs;

            switch (toolkit.Profile.GameGen)
            {
                case 0:
                    FBXArgs = PromptForFBXPaths("Select JMS/JMA save location", "Jointed model skeleton|*.JMS|Jointed model animation|*.JMA");
                    if (FBXArgs is not null)
                        switch (FBXArgs.Value.ext)
                        {
                            case ".jma":
                                await CreateJMAFromFBX(FBX2Jointed, FBXArgs.Value.fbxFileName, FBXArgs.Value.outputFileName);
                                break;
                            case ".jms":
                                {
                                    string? geo_class = null;

                                    await FBX2Jointed.JMSFromFBX(FBXArgs.Value.fbxFileName, FBXArgs.Value.outputFileName, geo_class);
                                    break;
                                }
                            default:
                                Debug.Fail($"Unexpected file extension: {FBXArgs.Value.ext}");
                                break;
                        }
                    break;

                case 1:
                    FBXArgs = PromptForFBXPaths("Select JMS/JMA/JMI save location", "Jointed model skeleton|*.JMS|Jointed model animation|*.JMA|Jointed model instance|*.JMI");
                    if (FBXArgs is not null)
                        switch (FBXArgs.Value.ext)
                        {
                            case ".jma":
                                await CreateJMAFromFBX(FBX2Jointed, FBXArgs.Value.fbxFileName, FBXArgs.Value.outputFileName);
                                break;
                            case ".jms":
                                {
                                    string? geo_class = null;

                                    var dialog = new GeoClassPrompt();
                                    dialog.ShowDialog();
                                    geo_class = dialog.geo_class.Text.ToString().ToLower();

                                    await FBX2Jointed.JMSFromFBX(FBXArgs.Value.fbxFileName, FBXArgs.Value.outputFileName, geo_class);
                                    break;
                                }
                            case ".jmi":
                                await FBX2JMI.JMIFromFBX(FBXArgs.Value.fbxFileName, FBXArgs.Value.outputFileName);
                                break;
                            default:
                                Debug.Fail($"Unexpected file extension: {FBXArgs.Value.ext}");
                                break;
                        }
                    break;

                case 2:
                    FBXArgs = PromptForFBXPaths("Select JMS/JMA/JMI save location", "Jointed model skeleton|*.JMS|Jointed model animation|*.JMA|Jointed model instance|*.JMI");
                    if (FBXArgs is not null)
                        switch (FBXArgs.Value.ext)
                        {
                            case ".jma":
                                await CreateJMAFromFBX(FBX2Jointed, FBXArgs.Value.fbxFileName, FBXArgs.Value.outputFileName);
                                break;
                            case ".jms":
                                {
                                    string? geo_class = null;

                                    var dialog = new GeoClassPrompt();
                                    dialog.ShowDialog();
                                    geo_class = dialog.geo_class.Text.ToString().ToLower();

                                    await FBX2Jointed.JMSFromFBX(FBXArgs.Value.fbxFileName, FBXArgs.Value.outputFileName, geo_class);
                                    break;
                                }
                            case ".jmi":
                                await FBX2JMI.JMIFromFBX(FBXArgs.Value.fbxFileName, FBXArgs.Value.outputFileName);
                                break;
                            default:
                                Debug.Fail($"Unexpected file extension: {FBXArgs.Value.ext}");
                                break;
                        }
                    break;
            }
        }

        private void open_explorer_Click(object sender, RoutedEventArgs e)
        {
            Process process = new();
            process.StartInfo.FileName = toolkit.BaseDirectory;
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }
    }
}
