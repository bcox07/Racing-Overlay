using iRacingSDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using NLog;

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

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public bool LapLengthValid
        {
            get
            {
                var speedDataCopy = new List<Speed>(SpeedData);
                if (speedDataCopy != null && speedDataCopy.Count >= TrackLength  * 0.80)
                {
                    return true;
                }
                else { return false; }
            }
        }

        public bool OnTrack
        {
            get
            {
                var speedDataCopy = new List<Speed>(SpeedData);
                if (speedDataCopy != null && speedDataCopy.Count(s => s.Location != TrackLocation.OnTrack) < 10)
                {
                    return true;
                }
                else { return false; }
            }
        }

        public bool LapTimeReasonable
        {
            get
            {
                var speedDataCopy = new List<Speed>(SpeedData);
                if (speedDataCopy != null && speedDataCopy.OrderBy(s => s.Meter).Last().TimeInSeconds <= EstLapTime * 1.2 && speedDataCopy.OrderBy(s => s.Meter).Last().TimeInSeconds >= EstLapTime * 0.95)
                {
                    return true;
                }
                else { return false; }
            }
        }
        public bool ValidLap 
        { 
            get
            {
                try
                {
                    var speedDataCopy = new List<Speed>(SpeedData);
                    if (LapLengthValid && OnTrack && LapTimeReasonable && speedDataCopy.OrderBy(s => s.Meter).Last().Meter >= TrackLength - 20)
                    {
                        return true;
                    }
                    else { return false; }
                }
                catch (Exception ex) 
                {
                    Logger.Error(ex);
                    return false;
                }
            }
        }

        public static List<Speed> GetSpeedData(int trackId, string carPath)
        {
            List<Speed> speedData = null;
            var trackDirectory = Directory.GetDirectories($"..\\..\\SpeedFiles", $"{trackId}-*").FirstOrDefault();
            if (trackDirectory != null)
            {
                var speedFile = $"{trackDirectory}\\{carPath}.csv";
                if (File.Exists(speedFile))
                {
                    try
                    {
                        using (var reader = new StreamReader(speedFile))
                        {
                            speedData = new List<Speed>();
                            string line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                var data = line.Split(',');
                                speedData.Add(new Speed
                                {
                                    Meter = int.Parse(data[0]),
                                    SpeedMS = double.Parse(data[1]),
                                    TimeInSeconds = double.Parse(data[2]),
                                });
                            }
                        }
                    }
                    catch (IOException ex)
                    {
                        Logger.Error(ex);
                        Trace.WriteLine(ex);
                    }
                }
            }
           
            return speedData;
        }

        public void FillMissingSpeedData()
        {
            var speedDataCopy = new List<Speed>(SpeedData);
            var lastDataPoint = speedDataCopy[0];
            for (int i = 0; i <= TrackLength; i++)
            {
                if (!speedDataCopy.Select(s => s.Meter).Contains(i))
                {
                    speedDataCopy.Add(new Speed
                    {
                        Meter = i,
                        SpeedMS = lastDataPoint.SpeedMS,
                        TimeInSeconds = lastDataPoint.TimeInSeconds,
                        Location = TrackLocation.OnTrack
                    });
                }
                lastDataPoint = speedDataCopy.Where(s => s.Meter == i).First();
            }
            SpeedData = speedDataCopy.OrderBy(s => s.Meter).ToList();
        }

        //Save fastest lap from session if it is faster than the current recorded file's lap
        public bool CheckFastestLapExists(List<Speed> savedSpeedData)
        {
            if (savedSpeedData == null || savedSpeedData.Count == 0 || savedSpeedData.OrderBy(s => s.Meter).Last().TimeInSeconds > SpeedData.OrderBy(s => s.Meter).Last().TimeInSeconds)
            {
                return false;
            }
            return true;
        }

        public void SaveLap()
        {
            var parentDirectory = $"..\\..\\SpeedFiles\\{TrackId}-{TrackName.Replace(" ", "").ToLower()}";
            var fileLocation = $"{parentDirectory}\\{CarPath}.json";
            if (!Directory.Exists(parentDirectory))
            {
                Directory.CreateDirectory(parentDirectory);
            }

            FillMissingSpeedData();

            if (!IsFileLocked(fileLocation))
            {
                using (StreamWriter writer = new StreamWriter(fileLocation))
                {
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    var timesJson = JsonSerializer.Serialize(SpeedData, options);
                    writer.Write(timesJson);
                }

                Logger.Info($"New Fastest Lap Recorded for {CarPath} at {TrackName} : {SpeedData.Last().TimeInSeconds}");
            }
        }

        private static bool IsFileLocked(string fileLocation)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(fileLocation))
                {
                    //Use this for only detecting IO Exception
                }
            }
            catch (IOException)
            {
                return true;
            }
            return false;
        }

    }
}
