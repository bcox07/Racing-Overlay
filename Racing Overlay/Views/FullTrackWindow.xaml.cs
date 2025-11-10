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
        public FullTrackWindow(TelemetryData telemetryData, GlobalSettings globalSettings)
        {
            LocalTelemetry = telemetryData;
            
            InitializeComponent();
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            Locked = bool.Parse(mainWindow.WindowSettings.FullTrackSettings["Locked"] ?? "false");
            Left = double.Parse(mainWindow.WindowSettings.FullTrackSettings["XPos"] ?? "0");
            Top = double.Parse(mainWindow.WindowSettings.FullTrackSettings["YPos"] ?? "0");
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
            map = (DrawingImage)mapResourceDictionary["di_Image"];

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

        private Dictionary<string, List<double>> GetTrackJsonData()
        {
            if (!HasTrackCoordinates())
                return null;

            var items = new Dictionary<string, List<double>>();
            using (var reader = new StreamReader($"./assets/tracks/{LocalTelemetry.TrackId}-{LocalTelemetry.TrackName}/coordinates.json"))
            {
                string json = reader.ReadToEnd();
                items = JsonConvert.DeserializeObject<Dictionary<string, List<double>>>(json);
            }
            return items;
        }

        private void DisplayTrackMap()
        {
            if (UpdatedWidth != DefaultWidth * (_GlobalSettings.UISize.Percentage / 100.0))
            {
                UpdatedWidth = DefaultWidth * (_GlobalSettings.UISize.Percentage / 100.0);
                var updatedHeight = 300 * (_GlobalSettings.UISize.Percentage / 100.0);
                Dispatcher.Invoke(() =>
                {
                    var transformData = GetTrackMapTransformData();
                    if (UpdatedWidth > DefaultWidth * (_GlobalSettings.UISize.Percentage / 100.0))
                    {
                        Width = UpdatedWidth;
                        Height = updatedHeight;
                        TrackMapViewbox.Width = UpdatedWidth * transformData.Item2;
                        TrackMapViewbox.Height = updatedHeight * transformData.Item2;
                        TrackMapViewbox.Margin = new Thickness(transformData.Item3, transformData.Item4, 0, 0);
                    }
                    else
                    {
                        TrackMapViewbox.Width = UpdatedWidth * transformData.Item2;
                        TrackMapViewbox.Height = updatedHeight * transformData.Item2;
                        TrackMapViewbox.Margin = new Thickness(transformData.Item3, transformData.Item4, 0, 0);
                        Width = UpdatedWidth;
                        Height = updatedHeight;
                    }
                    TrackMap.Visibility = Visibility.Visible;
                });
            }

            
            //var generatedCoordinates = GenerateCoordinates();
            //GetPointsBetween(3, 3718, 3801, generatedCoordinates);
            //GetPointsBetween(3, 3970, 4283, generatedCoordinates);

            foreach (var driver in LocalTelemetry.AllPositions)
            {
                Dispatcher.Invoke(() =>
                {
                    var textBox = new TextBox();
                    textBox.Visibility = Visibility.Visible;
                    textBox.Text = driver.ClassPosition.ToString();
                    textBox.Uid = $"{driver.ClassId}-{driver.CarId}";
                    textBox.Width = 22 * (_GlobalSettings.UISize.Percentage / 100.0);
                    textBox.Height = 22 * (_GlobalSettings.UISize.Percentage / 100.0);
                    textBox.Margin = new Thickness((30 - textBox.Width) / 2);
                    var loc = GetTrackJsonData();

                    //if (driver.Name.StartsWith("Brian D"))
                    //{
                    //    Console.WriteLine((int)driver.PosOnTrack);
                    //}

                    if (loc == null)
                    {
                        return;
                    }
                    
                    var coordinates = new List<double> { 0, 0 };
                    var prevCoordinates = new List<double> { 0, 0 };
                    var nextCoordinates = new List<double> { 0, 0 };
                    loc.TryGetValue(((int)driver.PosOnTrack).ToString(), out coordinates);
                    
                    if (coordinates == null)
                    {
                        loc.TryGetValue((((int)driver.PosOnTrack) - 1).ToString(), out prevCoordinates);
                        loc.TryGetValue((((int)driver.PosOnTrack) + 1).ToString(), out nextCoordinates);
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
                   
                    textBox.FontWeight = FontWeights.Bold;
                    textBox.FontSize = _GlobalSettings.UISize.SimpleTrackSettings.FontSize + 2;
                    textBox.TextAlignment = TextAlignment.Center;
                    textBox.HorizontalAlignment = HorizontalAlignment.Center;
                    textBox.VerticalAlignment = VerticalAlignment.Center;
                    textBox.Padding = new Thickness(0, 1, 0, 0);
                    textBox.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(driver.ClassColor.Replace("0x", "#"));
                    textBox.Foreground = Brushes.Black;
                    textBox.Resources = Player.Resources;
                    textBox.BorderThickness = new Thickness(0);

                    Canvas.SetLeft(textBox, coordinates[0] * (_GlobalSettings.UISize.Percentage / 100.0));
                    Canvas.SetTop(textBox, coordinates[1] * (_GlobalSettings.UISize.Percentage / 100.0));
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

                    foreach (var element in elementsToRemove)
                    {
                        TrackCanvas.Children.Remove(element);
                    }

                    if (driver.PosOnTrack > 0) 
                    {
                        TrackCanvas.Children.Add(textBox);
                    }
                });
            }
        }

        private void TraceTrackLine()
        {
            var loc = GetTrackJsonData();
            if (loc != null)
            {
                foreach (var coordinate in loc)
                {
                    if (int.Parse(coordinate.Key) % 4 == 0)
                    {
                        var textBoxTemp = new TextBox();
                        textBoxTemp.Visibility = Visibility.Visible;
                        textBoxTemp.Text = $"{Math.Round(double.Parse(coordinate.Key) / 1000, 1)}";
                        textBoxTemp.VerticalAlignment = VerticalAlignment.Center;
                        textBoxTemp.TextAlignment = TextAlignment.Center;
                        textBoxTemp.Width = 2;
                        textBoxTemp.Height = 2;
                        textBoxTemp.Margin = new Thickness(14);
                        textBoxTemp.BorderThickness = new Thickness(0);
                        textBoxTemp.Background = Brushes.Green;
                        Canvas.SetZIndex(textBoxTemp, 99);

                        if (int.Parse(coordinate.Key) % 200 == 0)
                        {
                            textBoxTemp.FontSize = 10;
                            textBoxTemp.FontWeight = FontWeights.Bold;
                            textBoxTemp.Width = 20;
                            textBoxTemp.Height = 20;
                            textBoxTemp.Margin = new Thickness(5);
                            textBoxTemp.Background = Brushes.Transparent;
                            textBoxTemp.Foreground = Brushes.White;
                            Canvas.SetZIndex(textBoxTemp, 100);
                        }


                        Canvas.SetLeft(textBoxTemp, coordinate.Value[0] * (_GlobalSettings.UISize.Percentage / 100.0));
                        Canvas.SetTop(textBoxTemp, coordinate.Value[1] * (_GlobalSettings.UISize.Percentage / 100.0));

                        TrackCanvas.Children.Add(textBoxTemp);
                    }
                }
            }
        }

        private Dictionary<int, (double, double)> GenerateCoordinates()
        {
            var fileLocation = $"..\\..\\trackline.txt";
            var points = new Dictionary<int, List<double>>();
            points.Add(0, new List<double> { 282, 209 });

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
            var step = (b - a) / (pointCount + 1);

            for (int i = a; i <= b; i += step)
            {
                Console.WriteLine($"{i}: Canvas.Left=\"{coordinates[i].Item1}\" Canvas.Top=\"{coordinates[i].Item2}\"");
            }
            Console.WriteLine();
        }
    }
}
