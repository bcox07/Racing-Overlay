using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
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
    /// Interaction logic for TireWindow.xaml
    /// </summary>
    public partial class TireWindow : Window
    {
        TelemetryData LocalTelemetry;
        public TireWindow(TelemetryData telemetryData)
        {
            LocalTelemetry = telemetryData;
            InitializeComponent();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            //if (!Locked)
            //{
                base.OnMouseLeftButtonDown(e);
                DragMove();
            //}
        }

        public void UpdateTelemetryData(TelemetryData telemetryData)
        {
            Dispatcher.Invoke(() =>
            {
                Topmost = false;
                Topmost = true;
            });
            
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
            lFTireData.Wear = new Tuple<double, double, double>(LocalTelemetry.FeedTelemetry.LFwearL, LocalTelemetry.FeedTelemetry.LFwearM, LocalTelemetry.FeedTelemetry.LFwearR);

            rFTireData.Temp = new Tuple<double, double, double>(LocalTelemetry.FeedTelemetry.RFtempCL, LocalTelemetry.FeedTelemetry.RFtempCM, LocalTelemetry.FeedTelemetry.RFtempCR);
            rFTireData.Wear = new Tuple<double, double, double>(LocalTelemetry.FeedTelemetry.RFwearL, LocalTelemetry.FeedTelemetry.RFwearM, LocalTelemetry.FeedTelemetry.RFwearR);

            lRTireData.Temp = new Tuple<double, double, double>(LocalTelemetry.FeedTelemetry.LRtempCL, LocalTelemetry.FeedTelemetry.LRtempCM, LocalTelemetry.FeedTelemetry.LRtempCR);
            lRTireData.Wear = new Tuple<double, double, double>(LocalTelemetry.FeedTelemetry.LRwearL, LocalTelemetry.FeedTelemetry.LRwearM, LocalTelemetry.FeedTelemetry.LRwearR);

            rRTireData.Temp = new Tuple<double, double, double>(LocalTelemetry.FeedTelemetry.RRtempCL, LocalTelemetry.FeedTelemetry.RRtempCM, LocalTelemetry.FeedTelemetry.RRtempCR);
            rRTireData.Wear = new Tuple<double, double, double>(LocalTelemetry.FeedTelemetry.RRwearL, LocalTelemetry.FeedTelemetry.RRwearM, LocalTelemetry.FeedTelemetry.RRwearR);

            Dispatcher.Invoke(() =>
            {
                this.LFTireBox.Text = lFTireData.Temp.Item2.ToString("N2");
                SetTireColor(this.LFTireBox, lFTireData);
                this.RFTireBox.Text = rFTireData.Temp.Item2.ToString("N2");
                SetTireColor(this.RFTireBox, rFTireData);
                this.LRTireBox.Text = lRTireData.Temp.Item2.ToString("N2");
                SetTireColor(this.LRTireBox, lRTireData);
                this.RRTireBox.Text = rRTireData.Temp.Item2.ToString("N2");
                SetTireColor(this.RRTireBox, rRTireData);
            });
        }
        
        private void SetTireColor(TextBlock tireBox, TireData tireData)
        {
            string leftColor;
            if (tireData.Temp.Item1 < 70)
            {
                leftColor = "#35a5f2";
            }
            else if (tireData.Temp.Item1 < 95)
            {
                leftColor = "#2cd129";
            }
            else if (tireData.Temp.Item1 < 105)
            {
                leftColor = "#e0ec21";
            }
            else
            {
                leftColor = "#e02e2e";
            }

            string middleColor;
            if (tireData.Temp.Item2 < 70)
            {
                middleColor = "#35a5f2";
            }
            else if (tireData.Temp.Item2 < 95)
            {
                middleColor = "#2cd129";
            }
            else if (tireData.Temp.Item2 < 105)
            {
                middleColor = "#e0ec21";
            }
            else
            {
                middleColor = "#e02e2e";
            }

            string rightColor;
            if (tireData.Temp.Item3 < 70)
            {
                rightColor = "#35a5f2";
            }
            else if (tireData.Temp.Item3 < 95)
            {
                rightColor = "#2cd129";
            }
            else if (tireData.Temp.Item3 < 105)
            {
                rightColor = "#e0ec21";
            }
            else
            {
                rightColor = "#e02e2e";
            }


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
    }

    class TireData
    {
        public TireData() { }

        public Tuple<double, double, double> Temp { get; set; }
        public Tuple<double, double, double> Wear { get; set; }
    }
}
