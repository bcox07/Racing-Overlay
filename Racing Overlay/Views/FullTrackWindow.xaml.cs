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
            if (UpdatedWidth != DefaultWidth * (_GlobalSettings.FullTrackSettings.Percentage / 100.0))
            {
                UpdatedWidth = DefaultWidth * (_GlobalSettings.FullTrackSettings.Percentage / 100.0);
                var updatedHeight = 300 * (_GlobalSettings.FullTrackSettings.Percentage / 100.0);
                var transformData = GetTrackMapTransformData();

                if (UpdatedWidth > DefaultWidth * (_GlobalSettings.FullTrackSettings.Percentage / 100.0))
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
            //GetPointsBetween(3, 1520, 1571, generatedCoordinates);
            //GetPointsBetween(3, 1622, 1930, generatedCoordinates);
            //GetPointsBetween(3, 3218, 3254, generatedCoordinates);
            //GetPointsBetween(3, 3254, 3285, generatedCoordinates);

            foreach (var driver in LocalTelemetry.AllPositions)
            {
                if (driver.Name.StartsWith("Daniel W"))
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
                    var basePositionSize = LocalTelemetry.TrackLength > 10000 ? _GlobalSettings.FullTrackSettings.PositionDiameter * 0.7 : _GlobalSettings.FullTrackSettings.PositionDiameter;
                    var baseFontSize = LocalTelemetry.TrackLength > 10000 ? _GlobalSettings.FullTrackSettings.DataFontSize * 0.7 : _GlobalSettings.FullTrackSettings.DataFontSize;
                    var position = CreatePositionPixel(coordinates,
                    $"{driver.ClassId}-{driver.CarId}",
                    basePositionSize,
                    basePositionSize,
                    driver.ClassPosition.ToString(),
                    (SolidColorBrush)new BrushConverter().ConvertFrom(driver.ClassColor.Replace("0x", "#")),
                    baseFontSize);

                    // For some reason the horizontal placement becomes misaligned as the scale gets larger
                    // This leftOffset re-aligns the placements
                    var leftOffset = _GlobalSettings.FullTrackSettings.Percentage - 100 > 0 ? (_GlobalSettings.FullTrackSettings.Percentage - 100) / 20 : 0;

                    Canvas.SetLeft(position, coordinates[0] * (_GlobalSettings.FullTrackSettings.Percentage / 100.0) - leftOffset);
                    Canvas.SetTop(position, coordinates[1] * (_GlobalSettings.FullTrackSettings.Percentage / 100.0));
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
            var leftOffset = _GlobalSettings.FullTrackSettings.Percentage - 100 > 0 ? (_GlobalSettings.FullTrackSettings.Percentage - 100) / 20 : 0;

            Canvas.SetLeft(position, coordinate[0] * (_GlobalSettings.FullTrackSettings.Percentage / 100.0) - leftOffset);
            Canvas.SetTop(position, coordinate[1] * (_GlobalSettings.FullTrackSettings.Percentage / 100.0));

            return position;
        }

        private void TraceTrackLine()
        {
            GetTrackJsonData();
            if (_TrackJsonData != null)
            {
                foreach (var coordinate in _TrackJsonData)
                {
                    if (int.Parse(coordinate.Key) % 10 == 0)
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

            points.Add(0, new List<double>    { 175.56, 263.00 });

            points.Add(56, new List<double>  { 192.04, 261.00 }); //
            points.Add(112, new List<double> { 208.38, 257.88 }); //
            points.Add(169, new List<double> { 224.53, 254.54 }); //
            points.Add(225, new List<double> { 240.72, 250.02 }); //
            points.Add(281, new List<double> { 254.26, 241.79 }); //
            points.Add(338, new List<double> { 267.86, 231.85 }); //
            points.Add(394, new List<double> { 281.1 , 221.83 }); //

            points.Add(408, new List<double> { 284.30, 219.21 }); //
            points.Add(422, new List<double> { 287.47, 216.35 }); //
            points.Add(436, new List<double> { 290.64, 213.18 }); //



            points.Add(451, new List<double>  { 292.40, 208.99 });

            points.Add(465, new List<double> { 291.93, 204.95 }); //
            points.Add(479, new List<double> { 290.01, 201.48 }); //
            points.Add(493, new List<double> { 287.08, 198.72 }); //
            points.Add(507, new List<double> { 283.26, 196.66 }); //
            points.Add(521, new List<double> { 279.33, 196.29 }); //


            points.Add(536, new List<double>  { 275.70, 197.09 });

            points.Add(566, new List<double> { 269.36, 203.61 }); //
            points.Add(597, new List<double> { 264.26, 210.50 }); //
            points.Add(628, new List<double> { 258.16, 216.96 }); //
            points.Add(659, new List<double> { 251.16, 222.54 }); //
            points.Add(689, new List<double> { 243.79, 227.36 }); //
            points.Add(720, new List<double> { 236.2 , 231.63 }); //
            points.Add(751, new List<double> { 228.2 , 235.21 }); //

            points.Add(766, new List<double> { 223.96, 236.53 }); //

            points.Add(782, new List<double>  { 219.40, 236.29 });

            points.Add(798, new List<double> { 214.42, 234.37 }); //

            points.Add(814, new List<double> { 211.69, 230.43 });  //
            points.Add(1009, new List<double> { 188.62, 175.85 }); //

            points.Add(1042, new List<double> { 188.40, 166.09 });

            points.Add(1056, new List<double> { 189.72, 161.96 }); //
            points.Add(1071, new List<double> { 192.41, 158.35 }); //
            points.Add(1086, new List<double> { 195.60, 154.93 }); //

            points.Add(1101, new List<double> { 199.19, 151.82 }); //
            points.Add(1161, new List<double> { 213.50, 139.60 }); //
            points.Add(1221, new List<double> { 227.81, 128.67 }); //
            points.Add(1281, new List<double> { 244.01, 120.24 }); //
            points.Add(1340, new List<double> { 261.57, 113.78 }); //
            points.Add(1400, new List<double> { 278.88, 107.55 }); //
            points.Add(1460, new List<double> { 296.18, 101.12 }); //

            points.Add(1475, new List<double> { 300.43, 98.67 }); //
            points.Add(1490, new List<double> { 303.55, 94.98 }); //
            points.Add(1505, new List<double> { 305.47, 90.68 }); //

            points.Add(1520, new List<double> { 305.99, 86.09  });

            points.Add(1532, new List<double> { 305.78, 82.70 }); //
            points.Add(1545, new List<double> { 304.75, 79.64 }); //
            points.Add(1558, new List<double> { 303.22, 76.57 }); //

            points.Add(1571, new List<double> { 301.29, 73.60 }); //
            points.Add(1622, new List<double> { 292.24, 63.77 }); //

            points.Add(1699, new List<double> { 277.49, 48.58 }); //
            points.Add(1776, new List<double> { 262.13, 33.38 }); //
            points.Add(1853, new List<double> { 246.76, 18.79 }); //


            points.Add(1930, new List<double> { 231.20, 4.59   });

            points.Add(1982, new List<double> { 216.25, 5.57  }); //
            points.Add(2035, new List<double> { 201.57, 8.72  }); //
            points.Add(2088, new List<double> { 187.29, 14.86 }); //
            points.Add(2141, new List<double> { 174.41, 23.21 }); //
            points.Add(2194, new List<double> { 161.83, 31.75 }); //
            points.Add(2247, new List<double> { 149.16, 40.35 }); //
            points.Add(2300, new List<double> { 136.58, 49.44 }); //


            points.Add(2353, new List<double> { 123.70, 59.69  });

            points.Add(2429, new List<double> { 102.26, 68.30 }); //


            points.Add(2440, new List<double> { 100.30, 70.69  });

            points.Add(2450, new List<double> { 99.97 , 73.57 }); //
            points.Add(2460, new List<double> { 101.06, 76.51 }); //
            points.Add(2470, new List<double> { 102.15, 79.45 }); //

            points.Add(2481, new List<double> { 102.50, 82.69 });

            points.Add(2511, new List<double> { 98.75, 89.77  }); //
            points.Add(2542, new List<double> { 92.13, 95.29  }); //
            points.Add(2572, new List<double> { 85.06, 100.50 }); //
            points.Add(2603, new List<double> { 77.44, 105.92 }); //
            points.Add(2634, new List<double> { 70.81, 112.14 }); //
            points.Add(2664, new List<double> { 65.80, 119.74 }); //
            points.Add(2695, new List<double> { 63.92, 128.17 }); //

            points.Add(2726, new List<double> { 64.60 , 136.69 });

            points.Add(2756, new List<double> { 69.12, 143.88 }); //
            points.Add(2938, new List<double> { 111.49, 176.83 }); //

            points.Add(2969, new List<double> { 115.99, 184.19 });

            points.Add(3004, new List<double> { 114.30, 193.90 }); //
            points.Add(3218, new List<double> { 86.80, 250.39 });  //

            points.Add(3227, new List<double> { 85.58, 252.63 });  //
            points.Add(3236, new List<double> { 84.52, 255.00 });  //
            points.Add(3245, new List<double> { 83.85, 257.54 });  //


            points.Add(3254, new List<double> { 84.09 , 260.19 });

            points.Add(3261, new List<double> { 85.72, 261.79 }); //
            points.Add(3269, new List<double> { 87.88, 262.46 }); //
            points.Add(3277, new List<double> { 90.19, 262.63 }); //

            points.Add(3285, new List<double> { 92.51, 262.61 });  //


            points.Add(3564, new List<double> { 174.56, 263.00 });

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
                Console.WriteLine($"{(int)i}: Canvas.Left=\"{coordinates[(int)i].Item1.ToString("F2")}\"\t\tCanvas.Top=\"{coordinates[(int)i].Item2.ToString("F2")}\"");
            }
            Console.WriteLine();
        }

        private void GetSamplePoints(Dictionary<string, List<double>> coordinates)
        {
            foreach (var point in coordinates.ToDictionary(c => int.Parse(c.Key), c => c.Value))
            {
                if (point.Key % 10 == 0)
                    Console.WriteLine($"points.Add({point.Key}, new List<double> {{{point.Value[0]},{point.Value[1]}}});");
            }
        }
    }
}
