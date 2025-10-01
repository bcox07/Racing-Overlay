using RacingOverlay.Helpers;
using iRacingSDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using static iRacingSDK.SessionData._DriverInfo;
using RacingOverlay.Models;

namespace RacingOverlay
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
        private int SafetyRatingWidth = 4;
        private int DeltaWidth = 3;
        private int FastestLapWidth = 7;
        private int LastLapWidth = 7;
        private int CarLogoWidth = 2;
        private int ColumnsWidth = 0;

        private UISize UISize = new UISize(1);

        private double? RaceStartOffset = null;
        private SessionState? PrevSessionState = null;

        public StandingsWindow(TelemetryData telemetryData, int uiSize)
        {
            InitializeComponent();
            try
            {
                InitializeOverlay(uiSize);
                UpdateTelemetryData(telemetryData, uiSize);
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

        private void InitializeOverlay(int uiSize)
        {
            UISize = new UISize(uiSize);
            var mainWindow = (MainWindow)(Application.Current.MainWindow);
            Locked = bool.Parse(mainWindow.WindowSettings.StandingsSettings["Locked"]);
            Left = double.Parse(mainWindow.WindowSettings.StandingsSettings["XPos"]);
            Top = double.Parse(mainWindow.WindowSettings.StandingsSettings["YPos"]);
            Height = 0;
            Background = Brushes.Transparent;
            ColumnsWidth = PosNumberWidth + CarNumberWidth + CarLogoWidth + DriverNameWidth + IRatingWidth + SafetyRatingWidth + DeltaWidth + FastestLapWidth + LastLapWidth;
            Dispatcher.Invoke(() =>
            {
                
                StandingsGrid.Background = Brushes.Transparent;
                for (var i = 0; i < ColumnsWidth; i++)
                {
                    ColumnDefinition colDef = new ColumnDefinition();
                    StandingsGrid.ColumnDefinitions.Add(colDef);
                }
            });
        }

        public void UpdateTelemetryData(TelemetryData telemetryData, int uiSize)
        {
            UISize = new UISize(uiSize);
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
            var sessionLapCurrent = viewedCar.LapsComplete;

            var driverClasses = telemetryData.SortedPositions.Select(s => s.Key);     

            if (sessionLapsTotal == 0)
            {
                sessionLapsTotal = (sessionLapCurrent ?? 0) +  (long)(_TelemetryData.FeedTelemetry.SessionTimeRemain / (viewedCar.FastestLap ?? 1 * 1.03)) + 1;
            }

            var rowIndex = 0;
            CellIndex = 0;
            Dispatcher.Invoke(() =>
            {
                if (rowIndex >= StandingsGrid.RowDefinitions.Count)
                {
                    RowDefinition rowDefinition = new RowDefinition();
                    rowDefinition.Name = "SessionTitle";
                    rowDefinition.Height = new GridLength(UISize.RowHeight);
                    StandingsGrid.RowDefinitions.Add(rowDefinition);
                }
                else if (StandingsGrid.RowDefinitions[rowIndex].Name != "SessionTitle")
                {
                    StandingsGrid.RowDefinitions[rowIndex].Name = "SessionTitle";
                    StandingsGrid.RowDefinitions[rowIndex].Height = new GridLength(UISize.RowHeight);
                }

                var title = new TextBlock();
                var elapsedTime = sessionTime - _TelemetryData.FeedTelemetry.SessionTimeRemain;
                var sessionState = _TelemetryData.FeedTelemetry.SessionState;

                // Set offset for the race start when watching a replay
                // For some reason in replays the race timer starts at the beginning of the formation lap
                if (sessionType.ToUpper() == "RACE" && sessionState == SessionState.Racing && PrevSessionState == SessionState.ParadeLaps && RaceStartOffset == null)
                    RaceStartOffset = elapsedTime;


                // IsReplayPlaying value is always set to true, so we shouldn't use it
                var sessionTimeString = StringHelper.GetTimeString(sessionTime, false);
                var elapsedTimeString = StringHelper.GetTimeString(elapsedTime, false);

                title.Text = $"{sessionType}";
                title.FontSize = UISize.TitleFontSize;
                title.Foreground = Brushes.White;
                title.Background = Brushes.Black;
                title.FontWeight = FontWeights.Bold;
                title.Padding = new Thickness(5);
                title.Margin = new Thickness(0, 0, -1, 0);
                title.VerticalAlignment = VerticalAlignment.Center;

                UIHelper.SetCellFormat(title, 0, 16, rowIndex);
                UIHelper.AddOrInsertChild(StandingsGrid, title, CellIndex);
                CellIndex++;

                var timeTitle = new TextBlock();
                timeTitle.Text = $"{elapsedTimeString} / {sessionTimeString}";
                timeTitle.FontSize = UISize.SubtitleFontSize;
                timeTitle.FontWeight = FontWeights.Bold;
                timeTitle.TextAlignment = TextAlignment.Right;
                timeTitle.VerticalAlignment = VerticalAlignment.Center;
                timeTitle.Padding = new Thickness(5);
                timeTitle.Margin = new Thickness(0, 0, -1, 0);
                timeTitle.Foreground = Brushes.White;
                timeTitle.Background = Brushes.Black;

                UIHelper.SetCellFormat(timeTitle, 16, 10, rowIndex);
                UIHelper.AddOrInsertChild(StandingsGrid, timeTitle, CellIndex);
                CellIndex++;

                var lapsTitle = new TextBlock();
                lapsTitle.Text = $"{sessionLapCurrent} / {sessionLapsTotal}";
                lapsTitle.FontSize = UISize.SubtitleFontSize;
                lapsTitle.FontWeight = FontWeights.Bold;
                lapsTitle.TextAlignment = TextAlignment.Right;
                lapsTitle.VerticalAlignment = VerticalAlignment.Center;
                lapsTitle.Padding = new Thickness(5);
                lapsTitle.Foreground = Brushes.White;
                lapsTitle.Background = Brushes.Black;

                UIHelper.SetCellFormat(lapsTitle, 26, 11, rowIndex);
                UIHelper.AddOrInsertChild(StandingsGrid, lapsTitle, CellIndex);
            });

            rowIndex++;
            CellIndex++;

            foreach (var driverClassGroup in telemetryData.SortedPositions)
            {
                rowIndex = UpdateRow(driverClassGroup, viewedCar, rowIndex);
            }

            Dispatcher.Invoke(() =>
            {
                while (StandingsGrid.RowDefinitions.Count > rowIndex + 1)
                {
                    StandingsGrid.RowDefinitions.RemoveAt(rowIndex + 1);
                }

                while (StandingsGrid.Children.Count > CellIndex + 1)
                {
                    StandingsGrid.Children.RemoveAt(CellIndex + 1);
                }
                                                      
                var shownDriverCount = StandingsGrid.RowDefinitions.Where(r => r.Name.StartsWith("Driver")).ToList().Count;
                var classTitleCount = StandingsGrid.RowDefinitions.Where(r => r.Name.Equals("ClassTitle")).ToList().Count;
                StandingsGrid.Width = UISize.StandingsWindowWidth;

                if (StandingsGrid.ActualHeight != (UISize.RowHeight) * (shownDriverCount + classTitleCount + 1))
                {
                    Height = (UISize.RowHeight) * (shownDriverCount + classTitleCount + 1) + 10;
                }
            });

            PrevSessionState = _TelemetryData.FeedTelemetry.SessionState;
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
                if (rowIndex >= StandingsGrid.RowDefinitions.Count)
                {
                    RowDefinition titleDef = new RowDefinition();
                    titleDef.Name = "ClassTitle";
                    titleDef.Height = new GridLength(UISize.RowHeight);   
                    StandingsGrid.RowDefinitions.Add(titleDef);
                }
                else if (StandingsGrid.RowDefinitions[rowIndex]?.Name != "ClassTitle")
                {
                    StandingsGrid.RowDefinitions[rowIndex].Name = "ClassTitle";
                    StandingsGrid.RowDefinitions[rowIndex].Height = new GridLength(UISize.RowHeight);
                }
            });

            var classColor = driverClassGroup.Value.First().ClassColor.Replace("0x", "#");
            var test = _TelemetryData.AllDrivers.ToList();
            var carClassName = test.Where(d => d != null && d.CarClassID == driverClassGroup.Key).ToList().First().CarClassShortName;
            Dispatcher.Invoke(() =>
            {
                
                var classTitle = UIHelper.CreateTextBlock(new Thickness(5, 0, 0, 0), TextAlignment.Left, fontSize: UISize.DataFontSize);
                classTitle.Text = _TelemetryData.AllDrivers.Where(d => d.CarClassID == driverClassGroup.Key).First().CarClassShortName;
                classTitle.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(classColor);
                classTitle.Foreground = Brushes.Black;
                classTitle.FontWeight = FontWeights.Bold;
                classTitle.Margin = new Thickness(0, 0, -1 , 0);
                classTitle.Padding = new Thickness(2);

                UIHelper.AddOrInsertChild(StandingsGrid, classTitle, CellIndex);
                UIHelper.SetCellFormat(classTitle, 0, ColumnsWidth - FastestLapWidth - 20, rowIndex);
                CellIndex++;

                var sof = (int)driverClassGroup.Value.Average(d => d.iRating);
                var carCount = (int)driverClassGroup.Value.Count;

                var carCountTitle = UIHelper.CreateTextBlock(new Thickness(0, 0, 5, 0), TextAlignment.Right, fontSize: UISize.DataFontSize);
                carCountTitle.Text = $"Cars: {carCount}";
                carCountTitle.FontWeight = FontWeights.Bold;
                carCountTitle.Foreground = Brushes.Black;
                carCountTitle.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(classColor);
                carCountTitle.Margin = new Thickness(0, 0, -1, 0);
                carCountTitle.Padding = new Thickness(2);

                UIHelper.AddOrInsertChild(StandingsGrid, carCountTitle, CellIndex);
                UIHelper.SetCellFormat(carCountTitle, ColumnsWidth - FastestLapWidth - 20, 10, rowIndex);
                CellIndex++;

                var sofTitle = UIHelper.CreateTextBlock(new Thickness(0, 0, 5, 0), TextAlignment.Right, fontSize: UISize.DataFontSize);
                sofTitle.Text = $"SoF: {sof}";
                sofTitle.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(classColor);
                sofTitle.Foreground = Brushes.Black;
                sofTitle.FontWeight = FontWeights.Bold;
                sofTitle.Padding = new Thickness(2);

                UIHelper.AddOrInsertChild(StandingsGrid, sofTitle, CellIndex);
                UIHelper.SetCellFormat(sofTitle, ColumnsWidth - FastestLapWidth - 10, 10, rowIndex);
                CellIndex++;
                rowIndex++;
            }); 

            foreach (var position in surroundingPositions.ToList())
            {
                var columnIndex = 0;

                Dispatcher.Invoke((Action)delegate
                {
                    if (rowIndex >= StandingsGrid.RowDefinitions.Count)
                    {
                        RowDefinition rowDef = new RowDefinition();
                        rowDef.Name = $"Driver{position.CarId}";
                        rowDef.Height = new GridLength(UISize.RowHeight);
                        StandingsGrid.RowDefinitions.Add(rowDef);
                    }
                    else if (StandingsGrid.RowDefinitions[rowIndex]?.Name != $"Driver{position.CarId}")
                    {
                        StandingsGrid.RowDefinitions[rowIndex].Name = $"Driver{position.CarId}";
                        StandingsGrid.RowDefinitions[rowIndex].Height = new GridLength(UISize.RowHeight);
                    }
                    
                    if (StandingsGrid.RowDefinitions[rowIndex]?.Height.Value != UISize.RowHeight)
                        StandingsGrid.RowDefinitions[rowIndex].Height = new GridLength(UISize.RowHeight);

                    var posNumber = UIHelper.CreateTextBlock(new Thickness(4, 4, 4, 4), fontSize: UISize.DataFontSize);
                    UpdateCell(posNumber, "PosNumber", position.ClassPosition.ToString(), rowIndex, position, viewedCar, null);
                    UIHelper.AddOrInsertChild(StandingsGrid, posNumber, CellIndex);
                    UIHelper.SetCellFormat(posNumber, columnIndex, PosNumberWidth, rowIndex);
                    columnIndex += PosNumberWidth;
                    CellIndex++;

                    Image carLogo = new Image();
                    carLogo.Tag = "CarLogo";
                    carLogo.Stretch = Stretch.Uniform;
                    carLogo.Source = new BitmapImage(new Uri($"assets/images/{CarLogo.GetLogoUri(position.CarPath)}{(rowIndex % 2 == 1 ? "-gray" : "")}.png", UriKind.Relative));
                    carLogo.Source.Freeze();

                    var carLogoBorder = new Border();
                    carLogoBorder.Background = rowIndex % 2 == 1 ? (SolidColorBrush)new BrushConverter().ConvertFrom("#FF262525") : Brushes.Black;
                    carLogoBorder.HorizontalAlignment = HorizontalAlignment.Left;
                    carLogoBorder.Child = carLogo;
                    UIHelper.AddOrInsertChild(StandingsGrid, carLogoBorder, CellIndex);
                    UIHelper.SetCellFormat(carLogoBorder, columnIndex, SafetyRatingWidth, rowIndex);
                    columnIndex += CarLogoWidth;
                    CellIndex++;

                    var carNumber = UIHelper.CreateTextBlock(new Thickness(0, 4, 3, 4), fontSize: UISize.DataFontSize);
                    UpdateCell(carNumber, "CarNumber", $"#{position.CarNumber}", rowIndex, position, viewedCar, FontWeights.SemiBold);
                    carNumber.FontStyle = FontStyles.Oblique;
                    UIHelper.AddOrInsertChild(StandingsGrid, carNumber, CellIndex);
                    UIHelper.SetCellFormat(carNumber, columnIndex, CarNumberWidth, rowIndex);
                    columnIndex += CarNumberWidth;
                    CellIndex++;

                    

                    var driverName = UIHelper.CreateTextBlock(new Thickness(7, 3.5, 0, 3.5), TextAlignment.Left, fontSize: UISize.DataFontSize);
                    driverName.TextTrimming = TextTrimming.CharacterEllipsis;
                    UpdateCell(driverName, "DriverName", Regex.Replace(position.Name, @"( .+ )", " "), rowIndex, position, viewedCar, null);
                    UIHelper.AddOrInsertChild(StandingsGrid, driverName, CellIndex);
                    UIHelper.SetCellFormat(driverName, columnIndex, DriverNameWidth, rowIndex);
                    columnIndex += DriverNameWidth;
                    CellIndex++;

                    var iRating = UIHelper.CreateTextBlock(null, fontSize: UISize.DataFontSize);
                    UpdateCell(iRating, "IRating", $"{position.iRating / 1000}.{position.iRating % 1000 / 100}k", rowIndex, position, viewedCar, null);
                    UIHelper.AddOrInsertChild(StandingsGrid, iRating, CellIndex);
                    UIHelper.SetCellFormat(iRating, columnIndex, IRatingWidth, rowIndex);
                    columnIndex += IRatingWidth;
                    CellIndex++;

                    var border = UIHelper.DesignSafetyRating(rowIndex, position, new Thickness(7, 3, 7, 3), UISize.DataFontSize);
                    border.Margin = new Thickness(-1, 0, -1, 0);
                    UIHelper.AddOrInsertChild(StandingsGrid, border, CellIndex);
                    UIHelper.SetCellFormat(border, columnIndex, SafetyRatingWidth, rowIndex);
                    columnIndex += SafetyRatingWidth;
                    CellIndex++;

                    var deltaFromLeader = UIHelper.CreateTextBlock(new Thickness(0, 4, 0, 4), TextAlignment.Right, fontSize: UISize.DataFontSize);
                    UpdateDeltaCell(deltaFromLeader, position, driverClassGroup.Value.First(), viewedCar, rowIndex);
                    UIHelper.AddOrInsertChild(StandingsGrid, deltaFromLeader, CellIndex);
                    UIHelper.SetCellFormat(deltaFromLeader, columnIndex, DeltaWidth, rowIndex);
                    columnIndex += DeltaWidth;
                    CellIndex++;

                    var fastestLap = UIHelper.CreateTextBlock(new Thickness(6, 4, 6, 4), TextAlignment.Right, fontSize: UISize.DataFontSize);
                    UpdateFastestLapCell(fastestLap, position, classFastestDriver.FastestLap, viewedCar, rowIndex);
                    UIHelper.AddOrInsertChild(StandingsGrid, fastestLap, CellIndex);
                    UIHelper.SetCellFormat(fastestLap, columnIndex, FastestLapWidth, rowIndex);
                    columnIndex += FastestLapWidth;
                    CellIndex++;

                    var lastLap = UIHelper.CreateTextBlock(new Thickness(6, 4, 6, 4), TextAlignment.Right, HorizontalAlignment.Left, fontSize: UISize.DataFontSize);
                    UpdateLastLapCell(lastLap, position, classFastestDriver.FastestLap, viewedCar, rowIndex);
                    UIHelper.AddOrInsertChild(StandingsGrid, lastLap, CellIndex);
                    UIHelper.SetCellFormat(lastLap, columnIndex, LastLapWidth, rowIndex);
                    columnIndex += LastLapWidth;
                    CellIndex++;
                });
                rowIndex++;
            }
            return rowIndex;
        }

        private void UpdateCell(TextBlock textBlock, string tag, string text, int rowIndex, Driver position, Driver viewedCar, FontWeight? fontWeight)
        {
            textBlock.Text = text;
            textBlock.Tag = tag;
            textBlock.Margin = new Thickness(-0.5, 0, -0.5, 0);

            textBlock.FontWeight = fontWeight ?? FontWeights.Bold;
            if (tag != "LastLap" && tag != "FastestLap" && tag != "SafetyRating")
                textBlock.Foreground = Brushes.White;

            if (tag != "LastLap")
                textBlock.Background = rowIndex % 2 == 1 ? (SolidColorBrush)new BrushConverter().ConvertFrom("#FF262525") : Brushes.Black;


            switch (tag)
            {
                case "DriverName":
                case "PosNumber":
                case "IRating":
                case "CarNumber":
                    if (_TelemetryData.FeedTelemetry.RadioTransmitCarIdx == position.CarId)
                        textBlock.Foreground = Brushes.LimeGreen;
                    else if (position.CarId == viewedCar.CarId)
                        textBlock.Foreground = Brushes.Gold;
                    break;
            }
        }

        private void UpdateDeltaCell(TextBlock textBlock, Driver position, Driver firstPosition, Driver viewedPosition, int rowIndex)
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

            UpdateCell(textBlock, "Delta", text, rowIndex, position, viewedPosition, null);
        }

        private void UpdateFastestLapCell(TextBlock textBlock, Driver position, double? classFastestLap, Driver viewedPosition, int rowIndex)
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

            UpdateCell(textBlock, "FastestLap", text, rowIndex, position, viewedPosition, null);
        }

        private void UpdateLastLapCell(TextBlock textBlock, Driver position, double? classFastestLap, Driver viewedPosition, int rowIndex)
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
                var timeText = StringHelper.GetTimeString(Math.Truncate((position.LastLap ?? 0) * 1000) / 1000, true);
                textBlock.Background = rowIndex % 2 == 1 ? (SolidColorBrush)new BrushConverter().ConvertFrom("#FF262525") : Brushes.Black;
                if (textBlock.Text != timeText)
                {
                    text = timeText;
                }
            }

            UpdateCell(textBlock, "LastLap", text, rowIndex, position, viewedPosition, null);
        }
    }
}
