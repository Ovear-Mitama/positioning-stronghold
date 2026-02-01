using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace positioning_stronghold
{
    public partial class SettingsWindow : Window
    {
        private bool _isLoaded = false;
        private Pages.BasicPage _basicPage;
        private Pages.AboutPage _aboutPage;

        public SettingsWindow()
        {
            InitializeComponent();
            Loaded += SettingsWindow_Loaded;
            Closing += SettingsWindow_Closing;
            Activated += SettingsWindow_Activated;
        }

        private void SettingsWindow_Activated(object sender, EventArgs e)
        {
            if (Topmost)
            {
                Topmost = false;
                Topmost = true;
            }
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _basicPage = new Pages.BasicPage();
            _aboutPage = new Pages.AboutPage();
            ContentFrame.Content = _basicPage;
            _isLoaded = true;
            BasicNav.IsChecked = true;
            
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _basicPage?.LoadSettings();
                _basicPage?.SyncWindowState();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void SettingsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_basicPage != null)
            {
                _basicPage.SaveSettings();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Navigation_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded || ContentFrame == null)
                return;

            if (sender is RadioButton radioButton)
            {
                if (radioButton == BasicNav && _basicPage != null)
                {
                    ContentFrame.Content = _basicPage;
                }
                else if (radioButton == AboutNav && _aboutPage != null)
                {
                    ContentFrame.Content = _aboutPage;
                }
            }
        }
    }
}