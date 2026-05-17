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
            //
            //GetPointsBetween(3, 1788, 1839, generatedCoordinates);
            //GetPointsBetween(3, 991, 1030, generatedCoordinates);
            //GetPointsBetween(3, 1030, 1080, generatedCoordinates);

            // points.Add(991, new List<double> { 191.48, 131.78 }); //
            // points.Add(1030, new List<double> { 199.25, 120.11 });
            // points.Add(1080, new List<double> { 192.97, 105.93 }); //


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
                    if (int.Parse(coordinate.Key) % 20 == 0)
                    {
                        var pixel = CreatePositionPixel(coordinate.Value, 
                            null, 
                            4, 
                            4, 
                            $"{Math.Round(double.Parse(coordinate.Key) / 1000, 1)}", Brushes.Green);
                        Canvas.SetZIndex(pixel, 99);

                        if (int.Parse(coordinate.Key) % 200 == 0)
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

            points.Add(0, new List<double>    { 260.05, 191.41 });


            points.Add(410, new List<double> { 129.31, 148.92 });
            points.Add(456, new List<double>  { 114.05, 145.81 });

            points.Add(472, new List<double> { 108.54, 147.31 }); //
            points.Add(489, new List<double> { 103.96, 150.29 }); //
            points.Add(505, new List<double> { 99.08 , 153.31 }); //

            points.Add(522, new List<double> { 93.89, 155.99 });

            points.Add(538, new List<double> { 88.07, 156.92 }); //
            points.Add(555, new List<double> { 82.27, 155.77 }); //
            points.Add(572, new List<double> { 77.77, 152.82 }); //
            points.Add(589, new List<double> { 74.17, 148.97 });
            points.Add(605, new List<double> { 71.8 , 144.24 }); //
            points.Add(622, new List<double> { 70.72, 139.01 }); //
            points.Add(639, new List<double> { 71.13, 133.77 }); //
            points.Add(656, new List<double> { 73.55, 128.54 });
            points.Add(672, new List<double> { 76.82, 124.36 }); //
            points.Add(689, new List<double> { 81.97, 121.35 }); //
            points.Add(706, new List<double> { 87.81, 119.93 }); //
            points.Add(723, new List<double> { 94.05, 120.01 });
            points.Add(761, new List<double> { 107.84, 122.11 }); //
            points.Add(799, new List<double> { 121.67, 124.24 }); //
            points.Add(838, new List<double> { 135.84, 126.37 }); //
            points.Add(876, new List<double> { 149.66, 128.57 }); //
            points.Add(914, new List<double> { 163.49, 130.62 }); //
            points.Add(953, new List<double> { 177.66, 132.65 }); //
            points.Add(991, new List<double> { 191.48, 131.78 }); //

            points.Add(1000, new List<double> { 194.5 , 129.7  }); //
            points.Add(1010, new List<double> { 196.75, 126.74 }); //
            points.Add(1020, new List<double> { 198.4 , 123.57 }); //

            points.Add(1030, new List<double> { 199.25, 120.11 });

            points.Add(1042, new List<double> { 199.39, 116.09 }); //
            points.Add(1055, new List<double> { 198.37, 112.21 }); //
            points.Add(1067, new List<double> { 196.19, 108.61 }); //


            points.Add(1080, new List<double> { 192.97, 105.93 }); //
            points.Add(1131, new List<double> { 176.14, 100.66 }); //
            points.Add(1181, new List<double> { 159.61, 97.08  }); //
            points.Add(1232, new List<double> { 142.77, 93.31  }); //
            points.Add(1282, new List<double> { 126.25, 89.83  }); //
            points.Add(1333, new List<double> { 109.41, 86.06  }); //
            points.Add(1383, new List<double> { 92.88 , 82.58  }); //
            points.Add(1434, new List<double> { 76.05 , 80.01  });
            points.Add(1459, new List<double> { 66.63, 83.23 }); //
            points.Add(1509, new List<double> { 50.4, 93.25  }); //
            points.Add(1535, new List<double> { 42.85 , 99.41  });
            points.Add(1560, new List<double> { 40.56, 107.72 }); //
            points.Add(1610, new List<double> { 37.42, 124.62 }); //
            points.Add(1636, new List<double> { 36.85 , 133.41 });
            points.Add(1648, new List<double> { 38.26, 137.26 }); //
            points.Add(1661, new List<double> { 40.56, 140.76 }); //
            points.Add(1673, new List<double> { 42.32, 144.27 }); //
            points.Add(1686, new List<double> { 43.22, 148.07 }); //
            points.Add(1698, new List<double> { 43.05, 152.18 }); //
            points.Add(1711, new List<double> { 42.04, 156.38 }); //
            points.Add(1724, new List<double> { 40.12, 159.99 }); //
            points.Add(1737, new List<double> { 37.63, 162.99 }); //
            points.Add(1749, new List<double> { 33.89, 165.69  }); //
            points.Add(1762, new List<double> { 29.48, 167.13  }); //
            points.Add(1775, new List<double> { 24.66, 167.36  }); //
            points.Add(1788, new List<double> { 20.14, 166.50 }); //
            points.Add(1800, new List<double> { 15.95, 164.44 }); //
            points.Add(1813, new List<double> { 12.59, 161.1  }); //
            points.Add(1826, new List<double> { 10.22, 156.95 }); //
            points.Add(1839, new List<double> { 9.15  , 152.41 });

            points.Add(1879, new List<double> { 9.01, 138.4 }); //


            points.Add(1999, new List<double> { 12.25 , 95.01  });

            points.Add(2009, new List<double> { 14.33, 92.03 }); //
            points.Add(2019, new List<double> { 17.25, 89.55 }); //
            points.Add(2029, new List<double> { 20.67, 87.26 }); //


            points.Add(2040, new List<double> { 22.75 , 83.91  });

            points.Add(2077, new List<double> { 23.86, 71.75 }); //
            points.Add(2115, new List<double> { 30.02, 60.14 }); //
            points.Add(2153, new List<double> { 40.49, 52.92 }); //

            points.Add(2191, new List<double> { 54.05 , 52.01 });

            points.Add(2676, new List<double> { 215.33, 88.03 }); //

            points.Add(2746, new List<double> { 238.05, 94.41  });

            points.Add(2771, new List<double> { 243.95, 102.3 }); //
            points.Add(2796, new List<double> { 247.59, 111.45 }); //
            points.Add(2821, new List<double> { 250.69, 120.81 }); //

            points.Add(2847, new List<double> { 255.55, 129.11 });

            points.Add(2889, new List<double> { 268.82, 132.87 }); //
            points.Add(3060, new List<double> { 328.2, 142.33  }); //


            points.Add(3103, new List<double> { 342.05, 147.11 });

            points.Add(3111, new List<double> { 344.11, 149.46 }); //
            points.Add(3120, new List<double> { 345.66, 152.34 }); //
            points.Add(3129, new List<double> { 346.71, 155.22 }); //

            points.Add(3280, new List<double> { 360.94, 203.49 }); //


            points.Add(3316, new List<double> { 356.90, 215.01 });

            points.Add(3334, new List<double> { 352.15, 218.29 }); //
            points.Add(3352, new List<double> { 346.01, 218.95 }); //
            points.Add(3370, new List<double> { 340.17, 217.51 }); //

            points.Add(3389, new List<double> { 334.22, 215.57 }); //



            points.Add(3610, new List<double>{ 261.55, 191.91 });
            

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
