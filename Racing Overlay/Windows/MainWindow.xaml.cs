﻿using IRacing_Standings.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace IRacing_Standings
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Thread thread;
        StandingsWindow StandingsWindow;
        FuelWindow FuelWindow;
        RelativeWindow RelativeWindow;
        TireWindow TireWindow;
        LiveTrackWindow TrackMapWindow;
        TelemetryData telemetryData;
        public WindowSettings WindowSettings;
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        public MainWindow(WindowSettings windowSettings)
        {
            InitializeComponent();
            WindowSettings = windowSettings;
            telemetryData = new TelemetryData();
            telemetryData.StartOperation(telemetryData.RetrieveData);

            standingsLock.Content = bool.Parse(WindowSettings.StandingsSettings["Locked"]) ? "Unlock" : "Lock";
            relativeLock.Content = bool.Parse(WindowSettings.RelativeSettings["Locked"]) ? "Unlock" : "Lock";
            fuelLock.Content = bool.Parse(WindowSettings.FuelSettings["Locked"]) ? "Unlock" : "Lock";
            
            StartOperation(CheckIRacingConnection);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void StartOperation(Action action)
        {
            if (thread != null)
                thread.Abort();

            thread = new Thread(() => action());
            thread.Start();
        }

        private void CheckIRacingConnection()
        {
            while (true)
            {
                var localTelemetryData = new TelemetryData(telemetryData);
                if (telemetryData.IsConnected)
                {
                    if (StandingsWindow == null)
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                        Dispatcher.Invoke(() =>
                        {
                            StandingsWindow = new StandingsWindow(new TelemetryData(telemetryData));
                            StandingsWindow.Show();
                        });
                    }
                    else
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                        StandingsWindow.UpdateTelemetryData(new TelemetryData(telemetryData));
                    }
                    if (FuelWindow == null)
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                        Dispatcher.Invoke(() =>
                        {
                            FuelWindow = new FuelWindow(new TelemetryData(telemetryData));
                            FuelWindow.Show();
                        });
                    }
                    else
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                        FuelWindow.UpdateTelemetryData(new TelemetryData(telemetryData));
                    }
                    if (RelativeWindow == null)
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                        Dispatcher.Invoke(() =>
                        {
                            RelativeWindow = new RelativeWindow(new TelemetryData(telemetryData));
                            RelativeWindow.Show();
                        });
                    }
                    else
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                        RelativeWindow.UpdateTelemetryData(new TelemetryData(telemetryData));
                    }
                    if (TireWindow == null)
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                        Dispatcher.Invoke(() =>
                        {
                            TireWindow = new TireWindow(new TelemetryData(telemetryData));
                            TireWindow.Show();
                        });
                    }
                    else
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                        TireWindow.UpdateTelemetryData(new TelemetryData(telemetryData));
                    }

                    /*
                    if (TrackMapWindow == null)
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                        Dispatcher.Invoke(() =>
                        {
                            TrackMapWindow = new LiveTrackWindow(new TelemetryData(telemetryData));
                            TrackMapWindow.Show();
                        });
                    }
                    else
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                        TrackMapWindow.UpdateTelemetryData(new TelemetryData(telemetryData));
                    }
                    */
                }
                else
                {
                    CloseAllWindows();
                    
                }
                Thread.Sleep(16);
            }
        }

        private void fuelButton_Click(object sender, RoutedEventArgs e)
        {
            if (FuelWindow == null)
            {
                FuelWindow = new FuelWindow(telemetryData);
                FuelWindow.Show();
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var sessionFastestLap = telemetryData.LapList?.Where(l => l.ValidLap)?.OrderBy(l => l.SpeedData.OrderBy(m => m.Meter).Last().TimeInSeconds)?.FirstOrDefault();

            if (sessionFastestLap == null) { return; }
            var carPath = telemetryData.FeedSessionData.DriverInfo.Drivers.First(d => d.CarIdx == telemetryData.FeedSessionData.DriverInfo.DriverCarIdx).CarPath;
            var carClass = telemetryData.FeedSessionData.DriverInfo.Drivers.First(d => d.CarIdx == telemetryData.FeedSessionData.DriverInfo.DriverCarIdx).CarClassID;

            sessionFastestLap.CheckFastestLap(telemetryData.SavedSpeedData.FirstOrDefault(s => s.Key == (int)carClass).Value.FirstOrDefault(s => s.Key == carPath).Value);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            var test = Lap.GetSpeedData(144, "dallarap217");
        }

        private void standingsLock_Click(object sender, RoutedEventArgs e)
        {
            if (StandingsWindow != null)
            {
                standingsLock.Content = StandingsWindow.Locked ? "Lock" : "Unlock";
                StandingsWindow.Locked = !StandingsWindow.Locked;
                WindowSettings.StandingsSettings["Locked"] = (StandingsWindow.Locked).ToString();
            }
        }

        private void standingsSave_Click(object sender, RoutedEventArgs e)
        {
            if (StandingsWindow != null)
            {
                WindowSettings.StandingsSettings["XPos"] = StandingsWindow.Left.ToString();
                WindowSettings.StandingsSettings["YPos"] = StandingsWindow.Top.ToString();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            CloseAllWindows();
            if (tokenSource != null)
            {
                tokenSource.Cancel();
            }
        }

        private void CloseAllWindows()
        {
            if (StandingsWindow != null)
            { 
                if (tokenSource.IsCancellationRequested) { return; }
                Dispatcher.Invoke(() =>
                {
                    StandingsWindow.Close();
                    StandingsWindow = null;
                });
            }
            if (FuelWindow != null)
            {
                if (tokenSource.IsCancellationRequested) { return; }
                Dispatcher.Invoke(() =>
                {
                    FuelWindow.Close();
                    FuelWindow = null;
                });
            }
            if (RelativeWindow != null)
            {
                if (tokenSource.IsCancellationRequested) { return; }
                Dispatcher.Invoke(() =>
                {
                    RelativeWindow.Close();
                    RelativeWindow = null;
                });
            }
            if (TireWindow != null)
            {
                if (tokenSource.IsCancellationRequested) { return; }
                Dispatcher.Invoke(() =>
                {
                    TireWindow.Close();
                    TireWindow = null;
                });
            }
            if (TrackMapWindow != null)
            {
                if (tokenSource.IsCancellationRequested) { return; }
                Dispatcher.Invoke(() =>
                {
                    TrackMapWindow.Close();
                    TrackMapWindow = null;
                });
            }
        }

        private void relativeLock_Click(object sender, RoutedEventArgs e)
        {
            if (RelativeWindow != null)
            {
                relativeLock.Content = RelativeWindow.Locked ? "Lock" : "Unlock";
                RelativeWindow.Locked = !RelativeWindow.Locked;
                WindowSettings.RelativeSettings["Locked"] = (RelativeWindow.Locked).ToString();
            }
        }

        private void relativeSave_Click(object sender, RoutedEventArgs e)
        {
            if (RelativeWindow != null)
            {
                WindowSettings.RelativeSettings["XPos"] = RelativeWindow.Left.ToString();
                WindowSettings.RelativeSettings["YPos"] = RelativeWindow.Top.ToString();
            }
        }

        private void fuelLock_Click(object sender, RoutedEventArgs e)
        {
            if (FuelWindow != null)
            {
                fuelLock.Content = FuelWindow.Locked ? "Lock" : "Unlock";
                FuelWindow.Locked = !FuelWindow.Locked;
                WindowSettings.FuelSettings["Locked"] = (FuelWindow.Locked).ToString();
            }
        }

        private void fuelSave_Click(object sender, RoutedEventArgs e)
        {
            if (FuelWindow != null)
            {
                WindowSettings.FuelSettings["XPos"] = FuelWindow.Left.ToString();
                WindowSettings.FuelSettings["YPos"] = FuelWindow.Top.ToString();
            }
        }
    }
}
