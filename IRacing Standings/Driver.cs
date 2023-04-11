using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRacing_Standings
{
    public class Driver
    {
        public int CarId { get; set; }
        public string CarPath { get; set; }
        public int ClassId { get; set; }
        public string ClassColor { get; set; }
        public int ClassPosition { get; set; }
        public int OverallPosition { get; set; }
        public string Name { get; set; }
        public int iRating { get; set; }
        public string SafetyRating { get; set; }
        public double PosOnTrack { get; set; }
        public double Distance { get; set; }
        public double Delta { get; set; }
        public double TimeBehindLeader { get; set; }
        public TimeSpan FastestLap { get; set; }
        public double FastestLapDelta { get; set; }
        public TimeSpan LastLap { get; set; }
        public int LapsComplete { get; set; }
        public double SecondsSinceLastLap { get; set; }
        public bool InPit { get; set; }
    }
}
