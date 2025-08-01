using System.Collections.Generic;
using System.Configuration;

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

        public Dictionary<string, string> SimpleTrackSettings { get; set; } = new Dictionary<string, string>
        {
            { "Locked", "false" },
            { "XPos", "0" },
            { "YPos", "0" }
        };

        public Dictionary<string, string> FullTrackSettings { get; set; } = new Dictionary<string, string>
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
                    { "Locked", appSettings.Settings["TireWindowLocked"].Value },
                    { "XPos", appSettings.Settings["TireWindowXPos"].Value },
                    { "YPos", appSettings.Settings["TireWindowYPos"].Value }
                };
                SimpleTrackSettings = new Dictionary<string, string>()
                {
                    { "Locked", appSettings.Settings["SimpleTrackWindowLocked"].Value },
                    { "XPos", appSettings.Settings["SimpleTrackWindowXPos"].Value },
                    { "YPos", appSettings.Settings["SimpleTrackWindowYPos"].Value }
                };
                FullTrackSettings = new Dictionary<string, string>()
                {
                    { "Locked", appSettings.Settings["FullTrackWindowLocked"].Value },
                    { "XPos", appSettings.Settings["FullTrackWindowXPos"].Value },
                    { "YPos", appSettings.Settings["FullTrackWindowYPos"].Value }
                };
            }
           
        }
    }
}
