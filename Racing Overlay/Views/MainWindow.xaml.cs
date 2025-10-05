using RacingOverlay.Windows;
using iRacingSDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using RacingOverlay.Models;

namespace RacingOverlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Thread thread;
        Thread thread2;
        StandingsWindow StandingsWindow;
        FuelWindow FuelWindow;
        RelativeWindow RelativeWindow;
        TireWindow TireWindow;
        SimpleTrackWindow SimpleTrackWindow;
        FullTrackWindow FullTrackWindow;
        TelemetryData telemetryData;
        public WindowSettings WindowSettings;
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        Configuration _configuration;
        GlobalSettings GlobalSettings = new GlobalSettings();
        public MainWindow(Configuration config)
        {
            InitializeComponent();
            _configuration = config;

            WindowSettings = new WindowSettings(_configuration.AppSettings);
            telemetryData = new TelemetryData();
            telemetryData.StartOperation(telemetryData.RetrieveData);

            GlobalSettings.UISize = new UISize(int.Parse(WindowSettings.GlobalSettings["UIZoom"]));
            GlobalSettings.DriverDisplay = new DriverDisplay(int.Parse(WindowSettings.GlobalSettings["DriverCount"]));

            uiZoom.Value = GlobalSettings.UISize.SizePreset;
            driverDisplayCount.Value = GlobalSettings.DriverDisplay.DisplayCount;

            standingsLock.Content = bool.Parse(WindowSettings.StandingsSettings["Locked"]) ? "Unlock" : "Lock";
            relativeLock.Content = bool.Parse(WindowSettings.RelativeSettings["Locked"]) ? "Unlock" : "Lock";
            fuelLock.Content = bool.Parse(WindowSettings.FuelSettings["Locked"]) ? "Unlock" : "Lock";
            tiresLock.Content = WindowSettings.TireSettings.ContainsKey("Locked") ? bool.Parse(WindowSettings.TireSettings["Locked"]) ? "Unlock" : "Lock" : "Lock";
            simpleTrackLock.Content = bool.Parse(WindowSettings.SimpleTrackSettings["Locked"]) ? "Unlock" : "Lock";
            fullTrackLock.Content = bool.Parse(WindowSettings.FullTrackSettings["Locked"]) ? "Unlock" : "Lock";

            StartOperation(CheckIRacingConnection, thread);
            try
            {
                StartOperation(UpdateLapData, thread2);
            }
            catch (Exception ex) 
            {
                Trace.WriteLine(ex);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void StartOperation(Action action, Thread thread)
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
                            StandingsWindow = new StandingsWindow(new TelemetryData(telemetryData), GlobalSettings);
                            StandingsWindow.Show();
                        });
                    }
                    else
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                        try
                        {
                            StandingsWindow.UpdateTelemetryData(new TelemetryData(telemetryData), GlobalSettings);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex);
                        }
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
                        try
                        {
                            FuelWindow.UpdateTelemetryData(new TelemetryData(telemetryData), false);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex);
                        }
                    }

                    if (RelativeWindow == null)
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                        Dispatcher.Invoke(() =>
                        {
                            RelativeWindow = new RelativeWindow(new TelemetryData(telemetryData), GlobalSettings);
                            RelativeWindow.Show();
                        });
                    }
                    else
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                        try
                        {
                            RelativeWindow.UpdateTelemetryData(new TelemetryData(telemetryData));
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex);
                        }
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

                    if (SimpleTrackWindow == null)
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                        Dispatcher.Invoke(() =>
                        {
                            SimpleTrackWindow = new SimpleTrackWindow(new TelemetryData(telemetryData), GlobalSettings);
                            SimpleTrackWindow.Show();
                        });
                    }
                    else
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                        try
                        {
                            SimpleTrackWindow.UpdateTelemetryData(new TelemetryData(telemetryData));
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex);
                        }
                    }

                    if (FullTrackWindow == null)
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                        Dispatcher.Invoke(() =>
                        {
                            FullTrackWindow = new FullTrackWindow(new TelemetryData(telemetryData));
                            FullTrackWindow.Show();
                        });
                    }
                    else
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }
                        try
                        {
                            if (FullTrackWindow.HasTrackMap())
                                FullTrackWindow.UpdateTelemetryData(new TelemetryData(telemetryData));
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex);
                        }
                    }
                }
                else
                {
                    CloseAllWindows(); 
                }
                Thread.Sleep(16);
            }
        }

        private void UpdateLapData()
        {
            while (true)
            {
                var sessionFastestLap = telemetryData.LapList?.Where(l => l.ValidLap)?.OrderBy(l => l.SpeedData.OrderBy(m => m.Meter).Last().TimeInSeconds)?.FirstOrDefault();

                if (sessionFastestLap != null) 
                {
                    var carPath = telemetryData.FeedSessionData.DriverInfo.Drivers.First(d => d.CarIdx == telemetryData.FeedSessionData.DriverInfo.DriverCarIdx).CarPath;
                    var carClass = telemetryData.FeedSessionData.DriverInfo.Drivers.First(d => d.CarIdx == telemetryData.FeedSessionData.DriverInfo.DriverCarIdx).CarClassID;

                    var fastestLap = telemetryData.SavedSpeedData?.FirstOrDefault(f => f.Key == (int)carClass).Value?.FirstOrDefault(c => c.Key == carPath).Value;

                    if (!sessionFastestLap.CheckFastestLapExists(fastestLap))
                    {
                        sessionFastestLap.SaveLap();
                        Trace.WriteLine("New Fastest Lap Saved: " + sessionFastestLap.SpeedData.OrderBy(s => s.TimeInSeconds).First().TimeInSeconds);
                    }
                }
                
                Thread.Sleep(1000 * 60);
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
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            CloseAllWindows();
            if (tokenSource != null)
            {
                tokenSource.Cancel();
            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            //Environment.Exit(0);
        }

        private void CloseAllWindows()
        {
            StandingsWindow = (StandingsWindow) CloseWindow(StandingsWindow);
            FuelWindow = (FuelWindow) CloseWindow(FuelWindow);
            RelativeWindow = (RelativeWindow) CloseWindow(RelativeWindow);
            TireWindow = (TireWindow) CloseWindow(TireWindow);
            SimpleTrackWindow = (SimpleTrackWindow) CloseWindow(SimpleTrackWindow);
            FullTrackWindow = (FullTrackWindow) CloseWindow(FullTrackWindow);
        }

        private Window CloseWindow(Window window)
        {
            if (window != null)
            {
                if (tokenSource.IsCancellationRequested) { return null ; }
                Dispatcher.Invoke(() =>
                {
                    window.Close();
                    window = null;
                });
            }
            return null;
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
                _configuration.AppSettings.Settings["StandingsWindowLocked"].Value = StandingsWindow.Locked.ToString();
                _configuration.AppSettings.Settings["StandingsWindowXPos"].Value = StandingsWindow.Left.ToString();
                _configuration.AppSettings.Settings["StandingsWindowYPos"].Value = StandingsWindow.Top.ToString();

                _configuration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        private void standingsReset_Click(object sender, RoutedEventArgs e)
        {
            if (StandingsWindow != null)
            {
                StandingsWindow.Left = 0;
                StandingsWindow.Top = 0;
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
                _configuration.AppSettings.Settings["RelativeWindowLocked"].Value = RelativeWindow.Locked.ToString();
                _configuration.AppSettings.Settings["RelativeWindowXPos"].Value = RelativeWindow.Left.ToString();
                _configuration.AppSettings.Settings["RelativeWindowYPos"].Value = RelativeWindow.Top.ToString();

                _configuration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        private void relativeReset_Click(object sender, RoutedEventArgs e)
        {
            if (RelativeWindow != null)
            {
                RelativeWindow.Left = 0;
                RelativeWindow.Top = 0;
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
                _configuration.AppSettings.Settings["FuelWindowLocked"].Value = FuelWindow.Locked.ToString();
                _configuration.AppSettings.Settings["FuelWindowXPos"].Value = FuelWindow.Left.ToString();
                _configuration.AppSettings.Settings["FuelWindowYPos"].Value = FuelWindow.Top.ToString();

                _configuration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        private void fuelReset_Click(object sender, RoutedEventArgs e)
        {
            if (FuelWindow != null)
            {
                FuelWindow.Left = 0;
                FuelWindow.Top = 0;
            }
        }

        private void tiresLock_Click(object sender, RoutedEventArgs e)
        {
            if (TireWindow != null)
            {
                tiresLock.Content = TireWindow.Locked ? "Lock" : "Unlock";
                TireWindow.Locked = !TireWindow.Locked;
                WindowSettings.TireSettings["Locked"] = (TireWindow.Locked).ToString();
            }
        }

        private void tiresSave_Click(object sender, RoutedEventArgs e)
        {
            if (TireWindow != null)
            {
                _configuration.AppSettings.Settings["TireWindowLocked"].Value = TireWindow.Locked.ToString();
                _configuration.AppSettings.Settings["TireWindowXPos"].Value = TireWindow.Left.ToString();
                _configuration.AppSettings.Settings["TireWindowYPos"].Value = TireWindow.Top.ToString();

                _configuration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        private void tiresReset_Click(object sender, RoutedEventArgs e)
        {
            if (TireWindow != null)
            {
                TireWindow.Left = 0;
                TireWindow.Top = 0;
            }
        }

        private void simpleTrackLock_Click(object sender, RoutedEventArgs e)
        {
            if (SimpleTrackWindow != null)
            {
                simpleTrackLock.Content = SimpleTrackWindow.Locked ? "Lock" : "Unlock";
                SimpleTrackWindow.Locked = !SimpleTrackWindow.Locked;
                WindowSettings.SimpleTrackSettings["Locked"] = (SimpleTrackWindow.Locked).ToString();
            }
        }

        private void simpleTrackSave_Click(object sender, RoutedEventArgs e)
        {
            if (SimpleTrackWindow != null)
            {
                _configuration.AppSettings.Settings["SimpleTrackWindowLocked"].Value = SimpleTrackWindow.Locked.ToString();
                _configuration.AppSettings.Settings["SimpleTrackWindowXPos"].Value = SimpleTrackWindow.Left.ToString();
                _configuration.AppSettings.Settings["SimpleTrackWindowYPos"].Value = SimpleTrackWindow.Top.ToString();

                _configuration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        private void simpleTrackReset_Click(object sender, RoutedEventArgs e)
        {
            if (SimpleTrackWindow != null)
            {
                SimpleTrackWindow.Left = 0;
                SimpleTrackWindow.Top = 0;
            }
        }

        private void fullTrackLock_Click(object sender, RoutedEventArgs e)
        {
            if (FullTrackWindow != null)
            {
                fullTrackLock.Content = FullTrackWindow.Locked ? "Lock" : "Unlock";
                FullTrackWindow.Locked = !FullTrackWindow.Locked;
                WindowSettings.FullTrackSettings["Locked"] = (FullTrackWindow.Locked).ToString();
            }
        }

        private void fullTrackSave_Click(object sender, RoutedEventArgs e)
        {
            if (FullTrackWindow != null)
            {
                _configuration.AppSettings.Settings["FullTrackWindowLocked"].Value = FullTrackWindow.Locked.ToString();
                _configuration.AppSettings.Settings["FullTrackWindowXPos"].Value = FullTrackWindow.Left.ToString();
                _configuration.AppSettings.Settings["FullTrackWindowYPos"].Value = FullTrackWindow.Top.ToString();

                _configuration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        private void fullTrackReset_Click(object sender, RoutedEventArgs e)
        {
            if (FullTrackWindow != null)
            {
                FullTrackWindow.Left = 0;
                FullTrackWindow.Top = 0;
            }
        }

        private void uiZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            GlobalSettings.UISize = new UISize((int)uiZoom.Value);
        }

        private void driverDisplayCount_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            GlobalSettings.DriverDisplay = new DriverDisplay((int)driverDisplayCount.Value);
        }

        private void globalSettingsSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            _configuration.AppSettings.Settings["UIZoom"].Value = uiZoom.Value.ToString();
            _configuration.AppSettings.Settings["DriverCount"].Value = driverDisplayCount.Value.ToString();

            _configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
