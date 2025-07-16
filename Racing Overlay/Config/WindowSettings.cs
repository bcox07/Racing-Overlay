using System;
using System.Collections.Generic;
using System.Configuration;
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

        public WindowSettings(AppSettingsSection appSettings = null)
        {
            if (appSettings != null)
            {
                StandingsSettings = new Dictionary<string, string>()
                {
                    { "Locked", appSettings.Settings["StandingsWindowLocked"].Value },
                    { "XPos", appSettings.Settings["StandingsWindowXPos"].Value },
                    { "YPos", appSettings.Settings["StandingsWindowYPos"].Value }
                };
                RelativeSettings = new Dictionary<string, string>()
                {
                    { "Locked", appSettings.Settings["RelativeWindowLocked"].Value },
                    { "XPos", appSettings.Settings["RelativeWindowXPos"].Value },
                    { "YPos", appSettings.Settings["RelativeWindowYPos"].Value }
                };
                FuelSettings = new Dictionary<string, string>()
                {
                    { "Locked", appSettings.Settings["FuelWindowLocked"].Value },
                    { "XPos", appSettings.Settings["FuelWindowXPos"].Value },
                    { "YPos", appSettings.Settings["FuelWindowYPos"].Value }
                };
                TireSettings = new Dictionary<string, string>()
                {
                    { "Locked", appSettings.Settings["LiveTrackWindowLocked"].Value },
                    { "XPos", appSettings.Settings["TireWindowXPos"].Value },
                    { "YPos", appSettings.Settings["TireWindowYPos"].Value }
                };
                LiveTrackSettings = new Dictionary<string, string>()
                {
                    { "Locked", appSettings.Settings["LiveTrackWindowLocked"].Value },
                    { "XPos", appSettings.Settings["LiveTrackWindowXPos"].Value },
                    { "YPos", appSettings.Settings["LiveTrackWindowYPos"].Value }
                };
            }
           
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
