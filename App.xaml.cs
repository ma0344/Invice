using System.Configuration;
using System.Data;
using System.Windows;
using System.Text;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Input;

namespace Invoice
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public App()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Load the main window
            MainWindow mainWindow = new();
            mainWindow.Show();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            EventManager.RegisterClassHandler(typeof(TextBox),
                TextBox.GotFocusEvent,
                new RoutedEventHandler(TextBox_GotFocus));
            EventManager.RegisterClassHandler(typeof(TextBox),
                TextBox.LostFocusEvent,
                new RoutedEventHandler(TextBox_LostFocus));
            EventManager.RegisterClassHandler(typeof(TextBox),
                TextBox.PreviewMouseUpEvent,
                new RoutedEventHandler(TextBox_PreviewMouseUp));

            base.OnStartup(e);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)

        {
            // Select all text only if the mouse isn't down.
            // This makes tabbing to the textbox select all.
            if (sender is TextBox textBox)
            {
                if (Mouse.LeftButton == MouseButtonState.Released)
                {
                    textBox.SelectAll();
                    textBox.Tag = true; //use the tag propety to signal that the box is already focused
                }
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.SelectionLength = 0;
                textBox.Tag = false; //use the tag propety to signal that the box is already focused
            }
        }

        private void TextBox_PreviewMouseUp(object sender, RoutedEventArgs e)

        {
            // If a user clicked in, want to select all text, unless they made a different selection...
            // so select all only if the textbox isn't already focused, and the user hasn't selected any text.
            if (sender is TextBox textBox)
            {
                if ((textBox.Tag == null || (bool)textBox.Tag == false) && textBox.SelectionLength == 0)
                {
                    textBox.Tag = true; //use the tag propety to signal that the box is already focused
                    textBox.SelectAll();
                }
            }
        }
    }

}
