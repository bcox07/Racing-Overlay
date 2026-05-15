namespace RacingOverlay.Models
{
    public class RelativeWindowSettings : UISize
    {
        public RelativeWindowSettings(int sizePreset) : base(sizePreset)
        {
            switch (sizePreset)
            {
                case 0:
                    WindowWidth = 290;
                    RowHeight = 20;
                    DataFontSize = 11;
                    break;
                case 1:
                    WindowWidth = 310;
                    RowHeight = 21;
                    DataFontSize = 11;
                    break;
                case 2:
                    WindowWidth = 330;
                    RowHeight = 22;
                    DataFontSize = 12;
                    break;
                case 3:
                    WindowWidth = 350;
                    RowHeight = 23;
                    DataFontSize = 13;
                    break;
                case 4:
                    WindowWidth = 370;
                    RowHeight = 24;
                    DataFontSize = 14;
                    break;
                case 5:
                    WindowWidth = 390;
                    RowHeight = 25;
                    DataFontSize = 15;
                    break;
                case 6:
                    WindowWidth = 410;
                    RowHeight = 26;
                    DataFontSize = 16;
                    break;
                default:
                    WindowWidth = 290;
                    RowHeight = 20;
                    DataFontSize = 11;
                    break;
            }
        }

        public int FontSize { get; set; }
        public int PositionDiameter { get; set; }
    }
}
