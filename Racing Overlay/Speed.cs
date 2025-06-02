using iRacingSDK;
using System.Text.Json.Serialization;

namespace IRacing_Standings
{
    public class Speed
    {
        public int Meter { get; set; }
        public double SpeedMS { get; set; }
        public double TimeInSeconds { get; set; }

        [JsonIgnore]
        public TrackLocation Location { get; set; }
    }
}
