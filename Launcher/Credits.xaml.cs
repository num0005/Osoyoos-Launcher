using System.Reflection;
using System.Windows;

namespace ToolkitLauncher
{
    /// <summary>
    /// Interaction logic for PathSettings.xaml
    /// </summary>
    public partial class Credits : Window
    {
        public Credits()
        {
            InitializeComponent();
            version.Text = Assembly.GetEntryAssembly()
                .GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
        }
    }
}
