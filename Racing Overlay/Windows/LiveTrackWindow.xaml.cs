using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IRacing_Standings.Windows
{
    /// <summary>
    /// Interaction logic for LiveTrackWindow.xaml
    /// </summary>
    /// 
   
    public partial class LiveTrackWindow : Window
    {
        TelemetryData LocalTelemetry;
        public bool Locked = false;
        public LiveTrackWindow(TelemetryData telemetryData)
        {
            LocalTelemetry = telemetryData;
            InitializeComponent();
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            Locked = bool.Parse(mainWindow.WindowSettings.LiveTrackSettings["Locked"] ?? "false");
            Left = double.Parse(mainWindow.WindowSettings.LiveTrackSettings["XPos"] ?? "0");
            Top = double.Parse(mainWindow.WindowSettings.LiveTrackSettings["YPos"] ?? "0");

            for (int i= 0; i < 1; i++)
            {
                var colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1500);
            }
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
                    var textBox = new TextBox();
                    textBox.Visibility = Visibility.Visible;
                    textBox.Text = driver.ClassPosition.ToString();
                    textBox.Uid = $"{driver.ClassId}-{driver.CarId}";
                    textBox.Width = 30;
                    textBox.Height = 30;
                    textBox.Margin = new Thickness(driver.PosOnTrack / LocalTelemetry.TrackLength * 1500 - 15, 1.65, 0, 0);
                    textBox.FontWeight = FontWeights.Bold;
                    textBox.FontSize = 16;
                    textBox.TextAlignment = TextAlignment.Center;
                    textBox.HorizontalAlignment = HorizontalAlignment.Left;
                    textBox.Padding = new Thickness(0, 4, 0, 0);
                    textBox.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(driver.ClassColor.Replace("0x", "#"));
                    textBox.Foreground = Brushes.Black;
                    textBox.Resources = Player.Resources;
                    textBox.BorderThickness = new Thickness(0);
                    Canvas.SetZIndex(textBox, 99 - (driver.OverallPosition ?? 99));

                    if (driver.CarId == LocalTelemetry.FeedTelemetry.CamCarIdx) 
                    {
                        textBox.BorderBrush = Brushes.LawnGreen;
                        textBox.BorderThickness = new Thickness(4);
                        textBox.Padding = new Thickness(-1, 0, 0, 0);
                        Canvas.SetZIndex(textBox, 99);
                    }
 

                   List<UIElement> elementsToRemove = new List<UIElement>();
                    foreach (UIElement uiElement in this. TrackCanvas.Children.OfType<TextBox>())
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
