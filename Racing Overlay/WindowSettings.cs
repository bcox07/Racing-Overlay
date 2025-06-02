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
        public Dictionary<string, string> StandingsSettings { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> RelativeSettings { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> FuelSettings { get; set; } = new Dictionary<string, string>
        {
            { "Locked", "false" },
            { "XPos", "0" },
            { "YPos", "0" }
        };
        public Dictionary<string, string> TireSettings { get; set; } = new Dictionary<string, string>
        {
            { "Locked", "false" },
            { "XPos", "0" },
            { "YPos", "0" }
        };

        public Dictionary<string, string> LiveTrackSettings { get; set; } = new Dictionary<string, string>
        {
            { "Locked", "false" },
            { "XPos", "0" },
            { "YPos", "0" }
        };

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
                case "Tires":
                    TireSettings = settings;
                    break;
                case "LiveTrack":
                    LiveTrackSettings = settings;
                    break;
            }
        }
    }
}
