using IRacing_Standings.Helpers;
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
using System.Windows.Media.Effects;
using static iRacingSDK.SessionData._DriverInfo;

namespace IRacing_Standings
{
    /// <summary>
    /// Interaction logic for RelativeWindow.xaml
    /// </summary>
    public partial class RelativeWindow : Window
    {
        private readonly FontWeight DefaultWeight = FontWeights.Bold;
        private int CellIndex;
        public bool Locked = false;
        TelemetryData LocalTelemetry;
        private double WindowWidth = 0;
        private int PosNumberWidth = 3;
        private int ClassColorWidth = 1;
        private int CarNumberWidth = 4;
        private int SafetyRatingWidth = 5;
        private int DriverNameWidth = 16;
        private int DeltaWidth = 6;
        private int IRatingWidth = 4;
        private double ColumnsWidth = 0;
        private int ColumnIndex = 0;
        public RelativeWindow(TelemetryData telemetryData)
        {
            LocalTelemetry = telemetryData;
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            Locked = bool.Parse(mainWindow.WindowSettings.RelativeSettings["Locked"]);
            Left = double.Parse(mainWindow.WindowSettings.RelativeSettings["XPos"]);
            Top = double.Parse(mainWindow.WindowSettings.RelativeSettings["YPos"]);
            ColumnsWidth = PosNumberWidth + ClassColorWidth + CarNumberWidth + SafetyRatingWidth + DriverNameWidth + DeltaWidth + IRatingWidth;
            WindowWidth = 350;
            Width = WindowWidth;
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
                for (var i = 0; i < ColumnsWidth; i++)
                {
                    var colDef = new ColumnDefinition();
                    colDef.Width = new GridLength(WindowWidth / ColumnsWidth);
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
                    localPosition.Delta = LocalTelemetry.GetRelativeDelta(viewedCar, localPosition, LocalTelemetry.TrackLength);
                    surroundingCars.Add(localPosition);
                }
            }
            var closestCarsAhead = surroundingCars.Where(s => s.Delta >= 0).OrderBy(s => s.Delta).Take(4).ToList();
            var closestCarsBehind = surroundingCars.Where(s => s.Delta < 0 && s.CarId != viewedCar.CarId).OrderByDescending(s => s.Delta).Take(3).ToList();
            surroundingCars = closestCarsAhead.Concat(closestCarsBehind).OrderByDescending(s => s.Delta).ToList();

            var rowIndex = 0;
            CellIndex = 0;
            
            foreach (var car in surroundingCars)
            {
                rowIndex = GenerateRow(car, viewedCar, rowIndex, LocalTelemetry);
            }
            Dispatcher.Invoke(() =>
            {
                RelativeGeometry.Rect = new Rect(0, 0, WindowWidth, rowIndex * 27);
                RelativeGrid.Width = WindowWidth;
            });
            Dispatcher.Invoke(() =>
            {

                while (RelativeGrid.RowDefinitions.Count > rowIndex + 1)
                {
                    RelativeGrid.RowDefinitions.RemoveAt(rowIndex + 1);
                }

                while (RelativeGrid.Children.Count > CellIndex + 1)
                {
                    RelativeGrid.Children.RemoveAt(CellIndex + 1);
                }
            });
            Thread.Sleep(16);
        }

