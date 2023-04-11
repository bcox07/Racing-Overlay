using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IRacing_Standings
{
    public class WindowSettings
    {
        public Dictionary<string, string> StandingsSettings { get; set; }
        public Dictionary<string, string> RelativeSettings { get; set; }
        public Dictionary<string, string> FuelSettings { get; set; }

        public void GetStoredSettings()
        {
            //Speed speed = null;
            //var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $"\\Speed Files\\{trackId}\\{carPath}.json";
            //if (File.Exists(directory))
            //{
            //    var timesJsonString = File.ReadAllText(directory);
            //    speed = JsonSerializer.Deserialize<Speed>(timesJsonString);
            //    speed.TrackId = trackId;
            //    speed.CarPath = carPath;
            //}
            //return speed;
        }

        public void SaveSettings(string window, Dictionary<string, string> settings)
        {
            switch(window)
            {
                case "Standings":
                    StandingsSettings = settings;
                    break;
                case "Relative":
                    RelativeSettings = settings;
                    break;
                case "Fuel":
                     FuelSettings = settings;
                    break;
            }
        }
    }
}
