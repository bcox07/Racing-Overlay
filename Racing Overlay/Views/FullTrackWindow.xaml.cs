using IRacing_Standings.Models;
using Newtonsoft.Json;
using NLog;
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

namespace IRacing_Standings.Windows
{
    /// <summary>
    /// Interaction logic for FullTrackWindow.xaml
    /// </summary>
    /// 
   
    public partial class FullTrackWindow : Window
    {
        TelemetryData LocalTelemetry;
        public bool Locked = false;
        public FullTrackWindow(TelemetryData telemetryData)
        {
            LocalTelemetry = telemetryData;
            InitializeComponent();
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            Locked = bool.Parse(mainWindow.WindowSettings.FullTrackSettings["Locked"] ?? "false");
            Left = double.Parse(mainWindow.WindowSettings.FullTrackSettings["XPos"] ?? "0");
            Top = double.Parse(mainWindow.WindowSettings.FullTrackSettings["YPos"] ?? "0");

            var nonGenericTrack = new BitmapImage();
            using (var stream = File.OpenRead($"TrackMaps/{LocalTelemetry.TrackId}-{LocalTelemetry.TrackName.ToLower()}/map.png"))
            {
                nonGenericTrack.BeginInit();
                nonGenericTrack.CacheOption = BitmapCacheOption.OnLoad; // Crucial for releasing the file lock
                nonGenericTrack.StreamSource = stream;
                nonGenericTrack.EndInit();
            }

            TrackMap.Source = nonGenericTrack;
            TrackMap.Source.Freeze();

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
                    if (int.Parse(coordinate.Key) % 2 == 0)
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

                        if (int.Parse(coordinate.Key) % 100 == 0)
                        {
                            //textBoxTemp.FontSize = 12;
                            //textBoxTemp.FontWeight = FontWeights.Bold;
                            //textBoxTemp.Width = 20;
                            //textBoxTemp.Height = 20;
                            //textBoxTemp.Margin = new Thickness(5);
                            //textBoxTemp.Background = Brushes.Transparent;
                            //textBoxTemp.Foreground = Brushes.White;
                            //Canvas.SetZIndex(textBoxTemp, 100);
                        }

                        
                        Canvas.SetLeft(textBoxTemp, coordinate.Value[0]);
                        Canvas.SetTop(textBoxTemp, coordinate.Value[1]);
                        
                        TrackCanvas.Children.Add(textBoxTemp);
                    }
                }
            }
        }

        private Dictionary<string, List<double>> GetTrackJsonData()
        {
            var items = new Dictionary<string, List<double>>();
            using (var reader = new StreamReader($"./TrackMaps/{LocalTelemetry.TrackId}-{LocalTelemetry.TrackName}/coordinates.json"))
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
            points.Add(0, new List<double> { 284, 204 });
            points.Add(230, new List<double> { 240, 188 });
            points.Add(428, new List<double> { 202, 170 });
            points.Add(624, new List<double> { 192, 132 });
            points.Add(905, new List<double> { 152, 99 });
            points.Add(1041, new List<double> { 172, 80 });
            points.Add(1151, new List<double> { 151, 74 });
            points.Add(1243, new List<double> { 133, 80 });
            points.Add(1900, new List<double> { 77, 198 }); 
            points.Add(1983, new List<double> { 83, 213 });
            points.Add(2099, new List<double> { 103, 212 });
            points.Add(2133, new List<double> { 109, 214 });
            points.Add(2167, new List<double> { 113, 222 });
            points.Add(2216, new List<double> { 111, 229 });
            points.Add(2278, new List<double> { 96, 232.5 });
            points.Add(2341, new List<double> { 81, 230 });
            points.Add(2532, new List<double> { 48, 213 });
            points.Add(2995, new List<double> { 13, 126 });
            points.Add(3058, new List<double> { 14, 114 });
            points.Add(3295, new List<double> { 55, 87 });
            points.Add(3584, new List<double> { 104, 68 });
            points.Add(3679, new List<double> { 118, 55 });
            points.Add(3844, new List<double> { 150, 54 });
            points.Add(3975, new List<double> { 173, 37 });
            points.Add(4131, new List<double> { 193, 49 });
            points.Add(4938, new List<double> { 346, 85.5 });
            points.Add(5014, new List<double> { 360, 101 });
            points.Add(5135, new List<double> { 350, 121 });
            points.Add(5340, new List<double> { 325, 155 });
            points.Add(5456, new List<double> { 314, 175 });
            points.Add(5524, new List<double> { 324, 186 });
            points.Add(5610, new List<double> { 317, 199 });
            points.Add(5698, new List<double> { 301, 210 });
            points.Add(5796, new List<double> { 284, 204 });

            var locationOnTrack = 0;
            var x = points.Values.First()[0];
            var y = points.Values.First()[1];
            using (StreamWriter writer = new StreamWriter(fileLocation))
            {
                writer.WriteLine("{");
                foreach (var corner in points)
                {
                    var xDev = (corner.Value[0] - x) / (corner.Key - locationOnTrack);
                    var yDev = (corner.Value[1] - y) / (corner.Key - locationOnTrack);

                    while (locationOnTrack < corner.Key)
                    {
                        writer.WriteLine($"\t\"{locationOnTrack}\": [{Math.Round(x, 2)}, {Math.Round(y, 2)}]");
                        locationOnTrack++;
                        x += xDev;
                        y += yDev;
                    }
                }
                writer.WriteLine("}");
            }
        }

        private void DisplayTrackMap()
        {
            //GenerateCoordinates();
            foreach(var driver in LocalTelemetry.AllPositions)
            {
                Dispatcher.Invoke(() =>
                {
                    var textBox = new TextBox();
                    textBox.Visibility = Visibility.Visible;
                    textBox.Text = driver.ClassPosition.ToString();
                    textBox.Uid = $"{driver.ClassId}-{driver.CarId}";
                    textBox.Width = 25;
                    textBox.Height = 25;
                    textBox.Margin = new Thickness(2.5);
                    var loc = GetTrackJsonData();

                    if (driver.Name.StartsWith("Unai"))
                    {
                        Console.WriteLine((int)driver.PosOnTrack);
                    }

                    if (loc == null)
                    {
                        return;
                    }
                    
                    var coordinates = new List<double> { 0, 0 };
                    loc.TryGetValue(((int)driver.PosOnTrack).ToString(), out coordinates);

                    
                    
                    if (coordinates == null)
                    {
                        coordinates = new List<double> { -30, -30 };
                    }

                    Canvas.SetLeft(textBox, coordinates[0]);
                    Canvas.SetTop(textBox, coordinates[1]);
                    //textBox.Margin = new Thickness(driver.PosOnTrack / LocalTelemetry.TrackLength * 1500 - 15, 1.65, 0, 0);
                    textBox.FontWeight = FontWeights.Bold;
                    textBox.FontSize = 14;
                    textBox.TextAlignment = TextAlignment.Center;
                    textBox.HorizontalAlignment = HorizontalAlignment.Center;
                    textBox.Padding = new Thickness(0, 3, 0, 0);
                    textBox.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(driver.ClassColor.Replace("0x", "#"));
                    textBox.Foreground = Brushes.Black;
                    textBox.Resources = Player.Resources;
                    textBox.BorderThickness = new Thickness(0);
                    Canvas.SetZIndex(textBox, 99 - (driver.OverallPosition ?? 99));

                    if (driver.CarId == LocalTelemetry.FeedTelemetry.CamCarIdx) 
                    {
                        textBox.BorderBrush = Brushes.OrangeRed;
                        textBox.BorderThickness = new Thickness(3);
                        textBox.Padding = new Thickness(0, -1, 0, 0);
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
    }
}
