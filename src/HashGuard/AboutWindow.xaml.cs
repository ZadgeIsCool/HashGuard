using System.Windows;

namespace HashGuard
{
    /// <summary>
    /// About dialog displaying application information and license.
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
