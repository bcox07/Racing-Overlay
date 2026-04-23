namespace RacingOverlay.Models
{
    public class FullTrackSettings
    {
        public FullTrackSettings(int globalSizePreset)
        {
            switch (globalSizePreset)
            {
                case 0:
                    FontSize = 8;
                    PositionDiameter = 18;
                    break;
                case 1:
                    FontSize = 10;
                    PositionDiameter = 20;
                    break;
                case 2:
                    FontSize = 12;
                    PositionDiameter = 22;
                    break;
                case 3:
                    FontSize = 13;
                    PositionDiameter = 24;
                    break;
                case 4:
                    FontSize = 14;
                    PositionDiameter = 26;
                    break;
                case 5:
                    FontSize = 15;
                    PositionDiameter = 28;
                    break;
                case 6:
                    FontSize = 16;
                    PositionDiameter = 30;
                    break;
                default:
                    FontSize = 10;
                    PositionDiameter = 20;
                    break;
            }
        }

        public int FontSize { get; set; }
        public int PositionDiameter { get; set; }
    }
}
