using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using ToolkitLauncher.Properties;
using static ToolkitLauncher.BindingToolkitParser;

namespace ToolkitLauncher
{

    /// <summary>
    /// Helper class for implementing one way converters
    /// </summary>
    public abstract class OneWayValueConverter : IValueConverter
    {
        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("One way converter!");
        }
    }

    /// <summary>
    /// Helper class for implementing one way converters
    /// </summary>
    public abstract class OneWayMultiValueConverter : IMultiValueConverter
    {
        public abstract object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("One way converter!");
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

    public class TextInputToVisibilityConverter : OneWayMultiValueConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Always test MultiValueConverter inputs for non-null
            // (to avoid crash bugs for views in the designer)
            if (values[0] is bool hasText && values[1] is bool hasFocus)
            {
                if (!hasText || hasFocus)
                    return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }
    }

    internal class BindingToolkitParser
    {
        [Flags]
        public enum TogglesUI
        {
            None = 0,

            // game type
            H1 = 1 << 0,
            H2 = 1 << 2,
            H3 = 1 << 3,
            HR = 1 << 4,
            H4 = 1 << 5,

            // what sort of toolkit is this?
            MCC = 1 << 6,
            H2Codez = 1 << 7,
            CommunityBuild = 1 << 8,
            LegacyStock = 1 << 9,
            ODST = 1 << 10,

            // what tools do we have?
            HasTool = 1 << 11,
            HasSapien = 1 << 12,
            HasGuerilla = 1 << 13,
            HasStandalone = 1 << 14,

        }

        static public TogglesUI ParseFlagSet(string input)
        {
            TogglesUI flags = TogglesUI.None;

            foreach (string element  in input.Split("|")) 
            {
                if (String.IsNullOrEmpty(element)) continue;

                TogglesUI value = Enum.Parse<TogglesUI>(element.Trim(), true);
                flags |= value;
            }

            return flags;
        }

        static public List<TogglesUI> ParseMultiFlagSet(string input)
        {
            List<TogglesUI> flagSets = new();

            foreach (string element in input.Split("+"))
            {
                TogglesUI flags = ParseFlagSet(element.Trim());
                flagSets.Add(flags);
            }

            return flagSets;
        }

        static public List<string> ParseBindingList(string input)
        {
            Debug.Assert(input != null);
            Debug.Assert(input.StartsWith("("));
            Debug.Assert(input.EndsWith(")"));

            input = input[1..^1];

            List<string> elements = new ();
            foreach (string element in input.Split(","))
            {
                elements.Add(element.Trim());
            }

            return elements;
        }

        static public bool IsAnyInEnableListValid(IEnumerable<TogglesUI> enable_for)
        {
            bool enable;
            if (MainWindow.toolkit_profile is not null)
            {
                enable = false;

                foreach (TogglesUI toggle in enable_for)
                {
                    if (toggle.HasFlag(TogglesUI.H1) && !MainWindow.halo_ce)
                        continue;

                    if (toggle.HasFlag(TogglesUI.H2) && !MainWindow.halo_2)
                        continue;

                    if (toggle.HasFlag(TogglesUI.H3) && !MainWindow.halo_3)
                        continue;

                    if (toggle.HasFlag(TogglesUI.HR) && !MainWindow.halo_reach)
                        continue;

                    if (toggle.HasFlag(TogglesUI.H4) && !MainWindow.halo_4)
                        continue;



                    if (toggle.HasFlag(TogglesUI.CommunityBuild) && !MainWindow.halo_community)
                        continue;

                    if (toggle.HasFlag(TogglesUI.LegacyStock) && (MainWindow.halo_mcc || MainWindow.halo_community))
                        continue;

                    if (toggle.HasFlag(TogglesUI.MCC) && !MainWindow.halo_mcc)
                        continue;

                    if (toggle.HasFlag(TogglesUI.HasTool) && string.IsNullOrEmpty(MainWindow.toolkit_profile.ToolPath))
                        continue;

                    if (toggle.HasFlag(TogglesUI.HasGuerilla) && string.IsNullOrEmpty(MainWindow.toolkit_profile.GuerillaPath))
                        continue;

                    if (toggle.HasFlag(TogglesUI.HasSapien) && string.IsNullOrEmpty(MainWindow.toolkit_profile.SapienPath))
                        continue;

                    if (toggle.HasFlag(TogglesUI.HasStandalone) && string.IsNullOrEmpty(MainWindow.toolkit_profile.GameExePath))
                        continue;

                    // check extension support
                    if (toggle.HasFlag(TogglesUI.H2Codez) && !MainWindow.toolkit_profile.IsH2Codez())
                        continue;

                    // everything checks out, exit early
                    enable = true;
                    break;
                }
            }
            else
            {
                //Either we're in desinger or there are no profiles. Reveal ourselves either way.
                enable = true;
            }

            return enable;
        }
    }

    public class BooleanToSpaceConverter : OneWayValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is true)
                return new GridLength(8);
            else
                return new GridLength(0);
        }
    }

    public class ForceBooleanConverter : OneWayValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool boolean_value = value is true;

            if (parameter is string param)
            {
                if ("reverse" == param)
                    boolean_value = !boolean_value;
            }

            return boolean_value;
        }
    }

    public class ToolkitToSpaceConverter : OneWayValueConverter
    {
        private GridLength ParseGridConfig(string input)
        {
            Debug.Assert(input != null);
            input = input.Trim();
            if (input == "star")
                return new GridLength(1, GridUnitType.Star);
            else if (input == "auto")
                return new GridLength(1, GridUnitType.Auto);
            else
            {
                _ = int.TryParse(input, out int intger_size);

                return new GridLength(intger_size);
            }
        }
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (parameter == null)
                return new GridLength(0);

            List<string> config = BindingToolkitParser.ParseBindingList(parameter as string);
            List<TogglesUI> enable_for = BindingToolkitParser.ParseMultiFlagSet(config[0]);

            bool enable = IsAnyInEnableListValid(enable_for);

            if (enable)
            {
                return ParseGridConfig(config[1]);
            }
            else
            {
                return ParseGridConfig(config[2]);
            }
        }
    }

    public class TextContentModifier : OneWayMultiValueConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int string_encoding = (int)values[0];
            int toolkit_selection = (int)values[1];
            string text_string = "Select a folder with an .hmt file to import.";
            if (MainWindow.toolkit_profile != null && string_encoding >= 0 && toolkit_selection >= 0)
            {
                //Not sure what to do here. Crashes designer otherwise cause the list or value is empty
                if (MainWindow.toolkit_profile.Generation >= ToolkitProfiles.GameGen.Halo2 || string_encoding == 1)
                    text_string = "Select a folder with .txt files to import.";
            }
            return text_string;
        }
    }

    public class ProfiletoVisibility : OneWayValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<TogglesUI> enable_for = BindingToolkitParser.ParseMultiFlagSet(parameter as string);

            bool enable = IsAnyInEnableListValid(enable_for);

            return enable ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public class IsScenarioPathToVisibilityConverter : OneWayValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string input = (string)value;
            bool reverse = parameter is string type && type == "inverse";

            bool enabled = input.EndsWith(".scenario");
            if (reverse)
                enabled = !enabled;

            if (MainWindow.profile_mapping.Count > 0)
            {
                return enabled ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }
    }


    public class ProfiletoVisibilityMulti : OneWayMultiValueConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool is_fp = (bool)values[0];
            int toolkit_selection = (int)values[1];
            var vis = Visibility.Collapsed;
            if (ToolkitProfiles.SettingsList.Count > 0)
            {
                if (MainWindow.halo_2_mcc && is_fp || MainWindow.halo_3 && is_fp || MainWindow.halo_reach && is_fp)
                    vis = Visibility.Visible;
            }
            else
            {
                //Either we're in desinger or there are no profiles. Reveal ourselves either way.
                vis = Visibility.Visible;
            }
            return vis;
        }
    }

    public class ProfiletoIsEnabled : OneWayValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            List<TogglesUI> enable_for = BindingToolkitParser.ParseMultiFlagSet(parameter as string);

            bool enable = IsAnyInEnableListValid(enable_for);

            return enable;
        }
    }

    public class ProfiletoContent : OneWayValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int selection_index = (int)value;
            object enum_item = null;
            if (ToolkitProfiles.SettingsList.Count > 0)
            {
                if (parameter is string && Int32.Parse(parameter as string) == 0)
                {
                    enum_item = ModelContent.gbxmodel;
                    if (!MainWindow.halo_ce)
                        enum_item = ModelContent.render;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 1)
                {
                    enum_item = LightmapContent.light_threshold;
                    if (!MainWindow.halo_ce)
                        enum_item = LightmapContent.light_quality;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 2)
                {
                    enum_item = Enum.GetValues(typeof(h2_quality_settings_stock));
                    if (MainWindow.halo_2_standalone_community)
                    {
                        enum_item = Enum.GetValues(typeof(h2_quality_settings_community));
                    }
                    else if (MainWindow.halo_2_mcc)
                    {
                        enum_item = Enum.GetValues(typeof(h2_quality_settings_mcc));
                    }
                    else if (MainWindow.halo_3 || MainWindow.halo_3_odst)
                    {
                        enum_item = Enum.GetValues(typeof(h3_quality_settings_stock));
                    }
                    else if (MainWindow.halo_reach)
                    {
                        enum_item = Enum.GetValues(typeof(hr_quality_settings_stock));
                    }
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 3)
                {
                    enum_item = "Run the lightmapper once before using" + "\n" + "Grants a speed boost for lightmapping by disabling error checking.";
                    if (MainWindow.halo_2_mcc || MainWindow.halo_3)
                    {
                        enum_item = "Run a PLAY build of tool." + "\n" + "This means it is optimized for speed and has fewer debug checks.";
                    }
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 4)
                {
                    enum_item = "Import just a GBX model.";
                    if (!MainWindow.halo_ce_mcc)
                    {
                        enum_item = "Import just a render model.";
                    }
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 5)
                {
                    enum_item = "Disable Asserts";
                    if (!MainWindow.halo_ce_mcc)
                    {
                        enum_item = "Use Tool Fast";
                    }
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 6)
                {
                    enum_item = "Select a folder with sound files to import.";
                    if (MainWindow.halo_2_standalone_community)
                    {
                        enum_item = "Select a sound tag to modify.";
                    }
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 7)
                {
                    enum_item = "Is MCC";
                    if (selection_index >= 2)
                    {
                        enum_item = "Is ODST";
                    }
                    if (selection_index >= 3)
                    {
                        enum_item = "Is Halo 4/H2AMP";
                    }
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 8)
                {
                    enum_item = "Is this an MCC build or legacy?";
                    if (selection_index >= 2)
                    {
                        enum_item = "Is this Halo 3 or Halo 3 ODST?";
                    }
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 9)
                {
                    enum_item = "Automatically run fbx-to-jms for relevant structure.";
                    if (!MainWindow.halo_ce_mcc)
                    {
                        enum_item = "Automatically run fbx-to-ass for relevant structure.";
                    }
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 10)
                {
                    enum_item = "Guerilla Path";
                    if (selection_index >= 3)
                    {
                        enum_item = "Foundation Path";
                    }
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 11)
                {
                    enum_item = "Run Guerilla";
                    if (MainWindow.halo_reach || MainWindow.halo_4)
                    {
                        enum_item = "Run Foundation";
                    }
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 12)
                {
                    enum_item = "Start Guerilla (the tag/content editor)";
                    if (MainWindow.halo_reach || MainWindow.halo_4)
                    {
                        enum_item = "Start Foundation (the tag/content editor)";
                    }
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 13)
                {
                    enum_item = "Select Guerilla Application";
                    if (selection_index >= 3)
                    {
                        enum_item = "Select Foundation Application";
                    }
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 14)
                {
                    enum_item = "Path to Guerilla.exe (content/tag editor).";
                    if (selection_index >= 3)
                    {
                        enum_item = "Path to Foundation.exe (content/tag editor).";
                    }
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 15)
                {
                    enum_item = Enum.GetValues(typeof(h2_sound_import_type));
                    if (MainWindow.halo_2)
                    {
                        enum_item = Enum.GetValues(typeof(h2_sound_import_type));
                    }
                    else if (MainWindow.halo_3)
                    {
                        enum_item = Enum.GetValues(typeof(h3_sound_import_type));
                    }
                    else if (MainWindow.halo_3_odst)
                    {
                        enum_item = Enum.GetValues(typeof(odst_sound_import_type));
                    }
                    else if (MainWindow.halo_reach)
                    {
                        enum_item = Enum.GetValues(typeof(reach_sound_import_type));
                    }
                }
            }
            else
            {
                if (parameter is string && Int32.Parse(parameter as string) == 0)
                {
                    enum_item = ModelContent.gbxmodel;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 1)
                {
                    enum_item = LightmapContent.light_threshold;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 2)
                {
                    enum_item = Enum.GetValues(typeof(h2_quality_settings_stock));
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 3)
                {
                    enum_item = "Run the lightmapper once before using" + "\n" + "Grants a speed boost for lightmapping by disabling error checking.";
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 4)
                {
                    enum_item = "Import just a GBX model.";
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 5)
                {
                    enum_item = "Disable Asserts";
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 6)
                {
                    enum_item = "Select a folder with sound files to import.";
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 7)
                {
                    enum_item = "Is MCC";
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 8)
                {
                    enum_item = "Is this an MCC build or legacy?";
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 9)
                {
                    enum_item = "Automatically run fbx-to-jms for relevant structure.";
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 10)
                {
                    enum_item = "Guerilla Path";
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 11)
                {
                    enum_item = "Run Guerilla";
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 12)
                {
                    enum_item = "Start Guerilla (the tag/content editor)";
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 13)
                {
                    enum_item = "Select Guerilla Application";
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 14)
                {
                    enum_item = "Path to Guerilla.exe (content/tag editor).";
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 15)
                {
                    enum_item = Enum.GetValues(typeof(h2_sound_import_type));
                }
            }

            return enum_item;
        }
    }

    public class ProfileSettingsVisibility : OneWayMultiValueConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isMCC = (bool)values[0];
            int gen_type_selection = (int)values[1];
            bool community_selection = (bool)values[2];
            string parameter_workaround = (string)values[3];
            var vis = Visibility.Collapsed;
            if (ToolkitProfiles.SettingsList.Count > 0)
            {
                if (parameter_workaround is string && Int32.Parse(parameter_workaround as string) == 0)
                {
                    //Check if the build type and gentype are set to an MCC
                    //or Halo 2 for tags and data directory args along with the verbose flag
                    if (isMCC && gen_type_selection <= 1)
                        vis = Visibility.Visible;
                }
                else if (parameter_workaround is string && Int32.Parse(parameter_workaround as string) == 1)
                {
                    //Check if the build type and gentype are set to a standalone Halo 1 or Halo 2 profile community flags
                    if (!isMCC && gen_type_selection <= 1)
                        vis = Visibility.Visible;
                }
                else if (parameter_workaround is string && Int32.Parse(parameter_workaround as string) == 2)
                {
                    //Check if the build type and gentype are set to a MCC Halo 1 for game root directory arg
                    if (isMCC && gen_type_selection == 0)
                        vis = Visibility.Visible;
                }
                else if (parameter_workaround is string && Int32.Parse(parameter_workaround as string) == 3)
                {
                    //Check if the build type and gentype are set to a MCC Halo 2 for batch and expert mode flag
                    if (isMCC && gen_type_selection == 1)
                        vis = Visibility.Visible;
                }
                else if (parameter_workaround is string && Int32.Parse(parameter_workaround as string) == 4)
                {
                    //Check if the build type is standalone Halo 2 community profile for H2Codez updates
                    if (!isMCC && gen_type_selection == 1 && community_selection == true)
                        vis = Visibility.Visible;
                }
                else if (parameter_workaround is string && Int32.Parse(parameter_workaround as string) == 5)
                {
                    //Check if the build type is set to a MCC and gentype is not Halo CE or is Halo 3 and above
                    if (isMCC && gen_type_selection != 0 || gen_type_selection >= 2)
                        vis = Visibility.Visible;
                }
            }
            else
            {
                //Either we're in desinger or there are no profiles. Reveal ourselves either way.
                vis = Visibility.Visible;
            }
            return vis;
        }
    }

    public class LightmapConfigModifier : OneWayMultiValueConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var lightmap_quality = values[0];
            int toolkit_selection = (int)values[1];
            if (MainWindow.toolkit_profile != null && lightmap_quality != null && toolkit_selection >= 0)
            {
                //Not sure what to do here. Crashes designer otherwise cause the list or value is empty
                if (MainWindow.toolkit_profile.Generation >= ToolkitProfiles.GameGen.Halo2 && lightmap_quality.ToString() == "custom")
                    return true;
                return false;
            }
            return false;
        }
    }

    // todo(num0005) figure out what this even is
    // num0005, I disabled it to see if anything happens, 2021-05-24
    // (General_101)Please test before disabling 
    public class ProfileIndexViewModel : INotifyPropertyChanged
    {
        public ProfileIndexViewModel()
        {
            int default_index = 0;
            if (Settings.Default.set_profile >= 0 && MainWindow.profile_mapping.Count > Settings.Default.set_profile)
                default_index = Settings.Default.set_profile;

            SelectedProfileIndex = default_index;
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
                MainWindow.profile_index = 0;
                if (value != -1)
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

    public class LightmapSlider : Slider
    {
        /*
         * The Slider class currently updates the contents of the tooltip in two functions
         * OnThumbDragDelta and OnThumbDragDelta
         * The contents is given by GetAutoToolTipNumber, but this is a private function so we can't trivially override it.
         * https://github.com/dotnet/wpf/blob/89d172db0b7a192de720c6cfba5e28a1e7d46123/src/Microsoft.DotNet.Wpf/src/PresentationFramework/System/Windows/Controls/Slider.cs#L903
         * So instead we use reflection to get _autoToolTip and override the functions that update it
         * This is fragile, if it breaks too much just copy the code into the launcher, it's MIT licensed.
         * 
         * Idea taken from https://joshsmithonwpf.wordpress.com/2007/09/14/modifying-the-auto-tooltip-of-a-slider/
         */

        protected override void OnThumbDragDelta(System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            base.OnThumbDragDelta(e);
            UpdateAutoTooltip();
        }

        protected override void OnThumbDragStarted(System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            base.OnThumbDragStarted(e);
            UpdateAutoTooltip();
        }

        /// <summary>
        /// Value converted to a lightmap threshold, will change the value of `Value`
        /// </summary>
        [Bindable(true)]
        [Category("Behavior")]
        public double ConvertedValue
        {
            get
            {
                return LinearToThreshold(Value);
            }

            set
            {
                Value = ThresholdToLinear(value);
            }
        }

        /// <summary>
        /// Ticks in lightmap threshold units, will change the value of `Ticks` on assignment
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        public DoubleCollection ConvertedTicks
        {
            get
            {
                DoubleCollection converted = new();
                foreach (double tick in Ticks)
                {
                    converted.Add(LinearToThreshold(tick));
                }
                return converted;
            }

            set
            {
                Ticks.Clear();
                foreach (double tick in value)
                {
                    Ticks.Add(ThresholdToLinear(tick));
                }
            }
        }

        public double ScaleBase = 10.0;

        private double LinearToThreshold(double x)
        {
            return (Math.Pow(ScaleBase, x) - 1.0) / (ScaleBase - 1.0);
        }

        private double ThresholdToLinear(double y)
        {
            return Math.Log(y * (ScaleBase - 1.0) + 1.0, ScaleBase);
        }

        private void UpdateAutoTooltip()
        {
            if (_autoToolTip != null)
            {
                _autoToolTip.Content = GetAutoToolTipNumber();
            }
        }

        // START copied from WPF source (Value replaced with convertedValue)
        private string GetAutoToolTipNumber()
        {
            NumberFormatInfo format = (NumberFormatInfo)(NumberFormatInfo.CurrentInfo.Clone());
            format.NumberDecimalDigits = this.AutoToolTipPrecision;
            return this.ConvertedValue.ToString("N", format);
        }
        // END copied

        private ToolTip _autoToolTip => typeof(Slider).GetField("_autoToolTip", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this) as ToolTip;
    }
}
