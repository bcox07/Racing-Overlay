using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using iRacingSDK;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Thread operation;
        public MainWindow()
        {
            InitializeComponent();
            Setup();
        }


        private static void Test()
        {
            var iracing = new iRacingConnection();

            //iracing.Replay.MoveToStartOfRace();
            //iracing.Replay.SetSpeed(16);

            foreach (var data in iRacing.GetDataFeed().AtSpeed(16))
            {
                string carPositionString = data.Telemetry.SessionTime.ToString();


                for (int i = 0; i<64; i++)
                {
                    carPositionString += "\t";
                    carPositionString += "Car: " + i.ToString();
                    carPositionString += "\t" + "Pos: ";
                    carPositionString += data.Telemetry.CarIdxClassPosition[i].ToString();
                }
                Trace.WriteLine(carPositionString);


                //Trace.WriteIf("Lap: {] \t".
                //Trace.WriteLine("Session State: {0}".F(data.Telemetry.SessionState));
                //Trace.WriteLine("Session Flags: {0}".F(data.Telemetry.SessionFlags));

                //Trace.WriteLine("Pace Car Location: {0}".F(data.Telemetry.CarIdxTrackSurface[0]));
                //Trace.WriteLine("Under pace car: {0}".F(data.Telemetry.UnderPaceCar));

                //Trace.WriteLine("Position:{0}".F(data.Telemetry.PlayerCarPosition));

                //Trace.WriteLine("Session Flags: {0}".F(data.Telemetry.SessionFlags.ToString()));
                //Trace.WriteLine("Engine Warnings: {0}".F(data.Telemetry.EngineWarnings.ToString()));

                //Trace.WriteLine("\n\n");
                Thread.Sleep(2000);
            }
        }

        public void Setup()
        {
            iRacing.NewData += iRacing_NewData;
            iRacing.StartListening();

            Trace.WriteLine(iRacing.GetDataFeed().First().SessionData.DriverInfo);
        }

        void iRacing_NewData(DataSample data)
        {
            var tractionControl = data.Telemetry.dcTractionControl;
            Trace.WriteLine($"TRACTION CONTROL: {tractionControl}");
        }

    }
}
