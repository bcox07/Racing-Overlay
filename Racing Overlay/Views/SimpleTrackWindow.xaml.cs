using RacingOverlay.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RacingOverlay.Windows
{
    /// <summary>
    /// Interaction logic for SimpleTrackWindow.xaml
    /// </summary>
    /// 

    public partial class SimpleTrackWindow : Window
    {
        TelemetryData LocalTelemetry;
        public bool Locked = false;
        private GlobalSettings _GlobalSettings;
        public SimpleTrackWindow(TelemetryData telemetryData, GlobalSettings globalSettings)
        {
            _GlobalSettings = globalSettings;
            LocalTelemetry = telemetryData;
            InitializeComponent();
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            Locked = bool.Parse(mainWindow.WindowSettings.SimpleTrackSettings["Locked"] ?? "false");
            Left = double.Parse(mainWindow.WindowSettings.SimpleTrackSettings["XPos"] ?? "0");
            Top = double.Parse(mainWindow.WindowSettings.SimpleTrackSettings["YPos"] ?? "0");
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!Locked)
            {
                base.OnMouseLeftButtonDown(e);
                DragMove();
            }
        }

        public void UpdateTelemetryData(TelemetryData telemetryData)
        {
            Dispatcher.Invoke(() =>
            {
                Topmost = false;
                Topmost = true;
            });

            LocalTelemetry = telemetryData;
            if (LocalTelemetry != null && LocalTelemetry.IsReady)
            {
                DisplayTrackMap();
            }
        }

        private void DisplayTrackMap()
        {
            foreach(var driver in LocalTelemetry.AllPositions)
            {
                Dispatcher.Invoke(() =>
                {
                    Width = _GlobalSettings.SimpleTrackSettings.ContainerWidth;
                    MainBorder.Height = _GlobalSettings.UISize.SimpleTrackSettings.PositionDiameter + 2;
                    MainBorder.Clip = new RectangleGeometry(new Rect(0, 0, Width - 4, _GlobalSettings.UISize.SimpleTrackSettings.PositionDiameter + 2), _GlobalSettings.UISize.SimpleTrackSettings.PositionDiameter / 2 + 4, _GlobalSettings.UISize.SimpleTrackSettings.PositionDiameter / 2 + 2);
                    TrackCanvas.Height = _GlobalSettings.UISize.SimpleTrackSettings.PositionDiameter + 2;
                    CanvasBorder.Height = _GlobalSettings.UISize.SimpleTrackSettings.PositionDiameter + 2;
                    CanvasBorder.Margin = new Thickness(0, (_GlobalSettings.UISize.SimpleTrackSettings.PositionDiameter * -1) - 2, 0, 0);
                    CanvasBorder.CornerRadius = new CornerRadius(_GlobalSettings.UISize.SimpleTrackSettings.PositionDiameter / 2 + 3);
                    RelativeGeometry.Rect = new Rect(0, 0, Width - 4, 26);

                    var textBox = new TextBox();
                    textBox.Visibility = Visibility.Visible;
                    textBox.Text = driver.ClassPosition.ToString();
                    textBox.Uid = $"{driver.ClassId}-{driver.CarId}";
                    textBox.Width = _GlobalSettings.UISize.SimpleTrackSettings.PositionDiameter;
                    textBox.Height = _GlobalSettings.UISize.SimpleTrackSettings.PositionDiameter;

                    textBox.Margin = new Thickness(driver.PosOnTrack / LocalTelemetry.TrackLength * (Width - 4)  - (textBox.Height / 2), 0, 0, 0);
                    textBox.FontWeight = FontWeights.Bold;
                    textBox.FontSize = _GlobalSettings.UISize.SimpleTrackSettings.FontSize;
                    textBox.TextAlignment = TextAlignment.Center;
                    textBox.HorizontalAlignment = HorizontalAlignment.Center;
                    textBox.VerticalAlignment = VerticalAlignment.Center;
                    textBox.Padding = new Thickness(0, _GlobalSettings.UISize.SimpleTrackSettings.PaddingTop, 0, 0);
                    textBox.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(driver.ClassColor.Replace("0x", "#"));
                    textBox.Foreground = Brushes.Black;
                    textBox.Resources = Player.Resources;
                    textBox.BorderThickness = new Thickness(0);
                    Canvas.SetZIndex(textBox, 99 - (driver.OverallPosition ?? 99));

                    if (driver.CarId == LocalTelemetry.FeedTelemetry.CamCarIdx) 
                    {
                        textBox.Background = Brushes.DarkGreen;
                        textBox.Foreground = Brushes.White;
                        Canvas.SetZIndex(textBox, 99);
                    }
 

                    List<UIElement> elementsToRemove = new List<UIElement>();
                    foreach (UIElement uiElement in TrackCanvas.Children.OfType<TextBox>())
                    { 
                        var element = (TextBox) uiElement;
                        if (element.Uid == textBox.Uid)//) && element.Margin != textBox.Margin)
                        {
                            elementsToRemove.Add(uiElement);
                        }
                    }

                    elementsToRemove.ForEach(element => TrackCanvas.Children.Remove(element));

                    if (driver.PosOnTrack > 0) 
                    {
                        TrackCanvas.Children.Add(textBox);
                    }
                });
            }
        }
    }
}
