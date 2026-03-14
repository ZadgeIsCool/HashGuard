using System;
using System.Windows;

namespace HashGuard
{
    /// <summary>
    /// Application entry point. Manages theme switching and global error handling.
    /// </summary>
    public partial class App : Application
    {
        private bool _isDarkTheme;

        /// <summary>
        /// Handles application startup and sets up global exception handling.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Global exception handler to prevent silent crashes
            DispatcherUnhandledException += (s, args) =>
            {
                MessageBox.Show(
                    $"An unexpected error occurred:\n\n{args.Exception.Message}\n\nPlease report this issue.",
                    "HashGuard Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                args.Handled = true;
            };
        }

        /// <summary>
        /// Toggles between dark and light application themes.
        /// </summary>
        public void ToggleTheme()
        {
            _isDarkTheme = !_isDarkTheme;
            string themeUri = _isDarkTheme
                ? "Themes/DarkTheme.xaml"
                : "Themes/LightTheme.xaml";

            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri(themeUri, UriKind.Relative)
            });
        }
    }
}
