using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace positioning_stronghold.Pages
{
    public partial class BasicPage : UserControl
    {
        private Window _mainWindow;
        private Window _settingsWindow;

        public BasicPage()
        {
            InitializeComponent();
            Loaded += BasicPage_Loaded;
        }

        private void BasicPage_Loaded(object sender, RoutedEventArgs e)
        {
            _mainWindow = Application.Current.MainWindow;
            _settingsWindow = Window.GetWindow(this);
        }

        public void LoadSettings()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\PositioningStronghold");
                if (key != null)
                {
                    var topmost = key.GetValue("Topmost");
                    var transparent = key.GetValue("Transparent");

                    TopmostCheckBox.IsChecked = topmost == null || topmost.ToString().ToLower() == "true" || topmost.ToString() == "True";
                    TransparentCheckBox.IsChecked = transparent != null && (transparent.ToString().ToLower() == "true" || transparent.ToString() == "True");

                    key.Close();
                }
            }
            catch
            {
                TopmostCheckBox.IsChecked = true;
                TransparentCheckBox.IsChecked = false;
            }

            SyncWindowState();
        }

        public void SyncWindowState()
        {
            if (_mainWindow != null)
            {
                _mainWindow.Topmost = TopmostCheckBox.IsChecked == true;
                _mainWindow.Opacity = TransparentCheckBox.IsChecked == true ? 0.8 : 1.0;
            }
            if (_settingsWindow != null)
            {
                _settingsWindow.Topmost = TopmostCheckBox.IsChecked == true;
            }
        }

        public void SaveSettings()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\PositioningStronghold");
                if (key != null)
                {
                    key.SetValue("Topmost", TopmostCheckBox?.IsChecked == true);
                    key.SetValue("Transparent", TransparentCheckBox?.IsChecked == true);
                    key.Close();
                }
            }
            catch
            {
            }
        }

        private void Topmost_Checked(object sender, RoutedEventArgs e)
        {
            if (_mainWindow != null)
            {
                _mainWindow.Topmost = true;
            }
            if (_settingsWindow != null)
            {
                _settingsWindow.Topmost = true;
            }
            SaveSettings();
        }

        private void Topmost_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_mainWindow != null)
            {
                _mainWindow.Topmost = false;
            }
            if (_settingsWindow != null)
            {
                _settingsWindow.Topmost = false;
            }
            SaveSettings();
        }

        private void Transparent_Checked(object sender, RoutedEventArgs e)
        {
            if (_mainWindow != null)
            {
                _mainWindow.Opacity = 0.8;
            }
            SaveSettings();
        }

        private void Transparent_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_mainWindow != null)
            {
                _mainWindow.Opacity = 1.0;
            }
            SaveSettings();
        }
    }
}