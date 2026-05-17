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
using System.Windows.Controls;

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
        DrawingImage UnlockedIcon;
        DrawingImage LockedIcon;
        DrawingImage VisibleIcon;
        DrawingImage HiddenIcon;

        public MainWindow(Configuration config)
        {
            InitializeComponent();
            _Initialized = true;

            telemetryData = new TelemetryData();
            telemetryData.StartOperation(telemetryData.RetrieveData);

            _configuration = config;

            InitializeSettings();     
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

        private void InitializeSettings()
        {
            WindowSettings = new WindowSettings(_configuration.AppSettings);
            GlobalSettings.DriverDisplay = new DriverDisplay(int.Parse(WindowSettings.GlobalSettings["DriverCount"]));
            GlobalSettings.StandingsSettings = new StandingsWindowSettings(int.Parse(WindowSettings.StandingsSettings["Size"]));
            GlobalSettings.RelativeWindowSettings = new RelativeWindowSettings(int.Parse(WindowSettings.RelativeSettings["Size"]));
            GlobalSettings.FuelWindowSettings = new FuelWindowSettings(int.Parse(WindowSettings.FuelSettings["Size"]));
            GlobalSettings.TireWindowSettings = new TireWindowSettings(int.Parse(WindowSettings.TireSettings["Size"]));
            GlobalSettings.SimpleTrackSettings = new SimpleTrackSettings(int.Parse(WindowSettings.SimpleTrackSettings["Size"]), int.Parse(WindowSettings.SimpleTrackSettings["Width"]));
            GlobalSettings.FullTrackSettings = new FullTrackSettings(int.Parse(WindowSettings.FullTrackSettings["Size"]));

            driverDisplayCount.Value = GlobalSettings.DriverDisplay.DisplayCount;
            StandingsSize.Value = GlobalSettings.StandingsSettings.SizePreset;
            RelativeSize.Value = GlobalSettings.RelativeWindowSettings.SizePreset;
            FuelSize.Value = GlobalSettings.FuelWindowSettings.SizePreset;
            TireSize.Value = GlobalSettings.TireWindowSettings.SizePreset;
            SimpleTrackWidth.Value = GlobalSettings.SimpleTrackSettings.ContainerWidth;
            FullTrackSize.Value = GlobalSettings.FullTrackSettings.SizePreset;
            double.TryParse(WindowSettings.FuelSettings["Measurement"], out double measurement);
            FuelMeasurement.Value = measurement;

            InitializeIcons();
            InitializeOpacitySettings();
            InitializeLockedSettings();
            InitializeVisibilitySettings();
        }

        private void InitializeIcons()
        {
            UnlockedIcon = (DrawingImage)FindResource("di_unlocked_xaml");
            LockedIcon = (DrawingImage)FindResource("di_locked_xaml");
            VisibleIcon = (DrawingImage)FindResource("di_view_xaml");
            HiddenIcon = VisibleIcon.Clone();

            var visibleIconGroup = ((DrawingGroup)VisibleIcon.Drawing).Children[0];
            var visibleSclera = (GeometryDrawing)((DrawingGroup)visibleIconGroup).Children[0];
            var visiblePupil = (GeometryDrawing)((DrawingGroup)visibleIconGroup).Children[1];
            var visibleBorder = (GeometryDrawing)((DrawingGroup)visibleIconGroup).Children[2];
            visibleSclera.Brush = GlobalSettings.PrimaryTextColorBrush;
            visiblePupil.Brush = GlobalSettings.MenuPrimaryColorBrush;
            visibleBorder.Pen.Brush = GlobalSettings.PrimaryTextColorBrush;

            var hiddenIconGroup = ((DrawingGroup)HiddenIcon.Drawing).Children[0];
            var hiddenSclera = (GeometryDrawing)((DrawingGroup)hiddenIconGroup).Children[0];
            var hiddenPupil = (GeometryDrawing)((DrawingGroup)hiddenIconGroup).Children[1];
            var hiddenBorder = (GeometryDrawing)((DrawingGroup)hiddenIconGroup).Children[2];
            hiddenSclera.Brush = GlobalSettings.MenuPrimaryColorBrush;
            hiddenPupil.Brush = GlobalSettings.PrimaryTextColorBrush;
            hiddenBorder.Pen.Brush = GlobalSettings.PrimaryTextColorBrush;
        }
        private void InitializeOpacitySettings()
        {
            StandingsOpacity.Value = double.Parse(WindowSettings.StandingsSettings["Opacity"]) * 100;
            RelativeOpacity.Value = double.Parse(WindowSettings.RelativeSettings["Opacity"]) * 100;
            TireOpacity.Value = double.Parse(WindowSettings.TireSettings["Opacity"]) * 100;
            FuelOpacity.Value = double.Parse(WindowSettings.FuelSettings["Opacity"]) * 100;
            SimpleTrackOpacity.Value = double.Parse(WindowSettings.SimpleTrackSettings["Opacity"]) * 100;
            FullTrackOpacity.Value = double.Parse(WindowSettings.FullTrackSettings["Opacity"]) * 100;
        }

        private void InitializeLockedSettings()
        {
            standingsLock.Content = bool.Parse(WindowSettings.StandingsSettings["Locked"]) ? new Image { Source = LockedIcon } : new Image { Source = UnlockedIcon };
            relativeLock.Content = bool.Parse(WindowSettings.RelativeSettings["Locked"]) ? new Image { Source = LockedIcon } : new Image { Source = UnlockedIcon };
            tiresLock.Content = WindowSettings.TireSettings.ContainsKey("Locked") ? bool.Parse(WindowSettings.TireSettings["Locked"]) ? new Image { Source = LockedIcon } : new Image { Source = UnlockedIcon } : new Image { Source = UnlockedIcon };
            fuelLock.Content = bool.Parse(WindowSettings.FuelSettings["Locked"]) ? new Image { Source = LockedIcon } : new Image { Source = UnlockedIcon };
            simpleTrackLock.Content = bool.Parse(WindowSettings.SimpleTrackSettings["Locked"]) ? new Image { Source = LockedIcon } : new Image { Source = UnlockedIcon };
            fullTrackLock.Content = bool.Parse(WindowSettings.FullTrackSettings["Locked"]) ? new Image { Source = LockedIcon } : new Image { Source = UnlockedIcon };
        }

        private void InitializeVisibilitySettings()
        {
            standingsVisibility.Content = bool.Parse(WindowSettings.StandingsSettings["Visible"]) ? new Image { Source = VisibleIcon } : new Image { Source = HiddenIcon };
            relativeVisibility.Content = bool.Parse(WindowSettings.RelativeSettings["Visible"]) ? new Image { Source = VisibleIcon } : new Image { Source = HiddenIcon };
            tiresVisibility.Content = bool.Parse(WindowSettings.TireSettings["Visible"]) ? new Image { Source = VisibleIcon } : new Image { Source = HiddenIcon };
            fuelVisibility.Content = bool.Parse(WindowSettings.FuelSettings["Visible"]) ? new Image { Source = VisibleIcon } : new Image { Source = HiddenIcon };
            simpleTrackVisibility.Content = bool.Parse(WindowSettings.SimpleTrackSettings["Visible"]) ? new Image { Source = VisibleIcon } : new Image { Source = HiddenIcon };
            fullTrackVisibility.Content = bool.Parse(WindowSettings.FullTrackSettings["Visible"]) ? new Image { Source = VisibleIcon } : new Image { Source = HiddenIcon };
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

                            if (bool.Parse(WindowSettings.FullTrackSettings["Visible"]))
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
                                if (bool.Parse(WindowSettings.FullTrackSettings["Visible"]))
                                {
                                    FullTrackWindow.Show();
                                    FullTrackWindow?.UpdateTelemetryData(new TelemetryData(telemetryData), WindowSettings);
                                }
                                else
                                    FullTrackWindow.Hide();
                            });
                        }
                        else
                        {
                            FullTrackWindow?.Dispatcher.Invoke(() =>
                            {
                                FullTrackWindow?.Hide();
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

                                if (bool.Parse(WindowSettings.SimpleTrackSettings["Visible"]))
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

                        SimpleTrackWindow?.Dispatcher?.Invoke(() =>
                        {
                            if (bool.Parse(WindowSettings.SimpleTrackSettings["Visible"]))
                            {
                                SimpleTrackWindow.Show();
                                SimpleTrackWindow?.UpdateTelemetryData(new TelemetryData(telemetryData), WindowSettings);
                            }
                            else
                                SimpleTrackWindow.Hide();
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

                            if (bool.Parse(WindowSettings.StandingsSettings["Visible"]))
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
                            if (bool.Parse(WindowSettings.StandingsSettings["Visible"]))
                            {
                                StandingsWindow.Show();
                                StandingsWindow?.UpdateTelemetryData(new TelemetryData(telemetryData), WindowSettings);
                            }
                            else
                                StandingsWindow.Hide();
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

                            if (bool.Parse(WindowSettings.RelativeSettings["Visible"]))
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
                            if (bool.Parse(WindowSettings.RelativeSettings["Visible"]))
                            {
                                RelativeWindow.Show();
                                RelativeWindow?.UpdateTelemetryData(new TelemetryData(telemetryData), WindowSettings);
                            }
                            else
                                RelativeWindow.Hide();
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

                            if (bool.Parse(WindowSettings.FuelSettings["Visible"]))
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
                            if (bool.Parse(WindowSettings.FuelSettings["Visible"]))
                            {
                                FuelWindow.Show();
                                FuelWindow?.UpdateTelemetryData(new TelemetryData(telemetryData), WindowSettings);
                            }
                            else
                                FuelWindow.Hide();
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

                            if (bool.Parse(WindowSettings.TireSettings["Visible"]))
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
                            if (bool.Parse(WindowSettings.TireSettings["Visible"]))
                            {
                                TireWindow.Show();
                                TireWindow?.UpdateTelemetryData(new TelemetryData(telemetryData), WindowSettings);
                            }
                            else
                                TireWindow.Hide();
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

        #region Save
        private void standingsSave_Click(object sender, RoutedEventArgs e)
        {
            if (StandingsWindow != null)
            {
                _configuration.AppSettings.Settings["StandingsWindowSize"].Value = ((int)StandingsSize.Value).ToString();
                StandingsWindow?.Dispatcher.Invoke(() =>
                {
                    _configuration.AppSettings.Settings["StandingsWindowVisible"].Value = (StandingsWindow.Visibility == Visibility.Visible).ToString();
                    _configuration.AppSettings.Settings["StandingsWindowOpacity"].Value = StandingsWindow.Opacity.ToString();
                    _configuration.AppSettings.Settings["StandingsWindowLocked"].Value = StandingsWindow.Locked.ToString();
                    _configuration.AppSettings.Settings["StandingsWindowXPos"].Value = StandingsWindow.Left.ToString();
                    _configuration.AppSettings.Settings["StandingsWindowYPos"].Value = StandingsWindow.Top.ToString();
                });

                _configuration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        private void relativeSave_Click(object sender, RoutedEventArgs e)
        {
            if (RelativeWindow != null)
            {
                _configuration.AppSettings.Settings["RelativeWindowSize"].Value = ((int)RelativeSize.Value).ToString();
                RelativeWindow?.Dispatcher.Invoke(() =>
                {
                    _configuration.AppSettings.Settings["RelativeWindowVisible"].Value = (RelativeWindow.Visibility == Visibility.Visible).ToString();
                    _configuration.AppSettings.Settings["RelativeWindowOpacity"].Value = RelativeWindow.Opacity.ToString();
                    _configuration.AppSettings.Settings["RelativeWindowLocked"].Value = RelativeWindow.Locked.ToString();
                    _configuration.AppSettings.Settings["RelativeWindowXPos"].Value = RelativeWindow.Left.ToString();
                    _configuration.AppSettings.Settings["RelativeWindowYPos"].Value = RelativeWindow.Top.ToString();
                });

                _configuration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        private void fuelSave_Click(object sender, RoutedEventArgs e)
        {
            if (FuelWindow != null)
            {
                _configuration.AppSettings.Settings["FuelWindowSize"].Value = ((int)FuelSize.Value).ToString();
                FuelWindow?.Dispatcher.Invoke(() =>
                {
                    _configuration.AppSettings.Settings["FuelWindowVisible"].Value = (FuelWindow.Visibility == Visibility.Visible).ToString();
                    _configuration.AppSettings.Settings["FuelWindowOpacity"].Value = FuelWindow.Opacity.ToString();
                    _configuration.AppSettings.Settings["FuelWindowLocked"].Value = FuelWindow.Locked.ToString();
                    _configuration.AppSettings.Settings["FuelWindowXPos"].Value = FuelWindow.Left.ToString();
                    _configuration.AppSettings.Settings["FuelWindowYPos"].Value = FuelWindow.Top.ToString();
                });

                _configuration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        private void tiresSave_Click(object sender, RoutedEventArgs e)
        {
            if (TireWindow != null)
            {

                TireWindow?.Dispatcher.Invoke(() =>
                {
                    _configuration.AppSettings.Settings["TireWindowOpacity"].Value = TireWindow.Opacity.ToString();
                    _configuration.AppSettings.Settings["TireWindowLocked"].Value = TireWindow.Locked.ToString();
                    _configuration.AppSettings.Settings["TireWindowXPos"].Value = TireWindow.Left.ToString();
                    _configuration.AppSettings.Settings["TireWindowYPos"].Value = TireWindow.Top.ToString();
                });


                _configuration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        private void simpleTrackSave_Click(object sender, RoutedEventArgs e)
        {
            if (SimpleTrackWindow != null)
            {
                SimpleTrackWindow?.Dispatcher.Invoke(() =>
                {
                    _configuration.AppSettings.Settings["SimpleTrackWindowOpacity"].Value = SimpleTrackWindow.Opacity.ToString();
                    _configuration.AppSettings.Settings["SimpleTrackWindowLocked"].Value = SimpleTrackWindow.Locked.ToString();
                    _configuration.AppSettings.Settings["SimpleTrackWindowXPos"].Value = SimpleTrackWindow.Left.ToString();
                    _configuration.AppSettings.Settings["SimpleTrackWindowYPos"].Value = SimpleTrackWindow.Top.ToString();
                });
                _configuration.AppSettings.Settings["SimpleTrackWindowWidth"].Value = SimpleTrackWidth.Value.ToString();

                _configuration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        private void fullTrackSave_Click(object sender, RoutedEventArgs e)
        {
            if (FullTrackWindow != null)
            {
                FullTrackWindow?.Dispatcher.Invoke(() =>
                {
                    _configuration.AppSettings.Settings["FullTrackWindowOpacity"].Value = FullTrackWindow.Opacity.ToString();
                    _configuration.AppSettings.Settings["FullTrackWindowLocked"].Value = FullTrackWindow.Locked.ToString();
                    _configuration.AppSettings.Settings["FullTrackWindowXPos"].Value = FullTrackWindow.Left.ToString();
                    _configuration.AppSettings.Settings["FullTrackWindowYPos"].Value = FullTrackWindow.Top.ToString();
                });
                _configuration.AppSettings.Settings["FullTrackWindowSize"].Value = ((int)FullTrackSize.Value).ToString();

                _configuration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        private void globalSettingsSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            _configuration.AppSettings.Settings["DriverCount"].Value = driverDisplayCount.Value.ToString();

            _configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        #endregion

        #region Opacity
        private void StandingsOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (StandingsWindow != null)
            {
                WindowSettings.StandingsSettings["Opacity"] = (StandingsOpacity.Value / 100).ToString();
            }
        }

        private void RelativeOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (RelativeWindow != null)
            {
                WindowSettings.RelativeSettings["Opacity"] = (RelativeOpacity.Value / 100).ToString();
            }
        }

        private void FuelOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (FuelWindow != null)
            {
                WindowSettings.FuelSettings["Opacity"] = (FuelOpacity.Value / 100).ToString();
            }
        }

        private void TireOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TireWindow != null)
            {
                WindowSettings.TireSettings["Opacity"] = (TireOpacity.Value / 100).ToString();
            }
        }

        private void SimpleTrackOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SimpleTrackWindow != null)
            {
                WindowSettings.SimpleTrackSettings["Opacity"] = (SimpleTrackOpacity.Value / 100).ToString();
            }
        }

        private void FullTrackOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (FullTrackWindow != null)
            {
                WindowSettings.FullTrackSettings["Opacity"] = (FullTrackOpacity.Value / 100).ToString();
            }
        }
        #endregion

        #region Size
        private void StandingsSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            GlobalSettings.StandingsSettings = new StandingsWindowSettings((int)e.NewValue);
        }

        private void RelativeSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            GlobalSettings.RelativeWindowSettings = new RelativeWindowSettings((int)e.NewValue);
        }

        private void FuelSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            GlobalSettings.FuelWindowSettings = new FuelWindowSettings((int)e.NewValue);
        }

        private void TireSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            GlobalSettings.TireWindowSettings = new TireWindowSettings((int)e.NewValue);
        }

        private void FullTrackSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            GlobalSettings.FullTrackSettings = new FullTrackSettings((int)e.NewValue);
        }
        #endregion

        #region Lock
        private void standingsLock_Click(object sender, RoutedEventArgs e)
        {
            if (StandingsWindow != null)
            {
                standingsLock.Content = StandingsWindow.Locked ? new Image { Source = UnlockedIcon } : new Image { Source = LockedIcon };
                StandingsWindow.Locked = !StandingsWindow.Locked;
                WindowSettings.StandingsSettings["Locked"] = (StandingsWindow.Locked).ToString();
            }
        }

        private void relativeLock_Click(object sender, RoutedEventArgs e)
        {
            if (RelativeWindow != null)
            {
                relativeLock.Content = RelativeWindow.Locked ? new Image { Source = UnlockedIcon } : new Image { Source = LockedIcon };
                RelativeWindow.Locked = !RelativeWindow.Locked;
                WindowSettings.RelativeSettings["Locked"] = (RelativeWindow.Locked).ToString();
            }
        }

        private void fuelLock_Click(object sender, RoutedEventArgs e)
        {
            if (FuelWindow != null)
            {
                fuelLock.Content = FuelWindow.Locked ? new Image { Source = UnlockedIcon } : new Image { Source = LockedIcon };
                FuelWindow.Locked = !FuelWindow.Locked;
                WindowSettings.FuelSettings["Locked"] = (FuelWindow.Locked).ToString();
            }
        }

        private void tiresLock_Click(object sender, RoutedEventArgs e)
        {
            if (TireWindow != null)
            {
                tiresLock.Content = TireWindow.Locked ? new Image { Source = UnlockedIcon } : new Image { Source = LockedIcon };
                TireWindow.Locked = !TireWindow.Locked;
                WindowSettings.TireSettings["Locked"] = (TireWindow.Locked).ToString();
            }
        }

        private void simpleTrackLock_Click(object sender, RoutedEventArgs e)
        {
            if (SimpleTrackWindow != null)
            {
                simpleTrackLock.Content = SimpleTrackWindow.Locked ? new Image { Source = UnlockedIcon } : new Image { Source = LockedIcon };
                SimpleTrackWindow.Locked = !SimpleTrackWindow.Locked;
                WindowSettings.SimpleTrackSettings["Locked"] = (SimpleTrackWindow.Locked).ToString();
            }
        }

        private void fullTrackLock_Click(object sender, RoutedEventArgs e)
        {
            if (FullTrackWindow != null)
            {
                fullTrackLock.Content = FullTrackWindow.Locked ? new Image { Source = UnlockedIcon } : new Image { Source = LockedIcon };
                FullTrackWindow.Locked = !FullTrackWindow.Locked;
                WindowSettings.FullTrackSettings["Locked"] = (FullTrackWindow.Locked).ToString();
            }
        }
        #endregion

        #region Visibility
        private void standingsVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (StandingsWindow != null)
            {
                StandingsWindow?.Dispatcher.Invoke(() =>
                {
                    StandingsWindow.Visibility = StandingsWindow.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                    WindowSettings.StandingsSettings["Visible"] = (StandingsWindow.Visibility == Visibility.Visible).ToString();
                });
                standingsVisibility.Content = StandingsWindow.Visibility == Visibility.Visible ? new Image { Source = VisibleIcon } : new Image { Source = HiddenIcon };
            }
        }

        private void relativeVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (RelativeWindow != null)
            {
                RelativeWindow?.Dispatcher.Invoke(() =>
                {
                    RelativeWindow.Visibility = RelativeWindow.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                    WindowSettings.RelativeSettings["Visible"] = (RelativeWindow.Visibility == Visibility.Visible).ToString();
                });
                relativeVisibility.Content = RelativeWindow.Visibility == Visibility.Visible ? new Image { Source = VisibleIcon } : new Image { Source = HiddenIcon };
            }
        }

        private void fuelVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (FuelWindow != null)
            {
                FuelWindow?.Dispatcher.Invoke(() =>
                {
                    FuelWindow.Visibility = FuelWindow.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                    WindowSettings.FuelSettings["Visible"] = (FuelWindow.Visibility == Visibility.Visible).ToString();
                });
                fuelVisibility.Content = FuelWindow.Visibility == Visibility.Visible ? new Image { Source = VisibleIcon } : new Image { Source = HiddenIcon };
            }
        }

        private void tiresVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (TireWindow != null)
            {
                TireWindow?.Dispatcher.Invoke(() =>
                {
                    TireWindow.Visibility = TireWindow.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                    WindowSettings.TireSettings["Visible"] = (TireWindow.Visibility == Visibility.Visible).ToString();
                });
                tiresVisibility.Content = TireWindow.Visibility == Visibility.Visible ? new Image { Source = VisibleIcon } : new Image { Source = HiddenIcon };
            }
        }

        private void simpleTrackVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (simpleTrackVisibility != null)
            {
                SimpleTrackWindow?.Dispatcher.Invoke(() =>
                {
                    SimpleTrackWindow.Visibility = SimpleTrackWindow.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                    WindowSettings.SimpleTrackSettings["Visible"] = (SimpleTrackWindow.Visibility == Visibility.Visible).ToString();
                });
                simpleTrackVisibility.Content = SimpleTrackWindow.Visibility == Visibility.Visible ? new Image { Source = VisibleIcon } : new Image { Source = HiddenIcon };
            }
        }

        private void fullTrackVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (FullTrackWindow != null)
            {
                FullTrackWindow?.Dispatcher.Invoke(() =>
                {
                    FullTrackWindow.Visibility = FullTrackWindow.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
                    WindowSettings.FullTrackSettings["Visible"] = (FullTrackWindow.Visibility == Visibility.Visible).ToString();
                });
                fullTrackVisibility.Content = FullTrackWindow.Visibility == Visibility.Visible ? new Image { Source = VisibleIcon } : new Image { Source = HiddenIcon };
            }
        }
        #endregion

        #region Reset

        private void standingsReset_Click(object sender, RoutedEventArgs e)
        {
            if (StandingsWindow != null)
            {
                StandingsWindow?.Dispatcher.Invoke(() =>
                {
                    StandingsWindow.Left = 0;
                    StandingsWindow.Top = 0;
                });
                    
            }
        }

        private void relativeReset_Click(object sender, RoutedEventArgs e)
        {
            if (RelativeWindow != null)
            {
                RelativeWindow?.Dispatcher.Invoke(() =>
                {
                    RelativeWindow.Left = 0;
                    RelativeWindow.Top = 0;
                });
            }
        }

        private void fuelReset_Click(object sender, RoutedEventArgs e)
        {
            if (FuelWindow != null)
            {
                FuelWindow?.Dispatcher.Invoke(() =>
                {
                    FuelWindow.Left = 0;
                    FuelWindow.Top = 0;
                });
            }
        }

        private void tiresReset_Click(object sender, RoutedEventArgs e)
        {
            if (TireWindow != null)
            {
                TireWindow?.Dispatcher.Invoke(() =>
                {
                    TireWindow.Left = 0;
                    TireWindow.Top = 0;
                });
            }
        }

        private void simpleTrackReset_Click(object sender, RoutedEventArgs e)
        {
            if (SimpleTrackWindow != null)
            {
                SimpleTrackWindow?.Dispatcher.Invoke(() =>
                {
                    SimpleTrackWindow.Left = 0;
                    SimpleTrackWindow.Top = 0;
                });
            }
        }

        private void fullTrackReset_Click(object sender, RoutedEventArgs e)
        {
            if (FullTrackWindow != null)
            {
                FullTrackWindow?.Dispatcher.Invoke(() =>
                {
                    FullTrackWindow.Left = 0;
                    FullTrackWindow.Top = 0;
                });
            }
        }

        #endregion

        #region Global/Custom

        private void FuelMeasurement_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (FuelWindow != null)
            {
                WindowSettings.FuelSettings["Measurement"] = FuelMeasurement.Value.ToString();
            }
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

        #endregion

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAllWindows();
            Application.Current.Shutdown();
        }

        
    }
}
