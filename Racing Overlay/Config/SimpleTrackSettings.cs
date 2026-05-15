namespace RacingOverlay.Models
{
    public class SimpleTrackSettings : UISize
    {
        public SimpleTrackSettings(int sizePreset, int simpleTrackWidth) : base(sizePreset)
        {
            switch (sizePreset)
            {
                case 0:
                    DataFontSize = 10;
                    ContainerHeight = 20;
                    PositionDiameter = 17;
                    PaddingTop = 1;
                    break;
                case 1:
                    DataFontSize = 12;
                    ContainerHeight = 25;
                    PositionDiameter = 21;
                    PaddingTop = 2;
                    break;
                case 2:
                    DataFontSize = 14;
                    ContainerHeight = 30;
                    PositionDiameter = 25;
                    PaddingTop = 3;
                    break;
                default:
                    DataFontSize = 14;
                    ContainerHeight = 30;
                    PositionDiameter = 25;
                    PaddingTop = 3;
                    break;
            }

            ContainerWidth = simpleTrackWidth;
        }
        public int ContainerHeight { get; set; }
        public int ContainerWidth { get; set; }
        public int PositionDiameter { get; set; }
        public int PaddingTop { get; set; }
    }
}
