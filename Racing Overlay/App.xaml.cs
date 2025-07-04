using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO.IsolatedStorage;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;
using System.Text.Json;

namespace IRacing_Standings
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
