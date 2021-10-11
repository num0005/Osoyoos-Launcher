using System.Windows;

namespace ToolkitLauncher
{
    /// <summary>
    /// Interaction logic for GeoClassPrompt.xaml
    /// </summary>
    public partial class GeoClassPrompt : Window
    {
        public GeoClassPrompt()
        {
            InitializeComponent();
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
