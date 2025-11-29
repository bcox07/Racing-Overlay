namespace RacingOverlay.Models
{
    public class SimpleTrackSettings
    {
        public SimpleTrackSettings(int globalSizePreset, int simpleTrackWidth)
        {
            switch (globalSizePreset)
            {
                case 0:
                    FontSize = 10;
                    ContainerHeight = 20;
                    PositionDiameter = 17;
                    PaddingTop = 1;
                    break;
                case 1:
                    FontSize = 12;
                    ContainerHeight = 25;
                    PositionDiameter = 21;
                    PaddingTop = 2;
                    break;
                case 2:
                    FontSize = 14;
                    ContainerHeight = 30;
                    PositionDiameter = 25;
                    PaddingTop = 3;
                    break;
                default:
                    FontSize = 14;
                    ContainerHeight = 30;
                    PositionDiameter = 25;
                    PaddingTop = 3;
                    break;
            }

            ContainerWidth = simpleTrackWidth;
        }

        public int FontSize { get; set; }
        public int ContainerHeight { get; set; }
        public int ContainerWidth { get; set; }
        public int PositionDiameter { get; set; }
        public int PaddingTop { get; set; }
    }
}
