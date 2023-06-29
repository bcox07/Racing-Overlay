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
