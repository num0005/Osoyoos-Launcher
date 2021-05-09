using System.IO;
using System.Windows;
using ToolkitLauncher.ToolkitInterface;

namespace ToolkitLauncher
{
    /// <summary>
    ///     Interaction logic for EditLightmapSettings.xaml
    /// </summary>
    public partial class EditLightmapSettings
    {
        public string LightmapConfigFile => toolkit.GetBaseDirectory() + @"\custom_lightmap_quality.conf";

        private static ToolkitBase toolkit => new H2Toolkit();

        private string[] lines;
        class DefaultLightmapConfigText
        {
            public string line0 = "is_checkboard = false";
            public string line0_t = "is_checkboard = false";
            public string line1 = "is_direct_only = false";
            public string line1_t = "is_direct_only = true";
            public string line2 = "is_draft = false";
            public string line2_t = "is_draft = false";
            public string line3 = "main_monte_carlo_setting = 8";
            public string line4 = "proton_count = 20000000";
            public string line5 = "secondary_monte_carlo_setting = 4";
            public string line6 = "unk7 = 4";
        }
        public EditLightmapSettings()
        {
            if (File.Exists(LightmapConfigFile) == false)
            {
                SaveConfigFile();
            }

            lines = File.ReadAllLines(LightmapConfigFile);

            InitializeComponent();

            txt_sample_count.Text = lines[3].Remove(0, 27);
            txt_photon_count.Text = lines[4].Remove(0, 15);
            txt_AA_sample_count.Text = lines[5].Remove(0, 32);
            txt_gather_distance.Text = lines[6].Remove(0, 7);

            if (lines[0].Remove(0, 16).Contains("true"))
                chk_is_checkboard.IsChecked = true;
            else
                chk_is_checkboard.IsChecked = false;

            if (lines[1].Remove(0, 17).Contains("true"))
                chk_is_direct_only.IsChecked = true;
            else
                chk_is_direct_only.IsChecked = false;

            if (lines[2].Remove(0, 11).Contains("true"))
                chk_is_draft.IsChecked = true;
            else
                chk_is_draft.IsChecked = false;
        }

        public void SaveConfigFile()
        {
            DefaultLightmapConfigText lightmapConfigText = new DefaultLightmapConfigText();

            using (StreamWriter sw = File.CreateText(LightmapConfigFile))
            {
                sw.WriteLine(lightmapConfigText.line0);
                sw.WriteLine(lightmapConfigText.line1);
                sw.WriteLine(lightmapConfigText.line2);
                sw.WriteLine(lightmapConfigText.line3);
                sw.WriteLine(lightmapConfigText.line4);
                sw.WriteLine(lightmapConfigText.line5);
                sw.WriteLine(lightmapConfigText.line6);
            }
        }

        private void btn_save_changes_Click(object sender, RoutedEventArgs e)
        {
            lines = File.ReadAllLines(LightmapConfigFile);

            //Saves the file with new settings based on what the user selected
            if (chk_is_checkboard.IsChecked == true)
                lines[0] = "is_checkboard = true";
            else
                lines[0] = "is_checkboard = false";
            if (chk_is_direct_only.IsChecked == true)
                lines[1] = "is_direct_only = true";
            else
                lines[1] = "is_direct_only = false";
            if (chk_is_draft.IsChecked == true)
                lines[2] = "is_draft = true";
            else
                lines[2] = "is_draft = false";
            lines[3] = "main_monte_carlo_setting = " + txt_sample_count.Text;
            lines[4] = "proton_count = " + txt_photon_count.Text;
            lines[5] = "secondary_monte_carlo_setting = " + txt_AA_sample_count.Text;
            lines[6] = "unk7 = " + txt_gather_distance.Text;

            File.WriteAllLines(LightmapConfigFile, lines);
            Close();
        }

        private void btn_reset_changes_Click(object sender, RoutedEventArgs e)
        {
            txt_sample_count.Text = "8";
            txt_photon_count.Text = "20000000";
            txt_AA_sample_count.Text = "4";
            txt_gather_distance.Text = "4";
            chk_is_checkboard.IsChecked = false;
            chk_is_direct_only.IsChecked = false;
            chk_is_draft.IsChecked = false;
        }
    }
}