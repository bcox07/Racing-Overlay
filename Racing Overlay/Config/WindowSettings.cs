using System.Collections.Generic;
using System.Configuration;

namespace RacingOverlay
{
    public class WindowSettings
    {
        public Dictionary<string, string> GlobalSettings { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> StandingsSettings { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> RelativeSettings { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> FuelSettings { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> TireSettings { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> SimpleTrackSettings { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> FullTrackSettings { get; set; } = new Dictionary<string, string>();

        public WindowSettings(AppSettingsSection appSettings = null)
        {
            if (appSettings != null)
            {
                GlobalSettings = new Dictionary<string, string>()
                {
                    { "DriverCount", appSettings.Settings["DriverCount"].Value }
                };
                StandingsSettings = new Dictionary<string, string>()
                {
                    { "Size", appSettings.Settings["StandingsWindowSize"].Value },
                    { "Opacity", appSettings.Settings["StandingsWindowOpacity"].Value },
                    { "Locked", appSettings.Settings["StandingsWindowLocked"].Value },
                    { "XPos", appSettings.Settings["StandingsWindowXPos"].Value },
                    { "YPos", appSettings.Settings["StandingsWindowYPos"].Value },
                    { "Visible", appSettings.Settings["StandingsWindowVisible"].Value }
                };
                RelativeSettings = new Dictionary<string, string>()
                {
                    { "Size", appSettings.Settings["RelativeWindowSize"].Value },
                    { "Opacity", appSettings.Settings["RelativeWindowOpacity"].Value },
                    { "Locked", appSettings.Settings["RelativeWindowLocked"].Value },
                    { "XPos", appSettings.Settings["RelativeWindowXPos"].Value },
                    { "YPos", appSettings.Settings["RelativeWindowYPos"].Value },
                    { "Visible", appSettings.Settings["RelativeWindowVisible"].Value }
                };
                FuelSettings = new Dictionary<string, string>()
                {
                    { "Size", appSettings.Settings["FuelWindowSize"].Value },
                    { "Opacity", appSettings.Settings["FuelWindowOpacity"].Value },
                    { "Measurement", appSettings.Settings["FuelWindowMeasurement"].Value },
                    { "Locked", appSettings.Settings["FuelWindowLocked"].Value },
                    { "XPos", appSettings.Settings["FuelWindowXPos"].Value },
                    { "YPos", appSettings.Settings["FuelWindowYPos"].Value },
                    { "Visible", appSettings.Settings["FuelWindowVisible"].Value }
                };
                TireSettings = new Dictionary<string, string>()
                {
                    { "Size", appSettings.Settings["TireWindowSize"].Value },
                    { "Opacity", appSettings.Settings["TireWindowOpacity"].Value },
                    { "Locked", appSettings.Settings["TireWindowLocked"].Value },
                    { "XPos", appSettings.Settings["TireWindowXPos"].Value },
                    { "YPos", appSettings.Settings["TireWindowYPos"].Value },
                    { "Visible", appSettings.Settings["TireWindowVisible"].Value }
                };
                SimpleTrackSettings = new Dictionary<string, string>()
                {
                    { "Size", appSettings.Settings["SimpleTrackWindowSize"].Value },
                    { "Opacity", appSettings.Settings["SimpleTrackWindowOpacity"].Value },
                    { "Locked", appSettings.Settings["SimpleTrackWindowLocked"].Value },
                    { "XPos", appSettings.Settings["SimpleTrackWindowXPos"].Value },
                    { "YPos", appSettings.Settings["SimpleTrackWindowYPos"].Value },
                    { "Visible", appSettings.Settings["SimpleTrackWindowVisible"].Value },
                    { "Width", appSettings.Settings["SimpleTrackWindowWidth"].Value }
                };
                FullTrackSettings = new Dictionary<string, string>()
                {
                    { "Size", appSettings.Settings["FullTrackWindowSize"].Value },
                    { "Opacity", appSettings.Settings["FullTrackWindowOpacity"].Value },
                    { "Locked", appSettings.Settings["FullTrackWindowLocked"].Value },
                    { "XPos", appSettings.Settings["FullTrackWindowXPos"].Value },
                    { "YPos", appSettings.Settings["FullTrackWindowYPos"].Value },
                    { "Visible", appSettings.Settings["FullTrackWindowVisible"].Value }
                };
            }

        }
    }
}
