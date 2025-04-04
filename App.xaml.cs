using System.Configuration;
using System.Data;
using System.Windows;

namespace Invoice
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Load the main window
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }

}
