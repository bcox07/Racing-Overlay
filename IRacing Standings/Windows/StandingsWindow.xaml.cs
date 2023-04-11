using iRacingSDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace IRacing_Standings
{
    /// <summary>
    /// Interaction logic for StandingsWindow.xaml
    /// </summary>
    public partial class StandingsWindow : Window
    {
        public Thread thread;
        private List<Driver> CachedAllPositions = new List<Driver>();
        private int CellIndex = 0;
        public TelemetryData _TelemetryData;
        private const int SecondsForReset = 20;
        public bool Locked = false;

        public StandingsWindow(TelemetryData telemetryData)
        {
            InitializeComponent();
            InitializeOverlay();
            UpdateTelemetryData(telemetryData);
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
            Dispatcher.Invoke(() =>
            {
                standingsGrid.Background = Brushes.Transparent;
                for (var i = 0; i < 30; i++)
                {
                    ColumnDefinition colDef = new ColumnDefinition();
                    standingsGrid.ColumnDefinitions.Add(colDef);
                }
            });
        }

        public void UpdateTelemetryData(TelemetryData telemetryData)
        {
            _TelemetryData = telemetryData;
            lock (telemetryData)
            {
                GetData(telemetryData);
            }
        }


        private void GetData(TelemetryData telemetryData)
        {
            Dispatcher.Invoke(() => { Topmost = true; });
            long myCarId = telemetryData.FeedSessionData.DriverInfo.DriverCarIdx;
            int myCarClass = (int)(telemetryData.FeedTelemetry["PlayerCarClass"] ?? 0);
            var sessionType = telemetryData.FeedSessionData.SessionInfo.Sessions[telemetryData.FeedTelemetry.Session.SessionNum].SessionType;
            var sessionTime = telemetryData.FeedSessionData.SessionInfo.Sessions[telemetryData.FeedTelemetry.Session.SessionNum]._SessionTime;
            var viewedCar = telemetryData.AllPositions.Where(p => p.CarId == telemetryData.FeedTelemetry.CamCarIdx).FirstOrDefault() ?? telemetryData.AllPositions.FirstOrDefault() ?? new Driver();
            var driverClasses = telemetryData.SortedPositions.Select(s => s.Key);

            var rowIndex = 0;
            CellIndex = 0;
            Dispatcher.Invoke((Action)delegate
            {
                if (rowIndex >= standingsGrid.RowDefinitions.Count)
                {
                    RowDefinition rowDefinition = new RowDefinition();
                    rowDefinition.Name = "SessionTitle";
                    rowDefinition.Height = new GridLength(30);
                    standingsGrid.RowDefinitions.Add(rowDefinition);
                }
                else if (standingsGrid.RowDefinitions[rowIndex].Name != "SessionTitle")
                {
                    standingsGrid.RowDefinitions[rowIndex].Name = "SessionTitle";
                    standingsGrid.RowDefinitions[rowIndex].Height = new GridLength(30);
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
                    title.Text = $"{sessionType} - {elapsedTimeString} / {sessionTimeString}";

                    title.FontSize = 20;
                    title.Height = 30;
                    title.Foreground = Brushes.White;
                    title.Background = Brushes.Black;
                    Grid.SetColumnSpan(title, 30);
                    Grid.SetRow(title, rowIndex);
                    standingsGrid.Children.Add(title);
                }
                else if (!title.Text.Contains($"{sessionType}"))
                {
                    title.Text = $"{sessionType} - {elapsedTimeString} / {sessionTimeString}";
                    title.FontSize = 20;
                    title.Height = 30;
                    title.Foreground = Brushes.White;
                    title.Background = Brushes.Black;
                    Grid.SetColumnSpan(title, 24);
                    Grid.SetRow(title, rowIndex);
                }
                else
                {
                    title.Foreground = Brushes.White;
                    title.Background = Brushes.Black;
                    title.Text = $"{sessionType} - {elapsedTimeString} / {sessionTimeString}";
                    title.Padding = new Thickness(5, 0, 0, 0);
                    Grid.SetColumnSpan(title, 24);
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
                standingsGrid.Width = 450;
                Content = standingsGrid;
            });
            CachedAllPositions = _TelemetryData.AllPositions;
            Thread.Sleep(16);
        }

        private int UpdateRow(KeyValuePair<int, List<Driver>> driverClassGroup, Driver viewedCar, int rowIndex)
        {
            var viewedClassGroup = driverClassGroup.Key == (int)viewedCar.ClassId;
            _TelemetryData.SurroundingPositions = driverClassGroup.Value.Where(p => (Math.Abs(p.ClassPosition - viewedCar.ClassPosition) < 3 && viewedClassGroup) || p.ClassPosition < 4).ToList();

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
                    if (classTitle == null)
                    {
                        classTitle = new TextBlock();
                        classTitle.FontSize = 14;
                        classTitle.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(classColor);
                        classTitle.Foreground = Brushes.Black;
                        classTitle.FontWeight = FontWeights.Bold;
                        classTitle.Text = _TelemetryData.AllDrivers.Where(d => d.CarClassID == driverClassGroup.Key).First().CarClassShortName;
                        classTitle.Padding = new Thickness(5, 0, 0, 0);                    
                        standingsGrid.Children.Add(classTitle);
                    }
                    else
                    {
                        classTitle.FontSize = 14;
                        classTitle.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(classColor);
                        classTitle.Foreground = Brushes.Black;
                        classTitle.FontWeight = FontWeights.Bold;
                        classTitle.TextAlignment = TextAlignment.Left;
                        classTitle.HorizontalAlignment = HorizontalAlignment.Stretch;
                        classTitle.Text = _TelemetryData.AllDrivers.Where(d => d.CarClassID == driverClassGroup.Key).First().CarClassShortName;
                        classTitle.Padding = new Thickness(5, 0, 0, 0);
                        standingsGrid.Children[CellIndex] = classTitle;
                    }
                }

                Grid.SetColumn(classTitle, 0);
                Grid.SetColumnSpan(classTitle, 12);
                Grid.SetRow(classTitle, rowIndex);
                CellIndex++;
            });

            var sof = (int)driverClassGroup.Value.Average(d => d.iRating);
            Dispatcher.Invoke(() =>
            {
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

                Grid.SetColumn(sofTitle, 12);
                Grid.SetColumnSpan(sofTitle, 12);
                Grid.SetRow(sofTitle, rowIndex);
                rowIndex++;
                CellIndex++;
            });

            foreach (var position in _TelemetryData.SurroundingPositions.ToList())
            {
                Dispatcher.Invoke((Action)delegate
                {
                    if (rowIndex >= standingsGrid.RowDefinitions.Count)
                    {
                        RowDefinition rowDef = new RowDefinition();
                        rowDef.Name = $"Driver{position.CarId}";
                        rowDef.Height = new GridLength(30);
                        standingsGrid.RowDefinitions.Add(rowDef);
                    }
                    else if (standingsGrid.RowDefinitions[rowIndex]?.Name != $"Driver{position.CarId}")
                    {
                        standingsGrid.RowDefinitions[rowIndex].Name = $"Driver{position.CarId}";
                        standingsGrid.RowDefinitions[rowIndex].Height = new GridLength(30);
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
                    Grid.SetColumnSpan(posNumber, 2);
                    Grid.SetColumn(posNumber, 0);
                    Grid.SetRow(posNumber, rowIndex);
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
                    Grid.SetColumn(driverName, 2);
                    Grid.SetColumnSpan(driverName, 10);
                    Grid.SetRow(driverName, rowIndex);
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
                    Grid.SetColumn(iRating, 12);
                    Grid.SetColumnSpan(iRating, 3);
                    Grid.SetRow(iRating, rowIndex);
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
                    Grid.SetColumn(deltaFromLeader, 15);
                    Grid.SetColumnSpan(deltaFromLeader, 3);
                    Grid.SetRow(deltaFromLeader, rowIndex);
                    CellIndex++;

                    var classFastestLap = _TelemetryData.SurroundingPositions.Where(s => s.FastestLap.TotalSeconds > 0).OrderBy(s => s.FastestLap).FirstOrDefault()?.FastestLap ?? TimeSpan.FromSeconds(0);
                    var fastestLap = CellIndex < standingsGrid.Children.Count ? (TextBlock)standingsGrid.Children[CellIndex] : null;
                    if (fastestLap == null)
                    {
                        fastestLap = new TextBlock();
                        UpdateFastestLapCell(fastestLap, position, classFastestLap.TotalSeconds);
                        UpdateCellGeneric(fastestLap, rowIndex, position, viewedCar);
                        standingsGrid.Children.Add(fastestLap);
                    }
                    else
                    {
                        UpdateFastestLapCell(fastestLap, position, classFastestLap.TotalSeconds);
                        UpdateCellGeneric(fastestLap, rowIndex, position, viewedCar);
                    }
                    Grid.SetColumn(fastestLap, 18);
                    Grid.SetColumnSpan(fastestLap, 6);
                    Grid.SetRow(fastestLap, rowIndex);
                    CellIndex++;

                    var lastLap = CellIndex < standingsGrid.Children.Count ? (TextBlock)standingsGrid.Children[CellIndex] : null;
                    if (lastLap == null)
                    {
                        lastLap = new TextBlock();
                        UpdateLastLapCell(lastLap, position, classFastestLap.TotalSeconds, rowIndex);
                        UpdateCellGeneric(lastLap, rowIndex, position, viewedCar);
                        standingsGrid.Children.Add(lastLap);
                    }
                    else
                    {
                        UpdateLastLapCell(lastLap, position, classFastestLap.TotalSeconds, rowIndex);
                        UpdateCellGeneric(lastLap, rowIndex, position, viewedCar);
                    }
                    Grid.SetColumn(lastLap, 24);
                    Grid.SetColumnSpan(lastLap, 6);
                    Grid.SetRow(lastLap, rowIndex);
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
            if (textBlock.Tag.ToString() != "LastLap" && textBlock.Tag.ToString() != "FastestLap")
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
                    if (position.CarId == viewedCar.CarId)
                    {
                        textBlock.Foreground = Brushes.Gold;
                    }
                    break;

            }
        }

        private void UpdatePosNumberCell(TextBlock textBlock, Driver position)
        {
            textBlock.Tag = "PosNumber";
            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.HorizontalAlignment = HorizontalAlignment.Stretch;
            textBlock.Padding = new Thickness(3.5);
            textBlock.Text = position.ClassPosition.ToString();
        }

        private void UpdateDriverNameCell(TextBlock textBlock, Driver position)
        {
            textBlock.Tag = "DriverName";
            textBlock.Padding = new Thickness(5, 3.5, 0, 3.5);
            textBlock.TextAlignment = TextAlignment.Left;
            textBlock.HorizontalAlignment = HorizontalAlignment.Stretch;
            textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
            textBlock.Text = Regex.Replace(position.Name, @"( .+ )", " ");
        }

        private void UpdateIRatingCell(TextBlock textBlock, Driver position)
        {
            textBlock.Tag = "IRating";
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.Padding = new Thickness(12, 3.5, 12, 3.5);
            textBlock.Text = $"{position.iRating / 1000}.{position.iRating % 1000 / 100}k";
        }

        private void UpdateDeltaCell(TextBlock textBlock, Driver position, Driver firstPosition)
        {
            textBlock.Tag = "Delta";
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.Padding = new Thickness(20, 3.5, 20, 3.5);
            if (_TelemetryData.IsRace)
            {
                if (position == firstPosition)
                {
                    textBlock.Text = "-";
                }
                else if (firstPosition.Distance - (position.Distance > 0 ? position.Distance : 0) > _TelemetryData.TrackLength)
                {
                    textBlock.Text = $"{(int)((firstPosition.Distance - position.Distance) / _TelemetryData.TrackLength)}L";
                }
                else
                {
                    textBlock.Text = (_TelemetryData.IsRace ? position.TimeBehindLeader - firstPosition.TimeBehindLeader : position.FastestLapDelta).ToString("N1");
                }
            }
            else
            {
                textBlock.Text = "-";
                if (position.FastestLap.TotalSeconds < 9999)
                {
                    textBlock.Text = (position.FastestLap.TotalSeconds - firstPosition.FastestLap.TotalSeconds).ToString("N1");
                }
            }
        }

        private void UpdateFastestLapCell(TextBlock textBlock, Driver position, double classFastestLap)
        {
            textBlock.Tag = "FastestLap";
            textBlock.Padding = new Thickness(10, 3.5, 0, 3.5);
            textBlock.TextAlignment = TextAlignment.Left;
            textBlock.HorizontalAlignment = HorizontalAlignment.Stretch;
            if (position.FastestLap.TotalSeconds == classFastestLap && classFastestLap < 9999)
            {
                textBlock.Foreground = Brushes.Purple;
            }
            else
            {
                textBlock.Foreground = position.FastestLap.TotalSeconds < 9999 && position.LastLap == position.FastestLap && position.SecondsSinceLastLap <= SecondsForReset ? Brushes.LimeGreen : Brushes.White;
            }
            if (position.FastestLap.TotalSeconds < 0 || position.FastestLap.TotalSeconds >= 9999)
            {
                textBlock.Text = "--:--.---";
            }
            else if (textBlock.Text != position.FastestLap.ToString(@"mm\:ss\.fff").TrimStart('m', '0'))
            {
                textBlock.Text = position.FastestLap.ToString(@"mm\:ss\.fff").TrimStart('m', '0');
            }
        }

        private void UpdateLastLapCell(TextBlock textBlock, Driver position, double classFastestLap, int rowIndex)
        {
            textBlock.Tag = "LastLap";
            textBlock.Padding = new Thickness(5, 3.5, 5, 3.5);
            textBlock.TextAlignment = TextAlignment.Left;
            textBlock.HorizontalAlignment = HorizontalAlignment.Left;
            if (position.FastestLap.TotalSeconds == classFastestLap)
            {
                textBlock.Foreground = position.LastLap == position.FastestLap && position.SecondsSinceLastLap <= SecondsForReset ? Brushes.Purple : Brushes.White;
            }
            else
            {
                textBlock.Foreground = position.LastLap == position.FastestLap && position.SecondsSinceLastLap <= SecondsForReset ? Brushes.LimeGreen : Brushes.White;
            }
            if (position.LastLap.TotalSeconds < 0 || position.LastLap.TotalSeconds > 2000 || position.SecondsSinceLastLap > SecondsForReset)
            {
                textBlock.Text = "--:--.---";
                textBlock.Foreground = textBlock.Background = Brushes.Transparent;
            }
            else if (textBlock.Text != position.LastLap.ToString(@"mm\:ss\.fff").TrimStart('m', '0'))
            {
                textBlock.Foreground = Brushes.White;
                if (rowIndex % 2 == 1)
                {
                    textBlock.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF262525");
                }
                else
                {
                    textBlock.Background = Brushes.Black;
                }
                textBlock.Text = position.LastLap.ToString(@"mm\:ss\.fff").TrimStart('m', '0');
            }
        }
    }
}
