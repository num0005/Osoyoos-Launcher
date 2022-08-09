using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using ToolkitLauncher.Properties;

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

    public class ToolkitToSpaceConverter : OneWayValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var grid = new GridLength(0);
            if (MainWindow.profile_mapping.Count > 0)
            {
                if (parameter is string && Int32.Parse(parameter as string) == 0)
                {
                    if (MainWindow.halo_ce_mcc || MainWindow.halo_2_standalone_community)
                        grid = new GridLength(8);
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 1)
                {
                    if (MainWindow.halo_ce_mcc)
                        grid = new GridLength(8);
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 2)
                {
                    if (MainWindow.halo_2_standalone_community)
                        grid = new GridLength(8);
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 3)
                {
                    if (MainWindow.halo_mcc || !MainWindow.halo_ce && !MainWindow.halo_2)
                        grid = new GridLength(8);
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 4)
                {
                    if (MainWindow.halo_2_standalone_community || MainWindow.halo_mcc)
                        grid = new GridLength(8);
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 5)
                {
                    if (MainWindow.halo_ce || MainWindow.halo_2)
                        grid = new GridLength(8);
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 6)
                {
                    if (MainWindow.halo_2_mcc || MainWindow.halo_3)
                        grid = new GridLength(8);
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 7)
                {
                    if (MainWindow.halo_2_mcc)
                        grid = new GridLength(8);
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 8)
                {
                    if ((bool)value)
                        grid = new GridLength(8);
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 9)
                {
                    if (MainWindow.halo_2_mcc)
                        grid = new GridLength(1, GridUnitType.Star);
                    else
                        grid = new GridLength((double)GridUnitType.Auto);
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 10)
                {
                    if (MainWindow.halo_ce_mcc)
                        grid = new GridLength(1, GridUnitType.Star);
                    else
                        grid = new GridLength(1, (double)GridUnitType.Auto);
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 11)
                {
                    if (!MainWindow.halo_ce && !MainWindow.halo_2_standalone)
                        grid = new GridLength(1, GridUnitType.Star);
                    else
                        grid = new GridLength(1, (double)GridUnitType.Auto);
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 12)
                {
                    if (!MainWindow.halo_ce && !MainWindow.halo_2_standalone_stock)
                        grid = new GridLength(8);
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 13)
                {
                    if (MainWindow.halo_3)
                        grid = new GridLength(1, GridUnitType.Star);
                    else
                        grid = new GridLength(1, (double)GridUnitType.Auto);
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 14)
                {
                    if (!MainWindow.halo_ce_standalone && !MainWindow.halo_2_standalone)
                        grid = new GridLength(1, GridUnitType.Star);
                    else
                        grid = new GridLength(1, (double)GridUnitType.Auto);
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 15)
                {
                    if (!MainWindow.halo_ce && !MainWindow.halo_2)
                        grid = new GridLength(1, GridUnitType.Star);
                    else
                        grid = new GridLength(1, (double)GridUnitType.Auto);
                }
            }
            else
            {
                //Either we're in desinger or there are no profiles. Reveal ourselves either way.
                if (parameter is string && Int32.Parse(parameter as string) == 9
                    || parameter is string && Int32.Parse(parameter as string) == 10
                    || parameter is string && Int32.Parse(parameter as string) == 11
                    || parameter is string && Int32.Parse(parameter as string) == 13
                    || parameter is string && Int32.Parse(parameter as string) == 14
                    || parameter is string && Int32.Parse(parameter as string) == 15)
                {
                    grid = new GridLength(1, GridUnitType.Star);
                }
                else
                {
                    grid = new GridLength(8);
                }

            }
            return grid;
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
                if (MainWindow.toolkit_profile.GameGen >= 1 || string_encoding == 1)
                    text_string = "Select a folder with .txt files to import.";
            }
            return text_string;
        }
    }

    public class ProfiletoVisibility : OneWayValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var vis = Visibility.Collapsed;
            if (ToolkitProfiles.SettingsList.Count > 0)
            {
                if (parameter is string && Int32.Parse(parameter as string) == 0)
                {
                    if (MainWindow.halo_community)
                        vis = Visibility.Visible;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 1)
                {
                    if (MainWindow.halo_mcc || !MainWindow.halo_ce && !MainWindow.halo_2)
                        vis = Visibility.Visible;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 2)
                {
                    if (MainWindow.halo_ce)
                        vis = Visibility.Visible;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 3)
                {
                    if (MainWindow.halo_ce_mcc)
                        vis = Visibility.Visible;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 4)
                {
                    if (MainWindow.halo_2)
                        vis = Visibility.Visible;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 5)
                {
                    if (MainWindow.halo_2_standalone_community)
                        vis = Visibility.Visible;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 6)
                {
                    if (MainWindow.halo_2_mcc)
                        vis = Visibility.Visible;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 7)
                {
                    if (!MainWindow.halo_ce && !MainWindow.halo_2_standalone_stock)
                        vis = Visibility.Visible;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 8)
                {
                    if (!MainWindow.halo_ce_standalone && !MainWindow.halo_2_standalone_stock)
                        vis = Visibility.Visible;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 9)
                {
                    string textbox_path = (string)value;
                    if (!textbox_path.EndsWith(".scenario"))
                    {
                        vis = Visibility.Visible;
                    }
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 10)
                {
                    string textbox_path = (string)value;
                    if (textbox_path.EndsWith(".scenario"))
                    {
                        vis = Visibility.Visible;
                    }
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 11)
                {
                    if (!MainWindow.halo_ce && !MainWindow.halo_2)
                        vis = Visibility.Visible;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 12)
                {
                    if (!MainWindow.halo_ce && !MainWindow.halo_2_standalone)
                        vis = Visibility.Visible;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 13)
                {
                    if (!MainWindow.halo_ce)
                        vis = Visibility.Visible;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 14)
                {
                    if ((bool)value)
                        vis = Visibility.Visible;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 15)
                {
                    if (MainWindow.halo_mcc || !MainWindow.halo_ce && !MainWindow.halo_2)
                        vis = Visibility.Visible;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 16)
                {
                    if (MainWindow.halo_ce_mcc || MainWindow.halo_2_mcc)
                        vis = Visibility.Visible;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 17)
                {
                    if (MainWindow.halo_2_standalone_community || MainWindow.halo_3)
                        vis = Visibility.Visible;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 18)
                {
                    if (MainWindow.halo_3_odst)
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

    public class ProfiletoVisibilityMulti : OneWayMultiValueConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool is_fp = (bool)values[0];
            int toolkit_selection = (int)values[1];
            var vis = Visibility.Collapsed;
            if (ToolkitProfiles.SettingsList.Count > 0)
            {
                if (MainWindow.halo_2_mcc && is_fp || MainWindow.halo_3 && is_fp)
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
            var is_enabled = false;
            if (ToolkitProfiles.SettingsList.Count > 0)
            {
                if (parameter is string && Int32.Parse(parameter as string) == 0)
                {
                    if (MainWindow.halo_ce)
                        is_enabled = true;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 1)
                {
                    if (MainWindow.halo_2)
                        is_enabled = true;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 2)
                {
                    if (MainWindow.halo_2_standalone_stock)
                        is_enabled = true;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 3)
                {
                    if (MainWindow.halo_3)
                        is_enabled = true;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 4)
                {
                    //Tool
                    if (!string.IsNullOrEmpty(MainWindow.toolkit_profile.ToolPath) && MainWindow.toolkit_profile != null)
                        is_enabled = true;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 5 && MainWindow.toolkit_profile != null)
                {
                    //Guerilla
                    if (!string.IsNullOrEmpty(MainWindow.toolkit_profile.GuerillaPath) && MainWindow.toolkit_profile != null)
                        is_enabled = true;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 6 && MainWindow.toolkit_profile != null)
                {
                    //Sapien
                    if (!string.IsNullOrEmpty(MainWindow.toolkit_profile.SapienPath))
                        is_enabled = true;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 7 && MainWindow.toolkit_profile != null)
                {
                    //Game
                    if (!string.IsNullOrEmpty(MainWindow.toolkit_profile.GameExePath))
                        is_enabled = true;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 8)
                {
                    if (!MainWindow.halo_ce)
                        is_enabled = true;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 9)
                {
                    if (!MainWindow.halo_2_standalone_stock)
                        is_enabled = true;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 10)
                {
                    if (!MainWindow.halo_2_standalone_stock && !MainWindow.halo_3)
                        is_enabled = true;
                }
                else if (parameter is string && Int32.Parse(parameter as string) == 11)
                {
                    bool is_level_import = (bool)value;
                    if (!is_level_import)
                        is_enabled = true;
                }
            }
            else
            {
                //Either we're in desinger or there are no profiles. Reveal ourselves either way.
                is_enabled = true;
            }
            return is_enabled;
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
                    //Check if the build type and gentype are set to an MCC Halo 1 or Halo 2 for tags and data directory args along with the verbose flag
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

    public class ProfileSettingsToSpace : OneWayMultiValueConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var grid = new GridLength(0);
            bool isMCC = (bool)values[0];
            int gen_type_selection = (int)values[1];
            bool community_selection = (bool)values[2];
            string parameter_workaround = (string)values[3];
            if (ToolkitProfiles.SettingsList.Count > 0)
            {
                if (parameter_workaround is string && Int32.Parse(parameter_workaround as string) == 0)
                {
                    //Check if the build type and gentype are set to a standalone Halo 1 or Halo 2 profile for community extensions
                    if (!isMCC)
                        if (gen_type_selection == 0 || gen_type_selection == 1)
                            grid = new GridLength(8);
                }
                else if (parameter_workaround is string && Int32.Parse(parameter_workaround as string) == 1)
                {
                    //Check if the build type and gentype are set to an MCC Halo 1 or Halo 2 profile for the verbose flag
                    if (isMCC)
                        if (gen_type_selection == 0 || gen_type_selection == 1)
                            grid = new GridLength(8);
                }
                else if (parameter_workaround is string && Int32.Parse(parameter_workaround as string) == 2)
                {
                    //Check if the build type and gentype are set to an MCC Halo 2 profile for the expert and batch flag
                    if (isMCC && gen_type_selection == 1)
                        grid = new GridLength(8);
                }
                else if (parameter_workaround is string && Int32.Parse(parameter_workaround as string) == 3)
                {
                    //Check if the build type is MCC Halo 2 profile for the asset directory
                    if (isMCC && gen_type_selection <= 1)
                        grid = new GridLength(8);
                }
                else if (parameter_workaround is string && Int32.Parse(parameter_workaround as string) == 4)
                {
                    //Check if the build type is standalone Halo 2 community profile for H2Codez updates
                    if (!isMCC && gen_type_selection == 1 && community_selection)
                        grid = new GridLength(8);
                }
                else if (parameter_workaround is string && Int32.Parse(parameter_workaround as string) == 5)
                {
                    //Check if the build type is set to a MCC and gentype is not Halo CE or is Halo 3 and above
                    if (isMCC && gen_type_selection != 0 || gen_type_selection >= 2)
                        grid = new GridLength(1, (double)GridUnitType.Auto);
                }
            }
            else
            {
                if (parameter_workaround is string && Int32.Parse(parameter_workaround as string) == 5)
                {
                    grid = new GridLength(1, (double)GridUnitType.Auto);
                }
                else
                {
                    //Either we're in desinger or there are no profiles. Reveal ourselves either way.
                    grid = new GridLength(8);
                }
            }
            return grid;
        }
    }

    public class LightmapConfigModifier : OneWayMultiValueConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int custom_index = 10;
            int lightmap_quality = (int)values[0];
            int toolkit_selection = (int)values[1];
            if (MainWindow.toolkit_profile != null && lightmap_quality >= 0 && toolkit_selection >= 0)
            {
                //Not sure what to do here. Crashes designer otherwise cause the list or value is empty
                if (MainWindow.toolkit_profile.GameGen >= 1 && lightmap_quality == custom_index)
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

    public class LauncherThemeSettings
    {
        public void SetLauncherTheme(ThemeType theme_index)
        {
            switch (theme_index)
            {
                case ThemeType.light:
                    // Light Theme

                    //Window Colors - Grids need Background set for this to work
                    Application.Current.Resources["WindowPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    Application.Current.Resources["WindowSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 229, 229, 229));

                    // Button Colors - Set in style. Will work across all buttons automatically
                    Application.Current.Resources["ButtonPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 221, 221, 221));
                    Application.Current.Resources["ButtonSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 112, 112, 112));
                    Application.Current.Resources["ButtonHoverPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 190, 230, 253));
                    Application.Current.Resources["ButtonHoverSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 60, 127, 177));
                    Application.Current.Resources["ButtonIsEnabledPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 244, 244, 244));
                    Application.Current.Resources["ButtonIsEnabledSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 173, 178, 181));

                    // Groupbox Colors - Set in style. Will work across all groupboxes automatically
                    Application.Current.Resources["GroupboxPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 213, 223, 229));
                    Application.Current.Resources["GroupboxSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

                    // TabControl Colors - Set in style. Will work across all tab controls automatically
                    Application.Current.Resources["TabControlPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    Application.Current.Resources["TabControlSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 172, 172, 172));

                    // TabItem Colors - Set in style. Will work across all tab items automatically
                    // (General_101) I'm replacing the gradient instead of using Dynamic colors in the gradient to work around an issue.
                    // (General_101) This should work fine for tab item. 
                    LinearGradientBrush TabItemBackgroundLight = new();
                    TabItemBackgroundLight.StartPoint = new Point(0, 0);
                    TabItemBackgroundLight.EndPoint = new Point(0, 1);
                    TabItemBackgroundLight.GradientStops.Add(new GradientStop(Color.FromArgb(255, 240, 240, 240), 0.0));
                    TabItemBackgroundLight.GradientStops.Add(new GradientStop(Color.FromArgb(255, 229, 229, 229), 1.0));
                    Application.Current.Resources["TabItemBackground"] = TabItemBackgroundLight;
                    Application.Current.Resources["TabItemTertiaryColor"] = new SolidColorBrush(Color.FromArgb(255, 140, 142, 148));
                    LinearGradientBrush TabItemHoverBackgroundLight = new();
                    TabItemHoverBackgroundLight.StartPoint = new Point(0, 0);
                    TabItemHoverBackgroundLight.EndPoint = new Point(0, 1);
                    TabItemHoverBackgroundLight.GradientStops.Add(new GradientStop(Color.FromArgb(255, 236, 244, 252), 0.0));
                    TabItemHoverBackgroundLight.GradientStops.Add(new GradientStop(Color.FromArgb(255, 220, 236, 252), 1.0));
                    Application.Current.Resources["TabItemHoverBackground"] = TabItemHoverBackgroundLight;
                    Application.Current.Resources["TabItemSelectedPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    Application.Current.Resources["TabItemSelectedSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 172, 172, 172));
                    Application.Current.Resources["TabItemIsEnabledPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 244, 244, 244));
                    Application.Current.Resources["TabItemIsEnabledSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 201, 199, 186));

                    // Slider Colors - Set in style. Will work across all sliders automatically
                    Application.Current.Resources["SliderThumbPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
                    Application.Current.Resources["SliderThumbSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 172, 172, 172));
                    Application.Current.Resources["SliderThumbHoverPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 220, 236, 252));
                    Application.Current.Resources["SliderThumbHoverSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 126, 180, 234));
                    Application.Current.Resources["SliderThumbHeldPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 218, 236, 252));
                    Application.Current.Resources["SliderThumbHeldSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 86, 157, 229));
                    Application.Current.Resources["SliderThumbIsEnabledPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
                    Application.Current.Resources["SliderThumbIsEnabledSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 217, 217, 217));
                    Application.Current.Resources["SliderTrackPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 231, 234, 234));
                    Application.Current.Resources["SliderTrackSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 214, 214, 214));

                    // Text Color - Items with text need Foreground set for this to work
                    Application.Current.Resources["TextColor"] = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));

                    // ComboBox Colors - Set in style. Will work across all comboboxes automatically
                    // (General_101) I'm replacing the gradient instead of using Dynamic colors in the gradient to work around an issue.
                    // (General_101) This should work fine for comboboxes. 
                    LinearGradientBrush ComboBoxBackgroundLight = new();
                    ComboBoxBackgroundLight.StartPoint = new Point(0, 0);
                    ComboBoxBackgroundLight.EndPoint = new Point(0, 1);
                    ComboBoxBackgroundLight.GradientStops.Add(new GradientStop(Color.FromArgb(255, 240, 240, 240), 0.0));
                    ComboBoxBackgroundLight.GradientStops.Add(new GradientStop(Color.FromArgb(255, 229, 229, 229), 1.0));
                    Application.Current.Resources["ComboBoxPrimaryColor"] = ComboBoxBackgroundLight;
                    Application.Current.Resources["ComboBoxSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 172, 172, 172));
                    LinearGradientBrush ComboBoxHoverBackgroundLight = new();
                    ComboBoxHoverBackgroundLight.StartPoint = new Point(0, 0);
                    ComboBoxHoverBackgroundLight.EndPoint = new Point(0, 1);
                    ComboBoxHoverBackgroundLight.GradientStops.Add(new GradientStop(Color.FromArgb(255, 236, 244, 252), 0.0));
                    ComboBoxHoverBackgroundLight.GradientStops.Add(new GradientStop(Color.FromArgb(255, 220, 236, 252), 1.0));
                    Application.Current.Resources["ComboBoxHoverPrimaryColor"] = ComboBoxHoverBackgroundLight;
                    Application.Current.Resources["ComboBoxHoverSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 126, 180, 234));
                    Application.Current.Resources["ComboBoxListPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    Application.Current.Resources["ComboBoxListSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
                    Application.Current.Resources["ComboBoxIsEnabledPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
                    Application.Current.Resources["ComboBoxIsEnabledSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 217, 217, 217));
                    break;
                case ThemeType.dark:
                    // Dark Theme

                    //Window Colors - Grids need Background set for this to work
                    Application.Current.Resources["WindowPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 54, 53, 57));
                    Application.Current.Resources["WindowSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 32, 32, 32));

                    // Button Colors - Set in style. Will work across all buttons automatically
                    Application.Current.Resources["ButtonPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 85, 85, 85));
                    Application.Current.Resources["ButtonSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
                    Application.Current.Resources["ButtonHoverPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 90, 130, 153));
                    Application.Current.Resources["ButtonHoverSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 0, 27, 77));
                    Application.Current.Resources["ButtonIsEnabledPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 144, 144, 144));
                    Application.Current.Resources["ButtonIsEnabledSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 73, 78, 81));

                    // Groupbox Colors - Set in style. Will work across all groupboxes automatically
                    Application.Current.Resources["GroupboxPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 42, 32, 26));
                    Application.Current.Resources["GroupboxSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));

                    // TabControl Colors - Set in style. Will work across all tab controls automatically
                    Application.Current.Resources["TabControlPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                    Application.Current.Resources["TabControlSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 83, 83, 83));

                    // TabItem Colors - Set in style. Will work across all tab items automatically
                    // (General_101) I'm replacing the gradient instead of using Dynamic colors in the gradient to work around an issue.
                    // (General_101) This should work fine for tab item. 
                    LinearGradientBrush TabItemBackgroundDark = new();
                    TabItemBackgroundDark.StartPoint = new Point(0, 0);
                    TabItemBackgroundDark.EndPoint = new Point(0, 1);
                    TabItemBackgroundDark.GradientStops.Add(new GradientStop(Color.FromArgb(255, 85, 85, 85), 0.0));
                    TabItemBackgroundDark.GradientStops.Add(new GradientStop(Color.FromArgb(255, 70, 70, 70), 1.0));
                    Application.Current.Resources["TabItemBackground"] = TabItemBackgroundDark;
                    Application.Current.Resources["TabItemTertiaryColor"] = new SolidColorBrush(Color.FromArgb(255, 40, 42, 48));
                    LinearGradientBrush TabItemHoverBackgroundDark = new();
                    TabItemHoverBackgroundDark.StartPoint = new Point(0, 0);
                    TabItemHoverBackgroundDark.EndPoint = new Point(0, 1);
                    TabItemHoverBackgroundDark.GradientStops.Add(new GradientStop(Color.FromArgb(255, 136, 144, 152), 0.0));
                    TabItemHoverBackgroundDark.GradientStops.Add(new GradientStop(Color.FromArgb(255, 120, 136, 152), 1.0));
                    Application.Current.Resources["TabItemHoverBackground"] = TabItemHoverBackgroundDark;
                    Application.Current.Resources["TabItemSelectedPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 125, 125, 125));
                    Application.Current.Resources["TabItemSelectedSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 80, 82, 88));
                    Application.Current.Resources["TabItemIsEnabledPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 50, 50, 50));
                    Application.Current.Resources["TabItemIsEnabledSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 20, 30, 40));

                    // Slider Colors - Set in style. Will work across all sliders automatically
                    Application.Current.Resources["SliderThumbPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 140, 140, 140));
                    Application.Current.Resources["SliderThumbSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 72, 72, 72));
                    Application.Current.Resources["SliderThumbHoverPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 120, 136, 152));
                    Application.Current.Resources["SliderThumbHoverSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 26, 80, 134));
                    Application.Current.Resources["SliderThumbHeldPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 118, 136, 152));
                    Application.Current.Resources["SliderThumbHeldSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 20, 57, 129));
                    Application.Current.Resources["SliderThumbIsEnabledPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 140, 140, 140));
                    Application.Current.Resources["SliderThumbIsEnabledSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 117, 117, 117));
                    Application.Current.Resources["SliderTrackPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 131, 134, 134));
                    Application.Current.Resources["SliderTrackSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 114, 114, 114));

                    // Text Color - Items with text need Foreground set for this to work
                    Application.Current.Resources["TextColor"] = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

                    // ComboBox Colors - Set in style. Will work across all comboboxes automatically
                    // (General_101) I'm replacing the gradient instead of using Dynamic colors in the gradient to work around an issue.
                    // (General_101) This should work fine for comboboxes. 
                    LinearGradientBrush ComboBoxBackgroundDark = new();
                    ComboBoxBackgroundDark.StartPoint = new Point(0, 0);
                    ComboBoxBackgroundDark.EndPoint = new Point(0, 1);
                    ComboBoxBackgroundDark.GradientStops.Add(new GradientStop(Color.FromArgb(255, 85, 85, 85), 0.0));
                    ComboBoxBackgroundDark.GradientStops.Add(new GradientStop(Color.FromArgb(255, 70, 70, 70), 1.0));
                    Application.Current.Resources["ComboBoxPrimaryColor"] = ComboBoxBackgroundDark;
                    Application.Current.Resources["ComboBoxSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
                    LinearGradientBrush ComboBoxHoverBackgroundDark = new();
                    ComboBoxHoverBackgroundDark.StartPoint = new Point(0, 0);
                    ComboBoxHoverBackgroundDark.EndPoint = new Point(0, 1);
                    ComboBoxHoverBackgroundDark.GradientStops.Add(new GradientStop(Color.FromArgb(255, 136, 144, 152), 0.0));
                    ComboBoxHoverBackgroundDark.GradientStops.Add(new GradientStop(Color.FromArgb(255, 120, 136, 152), 1.0));
                    Application.Current.Resources["ComboBoxHoverPrimaryColor"] = ComboBoxHoverBackgroundDark;
                    Application.Current.Resources["ComboBoxHoverSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 26, 80, 134));
                    Application.Current.Resources["ComboBoxListPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                    Application.Current.Resources["ComboBoxListSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
                    Application.Current.Resources["ComboBoxIsEnabledPrimaryColor"] = new SolidColorBrush(Color.FromArgb(255, 50, 50, 50));
                    Application.Current.Resources["ComboBoxIsEnabledSecondaryColor"] = new SolidColorBrush(Color.FromArgb(255, 20, 30, 40));
                    break;
            }
        }
    }
}
