using RacingOverlay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using static iRacingSDK.SessionData._DriverInfo;

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
        public SimpleTrackWindow(TelemetryData telemetryData, GlobalSettings globalSettings, WindowSettings settings)
        {
            _GlobalSettings = globalSettings;
            LocalTelemetry = telemetryData;
            InitializeComponent();
            Locked = bool.Parse(settings.SimpleTrackSettings["Locked"] ?? "false");
            Left = double.Parse(settings.SimpleTrackSettings["XPos"] ?? "0");
            Top = double.Parse(settings.SimpleTrackSettings["YPos"] ?? "0");
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
            if (DateTime.UtcNow.Second % 10 == 0)
            {
                Dispatcher.Invoke(() =>
                {
                    Topmost = false;
                    Topmost = true;
                });
            }

            LocalTelemetry = telemetryData;
            if (LocalTelemetry != null && LocalTelemetry.IsReady)
            {
                DisplayTrackMap();
            }
        }

        private void DisplayTrackMap()
        {
            foreach (var driver in LocalTelemetry.AllPositions)
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
                });

                var position = CreatePositionPixel(driver.PosOnTrack,
                    $"{driver.ClassId}-{driver.CarId}",
                    _GlobalSettings.UISize.SimpleTrackSettings.PositionDiameter,
                    _GlobalSettings.UISize.SimpleTrackSettings.PositionDiameter,
                    driver.ClassPosition.ToString(),
                    (SolidColorBrush)new BrushConverter().ConvertFrom(driver.ClassColor.Replace("0x", "#")),
                    _GlobalSettings.UISize.SimpleTrackSettings.FontSize);

                Canvas.SetZIndex(position, 99 - (driver.OverallPosition ?? 99));

                if (driver.CarId == LocalTelemetry.FeedTelemetry.CamCarIdx)
                {
                    var positionEllipse = (Ellipse)position.Children[0];
                    var positionText = (TextBlock)position.Children[1];

                    positionEllipse.Fill = Brushes.DarkGreen;
                    positionText.Foreground = Brushes.White;
                    Canvas.SetZIndex(position, 99);
                }


                List<UIElement> elementsToRemove = new List<UIElement>();
                foreach (UIElement uiElement in TrackCanvas.Children.OfType<Grid>())
                {
                    var element = (Grid)uiElement;
                    if (element.Uid == position.Uid)
                    {
                        elementsToRemove.Add(uiElement);
                    }
                }

                Dispatcher.Invoke(() =>
                {
                    elementsToRemove.ForEach(element => 
                    {
                        TrackCanvas.Children.Remove(element);
                    });

                    elementsToRemove = null;
                });
                    

                if (driver.PosOnTrack > 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        TrackCanvas.Children.Add(position);
                    });
                        
                }
            }
        }

        private Grid CreatePositionPixel(double posOnTrack, string id, double width, double height, string text, SolidColorBrush color, double fontSize = 0.01)
        {
            var position = new Grid();
            position.Visibility = Visibility.Visible;
            position.Uid = id;
            position.Width = width;
            position.Height = height;
            position.Margin = new Thickness(posOnTrack / LocalTelemetry.TrackLength * (Width - 4) - (position.Height / 2), 0, 0, 0);

            var positionEllipse = new Ellipse();
            positionEllipse.Fill = color;
            positionEllipse.Width = width;
            positionEllipse.Height = height;

            var positionText = new TextBlock();
            positionText.FontSize = fontSize;
            positionText.FontWeight = FontWeights.Bold;
            positionText.HorizontalAlignment = HorizontalAlignment.Center;
            positionText.VerticalAlignment = VerticalAlignment.Center;
            positionText.Text = text;

            position.Children.Add(positionEllipse);
            position.Children.Add(positionText);

            return position;
        }
    }
}
