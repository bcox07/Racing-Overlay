using iRacingSDK;
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
    /// Interaction logic for StandingsWindow.xaml
    /// </summary>
    public partial class StandingsWindow : Window
    {
        public Thread thread;
        private int CellIndex = 0;
        public TelemetryData _TelemetryData;
        private const double SecondsForReset = 30;
        public bool Locked = false;
        private int PosNumberWidth = 2;
        private int CarNumberWidth = 3;
        private int DriverNameWidth = 12;
        private int IRatingWidth = 4;
        private int SafetyRatingWidth = 5;
        private int DeltaWidth = 3;
        private int FastestLapWidth = 7;
        private int LastLapWidth = 7;
        private int ColumnsWidth = 0;

        public StandingsWindow(TelemetryData telemetryData)
        {
            InitializeComponent();
            InitializeOverlay();
            try
            {
                UpdateTelemetryData(telemetryData);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
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

        private void InitializeOverlay()
        {
            var mainWindow = (MainWindow)(Application.Current.MainWindow);
            Locked = bool.Parse(mainWindow.WindowSettings.StandingsSettings["Locked"]);
            Left = double.Parse(mainWindow.WindowSettings.StandingsSettings["XPos"]);
            Top = double.Parse(mainWindow.WindowSettings.StandingsSettings["YPos"]);
            Height = 0;
            Background = Brushes.Transparent;
            ColumnsWidth = PosNumberWidth + CarNumberWidth + DriverNameWidth + IRatingWidth + SafetyRatingWidth + DeltaWidth + FastestLapWidth + LastLapWidth;
            Dispatcher.Invoke(() =>
            {
                standingsGrid.Background = Brushes.Transparent;
                for (var i = 0; i < ColumnsWidth; i++)
                {
                    ColumnDefinition colDef = new ColumnDefinition();
                    standingsGrid.ColumnDefinitions.Add(colDef);
                }
            });
        }

        public void UpdateTelemetryData(TelemetryData telemetryData)
        {
            _TelemetryData = telemetryData;
            if (telemetryData.IsReady)
            {
                GetData(telemetryData);
            }
        }


        private void GetData(TelemetryData telemetryData)
        {
            Dispatcher.Invoke(() => 
            { 
                Topmost = false;
                Topmost = true; 
            });
            var sessionTimeString1 = telemetryData.FeedSessionData.SessionInfo.Sessions[telemetryData.FeedTelemetry.Session.SessionNum].SessionTime;
            var sessionType = telemetryData.FeedSessionData.SessionInfo.Sessions[telemetryData.FeedTelemetry.Session.SessionNum].SessionType;
            var sessionTime = telemetryData.FeedSessionData.SessionInfo.Sessions[telemetryData.FeedTelemetry.Session.SessionNum]._SessionTime;
            var viewedCar = telemetryData.AllPositions.Where(p => p.CarId == telemetryData.FeedTelemetry.CamCarIdx).FirstOrDefault() ?? telemetryData.AllPositions.FirstOrDefault() ?? new Driver();
            var sessionLapsTotal = telemetryData.FeedSessionData.SessionInfo.Sessions[telemetryData.FeedTelemetry.Session.SessionNum].SessionLaps == "unlimited" ? 0 : long.Parse(telemetryData.FeedSessionData.SessionInfo.Sessions[telemetryData.FeedTelemetry.Session.SessionNum].SessionLaps);
            var viewedCarPosition = telemetryData.AllResultsPositions?.FirstOrDefault(r => r.CarIdx == viewedCar.CarId) ?? telemetryData.AllResultsPositions.FirstOrDefault() ?? new SessionData._SessionInfo._Sessions._ResultsPositions();
            var sessionLapCurrent = viewedCarPosition.LapsComplete + 1;
            var driverClasses = telemetryData.SortedPositions.Select(s => s.Key);

            var rowIndex = 0;
            CellIndex = 0;
            Dispatcher.Invoke(() =>
            {
                if (rowIndex >= standingsGrid.RowDefinitions.Count)
                {
                    RowDefinition rowDefinition = new RowDefinition();
                    rowDefinition.Name = "SessionTitle";
                    rowDefinition.Height = new GridLength(25);
                    standingsGrid.RowDefinitions.Add(rowDefinition);
                }
                else if (standingsGrid.RowDefinitions[rowIndex].Name != "SessionTitle")
                {
                    standingsGrid.RowDefinitions[rowIndex].Name = "SessionTitle";
                    standingsGrid.RowDefinitions[rowIndex].Height = new GridLength(25);
                }

                TextBlock title = CellIndex < standingsGrid.Children.Count ? (TextBlock)standingsGrid.Children[CellIndex] : new TextBlock();
                var elapsedTime = sessionTime - _TelemetryData.FeedTelemetry.SessionTimeRemain;
                
                var test = TimeSpan.FromSeconds(sessionTime).ToString(@"hh\:mm\:ss").TrimStart('m', '0').TrimStart('h', '0');
                var sessionTimeString = "";
                var elapsedTimeString = "";

                if (sessionTime >= 3600)
                    sessionTimeString = TimeSpan.FromSeconds(sessionTime).ToString(@"hh\:mm\:ss").TrimStart('m', '0').TrimStart('h', '0');
                else
                    sessionTimeString = TimeSpan.FromSeconds(sessionTime).ToString(@"mm\:ss").TrimStart('m', '0');

                if (elapsedTime >= 3600)
                    elapsedTimeString = TimeSpan.FromSeconds(elapsedTime).ToString(@"hh\:mm\:ss").TrimStart('m', '0').TrimStart('h', '0');
                else
                    elapsedTimeString = TimeSpan.FromSeconds(elapsedTime).ToString(@"mm\:ss").TrimStart('m', '0');

                if (title.Text == "")
                {
                    title = new TextBlock();
                    if (sessionTimeString1 == "unlimited")
                    {
                        title.Text = $"{sessionType} - {sessionLapCurrent} / {sessionLapsTotal}";
                    }
                    else
                    {
                        title.Text = $"{sessionType} - {elapsedTimeString} / {sessionTimeString}";
                    }

                    title.FontSize = 20;
                    title.Height = 30;
                    title.Foreground = Brushes.White;
                    title.Background = Brushes.Black;
                    Grid.SetColumnSpan(title, ColumnsWidth - FastestLapWidth);
                    Grid.SetRow(title, rowIndex);
                    standingsGrid.Children.Add(title);
                }
                else if (!title.Text.Contains($"{sessionType}"))
                {
                    if (sessionTimeString1 == "unlimited" || sessionLapsTotal > 0)
                    {
                        title.Text = $"{sessionType} - {sessionLapCurrent} / {sessionLapsTotal}";
                    }
                    else
                    {
                        title.Text = $"{sessionType} - {elapsedTimeString} / {sessionTimeString}";
                    }
                    title.FontSize = 20;
                    title.Height = 30;
                    title.Foreground = Brushes.White;
                    title.Background = Brushes.Black;
                    Grid.SetColumnSpan(title, ColumnsWidth - FastestLapWidth);
                    Grid.SetRow(title, rowIndex);
                }
                else
                {
                    if (sessionTimeString1 == "unlimited" || sessionLapsTotal > 0)
                    {
                        title.Text = $"{sessionType} - {sessionLapCurrent} / {sessionLapsTotal}";
                    }
                    else
                    {
                        title.Text = $"{sessionType} - {elapsedTimeString} / {sessionTimeString}";
                    }
                    title.Foreground = Brushes.White;
                    title.Background = Brushes.Black;
                    title.Padding = new Thickness(5, 0, 0, 0);
                    Grid.SetColumnSpan(title, ColumnsWidth - FastestLapWidth);
                    Grid.SetRow(title, rowIndex);
                }
            });
           
            rowIndex++;
            CellIndex++;
            foreach (var driverClassGroup in telemetryData.SortedPositions)
            {
                rowIndex = UpdateRow(driverClassGroup, viewedCar, rowIndex);
            }

            Dispatcher.Invoke(() =>
            {
                for (var i = rowIndex; i < standingsGrid.RowDefinitions.Count; i++)
                {
                    standingsGrid.RowDefinitions.RemoveAt(i);
                }
                for (var i = CellIndex; i < standingsGrid.Children.Count; i++)
                {
                    standingsGrid.Children.RemoveAt(i);
                }
                if (standingsGrid.ActualHeight != ((rowIndex - _TelemetryData.SortedPositions.Count - 1)  * 30) + (_TelemetryData.SortedPositions.Count * 20))
                {
                    Height = ((rowIndex - _TelemetryData.SortedPositions.Count)  * 30) + (_TelemetryData.SortedPositions.Count * 20);
                }
                standingsGrid.Width = 550;
                Content = standingsGrid;
            });
            Thread.Sleep(16);
        }

        private int UpdateRow(KeyValuePair<int, List<Driver>> driverClassGroup, Driver viewedCar, int rowIndex)
        {
            var viewedClassGroup = driverClassGroup.Key == viewedCar.ClassId;
            var surroundingPositions = driverClassGroup.Value.Where(
                p => p.ClassPosition != null 
                && (Math.Abs( p.ClassPosition.Value - viewedCar.ClassPosition.Value) < 4 
                && viewedClassGroup) || p.ClassPosition < 4).ToList();

            if (viewedCar.ClassPosition < 4 && viewedClassGroup)
            {
                surroundingPositions = driverClassGroup.Value.Where(
                p => p.ClassPosition < 7 && viewedClassGroup).ToList();
            }

            Driver classFastestDriver = null;
            if (driverClassGroup.Value.Where(p => p.ClassPosition != null && viewedClassGroup && p.FastestLap != null).Count() > 0)
            {
                classFastestDriver = driverClassGroup.Value.Where(p => p.ClassPosition != null && viewedClassGroup && p.FastestLap != null).OrderBy(p => p.FastestLap).First();
            }
            else
            {
                classFastestDriver = driverClassGroup.Value.Where(p => p.ClassPosition != null).OrderBy(p => p.FastestLap).First();
            }

            Dispatcher.Invoke(() =>
            {
                if (rowIndex >= standingsGrid.RowDefinitions.Count)
                {
                    RowDefinition titleDef = new RowDefinition();
                    titleDef.Name = "ClassTitle";
                    titleDef.Height = new GridLength(20);
                    standingsGrid.RowDefinitions.Add(titleDef);
                }
                else if (standingsGrid.RowDefinitions[rowIndex]?.Name != "ClassTitle")
                {
                    standingsGrid.RowDefinitions[rowIndex].Name = "ClassTitle";
                    standingsGrid.RowDefinitions[rowIndex].Height = new GridLength(20);
                }
            });

            var classColor = driverClassGroup.Value.First().ClassColor.Replace("0x", "#");
            var test = _TelemetryData.AllDrivers.ToList();
            var carClassName = test.Where(d => d != null && d.CarClassID == driverClassGroup.Key).ToList().First().CarClassShortName;
            Dispatcher.Invoke(() =>
            {
                
                var classTitle = standingsGrid.Children.Count > CellIndex ? (TextBlock)standingsGrid.Children[CellIndex] : null;
                if (classTitle == null || classTitle.Text != carClassName)
                {
                    var newCell = classTitle == null;
                    classTitle = classTitle != null ? classTitle : new TextBlock();
                    classTitle.FontSize = 14;
                    classTitle.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(classColor);
                    classTitle.Foreground = Brushes.Black;
                    classTitle.FontWeight = FontWeights.Bold;
                    classTitle.TextAlignment = TextAlignment.Left;
                    classTitle.HorizontalAlignment = HorizontalAlignment.Stretch;
                    classTitle.Text = _TelemetryData.AllDrivers.Where(d => d.CarClassID == driverClassGroup.Key).First().CarClassShortName;
                    classTitle.Padding = new Thickness(5, 0, 0, 0);

                    if (newCell)
                    {
                        standingsGrid.Children.Add(classTitle);
                    }
                    else
                    {
                        standingsGrid.Children[CellIndex] = classTitle;
                    }
                }

                Grid.SetColumn(classTitle, 0);
                Grid.SetColumnSpan(classTitle, ColumnsWidth - FastestLapWidth);
                Grid.SetRow(classTitle, rowIndex);
                CellIndex++;
            });

            var sof = (int)driverClassGroup.Value.Average(d => d.iRating);
            var carCount = (int)driverClassGroup.Value.Count;

            Dispatcher.Invoke(() =>
            {
                var carCountTitle = standingsGrid.Children.Count > CellIndex ? (TextBlock)standingsGrid.Children[CellIndex] : null;
                if (carCountTitle == null || carCountTitle.Text != $"Cars: {carCount}")
                {
                    if (carCountTitle == null)
                    {
                        carCountTitle = new TextBlock();
                        carCountTitle.FontSize = 14;
                        carCountTitle.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(classColor);
                        carCountTitle.Foreground = Brushes.Black;
                        carCountTitle.FontWeight = FontWeights.Bold;
                        carCountTitle.TextAlignment = TextAlignment.Right;
                        carCountTitle.HorizontalAlignment = HorizontalAlignment.Stretch;
                        carCountTitle.Text = $"Cars: {carCount}";
                        carCountTitle.Padding = new Thickness(0, 0, 5, 0);
                        standingsGrid.Children.Add(carCountTitle);
                    }
                    else
                    {
                        carCountTitle.FontSize = 14;
                        carCountTitle.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(classColor);
                        carCountTitle.Foreground = Brushes.Black;
                        carCountTitle.FontWeight = FontWeights.Bold;
                        carCountTitle.TextAlignment = TextAlignment.Right;
                        carCountTitle.HorizontalAlignment = HorizontalAlignment.Stretch;
                        carCountTitle.Text = $"Cars: {carCount}";
                        carCountTitle.Padding = new Thickness(0, 0, 5, 0);
                        standingsGrid.Children[CellIndex] = carCountTitle;
                    }
                }
                else
                {
                    carCountTitle.FontSize = 14;
                    carCountTitle.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(classColor);
                    carCountTitle.Foreground = Brushes.Black;
                    carCountTitle.FontWeight = FontWeights.Bold;
                    carCountTitle.TextAlignment = TextAlignment.Right;
                    carCountTitle.HorizontalAlignment = HorizontalAlignment.Stretch;
                    carCountTitle.Padding = new Thickness(0, 0, 5, 0);
                    carCountTitle.Text = $"Cars: {carCount}";
                }

                Grid.SetColumn(carCountTitle, ColumnsWidth - FastestLapWidth - 20);
                Grid.SetColumnSpan(carCountTitle, 10);
                Grid.SetRow(carCountTitle, rowIndex);
                CellIndex++;

                var sofTitle = standingsGrid.Children.Count > CellIndex ? (TextBlock)standingsGrid.Children[CellIndex] : null;
                if (sofTitle == null || sofTitle.Text != $"SoF - {sof}")
                {
                    if (sofTitle == null)
                    {
                        sofTitle = new TextBlock();
                        sofTitle.FontSize = 14;
                        sofTitle.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(classColor);
                        sofTitle.Foreground = Brushes.Black;
                        sofTitle.FontWeight = FontWeights.Bold;
                        sofTitle.TextAlignment = TextAlignment.Right;
                        sofTitle.HorizontalAlignment = HorizontalAlignment.Stretch;
                        sofTitle.Text = $"SoF: {sof}";
                        sofTitle.Padding = new Thickness(0, 0, 5, 0);
                        standingsGrid.Children.Add(sofTitle);
                    }
                    else
                    {
                        sofTitle.FontSize = 14;
                        sofTitle.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(classColor);
                        sofTitle.Foreground = Brushes.Black;
                        sofTitle.FontWeight = FontWeights.Bold;
                        sofTitle.TextAlignment = TextAlignment.Right;
                        sofTitle.HorizontalAlignment = HorizontalAlignment.Stretch;
                        sofTitle.Text = $"SoF: {sof}";
                        sofTitle.Padding = new Thickness(0, 0, 5, 0);
                        standingsGrid.Children[CellIndex] = sofTitle;
                    }
                }
                else
                {
                    sofTitle.FontSize = 14;
                    sofTitle.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(classColor);
                    sofTitle.Foreground = Brushes.Black;
                    sofTitle.FontWeight = FontWeights.Bold;
                    sofTitle.TextAlignment = TextAlignment.Right;
                    sofTitle.HorizontalAlignment = HorizontalAlignment.Stretch;
                    sofTitle.Padding = new Thickness(0, 0, 5, 0);
                    sofTitle.Text = $"SoF: {sof}";
                }

                Grid.SetColumn(sofTitle, ColumnsWidth - FastestLapWidth - 10);
                Grid.SetColumnSpan(sofTitle, 10);
                Grid.SetRow(sofTitle, rowIndex);
                CellIndex++;
                rowIndex++;            
            });

            

            foreach (var position in surroundingPositions.ToList())
            {
                var columnIndex = 0;

                Dispatcher.Invoke((Action)delegate
                {
                    if (rowIndex >= standingsGrid.RowDefinitions.Count)
                    {
                        RowDefinition rowDef = new RowDefinition();
                        rowDef.Name = $"Driver{position.CarId}";
                        rowDef.Height = new GridLength(25);
                        standingsGrid.RowDefinitions.Add(rowDef);
                    }
                    else if (standingsGrid.RowDefinitions[rowIndex]?.Name != $"Driver{position.CarId}")
                    {
                        standingsGrid.RowDefinitions[rowIndex].Name = $"Driver{position.CarId}";
                        standingsGrid.RowDefinitions[rowIndex].Height = new GridLength(25);
                    }

                    var posNumber = CellIndex < standingsGrid.Children.Count ? (TextBlock)standingsGrid.Children[CellIndex] : null;
                    
                    if (posNumber == null)
                    {
                        posNumber = new TextBlock();
                        UpdatePosNumberCell(posNumber, position);
                        UpdateCellGeneric(posNumber, rowIndex, position, viewedCar);
                        standingsGrid.Children.Add(posNumber);
                    }
                    else
                    {
                        UpdatePosNumberCell(posNumber, position);
                        UpdateCellGeneric(posNumber, rowIndex, position, viewedCar);
                    }
                    Grid.SetColumnSpan(posNumber, PosNumberWidth);
                    Grid.SetColumn(posNumber, columnIndex);
                    Grid.SetRow(posNumber, rowIndex);
                    columnIndex += PosNumberWidth;
                    CellIndex++;

                    var carNumber = CellIndex < standingsGrid.Children.Count ? (TextBlock)standingsGrid.Children[CellIndex] : null;
                    if (carNumber == null)
                    {
                        carNumber = new TextBlock();
                        UpdateCarNumberCell(carNumber, position);
                        UpdateCellGeneric(carNumber, rowIndex, position, viewedCar);
                        standingsGrid.Children.Add(carNumber);
                    }
                    else
                    {
                        UpdateCarNumberCell(carNumber, position);
                        UpdateCellGeneric(carNumber, rowIndex, position, viewedCar);
                    }
                    Grid.SetColumnSpan(carNumber, CarNumberWidth);
                    Grid.SetColumn(carNumber, columnIndex);
                    Grid.SetRow(carNumber, rowIndex);
                    columnIndex += CarNumberWidth;
                    CellIndex++;

                    var driverName = CellIndex < standingsGrid.Children.Count ? (TextBlock)standingsGrid.Children[CellIndex] : null;
                    if (driverName == null)
                    {
                        driverName = new TextBlock();
                        driverName.Tag = "DriverName";
                        UpdateCellGeneric(driverName, rowIndex, position, viewedCar);
                        UpdateDriverNameCell(driverName, position);
                        standingsGrid.Children.Add(driverName);
                    }
                    else
                    {
                        driverName.Tag = "DriverName";
                        UpdateCellGeneric(driverName, rowIndex, position, viewedCar);
                        UpdateDriverNameCell(driverName, position);
                    }
                    Grid.SetColumn(driverName, columnIndex);
                    Grid.SetColumnSpan(driverName, DriverNameWidth);
                    Grid.SetRow(driverName, rowIndex);
                    columnIndex += DriverNameWidth;
                    CellIndex++;

                    var iRating = CellIndex < standingsGrid.Children.Count ? (TextBlock)standingsGrid.Children[CellIndex] : null;
                    if (iRating == null)
                    {
                        iRating = new TextBlock();
                        UpdateIRatingCell(iRating, position);
                        UpdateCellGeneric(iRating, rowIndex, position, viewedCar);
                        standingsGrid.Children.Add(iRating);
                    }
                    else
                    {
                        UpdateIRatingCell(iRating, position);
                        UpdateCellGeneric(iRating, rowIndex, position, viewedCar);
                    }
                    Grid.SetColumn(iRating, columnIndex);
                    Grid.SetColumnSpan(iRating, IRatingWidth);
                    Grid.SetRow(iRating, rowIndex);
                    columnIndex += IRatingWidth;
                    CellIndex++;

                    var safetyRating = CellIndex < standingsGrid.Children.Count ? (TextBlock)standingsGrid.Children[CellIndex] : null;
                    if (safetyRating == null)
                    {
                        safetyRating = new TextBlock();
                        UpdateSafetyRatingCell(safetyRating, position);
                        UpdateCellGeneric(safetyRating, rowIndex, position, viewedCar);
                        standingsGrid.Children.Add(safetyRating);
                    }
                    else
                    {
                        UpdateSafetyRatingCell(safetyRating, position);
                        UpdateCellGeneric(safetyRating, rowIndex, position, viewedCar);
                    }
                    SetCellLocation(safetyRating, columnIndex, SafetyRatingWidth, rowIndex);
                    columnIndex += SafetyRatingWidth;
                    CellIndex++;

                    var deltaFromLeader = CellIndex < standingsGrid.Children.Count ? (TextBlock)standingsGrid.Children[CellIndex] : null;
                    if (deltaFromLeader == null)
                    {
                        deltaFromLeader = new TextBlock();
                        UpdateDeltaCell(deltaFromLeader, position, driverClassGroup.Value.First());
                        UpdateCellGeneric(deltaFromLeader, rowIndex, position, viewedCar);
                        standingsGrid.Children.Add(deltaFromLeader);
                    }
                    else
                    {
                        UpdateDeltaCell(deltaFromLeader, position, driverClassGroup.Value.First());
                        UpdateCellGeneric(deltaFromLeader, rowIndex, position, viewedCar);
                    }
                    SetCellLocation(deltaFromLeader, columnIndex, DeltaWidth, rowIndex);
                    columnIndex += DeltaWidth;
                    CellIndex++;

                    var classFastestLap = surroundingPositions.Where(s => s.FastestLap != null).OrderBy(s => s.FastestLap).FirstOrDefault()?.FastestLap;
                    var fastestLap = CellIndex < standingsGrid.Children.Count ? (TextBlock)standingsGrid.Children[CellIndex] : null;
                    if (fastestLap == null)
                    {
                        fastestLap = new TextBlock();
                        UpdateFastestLapCell(fastestLap, position, classFastestDriver.FastestLap);
                        UpdateCellGeneric(fastestLap, rowIndex, position, viewedCar);
                        standingsGrid.Children.Add(fastestLap);
                    }
                    else
                    {
                        UpdateFastestLapCell(fastestLap, position, classFastestDriver.FastestLap);
                        UpdateCellGeneric(fastestLap, rowIndex, position, viewedCar);
                    }
                    SetCellLocation(fastestLap, columnIndex, FastestLapWidth, rowIndex);
                    columnIndex += FastestLapWidth;
                    CellIndex++;

                    var lastLap = CellIndex < standingsGrid.Children.Count ? (TextBlock)standingsGrid.Children[CellIndex] : null;
                    if (lastLap == null)
                    {
                        lastLap = new TextBlock();
                        UpdateLastLapCell(lastLap, position, classFastestLap, rowIndex);
                        UpdateCellGeneric(lastLap, rowIndex, position, viewedCar);
                        standingsGrid.Children.Add(lastLap);
                    }
                    else
                    {
                        UpdateLastLapCell(lastLap, position, classFastestLap, rowIndex);
                        UpdateCellGeneric(lastLap, rowIndex, position, viewedCar);
                    }
                    SetCellLocation(lastLap, columnIndex, LastLapWidth, rowIndex);
                    columnIndex += LastLapWidth;
                    CellIndex++;
                });
                rowIndex++;
            }
            return rowIndex;
        }

        private void UpdateCellGeneric(TextBlock textBlock, int rowIndex, Driver position, Driver viewedCar)
        {
            textBlock.FontSize = 16;
            textBlock.FontWeight = FontWeights.Bold;
            textBlock.TextTrimming = TextTrimming.None;
            if (textBlock.Tag.ToString() != "LastLap" && textBlock.Tag.ToString() != "FastestLap" && textBlock.Tag.ToString() != "SafetyRating")
            {
                textBlock.Foreground = Brushes.White;
            }

            if (textBlock.Tag.ToString() != "LastLap")
            {
                if (rowIndex % 2 == 1)
                {
                    textBlock.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF262525");
                }
                else
                {
                    textBlock.Background = Brushes.Black;
                }
            }
            

            switch (textBlock.Tag.ToString())
            {
                case "PosNumber":
                case "DriverName":
                case "IRating":
                case "CarNumber":
                    if (position.CarId == viewedCar.CarId)
                    {
                        textBlock.Foreground = Brushes.Gold;
                    }
                    break;

            }
        }

        private void SetCellLocation(TextBlock textBlock, int columnIndex, int columnWidth, int rowIndex)
        {
            Grid.SetColumn(textBlock, columnIndex);
            Grid.SetColumnSpan(textBlock, columnWidth);
            Grid.SetRow(textBlock, rowIndex);
        }

        private void UpdateCell(TextBlock textBlock, string tag, string text, TextAlignment textAlignment = TextAlignment.Center, HorizontalAlignment hAlignment = HorizontalAlignment.Stretch)
        {
            textBlock.Text = text;
            textBlock.Tag = tag;
            textBlock.TextAlignment = textAlignment;
            textBlock.HorizontalAlignment = hAlignment;
            textBlock.Margin = new Thickness(-0.5, 0, -0.5, 0);
            textBlock.Padding = new Thickness(0, 1, 0, 0);
        }

        private void UpdatePosNumberCell(TextBlock textBlock, Driver position)
        {
            UpdateCell(textBlock, "PosNumber", position.ClassPosition.ToString());
        }

        private void UpdateCarNumberCell(TextBlock textBlock, Driver position)
        {
            UpdateCell(textBlock, "CarNumber", $"#{position.CarNumber}");
            textBlock.Padding = new Thickness(1);
        }

        private void UpdateDriverNameCell(TextBlock textBlock, Driver position)
        {
            UpdateCell(textBlock, "DriverName", Regex.Replace(position.Name, @"( .+ )", " "), TextAlignment.Left);
            textBlock.Padding = new Thickness(5, 1, 0, 3.5);
            textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
        }

        private void UpdateIRatingCell(TextBlock textBlock, Driver position)
        {
            UpdateCell(textBlock, "IRating", $"{position.iRating / 1000}.{position.iRating % 1000 / 100}k");
        }

        private void UpdateSafetyRatingCell(TextBlock textBlock, Driver position)
        {
            UpdateCell(textBlock, "SafetyRating", position.SafetyRating.Item1);
            
            textBlock.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom(position.SafetyRating.Item2.Replace("0x", "#"));
        }

        private void UpdateDeltaCell(TextBlock textBlock, Driver position, Driver firstPosition)
        {
            var text = "-";
            if (_TelemetryData.IsRace)
            {
                if (firstPosition.Distance - (position.Distance > 0 ? position.Distance : 0) > _TelemetryData.TrackLength)
                {
                    text = $"{(int)((firstPosition.Distance - position.Distance) / _TelemetryData.TrackLength)}L";
                }
                else if (position != firstPosition)
                {
                    text = (position.TimeBehindLeader - firstPosition.TimeBehindLeader).ToString("N1");
                }
            }
            else
            {
                if (position.FastestLap != null && firstPosition.FastestLap != null &&  position.FastestLap.Value != firstPosition.FastestLap.Value)
                {
                    text = (position.FastestLap.Value - firstPosition.FastestLap.Value).ToString("N1");
                }
            }

            UpdateCell(textBlock, "Delta", text);
        }

        private void UpdateFastestLapCell(TextBlock textBlock, Driver position, double? classFastestLap)
        {
            if (position.FastestLap != null && position.FastestLap.Value == (classFastestLap ?? 0))
            {
                textBlock.Foreground = Brushes.Purple;
            }
            else
            {
                textBlock.Foreground = position.FastestLap != null && position.LastLap != null && 
                    position.LastLap.Value == position.FastestLap.Value && 
                    DateTime.UtcNow.Subtract(position.LapChangeTime).TotalSeconds <= SecondsForReset ? Brushes.LimeGreen : Brushes.White;
            }

            var text = "--:--.---";
            if (position.FastestLap != null)
            {
                var aboveMinuteText = TimeSpan.FromSeconds(Math.Truncate(position.FastestLap.Value * 1000) / 1000).ToString(@"mm\:ss\.fff").TrimStart('m', '0');
                var subMinuteText = TimeSpan.FromSeconds(Math.Truncate(position.FastestLap.Value * 1000) / 1000).ToString(@"ss\.fff").TrimStart('m', '0');

                if (textBlock.Text != aboveMinuteText || textBlock.Text != subMinuteText)
                {
                    text = position.FastestLap.Value > 60 ? aboveMinuteText : subMinuteText;
                }
            }

            UpdateCell(textBlock, "FastestLap", text, TextAlignment.Right);
            textBlock.Padding = new Thickness(0, 1, 10, 0);
        }

        private void UpdateLastLapCell(TextBlock textBlock, Driver position, double? classFastestLap, int rowIndex)
        {
            var text = textBlock.Text ?? string.Empty;

            if ((position.FastestLap ?? 9999.9) == classFastestLap)
            {
                textBlock.Foreground = position.LastLap == position.FastestLap && DateTime.UtcNow.Subtract(position.LapChangeTime).TotalMilliseconds <= SecondsForReset * 1000 ? Brushes.Purple : Brushes.White;
            }
            else
            {
                textBlock.Foreground = position.LastLap == position.FastestLap && DateTime.UtcNow.Subtract(position.LapChangeTime).TotalMilliseconds <= SecondsForReset * 1000 ? Brushes.LimeGreen : Brushes.White;
            }

            if (position.LastLap == null || DateTime.UtcNow.Subtract(position.LapChangeTime).TotalMilliseconds > SecondsForReset * 1000)
            {
                textBlock.Foreground = Brushes.Transparent;
                textBlock.Background = Brushes.Transparent;
            }
            else
            {
                var aboveMinuteText = TimeSpan.FromSeconds(Math.Truncate((position.LastLap ?? 0) * 1000) / 1000).ToString(@"mm\:ss\.fff").TrimStart('m', '0');
                var subMinuteText = TimeSpan.FromSeconds(Math.Truncate((position.LastLap ?? 0) * 1000) / 1000).ToString(@"ss\.fff").TrimStart('m', '0');
                textBlock.Background = rowIndex % 2 == 1 ? (SolidColorBrush)new BrushConverter().ConvertFrom("#FF262525") : Brushes.Black;
                if (textBlock.Text != aboveMinuteText && textBlock.Text != subMinuteText)
                {
                    text = position.LastLap.Value > 60 ? aboveMinuteText : subMinuteText;
                }
            }

            UpdateCell(textBlock, "LastLap", text, TextAlignment.Right);
            textBlock.Padding = new Thickness(5, 1, 5, 3.5);
        }
    }
}
