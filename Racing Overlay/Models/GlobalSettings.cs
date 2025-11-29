using System.Collections.Generic;

namespace RacingOverlay.Models
{

    public class GlobalSettings
    {
        public DriverDisplay DriverDisplay { get; set; }
        public UISize UISize { get; set; }
        public SimpleTrackSettings SimpleTrackSettings { get; set; }

        public string PrimaryColor = "#280f1d";
        public string SecondaryColor = "#521439";
    }

    public class DriverDisplay
    {
        public DriverDisplay(int count)
        {
            DisplayCount = count;
        }

        public int DisplayCount { get; set; }
    }

    public class UISize
    {
        public UISize(int sizePreset)
        {
            SizePreset = sizePreset;
        }

        // 0 = Small
        // 1 = Medium
        // 2 = Large
        public int SizePreset { get; set; }
        public int TitleFontSize
        {
            get
            {
                switch (SizePreset)
                {
                    case 0: return 16;
                    case 1: return 17;
                    case 2: return 18;
                    default: return 18;
                }
            }
        }
        public int SubtitleFontSize
        {
            get
            {
                switch (SizePreset)
                {
                    case 0: return 15;
                    case 1: return 16;
                    case 2: return 17;
                    default: return 17;
                }
            }
        }

        public int DataFontSize
        {
            get
            {
                switch (SizePreset)
                {
                    case 0: return 12;
                    case 1: return 13;
                    case 2: return 14;
                    default: return 14;
                }
            }
        }
        public int RowHeight
        {
            get
            {
                switch (SizePreset)
                {
                    case 0: return 20;
                    case 1: return 22;
                    case 2: return 25;
                    default: return 25;
                }
            }
        }

        public int StandingsWindowWidth
        {
            get
            {
                switch (SizePreset)
                {
                    case 0: return 450;
                    case 1: return 500;
                    case 2: return 550;
                    default: return 550;
                }
            }
        }
        public int RelativeWindowWidth
        {
            get
            {
                switch (SizePreset)
                {
                    case 0: return 290;
                    case 1: return 320;
                    case 2: return 350;
                    default: return 350;
                }
            }
        }

        public int Percentage
        {
            get
            {
                switch (SizePreset)
                {
                    case 0: return 80;
                    case 1: return 100;
                    case 2: return 120;
                    default: return 120;
                }
            }
        }

        public SimpleTrackSettings SimpleTrackSettings => new SimpleTrackSettings(SizePreset, 0);

        public Dictionary<string, int> FuelWindowSettings
        {
            get
            {
                switch (SizePreset)
                {
                    case 0:
                        return new Dictionary<string, int>
                        {
                            { "WindowWidth", 200 },
                            { "WindowHeight", 100 },
                            { "TitleFontSize", 11 },
                            { "DataFontSize", 11 },
                        };
                    case 1:
                        return new Dictionary<string, int>
                        {
                            { "WindowWidth", 240 },
                            { "WindowHeight", 120 },
                            { "TitleFontSize", 14 },
                            { "DataFontSize", 14 },
                        };
                    case 2:
                        return new Dictionary<string, int>
                        {
                            { "WindowWidth", 280 },
                            { "WindowHeight", 140 },
                            { "TitleFontSize", 16 },
                            { "DataFontSize", 16 },
                        };
                    default:
                        return new Dictionary<string, int>
                        {
                            { "WindowWidth", 280 },
                            { "WindowHeight", 140 },
                            { "TitleFontSize", 16 },
                            { "DataFontSize", 16 },
                        };
                }
            }
        }
    }
}
