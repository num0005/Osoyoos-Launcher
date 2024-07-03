using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Xml.Linq;
using ToolkitLauncher.Properties;
using ToolkitLauncher.ToolkitInterface;

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
        [Description("Red")]
        red,
        [Description("Blue")]
        blue,
        [Description("Green")]
        green,
        [Description("Purple")]
        purple,
        [Description("Rainbow")]
        rainbow
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

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum hr_quality_settings_stock
    {
        [Description("Direct Only")]
        direct_only,
        [Description("Draft")]
        draft,
        [Description("Low")]
        low,
        [Description("Medium")]
        medium,
        [Description("High")]
        high,
        [Description("Super")]
        super_slow,
        [Description("Checkerboard")]
        checkerboard,
        [Description("Special V1")]
        special_v1,
        [Description("Special Weekend")]
        special_weekend,
    }

    public enum h2_sound_import_type
    {
        projectile_impact,
        projectile_detonation,
        projectile_flyby,
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
        weapon_fire_lod,
        weapon_fire_lod_far,
        lfe,
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
        reflection,
        reflection_lod,
        reflection_lod_far,
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

    public enum h3_sound_import_type
    {
        projectile_impact,
        projectile_detonation,
        projectile_flyby,
        projectile_detonation_lod,
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
        weapon_fire_lod,
        unit_footsteps,
        unit_dialog,
        unit_animation,
        vehicle_collision,
        vehicle_engine,
        vehicle_animation,
        vehicle_engine_lod,
        device_door,
        device_machinery,
        device_stationary,
        music,
        ambient_nature,
        ambient_machinery,
        ambient_stationary,
        huge_ass,
        object_looping,
        cinematic_music,
        ambient_flock,
        no_pad,
        no_pad_stationary,
        cortana_mission,
        cortana_gravemind_channel,
        mission_dialog,
        cinematic_dialog,
        scripted_cinematic_foley,
        game_event,
        ui,
        test,
        multilingual_test
    }

    public enum odst_sound_import_type
    {
        projectile_impact,
        projectile_detonation,
        projectile_flyby,
        projectile_detonation_lod,
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
        weapon_fire_lod,
        unit_footsteps,
        unit_dialog,
        unit_animation,
        vehicle_collision,
        vehicle_engine,
        vehicle_animation,
        vehicle_engine_lod,
        device_door,
        device_machinery,
        device_stationary,
        music,
        ambient_nature,
        ambient_machinery,
        ambient_stationary,
        huge_ass,
        object_looping,
        cinematic_music,
        ambient_flock,
        no_pad,
        no_pad_stationary,
        arg,
        cortana_mission,
        cortana_gravemind_channel,
        mission_dialog,
        cinematic_dialog,
        scripted_cinematic_foley,
        game_event,
        ui,
        test,
        multilingual_test,
        ambient_nature_details,
        ambient_machinery_details,
        inside_surround_tail,
        outside_surround_tail,
        vehicle_detonation,
        ambient_detonation,
        first_person_inside,
        first_person_outside,
        first_person_anywhere,
        ui_pda,
    }

    public enum reach_sound_import_type
    {
        projectile_impact,
        projectile_detonation,
        projectile_flyby,
        projectile_detonation_lod,
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
        weapon_fire_lod,
        water_transitions,
        lowpass_effects,
        unit_footsteps,
        unit_dialog,
        unit_animation,
        vehicle_collision,
        vehicle_engine,
        vehicle_animation,
        vehicle_engine_lod,
        device_door,
        device_machinery,
        device_stationary,
        music,
        ambient_nature,
        ambient_machinery,
        ambient_stationary,
        huge_ass,
        object_looping,
        cinematic_music,
        ambient_flock,
        no_pad,
        no_pad_stationary,
        equipment_effect,
        mission_dialog,
        cinematic_dialog,
        scripted_cinematic_foley,
        game_event,
        ui,
        test,
        multilingual_test,
        ambient_nature_details,
        ambient_machinery_details,
        inside_surround_tail,
        outside_surround_tail,
        vehicle_detonation,
        ambient_detonation,
        first_person_inside,
        first_person_outside,
        first_person_anywhere,
        space_projectile_detonation,
        space_projectile_flyby,
        space_vehicle_engine,
        space_weapon_fire,
        player_voice_team,
        player_voice_proxy,
        projectile_impact_postpone,
        unit_footsteps_postpone,
        weapon_ready_third_person,
        ui_music
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum BitmapCompressionType
    {
        [Description("Default")]
        [EnumMember(Value = "Use Default (defined by usage)")]
        Default,

        [Description("DXT1 (Color-key Alpha)")]
        [EnumMember(Value = "DXT1 (Compressed Color + Color-Key Alpha)")]
        DXT1,
        [Description("DXT3 (4-bit alpha)")]
        [EnumMember(Value = "DXT3 (Compressed Color + 4-bit Alpha)")]
        DXT3,
        [Description("DXT5 (8-bit compressed alpha)")]
        [EnumMember(Value = "DXT5 (Compressed Color + Compressed 8-bit Alpha)")]
        DXT5,

        [Description("24-bit Color + 8-bit Alpha")]
        [EnumMember(Value = "24-bit Color + 8-bit Alpha")]
        Color24BitAlpha8Bit,
        [Description("Best Compressed Color")]
        [EnumMember(Value = "Best Compressed Color Format")]
        BestCompressedColor,
        [Description("Uncompressed")]
        [EnumMember(Value = "Best Uncompressed Color Format")]
        Uncompressed
    }

    // Return the associated EnumMember string value given the selected value of the compression type above
    public static class EnumExtensions
    {
        public static string GetEnumMemberValue(this Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            var attribute = (EnumMemberAttribute)fieldInfo.GetCustomAttributes(typeof(EnumMemberAttribute), false)[0];

            return attribute.Value;
        }
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

        string assetName;
        string default_path;
        string fullPath;
        string dataPath;
        string inputFileType = ".fbx";

        List<CheckBox> TagTypes = new List<CheckBox>();

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
                    return toolkit_profile.IsMCC;
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
                    return toolkit_profile.Generation == ToolkitProfiles.GameGen.Halo1;
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
                    return toolkit_profile.Generation == ToolkitProfiles.GameGen.Halo1 && !toolkit_profile.IsMCC;
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
                    return toolkit_profile.Generation == ToolkitProfiles.GameGen.Halo1 && toolkit_profile.IsMCC;
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
                    return toolkit_profile.Generation == ToolkitProfiles.GameGen.Halo2;
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
                    return toolkit_profile.Generation == ToolkitProfiles.GameGen.Halo2 && !toolkit_profile.IsMCC;
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
                    return toolkit_profile.Generation == ToolkitProfiles.GameGen.Halo2 && !toolkit_profile.CommunityTools && !toolkit_profile.IsMCC;
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
                    return toolkit_profile.Generation == ToolkitProfiles.GameGen.Halo2 && toolkit_profile.CommunityTools && !toolkit_profile.IsMCC;
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
                    return toolkit_profile.Generation == ToolkitProfiles.GameGen.Halo2 && toolkit_profile.IsMCC;
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
                    return toolkit_profile.Generation == ToolkitProfiles.GameGen.Halo3;
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
                    return toolkit_profile.IsODST;
                }
                return false;
            }
        }

        public static bool halo_reach
        {
            get
            {
                if (profile_mapping.Count > 0 && profile_index >= 0)
                {
                    return toolkit_profile.IsReach;
                }
                return false;
            }
        }

        public static bool halo_4
        {
            get
            {
                if (profile_mapping.Count > 0 && profile_index >= 0)
                {
                    return toolkit_profile.IsH4;
                }
                return false;
            }
        }

        public static int string_encoding_index { get; set; }
        public ToolkitBase Toolkit { get; internal set; }

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

            switch (profile.Generation)
            {
                case ToolkitProfiles.GameGen.Halo1:
                    toolkit = !profile.IsMCC ?
                        new H1Toolkit(profile, base_path, tool_paths) :
                        new H1AToolkit(profile, base_path, tool_paths);
                    break;
                case ToolkitProfiles.GameGen.Halo2:
                    toolkit = !profile.IsMCC ?
                        new H2Toolkit(profile, base_path, tool_paths) :
                        new H2AToolkit(profile, base_path, tool_paths);
                    break;
                case ToolkitProfiles.GameGen.Halo3:
                    toolkit = !profile.IsODST ?
                    toolkit = new H3Toolkit(profile, base_path, tool_paths) :
                    toolkit = new H3ODSTToolkit(profile, base_path, tool_paths);
                    break;
                case ToolkitProfiles.GameGen.Gen4:
                    toolkit = profile.IsReach ?
                    toolkit = new HRToolkit(profile, base_path, tool_paths) :
                    toolkit = new H4Toolkit(profile, base_path, tool_paths);
                    break;
                default:
                    Debug.Assert(false, "Profile has a game gen that isn't supported. Using the disabled toolkit");
                    toolkit = new DisabledToolkit(profile, base_path, tool_paths);
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
            int class_dropdown_index = class_dropdown.SelectedIndex;
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
                    Trace.WriteLine($"Profile '{current_profile.ProfileName}' has been disabled!");
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

            if (class_dropdown_index >= 0)
            {
                if (class_dropdown.Items.Count <= class_dropdown_index)
                {
                    class_dropdown.SelectedIndex = class_dropdown.Items.Count - 1;
                }
                else
                {
                    class_dropdown.SelectedIndex = class_dropdown_index;
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
            if (!halo_ce && !halo_2_standalone_stock && !halo_4)
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
            CompileLevel(compile_level_path.Text, bsp_path.Text, light_level, light_level_slider, Final.IsChecked is true, instance_count, phantom_hack.IsChecked is true, lightmap_group.Text, lightmapper_globals_path.Text);
        }

        private async void CompileLevel(string level_path, string bsp_path, string Level_Quality, float level_slider, bool radiosity_quality_toggle, int instance_count, bool phantom_fix, string lightmap_group, string lightmapper_globals)
        {
            if (levelCompileType.HasFlag(level_compile_type.compile))
            {
                bool is_release = true;
                StructureType structure_command = (StructureType)structure_import_type.SelectedIndex;

                var import_args = new ToolkitBase.ImportArgs(
                    import_check.IsChecked ?? false,
                    import_force.IsChecked ?? false,
                    import_verbose.IsChecked ?? false,
                    import_repro.IsChecked ?? false,
                    import_draft.IsChecked ?? false,
                    import_seam_debug.IsChecked ?? false,
                    import_skip_instances.IsChecked ?? false,
                    import_local.IsChecked ?? false,
                    import_farm_seams.IsChecked ?? false,
                    import_farm_bsp.IsChecked ?? false,
                    import_decompose_instances.IsChecked ?? false,
                    import_supress_errors_to_vrml.IsChecked ?? false
                    );

                await toolkit.ImportStructure(structure_command, level_path, phantom_fix, is_release, disable_asserts.IsChecked ?? false, struct_auto_fbx.IsChecked ?? false, import_args);
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
                    (OutputMode)output_mode.SelectedIndex,
                    lightmapper_globals
                    );

                var info = ToolkitBase.SplitStructureFilename(level_path, bsp_path, Path.GetDirectoryName(toolkit_profile.ToolPath));
                var scen_path = Path.Join(info.ScenarioPath, info.ScenarioName);
                CancelableProgressBarWindow<int> progress = null;
                if (lightmaps_args.instanceCount > 1 || halo_3 || halo_reach)
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
            BitmapCompressionType selectedComp = (BitmapCompressionType)bitmap_compression_type.SelectedItem; // Convert selected compression string to enum value
            await toolkit.ImportBitmaps(compile_image_path.Text, listEntry, selectedComp.GetEnumMemberValue(), bitmap_clear_old_settings.IsChecked is true, debug_plate.IsChecked is true);
        }

        private async void PackageLevel(object sender, RoutedEventArgs e)
        {
            // Halo 2 uses win64, win32, and xbox1 as platform options. Default if improper arg is given is win64
            string cache_platform = "win64";
            if (halo_3)
                cache_platform = "pc";

            CacheType cache_type_item = (CacheType)cache_type.SelectedIndex;
            ToolkitBase.ResourceMapUsage usage = (ToolkitBase.ResourceMapUsage)resource_map_usage.SelectedIndex;

            await toolkit.BuildCache(package_level_path.Text, cache_type_item, usage, log_tag_loads.IsChecked ?? false, cache_platform, cache_compress.IsChecked ?? false, cache_resource_sharing.IsChecked ?? false, cache_multilingual_sounds.IsChecked ?? false, cache_remastered_support.IsChecked ?? false, cache_mp_tag_sharing.IsChecked ?? false);
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
            string class_name = class_dropdown.SelectedItem.ToString();
            string custom_extension = bank_extension.Text.ToString();

            string sound_path = import_sound_path.Text;
            string bitrate_value = bitrate_slider.Value.ToString();
            string ltf_path = "data\\" + import_ltf_path.Text;

            await toolkit.ImportSound(sound_path, platform.ToString(), bitrate_value, ltf_path, sound_command.ToString(), class_name, ((ComboBoxItem)sound_compression_type.SelectedItem).Content.ToString().ToLower(), custom_extension);
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

        readonly FilePicker.Options XMLlevelOptions = FilePicker.Options.FileSelect(
            "Select your level",
            "map data|*.XML;*.scenario",
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
                else if (halo_reach)
                {
                    levelOptions = XMLlevelOptions;
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

        private void browse_bitmap_Click(object sender, RoutedEventArgs e)
        {
            const string gen1BitmapOptions = "*.tif";
            const string gen2BitmapOptions = "*.tif;*.tiff;*.tga;*.jpg;*.bmp";
            const string MCCBitmapOptions = "*.tif;*.tiff";
            const string H3BitmapOptions = "*.tif;*.tiff;*.dds";

            string bitmap_file_types = gen1BitmapOptions;
            if (halo_2_standalone)
            {
                bitmap_file_types = gen2BitmapOptions;
                if (halo_community)
                {
                    bitmap_file_types += ";*.png";
                }
            }
            else if (halo_3)
            {
                bitmap_file_types = H3BitmapOptions;
            }
            else if (halo_mcc)
            {
                bitmap_file_types = MCCBitmapOptions;
            }

            FilePicker.Options BitmapOptions = FilePicker.Options.FileSelect(
               "Select any source image file to choose folder",
               "Supported image files|" + bitmap_file_types,
               FilePicker.Options.PathRoot.Data,
               parent: true
            );

            bool tag_dir = false;
            bool is_file = false;
            string default_path = get_default_path(compile_image_path.Text, tag_dir, is_file);


            var picker = new FilePicker(compile_image_path, toolkit, BitmapOptions, default_path);
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

        readonly FilePicker.Options lightmapperGlobalsOptions = FilePicker.Options.FileSelect(
           "Select lightmapper globals",
           "lightmapper globals tag|*.lightmapper_globals",
           FilePicker.Options.PathRoot.Tag
        );

        private void browse_lightmapper_globals_Click(object sender, RoutedEventArgs e)
        {
            string default_path = get_default_path(compile_level_path.Text, tag_dir: true, is_file: true);
            var picker = new FilePicker(lightmapper_globals_path, toolkit, lightmapperGlobalsOptions, default_path);
            picker.Prompt();
        }

        private void toolkit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            light_quality_level.SelectedIndex = 0;
            class_dropdown.SelectedIndex = 0;
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
            IToolkitFBX2GR2 FBX2GR2 = toolkit as IToolkitFBX2GR2;

            (string ext, string fbxFileName, string outputFileName)? FBXArgs;

            switch (toolkit.Profile.Generation)
            {
                case ToolkitProfiles.GameGen.Halo1:
                    FBXArgs = PromptForFBXPaths("Select JMS save location", "Jointed model skeleton|*.JMS");
                    if (FBXArgs is not null)
                        await FBX2Jointed.JMSFromFBX(FBXArgs.Value.fbxFileName, FBXArgs.Value.outputFileName, null);
                    break;

                case ToolkitProfiles.GameGen.Halo2:
                case ToolkitProfiles.GameGen.Halo3:
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
                case ToolkitProfiles.GameGen.Gen4:
                    FBXArgs = PromptForFBXPaths("Select GR2 save location", "Granny3D 2|*.GR2");
                    if (FBXArgs is not null)
                    {
                        string jsonPath = Path.Join(Path.GetDirectoryName(FBXArgs.Value.fbxFileName), Path.GetFileNameWithoutExtension(FBXArgs.Value.fbxFileName) + ".json");
                        await FBX2GR2.GR2FromFBX(FBXArgs.Value.fbxFileName, jsonPath, FBXArgs.Value.outputFileName);
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
            IToolkitFBX2GR2 FBX2GR2 = toolkit as IToolkitFBX2GR2;

            (string ext, string fbxFileName, string outputFileName)? FBXArgs;

            switch (toolkit.Profile.Generation)
            {
                case ToolkitProfiles.GameGen.Halo1:
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

                case ToolkitProfiles.GameGen.Halo2:
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

                case ToolkitProfiles.GameGen.Halo3:
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

                case ToolkitProfiles.GameGen.Gen4:
                    FBXArgs = PromptForFBXPaths("Select GR2 save location", "Granny3D 2|*.GR2");
                    if (FBXArgs is not null)
                    {
                        string jsonPath = Path.GetFileNameWithoutExtension(FBXArgs.Value.fbxFileName) + ".json";
                        await FBX2GR2.GR2FromFBX(FBXArgs.Value.fbxFileName, jsonPath, FBXArgs.Value.outputFileName);
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

        private async void GenerateXML(object sender, RoutedEventArgs e)
        {
            TagTypes.Clear();
            TagTypes.Add(biped);
            TagTypes.Add(crate);
            TagTypes.Add(creature);
            TagTypes.Add(device_control);
            TagTypes.Add(device_machine);
            TagTypes.Add(device_terminal);
            TagTypes.Add(effect_scenery);
            TagTypes.Add(equipment);
            TagTypes.Add(giant);
            TagTypes.Add(scenery);
            TagTypes.Add(vehicle);
            TagTypes.Add(weapon);

            HRToolkit tool = toolkit as HRToolkit;

            if (generate_sidecar.IsChecked == true)
            {
                if (createDirectories.IsChecked == true)
                    CreateModelFolders();

                switch (asset_type.SelectedIndex)
                {
                    case 0:
                        bool error = true;

                        foreach (CheckBox c in TagTypes)
                        {
                            if (c.IsChecked == true)
                                error = false;
                        }

                        if (error)
                            MessageBox.Show("No output tags selected. Model sidecar generation aborted.");
                        else
                            GenerateModelSidecar();
                        break;
                    case 1:
                        GenerateStructureSidecar();
                        break;
                    case 2:
                        GenerateDecoratorSidecar();
                        break;
                    case 3:
                        GenerateParticleSidecar();
                        break;
                    case 4:
                        MessageBox.Show("Cinematics Not Currently Supported Yet!"); // Need more info about how cinematic sidecars work before this can be implemented
                        //GenerateCinematicSidecar();
                        break;
                }
            }
            else if (fbx_to_gr2.IsChecked == true)
                await ConvertFBX();

            else if (import_sidecar.IsChecked == true)
                await tool.ImportSidecar(dataPath + "\\" + assetName + ".sidecar.xml");

            else if (import_all.IsChecked == true)
            {
                if (createDirectories.IsChecked == true)
                    CreateModelFolders();

                await ConvertFBX();
                var processes = Process.GetProcessesByName("tool"); // waits for the fbx-to-gr2 windows to complete before moving onto sidecar generation.

                if (processes.Length > 0)
                {
                    foreach (var process in processes)
                    {
                        process.WaitForExit();
                    }
                }
                switch (asset_type.SelectedIndex)
                {
                    case 0:
                        GenerateModelSidecar();
                        break;
                    case 1:
                        GenerateStructureSidecar();
                        break;
                    case 2:
                        GenerateDecoratorSidecar();
                        break;
                    case 3:
                        GenerateParticleSidecar();
                        break;
                    case 4:
                        MessageBox.Show("Cinematics Not Currently Supporterd Yet!");
                        //GenerateCinematicSidecar();
                        break;
                }

                await tool.ImportSidecar(dataPath + "\\" + assetName + ".sidecar.xml");
            }
        }

        private async Task ConvertFBX()
        {
            HRToolkit tool = toolkit as HRToolkit;

            await tool.GR2FromFBXBatch(
                fullPath,
                (json_rebuild.IsChecked == true),
                (bool)show_output.IsChecked);
        }

        private void CreateModelFolders()
        {
            try
            {
                dataPath = xml_path.Text.ToLower();
                fullPath = default_path + "\\" + dataPath;

                if (!Directory.Exists(fullPath))
                {
                    DirectoryInfo newfolder = Directory.CreateDirectory(fullPath);
                }

                DirectoryInfo folder;

                string[] modelFolders = new string[] { "\\render", "\\physics", "\\markers", "\\skeleton", "\\collision" };
                string[] animationFolders = new string[] { "\\animations\\JMM", "\\animations\\JMA", "\\animations\\JMA", "\\animations\\JMT", "\\animations\\JMZ", "\\animations\\JMV", "\\animations\\JMO (Keyframe)", "\\animations\\JMO (Pose)", "\\animations\\JMR (Local)", "\\animations\\JMR (Object)" };
                string[] scenarioFolders = new string[] { "\\000\\structure", "\\000\\structure_design", "\\shared\\structure", "\\shared\\structure_design" };

                switch (asset_type.SelectedIndex)
                {
                    case 0:
                        foreach (var f in modelFolders)
                            folder = Directory.CreateDirectory(fullPath + f);

                        foreach (var f in animationFolders)
                            folder = Directory.CreateDirectory(fullPath + f);

                        break;
                    case 1:
                        foreach (var f in scenarioFolders)
                            folder = Directory.CreateDirectory(fullPath + f);
                        break;
                    case 2:
                        folder = Directory.CreateDirectory(fullPath + "\\render");
                        break;
                    case 3:
                        folder = Directory.CreateDirectory(fullPath + "\\render");
                        break;
                    case 4:
                        folder = Directory.CreateDirectory(fullPath + "\\cinematic");

                        foreach (var f in animationFolders)
                            folder = Directory.CreateDirectory(fullPath + f);
                        break;
                }

            }
            catch (Exception)
            {

            }
        }

        // Generate a Sidecar when the user has opted to create one for a model
        private void GenerateModelSidecar()
        {
            XDocument srcTree = new XDocument(
                new XElement("Metadata", WriteHeader(),
                    new XElement("Asset", new XAttribute("Name", assetName), new XAttribute("Type", "model"), GetOutputObjectTypes()),
                    WriteFolders(),
                    WriteFaceCollections(true, true), // first bool is whether the asset type has regions, the second is if it has global materials
                    new XElement("Contents", GetModelContentObjects()
                        )
                    )
                );

            srcTree.Save(fullPath + "\\" + assetName + ".sidecar.xml", SaveOptions.None);

            if (import_all.IsChecked != true)
                MessageBox.Show("Sidecar Generated Successfully!");
        }

        private void GenerateStructureSidecar()
        {
            XDocument srcTree = new XDocument(
                new XElement("Metadata", WriteHeader(),
                    new XElement("Asset", new XAttribute("Name", assetName), new XAttribute("Type", "scenario"),
                        new XElement("OutputTagCollection",
                            new XElement("OutputTag", new XAttribute("Type", "scenario_lightmap"), dataPath + "\\" + assetName + "_faux_lightmap"),
                            new XElement("OutputTag", new XAttribute("Type", "structure_seams"), dataPath + "\\" + assetName),
                            new XElement("OutputTag", new XAttribute("Type", "scenario"), dataPath + "\\" + assetName)
                            )),
                    WriteFolders(),
                    WriteFaceCollections(false, true), // first bool is whether the asset type has regions, the second is if it has global materials
                    GetStructureContentObjects()
                    )
                );

            srcTree.Save(fullPath + "\\" + assetName + ".sidecar.xml", SaveOptions.None);

            if (import_all.IsChecked != true)
                MessageBox.Show("Sidecar Generated Successfully!");
        }

        private void GenerateDecoratorSidecar()
        {
            XDocument srcTree = new XDocument(
                new XElement("Metadata", WriteHeader(),
                    new XElement("Asset", new XAttribute("Name", assetName), new XAttribute("Type", "decorator_set"),
                        new XElement("OutputTagCollection",
                            new XElement("OutputTag", new XAttribute("Type", "decorator_set"), dataPath + "\\" + assetName)
                            )),
                    WriteFolders(),
                    WriteFaceCollections(false, false), // first bool is whether the asset type has regions, the second is if it has global materials
                    new XElement("Contents", GetDecoratorContentObjects())
                    )
                );

            srcTree.Save(fullPath + "\\" + assetName + ".sidecar.xml", SaveOptions.None);

            if (import_all.IsChecked != true)
                MessageBox.Show("Sidecar Generated Successfully!");
        }

        private void GenerateParticleSidecar()
        {
            XDocument srcTree = new XDocument(
                new XElement("Metadata", WriteHeader(),
                    new XElement("Asset", new XAttribute("Name", assetName), new XAttribute("Type", "particle_model"),
                        new XElement("OutputTagCollection",
                            new XElement("OutputTag", new XAttribute("Type", "particle_model"), dataPath + "\\" + assetName)
                            )),
                    WriteFolders(),
                    WriteFaceCollections(false, false), // first bool is whether the asset type has regions, the second is if it has global materials
                    new XElement("Contents", GetParticleContentObjects())
                    )
                );

            srcTree.Save(fullPath + "\\" + assetName + ".sidecar.xml", SaveOptions.None);

            if (import_all.IsChecked != true)
                MessageBox.Show("Sidecar Generated Successfully!");
        }

        private void GenerateCinematicSidecar()
        {
            XDocument srcTree = new XDocument(
                new XElement("Metadata", WriteHeader(),
                    new XElement("Asset", new XAttribute("Name", assetName), new XAttribute("Type", "cinematic"),
                        new XElement("OutputTagCollection",
                            new XElement("OutputTag", new XAttribute("Type", "cinematic"), dataPath + "\\" + assetName)
                            )),
                    WriteFolders(),
                    WriteFaceCollections(false, false), // first bool is whether the asset type has regions, the second is if it has global materials
                    new XElement("Contents", GetCinematicContentObjects())
                    )
                );

            srcTree.Save(fullPath + "\\" + assetName + ".sidecar.xml", SaveOptions.None);

            if (import_all.IsChecked != true)
                MessageBox.Show("Sidecar Generated Successfully!");
        }

        private XElement GetCinematicContentObjects()
        {
            string[] cFiles = Directory.EnumerateFiles(fullPath + "\\cinematics", "*.gr2").ToArray();

            foreach (var f in cFiles)
            {
                XElement c1 = new XElement("Content", new XAttribute("Name", assetName), new XAttribute("Type", "scene"),
                    new XElement("ContentObject", new XAttribute("Name", ""), new XAttribute("Type", "cinematic_audio"),
                        new XElement("OutputTagCollection")),
                    new XElement("ContentObject", new XAttribute("Name", ""), new XAttribute("Type", "cinematic_scene"),
                        new XElement("ContentNetwork", new XAttribute("Name", ""), new XAttribute("Type", ""),
                            new XElement("InputFile", dataPath + "\\cinematics\\" + getFileNames(f)[1] + inputFileType),
                            new XElement("IntermediateFile", dataPath + "\\cinematics\\" + getFileNames(f)[0])),
                        new XElement("ContentNetwork", new XAttribute("Name", "environment"), new XAttribute("Type", ""),
                            new XElement("InputFile", dataPath + "\\environment\\" + getFileNames(f)[1] + inputFileType)),
                        new XElement("OutputTagCollection",
                            new XElement("OutputTag", new XAttribute("Type", "cinematic_scene"), dataPath + "\\" + assetName))
                    ));
            }

            return null;
        }

        private XElement GetParticleContentObjects()
        {
            if (IntermediateFileExists("render"))
            {
                string[] dFiles = Directory.EnumerateFiles(fullPath + "\\render", "*.gr2").ToArray();

                XElement c1 = new XElement("Content", new XAttribute("Name", assetName), new XAttribute("Type", "particle_model"));

                XElement co = new XElement("ContentObject", new XAttribute("Name", ""), new XAttribute("Type", "particle_model"));
                XElement ce = null;

                foreach (var f in dFiles)
                {
                    ce = new XElement("ContentNetwork", new XAttribute("Name", assetName), new XAttribute("Type", ""),
                        new XElement("InputFile", dataPath + "\\render\\" + getFileNames(f)[1] + inputFileType),
                        new XElement("IntermediateFile", dataPath + "\\render\\" + getFileNames(f)[0]));
                    co.Add(ce);
                }
                XElement ot = new XElement("OutputTagCollection");

                co.Add(ot);
                c1.Add(co);

                return c1;
            }
            else
                return null;
        }

        private XElement GetDecoratorContentObjects()
        {
            if (IntermediateFileExists("render"))
            {
                string[] dFiles = Directory.EnumerateFiles(fullPath + "\\render", "*.gr2").ToArray();

                XElement c1 = new XElement("Content", new XAttribute("Name", assetName), new XAttribute("Type", "decorator_set"));

                int count = 0;

                while (count < 4) // Using a count of 4 here as Decorator Set sidecars support up to 4 LOD Content Networks. So this just lets us loop through the code below 4 times to support each LOD.
                {
                    XElement co = new XElement("ContentObject", new XAttribute("Name", count.ToString()), new XAttribute("Type", "render_model"), new XAttribute("LOD", count.ToString()));
                    XElement ce = null;

                    foreach (var f in dFiles)
                    {
                        ce = new XElement("ContentNetwork", new XAttribute("Name", "default"), new XAttribute("Type", ""),
                            new XElement("InputFile", dataPath + "\\render\\" + getFileNames(f)[1] + inputFileType),
                            new XElement("IntermediateFile", dataPath + "\\render\\" + getFileNames(f)[0]));
                        co.Add(ce);
                    }
                    XElement ot = new XElement("OutputTagCollection",
                        new XElement("OutputTag", new XAttribute("Type", "render_model"), dataPath + "\\" + assetName + "_lod" + count.ToString()));

                    co.Add(ot);
                    c1.Add(co);

                    count++;
                }

                return c1;
            }
            else
                return null;
        }

        private List<string> getFileNames(string file)
        {
            string[] t = file.Split("\\");
            string filename = t.Last();
            t = filename.Split(".");
            string input = "";
            for (int i = 0; i < t.Length - 1; i++)
                input += t[i];

            List<string> results = new List<string>();
            results.Add(filename);
            results.Add(input);
            return results;
        }

        private XElement GetStructureContentObjects()
        {
            List<XElement> temp = new List<XElement>();
            List<XElement> sharedStructure = new List<XElement>();
            List<XElement> sharedDesign = new List<XElement>();

            var directories = Directory.GetDirectories(fullPath);

            if (IntermediateFileExists("\\shared\\structure"))
                sharedStructure = GetSharedStructureList("\\shared\\structure");

            if (IntermediateFileExists("\\shared\\structure_design"))
                sharedDesign = GetSharedStructureList("\\shared\\structure_design");

            foreach (string s in directories)
            {
                string[] path = s.Split("\\");
                string folderName = path.Last();

                if (folderName != "shared")
                {
                    List<XElement> sList = GenerateStructureContent("_bsp", "bsp", "\\structure", "scenario_structure_bsp", "scenario_structure_lighting_info", folderName, sharedStructure);
                    sList.ForEach(item => temp.Add(item));

                    sList = GenerateStructureContent("_structure_design", "design", "\\structure_design", "structure_design", "", folderName, sharedDesign);
                    sList.ForEach(item => temp.Add(item));
                }
            }

            XElement StructureObjects = new XElement("Contents");
            foreach (XElement e in temp)
                StructureObjects.Add(e);

            return StructureObjects;
        }

        private List<XElement> GetSharedStructureList(string type)
        {
            List<XElement> temp = new List<XElement>();
            string[] sharedFiles = Directory.EnumerateFiles(fullPath + type, "*.gr2").ToArray();
            foreach (var sf in sharedFiles)
            {
                XElement sf1 = new XElement("ContentNetwork", new XAttribute("Name", getFileNames(sf)[1]), new XAttribute("Type", ""),
                                new XElement("InputFile", dataPath + type + "\\" + getFileNames(sf)[1] + inputFileType),
                                new XElement("IntermediateFile", dataPath + type + "\\" + getFileNames(sf)[0])
                                );
                temp.Add(sf1);
            }
            return temp;
        }

        private List<XElement> GenerateStructureContent(string type1, string type2, string type3, string type4, string type5, string folderName, List<XElement> sharedList)
        {
            List<XElement> temp = new List<XElement>();
            XElement e1 = new XElement("Content", new XAttribute("Name", assetName + "_" + folderName + type1), new XAttribute("Type", type2));
            XElement co = new XElement("ContentObject", new XAttribute("Name", ""), new XAttribute("Type", type4));

            if (IntermediateFileExists(folderName + type3))
            {
                string[] files = Directory.EnumerateFiles(fullPath + "\\" + folderName + type3, "*.gr2").ToArray();

                foreach (var f in files)
                {
                    XElement e2 = new XElement("ContentNetwork", new XAttribute("Name", getFileNames(f)[1]), new XAttribute("Type", ""),
                            new XElement("InputFile", dataPath + "\\" + folderName + type3 + "\\" + getFileNames(f)[1] + inputFileType),
                            new XElement("IntermediateFile", dataPath + "\\" + folderName + type3 + "\\" + getFileNames(f)[0])
                            );
                    co.Add(e2);
                }

                foreach (var st in sharedList)
                    co.Add(st);

                if (type5 == "")
                {
                    XElement e3 = new XElement("OutputTagCollection",
                        new XElement("OutputTag", new XAttribute("Type", type4), dataPath + "\\" + assetName + "_" + folderName + type1));
                    co.Add(e3);
                    e1.Add(co);
                    temp.Add(e1);
                }
                else
                {
                    XElement e3 = new XElement("OutputTagCollection",
                        new XElement("OutputTag", new XAttribute("Type", type4), dataPath + "\\" + assetName + "_" + folderName + type1),
                        new XElement("OutputTag", new XAttribute("Type", type5), dataPath + "\\" + assetName + "_" + folderName + type1));
                    co.Add(e3);
                    e1.Add(co);
                    temp.Add(e1);
                }
            }
            return temp;
        }

        private XElement WriteHeader()
        {
            XElement header;

            string version = Assembly.GetEntryAssembly()
                .GetCustomAttribute<AssemblyFileVersionAttribute>().Version;

            header = new XElement("Header",
                        new XElement("MainRev", "0"),
                        new XElement("PointRev", "6"),
                        new XElement("Description", $"Created By Osoyoos SideCar Gen v1.0 ({version})"),
                        new XElement("Created", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")), // Long live ISO 8601!
                        new XElement("By", Environment.UserName),
                        new XElement("DirectoryType", "TAE.Shared.NWOAssetDirectory"),
                        new XElement("Schema", "1"));

            return header;
        }

        private XElement WriteFolders()
        {
            XElement folders;

            folders = new XElement("Folders",
                        new XElement("Reference", "\\reference"),
                        new XElement("Temp", "\\temp"),
                        new XElement("SourceModels", "\\work"),
                        new XElement("GameModels", "\\render"),
                        new XElement("GamePhysicsModels", "\\physics"),
                        new XElement("GameCollisionModels", "\\collision"),
                        new XElement("ExportModels", "\\render"),
                        new XElement("ExportPhysicsModels", "\\physics"),
                        new XElement("ExportCollisionModels", "\\collision"),
                        new XElement("SourceAnimations", "\\animations\\work"),
                        new XElement("AnimationRigs", "\\animations\\rigs"),
                        new XElement("GameAnimations", "\\animations"),
                        new XElement("ExportAnimations", "\\animations"),
                        new XElement("SourceBitmaps", "\\bitmaps"),
                        new XElement("GameBitmaps", "\\bitmaps"),
                        new XElement("CinemaSource", "\\cinematics"),
                        new XElement("CinemaExport", "\\cinematics"),
                        new XElement("ExportBSPs", "\\"),
                        new XElement("SourceBSPs", "\\"),
                        new XElement("Scripts", "\\scripts"));

            return folders;
        }

        private XElement WriteFaceCollections(bool regions, bool materials)
        {
            bool newfaceCollection = false;

            if (File.Exists(fullPath + "\\" + assetName + ".sidecar.xml"))
            {
                try
                {
                    XDocument sidecar = XDocument.Load(fullPath + "\\" + assetName + ".sidecar.xml");
                    XElement faceCollections = new XElement("FaceCollections");

                    foreach (XElement fc in sidecar.Root.Descendants("FaceCollections").Descendants("FaceCollection"))
                        faceCollections.Add(fc);

                    return faceCollections;
                }
                catch (Exception)
                {
                    newfaceCollection = true;

                    MessageBox.Show("Couldn't parse " + assetName + ".sidecar.xml. Rebuilding Sidecar and resetting FaceCollections");
                }
            }
            else
                newfaceCollection = true;

            if (newfaceCollection)
            {
                if (regions || materials)
                {
                    XElement faceCollections = new XElement("FaceCollections");

                    if (regions)
                    {
                        XElement f1 = new XElement("FaceCollection", new XAttribute("Name", "regions"), new XAttribute("StringTable", "connected_geometry_regions_table"), new XAttribute("Description", "Model regions"),
                            new XElement("FaceCollectionEntries", new XElement("FaceCollectionEntry", new XAttribute("Index", "0"), new XAttribute("Name", "default"), new XAttribute("Active", "true"))));
                        faceCollections.Add(f1);
                    }

                    if (materials)
                    {
                        XElement f2 = new XElement("FaceCollection", new XAttribute("Name", "global materials override"), new XAttribute("StringTable", "connected_geometry_global_material_table"), new XAttribute("Description", "Global material overrides"),
                            new XElement("FaceCollectionEntries", new XElement("FaceCollectionEntry", new XAttribute("Index", "0"), new XAttribute("Name", "default"), new XAttribute("Active", "true"))));
                        faceCollections.Add(f2);
                    }

                    return faceCollections;
                }
                else
                    return null;
            }
            return null;
        }

        private XElement GetOutputObjectTypes()
        {
            List<XElement> temp = new List<XElement>();

            foreach (CheckBox c in TagTypes)
            {
                if (c.IsChecked == true)
                {
                    XElement e = new XElement("OutputTag", new XAttribute("Type", c.Name), dataPath + "\\" + assetName);
                    temp.Add(e);
                }
            }

            XElement OutputTags = new XElement("OutputTagCollection");
            OutputTags.Add(new XElement("OutputTag", new XAttribute("Type", "model"), dataPath + "\\" + assetName));
            foreach (XElement e in temp)
                OutputTags.Add(e);

            return OutputTags;
        }

        // check if any files exist before we create a content object for each model type
        private XElement GetModelContentObjects()
        {
            List<XElement> temp = new List<XElement>();

            if (IntermediateFileExists("render"))
                temp.Add(CreateContentObject("render"));

            if (IntermediateFileExists("physics"))
                temp.Add(CreateContentObject("physics"));

            if (IntermediateFileExists("collision"))
                temp.Add(CreateContentObject("collision"));

            if (IntermediateFileExists("markers"))
                temp.Add(CreateContentObject("markers"));

            if (IntermediateFileExists("skeleton"))
                temp.Add(CreateContentObject("skeleton"));

            if (IntermediateFileExists("animations\\JMM") || IntermediateFileExists("animations\\JMA") || IntermediateFileExists("animations\\JMT") || IntermediateFileExists("animations\\JMZ") || IntermediateFileExists("animations\\JMV")
                || IntermediateFileExists("animations\\JMO (Keyframe)") || IntermediateFileExists("animations\\JMO (Pose)") || IntermediateFileExists("animations\\JMR (Object)") || IntermediateFileExists("animations\\JMR (Local)"))
            {
                XElement animations = new XElement("ContentObject", new XAttribute("Name", ""), new XAttribute("Type", "model_animation_graph"));

                if (IntermediateFileExists("animations\\JMM"))
                    animations.Add(CreateContentObject("animations\\JMM", "Base", "ModelAnimationMovementData", "None", "", ""));

                if (IntermediateFileExists("animations\\JMA"))
                    animations.Add(CreateContentObject("animations\\JMA", "Base", "ModelAnimationMovementData", "XY", "", ""));

                if (IntermediateFileExists("animations\\JMT"))
                    animations.Add(CreateContentObject("animations\\JMT", "Base", "ModelAnimationMovementData", "XYYaw", "", ""));

                if (IntermediateFileExists("animations\\JMZ"))
                    animations.Add(CreateContentObject("animations\\JMZ", "Base", "ModelAnimationMovementData", "XYZYaw", "", ""));

                if (IntermediateFileExists("animations\\JMV"))
                    animations.Add(CreateContentObject("animations\\JMV", "Base", "ModelAnimationMovementData", "XYZFullRotation", "", ""));

                if (IntermediateFileExists("animations\\JMO (Keyframe)"))
                    animations.Add(CreateContentObject("animations\\JMO (Keyframe)", "Overlay", "ModelAnimationOverlayType", "Keyframe", "ModelAnimationOverlayBlending", "Additive"));

                if (IntermediateFileExists("animations\\JMO (Pose)"))
                    animations.Add(CreateContentObject("animations\\JMO (Pose)", "Overlay", "ModelAnimationOverlayType", "Pose", "ModelAnimationOverlayBlending", "Additive"));

                if (IntermediateFileExists("animations\\JMR (Local)"))
                    animations.Add(CreateContentObject("animations\\JMR (Local)", "Overlay", "ModelAnimationOverlayType", "keyframe", "ModelAnimationOverlayBlending", "ReplacementLocalSpace"));

                if (IntermediateFileExists("animations\\JMR (Object)"))
                    animations.Add(CreateContentObject("animations\\JMR (Object)", "Overlay", "ModelAnimationOverlayType", "keyframe", "ModelAnimationOverlayBlending", "ReplacementObjectSpace"));

                XElement r2 = new XElement("OutputTagCollection",
                        new XElement("OutputTag", new XAttribute("Type", "frame_event_list"), dataPath + "\\" + assetName),
                        new XElement("OutputTag", new XAttribute("Type", "model_animation_graph"), dataPath + "\\" + assetName));
                animations.Add(r2);
                temp.Add(animations);
            }

            XElement ContentObjects = new XElement("Content", new XAttribute("Name", assetName), new XAttribute("Type", "model"));
            foreach (XElement e in temp)
                ContentObjects.Add(e);

            return ContentObjects;
        }

        private XElement CreateContentObject(string type)
        {
            string[] files = Directory.EnumerateFiles(fullPath + "\\" + type, "*.gr2").ToArray();

            XElement content;

            if (type == "markers" || type == "skeleton")
                content = new XElement("ContentObject", new XAttribute("Name", ""), new XAttribute("Type", type));
            else
                content = new XElement("ContentObject", new XAttribute("Name", ""), new XAttribute("Type", type + "_model"));

            foreach (var f in files)
            {
                XElement r1 = new XElement("ContentNetwork", new XAttribute("Name", getFileNames(f)[1]), new XAttribute("Type", ""),
                    new XElement("InputFile", dataPath + "\\" + type + "\\" + getFileNames(f)[1] + inputFileType),
                    new XElement("IntermediateFile", dataPath + "\\" + type + "\\" + getFileNames(f)[0])
                );
                content.Add(r1);
            }

            XElement r2;
            if (type == "markers" || type == "skeleton")
                r2 = new XElement("OutputTagCollection");
            else
                r2 = new XElement("OutputTagCollection",
                    new XElement("OutputTag", new XAttribute("Type", type + "_model"), dataPath + "\\" + assetName));

            content.Add(r2);
            return content;
        }

        private List<XElement> CreateContentObject(string type1, string type2, string type3, string type4, string type5, string type6)
        {
            string[] files = Directory.EnumerateFiles(fullPath + "\\" + type1, "*.gr2").ToArray();

            List<XElement> content = new List<XElement>();

            XElement r1;
            foreach (var f in files)
            {
                if (type5 == "" || type6 == "")
                {
                    r1 = new XElement("ContentNetwork", new XAttribute("Name", getFileNames(f)[1]), new XAttribute("Type", type2), new XAttribute(type3, type4),
                    new XElement("InputFile", dataPath + "\\" + type1 + "\\" + getFileNames(f)[1] + inputFileType),
                    new XElement("IntermediateFile", dataPath + "\\" + type1 + "\\" + getFileNames(f)[0]));
                }
                else
                {
                    r1 = new XElement("ContentNetwork", new XAttribute("Name", getFileNames(f)[1]), new XAttribute("Type", type2), new XAttribute(type3, type4), new XAttribute(type5, type6),
                    new XElement("InputFile", dataPath + "\\" + type1 + "\\" + getFileNames(f)[1] + inputFileType),
                    new XElement("IntermediateFile", dataPath + "\\" + type1 + "\\" + getFileNames(f)[0]));
                }
                content.Add(r1);
            }

            return content;
        }

        private bool IntermediateFileExists(string folderName)
        {
            try
            {
                return Directory.GetFiles(fullPath + "\\" + folderName, "*.gr2").Length > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //private string get_default_path_sidecar(string textbox_string, bool tag_dir, bool is_file)
        //{
        //    string base_path = toolkit.GetDataDirectory();
        //string local_path = "";

        //fullPath = base_path;

        //    if (tag_dir is true)
        //        base_path = toolkit.GetTagDirectory();

        //    if (!string.IsNullOrWhiteSpace(textbox_string))
        //    {
        //        if (is_file == true)
        //            local_path = Path.GetDirectoryName(textbox_string);
        //        else
        //            local_path = textbox_string;
        //    }

        //    if (Directory.Exists(Path.Join(base_path, local_path)))
        //        return Path.Join(base_path, local_path);
        //    return base_path;
        //}

readonly FilePicker.Options xmlOptions = FilePicker.Options.FolderSelect(
   "Select your folder with your sidecar files",
   FilePicker.Options.PathRoot.Data
);

        private void browse_path_Click(object sender, RoutedEventArgs e)
        {
            xml_path.Text = "";
            bool tag_dir = false;
            bool is_file = false;
            string default_path = get_default_path(xml_path.Text.ToLower(), tag_dir, is_file);
            var picker = new FilePicker(xml_path, toolkit, xmlOptions, default_path);
            picker.Prompt();

            try
            {
                dataPath = xml_path.Text.ToLower();
                string[] tmp = dataPath.Split('\\');
                assetName = tmp.Last();
                fullPath = "";
                fullPath = default_path + "\\" + dataPath;
                textBlock.Visibility = Visibility.Hidden;
            }
            catch (Exception)
            {

            }
        }

        private void asset_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (asset_type.SelectedIndex > 0)
            {
                foreach (CheckBox c in TagTypes)
                    c.IsEnabled = false;
            }
            else if (asset_type.SelectedIndex == 0)
            {
                try
                {
                    foreach (CheckBox c in TagTypes)
                        c.IsEnabled = true;
                }
                catch (Exception)
                {

                }
            }
        }

        private void radioSelectionChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (import_sidecar.IsChecked == true || fbx_to_gr2.IsChecked == true)
                {
                    foreach (CheckBox c in TagTypes)
                        c.IsEnabled = false;

                    asset_type.IsEnabled = false;
                    createDirectories.IsEnabled = false;
                }
                else
                {
                    try
                    {
                        foreach (CheckBox c in TagTypes)
                            c.IsEnabled = true;

                        asset_type.IsEnabled = true;
                        createDirectories.IsEnabled = true;
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private async void convert_model_from_fbx_to_gr2_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "", // Default file name
                DefaultExt = ".fbx", // Default file extension
                Filter = "FBX files (.fbx)|*.fbx", // Filter files by extension
                Multiselect = true
            };

            // Show open file dialog box
            bool? result = dialog.ShowDialog();
            List<Task> dispatchedTasks = new();

            // Process open file dialog box results
            if (result == true)
            {
                HRToolkit tool = toolkit as HRToolkit;

                foreach (var filename in dialog.FileNames)
                {
                    Task task = tool.GR2FromFBX(
                        filename, 
                        Path.ChangeExtension(filename, ".json"),
                        Path.ChangeExtension(filename, ".gr2"), 
                        (json_rebuild.IsChecked == true), 
                        (bool)show_output.IsChecked);
                    dispatchedTasks.Add(task);
                }
            }

            await Task.WhenAll(dispatchedTasks);
        }

        private void xml_path_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool tag_dir = false;
            bool is_file = false;
            default_path = get_default_path("", tag_dir, is_file);

            try
            {
                dataPath = xml_path.Text.ToLower();
                fullPath = "";
                fullPath = default_path + "\\" + dataPath;
                textBlockImport.Visibility = Visibility.Hidden;
                if (xml_path.Text == "" || xml_path.Text == " ")
                    textBlockImport.Visibility = Visibility.Visible;
                string[] tmp = dataPath.Split('\\');
                if (tmp.Length > 1)
                    assetName = tmp.Last().ToString();
                else
                    assetName = dataPath;
            }
            catch (Exception)
            {

            }
        }
    }
}
