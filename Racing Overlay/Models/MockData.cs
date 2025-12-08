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

        private static List<string> FirstNames => new List<string>{ "Brian", "Max", "Esteban", "Carlos", "Lando", "Fernando", "Oliver", "Charles", "Jarno", "James", "Oscar", "Lewis", "George", "Kimi", "Valtteri" };

        private static List<string> LastNames => new List<string> { "Cox", "Verstappen", "Ocon", "Norris", "Sainz", "Albon", "Broadbent", "Opmeer", "Leclerc", "Piastri", "Bearman" , "Hamilton", "Russell", "Antonelli", "Alonso", "Bottas" };

        private static Dictionary<string, Tuple<string, List<string>>> CarClasses => new Dictionary<string, Tuple<string, List<string>>>
        {
            { "GTP", new Tuple<string, List<string>>("#e0aa01", new List<string> { "Cadillac", "Ferrari", "BMW", "Porsche", "Acura" }) },
            { "LMP2", new Tuple<string, List<string>>("#6f94c7", new List<string> { "Dallara" }) },
            { "GT3", new Tuple<string, List<string>>("#C32148", new List<string> { "Ferrari", "BMW", "Porsche", "McLaren", "Mercedes", "Audi", "Acura", "Ford", "Lamborghini", "Aston Martin" }) },
            { "Porsche Cup", new Tuple<string, List<string>>("#3dc068", new List<string> { "Porsche" }) },
            { "GT4", new Tuple<string, List<string>>("#999999", new List<string> { "BMW", "Porsche", "McLaren", "Mercedes", "Ford", "Aston Martin" }) },
        };

        private static Dictionary<string, string> Licenses => new Dictionary<string, string>
        {
            { "A", "#284d93" },
            { "B", "#269d00" },
            { "C", "#e3b600" },
            { "D", "#d05501" },
            { "R", "#b52526" },
        };

        public static List<Driver> MockDrivers
        {
            get
            {
                var drivers = new List<Driver>();
                var totalClasses = random.Next(2, 6);
                var carId = 0;

                for (int i = 0; i < totalClasses; i++)
                {
                    var randomClassSize = random.Next(1, 11);
                    for (int j = 0; j < randomClassSize; j++)
                    {
                        var chosenName = $"{FirstNames[random.Next(FirstNames.Count)]} {LastNames[random.Next(LastNames.Count)]}";
                        var chosenSafetyRating = Licenses.ElementAt(random.Next(Licenses.Count));
                        var classDiff = 0.1;

                        drivers.Add(new Driver()
                        {
                            CarId = carId,
                            CarNumber = random.Next(1, 500).ToString(),
                            FastestLap = ((float)random.Next((int)(74000 * (1 + classDiff * i)), (int)(77000 * (1 + classDiff * i)))) / 1000F,
                            LastLap = random.Next(74500, 77000) / 1000F,
                            OverallPosition = carId,
                            ClassPosition = j,
                            ClassId = i,
                            ClassName = CarClasses.ElementAt(i).Key,
                            ClassColor = CarClasses.ElementAt(i).Value.Item1,
                            LapsComplete = 13,
                            Name = chosenName,
                            SafetyRating = new Tuple<string, string>(chosenSafetyRating.Key, string.Format("{0:#.#0}", Math.Round((float)random.Next(100, 400) / 100, 2))),
                            SafetyRatingColor = chosenSafetyRating.Value,
                            iRating = random.Next(500, 5000),
                            CarPath = CarClasses.ElementAt(i).Value.Item2[random.Next(CarClasses.ElementAt(i).Value.Item2.Count - 1)],
                            InPit = false,
                            Location = TrackLocation.OnTrack,
                            LapChangeTime = DateTime.UtcNow
                        });

                        carId++;
                    }
                }
                return drivers;
            }
        }
        
    }
}