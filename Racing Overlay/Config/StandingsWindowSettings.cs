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
                    TitleFontSize = 13;
                    SubtitleFontSize = 14;
                    DataFontSize = 12;
                    RowHeight = 20;
                    break;
                case 1:
                    WindowWidth = 480;
                    TitleFontSize = 13.5;
                    SubtitleFontSize = 14.5;
                    DataFontSize = 12.5;
                    RowHeight = 20.7;
                    break;
                case 2:
                    WindowWidth = 510;
                    TitleFontSize = 14;
                    SubtitleFontSize = 15;
                    DataFontSize = 13;
                    RowHeight = 21.4;
                    break;
                case 3:
                    WindowWidth = 540;
                    TitleFontSize = 14.5;
                    SubtitleFontSize = 15.5;
                    DataFontSize = 13.5;
                    RowHeight = 22.1;
                    break;
                case 4:
                    WindowWidth = 570;
                    TitleFontSize = 15;
                    SubtitleFontSize = 16;
                    DataFontSize = 14;
                    RowHeight = 22.8;
                    break;
                case 5:
                    WindowWidth = 600;
                    TitleFontSize = 15.5;
                    SubtitleFontSize = 16.5;
                    DataFontSize = 14.5;
                    RowHeight = 23.5;
                    break;
                case 6:
                    WindowWidth = 630;
                    TitleFontSize = 16;
                    SubtitleFontSize = 17;
                    DataFontSize = 15;
                    RowHeight = 24.2;
                    break;
                case 7:
                    WindowWidth = 660;
                    TitleFontSize = 16.5;
                    SubtitleFontSize = 17.5;
                    DataFontSize = 15.5;
                    RowHeight = 24.9;
                    break;
                case 8:
                    WindowWidth = 690;
                    TitleFontSize = 17;
                    SubtitleFontSize = 18;
                    DataFontSize = 16;
                    RowHeight = 25.6;
                    break;
                default:
                    WindowWidth = 450;
                    TitleFontSize = 14;
                    SubtitleFontSize = 14;
                    DataFontSize = 12;
                    RowHeight = 20;
                    break;
            }
        }
    }
}
