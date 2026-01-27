using Newtonsoft.Json;
using RacingOverlay.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RacingOverlay.Windows
{
    /// <summary>
    /// Interaction logic for FullTrackWindow.xaml
    /// </summary>
    /// 

    public partial class FullTrackWindow : Window
    {
        TelemetryData LocalTelemetry;
        public bool Locked = false;
        private GlobalSettings _GlobalSettings;
        private int DefaultWidth = 400;
        private double UpdatedWidth;
        private Dictionary<string, List<double>> _TrackJsonData;
        public FullTrackWindow(TelemetryData telemetryData, GlobalSettings globalSettings, WindowSettings settings)
        {
            LocalTelemetry = telemetryData;

            InitializeComponent();
            Opacity = double.Parse(settings.FullTrackSettings["Opacity"]);
            Locked = bool.Parse(settings.FullTrackSettings["Locked"] ?? "false");
            Left = double.Parse(settings.FullTrackSettings["XPos"] ?? "0");
            Top = double.Parse(settings.FullTrackSettings["YPos"] ?? "0");
            _GlobalSettings = globalSettings;

            if (HasTrackMap(out DrawingImage map))
            {
                try
                {
                    var mapColor = (GeometryDrawing)((DrawingGroup)map.Drawing).Children[0];
                    mapColor.Brush = (SolidColorBrush)new BrushConverter().ConvertFrom(_GlobalSettings.SecondaryColor);
                }
                catch (Exception)
                {

                }

                TrackMap.Tag = "MapImage";
                TrackMap.Source = map;
                TrackMap.Visibility = Visibility.Hidden;
                TrackMap.Source.Freeze();
            }

            //TraceTrackLine();

#if SAMPLE 
            GetTrackJsonData();
#endif
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!Locked)
            {
                base.OnMouseLeftButtonDown(e);
                DragMove();
            }
        }

        public bool HasTrackMap(out DrawingImage map)
        {
            var mapResourceDictionary = (ResourceDictionary)Application.Current.Resources[$"{LocalTelemetry.TrackId}-{LocalTelemetry.TrackName.ToLower()}"];
            map = (DrawingImage)mapResourceDictionary?["di_map_xaml"];

            return map != null;
        }

        public (string, double, double, double) GetTrackMapTransformData()
        {
            var transformDataList = new List<(string, double, double, double)>();
            var trackDirectory = Directory.GetDirectories("assets/tracks/");
            var transformFile = $"assets/tracks/transform.csv";
            if (File.Exists(transformFile))
            {
                try
                {
                    using (var reader = new StreamReader(transformFile))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var data = line.Split(',');
                            if (data[0] == "trackname")
                                continue;
                            transformDataList.Add((data[0], double.Parse(data[1]), double.Parse(data[2]), double.Parse(data[3])));
                        }
                    }
                }
                catch (IOException ex)
                {
                    Trace.WriteLine(ex);
                }
            }

            var transformData = transformDataList.Where(t => t.Item1 == $"{LocalTelemetry.TrackId}-{LocalTelemetry.TrackName.ToLower()}").FirstOrDefault();

            if (transformData.Item1 == null)
                transformData = transformDataList.Where(t => t.Item1 == "default").FirstOrDefault();

            return transformData;
        }

        public bool HasTrackCoordinates()
        {
            return File.Exists($"assets/tracks/{LocalTelemetry.TrackId}-{LocalTelemetry.TrackName.ToLower()}/coordinates.json");
        }

        public void UpdateTelemetryData(TelemetryData telemetryData, WindowSettings settings)
        {
            Opacity = double.Parse(settings.FullTrackSettings["Opacity"]);
            if (DateTime.UtcNow.Second % 10 == 0)
            {
                Dispatcher.Invoke(() =>
                {
                    Topmost = false;
                    Topmost = true;
                });
            }

            LocalTelemetry = telemetryData;
#if SAMPLE
            DisplayTrackMap();
#else
            if (LocalTelemetry != null && LocalTelemetry.IsReady)
            {
                DisplayTrackMap();
            }
#endif

        }

        private void GetTrackJsonData()
        {
            if (!HasTrackCoordinates())
                return;

            // Use cached json object
            if (LocalTelemetry.LastSample?.TrackName == LocalTelemetry.TrackName)
                return;
                
            using (var reader = new StreamReader($"./assets/tracks/{LocalTelemetry.TrackId}-{LocalTelemetry.TrackName}/coordinates.json"))
            {
                string json = reader.ReadToEnd();
                _TrackJsonData = JsonConvert.DeserializeObject<Dictionary<string, List<double>>>(json);
            }
        }

        private void DisplayTrackMap()
        {
            if (UpdatedWidth != DefaultWidth * (_GlobalSettings.UISize.Percentage / 100.0))
            {
                UpdatedWidth = DefaultWidth * (_GlobalSettings.UISize.Percentage / 100.0);
                var updatedHeight = 300 * (_GlobalSettings.UISize.Percentage / 100.0);
                var transformData = GetTrackMapTransformData();

                if (UpdatedWidth > DefaultWidth * (_GlobalSettings.UISize.Percentage / 100.0))
                {
                    Dispatcher.Invoke(() =>
                    {
                        Width = UpdatedWidth;
                        Height = updatedHeight;
                        TrackMapViewbox.Width = UpdatedWidth * transformData.Item2;
                        TrackMapViewbox.Height = updatedHeight * transformData.Item2;
                        TrackMapViewbox.Margin = new Thickness(transformData.Item3, transformData.Item4, 0, 0);
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        TrackMapViewbox.Width = UpdatedWidth * transformData.Item2;
                        TrackMapViewbox.Height = updatedHeight * transformData.Item2;
                        TrackMapViewbox.Margin = new Thickness(transformData.Item3, transformData.Item4, 0, 0);
                        Width = UpdatedWidth;
                        Height = updatedHeight;
                    });
                        
                }
                Dispatcher.Invoke(() =>
                {
                    TrackMap.Visibility = Visibility.Visible;
                }); 
            }

            //GetSamplePoints(_TrackJsonData);
            GetTrackJsonData();
            //var generatedCoordinates = GenerateCoordinates();
            //GetPointsBetween(5, 1721, 1823, generatedCoordinates);

            foreach (var driver in LocalTelemetry.AllPositions)
            {
                //if (driver.Name.StartsWith("Brian D"))
                //{
                //    Trace.WriteLine((int)driver.PosOnTrack);
                //}

                if (_TrackJsonData == null)
                {
                    return;
                }
                
                var coordinates = new List<double> { 0, 0 };
                var prevCoordinates = new List<double> { 0, 0 };
                var nextCoordinates = new List<double> { 0, 0 };
                _TrackJsonData.TryGetValue(((int)driver.PosOnTrack).ToString(), out coordinates);
                
                if (coordinates == null)
                {
                    _TrackJsonData.TryGetValue((((int)driver.PosOnTrack) - 1).ToString(), out prevCoordinates);
                    _TrackJsonData.TryGetValue((((int)driver.PosOnTrack) + 1).ToString(), out nextCoordinates);
                }
                
                
                
                if (prevCoordinates == null || nextCoordinates == null)
                {
                    prevCoordinates = new List<double> { -30, -30 };
                    nextCoordinates = new List<double> { -30, -30 };
                
                }
                
                if (coordinates == null)
                {
                    coordinates = new List<double>
                    {
                        (prevCoordinates[0] + nextCoordinates[0]) / 2,
                        (prevCoordinates[1] + nextCoordinates[1]) / 2
                    };
                }
                
                
                Dispatcher.Invoke(() =>
                {
                    var position = CreatePositionPixel(coordinates,
                    $"{driver.ClassId}-{driver.CarId}",
                    22 * (_GlobalSettings.UISize.Percentage / 100.0),
                    22 * (_GlobalSettings.UISize.Percentage / 100.0),
                    driver.ClassPosition.ToString(),
                    (SolidColorBrush)new BrushConverter().ConvertFrom(driver.ClassColor.Replace("0x", "#")),
                    _GlobalSettings.UISize.SimpleTrackSettings.FontSize + 2);
                
                    Canvas.SetLeft(position, coordinates[0] * (_GlobalSettings.UISize.Percentage / 100.0));
                    Canvas.SetTop(position, coordinates[1] * (_GlobalSettings.UISize.Percentage / 100.0));
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
                
                    foreach (var element in elementsToRemove)
                    {
                        TrackCanvas.Children.Remove(element);
                    }
                    elementsToRemove = null;
                
                    if (driver.PosOnTrack > 0)
                    {
                        TrackCanvas.Children.Add(position);
                    }
                });
            }
        }

        private Grid CreatePositionPixel(List<double> coordinate, string id, double width, double height, string text, SolidColorBrush color, double fontSize = 0.01)
        {
            var position = new Grid();
            position.Visibility = Visibility.Visible;
            position.Uid = id;
            position.Width = 30;
            position.Height = 30;

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

            Canvas.SetLeft(position, coordinate[0] * (_GlobalSettings.UISize.Percentage / 100.0));
            Canvas.SetTop(position, coordinate[1] * (_GlobalSettings.UISize.Percentage / 100.0));

            return position;
        }

        private void TraceTrackLine()
        {
            GetTrackJsonData();
            if (_TrackJsonData != null)
            {
                foreach (var coordinate in _TrackJsonData)
                {
                    if (int.Parse(coordinate.Key) % 20 == 0)
                    {
                        var pixel = CreatePositionPixel(coordinate.Value, 
                            null, 
                            4, 
                            4, 
                            $"{Math.Round(double.Parse(coordinate.Key) / 1000, 1)}", Brushes.Green);
                        Canvas.SetZIndex(pixel, 99);

                        if (int.Parse(coordinate.Key) % 100 == 0)
                        {
                            var text = (TextBlock)pixel.Children[1];
                            text.Foreground = Brushes.White;
                            text.FontSize = 14;
                            Canvas.SetZIndex(pixel, 100);
                        }
                        TrackCanvas.Children.Add(pixel);
                    }
                }
            }
        }

        private Dictionary<int, (double, double)> GenerateCoordinates()
        {
            var fileLocation = $"..\\..\\trackline.txt";
            var points = new Dictionary<int, List<double>>();

            points.Add(0, new List<double> { 112.03, 18 });

            var coordinatesDictionary = new Dictionary<int, (double, double)>();
            var locationOnTrack = 0;
            var x = points.Values.First()[0];
            var y = points.Values.First()[1];
            using (StreamWriter writer = new StreamWriter(fileLocation))
            {
                writer.WriteLine("{");
                foreach (var corner in points)
                {
                    var xDev = (corner.Value[0] - x) / (corner.Key - locationOnTrack == 0 ? 1 : corner.Key - locationOnTrack);
                    var yDev = (corner.Value[1] - y) / (corner.Key - locationOnTrack == 0 ? 1 : corner.Key - locationOnTrack);

                    while (locationOnTrack <= corner.Key)
                    {

                        if (locationOnTrack == corner.Key)
                            coordinatesDictionary.Add(locationOnTrack, (Math.Round(corner.Value[0], 2), Math.Round(corner.Value[1], 2)));
                        else
                            coordinatesDictionary.Add(locationOnTrack, (Math.Round(x, 2), Math.Round(y, 2)));

                        if (locationOnTrack % 2 == 0)
                        {
                            if (locationOnTrack == corner.Key)
                                writer.WriteLine($"\t\"{locationOnTrack}\": [{Math.Round(corner.Value[0], 2)}, {Math.Round(corner.Value[1], 2)}]");
                            else
                                writer.WriteLine($"\t\"{locationOnTrack}\": [{Math.Round(x, 2)}, {Math.Round(y, 2)}]");
                        }


                        locationOnTrack++;
                        x += xDev;
                        y += yDev;
                    }
                }
                writer.WriteLine("}");
            }

            return coordinatesDictionary;
        }

        private void GetPointsBetween(int pointCount, int a, int b, Dictionary<int, (double, double)> coordinates)
        {
            var step = (double)(b - a) / (double)(pointCount + 1);

            for (double i = a; i <= b; i += step)
            {
                Console.WriteLine($"{(int)i}: Canvas.Left=\"{coordinates[(int)i].Item1}\"\t\tCanvas.Top=\"{coordinates[(int)i].Item2}\"");
            }
            Console.WriteLine();
        }

        private void GetSamplePoints(Dictionary<string, List<double>> coordinates)
        {
            foreach (var point in coordinates.ToDictionary(c => int.Parse(c.Key), c => c.Value))
            {
                if (point.Key % 50 == 0)
                    Console.WriteLine($"points.Add({point.Key}, new List<double> {{{point.Value[0]},{point.Value[1]}}});");
            }
        }
    }
}
