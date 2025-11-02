using Newtonsoft.Json;
using NLog;
using RacingOverlay.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static iRacingSDK.SessionData._DriverInfo;

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
            var test = (ResourceDictionary)Application.Current.Resources[$"{LocalTelemetry.TrackId}-{LocalTelemetry.TrackName.ToLower()}"];
            map = (DrawingImage)test["di_Image"];

            return map != null;
        }

        public (string, double, double, double) GetTrackMapTransformData()
        {
            var transformData = new List<(string, double, double, double)>();
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
                            transformData.Add((data[0], double.Parse(data[1]), double.Parse(data[2]), double.Parse(data[3])));
                        }
                    }
                }
                catch (IOException ex)
                {
                    Trace.WriteLine(ex);
                }
            }

            return transformData.Where(t => t.Item1 == $"{LocalTelemetry.TrackId}-{LocalTelemetry.TrackName.ToLower()}").FirstOrDefault();
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

        private void GenerateCoordinates()
        {
            var fileLocation = $"..\\..\\trackline.txt";
            var points = new Dictionary<int, List<double>>();
            points.Add(0   , new List<double> { 264, 184 });

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
        }

        private void GetPointsBetween(int pointCount, int a, int b)
        {
            var step = (b - a) / (pointCount + 1);

            for (int i = a; i <= b; i += step)
            {
                Console.Write(i + " ");
            }
            Console.WriteLine();
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

            //GetPointsBetween(3, 1533, 1630);
            //GetPointsBetween(5, 3312, 3607);
            //GenerateCoordinates();

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
                    textBox.FontSize = _GlobalSettings.UISize.SimpleTrackSettings["FontSize"] + 2;
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
