using iRacingSDK;
using RacingOverlay.Models;
using RacingOverlay.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace RacingOverlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Thread MainThread;
        Thread SecondaryThread;
        Thread FullTrackThread = null;
        Thread SimpleTrackThread = null;
        Thread StandingsThread = null;
        Thread RelativeThread = null;
        Thread FuelThread = null;
        Thread TireThread = null;

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
        private bool _Initialized = false;
        public MainWindow(Configuration config)
        {
            InitializeComponent();
            _Initialized = true;

            telemetryData = new TelemetryData();
            telemetryData.StartOperation(telemetryData.RetrieveData);

            _configuration = config;
            WindowSettings = new WindowSettings(_configuration.AppSettings);
            GlobalSettings.UISize = new UISize(int.Parse(WindowSettings.GlobalSettings["UIZoom"]));
            GlobalSettings.DriverDisplay = new DriverDisplay(int.Parse(WindowSettings.GlobalSettings["DriverCount"]));
            GlobalSettings.SimpleTrackSettings = new SimpleTrackSettings(int.Parse(WindowSettings.GlobalSettings["UIZoom"]), int.Parse(WindowSettings.SimpleTrackSettings["Width"]));

            uiZoom.Value = GlobalSettings.UISize.SizePreset;
            driverDisplayCount.Value = GlobalSettings.DriverDisplay.DisplayCount;
            SimpleTrackWidth.Value = GlobalSettings.SimpleTrackSettings.ContainerWidth;

            standingsLock.Content = bool.Parse(WindowSettings.StandingsSettings["Locked"]) ? "Unlock" : "Lock";
            relativeLock.Content = bool.Parse(WindowSettings.RelativeSettings["Locked"]) ? "Unlock" : "Lock";
            fuelLock.Content = bool.Parse(WindowSettings.FuelSettings["Locked"]) ? "Unlock" : "Lock";
            tiresLock.Content = WindowSettings.TireSettings.ContainsKey("Locked") ? bool.Parse(WindowSettings.TireSettings["Locked"]) ? "Unlock" : "Lock" : "Lock";
            simpleTrackLock.Content = bool.Parse(WindowSettings.SimpleTrackSettings["Locked"]) ? "Unlock" : "Lock";
            fullTrackLock.Content = bool.Parse(WindowSettings.FullTrackSettings["Locked"]) ? "Unlock" : "Lock";

            ThreadPool.SetMaxThreads(10, 10);

            StartOperation(CheckIRacingConnection, MainThread);
            try
            {
                StartOperation(UpdateLapData, SecondaryThread);
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
            float posIndex = 0.0F;
#if SAMPLE
            telemetryData = TelemetryData.CreateSampleData();
            telemetryData.LastSample = telemetryData;
            var loops = 0;
#endif
            while (true)
            {
                var loopStart = DateTime.UtcNow;
#if SAMPLE
                telemetryData.UpdateSampleData();
#endif

                if (telemetryData.IsConnected)
                {
                    if (FullTrackThread == null)
                    {
                        FullTrackThread = new Thread(() =>
                        {
                            if (tokenSource.IsCancellationRequested)
                            {
                                return;
                            }
                    
                            FullTrackWindow = new FullTrackWindow(new TelemetryData(telemetryData), GlobalSettings, WindowSettings);
                            FullTrackWindow.Show();
                    
                            Dispatcher.Run();
                               
                        });
                        FullTrackThread.SetApartmentState(ApartmentState.STA);
                        FullTrackThread.Start();
                    }
                    else
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            return;
                        }
                    
                        if (FullTrackWindow != null && FullTrackWindow.HasTrackMap(out DrawingImage map))
                        {
                            FullTrackWindow?.Dispatcher.Invoke(() =>
                            {
                                FullTrackWindow?.UpdateTelemetryData(new TelemetryData(telemetryData));
                            });
                        }
                    }

                    if (SimpleTrackThread == null)
                    {
                        SimpleTrackThread = new Thread(() =>
                        {
                            if (tokenSource.IsCancellationRequested)
                            {
                                return;
                            }
                    
                            if (SimpleTrackWindow == null) 
                            {
                                SimpleTrackWindow = new SimpleTrackWindow(new TelemetryData(telemetryData), GlobalSettings, WindowSettings);
                                SimpleTrackWindow.Show();
                    
                                Dispatcher.Run();
                            }
                     
                        });
                        SimpleTrackThread.SetApartmentState(ApartmentState.STA);
                        SimpleTrackThread.Start();
                    }
                    else 
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            return;
                        }

                        SimpleTrackWindow?.Dispatcher.Invoke(() =>
                        {
                            SimpleTrackWindow.UpdateTelemetryData(new TelemetryData(telemetryData));
                        });
                    }


                    
                    if (StandingsThread == null)
                    {
                        StandingsThread = new Thread(() =>
                        {
                            if (tokenSource.IsCancellationRequested)
                            {
                                return;
                            }
                    
                            StandingsWindow = new StandingsWindow(new TelemetryData(telemetryData), GlobalSettings, WindowSettings);
                            StandingsWindow.Show();
                    
                            Dispatcher.Run();
                            
                        });
                        StandingsThread.SetApartmentState(ApartmentState.STA);
                        StandingsThread.Start();
                    }
                    else
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            return;
                        }
                    
                        StandingsWindow?.Dispatcher.Invoke(() =>
                        {
                            StandingsWindow?.UpdateTelemetryData(new TelemetryData(telemetryData));
                        });
                    }

                    if (RelativeThread == null) 
                    {
                        RelativeThread = new Thread(() =>
                        {

                            if (tokenSource.IsCancellationRequested)
                            {
                                return;
                            }

                            RelativeWindow = new RelativeWindow(new TelemetryData(telemetryData), GlobalSettings, WindowSettings);
                            RelativeWindow.Show();

                            Dispatcher.Run();
                        });
                        RelativeThread.SetApartmentState(ApartmentState.STA);
                        RelativeThread.Start();
                    }
                    else
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            return;
                        }

                        RelativeWindow?.Dispatcher.Invoke(() =>
                        {
                            RelativeWindow?.UpdateTelemetryData(new TelemetryData(telemetryData));
                        });
                    }

                    if (FuelThread == null)
                    {
                        FuelThread = new Thread(() =>
                        {

                            if (tokenSource.IsCancellationRequested)
                            {
                                return;
                            }

                            FuelWindow = new FuelWindow(new TelemetryData(telemetryData), GlobalSettings, WindowSettings);
                            FuelWindow.Show();

                            Dispatcher.Run();
                        });

                        FuelThread.SetApartmentState(ApartmentState.STA);
                        FuelThread.Start();
                    }
                    else
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            return;
                        }

                        FuelWindow?.Dispatcher.Invoke(() =>
                        {
                            FuelWindow?.UpdateTelemetryData(new TelemetryData(telemetryData));
                        });
                    }

                    if (TireThread == null)
                    {
                        TireThread = new Thread(() =>
                        {

                            if (tokenSource.IsCancellationRequested)
                            {
                                return;
                            }

                            TireWindow = new TireWindow(new TelemetryData(telemetryData), WindowSettings);
                            TireWindow.Show();

                            Dispatcher.Run();
                        });

                        TireThread.SetApartmentState(ApartmentState.STA);
                        TireThread.Start();
                    }
                    else
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            return;
                        }

                        TireWindow?.Dispatcher.Invoke(() =>
                        {
                            TireWindow?.UpdateTelemetryData(new TelemetryData(telemetryData));
                        });
                    }
                }
                else
                {
                    CloseAllWindows();
                }

                var loopTime = (DateTime.UtcNow - loopStart).TotalMilliseconds;
                if (loopTime < 16)
                {
                    Thread.Sleep((int)(16 - loopTime));
                }

            }
        }

        private void UpdateLapData()
        {
            while (true)
            {
                var sessionFastestLap = telemetryData?.LapList?.Where(l => l.ValidLap)?.OrderBy(l => l.SpeedData.OrderBy(m => m.Meter).Last().TimeInSeconds)?.FirstOrDefault();

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
            Environment.Exit(0);
        }

        private void CloseAllWindows()
        {
            StandingsWindow = (StandingsWindow)CloseWindow(StandingsWindow);
            FuelWindow = (FuelWindow)CloseWindow(FuelWindow);
            RelativeWindow = (RelativeWindow)CloseWindow(RelativeWindow);
            TireWindow = (TireWindow)CloseWindow(TireWindow);
            SimpleTrackWindow = (SimpleTrackWindow)CloseWindow(SimpleTrackWindow);
            FullTrackWindow = (FullTrackWindow)CloseWindow(FullTrackWindow);
        }

        private Window CloseWindow(Window window)
        {
            if (window != null)
            {
                if (tokenSource.IsCancellationRequested) { return null; }
                window.Dispatcher.Invoke(() =>
                {
                    window.Close();
                });
                window = null;
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
                _configuration.AppSettings.Settings["SimpleTrackWindowWidth"].Value = SimpleTrackWidth.Value.ToString();

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

        private void SimpleTrackWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_Initialized)
                GlobalSettings.SimpleTrackSettings.ContainerWidth = (int)SimpleTrackWidth.Value;
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
