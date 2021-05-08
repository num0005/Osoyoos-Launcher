using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using ToolkitLauncher.Properties;
using ToolkitLauncher.ToolkitInterface;


namespace ToolkitLauncher
{

    /// <summary>
    /// Interaction logic for EditLightmapSettings.xaml
    /// </summary>
    public partial class EditLightmapSettings : Window
    {
        ToolkitBase[] toolkits = new ToolkitBase[]
        {
            new H1Toolkit(),
            new H2Toolkit()
        };
        ToolkitBase toolkit
        {
            get
            {
                return toolkits[1];
            }
        }

        public EditLightmapSettings()
        {
            string lightmap_config = (toolkit.GetBaseDirectory() + @"\custom_lightmap_quality.conf");
            bool file_exists = File.Exists(lightmap_config);
            string[] lines;
            if (file_exists == false)
            {
                using (StreamWriter sw = File.CreateText(lightmap_config))
                {
                    sw.WriteLine("is_checkboard = false");
                    sw.WriteLine("is_direct_only = false");
                    sw.WriteLine("is_draft = false");
                    sw.WriteLine("main_monte_carlo_setting = 8");
                    sw.WriteLine("proton_count = 20000000");
                    sw.WriteLine("secondary_monte_carlo_setting = 4");
                    sw.WriteLine("unk7 = 4");

                }
            }
            lines = File.ReadAllLines(lightmap_config);

            InitializeComponent();

            txt_sample_count.Text = lines[3].Remove(0, 27);
            txt_photon_count.Text = lines[4].Remove(0, 15);
            txt_AA_sample_count.Text = lines[5].Remove(0, 32);
            txt_gather_distance.Text = lines[6].Remove(0, 7);

            if (lines[0].Remove(0, 16).Contains("true"))
            {
                chk_is_checkboard.IsChecked = true;
            }
            else
            {
                chk_is_checkboard.IsChecked = false;
            }
            if (lines[1].Remove(0, 17).Contains("true"))
            {
                chk_is_direct_only.IsChecked = true;
            }
            else
            {
                chk_is_direct_only.IsChecked = false;
            }
            if (lines[2].Remove(0, 11).Contains("true"))
            {
                chk_is_draft.IsChecked = true;
            }
            else
            {
                chk_is_draft.IsChecked = false;
            }
        }

        private void btn_save_changes_Click(object sender, RoutedEventArgs e)
        {
            string lightmap_config = (toolkit.GetBaseDirectory() + @"\custom_lightmap_quality.conf");
            string[] lines = File.ReadAllLines(lightmap_config);

            //Saves the file with new settings based on what the user selected
            if (chk_is_checkboard.IsChecked == true)
            {
                lines[0] = "is_checkboard = true";
            }
            else
            {
                lines[0] = "is_checkboard = false";
            }
            if (chk_is_direct_only.IsChecked == true)
            {
                lines[1] = "is_direct_only = true";
            }
            else
            {
                lines[1] = "is_direct_only = false";
            }
            if (chk_is_draft.IsChecked == true)
            {
                lines[2] = "is_draft = true";
            }
            else
            {
                lines[2] = "is_draft = false";
            }
            lines[3] = "main_monte_carlo_setting = " + txt_sample_count.Text;
            lines[4] = "proton_count = " + txt_photon_count.Text;
            lines[5] = "secondary_monte_carlo_setting = " + txt_AA_sample_count.Text;
            lines[6] = "unk7 = " + txt_gather_distance.Text;

            File.WriteAllLines(lightmap_config, lines);
            this.Close();
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
