using iRacingSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RacingOverlay.Models
{
    public class MockData
    {
        private static Random random = new Random();
        public static List<Driver> MockDrivers => new List<Driver>
                {
                    new Driver
                    {
                        CarId = 0,
                        CarNumber = random.Next(1, 500).ToString(),
                        FastestLap = ((float)random.Next(74000, 77000)) / 1000F,
                        LastLap = random.Next(74500, 77000) / 1000F,
                        OverallPosition = 0,
                        ClassPosition = 0,
                        ClassId = 1,
                        ClassColor = "#555555",
                        LapsComplete = 13,
                        Name = "Brinkle McCrinkle",
                        SafetyRating = new Tuple<string, string>("A", "3.55"),
                        iRating = random.Next(500, 5000),
                        CarPath = "Cadillac",
                        InPit = false,
                        Location = TrackLocation.OnTrack,
                        LapChangeTime = DateTime.UtcNow
                    },
                    new Driver
                    {
                        CarId = 1,
                        CarNumber = random.Next(1, 500).ToString(),
                        FastestLap = ((float)random.Next(74000, 77000)) / 1000F,
                        LastLap = random.Next(74500, 77000) / 1000F,
                        OverallPosition = 1,
                        ClassPosition = 1,
                        ClassColor = "#555555",
                        ClassId = 1,
                        LapsComplete = 13,
                        Name = "Brinkle McCrinkle2",
                        SafetyRating = new Tuple<string, string>("B", "2.78"),
                        iRating = random.Next(500, 5000),
                        CarPath = "Acura",
                        InPit = false,
                        Location = TrackLocation.OnTrack,
                        LapChangeTime = DateTime.UtcNow
                    },
                    new Driver
                    {
                        CarId = 2,
                        CarNumber = random.Next(1, 500).ToString(),
                        FastestLap = ((float)random.Next(74000, 77000)) / 1000F,
                        LastLap = random.Next(74500, 77000) / 1000F,
                        OverallPosition = 2,
                        ClassPosition = 2,
                        ClassId = 1,
                        ClassColor = "#555555",
                        LapsComplete = 13,
                        Name = "Brinkle McCrinkle3",
                        SafetyRating = new Tuple<string, string>("B", "3.99"),
                        iRating = random.Next(500, 5000),
                        CarPath = "Porsche",
                        InPit = false,
                        Location = TrackLocation.OnTrack,
                        LapChangeTime = DateTime.UtcNow
                    },
                    new Driver
                    {
                        CarId = 3,
                        CarNumber = random.Next(1, 500).ToString(),
                        FastestLap = ((float)random.Next(74000, 77000)) / 1000F,
                        LastLap = random.Next(74500, 77000) / 1000F,
                        OverallPosition = 3,
                        ClassPosition = 3,
                        ClassId = 1,
                        ClassColor = "#555555",
                        LapsComplete = 13,
                        Name = "Brinkle McCrinkle4",
                        SafetyRating = new Tuple<string, string>("B", "3.99"),
                        iRating = random.Next(500, 5000),
                        CarPath = "Porsche",
                        InPit = false,
                        Location = TrackLocation.OnTrack,
                        LapChangeTime = DateTime.UtcNow
                    },
                    new Driver
                    {
                        CarId = 4,
                        CarNumber = random.Next(1, 500).ToString(),
                        FastestLap = ((float)random.Next(74000, 77000)) / 1000F,
                        LastLap = random.Next(74500, 77000) / 1000F,
                        OverallPosition = 4,
                        ClassPosition = 4,
                        ClassId = 1,
                        ClassColor = "#555555",
                        LapsComplete = 13,
                        Name = "Brinkle McCrinkle5",
                        SafetyRating = new Tuple<string, string>("B", "3.99"),
                        iRating = random.Next(500, 5000),
                        CarPath = "Porsche",
                        InPit = false,
                        Location = TrackLocation.OnTrack,
                        LapChangeTime = DateTime.UtcNow
                    },
                    new Driver
                    {
                        CarId = 5,
                        CarNumber = random.Next(1, 500).ToString(),
                        FastestLap = ((float)random.Next(74000, 77000)) / 1000F,
                        LastLap = random.Next(74500, 77000) / 1000F,
                        OverallPosition = 5,
                        ClassPosition = 5,
                        ClassId = 1,
                        ClassColor = "#555555",
                        LapsComplete = 13,
                        Name = "Brinkle McCrinkle6",
                        SafetyRating = new Tuple<string, string>("B", "3.99"),
                        iRating = random.Next(500, 5000),
                        CarPath = "Porsche",
                        InPit = false,
                        Location = TrackLocation.OnTrack,
                        LapChangeTime = DateTime.UtcNow
                    },
                    new Driver
                    {
                        CarId = 6,
                        CarNumber = random.Next(1, 500).ToString(),
                        FastestLap = random.Next(81000, 84000) / 1000F,
                        LastLap = random.Next(81500, 84000) / 1000F,
                        OverallPosition = 6,
                        ClassPosition = 0,
                        ClassId = 2,
                        ClassColor = "#FFFFFF",
                        LapsComplete = 13,
                        Name = "Brinkle McCrinkle7",
                        SafetyRating = new Tuple<string, string>("C", "2.78"),
                        iRating = new Random().Next(500, 5000),
                        CarPath = "BMW",
                        InPit = false,
                        Location = TrackLocation.OnTrack,
                        LapChangeTime = DateTime.UtcNow
                    },
                    new Driver
                    {
                        CarId = 7,
                        CarNumber = random.Next(1, 500).ToString(),
                        FastestLap = random.Next(81000, 84000) / 1000F,
                        LastLap = random.Next(81500, 84000) / 1000F,
                        OverallPosition = 6,
                        ClassPosition = 1,
                        ClassId = 2,
                        ClassColor = "#FFFFFF",
                        LapsComplete = 13,
                        Name = "Brinkle McCrinkle8",
                        SafetyRating = new Tuple<string, string>("B", "1.95"),
                        iRating = random.Next(500, 5000),
                        CarPath = "Ferrari",
                        InPit = false,
                        Location = TrackLocation.OnTrack,
                        LapChangeTime = DateTime.UtcNow
                    },
                };
    }
}