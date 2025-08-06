using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RacingOverlay
{
    public class FuelUse
    {
        public FuelUse(int lapNumber, double fuelUsed, double fuelInTank, bool inPit = false)
        {
            LapNumber = lapNumber;
            FuelUsed = fuelUsed;
            FuelInTank = fuelInTank;
            InPit = inPit;
        }

        public int LapNumber { get; set; }
        public double FuelUsed { get; set; }
        public double FuelInTank { get; set; } 
        public bool InPit { get; set; }

        public double CalculateFuelToAdd(double lapsToEnd, double avgFuelUse)
        {
            return (lapsToEnd * avgFuelUse) - FuelInTank;
        }

    }
}
