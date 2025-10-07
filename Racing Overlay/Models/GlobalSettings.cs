using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RacingOverlay.Models
{

    public class GlobalSettings
    {
        public DriverDisplay DriverDisplay { get; set; }
        public UISize UISize { get; set; }
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
                    case 0:return 16;
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

        public Dictionary<string, int> SimpleTrackSettings
        {
            get
            {
                switch (SizePreset)
                {
                    case 0:
                        return new Dictionary<string, int>
                        {
                            { "FontSize", 10 },
                            { "ContainerHeight", 20 },
                            { "PositionDiameter", 17 },
                            { "PaddingTop", 1 }
                        };
                    case 1:
                        return new Dictionary<string, int>
                        {
                            { "FontSize", 12 },
                            { "ContainerHeight", 25 },
                            { "PositionDiameter", 21 },
                            { "PaddingTop", 2 }
                        };
                    case 2:
                        return new Dictionary<string, int>
                        {
                            { "FontSize", 14 },
                            { "ContainerHeight", 30 },
                            { "PositionDiameter", 25 },
                            { "PaddingTop", 3 }
                        };
                    default:
                        return new Dictionary<string, int>
                        {
                            { "FontSize", 14 },
                            { "ContainerHeight", 30 },
                            { "PositionDiameter", 25 },
                            { "PaddingTop", 3 }
                        };
                }
            }
        }

        public Dictionary<string, int> FuelWindowSettings
        {
            get
            {
                switch (SizePreset)
                {
                    case 0:
                        return new Dictionary<string, int>
                        {
                            { "WindowWidth", 180 },
                            { "WindowHeight", 80 },
                            { "TitleFontSize", 11 },
                            { "DataFontSize", 11 },
                        };
                    case 1:
                        return new Dictionary<string, int>
                        {
                            { "WindowWidth", 220 },
                            { "WindowHeight", 100 },
                            { "TitleFontSize", 14 },
                            { "DataFontSize", 14 },
                        };
                    case 2:
                        return new Dictionary<string, int>
                        {
                            { "WindowWidth", 260 },
                            { "WindowHeight", 120 },
                            { "TitleFontSize", 16 },
                            { "DataFontSize", 16 },
                        };
                    default:
                        return new Dictionary<string, int>
                        {
                            { "WindowWidth", 260 },
                            { "WindowHeight", 120 },
                            { "DataFontSize", 16 },
                        };
                }
            }
        }
    }
}
