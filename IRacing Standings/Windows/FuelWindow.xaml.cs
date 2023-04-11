using iRacingSDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
    /// Interaction logic for FuelWindow.xaml
    /// </summary>
    public partial class FuelWindow : Window
    {
        public TelemetryData _TelemetryData;
        List<FuelUse> FuelUseList = new List<FuelUse>();
        private double LastLapFuelLevel;
        private double FuelRemaining = 0;
        private double FuelToAdd = -1;
        private double AvgFuelUsage = -1;
        private double LapsRemaining = -1;
        private double LapsToEnd = -1;
        private TrackLocation LastTrackLocation = TrackLocation.NotInWorld;
        public bool Locked = false;
        public FuelWindow(TelemetryData telemetryData)
        {
            InitializeComponent();
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            Locked = bool.Parse(mainWindow.WindowSettings.FuelSettings["Locked"]);
            Left = double.Parse(mainWindow.WindowSettings.FuelSettings["XPos"]);
            Top = double.Parse(mainWindow.WindowSettings.FuelSettings["YPos"]);
            _TelemetryData = telemetryData;
            LastLapFuelLevel = _TelemetryData.FeedTelemetry.FuelLevel;
            Dispatcher.Invoke(() =>
            {
                Topmost = true;
                Background = Brushes.Transparent;
            });
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
            _TelemetryData = telemetryData;

            FuelRemaining = _TelemetryData.FeedTelemetry.FuelLevel;
            LapsToEnd = _TelemetryData.FeedTelemetry.SessionTimeRemain / (_TelemetryData.FeedSessionData.DriverInfo.DriverCarEstLapTime * 1.07);
            if (FuelUseList.Count >= 2)
            {
                FuelToAdd = LapsToEnd * AvgFuelUsage - FuelRemaining;
                AvgFuelUsage = FuelUseList.Select(f => f.FuelUsed).Average();
                LapsRemaining = FuelRemaining / AvgFuelUsage;
            }
            var lap = _TelemetryData.FeedTelemetry.Lap;
            var currentLapFuel = new FuelUse(lap, LastLapFuelLevel - FuelRemaining, _TelemetryData.FeedTelemetry.OnPitRoad);
            
            var currentTrackLocation = _TelemetryData.FeedTelemetry.CarIdxTrackSurface[_TelemetryData.FeedSessionData.DriverInfo.DriverCarIdx];
            //Reset calculator after leaving pits
            if (_TelemetryData.CurrentSession.SessionNum != (_TelemetryData.LastSample?.CurrentSession?.SessionNum ?? -1) || (_TelemetryData.FeedSessionData.WeekendInfo.SessionID != _TelemetryData.LastSample?.FeedSessionData.WeekendInfo.SessionID))
            {
                FuelUseList = new List<FuelUse>();
                FuelToAdd = -1;
                AvgFuelUsage = -1;
            } 

            //Only save laps that are not entering or exiting pits or are not representative
            if (!FuelUseList.Select(f => f.LapNumber).Contains(currentLapFuel.LapNumber) 
                && !currentLapFuel.InPit
                && _TelemetryData.FeedTelemetry.CarIdxEstTime[_TelemetryData.FeedSessionData.DriverInfo.DriverCarIdx] <= _TelemetryData.FeedSessionData.DriverInfo.DriverCarEstLapTime + 15
                && _TelemetryData.TrackLength - (_TelemetryData.FeedTelemetry.CarIdxLapDistPct[_TelemetryData.FeedSessionData.DriverInfo.DriverCarIdx] * _TelemetryData.TrackLength) <= 10)
            {
                if (currentLapFuel.FuelUsed > 0 )
                {
                    FuelUseList.Add(currentLapFuel);
                }
            }

            Dispatcher.Invoke(() =>
            {
                if (AvgFuelUsage > 0 && FuelUseList.Count >= 3)
                {
                    if (FuelRemaining < AvgFuelUsage * 2)
                    {
                        fuelRemainingCell.Foreground = Brushes.Red;
                    }
                    else if (FuelRemaining < AvgFuelUsage * 5)
                    {
                        fuelRemainingCell.Foreground = Brushes.Yellow;
                    }
                    else
                    {
                        fuelRemainingCell.Foreground = Brushes.White;
                    }
                }

                fuelRemainingCell.Text = FuelRemaining.ToString("N2");
                fuelUsageCell.Text = AvgFuelUsage.ToString("N2");
                fuelToAddCell.Text = FuelToAdd.ToString("N2");
                lapsRemainingCell.Text = LapsRemaining.ToString("N2");
                

                if (FuelRemaining <= 0)
                {
                    fuelRemainingCell.Text = "-";
                    fuelRemainingCell.Foreground = Brushes.White;
                }

                if (LapsRemaining < 0)
                {
                    lapsRemainingCell.Text = "-";
                }

                if (FuelToAdd < 0)
                {
                    fuelToAddCell.Text = "-";
                }

                if (AvgFuelUsage < 0)
                {
                    fuelUsageCell.Text = "-";
                }
            });
            LastLapFuelLevel = FuelRemaining;
            LastTrackLocation = currentTrackLocation;
        }
    }
}
