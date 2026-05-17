using RacingOverlay.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RacingOverlay
{
    /// <summary>
    /// Interaction logic for TireWindow.xaml
    /// </summary>
    public partial class TireWindow : Window
    {
        TelemetryData LocalTelemetry;
        public bool Locked = false;
        public TireWindow(TelemetryData telemetryData, GlobalSettings globalSettings, WindowSettings settings)
        {
            LocalTelemetry = telemetryData;
            InitializeComponent();

            Opacity = double.Parse(settings.TireSettings["Opacity"]);
            Locked = bool.Parse(settings.TireSettings["Locked"] ?? "false");
            Left = double.Parse(settings.TireSettings["XPos"] ?? "0");
            Top = double.Parse(settings.TireSettings["YPos"] ?? "0");
            Width = globalSettings.TireWindowSettings.WindowWidth;
            Height = globalSettings.TireWindowSettings.WindowWidth * 1.571428;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!Locked)
            {
                base.OnMouseLeftButtonDown(e);
                DragMove();
            }
        }

        public void UpdateTelemetryData(TelemetryData telemetryData, GlobalSettings globalSettings, WindowSettings settings)
        {
            Opacity = double.Parse(settings.TireSettings["Opacity"]);
            Width = globalSettings.TireWindowSettings.WindowWidth;
            Height = globalSettings.TireWindowSettings.WindowWidth * 1.571428;
            LFTempBorder.CornerRadius = new CornerRadius(Width * 0.09375);
            RFTempBorder.CornerRadius = new CornerRadius(Width * 0.09375);
            LRTempBorder.CornerRadius = new CornerRadius(Width * 0.09375);
            RRTempBorder.CornerRadius = new CornerRadius(Width * 0.09375);
            LFTemp.FontSize = globalSettings.TireWindowSettings.DataFontSize;
            RFTemp.FontSize = globalSettings.TireWindowSettings.DataFontSize;
            LRTemp.FontSize = globalSettings.TireWindowSettings.DataFontSize;
            RRTemp.FontSize = globalSettings.TireWindowSettings.DataFontSize;
            LFWear.FontSize = globalSettings.TireWindowSettings.DataFontSize;
            RFWear.FontSize = globalSettings.TireWindowSettings.DataFontSize;
            LRWear.FontSize = globalSettings.TireWindowSettings.DataFontSize;
            RRWear.FontSize = globalSettings.TireWindowSettings.DataFontSize;
            if (DateTime.UtcNow.Second % 10 == 0)
            {
                Dispatcher.Invoke(() =>
                {
                    Topmost = false;
                    Topmost = true;
                });
            }

            LocalTelemetry = telemetryData;

            if (LocalTelemetry != null && LocalTelemetry.IsReady)
            {
                DisplayTireData();
            }
        }

        private void DisplayTireData()
        {
            var lFTireData = new TireData();
            var rFTireData = new TireData();
            var lRTireData = new TireData();
            var rRTireData = new TireData();

            lFTireData.Temp = new Tuple<double, double, double>(LocalTelemetry.FeedTelemetry.LFtempCL, LocalTelemetry.FeedTelemetry.LFtempCM, LocalTelemetry.FeedTelemetry.LFtempCR);
            lFTireData.Wear = new Tuple<double, double, double>(LocalTelemetry.FeedTelemetry.LFwearL * 100, LocalTelemetry.FeedTelemetry.LFwearM * 100, LocalTelemetry.FeedTelemetry.LFwearR * 100);

            rFTireData.Temp = new Tuple<double, double, double>(LocalTelemetry.FeedTelemetry.RFtempCL, LocalTelemetry.FeedTelemetry.RFtempCM, LocalTelemetry.FeedTelemetry.RFtempCR);
            rFTireData.Wear = new Tuple<double, double, double>(LocalTelemetry.FeedTelemetry.RFwearL * 100, LocalTelemetry.FeedTelemetry.RFwearM * 100, LocalTelemetry.FeedTelemetry.RFwearR * 100);

            lRTireData.Temp = new Tuple<double, double, double>(LocalTelemetry.FeedTelemetry.LRtempCL, LocalTelemetry.FeedTelemetry.LRtempCM, LocalTelemetry.FeedTelemetry.LRtempCR);
            lRTireData.Wear = new Tuple<double, double, double>(LocalTelemetry.FeedTelemetry.LRwearL * 100, LocalTelemetry.FeedTelemetry.LRwearM * 100, LocalTelemetry.FeedTelemetry.LRwearR * 100);

            rRTireData.Temp = new Tuple<double, double, double>(LocalTelemetry.FeedTelemetry.RRtempCL, LocalTelemetry.FeedTelemetry.RRtempCM, LocalTelemetry.FeedTelemetry.RRtempCR);
            rRTireData.Wear = new Tuple<double, double, double>(LocalTelemetry.FeedTelemetry.RRwearL * 100, LocalTelemetry.FeedTelemetry.RRwearM * 100, LocalTelemetry.FeedTelemetry.RRwearR * 100);

            Dispatcher.Invoke(() =>
            {
                LFTemp.Text = $"{lFTireData.Temp.Item2:N1}°";
                LFWear.Text = $"{lFTireData.Wear.Item2:N0}%";
                SetTireColor(LFTempBorder, lFTireData);
                RFTemp.Text = $"{rFTireData.Temp.Item2:N1}°";
                RFWear.Text = $"{rFTireData.Wear.Item2:N0}%";
                SetTireColor(RFTempBorder, rFTireData);
                LRTemp.Text = $"{lRTireData.Temp.Item2:N1}°";
                LRWear.Text = $"{lRTireData.Wear.Item2:N0}%";
                SetTireColor(LRTempBorder, lRTireData);
                RRTemp.Text = $"{rRTireData.Temp.Item2:N1}°";
                RRWear.Text = $"{rRTireData.Wear.Item2:N0}%";
                SetTireColor(RRTempBorder, rRTireData);
            });
        }

        private void SetTireColor(Border tireBox, TireData tireData)
        {
            string leftColor = SetSectionColor(tireData.Temp.Item1);
            string middleColor = SetSectionColor(tireData.Temp.Item2);
            string rightColor = SetSectionColor(tireData.Temp.Item3);


            var gradientStopCollection = new GradientStopCollection
            {
                new GradientStop((Color)ColorConverter.ConvertFromString(leftColor), 0),
                new GradientStop((Color)ColorConverter.ConvertFromString(middleColor), 0.3),
                new GradientStop((Color)ColorConverter.ConvertFromString(middleColor), 0.5),
                new GradientStop((Color)ColorConverter.ConvertFromString(middleColor), 0.7),
                new GradientStop((Color)ColorConverter.ConvertFromString(rightColor), 1)
            };
            var gradient = new System.Windows.Media.LinearGradientBrush(gradientStopCollection, 0);

            tireBox.Background = gradient;
        }

        private string SetSectionColor(double temp)
        {
            if (temp < 70)
                return "#35a5f2";

            if (temp < 95)
                return "#2cd129";

            if (temp < 105)
                return "#e0ec21";

            return "#e02e2e";
        }
    }

    class TireData
    {
        public TireData() { }

        public Tuple<double, double, double> Temp { get; set; }
        public Tuple<double, double, double> Wear { get; set; }
    }
}