        private int GenerateRow(Driver driver, Driver viewedDriver, int rowIndex, TelemetryData telemetryData)
        {
            ColumnIndex = 0;
            Dispatcher.Invoke(() =>
            {
                
                if (rowIndex >= RelativeGrid.RowDefinitions.Count)
                {
                    var rowDef = new RowDefinition();
                    rowDef.Height = new GridLength(27);
                    RelativeGrid.RowDefinitions.Add(rowDef);
                }

                var posNumber = UIHelper.CreateTextBlock(null);
                posNumber.Tag = "PosNumber";
                posNumber.Text = driver.ClassPosition.ToString();
                UpdateDriverCell(posNumber, rowIndex, driver, viewedDriver, telemetryData, null, null);
                
                UIHelper.SetCellFormat(posNumber, ColumnIndex, PosNumberWidth, rowIndex);
                UIHelper.AddOrInsertChild(RelativeGrid, posNumber, CellIndex);
                ColumnIndex += PosNumberWidth;
                CellIndex++;


                var classColor = UIHelper.CreateTextBlock(new Thickness(4, 4, 4, 4));
                classColor.Tag = "ClassColor";
                UpdateDriverCell(classColor, rowIndex, driver, viewedDriver, telemetryData, null, (SolidColorBrush)new BrushConverter().ConvertFrom(driver.ClassColor.Replace("0x", "#")));

                UIHelper.SetCellFormat(classColor, ColumnIndex, ClassColorWidth, rowIndex);
                UIHelper.AddOrInsertChild(RelativeGrid, classColor, CellIndex);
                ColumnIndex += ClassColorWidth;
                CellIndex++;

                var carNumber = UIHelper.CreateTextBlock(new Thickness(0, 4, 0, 4), fontSize: 15);
                carNumber.Tag = "CarNumber";
                UpdateDriverCell(carNumber, rowIndex, driver, viewedDriver, telemetryData, FontWeights.SemiBold, null);
                carNumber.Text = $"#{driver.CarNumber}";
                carNumber.FontStyle = FontStyles.Oblique;
                carNumber.Margin = new Thickness(0, 0, -1, 0);
                
                UIHelper.SetCellFormat(carNumber, ColumnIndex, CarNumberWidth, rowIndex);
                UIHelper.AddOrInsertChild(RelativeGrid, carNumber, CellIndex);
                ColumnIndex += CarNumberWidth;
                CellIndex++;

                var driverName = UIHelper.CreateTextBlock(new Thickness(6, 3, 6, 3), textAlignment: TextAlignment.Left);
                driverName.Tag = "DriverName";
                UpdateDriverCell(driverName, rowIndex, driver, viewedDriver, telemetryData, null, null);
                driverName.Text = Regex.Replace(driver.Name, @"( .+ )", " ");
                driverName.TextTrimming = TextTrimming.CharacterEllipsis;
                driverName.Margin = new Thickness(0, 0, -1, 0);

                UIHelper.SetCellFormat(driverName, ColumnIndex, DriverNameWidth, rowIndex);
                UIHelper.AddOrInsertChild(RelativeGrid, driverName, CellIndex);
                ColumnIndex += DriverNameWidth;
                CellIndex++;


                var border = UIHelper.DesignSafetyRating(rowIndex, driver, new Thickness(6, 3, 6, 3));
                border.VerticalAlignment = VerticalAlignment.Stretch;
                UIHelper.SetCellFormat(border, ColumnIndex, SafetyRatingWidth, rowIndex);
                UIHelper.AddOrInsertChild(RelativeGrid, border, CellIndex);

                ColumnIndex += SafetyRatingWidth;
                CellIndex++;

                var iRating = UIHelper.CreateTextBlock(null);
                iRating.Tag = "IRating";
                UpdateDriverCell(iRating, rowIndex, driver, viewedDriver, telemetryData, null, null);
                iRating.Text = $"{driver.iRating / 1000}.{driver.iRating % 1000 / 100}k";
                iRating.Margin = new Thickness(-1, 0, -1, 0);

                UIHelper.SetCellFormat(iRating, ColumnIndex, IRatingWidth, rowIndex);
                UIHelper.AddOrInsertChild(RelativeGrid, iRating, CellIndex);
                ColumnIndex += IRatingWidth;
                CellIndex++;

                var delta = UIHelper.CreateTextBlock(new Thickness(2, 3, 7, 3), textAlignment: TextAlignment.Right);
                delta.Tag = "Delta";
                UpdateDriverCell(delta, rowIndex, driver, viewedDriver, telemetryData, null, null); 
                delta.Text = ((driver.PosOnTrack < 0 && driver.LapsComplete < 0) || driver == viewedDriver) ? "  -  " : driver.Delta.ToString("N1");
                
                UIHelper.SetCellFormat(delta, ColumnIndex, DeltaWidth, rowIndex);
                UIHelper.AddOrInsertChild(RelativeGrid, delta, CellIndex);
                ColumnIndex += DeltaWidth;
                CellIndex++;
            });
            rowIndex++;
            return rowIndex;
        }

        private void SetCellFormat(TextBlock textBlock, int columnWidth, int rowIndex)
        {
            Grid.SetColumnSpan(textBlock, columnWidth);
            Grid.SetColumn(textBlock, ColumnIndex);
            Grid.SetRow(textBlock, rowIndex);
        }

        private void UpdateDriverCell(
            TextBlock textBlock, 
            int posIndex, 
            Driver position, 
            Driver viewedCar, 
            TelemetryData telemetryData, 
            FontWeight? fontWeight,
            Brush backgroundColor)
        {
            var fadedWhiteBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#A8A8A8");
            var fadedGoldBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#998100");
            var fadedRedBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#5C0000");
            var blueBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#00BFFF");
            var fadedBlueBrush = (SolidColorBrush)new BrushConverter().ConvertFrom("#006385");

            textBlock.FontWeight = fontWeight ?? FontWeights.Bold;
            textBlock.Foreground = Brushes.White;
            if (backgroundColor == null)
                textBlock.Background = posIndex % 2 == 1 ? (SolidColorBrush)new BrushConverter().ConvertFrom("#FF262525"): Brushes.Black;
            else
                textBlock.Background = backgroundColor;

            switch (textBlock.Tag)
            {
                case "PosNumber":
                case "CarNumber":
                case "DriverName":
                case "IRating":
                case "Delta":
                    if (telemetryData.IsRace)
                    {
                        textBlock.Foreground = position.InPit ? fadedWhiteBrush : Brushes.White;
                        if (position.CarId == viewedCar.CarId)
                            textBlock.Foreground = position.InPit ? fadedGoldBrush : Brushes.Gold;
                        else if (position.Distance > viewedCar.Distance && position.Delta < 0 || position.Distance - viewedCar.Distance > telemetryData.TrackLength)
                            textBlock.Foreground = position.InPit ? fadedRedBrush : Brushes.Red;
                        else if (position.Distance < viewedCar.Distance && position.Delta > 0 || viewedCar.Distance - position.Distance > telemetryData.TrackLength)
                            textBlock.Foreground = position.InPit ? fadedBlueBrush : blueBrush;
                    }
                    else
                    {
                        textBlock.Foreground = position.InPit ? fadedWhiteBrush : Brushes.White;

                        if (position.CarId == viewedCar.CarId)
                            textBlock.Foreground = position.InPit ? fadedGoldBrush : Brushes.Gold;
                    }
                    break;
            }
        }
    }
}
