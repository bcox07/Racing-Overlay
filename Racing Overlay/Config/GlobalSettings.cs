using System.Collections.Generic;
using System.Windows.Media;

namespace RacingOverlay.Models
{

    public class GlobalSettings
    {
        public DriverDisplay DriverDisplay { get; set; }
        public StandingsWindowSettings StandingsSettings { get; set; }
        public RelativeWindowSettings RelativeWindowSettings { get; set; }
        public FuelWindowSettings FuelWindowSettings { get; set; }
        public TireWindowSettings TireWindowSettings { get; set; }
        public SimpleTrackSettings SimpleTrackSettings { get; set; }
        public FullTrackSettings FullTrackSettings { get; set; }

        public string MenuPrimaryColor = "#641C47";
        public string PrimaryColor = "#280f1d";
        public string SecondaryColor = "#521439";
        private string PrimaryTextColor = "#C6C6C6";

        public Brush MenuPrimaryColorBrush => (SolidColorBrush)new BrushConverter().ConvertFrom(MenuPrimaryColor);
        public Brush PrimaryColorBrush => (SolidColorBrush)new BrushConverter().ConvertFrom(PrimaryColor);
        public Brush SecondaryColorBrush => (SolidColorBrush)new BrushConverter().ConvertFrom(SecondaryColor);
        public Brush PrimaryTextColorBrush => (SolidColorBrush)new BrushConverter().ConvertFrom(PrimaryTextColor);
    }

    public class DriverDisplay
    {
        public DriverDisplay(int count)
        {
            DisplayCount = count;
        }

        public int DisplayCount { get; set; }
    }

    public class UISize
    {
        public UISize(int sizePreset)
        {
            SizePreset = sizePreset;
        }

        public int SizePreset { get; set; }
        public virtual double TitleFontSize { get; set; }
        public virtual double SubtitleFontSize { get; set; }
        public virtual double DataFontSize { get; set; }
        public virtual int RowHeight { get; set; }
        public virtual int WindowWidth { get; set; }
        public virtual int WindowHeight { get; set; }

    }
}
