using iRacingSDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IRacing_Standings
{
    public class Lap
    {
        public int TrackId { get; set; }
        public string TrackName {get; set;}
        public int TrackLength { get; set; }
        public string CarPath { get; set; }
        public int LapNumber { get; set; }
        public double EstLapTime { get; set; }
        public List<Speed> SpeedData { get; set; }
        public bool ValidLap 
        { 
            get
            {
                if (SpeedData != null 
                    && SpeedData.Count >= TrackLength * 0.86
                    && SpeedData.All(s => s.Location == TrackLocation.OnTrack)
                    && SpeedData.OrderBy(s => s.Meter).Last().TimeInSeconds <= EstLapTime * 1.2
                    && SpeedData.OrderBy(s => s.Meter).Last().TimeInSeconds >= EstLapTime
                    && SpeedData.OrderBy(s => s.Meter).Last().Meter >= TrackLength - 20)
                {
                    return true;
                }
                else { return false; }
            }
        }

        public static List<Speed> GetSpeedData(int trackId, string carPath)
        {
            List<Speed> speedData = null;
            var trackDirectory = Directory.GetDirectories($"..\\..\\Speed Files", $"{trackId}-*").FirstOrDefault();
            if (trackDirectory != null)
            {
                var speedFile = $"{trackDirectory}\\{carPath}.json";
                if (File.Exists(speedFile))
                {
                    var speedJsonString = File.ReadAllText(speedFile);
                    speedData = JsonSerializer.Deserialize<List<Speed>>(speedJsonString);
                }
            }
           
            return speedData;
        }

        public void FillMissingSpeedData()
        {
            var lastDataPoint = SpeedData[0];
            for (int i = 0; i <= TrackLength; i++)
            {
                if (!SpeedData.Select(s => s.Meter).Contains(i))
                {
                    SpeedData.Add(new Speed
                    {
                        Meter = i,
                        SpeedMS = lastDataPoint.SpeedMS,
                        TimeInSeconds = lastDataPoint.TimeInSeconds,
                        Location = TrackLocation.OnTrack
                    });
                }
                lastDataPoint = SpeedData.Where(s => s.Meter == i).First();
            }
            SpeedData = SpeedData.OrderBy(s => s.Meter).ToList();
        }

        //Save fastest lap from session if it is faster than the current recorded file's lap
        public void CheckFastestLap(List<Speed> savedSpeedData)
        {
            //var fastestLap = false;
            if (savedSpeedData == null || savedSpeedData.Count == 0 || savedSpeedData.OrderBy(s => s.Meter).Last().TimeInSeconds > SpeedData.OrderBy(s => s.Meter).Last().TimeInSeconds)
            {
                //fastestLap = true;
                SaveLap();
            }
        }

        public void SaveLap()
        {
            var parentDirectory = $"..\\..\\Speed Files\\{TrackId}-{TrackName.Replace(" ", "").ToLower()}";
            var fileLocation = $"{parentDirectory}\\{CarPath}.json";
            if (!Directory.Exists(parentDirectory))
            {
                Directory.CreateDirectory(parentDirectory);
            }

            FillMissingSpeedData();
            using (StreamWriter writer = new StreamWriter(fileLocation))
            {
                var options = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };
                var timesJson = JsonSerializer.Serialize(SpeedData, options);
                writer.Write(timesJson);
            }
        }
    }
}
