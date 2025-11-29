using System.Configuration;
using System.Windows;

namespace RacingOverlay
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {

        }
        private void App_Startup(object sender, StartupEventArgs e)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var mainWindow = new MainWindow(config);
            mainWindow.Show();
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
        }
    }
}
