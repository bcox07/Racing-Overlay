namespace RacingOverlay.Models
{
    public class StandingsWindowSettings : UISize
    {
        public StandingsWindowSettings(int sizePreset) : base(sizePreset)
        {
            switch (sizePreset)
            {
                case 0:
                    WindowWidth = 450;
                    TitleFontSize = 16;
                    SubtitleFontSize = 15;
                    DataFontSize = 12;
                    RowHeight = 20;
                    break;
                case 1:
                    WindowWidth = 500;
                    TitleFontSize = 17;
                    SubtitleFontSize = 16;
                    DataFontSize = 13;
                    RowHeight = 21;
                    break;
                case 2:
                    WindowWidth = 550;
                    TitleFontSize = 18;
                    SubtitleFontSize = 17;
                    DataFontSize = 14;
                    RowHeight = 22;
                    break;
                case 3:
                    WindowWidth = 600;
                    TitleFontSize = 19;
                    SubtitleFontSize = 18;
                    DataFontSize = 15;
                    RowHeight = 23;
                    break;
                case 4:
                    WindowWidth = 650;
                    TitleFontSize = 20;
                    SubtitleFontSize = 19;
                    DataFontSize = 16;
                    RowHeight = 24;
                    break;
                case 5:
                    WindowWidth = 700;
                    TitleFontSize = 21;
                    SubtitleFontSize = 20;
                    DataFontSize = 17;
                    RowHeight = 25;
                    break;
                case 6:
                    WindowWidth = 750;
                    TitleFontSize = 22;
                    SubtitleFontSize = 21;
                    DataFontSize = 18;
                    RowHeight = 26;
                    break;
                default:
                    WindowWidth = 450;
                    TitleFontSize = 16;
                    SubtitleFontSize = 15;
                    DataFontSize = 12;
                    RowHeight = 20;
                    break;
            }
        }
    }
}
