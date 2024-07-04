using System.Windows;
using System.Windows.Media;

namespace ToolkitLauncher.UI
{

    // todo(num0005) move the colour config into a JSON file
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
                case ThemeType.darkblue:
                case ThemeType.darkpurple:
                case ThemeType.darkorange:
                case ThemeType.darkgreen:
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
