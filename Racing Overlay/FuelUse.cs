using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRacing_Standings
{
    public class FuelUse
    {
        public FuelUse(int lapNumber, double fuelUsed, bool inPit = false)
        {
            LapNumber = lapNumber;
            FuelUsed = fuelUsed;
            InPit = inPit;
        }

        public int LapNumber { get; set; }
        public double FuelUsed { get; set; }
        public bool InPit { get; set; }

    }
}
