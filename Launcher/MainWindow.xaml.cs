using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.IO;
using System.Diagnostics;
using ToolkitLauncher.Properties;
using ToolkitLauncher.ToolkitInterface;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;
using System.Threading.Tasks;

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
    public enum PRTContent
    {
        [Description("Render")]
        render,
        [Description("Render PRT")]
        render_prt
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum RadiosityContent
    {
        [Description("Draft")]
        draft,
        [Description("Final")]
        final
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ToolkitBase[] toolkits = new ToolkitBase[]
        {
            new H1Toolkit(),
            new H2Toolkit()
        };
        ToolkitBase toolkit {
            get
            {
                int game_gen_index = toolkit_profile.game_gen;
                if (game_gen_index > 1)
                    game_gen_index = 1;
                return toolkits[game_gen_index];
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

        [Flags]
        enum model_compile : Byte
        {
            none = 0,
            collision = 2,
            physics = 4,
            render = 8,
            animations = 16,
            all = 32,
        }
        model_compile model_compile_type;

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

        public static ToolkitProfiles.ProfileSettingsLauncher toolkit_profile
        {
            get
            {
                int profile_int = 0;
                if (profile_index >= 0)
                    profile_int = profile_index;

                return ToolkitProfiles.SettingsList[profile_int];
            }
        }

        public static bool halo_ce_mcc
        {
            get
            {
                if (ToolkitProfiles.SettingsList.Count > 0)
                {
                    return toolkit_profile.game_gen == 0 && toolkit_profile.build_type == build_type.release_mcc;
                }
                return false;
            }
        }

        public static bool halo_2_mcc
        {
            get
            {
                if (ToolkitProfiles.SettingsList.Count > 0)
                {
                    return toolkit_profile.game_gen == 1 && toolkit_profile.build_type == build_type.release_mcc;
                }
                return false;
            }
        }

        public static bool halo_2_standalone
        {
            get
            {
                if (ToolkitProfiles.SettingsList.Count > 0)
                {
                    return toolkit_profile.game_gen == 1 && toolkit_profile.build_type == build_type.release_standalone;
                }
                return false;
            }
        }

        public static bool halo_2_standalone_community
        {
            get
            {
                if (ToolkitProfiles.SettingsList.Count > 0)
                {
                    return toolkit_profile.game_gen == 1 && toolkit_profile.community_tools && toolkit_profile.build_type == build_type.release_standalone;
                }
                return false;
            }
        }

        public static bool halo_2_standalone_not_community
        {
            get
            {
                if (ToolkitProfiles.SettingsList.Count > 0)
                {
                    return toolkit_profile.game_gen == 1 && !toolkit_profile.community_tools && toolkit_profile.build_type == build_type.release_standalone;
                }
                return false;
            }
        }

        public static bool halo_2_internal
        {
            get
            {
                if (ToolkitProfiles.SettingsList.Count > 0)
                {
                    return toolkit_profile.game_gen == 1 && toolkit_profile.build_type == build_type.release_internal;
                }
                return false;
            }
        }

        public static bool halo_2
        {
            get
            {
                if (ToolkitProfiles.SettingsList.Count > 0)
                {
                    return toolkit_profile.game_gen == 1;
                }
                return false;
            }
        }

        public static int profile_index;
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
            } else {
                MessageBoxResult result = MessageBox.Show(e.Exception.ToString(), "An unhandled exception has occurred!", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                    e.Handled = true;
            }
            handling_exception = false;
        }

        private string get_default_path(string textbox_string, bool tag_dir, bool is_file)
        {
            string path = toolkit.GetDataDirectory();
            if (tag_dir is true)
            {
                path = toolkit.GetTagDirectory();
            }

            if (!string.IsNullOrWhiteSpace(textbox_string))
            {
                if (is_file == true)
                {
                    path = path + Path.GetDirectoryName(textbox_string);
                }
                else
                {
                    path = path + textbox_string;
                }
            }

            return path;
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
            int default_index = 0;
            if (Settings.Default.set_profile >= 0)
                default_index = Settings.Default.set_profile;
            toolkit_selection.SelectedIndex = default_index;
            DataContext = new PackageResourceVisibility.MyViewModel();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            int last_index = 0;
            if (toolkit_selection.SelectedIndex >= 0)
                last_index = toolkit_selection.SelectedIndex;
            Settings.Default.set_profile = last_index;
            Settings.Default.Save();
        }

        private void UpdateToolkitStatus()
        {
            toolkit_selection.Items.Clear();
            bool is_any_toolkit_enabled = false;
            // always reset if we had no toolkits enabled before
            bool reset_selection_index = toolkit_selection.SelectedIndex == -1;
            int? lowest_valid_index = null;
            for (int i = 0; i < ToolkitProfiles.SettingsList.Count; i++)
            {
                toolkit_selection.Items.Add(ToolkitProfiles.SettingsList[i].profile_name);
                toolkit_selection.SelectedIndex = i;
                if (toolkit.IsEnabled())
                {
                    is_any_toolkit_enabled = true;
                    lowest_valid_index = lowest_valid_index ?? i;
                }
                else
                {
                    if (i == toolkit_selection.SelectedIndex)
                        reset_selection_index = true;
                }
            }
            programs_box.IsEnabled = is_any_toolkit_enabled;
            tasks_box.IsEnabled = is_any_toolkit_enabled;
                if (reset_selection_index)
                toolkit_selection.SelectedIndex = lowest_valid_index ?? -1;
            if (!is_any_toolkit_enabled)
                MessageBox.Show("No valid profiles were found, please set one in \"toolkit profiles\" to proceed", "No valid profiles!", MessageBoxButton.OK);
        }


        private async void RunSapien(object sender, RoutedEventArgs e)
        {
            await toolkit.RunTool(ToolType.Sapien);
        }
        private async void RunGuerilla(object sender, RoutedEventArgs e)
        {
            await toolkit.RunTool(ToolType.Guerilla);
        }

        private void HandleClickCompile(object sender, RoutedEventArgs e)
        {
            var light_level_combobox = (ToolkitBase.LightmapArgs.Level_Quality)light_quality_level.SelectedIndex;
            float light_level_slider = (float)light_quality_slider.Value;
            bool radiosity_quality_toggle = (bool)radiosity_quality.IsChecked;
            Int32 instance_count = 1;
            if (instance_value.Text.Length == 0 ? true : Int32.TryParse(instance_value.Text, out instance_count))
            {
                if (halo_2_standalone_not_community)
                    //If there is no instance support then set whatever got passed back to 1
                    instance_count = 1;
                else if (Environment.ProcessorCount < instance_count)
                {
                    //Prevent people from setting the instance count higher than their PC can realisticly run.
                    MessageBox.Show(string.Format("Instance count exceeded logical processor count of {0}.", Environment.ProcessorCount) + "\n" + "Logical processor count is the cutoff." + "\n" + "This is for your own good.", "Woah there Partner", MessageBoxButton.OK);
                    instance_value.Text = Environment.ProcessorCount.ToString();
                    instance_count = Environment.ProcessorCount;
                }
                CompileLevel(compile_level_path.Text, light_level_combobox, light_level_slider, radiosity_quality_toggle, instance_count, phantom_hack.IsChecked is true);
            }
            else
            {
                MessageBox.Show("Invalid instance count!", "Error!");
            }
        }

        private async void CompileLevel(string level_path, ToolkitBase.LightmapArgs.Level_Quality Level_Quality, float level_slider, bool radiosity_quality_toggle, int instance_count, bool phantom_fix)
        {
            if (levelCompileType.HasFlag(level_compile_type.compile))
            {
                await toolkit.ImportStructure(level_path, phantom_fix);
            }
            if (levelCompileType.HasFlag(level_compile_type.light))
            {
                var lightmaps_args = new ToolkitBase.LightmapArgs(Level_Quality, level_slider, radiosity_quality_toggle, .999f);
                var info = ToolkitBase.SplitStructureFilename(level_path);
                var scen_path = Path.Combine(info.ScenarioPath, info.ScenarioName);
                H2Toolkit h2toolkit = toolkit as H2Toolkit;
                if (instance_count < 2)
                {
                    await toolkit.BuildLightmap(scen_path, info.BspName, lightmaps_args);
                }
                else if (h2toolkit is not null)
                {
                    await h2toolkit.BuildLightmapMultiInstance(scen_path, info.BspName, lightmaps_args, instance_count);
                }

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
                "inteface"
            };
        }

        private async void CompileImage(object sender, RoutedEventArgs e)
        {
            string listEntry = BitmapCompile.bitmapType[bitmap_compile_type.SelectedIndex];
            await toolkit.ImportBitmaps(compile_image_path.Text, listEntry);
        }

        private async void PackageLevel(object sender, RoutedEventArgs e)
        {
            await toolkit.BuildCache(package_level_path.Text, cache_type.SelectedIndex, update_resource_maps.IsChecked is true);
        }

        private void CompileOnly_Checked(object sender, RoutedEventArgs e)
        {
            levelCompileType = level_compile_type.compile;
            light_quality_select_box.IsEnabled = false;
        }

        private void LightOnly_Checked(object sender, RoutedEventArgs e)
        {
            levelCompileType = level_compile_type.light;
            light_quality_select_box.IsEnabled = true;
        }

        private void CompileAndLight_Checked(object sender, RoutedEventArgs e)
        {
            levelCompileType = level_compile_type.compile | level_compile_type.light;
            light_quality_select_box.IsEnabled = true;
        }

        private void run_cmd_Click(object sender, RoutedEventArgs e)
        {
            var process = new ProcessStartInfo();
            process.FileName = "cmd";
            process.Arguments = "/K \"cd /d \"" + toolkit.GetBaseDirectory() + "\"";
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
        }

        private void custom_run_Click(object sender, RoutedEventArgs e)
        {
            Custom_Command.Visibility = Visibility.Collapsed;
            var process = new ProcessStartInfo();

            _ = toolkit.RunCustomToolCommand(custom_command_text.Text);

            custom_command_text.Text = "";
        }

        private void lightmap_config_Click(object sender, RoutedEventArgs e)
        {
            lightmap_config_ui.Visibility = Visibility.Visible;
            LightmapConfigUI();
        }

        private async void lightmap_save_Click(object sender, RoutedEventArgs e)
        {
            lightmap_config_ui.Visibility = Visibility.Collapsed;
            await SaveConfig(toolkit.GetBaseDirectory() + "\\" + "custom_lightmap_quality.conf");
        }

        private void lightmap_reset_Click(object sender, RoutedEventArgs e)
        {
            lightmap_is_checkboard.IsChecked = false;
            lightmap_is_direct_only.IsChecked = false;
            lightmap_is_draft.IsChecked = false;
            lightmap_primary_monte_carlo_count.Text = "8";
            lightmap_proton_count.Text = "20000000";
            lightmap_secondary_monte_carlo_count.Text = "4";
            lightmap_unk7_count.Text = "4.000000";
        }

        private void model_compile_collision_Checked(object sender, RoutedEventArgs e)
        {
            model_compile_type = model_compile.collision;
        }

        private void model_compile_physics_Checked(object sender, RoutedEventArgs e)
        {
            model_compile_type = model_compile.physics;
        }

        private void model_compile_animations_Checked(object sender, RoutedEventArgs e)
        {
            model_compile_type = model_compile.animations;
        }

        private void model_compile_all_Checked(object sender, RoutedEventArgs e)
        {
            model_compile_type = model_compile.all;
        }

        private void model_compile_render_Checked(object sender, RoutedEventArgs e)
        {
            model_compile_type = model_compile.render;
        }

        private async void compile_model_Click(object sender, RoutedEventArgs e)
        {
            string path = compile_model_path.Text;
            string import_type = model_compile_type.ToString();
            bool render_prt_toggle = (bool)render_prt.IsChecked;
            await toolkit.ImportModel(path, import_type, render_prt_toggle);
        }

        private async void import_sound_Click(object sender, RoutedEventArgs e)
        {
            sound_command_type sound_command = (sound_command_type)sound_import_type.SelectedIndex;
            codec_type platform = (codec_type)platform_type.SelectedIndex;
            import_type class_name = (import_type)class_dropdown.SelectedIndex;
            string sound_path = import_sound_path.Text;
            string bitrate_value = bitrate_slider.Value.ToString();
            string ltf_path = import_ltf_path.Text;
            await toolkit.ImportSound(sound_command.ToString(), sound_path, platform.ToString(), class_name.ToString(), bitrate_value, "data\\" + ltf_path);
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
            if (halo_2_standalone_community) // Switching from sound compiling to LTF importing for H2Codez
            {
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
            "Select Uncompiled level",
            "Uncompiled map geometry|*.ASS",
            FilePicker.Options.PathRoot.Data,
            strip_extension: false
            );

        readonly FilePicker.Options JMSlevelOptions = FilePicker.Options.FileSelect(
            "Select Uncompiled level",
            "Uncompiled map geometry|*.JMS",
            FilePicker.Options.PathRoot.Data,
            strip_extension: false
            );

        private void browse_level_compile_Click(object sender, RoutedEventArgs e)
        {
            bool tag_dir = false;
            bool is_file = true;
            string default_path = get_default_path(compile_level_path.Text, tag_dir, is_file);
            var levelOptions = JMSlevelOptions;
            if (halo_2)
            {
                levelOptions = ASSlevelOptions;
            }
            var picker = new FilePicker(compile_level_path, toolkit, levelOptions, default_path);
            picker.Prompt();
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

        private void browse_bitmap_Click(object sender, RoutedEventArgs e)
        {
            bool tag_dir = false;
            bool is_file = false;
            string default_path = get_default_path(compile_image_path.Text, tag_dir, is_file);
            var bitmapOptions = gen1BitmapOptions;
            if (halo_2_standalone_community)
            {
                bitmapOptions = gen2H2CodezBitmapOptions;
            }
            else if (halo_2_standalone_not_community)
            {
                bitmapOptions = gen2BitmapOptions;
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
            bool tag_dir = true;
            bool is_file = true;
            string default_path = get_default_path(package_level_path.Text, tag_dir, is_file);
            var picker = new FilePicker(package_level_path, toolkit, packageOptions, default_path);
            picker.Prompt();
        }

        readonly FilePicker.Options modelOptions = FilePicker.Options.FolderSelect(
           "Select model folder",
           FilePicker.Options.PathRoot.Data
        );

        private void browse_model_Click(object sender, RoutedEventArgs e)
        {
            bool tag_dir = false;
            bool is_file = false;
            string default_path = get_default_path(compile_model_path.Text, tag_dir, is_file);
            var picker = new FilePicker(compile_model_path, toolkit, modelOptions, default_path);
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

        private void toolkit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (light_quality_level != null && ToolkitProfiles.SettingsList.Count > 0 && profile_index >= 0)
            {
                int super_index = 9;
                int custom_index = 10;

                ComboBoxItem custom_quality = (ComboBoxItem)light_quality_level.Items[custom_index];
                custom_quality.IsEnabled = false;
                if (halo_2_standalone_community)
                {
                    custom_quality.IsEnabled = true;
                }
                else
                {
                    if (light_quality_level.SelectedIndex == custom_index)
                    {
                        light_quality_level.SelectedIndex = super_index;
                    }
                }
            }
        }

        private void string_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string_encoding_index = string_encoding.SelectedIndex;
        }

        public void LightmapConfigUI()
        {

            LightmapConfigSettings.ReadConfig(toolkit.GetBaseDirectory() + "\\" + "custom_lightmap_quality.conf");
            LightmapSetUI();
        }

        private void LightmapSetUI()
        {
            lightmap_is_checkboard.IsChecked = LightmapConfigSettings.ConfigSettings.is_checkboard;
            lightmap_is_direct_only.IsChecked = LightmapConfigSettings.ConfigSettings.is_direct_only;
            lightmap_is_draft.IsChecked = LightmapConfigSettings.ConfigSettings.is_draft;
            lightmap_primary_monte_carlo_count.Text = LightmapConfigSettings.ConfigSettings.main_monte_carlo_setting.ToString();
            lightmap_proton_count.Text = LightmapConfigSettings.ConfigSettings.proton_count.ToString();
            lightmap_secondary_monte_carlo_count.Text = LightmapConfigSettings.ConfigSettings.secondary_monte_carlo_setting.ToString();
            lightmap_unk7_count.Text = LightmapConfigSettings.ConfigSettings.unk7.ToString();
        }

        private async Task SaveConfig(string path)
        {
            File.Delete(path);

            using StreamWriter file = new(path, append: true);
            await file.WriteLineAsync("is_checkboard = " + lightmap_is_checkboard.IsChecked.ToString().ToLower());
            await file.WriteLineAsync("is_direct_only = " + lightmap_is_direct_only.IsChecked.ToString().ToLower());
            await file.WriteLineAsync("is_draft = " + lightmap_is_draft.IsChecked.ToString().ToLower());
            await file.WriteLineAsync("main_monte_carlo_setting = " + lightmap_primary_monte_carlo_count.Text);
            await file.WriteLineAsync("proton_count = " + lightmap_proton_count.Text);
            await file.WriteLineAsync("secondary_monte_carlo_setting = " + lightmap_secondary_monte_carlo_count.Text);
            await file.WriteLineAsync("unk7 = " + lightmap_unk7_count.Text);
        }
    }

    /*
     * Based on https://brianlagunas.com/a-better-way-to-data-bind-enums-in-wpf/
     */

    public class EnumBindingSourceExtension : MarkupExtension
    {
        private Type _enumType;
        public Type EnumType
        {
            get { return this._enumType; }
            set
            {
                if (value != this._enumType)
                {
                    if (null != value)
                    {
                        Type enumType = Nullable.GetUnderlyingType(value) ?? value;

                        if (!enumType.IsEnum)
                            throw new ArgumentException("Type must be for an Enum.");
                    }

                    this._enumType = value;
                }
            }
        }

        public EnumBindingSourceExtension() { }

        public EnumBindingSourceExtension(Type enumType)
        {
            this.EnumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (null == this._enumType)
                throw new InvalidOperationException("The EnumType must be specified.");

            Type actualEnumType = Nullable.GetUnderlyingType(this._enumType) ?? this._enumType;
            Array enumValues = Enum.GetValues(actualEnumType);

            if (actualEnumType == this._enumType)
                return enumValues;

            Array tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
            enumValues.CopyTo(tempArray, 1);
            return tempArray;
        }
    }

    public class EnumDescriptionTypeConverter : EnumConverter
    {
        public EnumDescriptionTypeConverter(Type type)
            : base(type)
        {
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value != null)
                {
                    FieldInfo fi = value.GetType().GetField(value.ToString());
                    if (fi != null)
                    {
                        var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                        return ((attributes.Length > 0) && (!String.IsNullOrEmpty(attributes[0].Description))) ? attributes[0].Description : value.ToString();
                    }
                }

                return string.Empty;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class TextInputToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Always test MultiValueConverter inputs for non-null
            // (to avoid crash bugs for views in the designer)
            if (values[0] is bool && values[1] is bool)
            {
                bool hasText = !(bool)values[0];
                bool hasFocus = (bool)values[1];
                if (hasFocus || hasText)
                    return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DropdownSelectionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int game_gen_index = 0;
            var vis = Visibility.Collapsed;
            if (ToolkitProfiles.SettingsList != null && (int)value >= 0) //Not sure what to do here. Crashes designer otherwise cause the list or value is empty
            {
                game_gen_index = ToolkitProfiles.SettingsList[(int)value].game_gen;
            }
            else
            {
                vis = Visibility.Visible;
            }
            if (parameter is string && Int32.Parse(parameter as string) == game_gen_index)
                return Visibility.Visible;
            return vis;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class H2VDropdownSelectionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            build_type build_type = (build_type)0;
            bool community_tools = false;
            int game_gen_index = 0;
            var vis = Visibility.Collapsed;
            if (ToolkitProfiles.SettingsList != null && (int)value >= 0) //Not sure what to do here. Crashes designer otherwise cause the list or value is empty
            {
                game_gen_index = ToolkitProfiles.SettingsList[(int)value].game_gen;
                build_type = ToolkitProfiles.SettingsList[(int)value].build_type;
                community_tools = ToolkitProfiles.SettingsList[(int)value].community_tools;
            }
            else
            {
                vis = Visibility.Visible;
            }
            if (parameter is string && Int32.Parse(parameter as string) == game_gen_index && !community_tools && build_type != build_type.release_standalone)
                return Visibility.Visible;
            return vis;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ToolkitToLevelSpaceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (MainWindow.halo_ce_mcc || MainWindow.halo_2_standalone_community || MainWindow.halo_2_internal)
                return new GridLength(8);
            if (ToolkitProfiles.SettingsList.Count > 0)
            {
                return new GridLength(0);
            }
            else
            {
                //Either we're in desinger or there are no profiles. Reveal ourselves either way.
                return new GridLength(8);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ToolkitToSoundSpaceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (MainWindow.halo_2_standalone_community)
                return new GridLength(8);
            if (ToolkitProfiles.SettingsList.Count > 0)
            {
                return new GridLength(0);
            }
            else
            {
                //Either we're in desinger or there are no profiles. Reveal ourselves either way.
                return new GridLength(8);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GameGenToIsEnabled : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int game_gen_index = 0;
            if (ToolkitProfiles.SettingsList != null && (int)value >= 0) //Not sure what to do here. Crashes designer otherwise cause the list or value is empty
            {
                game_gen_index = ToolkitProfiles.SettingsList[(int)value].game_gen;
            }
            if (parameter is string && Int32.Parse(parameter as string) == game_gen_index)
                return true;
            return false;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CommunityToolsToIsEnabled : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (MainWindow.halo_2_standalone_not_community)
                return false;
            return true;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LightmapConfigModifier : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int lightmap_quality = (int)values[0];
            int toolkit_selection = (int)values[1];
            if (ToolkitProfiles.SettingsList != null && lightmap_quality >= 0 && toolkit_selection >= 0) //Not sure what to do here. Crashes designer otherwise cause the list or value is empty
            {
                if (MainWindow.toolkit_profile.game_gen >= 1 && lightmap_quality == 10)
                    return true;
                return false;
            }
            return false;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CommunityToolsToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (MainWindow.halo_2_standalone_community || MainWindow.halo_2_mcc || MainWindow.halo_2_internal)
                return Visibility.Visible;
            if (ToolkitProfiles.SettingsList.Count > 0)
            {
                return Visibility.Collapsed;
            }
            else
            {
                //Either we're in desinger or there are no profiles. Reveal ourselves either way.
                return Visibility.Visible;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class H2VCommunityToolsToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (MainWindow.halo_2_standalone_community)
                return Visibility.Visible;
            if (ToolkitProfiles.SettingsList.Count > 0)
            {
                return Visibility.Collapsed;
            }
            else
            {
                //Either we're in desinger or there are no profiles. Reveal ourselves either way.
                return Visibility.Visible;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ModelContentModifier : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int index = 0;
            if (ToolkitProfiles.SettingsList != null && (int)value >= 0) //Not sure what to do here. Crashes designer otherwise cause the list or value is empty
            {
                index = ToolkitProfiles.SettingsList[(int)value].game_gen;
            }
            return ((ModelContent)index);
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TextContentModifier : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int string_encoding = (int)values[0];
            int toolkit_selection = (int)values[1];
            string text_string = "Select a folder with an .hmt file to compile.";
            if (ToolkitProfiles.SettingsList != null && string_encoding >= 0 && toolkit_selection >= 0) //Not sure what to do here. Crashes designer otherwise cause the list or value is empty
            {
                if (ToolkitProfiles.SettingsList[toolkit_selection].game_gen >= 1 || string_encoding == 1)
                    text_string = "Select a folder with .txt files to compile.";
            }
            return text_string;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PRTContentModifier : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value == false)
            {
                return ((PRTContent)0);
            }
            else
            {
                return ((PRTContent)1);
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RadiosityContentModifier : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value == false)
            {
                return ((RadiosityContent)0);
            }
            else
            {
                return ((RadiosityContent)1);
            }

        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PRTToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (MainWindow.halo_2_mcc || MainWindow.halo_2_internal)
                return Visibility.Visible;
            if (ToolkitProfiles.SettingsList.Count > 0)
            {
                return Visibility.Collapsed;
            }
            else
            {
                //Either we're in desinger or there are no profiles. Reveal ourselves either way.
                return Visibility.Visible;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PhantomVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int game_gen_index = 0;
            build_type build_type = (build_type)0;
            var vis = Visibility.Collapsed;
            if (ToolkitProfiles.SettingsList != null && (int)value >= 0) //Not sure what to do here. Crashes designer otherwise cause the list or value is empty
            {
                game_gen_index = ToolkitProfiles.SettingsList[(int)value].game_gen;
                build_type = ToolkitProfiles.SettingsList[(int)value].build_type;
            }
            else
            {
                vis = Visibility.Visible;
            }
            if (parameter is string && Int32.Parse(parameter as string) == game_gen_index)
                if (build_type == build_type.release_mcc)
                    vis = Visibility.Visible;
            return vis;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PackageResourceVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int game_gen_index = 0;
            build_type build_type = (build_type)0;
            var vis = Visibility.Collapsed;
            if (ToolkitProfiles.SettingsList != null && (int)value >= 0) //Not sure what to do here. Crashes designer otherwise cause the list or value is empty
            {
                game_gen_index = ToolkitProfiles.SettingsList[(int)value].game_gen;
                build_type = ToolkitProfiles.SettingsList[(int)value].build_type;
            }
            else
            {
                vis = Visibility.Visible;
            }
            if (parameter is string && Int32.Parse(parameter as string) == game_gen_index && build_type == build_type.release_mcc)
                vis = Visibility.Visible;
            return vis;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public class MyViewModel : INotifyPropertyChanged
        {
            public MyViewModel()
            {
                SelectedProfileIndex = 0;
            }
            private int _SelectedProfileIndex;
            public int SelectedProfileIndex
            {
                get
                {
                    return _SelectedProfileIndex;
                }
                set
                {
                    _SelectedProfileIndex = value;
                    MainWindow.profile_index = value;
                    Notify("SelectedProfileIndex");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void Notify(string propertyName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class LightmapConfigSettings
    {
        public static LightmapConfig ConfigSettings = new LightmapConfig();

        public class LightmapConfig
        {
            public bool is_checkboard { get; set; }
            public bool is_direct_only { get; set; }
            public bool is_draft { get; set; }
            public int main_monte_carlo_setting { get; set; }
            public int proton_count { get; set; }
            public int secondary_monte_carlo_setting { get; set; }
            public float unk7 { get; set; }
        }

        public static void ReadConfig(string path)
        {
            if (File.Exists(path))
            {
                string theFile = path;
                using (StreamReader sr = new StreamReader(theFile))
                {
                    string line = "";
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim();
                            string value = parts[1].Trim();
                            bool bool_pass;
                            int int_pass;
                            float float_pass;
                            if (key == "is_checkboard")
                            {
                                if (bool.TryParse(value, out bool_pass))
                                {
                                    bool_pass = bool.Parse(value);
                                }
                                ConfigSettings.is_checkboard = bool_pass;
                            }
                            else if (key == "is_direct_only")
                            {
                                if (bool.TryParse(value, out bool_pass))
                                {
                                    bool_pass = bool.Parse(value);
                                }
                                ConfigSettings.is_direct_only = bool_pass;
                            }
                            else if (key == "is_draft")
                            {
                                if (bool.TryParse(value, out bool_pass))
                                {
                                    bool_pass = bool.Parse(value);
                                }
                                ConfigSettings.is_draft = bool_pass;
                            }
                            else if (key == "main_monte_carlo_setting")
                            {
                                if (int.TryParse(value, out int_pass))
                                {
                                    int_pass = int.Parse(value);
                                }
                                ConfigSettings.main_monte_carlo_setting = int_pass;
                            }
                            else if (key == "proton_count")
                            {
                                if (int.TryParse(value, out int_pass))
                                {
                                    int_pass = int.Parse(value);
                                }
                                ConfigSettings.proton_count = int_pass;
                            }
                            else if (key == "secondary_monte_carlo_setting")
                            {
                                if (int.TryParse(value, out int_pass))
                                {
                                    int_pass = int.Parse(value);
                                }
                                ConfigSettings.secondary_monte_carlo_setting = int_pass;
                            }
                            else if (key == "unk7")
                            {
                                if (float.TryParse(value, out float_pass))
                                {
                                    float_pass = float.Parse(value);
                                }
                                ConfigSettings.unk7 = float_pass;
                            }
                        }
                    }
                }
            }
        }
    }
}
