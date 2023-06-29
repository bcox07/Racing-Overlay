using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IRacing_Standings.Windows
{
    /// <summary>
    /// Interaction logic for LiveTrackWindow.xaml
    /// </summary>
    public partial class LiveTrackWindow : Window
    {
        TelemetryData LocalTelemetry;
        public LiveTrackWindow(TelemetryData telemetryData)
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
                DisplayTrackMap();
            }
        }

        private void DisplayTrackMap()
        {
            var speedJsonString = File.ReadAllText("..\\..\\TrackMaps\\roadamerica\\roadamerica.json");
            var trackData = JsonSerializer.Deserialize<Dictionary<string, List<int>>>(speedJsonString);
            
            var location = (int)(LocalTelemetry.FeedTelemetry.CarIdxLapDistPct[LocalTelemetry.FeedSessionData.DriverInfo.DriverCarIdx] * LocalTelemetry.TrackLength);
            var closestPosition = 0;
            if (location % 10 != 0)
            {
                closestPosition = location - location % 10;
            }
            var value = new List<int> { 0, 0 };
            var test = trackData.TryGetValue(closestPosition.ToString(), out value);
            var marginLeft = value?[0] ?? 0;
            var marginTop = value?[1] ?? 0;
            Dispatcher.Invoke(() =>
            {
                this.Player.Margin = new Thickness(marginLeft, marginTop, 0, 0);
            });
        }
    }
}
