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
        public FullTrackWindow(TelemetryData telemetryData)
        {
            LocalTelemetry = telemetryData;
            InitializeComponent();
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            Locked = bool.Parse(mainWindow.WindowSettings.FullTrackSettings["Locked"] ?? "false");
            Left = double.Parse(mainWindow.WindowSettings.FullTrackSettings["XPos"] ?? "0");
            Top = double.Parse(mainWindow.WindowSettings.FullTrackSettings["YPos"] ?? "0");

            if (HasTrackMap())
            {
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

        public bool HasTrackMap()
        {
            return File.Exists($"TrackMaps/{LocalTelemetry.TrackId}-{LocalTelemetry.TrackName.ToLower()}/map.png");
        }

        public bool HasTrackCoordinates()
        {
            return File.Exists($"TrackMaps/{LocalTelemetry.TrackId}-{LocalTelemetry.TrackName.ToLower()}/coordinates.json");
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
                    if (int.Parse(coordinate.Key) % 8 == 0)
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
            if (!HasTrackCoordinates())
                return null;

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
            points.Add(0   , new List<double> { 185.5, 224   });

            points.Add(118, new List<double> { 211, 221.5 });
            points.Add(236, new List<double> { 236, 214.5 });
            points.Add(354, new List<double> { 259, 205.5 });
            points.Add(414, new List<double> { 271, 196 });
            points.Add(474 , new List<double> { 275, 184     });
            points.Add(522, new List<double> { 269, 172   });
            points.Add(570, new List<double> { 255, 167.4 });
            points.Add(618 , new List<double> { 241, 166     });
            points.Add(680 , new List<double> { 230, 159     });
            points.Add(914, new List<double> { 182, 157.5 });

            points.Add(936, new List<double> { 175, 155 });

            points.Add(958 , new List<double> { 172.5, 149   });

            points.Add(980, new List<double> { 176, 143 });

            points.Add(1002, new List<double> { 181, 140.5 });
            points.Add(1060, new List<double> { 194, 138.5 });
            points.Add(1430, new List<double> { 274, 137     });
            points.Add(1498, new List<double> { 288, 129 });
            points.Add(1708, new List<double> { 324, 100 });
            points.Add(1760, new List<double> { 335, 101     });
            points.Add(1828, new List<double> { 337, 113 });
            points.Add(1922, new List<double> { 324, 131 });
            points.Add(2090, new List<double> { 293, 158 });
            points.Add(2140, new List<double> { 287, 169     });
            points.Add(2178, new List<double> { 292, 176 });
            points.Add(2224, new List<double> { 304, 182 });
            points.Add(2284, new List<double> { 318, 182     });
            points.Add(2394, new List<double> { 338.5, 170 });
            points.Add(2504, new List<double> { 358, 150   });
            points.Add(2614, new List<double> { 367.5, 127 });
            points.Add(2726, new List<double> { 368, 101   });
            points.Add(2834, new List<double> { 359, 78    });
            points.Add(2946, new List<double> { 344, 61    });
            points.Add(3056, new List<double> { 321, 49 });
            points.Add(3166, new List<double> { 295, 44.5    });
            points.Add(3784, new List<double> { 162, 43.5    });
            points.Add(3858, new List<double> { 148, 52 });
            points.Add(3892, new List<double> { 139, 54 });
            points.Add(3930, new List<double> { 129, 53 });
            points.Add(3980, new List<double> { 121, 45.5 });
            points.Add(4006, new List<double> { 115, 43.5    });
            points.Add(4146, new List<double> { 82 , 43   });
            points.Add(4286, new List<double> { 50 , 48   });
            points.Add(4426, new List<double> { 24 , 64   });
            points.Add(4566, new List<double> { 8  , 86   });
            points.Add(4706, new List<double> { 2  , 112  });
            points.Add(4846, new List<double> { 8  , 138  });
            points.Add(4986, new List<double> { 24 , 161  });
            points.Add(5126, new List<double> { 52 , 180  });
            points.Add(5266, new List<double> { 83 , 193  });
            points.Add(5406, new List<double> { 115, 206  });
            points.Add(5546, new List<double> { 149, 218 });
            points.Add(5686, new List<double> { 185.5, 224   });

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

        private void DisplayTrackMap()
        {
            //GenerateCoordinates();
            
            foreach (var driver in LocalTelemetry.AllPositions)
            {
                Dispatcher.Invoke(() =>
                {
                    var textBox = new TextBox();
                    textBox.Visibility = Visibility.Visible;
                    textBox.Text = driver.ClassPosition.ToString();
                    textBox.Uid = $"{driver.ClassId}-{driver.CarId}";
                    textBox.Width = 22;
                    textBox.Height = 22;
                    textBox.Margin = new Thickness(4);
                    var loc = GetTrackJsonData();

                    if (driver.Name.StartsWith("Dylan Hs"))
                    {
                        Console.WriteLine((int)driver.PosOnTrack);
                    }

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
                    textBox.FontSize = 14;
                    textBox.TextAlignment = TextAlignment.Center;
                    textBox.HorizontalAlignment = HorizontalAlignment.Center;
                    textBox.Padding = new Thickness(0, 1, 0, 0);
                    textBox.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(driver.ClassColor.Replace("0x", "#"));
                    textBox.Foreground = Brushes.Black;
                    textBox.Resources = Player.Resources;
                    textBox.BorderThickness = new Thickness(0);

                    Canvas.SetLeft(textBox, coordinates[0]);
                    Canvas.SetTop(textBox, coordinates[1]);
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
