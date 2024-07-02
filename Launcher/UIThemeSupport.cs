using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace ToolkitLauncher
{
    record ARGB(byte a, byte r, byte g, byte b);
    record SolidColorTheme(
         ARGB TextColor,

         //Window Colors - Grids need Background set for this to work
         ARGB WindowPrimaryColor,
         ARGB WindowSecondaryColor,

         ARGB ButtonPrimaryColor,
         ARGB ButtonSecondaryColor,
         ARGB ButtonHoverPrimaryColor,
         ARGB ButtonHoverSecondaryColor,
         ARGB ButtonIsEnabledPrimaryColor,
         ARGB ButtonIsEnabledSecondaryColor,

         ARGB GroupboxPrimaryColor,
         ARGB GroupboxSecondaryColor,

         ARGB TabControlPrimaryColor,
         ARGB TabControlSecondaryColor,

         List<ARGB> TabItemBackground,
         ARGB TabItemTertiaryColor,
         List<ARGB> TabItemHoverBackground,
         ARGB TabItemSelectedPrimaryColor,
         ARGB TabItemSelectedSecondaryColor,
         ARGB TabItemIsEnabledPrimaryColor,
         ARGB TabItemIsEnabledSecondaryColor,


         ARGB SliderThumbPrimaryColor,
         ARGB SliderThumbSecondaryColor,
         ARGB SliderThumbHoverPrimaryColor,
         ARGB SliderThumbHoverSecondaryColor,
         ARGB SliderThumbHeldPrimaryColor,
         ARGB SliderThumbHeldSecondaryColor,
         ARGB SliderThumbIsEnabledSecondaryColor,
         ARGB SliderTrackPrimaryColor,
         ARGB SliderTrackSecondaryColor,



         List<ARGB> ComboBoxPrimaryColor,
         ARGB ComboBoxSecondaryColor,
         List<ARGB> ComboBoxHoverPrimaryColor,
         ARGB ComboBoxHoverSecondaryColor,
         ARGB ComboBoxListPrimaryColor,
         ARGB ComboBoxListSecondaryColor,
         ARGB ComboBoxIsEnabledPrimaryColor,
         ARGB ComboBoxIsEnabledSecondaryColor
    );
    public class LauncherThemeSettings
    {
        private static readonly ARGB white = new ARGB(255, 255, 255, 255);
        private static readonly ARGB black = new ARGB(255, 0, 0, 0);

        private static readonly ARGB light_accent_1 = new ARGB(255, 172, 172, 172);

        /*
        readonly IDictionary<ThemeType, SolidColorTheme> themes = new Dictionary<ThemeType, SolidColorTheme>
        {
            /*
            { ThemeType.light,
                new SolidColorTheme(
                    TextColor: black,

                    WindowPrimaryColor: white,
                    WindowSecondaryColor: new ARGB(255, 229, 229, 229),

                    ButtonPrimaryColor: new ARGB(255, 221, 221, 221),
                    ButtonSecondaryColor: new ARGB(255, 112, 112, 112),
                    ButtonHoverPrimaryColor: new ARGB(255, 190, 230, 253),
                    ButtonHoverSecondaryColor: new ARGB(255, 60, 127, 177),
                    ButtonIsEnabledPrimaryColor: new ARGB(255, 244, 244, 244),
                    ButtonIsEnabledSecondaryColor: new ARGB(255, 173, 178, 181),

                    GroupboxPrimaryColor: new ARGB(255, 213, 223, 229),
                    GroupboxSecondaryColor: white,

                    TabControlPrimaryColor: white,
                    TabControlSecondaryColor: light_accent_1,

                    TabItemBackground: new() { new ARGB(255, 240, 240, 240), new ARGB(255, 229, 229, 229) },
                    TabItemTertiaryColor: new ARGB(255, 140, 142, 148),

                    TabItemHoverBackground: new() { new ARGB(255, 236, 244, 252), new ARGB(255, 220, 236, 252) },
                    TabItemSelectedPrimaryColor: white,
                    TabItemSelectedSecondaryColor: new ARGB(255, 172, 172, 172),
                    )
            }


    };
         */

        readonly IDictionary<ThemeType, SolidColorTheme> themes = new Dictionary<ThemeType, SolidColorTheme>
    {
        { ThemeType.light,
            new SolidColorTheme(
                TextColor: black,

                WindowPrimaryColor: white,
                WindowSecondaryColor: new ARGB(255, 229, 229, 229),

                ButtonPrimaryColor: new ARGB(255, 221, 221, 221),
                ButtonSecondaryColor: new ARGB(255, 112, 112, 112),
                ButtonHoverPrimaryColor: new ARGB(255, 190, 230, 253),
                ButtonHoverSecondaryColor: new ARGB(255, 60, 127, 177),
                ButtonIsEnabledPrimaryColor: new ARGB(255, 244, 244, 244),
                ButtonIsEnabledSecondaryColor: new ARGB(255, 173, 178, 181),

                GroupboxPrimaryColor: new ARGB(255, 213, 223, 229),
                GroupboxSecondaryColor: white,

                TabControlPrimaryColor: white,
                TabControlSecondaryColor: light_accent_1,

                TabItemBackground: new() { new ARGB(255, 240, 240, 240), new ARGB(255, 229, 229, 229) },
                TabItemTertiaryColor: new ARGB(255, 140, 142, 148),

                TabItemHoverBackground: new() { new ARGB(255, 236, 244, 252), new ARGB(255, 220, 236, 252) },
                TabItemSelectedPrimaryColor: white,
                TabItemSelectedSecondaryColor: light_accent_1,
                TabItemIsEnabledPrimaryColor: new ARGB(255, 244, 244, 244),
                TabItemIsEnabledSecondaryColor: new ARGB(255, 201, 199, 186),

                SliderThumbPrimaryColor: new ARGB(255, 240, 240, 240),
                SliderThumbSecondaryColor: light_accent_1,
                SliderThumbHoverPrimaryColor: new ARGB(255, 220, 236, 252),
                SliderThumbHoverSecondaryColor: new ARGB(255, 126, 180, 234),
                SliderThumbHeldPrimaryColor: new ARGB(255, 218, 236, 252),
                SliderThumbHeldSecondaryColor: new ARGB(255, 86, 157, 229),
                SliderThumbIsEnabledSecondaryColor: new ARGB(255, 217, 217, 217),
                SliderTrackPrimaryColor: new ARGB(255, 231, 234, 234),
                SliderTrackSecondaryColor: new ARGB(255, 214, 214, 214),

                ComboBoxPrimaryColor: new() { new ARGB(255, 240, 240, 240), new ARGB(255, 229, 229, 229) },
                ComboBoxSecondaryColor: light_accent_1,
                ComboBoxHoverPrimaryColor: new() { new ARGB(255, 236, 244, 252), new ARGB(255, 220, 236, 252) },
                ComboBoxHoverSecondaryColor: new ARGB(255, 126, 180, 234),
                ComboBoxListPrimaryColor: white,
                ComboBoxListSecondaryColor: new ARGB(255, 100, 100, 100),
                ComboBoxIsEnabledPrimaryColor: new ARGB(255, 240, 240, 240),
                ComboBoxIsEnabledSecondaryColor: new ARGB(255, 217, 217, 217)
            )
        },
        { ThemeType.dark,
            new SolidColorTheme(
                TextColor: white,

                WindowPrimaryColor: new ARGB(255, 54, 53, 57),
                WindowSecondaryColor: new ARGB(255, 32, 32, 32),

                ButtonPrimaryColor: new ARGB(255, 85, 85, 85),
                ButtonSecondaryColor: new ARGB(255, 51, 51, 51),
                ButtonHoverPrimaryColor: new ARGB(255, 90, 130, 153),
                ButtonHoverSecondaryColor: new ARGB(255, 0, 27, 77),
                ButtonIsEnabledPrimaryColor: new ARGB(255, 144, 144, 144),
                ButtonIsEnabledSecondaryColor: new ARGB(255, 73, 78, 81),

                GroupboxPrimaryColor: new ARGB(255, 42, 32, 26),
                GroupboxSecondaryColor: black,

                TabControlPrimaryColor: black,
                TabControlSecondaryColor: new ARGB(255, 83, 83, 83),

                TabItemBackground: new() { new ARGB(255, 85, 85, 85), new ARGB(255, 70, 70, 70) },
                TabItemTertiaryColor: new ARGB(255, 40, 42, 48),

                TabItemHoverBackground: new() { new ARGB(255, 136, 144, 152), new ARGB(255, 120, 136, 152) },
                TabItemSelectedPrimaryColor: new ARGB(255, 125, 125, 125),
                TabItemSelectedSecondaryColor: new ARGB(255, 80, 82, 88),
                TabItemIsEnabledPrimaryColor: new ARGB(255, 50, 50, 50),
                TabItemIsEnabledSecondaryColor: new ARGB(255, 20, 30, 40),

                SliderThumbPrimaryColor: new ARGB(255, 140, 140, 140),
                SliderThumbSecondaryColor: new ARGB(255, 72, 72, 72),
                SliderThumbHoverPrimaryColor: new ARGB(255, 120, 136, 152),
                SliderThumbHoverSecondaryColor: new ARGB(255, 26, 80, 134),
                SliderThumbHeldPrimaryColor: new ARGB(255, 118, 136, 152),
                SliderThumbHeldSecondaryColor: new ARGB(255, 20, 57, 129),
                SliderThumbIsEnabledSecondaryColor: new ARGB(255, 117, 117, 117),
                SliderTrackPrimaryColor: new ARGB(255, 131, 134, 134),
                SliderTrackSecondaryColor: new ARGB(255, 114, 114, 114),

                ComboBoxPrimaryColor: new() { new ARGB(255, 85, 85, 85), new ARGB(255, 70, 70, 70) },
                ComboBoxSecondaryColor: new ARGB(255, 51, 51, 51),
                ComboBoxHoverPrimaryColor: new() { new ARGB(255, 136, 144, 152), new ARGB(255, 120, 136, 152) },
                ComboBoxHoverSecondaryColor: new ARGB(255, 26, 80, 134),
                ComboBoxListPrimaryColor: black,
                ComboBoxListSecondaryColor: new ARGB(255, 150, 150, 150),
                ComboBoxIsEnabledPrimaryColor: new ARGB(255, 50, 50, 50),
                ComboBoxIsEnabledSecondaryColor: new ARGB(255, 20, 30, 40)
            )
        }
    };



        private void SetSolidColorTheme(SolidColorTheme theme)
        {
            static Color MakeColor(ARGB argb)
            {
                return Color.FromArgb(argb.a, argb.r, argb.g, argb.b);
            }

            static SolidColorBrush MakeSolidColorBrush(ARGB argb)
            {
                return new SolidColorBrush(MakeColor(argb));
            }

            static Brush MakeLinearGradientBrush(IReadOnlyList<ARGB> colors)
            {
                Debug.Assert(colors.Count > 0);

                if (colors.Count == 1)
                {
                    return MakeSolidColorBrush(colors[0]);
                }

                LinearGradientBrush brush = new();
                brush.StartPoint = new Point(0, 0);
                brush.EndPoint = new Point(0, 1);

                for (int index = 0; index < colors.Count; index++)
                {
                    Color color = MakeColor(colors[index]);
                    GradientStop stop = new GradientStop(color, index / (float)(colors.Count - 1));
                    brush.GradientStops.Add(stop);
                }

                return brush;
            }

            //Window Colors - Grids need Background set for this to work
            Application.Current.Resources["WindowPrimaryColor"] = MakeSolidColorBrush(theme.WindowPrimaryColor);
            Application.Current.Resources["WindowSecondaryColor"] = MakeSolidColorBrush(theme.WindowSecondaryColor);

            // Button Colors - Set in style. Will work across all buttons automatically
            Application.Current.Resources["ButtonPrimaryColor"] = MakeSolidColorBrush(theme.ButtonPrimaryColor);
            Application.Current.Resources["ButtonSecondaryColor"] = MakeSolidColorBrush(theme.ButtonSecondaryColor);
            Application.Current.Resources["ButtonHoverPrimaryColor"] = MakeSolidColorBrush(theme.ButtonHoverPrimaryColor);
            Application.Current.Resources["ButtonHoverSecondaryColor"] = MakeSolidColorBrush(theme.ButtonHoverSecondaryColor);
            Application.Current.Resources["ButtonIsEnabledPrimaryColor"] = MakeSolidColorBrush(theme.ButtonIsEnabledPrimaryColor);
            Application.Current.Resources["ButtonIsEnabledSecondaryColor"] = MakeSolidColorBrush(theme.ButtonIsEnabledSecondaryColor);

            // Groupbox Colors - Set in style. Will work across all groupboxes automatically
            Application.Current.Resources["GroupboxPrimaryColor"] = MakeSolidColorBrush(theme.GroupboxPrimaryColor);
            Application.Current.Resources["GroupboxSecondaryColor"] = MakeSolidColorBrush(theme.GroupboxSecondaryColor);

            // TabControl Colors - Set in style. Will work across all tab controls automatically
            Application.Current.Resources["TabControlPrimaryColor"] = MakeSolidColorBrush(theme.TabControlPrimaryColor);
            Application.Current.Resources["TabControlSecondaryColor"] = MakeSolidColorBrush(theme.TabControlSecondaryColor);

            // TabItem Colors - Set in style. Will work across all tab items automatically
            // (General_101) I'm replacing the gradient instead of using Dynamic colors in the gradient to work around an issue.
            // (General_101) This should work fine for tab item. 
            Application.Current.Resources["TabItemBackground"] = MakeLinearGradientBrush(theme.TabItemBackground);
            Application.Current.Resources["TabItemTertiaryColor"] = MakeSolidColorBrush(theme.TabItemTertiaryColor);
            Application.Current.Resources["TabItemHoverBackground"] = MakeLinearGradientBrush(theme.TabItemHoverBackground);
            Application.Current.Resources["TabItemSelectedPrimaryColor"] = MakeSolidColorBrush(theme.TabItemSelectedPrimaryColor);
            Application.Current.Resources["TabItemSelectedSecondaryColor"] = MakeSolidColorBrush(theme.TabItemSelectedSecondaryColor);
            Application.Current.Resources["TabItemIsEnabledPrimaryColor"] = MakeSolidColorBrush(theme.TabItemIsEnabledPrimaryColor);
            Application.Current.Resources["TabItemIsEnabledSecondaryColor"] = MakeSolidColorBrush(theme.TabItemIsEnabledSecondaryColor);

            // Slider Colors - Set in style. Will work across all sliders automatically
            Application.Current.Resources["SliderThumbPrimaryColor"] = MakeSolidColorBrush(theme.SliderThumbPrimaryColor);
            Application.Current.Resources["SliderThumbSecondaryColor"] = MakeSolidColorBrush(theme.SliderThumbSecondaryColor);
            Application.Current.Resources["SliderThumbHoverPrimaryColor"] = MakeSolidColorBrush(theme.SliderThumbHoverPrimaryColor);
            Application.Current.Resources["SliderThumbHoverSecondaryColor"] = MakeSolidColorBrush(theme.SliderThumbHoverSecondaryColor);
            Application.Current.Resources["SliderThumbHeldPrimaryColor"] = MakeSolidColorBrush(theme.SliderThumbHeldPrimaryColor);
            Application.Current.Resources["SliderThumbHeldSecondaryColor"] = MakeSolidColorBrush(theme.SliderThumbHeldSecondaryColor);
            Application.Current.Resources["SliderThumbIsEnabledPrimaryColor"] = MakeSolidColorBrush(theme.SliderThumbPrimaryColor);
            Application.Current.Resources["SliderThumbIsEnabledSecondaryColor"] = MakeSolidColorBrush(theme.SliderThumbIsEnabledSecondaryColor);
            Application.Current.Resources["SliderTrackPrimaryColor"] = MakeSolidColorBrush(theme.SliderTrackPrimaryColor);
            Application.Current.Resources["SliderTrackSecondaryColor"] = MakeSolidColorBrush(theme.SliderTrackSecondaryColor);

            // Text Color - Items with text need Foreground set for this to work
            Application.Current.Resources["TextColor"] = MakeSolidColorBrush(theme.TextColor);

            // ComboBox Colors - Set in style. Will work across all comboboxes automatically
            // (General_101) I'm replacing the gradient instead of using Dynamic colors in the gradient to work around an issue.
            // (General_101) This should work fine for comboboxes. 
            Application.Current.Resources["ComboBoxPrimaryColor"] = MakeLinearGradientBrush(theme.ComboBoxPrimaryColor);
            Application.Current.Resources["ComboBoxSecondaryColor"] = MakeSolidColorBrush(theme.ComboBoxSecondaryColor);
            Application.Current.Resources["ComboBoxHoverPrimaryColor"] = MakeLinearGradientBrush(theme.ComboBoxHoverPrimaryColor);
            Application.Current.Resources["ComboBoxHoverSecondaryColor"] = MakeSolidColorBrush(theme.ComboBoxHoverSecondaryColor);
            Application.Current.Resources["ComboBoxListPrimaryColor"] = MakeSolidColorBrush(theme.ComboBoxListPrimaryColor);
            Application.Current.Resources["ComboBoxListSecondaryColor"] = MakeSolidColorBrush(theme.ComboBoxListSecondaryColor);
            Application.Current.Resources["ComboBoxIsEnabledPrimaryColor"] = MakeSolidColorBrush(theme.ComboBoxIsEnabledPrimaryColor);
            Application.Current.Resources["ComboBoxIsEnabledSecondaryColor"] = MakeSolidColorBrush(theme.ComboBoxIsEnabledSecondaryColor);
        }

        public void SetLauncherTheme(ThemeType theme_index)
        {
            SetSolidColorTheme(themes[theme_index]);
        }
    }
}