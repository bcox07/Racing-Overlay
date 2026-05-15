using System.Collections.Generic;

namespace RacingOverlay.Models
{
    public class FuelWindowSettings : UISize
    {
        public FuelWindowSettings(int sizePreset) : base(sizePreset)
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
                    WindowWidth = 230;
                    WindowHeight = 115;
                    TitleFontSize = 12;
                    DataFontSize = 12;
                    break;
                case 2:
                    WindowWidth = 260;
                    WindowHeight = 130;
                    TitleFontSize = 13;
                    DataFontSize = 13;
                    break;
                case 3:
                    WindowWidth = 290;
                    WindowHeight = 145;
                    TitleFontSize = 14;
                    DataFontSize = 14;
                    break;
                case 4:
                    WindowWidth = 320;
                    WindowHeight = 160;
                    TitleFontSize = 15;
                    DataFontSize = 15;
                    break;
                case 5:
                    WindowWidth = 350;
                    WindowHeight = 175;
                    TitleFontSize = 16;
                    DataFontSize = 16;
                    break;
                case 6:
                    WindowWidth = 380;
                    WindowHeight = 180;
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