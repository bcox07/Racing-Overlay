namespace RacingOverlay
{
    public struct FuelUse
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
            if (avgFuelUse < 0)
                return -1.0;

            return (lapsToEnd * avgFuelUse) - FuelInTank;
        }

    }
}
