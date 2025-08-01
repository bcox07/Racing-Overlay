﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace IRacing_Standings.Windows
{
    /// <summary>
    /// Interaction logic for SimpleTrackWindow.xaml
    /// </summary>
    /// 

    public partial class SimpleTrackWindow : Window
    {
        TelemetryData LocalTelemetry;
        public bool Locked = false;
        public SimpleTrackWindow(TelemetryData telemetryData)
        {
            LocalTelemetry = telemetryData;
            InitializeComponent();
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            Locked = bool.Parse(mainWindow.WindowSettings.SimpleTrackSettings["Locked"] ?? "false");
            Left = double.Parse(mainWindow.WindowSettings.SimpleTrackSettings["XPos"] ?? "0");
            Top = double.Parse(mainWindow.WindowSettings.SimpleTrackSettings["YPos"] ?? "0");

            for (int i= 0; i < 1; i++)
            {
                var colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1470);
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
                    textBox.Padding = new Thickness(0, 3, 0, 0);
                    textBox.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(driver.ClassColor.Replace("0x", "#"));
                    textBox.Foreground = Brushes.Black;
                    textBox.Resources = Player.Resources;
                    textBox.BorderThickness = new Thickness(0);
                    Canvas.SetZIndex(textBox, 99 - (driver.OverallPosition ?? 99));

                    if (driver.CarId == LocalTelemetry.FeedTelemetry.CamCarIdx) 
                    {
                        textBox.BorderBrush = Brushes.OrangeRed;
                        textBox.BorderThickness = new Thickness(4);
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

                    elementsToRemove.ForEach(element => TrackCanvas.Children.Remove(element));

                    if (driver.PosOnTrack > 0) 
                    {
                        TrackCanvas.Children.Add(textBox);
                    }
                });
            }
        }
    }
}
