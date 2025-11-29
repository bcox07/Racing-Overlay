using System;

namespace RacingOverlay
{
    public class Driver
    {
        public int CarId { get; set; }
        public string CarPath { get; set; }
        public string CarNumber { get; set; }
        public int ClassId { get; set; }
        public string ClassColor { get; set; }
        public int? ClassPosition { get; set; }
        public int? OverallPosition { get; set; }
        public string Name { get; set; }
        public int iRating { get; set; }
        public Tuple<string, string> SafetyRating { get; set; }
        public double PosOnTrack { get; set; }
        public double Distance { get; set; }
        public double Delta { get; set; }
        public double TimeBehindLeader { get; set; }
        public double? FastestLap { get; set; }
        public double? FastestLapDelta { get; set; }
        public double? LastLap { get; set; }
        public int? LapsComplete { get; set; }
        //public Dictionary<int, double> LapHistory { get; set; }  
        public DateTime LapChangeTime { get; set; }
        public bool InPit { get; set; }
    }
}
