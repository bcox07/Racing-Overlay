using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IRacing_Standings
{
    public class Speed
    {
        [JsonIgnore]
        public int TrackId { get; set; }

        [JsonIgnore]
        public string CarPath { get; set; }

        [JsonPropertyName("Speed")]
        public Dictionary<string, double[]> SpeedStringDictionary { get; set; }

        [JsonIgnore]
        public SortedDictionary<int, double[]> SpeedIntDictionary
        {
            get
            {
                var timeIntDictionary = new SortedDictionary<int, double[]>();
                if (SpeedStringDictionary != null)
                {
                    timeIntDictionary = new SortedDictionary<int, double[]>(SpeedStringDictionary.ToDictionary(t => int.Parse(t.Key), t => t.Value));
                }
                return timeIntDictionary;
            } 
        }

        public static void SaveSpeedToFile(Speed speedData)
        {
            var parentDirectory = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Speed Files\\{speedData.TrackId}";
            if (!Directory.Exists(parentDirectory))
            {
                Directory.CreateDirectory(parentDirectory);
            }
            var directory = $"{parentDirectory}\\{speedData.CarPath}.json";
            speedData.SpeedStringDictionary = speedData.SpeedIntDictionary.ToDictionary(s => s.Key.ToString(), s => s.Value);
            
            using (StreamWriter writer = new StreamWriter(directory))
            {
                var options = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };
                var timesJson = JsonSerializer.Serialize(speedData, options);
                writer.Write(timesJson);
            }
        }

        public static Speed GetSpeedData(int trackId, string carPath)
        {
            Speed speed = null;
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $"\\Speed Files\\{trackId}\\{carPath}.json";
            if (File.Exists(directory))
            {
                var timesJsonString = File.ReadAllText(directory);
                speed = JsonSerializer.Deserialize<Speed>(timesJsonString);
                speed.TrackId = trackId;
                speed.CarPath = carPath;
            }
            return speed;
        }
    }
}
