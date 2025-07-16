using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace IRacing_Standings
{
    /// <summary>
    /// Interaction logic for RelativeWindow.xaml
    /// </summary>
    public partial class RelativeWindow : Window
    {
        private int CellIndex;
        public bool Locked = false;
        TelemetryData LocalTelemetry;
        public RelativeWindow(TelemetryData telemetryData)
        {
            LocalTelemetry = telemetryData;
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            Locked = bool.Parse(mainWindow.WindowSettings.RelativeSettings["Locked"]);
            Left = double.Parse(mainWindow.WindowSettings.RelativeSettings["XPos"]);
            Top = double.Parse(mainWindow.WindowSettings.RelativeSettings["YPos"]);
            InitializeComponent();
            InitializeGrid();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!Locked)
            {
                base.OnMouseLeftButtonDown(e);
                DragMove();
            }
        }

        public void InitializeGrid()
        {
            Dispatcher.Invoke(() =>
            {
                Topmost = true;
                RelativeGrid.Background = Brushes.Transparent;
                for (var i = 0; i < 30; i++)
                {
                    var colDef = new ColumnDefinition();
                    colDef.Width = new GridLength(10);
                    RelativeGrid.ColumnDefinitions.Add(colDef);
                }
            });
            
        }

        public void UpdateTelemetryData(TelemetryData telemetryData)
        {
            LocalTelemetry = new TelemetryData(telemetryData);
            if (LocalTelemetry != null && LocalTelemetry.IsReady)
            {
                DisplayRelative();
            }
        }

        private void DisplayRelative()
        {
            var allPositions = new List<Driver>(LocalTelemetry.AllPositions);
            Dispatcher.Invoke(() => { Topmost = true; });
            var viewedCar = allPositions.Where(p => p.CarId == LocalTelemetry.FeedTelemetry.CamCarIdx).FirstOrDefault() ?? allPositions.FirstOrDefault() ?? new Driver();

            var surroundingCars = new List<Driver>();
            foreach (var localPosition in allPositions)
            {
                if (localPosition.PosOnTrack > 0 && viewedCar.PosOnTrack > 0)
                {
                    var targetDistanceFromStart = LocalTelemetry.TrackLength - localPosition.PosOnTrack;
                    var viewedDistanceFromStart = LocalTelemetry.TrackLength - viewedCar.PosOnTrack;
                    if (Math.Abs(viewedCar.PosOnTrack - localPosition.PosOnTrack) <= (LocalTelemetry.TrackLength / 2) ||
                        LocalTelemetry.TrackLength - localPosition.PosOnTrack + viewedCar.PosOnTrack <= (LocalTelemetry.TrackLength / 2) ||
                        LocalTelemetry.TrackLength - viewedCar.PosOnTrack + localPosition.PosOnTrack <= (LocalTelemetry.TrackLength / 2))
                    {
                        localPosition.Delta = LocalTelemetry.GetRelativeDelta(viewedCar, localPosition, LocalTelemetry.TrackLength);
                        surroundingCars.Add(localPosition);
                    }
                }
            }
            surroundingCars = surroundingCars.OrderByDescending(s => s.Delta).ToList();
            var newSurroundingCars = new List<Driver>();
            foreach (var car in surroundingCars)
            {
                if (Math.Abs(surroundingCars.IndexOf(car) - surroundingCars.IndexOf(viewedCar)) < 4)
                {
                    newSurroundingCars.Add(car);
                }
            }

            var rowIndex = 0;
            CellIndex = 0;
            foreach (var car in newSurroundingCars)
            {
                rowIndex = GenerateRow(car, viewedCar, rowIndex, LocalTelemetry);
            }
            Dispatcher.Invoke(() =>
            {
                RelativeGeometry.Rect = new Rect(0, 0, 300, rowIndex * 27);
                RelativeGrid.Width = 300;
            });
            Dispatcher.Invoke(() =>
            {
                for (var i = rowIndex; i < RelativeGrid.RowDefinitions.Count; i++)
                {
                    RelativeGrid.RowDefinitions.RemoveAt(i);
                }
                for (var i = CellIndex; i < RelativeGrid.Children.Count; i++)
                {
                    RelativeGrid.Children.RemoveAt(i);
                }
            });
            Thread.Sleep(16);
        }

        private int GenerateRow(Driver driver, Driver viewedDriver, int rowIndex, TelemetryData telemetryData)
        {
            Dispatcher.Invoke(() =>
            {
                
                if (rowIndex >= RelativeGrid.RowDefinitions.Count)
                {
                    var rowDef = new RowDefinition();
                    rowDef.Height = new GridLength(27);
                    RelativeGrid.RowDefinitions.Add(rowDef);
                }

                var posNumber = CellIndex < RelativeGrid.Children.Count ? (TextBlock)RelativeGrid.Children[CellIndex] : null;
                if (posNumber == null)
                {
                    posNumber = new TextBlock();
                    posNumber.Tag = "PosNumber";
                    UpdateDriverCell(posNumber, rowIndex, driver, viewedDriver, telemetryData);
                    posNumber.TextAlignment = TextAlignment.Center;
                    posNumber.HorizontalAlignment = HorizontalAlignment.Stretch;
                    posNumber.Padding = new Thickness(2);
                    posNumber.Text = driver.ClassPosition.ToString();
                    RelativeGrid.Children.Add(posNumber);
                }
                else
                {
                    posNumber.Tag = "PosNumber";
                    UpdateDriverCell(posNumber, rowIndex, driver, viewedDriver, telemetryData);
                    posNumber.TextAlignment = TextAlignment.Center;
                    posNumber.HorizontalAlignment = HorizontalAlignment.Stretch;
                    posNumber.Padding = new Thickness(2);
                    posNumber.Text = driver.ClassPosition.ToString();
                }
                Grid.SetColumnSpan(posNumber, 3);
                Grid.SetColumn(posNumber, 0);
                Grid.SetRow(posNumber, rowIndex);
                CellIndex++;

                var classColor = CellIndex < RelativeGrid.Children.Count ? (TextBlock)RelativeGrid.Children[CellIndex] : null;
                if (classColor == null)
                {
                    classColor = new TextBlock();
                    classColor.Tag = "ClassColor";
                    UpdateDriverCell(classColor, rowIndex, driver, viewedDriver, telemetryData);
                    classColor.HorizontalAlignment = HorizontalAlignment.Stretch;
                    classColor.Padding = new Thickness(2);
                    var test = driver.ClassColor.Replace("0x", "#");
                    classColor.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(test);
                    RelativeGrid.Children.Add(classColor);
                }
                else
                {
                    classColor.Tag = "ClassColor";
                    UpdateDriverCell(classColor, rowIndex, driver, viewedDriver, telemetryData);
                    classColor.HorizontalAlignment = HorizontalAlignment.Stretch;
                    classColor.Padding = new Thickness(2);
                    var test = driver.ClassColor.Replace("0x", "#");
                    classColor.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(test);
                    classColor.Text = "";
                }
                Grid.SetColumnSpan(classColor, 1);
                Grid.SetColumn(classColor, 3);
                Grid.SetRow(classColor, rowIndex);
                CellIndex++;

                var driverName = CellIndex < RelativeGrid.Children.Count ? (TextBlock)RelativeGrid.Children[CellIndex] : null;
                if (driverName == null)
                {
                    driverName = new TextBlock();
                    driverName.Tag = "DriverName";
                    UpdateDriverCell(driverName, rowIndex, driver, viewedDriver, telemetryData);
                    driverName.Padding = new Thickness(5, 2, 0, 3.5);
                    driverName.HorizontalAlignment = HorizontalAlignment.Stretch;
                    driverName.TextTrimming = TextTrimming.CharacterEllipsis;
                    RelativeGrid.Children.Add(driverName);
                }
                else
                {
                    driverName.Tag = "DriverName";
                    UpdateDriverCell(driverName, rowIndex, driver, viewedDriver, telemetryData);
                    driverName.Padding = new Thickness(5, 2, 0, 3.5);
                    driverName.TextAlignment = TextAlignment.Left;
                    driverName.HorizontalAlignment = HorizontalAlignment.Stretch;
                    driverName.TextTrimming = TextTrimming.CharacterEllipsis;
                    if (driverName.Text != Regex.Replace(driver.Name, @"( .+ )", " "))
                    {
                        driverName.Text = Regex.Replace(driver.Name, @"( .+ )", " ");
                    }
                }
                Grid.SetColumn(driverName, 4);
                Grid.SetColumnSpan(driverName, 16);
                Grid.SetRow(driverName, rowIndex);
                CellIndex++;

                var iRating = CellIndex < RelativeGrid.Children.Count ? (TextBlock)RelativeGrid.Children[CellIndex] : null;
                if (iRating == null)
                {
                    iRating = new TextBlock();
                    iRating.Tag = "IRating";
                    UpdateDriverCell(iRating, rowIndex, driver, viewedDriver, telemetryData);
                    iRating.HorizontalAlignment = HorizontalAlignment.Center;
                    iRating.Padding = new Thickness(12, 32, 12, 3.5);
                    RelativeGrid.Children.Add(iRating);
                }
                else
                {
                    iRating.Tag = "IRating";
                    UpdateDriverCell(iRating, rowIndex, driver, viewedDriver, telemetryData);
                    iRating.HorizontalAlignment = HorizontalAlignment.Center;
                    iRating.Padding = new Thickness(12, 2, 12, 3.5);
                    if (iRating.Text != $"{driver.iRating / 1000}.{driver.iRating % 1000 / 100}k")
                    {
                        iRating.Text = $"{driver.iRating / 1000}.{driver.iRating % 1000 / 100}k";
                    }
                }
                Grid.SetColumn(iRating, 20);
                Grid.SetColumnSpan(iRating, 5);
                Grid.SetRow(iRating, rowIndex);
                CellIndex++;

                var delta = CellIndex < RelativeGrid.Children.Count ? (TextBlock)RelativeGrid.Children[CellIndex] : null;
                if (delta == null)
                {
                    delta = new TextBlock();
                    if (driver.PosOnTrack < 0)
                    {
                        delta.Text = "-";
                    }
                    else
                    {
                        delta.Text = driver.Delta.ToString("N1");
                    }
                    delta.Tag = "Delta";
                    UpdateDriverCell(delta, rowIndex, driver, viewedDriver, telemetryData);
                    delta.HorizontalAlignment = HorizontalAlignment.Center;
                    delta.Padding = new Thickness(25, 2, 25, 3.5);
                    RelativeGrid.Children.Add(delta);
                }
                else
                {
                    if ((driver.PosOnTrack < 0 && driver.LapsComplete < 0) || driver == viewedDriver)
                    {
                        delta.Text = "-";
                    }
                    else
                    {
                        delta.Text = driver.Delta.ToString("N1");
                    }
                    delta.Tag = "Delta";
                    UpdateDriverCell(delta, rowIndex, driver, viewedDriver, telemetryData);
                    delta.HorizontalAlignment = HorizontalAlignment.Center;
                    delta.Padding = new Thickness(25, 2, 25, 3.5);
                }
                Grid.SetColumn(delta, 25);
                Grid.SetColumnSpan(delta, 5);
                Grid.SetRow(delta, rowIndex);
                CellIndex++;
            });
            rowIndex++;
            return rowIndex;
        }

        private void UpdateDriverCell(TextBlock textBlock, int posIndex, Driver position, Driver viewedCar, TelemetryData telemetryData)
        {
            textBlock.FontSize = 16;
            textBlock.FontWeight = FontWeights.Bold;
            textBlock.Foreground = Brushes.White;
            textBlock.TextTrimming = TextTrimming.None;
            if (posIndex % 2 == 1)
            {
                textBlock.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF262525");
            }
            else
            {
                textBlock.Background = Brushes.Black;
            }

            switch (textBlock.Tag)
            {
                case "PosNumber":
                case "DriverName":
                case "IRating":
                case "Delta":
                    if (telemetryData.IsRace)
                    {
                        if (position.InPit)
                        {
                            textBlock.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#A8A8A8");
                            if (position.CarId == viewedCar.CarId)
                            {
                                textBlock.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#998100");
                            }
                            else if (position.Distance - viewedCar.Distance > telemetryData.TrackLength * 0.75)
                            {
                                textBlock.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#5c0000");
                            }
                            else if (viewedCar.Distance - telemetryData.TrackLength * 0.75 > position.Distance)
                            {
                                textBlock.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#006385");
                            }
                        }
                        else
                        {
                            textBlock.Foreground = Brushes.White;
                            if (position.CarId == viewedCar.CarId) 
                            {
                                textBlock.Foreground = Brushes.Gold;
                            }
                            else if (position.Distance - viewedCar.Distance > telemetryData.TrackLength * 0.75)
                            {
                                textBlock.Foreground = Brushes.Red;
                            }
                            else if (viewedCar.Distance - (telemetryData.TrackLength * 0.75) > position.Distance)
                            {
                                textBlock.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#00bfff");
                            }
                        }
                    }
                    else
                    {
                        textBlock.Foreground = Brushes.White;
                        if (position.InPit)
                        {
                            textBlock.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#A8A8A8");
                            if (position.CarId == viewedCar.CarId)
                            {
                                textBlock.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#998100");
                            }
                        }
                        else
                        {
                            if (position.CarId == viewedCar.CarId)
                            {
                                textBlock.Foreground = Brushes.Gold;
                            }
                        }
                    }
                    break;

            }
        }
    }
}
