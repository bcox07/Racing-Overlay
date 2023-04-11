using iRacingSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRacing_Standings
{
    public class Lap
    {
        public int TrackId { get; set; }
        public int TrackLength { get; set; }
        public string CarPath { get; set; }
        public int LapNumber { get; set; }
        public double EstLapTime { get; set; }
        public Dictionary<int, Tuple<double, double, TrackLocation>> SpeedData { get; set; }
        public bool ValidLap 
        { 
            get
            {
                if (SpeedData != null 
                    && SpeedData.Count >= TrackLength * 0.92
                    && SpeedData.All(s => s.Value.Item3 == TrackLocation.OnTrack)
                    && SpeedData.OrderBy(s => s.Key).Last().Value.Item2 <= EstLapTime * 1.07
                    && SpeedData.OrderBy(s => s.Key).Last().Value.Item2 >= EstLapTime
                    && SpeedData.OrderBy(s => s.Key).Last().Key >= TrackLength - 3)
                {
                    return true;
                }
                else { return false; }
            }
        }
    }
}
