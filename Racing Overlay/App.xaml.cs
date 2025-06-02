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
        string fileName = "WindowSettings.json";
        WindowSettings defaultSettings = new WindowSettings()
        {
            StandingsSettings = new Dictionary<string, string>() { { "XPos", "0" }, { "YPos", "0" }, { "Locked", "false" } },
            RelativeSettings = new Dictionary<string, string>() { { "XPos", "0" }, { "YPos", "0" }, { "Locked", "false" } },
            FuelSettings = new Dictionary<string, string>() { { "XPos", "0" }, { "YPos", "0" }, { "Locked", "false" } },
            TireSettings = new Dictionary<string, string>() { { "XPos", "0" }, { "YPos", "0" }, { "Locked", "false" } },
            LiveTrackSettings = new Dictionary<string, string>() { { "XPos", "0" }, { "YPos", "0" }, { "Locked", "false" } }
        };
        WindowSettings settings;

        public App()
        {

        }
        private void App_Startup(object sender, StartupEventArgs e)
        {
            var storage = IsolatedStorageFile.GetUserStoreForDomain();

            if (!storage.FileExists(fileName))
            {
                storage.CreateFile(fileName);
                return;
            }
            else
            {
                try
                {
                    using (IsolatedStorageFileStream stream = new IsolatedStorageFileStream(fileName, FileMode.Open, storage))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            if (reader.EndOfStream)
                            {
                                settings = defaultSettings;
                            }
                            else
                            {
                                var settingsJsonString = reader.ReadToEnd();
                                settings = JsonSerializer.Deserialize<WindowSettings>(settingsJsonString);
                            }
                            
                        }
                    }
                }
                catch (FileNotFoundException ex)
                {
                }
            }
            var mainWindow = new MainWindow(settings);
            mainWindow.Show();
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            var storage = IsolatedStorageFile.GetUserStoreForDomain();
            using (var stream = new IsolatedStorageFileStream(fileName, FileMode.Create, storage))
            using (var writer = new StreamWriter(stream))
            {
                var options = new JsonSerializerOptions() { WriteIndented = true };
                writer.Write(JsonSerializer.Serialize(settings, options));
            }
        }
    }
}
