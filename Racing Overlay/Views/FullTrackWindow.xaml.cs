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
                    mapColor.Brush = _GlobalSettings.SecondaryColorBrush;
                }
                catch (Exception)
                {

                }

                TrackMap.Tag = "MapImage";
                TrackMap.Source = map;
                TrackMap.Visibility = Visibility.Hidden;
                TrackMap.Source.Freeze();
            }

            TraceTrackLine();

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
            // 
            //GetPointsBetween(5, 10979, 11635, generatedCoordinates);
            //GetPointsBetween(3, 11635, 12127, generatedCoordinates);

            // points.Add(12325, new List<double> { 148.38, 13.32 });
            // points.Add(12855, new List<double> { 167.28, 7.42 });
            // points.Add(13134, new List<double> { 178.28, 1.82 });
            // points.Add(14072, new List<double> { 196.48, 32.27 });
            // points.Add(15457, new List<double> { 249.28, 34.42 });


            foreach (var driver in LocalTelemetry.AllPositions)
            {
                if (driver.Name.StartsWith("Brian D"))
                {
                    Trace.WriteLine((int)driver.PosOnTrack);
                }

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
                    var basePositionSize = LocalTelemetry.TrackLength > 10000 ? _GlobalSettings.UISize.FullTrackSettings.PositionDiameter * 0.7 : _GlobalSettings.UISize.FullTrackSettings.PositionDiameter;
                    var baseFontSize = LocalTelemetry.TrackLength > 10000 ? _GlobalSettings.UISize.FullTrackSettings.FontSize * 0.7 : _GlobalSettings.UISize.FullTrackSettings.FontSize;
                    var position = CreatePositionPixel(coordinates,
                    $"{driver.ClassId}-{driver.CarId}",
                    basePositionSize,
                    basePositionSize,
                    driver.ClassPosition.ToString(),
                    (SolidColorBrush)new BrushConverter().ConvertFrom(driver.ClassColor.Replace("0x", "#")),
                    baseFontSize);

                    // For some reason the horizontal placement becomes misaligned as the scale gets larger
                    // This leftOffset re-aligns the placements
                    var leftOffset = _GlobalSettings.UISize.Percentage - 100 > 0 ? (_GlobalSettings.UISize.Percentage - 100) / 20 : 0;

                    Canvas.SetLeft(position, coordinates[0] * (_GlobalSettings.UISize.Percentage / 100.0) - leftOffset);
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

            // For some reason the horizontal placement becomes misaligned as the scale gets larger
            // This leftOffset re-aligns the placements
            var leftOffset = _GlobalSettings.UISize.Percentage - 100 > 0 ? (_GlobalSettings.UISize.Percentage - 100) / 20 : 0;

            Canvas.SetLeft(position, coordinate[0] * (_GlobalSettings.UISize.Percentage / 100.0) - leftOffset);
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
                    if (int.Parse(coordinate.Key) % 60 == 0)
                    {
                        var pixel = CreatePositionPixel(coordinate.Value, 
                            null, 
                            4, 
                            4, 
                            $"{Math.Round(double.Parse(coordinate.Key) / 1000, 1)}", Brushes.Green);
                        Canvas.SetZIndex(pixel, 99);

                        if (int.Parse(coordinate.Key) % 1000 == 0)
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

            points.Add(0, new List<double>    { 138.14, 217.09 });
            points.Add(397, new List<double> { 128.04, 227.57 });
            points.Add(477, new List<double> { 125.9 , 229.33 });
            points.Add(556, new List<double> { 123.68, 231.06 });
            points.Add(636, new List<double>  { 121.14, 232.62 });
            points.Add(698, new List<double>  { 118.14, 231.32 });
            points.Add(756, new List<double> { 116.74, 232.99 });
            points.Add(1106, new List<double> { 112.92, 246.25 });
            points.Add(1165, new List<double> { 112.74, 248.42 });
            points.Add(1206, new List<double> { 113.74, 249.95 });
            points.Add(1247, new List<double> { 115.47, 250.92 });
            points.Add(1288, new List<double> { 117.31, 251.8  });
            points.Add(1329, new List<double> { 118.44, 253.32 });
            points.Add(1404, new List<double> { 116.46, 255.46 });
            points.Add(1480, new List<double> { 113.47, 257.12 });
            points.Add(1555, new List<double> { 110.82, 258.95 });
            points.Add(1631, new List<double> { 108.44, 260.95 });
            points.Add(1707, new List<double> { 106.26, 263.07 });
            points.Add(1782, new List<double> { 104.31, 265.4  });
            points.Add(1858, new List<double> { 102.32, 267.76 });
            points.Add(1934, new List<double> { 99.54 , 268.12 });
            points.Add(1983, new List<double> { 98.83, 266.33 });
            points.Add(2278, new List<double> { 107.08, 256.70 });
            points.Add(2328, new List<double> { 107.84, 254.92 });
            points.Add(2395, new List<double> { 107.33, 252.31 });
            points.Add(2463, new List<double> { 107.22, 249.67 });
            points.Add(2531, new List<double> { 107.81, 247.02 });
            points.Add(2599, new List<double> { 108.69, 244.37 });
            points.Add(2666, new List<double> { 109.53, 241.76 });
            points.Add(2734, new List<double> { 110.39, 239.12 });
            points.Add(2802, new List<double> { 111.25, 236.47 });
            points.Add(2870, new List<double> { 111.74, 233.82 });
            points.Add(2920, new List<double> { 110.29, 232.26 });
            points.Add(2971, new List<double> { 108.36, 231.08 });
            points.Add(3022, new List<double> { 106.43, 230.03 });
            points.Add(3073, new List<double> { 104.90, 228.32 });
            points.Add(3186, new List<double> { 107.4 , 224.52 });
            points.Add(3300, new List<double> { 110.21, 221.1  });
            points.Add(3414, new List<double> { 113.32, 217.80 });
            points.Add(3528, new List<double> { 117.34, 214.67 });
            points.Add(3641, new List<double> { 122.61, 213.27 });
            points.Add(3755, new List<double> { 127.52, 212.25 });
            points.Add(3869, new List<double> { 132.83, 211.14 });
            points.Add(3983, new List<double> { 137.64, 209.82 });
            points.Add(4039, new List<double> { 139.84, 206.62 });
            points.Add(4091, new List<double> { 141.35, 205.99 });
            points.Add(4143, new List<double> { 142.82, 205.29 });
            points.Add(4195, new List<double> { 144.01, 204.14 });
            points.Add(4247, new List<double> { 144.60, 202.58 });
            points.Add(4299, new List<double> { 144.66, 200.90 });
            points.Add(4351, new List<double> { 143.9 , 199.39 });
            points.Add(4403, new List<double> { 142.77, 198.33 });
            points.Add(4455, new List<double> { 141.24, 198.06 });
            points.Add(4554, new List<double> { 138.31, 200.62 });
            points.Add(4653, new List<double> { 135.87, 203.46 });
            points.Add(4752, new List<double> { 132.63, 205.62 });
            points.Add(4851, new List<double> { 128.39, 206.11 });
            points.Add(4950, new List<double> { 124.65, 204.61 });
            points.Add(5049, new List<double> { 120.92, 203.56 });
            points.Add(5148, new List<double> { 117.38, 203.42 });
            points.Add(5247, new List<double>  { 113.74, 204.23 });
            points.Add(5295, new List<double> { 111.92, 203.13 });
            points.Add(5490, new List<double> { 106.44, 196.6 });
            points.Add(5539, new List<double>  { 105.14, 194.93 });
            points.Add(5678, new List<double>  { 99.81, 193.73 });
            points.Add(5725, new List<double>  { 98.14 , 192.93 });
            points.Add(5826, new List<double> { 96.45, 189.34 });
            points.Add(5928, new List<double> { 93.33, 186.4  });
            points.Add(6029, new List<double> { 89.45, 183.59 });
            points.Add(6131, new List<double> { 85.71, 180.54 });
            points.Add(6232, new List<double> { 82.58, 177.13 });
            points.Add(6334, new List<double> { 79.54, 173.69 });
            points.Add(6435, new List<double> { 76.8 , 170.37 });
            points.Add(6537, new List<double>  { 75.34 , 166.33 });
            points.Add(6734, new List<double> { 77.05, 158.88 });
            points.Add(6931, new List<double> { 78.41, 151.3  });
            points.Add(7128, new List<double> { 78.17, 143.68 });
            points.Add(7325, new List<double> { 76.35, 136.33 });
            points.Add(7522, new List<double> { 73.49, 129.58 });
            points.Add(7719, new List<double> { 70.09, 122.23 });
            points.Add(7916, new List<double> { 64.69, 115.98 });
            points.Add(8113, new List<double>  { 57.46 , 110.73 });
            points.Add(8260, new List<double> { 62.33, 107.27 });
            points.Add(8407, new List<double> { 67.77, 104.66 });
            points.Add(8555, new List<double> { 72.73, 101.21 });
            points.Add(8702, new List<double> { 77.66, 97.15  });
            points.Add(8849, new List<double> { 82.4 , 92.84  });
            points.Add(8997, new List<double> { 86.30, 87.59  });
            points.Add(9144, new List<double> { 88.3 , 81.32 });
            points.Add(9292, new List<double>  { 88.96 , 74.93  });
            points.Add(9364, new List<double> { 90.44, 73.42 });
            points.Add(9436, new List<double>  { 91.66 , 71.73  });
            points.Add(9496, new List<double> { 91.12, 69.56 });
            points.Add(9557, new List<double>  { 91.02 , 67.33  });
            points.Add(9663, new List<double>  { 94.49 , 64.17 });
            points.Add(9769, new List<double>  { 98.09 , 60.98 });
            points.Add(9875, new List<double>  { 101.19, 57.50 });
            points.Add(9982, new List<double>  { 104.41, 54.39 });
            points.Add(10088, new List<double> { 107.67, 50.90 });
            points.Add(10194, new List<double> { 109.80, 46.72 });
            points.Add(10300, new List<double> { 109.40, 42.06 });
            points.Add(10407, new List<double> { 107.02, 37.96  });
            points.Add(10481, new List<double> { 104.05, 37.45 });
            points.Add(10556, new List<double> { 101.3 , 36.22 });
            points.Add(10630, new List<double> { 98.92 , 34.35 });
            points.Add(10705, new List<double> { 97.72 , 31.62  });
            points.Add(10773, new List<double> { 99.72 , 29.71 });
            points.Add(10842, new List<double> { 102.31, 28.27 });
            points.Add(10910, new List<double> { 104.87, 26.49 });
            points.Add(10979, new List<double> { 106.77, 24.32 });



            points.Add(11635, new List<double> { 125.57, 24.12 });
            points.Add(12127, new List<double> { 145.97, 21.18 });
            points.Add(12325, new List<double> { 148.38, 13.32 });
            points.Add(12855, new List<double> { 167.28, 7.42  });
            points.Add(13134, new List<double> { 178.28, 1.82  });
            points.Add(14072, new List<double> { 196.48, 32.27 });
            points.Add(15457, new List<double> { 249.28, 34.42 });
            points.Add(15791, new List<double> { 263.58, 32.42 });
            points.Add(16209, new List<double> { 252.58, 46.27 });
            points.Add(16713, new List<double> { 271.28, 38.52 });
            points.Add(16957, new List<double> { 278.32, 29.62 });
            points.Add(17209, new List<double> { 281.72, 20.99 });
            points.Add(17537, new List<double> { 294.02, 23.87 });
            points.Add(17627, new List<double> { 297.52, 27.22 });
            points.Add(17743, new List<double> { 302.82, 29.57 });
            points.Add(17953, new List<double> { 307.32, 36.42 });
            points.Add(18231, new List<double> { 305.45, 46.22 });
            points.Add(18373, new List<double> { 311.63, 50.12 });
            points.Add(18602, new List<double> { 311.45, 59.32 });
            points.Add(18866, new List<double> { 301.05, 59.72 });
            points.Add(19461, new List<double> { 293.81, 84.72 });
            points.Add(20664, new List<double> { 252.02, 110.75 });
            points.Add(20840, new List<double> { 246.62, 107.35 });
            points.Add(21091, new List<double> { 238.48, 112.45 });
            points.Add(21442, new List<double> { 249.98, 120.45 });
            points.Add(21733, new List<double> { 252.92, 129.25 });
            points.Add(24593, new List<double> { 157.96, 199.45 });
            points.Add(24674, new List<double> { 156.36, 203.75 });
            points.Add(25175, new List<double>{ 139.10, 216.09 });
            

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
