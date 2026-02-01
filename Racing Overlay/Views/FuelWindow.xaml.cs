using iRacingSDK;
using NLog;
using RacingOverlay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace RacingOverlay
{
    /// <summary>
    /// Interaction logic for FuelWindow.xaml
    /// </summary>
    public partial class FuelWindow : Window
    {
        public TelemetryData _TelemetryData;
        private GlobalSettings _GlobalSettings;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        FuelUse CurrentLapFuelUse
        {
            get
            {
                var fuelUse = new FuelUse(_TelemetryData.FeedTelemetry.Lap, LastLapFuelLevel - _TelemetryData.FeedTelemetry.FuelLevel, _TelemetryData.FeedTelemetry.FuelLevel, _TelemetryData.FeedTelemetry.OnPitRoad);

                if (Measurement == 1)
                {
                    fuelUse.FuelUsed = fuelUse.FuelUsed * 0.264172;
                    fuelUse.FuelInTank = fuelUse.FuelInTank * 0.264172;
                }

                return fuelUse;
            }
        }
        List<FuelUse> FuelUseList = new List<FuelUse>();
        private double LastLapFuelLevel { get; set; }
        private double FuelToAdd => CurrentLapFuelUse.CalculateFuelToAdd(LapsToEnd, AvgFuelUsage);
        private double AvgFuelUsage
        {
            get
            {
                if (FuelUseList.Count >= 2)
                {
                    var avg = FuelUseList.Select(f => f.FuelUsed).Average();
                    var stdDev = Math.Sqrt(FuelUseList.Select(f => Math.Pow(f.FuelUsed - avg, 2)).Sum() / FuelUseList.Count());
                    var validLaps = FuelUseList.Where(f => Math.Abs(avg - f.FuelUsed) < stdDev).ToList();
                    if (validLaps.Any())
                    {
                        return validLaps.Select(f => f.FuelUsed).Average();
                    }
                }

                return -1.0;
            }
        }

        private double Last5FuelUsage
        {
            get
            {
                if (FuelUseList.Count >= 5)
                {
                    var avg = FuelUseList.Select(f => f.FuelUsed).Average();
                    var stdDev = Math.Sqrt(FuelUseList.Select(f => Math.Pow(f.FuelUsed - AvgFuelUsage, 2)).Sum() / FuelUseList.Count());
                    var validLaps = FuelUseList.Where(f => Math.Abs(AvgFuelUsage - f.FuelUsed) < stdDev).ToList();
                    if (validLaps.Count >= 5)
                    {
                        return validLaps.OrderByDescending(l => l.LapNumber).Take(5).Select(l => l.FuelUsed).Average();
                    }
                }

                return -1.0;
            }
        }

        private double LastFuelUsage
        {
            get
            {
                if (FuelUseList.Any())
                {
                    return FuelUseList.OrderByDescending(l => l.LapNumber).First().FuelUsed;
                }

                return -1.0;
            }
        }

        private double LapsRemaining => _TelemetryData.FeedTelemetry.FuelLevel / AvgFuelUsage;
        private double LapsToEnd => _TelemetryData.FeedTelemetry.SessionTimeRemain / (_TelemetryData.CurrentSession.ResultsFastestLap[0].FastestTime * 1.01);

        private int Measurement = 0;
        private string MeasurementSymbol => Measurement == 0 ? "L" : "gal";

        public bool Locked = false;
        public FuelWindow(TelemetryData telemetryData, GlobalSettings globalSettings, WindowSettings settings)
        {
            InitializeComponent();
            _GlobalSettings = globalSettings;

            Opacity = double.Parse(settings.FuelSettings["Opacity"]);
            Measurement = int.Parse(settings.FuelSettings["Measurement"]);
            Locked = bool.Parse(settings.FuelSettings["Locked"]);
            Left = double.Parse(settings.FuelSettings["XPos"]);
            Top = double.Parse(settings.FuelSettings["YPos"]);
            _TelemetryData = telemetryData;

#if SAMPLE
            LastLapFuelLevel = 100;
#else
            LastLapFuelLevel = _TelemetryData.FeedTelemetry.FuelLevel;
#endif

        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!Locked)
            {
                base.OnMouseLeftButtonDown(e);
                DragMove();
            }
        }

        private void SetDisplaySettings()
        {
            Width = _GlobalSettings.UISize.FuelWindowSettings["WindowWidth"];
            Height = _GlobalSettings.UISize.FuelWindowSettings["WindowHeight"];
            LeftColDefinition.Width = new GridLength(Width / 2);

            MainContainer.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(_GlobalSettings.PrimaryColor);
            FuelTitle.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(_GlobalSettings.SecondaryColor);
            EstLapsTitle.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(_GlobalSettings.SecondaryColor);
            AddTitle.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(_GlobalSettings.SecondaryColor);
            UsageTitle.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(_GlobalSettings.SecondaryColor);

            var appResources = Application.Current.Resources;
            appResources["TitleFontSize"] = (double)_GlobalSettings.UISize.FuelWindowSettings["TitleFontSize"];
            appResources["DataFontSize"] = (double)_GlobalSettings.UISize.FuelWindowSettings["DataFontSize"];

            foreach (var rowDefinition in fuelGrid.RowDefinitions)
            {
                rowDefinition.Height = new GridLength((Height - (Height * 0.025)) / 6.0);
            }

        }

        public void UpdateTelemetryData(TelemetryData telemetryData, WindowSettings settings)
        {
            Opacity = double.Parse(settings.FuelSettings["Opacity"]);
            if (DateTime.UtcNow.Second % 10 == 0)
            {
                Dispatcher.Invoke(() =>
                {
                    Topmost = false;
                    Topmost = true;
                });
            }

            Dispatcher.Invoke(() =>
            {
                Background = Brushes.Transparent;
                SetDisplaySettings();
            });
            _TelemetryData = telemetryData;

            if (_TelemetryData.FeedTelemetry.CarIdxTrackSurface[(int)_TelemetryData.FeedTelemetry["PlayerCarIdx"]] == TrackLocation.InPitStall
                || _TelemetryData.FeedTelemetry.CarIdxTrackSurface[(int)_TelemetryData.FeedTelemetry["PlayerCarIdx"]] == TrackLocation.NotInWorld
                || _TelemetryData.FeedTelemetry.IsReplayPlaying)
            {
                Dispatcher.Invoke(() =>
                {
                    Hide();
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    Show();
                });
            }

            var currentTrackLocation = _TelemetryData.FeedTelemetry.CarIdxTrackSurface[_TelemetryData.FeedSessionData.DriverInfo.DriverCarIdx];
            var lapDistPct = _TelemetryData.FeedTelemetry.CarIdxLapDistPct[_TelemetryData.FeedSessionData.DriverInfo.DriverCarIdx];


#if !SAMPLE
            //Reset calculator after leaving pits
            if (_TelemetryData.CurrentSession.SessionNum != ((int?)_TelemetryData._DataSample.LastSample?.Telemetry?.Session?.SessionNum ?? _TelemetryData.CurrentSession.SessionNum)
                || (_TelemetryData.FeedSessionData.WeekendInfo.SessionID != (_TelemetryData._DataSample.LastSample?.SessionData.WeekendInfo.SessionID ?? _TelemetryData.FeedSessionData.WeekendInfo.SessionID)))
            {
                FuelUseList = new List<FuelUse>();
            }
#endif

            //Only save laps that are not entering or exiting pits or are not representative
            if (!FuelUseList.Select(f => f.LapNumber).Contains(CurrentLapFuelUse.LapNumber)
                && !CurrentLapFuelUse.InPit
                && _TelemetryData.FeedTelemetry.CarIdxEstTime[_TelemetryData.FeedSessionData.DriverInfo.DriverCarIdx] <= _TelemetryData.FeedSessionData.DriverInfo.DriverCarEstLapTime + 15
                && _TelemetryData.TrackLength - (_TelemetryData.FeedTelemetry.CarIdxLapDistPct[_TelemetryData.FeedSessionData.DriverInfo.DriverCarIdx] * _TelemetryData.TrackLength) <= 20)
            {
                if (CurrentLapFuelUse.FuelUsed > 0)
                {
                    FuelUseList.Add(CurrentLapFuelUse);
                    Logger.Info($"Fuel Use Added -- Lap: {CurrentLapFuelUse.LapNumber} - Fuel Used: {CurrentLapFuelUse.FuelUsed} - Fuel In Tank: {CurrentLapFuelUse.FuelInTank} - In Pit {CurrentLapFuelUse.InPit}");
                    Logger.Info($"Fuel Use List Size: {FuelUseList.Count}");
                }

                LastLapFuelLevel = _TelemetryData.FeedTelemetry.FuelLevel;
            }

            Dispatcher.Invoke(() =>
            {
                if (AvgFuelUsage > 0 && FuelUseList.Count >= 3)
                {
                    if (_TelemetryData.FeedTelemetry.FuelLevel < AvgFuelUsage * 2)
                        fuelRemainingCell.Foreground = Brushes.Red;
                    else if (_TelemetryData.FeedTelemetry.FuelLevel < AvgFuelUsage * 5)
                        fuelRemainingCell.Foreground = Brushes.Yellow;
                    else
                        fuelRemainingCell.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#C6C6C6");
                }
                else
                {
                    fuelRemainingCell.Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#C6C6C6");
                }

                fuelRemainingCell.Text = _TelemetryData.FeedTelemetry.FuelLevel <= 0 ? "-" : _TelemetryData.FeedTelemetry.FuelLevel.ToString("N2");
                lapsRemainingCell.Text = LapsRemaining <= 0 ? "-" : LapsRemaining.ToString("N2");
                fuelToAddCell.Text = FuelToAdd < 0 ? "-" : $"{FuelToAdd.ToString("N2")} - {(FuelToAdd + AvgFuelUsage).ToString("N2")} {MeasurementSymbol}";
                avgFuelUsageCell.Text = AvgFuelUsage < 0 ? "-" : $"{AvgFuelUsage.ToString("N2")} {MeasurementSymbol}";
                last5FuelUsageCell.Text = Last5FuelUsage < 0 ? "-" : $"{Last5FuelUsage.ToString("N2")} {MeasurementSymbol}";
                lastFuelUsageCell.Text = LastFuelUsage < 0 ? "-" : $"{LastFuelUsage.ToString("N2")} {MeasurementSymbol}";
            });
        }
    }
}
