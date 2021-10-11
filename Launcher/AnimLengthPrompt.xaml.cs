using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ToolkitLauncher
{
    /// <summary>
    /// Interaction logic for AnimLengthPrompt.xaml
    /// </summary>
    public partial class AnimLengthPrompt : Window
    {
        public AnimLengthPrompt()
        {
            InitializeComponent();
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
    }
}
