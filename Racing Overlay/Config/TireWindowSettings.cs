using System.Collections.Generic;

namespace RacingOverlay.Models
{
    public class TireWindowSettings : UISize
    {
        public TireWindowSettings(int sizePreset) : base(sizePreset)
        {
            switch (sizePreset)
            {
                case 0:
                    WindowWidth = 200;
                    WindowHeight = 100;
                    TitleFontSize = 11;
                    DataFontSize = 11;
                    break;
                case 1:
                    WindowWidth = 220;
                    WindowHeight = 100;
                    TitleFontSize = 12;
                    DataFontSize = 12;
                    break;
                case 2:
                    WindowWidth = 240;
                    WindowHeight = 100;
                    TitleFontSize = 13;
                    DataFontSize = 13;
                    break;
                case 3:
                    WindowWidth = 260;
                    WindowHeight = 100;
                    TitleFontSize = 14;
                    DataFontSize = 14;
                    break;
                case 4:
                    WindowWidth = 280;
                    WindowHeight = 100;
                    TitleFontSize = 15;
                    DataFontSize = 15;
                    break;
                case 5:
                    WindowWidth = 300;
                    WindowHeight = 100;
                    TitleFontSize = 16;
                    DataFontSize = 16;
                    break;
                case 6:
                    WindowWidth = 320;
                    WindowHeight = 100;
                    TitleFontSize = 17;
                    DataFontSize = 17;
                    break;
                default:
                    WindowWidth = 200;
                    WindowHeight = 100;
                    TitleFontSize = 11;
                    DataFontSize = 11;
                    break;
            }
        }
    }
}