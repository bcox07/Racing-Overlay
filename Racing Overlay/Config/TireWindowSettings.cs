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
                    WindowWidth = 100;
                    TitleFontSize = 11;
                    DataFontSize = 11;
                    break;
                case 1:
                    WindowWidth = 115;
                    TitleFontSize = 12.5;
                    DataFontSize = 12.5;
                    break;
                case 2:
                    WindowWidth = 130;
                    TitleFontSize = 15;
                    DataFontSize = 14;
                    break;
                case 3:
                    WindowWidth = 145;
                    TitleFontSize = 17;
                    DataFontSize = 15.5;
                    break;
                case 4:
                    WindowWidth = 160;
                    TitleFontSize = 19;
                    DataFontSize = 17;
                    break;
                case 5:
                    WindowWidth = 175;
                    TitleFontSize = 21;
                    DataFontSize = 18.5;
                    break;
                case 6:
                    WindowWidth = 190;
                    TitleFontSize = 23;
                    DataFontSize = 20;
                    break;
                default:
                    WindowWidth = 100;
                    TitleFontSize = 11;
                    DataFontSize = 11;
                    break;
            }
        }
    }
}