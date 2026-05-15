namespace RacingOverlay.Models
{
    public class FullTrackSettings : UISize
    {
        public FullTrackSettings(int sizePreset) : base(sizePreset)
        {
            switch (sizePreset)
            {
                case 0:
                    DataFontSize = 8;
                    PositionDiameter = 18;
                    Percentage = 70;
                    break;
                case 1:
                    DataFontSize = 10;
                    PositionDiameter = 20;
                    Percentage = 100;
                    break;
                case 2:
                    DataFontSize = 12;
                    PositionDiameter = 22;
                    Percentage = 130;
                    break;
                case 3:
                    DataFontSize = 13;
                    PositionDiameter = 24;
                    Percentage = 160;
                    break;
                case 4:
                    DataFontSize = 14;
                    PositionDiameter = 26;
                    Percentage = 190;
                    break;
                case 5:
                    DataFontSize = 15;
                    PositionDiameter = 28;
                    Percentage = 220;
                    break;
                case 6:
                    DataFontSize = 16;
                    PositionDiameter = 30;
                    Percentage = 250;
                    break;
                default:
                    DataFontSize = 10;
                    PositionDiameter = 20;
                    Percentage = 70;
                    break;
            }
        }

        public int PositionDiameter { get; set; }
        public int Percentage { get; set; }
    }
}
