using iRacingSDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Timers;
using static System.Net.Mime.MediaTypeNames;

namespace IRacing_Standings
{
    public class TelemetryData
    {
        Thread thread;
        public bool IsConnected { get; set; }
        public DataSample _DataSample { get; set; }
        public TelemetryData LastSample { get; set; }
        private Telemetry _feedTelemetry;
        public Telemetry FeedTelemetry 
        { 
            get
            {
                if (_DataSample != null && _DataSample.IsConnected && _DataSample.Telemetry != null)
                {
                    _feedTelemetry = _DataSample.Telemetry;
                }
                return _feedTelemetry;
            }
            set
            {
                _feedTelemetry = value;
            }

        }
        private SessionData _feedSessionData;
        public SessionData FeedSessionData 
        {
            get
            {
                if (_DataSample != null && _DataSample.IsConnected && _DataSample.SessionData != null)
                {
                    _feedSessionData = _DataSample.SessionData;
                }
                return _feedSessionData;
            }
            set
            {
                FeedSessionData = value;
            }
        }
        public List<SessionData._SessionInfo._Sessions._ResultsPositions> AllResultsPositions { get; set; }
        public List<SessionData._DriverInfo._Drivers> AllDrivers { get; set; }
        public List<Driver> AllPositions { get; set; }
        public List<Driver> SurroundingPositions { get; set; }
        public List<Driver> CachedAllPositions { get; set; }
        public Dictionary<int, List<Driver>> SortedPositions { get; set; }

        private Speed _speedAtPosition;
        public Speed SpeedAtPosition
        {
            get
            {
                if (_speedAtPosition != null)
                {
                    return _speedAtPosition;
                }

                if (FeedTelemetry != null && FeedSessionData != null && TrackId > 0)
                {
                    _speedAtPosition = Speed.GetSpeedData(TrackId, FeedSessionData.DriverInfo.Drivers.First(d => d.CarIdx == FeedTelemetry.CamCarIdx).CarPath);
                }
                else
                {
                    _speedAtPosition = new Speed();
                }
                return _speedAtPosition;
            }
            set
            {
                _speedAtPosition = value;
            }
        }
        private int _trackId;
        public int TrackId 
        {
            get
            {
                if (_trackId != 0)
                {
                    return _trackId;
                }
                _trackId = 0;
                if (FeedSessionData != null && _DataSample.SessionData != null)
                {
                    _trackId = (int)FeedSessionData.WeekendInfo.TrackID;
                }
                return _trackId;
            }
            set
            {
                _trackId = value;
            }
        }
        public double TrackLength { get; set; }
        public bool IsRace { get; set; }
        public SessionData._SessionInfo._Sessions CurrentSession { get; set; }

        public List<Lap> LapList { get; set; }
        Stopwatch StopWatch;

        public TelemetryData()
        {

        }

        public TelemetryData(TelemetryData original)
        {
            _DataSample = original._DataSample;
            AllResultsPositions = original.AllResultsPositions;
            AllDrivers = original.AllDrivers;
            AllPositions = original.AllPositions;
            SurroundingPositions = original.SurroundingPositions;
            CachedAllPositions = original.CachedAllPositions;
            SortedPositions = original.SortedPositions;
            TrackLength = original.TrackLength;
            IsRace = original.IsRace;
            CurrentSession = original.CurrentSession;
        }
        public void StartOperation(Action action)
        {
            if (thread != null)
                thread.Abort();

            thread = new Thread(() => action());
            thread.Start();
        }

        public void RetrieveData()
        {
            LapList = new List<Lap>();
            StopWatch = Stopwatch.StartNew();
            var iracing = new iRacingConnection();
            while (true)
            {
                try
                {
                    foreach (var data in iRacing.GetDataFeed().WithLastSample())
                    {
                        if (data != null)
                        {
                            IsConnected = data.IsConnected;
                            if (IsConnected)
                            {
                                
                                CollectData(data);
                                SortPositions();
                                GetSpeedAtPosition();
                                LastSample = this;
                            }
                            else
                            {
                                Trace.WriteLine("No connection to IRacing. Retrying in 5 seconds...");
                                Thread.Sleep(5000);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    Trace.WriteLine("Retrying in 10 seconds...");
                    Thread.Sleep(10000);
                }
            }
        }

        private void CollectData(DataSample data)
        {
            _DataSample = data;
        }

        public void SortPositions()
        {
            CurrentSession = FeedSessionData.SessionInfo.Sessions[FeedTelemetry.Session.SessionNum];
            IsRace = CurrentSession.IsRace;
            AllResultsPositions = new List<SessionData._SessionInfo._Sessions._ResultsPositions>();
            if (CurrentSession.ResultsPositions?.Length > 0)
            {
                for (int i = 0; i < CurrentSession.ResultsPositions.Length; i++)
                {
                    AllResultsPositions.Add(CurrentSession.ResultsPositions[i]);
                }
            }

            if (AllResultsPositions.Count == 0 && CurrentSession.IsRace)
            {
                AllResultsPositions = FeedSessionData.SessionInfo.Sessions.Where(s => s.SessionType.ToUpper().Contains("QUALI")).First().ResultsPositions.ToList();
            }

            AllDrivers = new List<SessionData._DriverInfo._Drivers>();
            if (FeedSessionData.DriverInfo.Drivers.Length > 0)
            {
                for (int i = 0; i < FeedSessionData.DriverInfo.Drivers.Length; i++)
                {
                    AllDrivers.Add(FeedSessionData.DriverInfo.Drivers[i]);
                }
            }
            TrackLength = double.Parse(FeedSessionData.WeekendInfo.TrackLength.Substring(0, FeedSessionData.WeekendInfo.TrackLength.Length - 3)) * 1000;
            
            long myCarId = FeedSessionData.DriverInfo.DriverCarIdx;
            int myCarClass = (int)(FeedTelemetry["PlayerCarClass"] ?? 0);
            var sessionType = CurrentSession.SessionType;
            var sessionTime = CurrentSession._SessionTime;
            
            AllPositions = AllResultsPositions.Join(AllDrivers, p => p.CarIdx, d => d.CarIdx, (p, d) => new
            {
                d.CarIdx,
                d.CarPath,
                d.CarClassID,
                d.CarClassColor,
                ClassPosition = p.ClassPosition + 1,
                p.Position,
                d.UserName,
                d.IRating,
                d.LicString,
                p.FastestTime,
                p.LastTime,
                p.LapsComplete,
                InPit = FeedTelemetry.CarIdxOnPitRoad[d.CarIdx]
            }).Select(x => new Driver()
            {
                CarId = (int)x.CarIdx,
                CarPath = x.CarPath,
                ClassColor = x.CarClassColor,
                ClassId = (int)x.CarClassID,
                ClassPosition = (int)x.ClassPosition,
                OverallPosition = (int)x.Position,
                Name = x.UserName,
                iRating = (int)x.IRating,
                SafetyRating = x.LicString,
                FastestLap = TimeSpan.FromSeconds(x.FastestTime),
                LastLap = TimeSpan.FromSeconds(x.LastTime),
                LapsComplete = (int)x.LapsComplete,
                PosOnTrack = FeedTelemetry.CarIdxLapDistPct[x.CarIdx] * TrackLength,
                Distance = FeedTelemetry.CarIdxDistance[x.CarIdx] * TrackLength,
                InPit = x.InPit
            }).ToList();

            foreach (var driver in AllDrivers.OrderByDescending(d => d.IRating).Where(d => !d.IsPaceCar))
            {
                if (!AllPositions.Select(p => p.CarId).Contains((int)driver.CarIdx))
                {
                    AllPositions.Add(new Driver
                    {
                        CarId = (int)driver.CarIdx,
                        CarPath = driver.CarPath, 
                        ClassColor = driver.CarClassColor,
                        ClassId = (int)driver.CarClassID,
                        ClassPosition = FeedTelemetry.CarIdxClassPosition[driver.CarIdx] == 0 ? 99 : FeedTelemetry.CarIdxClassPosition[driver.CarIdx], 
                        OverallPosition = FeedTelemetry.CarIdxPosition[driver.CarIdx] == 0 ? 99 : FeedTelemetry.CarIdxPosition[driver.CarIdx],
                        Name = driver.UserName,
                        iRating = (int)driver.IRating,
                        SafetyRating = driver.LicString,
                        FastestLap = TimeSpan.FromSeconds(9999),
                        LastLap = TimeSpan.FromSeconds(9999),
                        LapsComplete = -1,
                        PosOnTrack = FeedTelemetry.CarIdxLapDistPct[driver.CarIdx] * TrackLength,
                        Distance = FeedTelemetry.CarIdxDistance[driver.CarIdx] * TrackLength,
                        InPit = FeedTelemetry.CarIdxOnPitRoad[driver.CarIdx]
                    });
                }
            }

            var viewedCar = AllPositions.Where(p => p.CarId == FeedTelemetry.CamCarIdx).FirstOrDefault() ?? AllPositions.FirstOrDefault() ?? new Driver();
            var driverClasses = AllDrivers.GroupBy(d => d.CarClassID).Select(d => d.Key).ToList();

            foreach (var position in AllPositions)
            {
                if (CachedAllPositions != null)
                {
                    if ((CachedAllPositions.FirstOrDefault(c => c.CarId == position.CarId)?.LastLap ?? TimeSpan.FromSeconds(-1)) == position.LastLap)
                    {
                        var test = CachedAllPositions.FirstOrDefault(c => c.CarId == position.CarId)?.SecondsSinceLastLap ?? 0;
                        position.SecondsSinceLastLap = test + StopWatch.Elapsed.TotalSeconds;
                    }
                    else if (position.LastLap.TotalSeconds < 0 || position.LastLap.TotalSeconds > 20000)
                    {
                        position.SecondsSinceLastLap = 0;
                    }
                }
            }
            StopWatch.Restart();
            SortedPositions = AllPositions
                .GroupBy(a => a.ClassId)
                .OrderBy(a => a.First().FastestLap)
                .ToDictionary(a => a.Key, a => a
                    .OrderBy(p => p.ClassPosition).ToList());

            Dictionary<int, List<Driver>> updatedSortedPostions = new Dictionary<int, List<Driver>>();
            foreach (var positionGroup in SortedPositions)
            {
                var sortedUnplacedDrivers = SortUnplacedDrivers(positionGroup);
                foreach (var position in sortedUnplacedDrivers.Value)
                {
                    position.TimeBehindLeader = FeedTelemetry.CarIdxF2Time[position.CarId];
                    position.FastestLapDelta = GetFastestLapDelta(position, positionGroup.Value.Where(p => p.PosOnTrack > 0).FirstOrDefault());
                    AllPositions.Where(p => p.CarId == position.CarId).First().TimeBehindLeader = position.TimeBehindLeader;
                    AllPositions.Where(p => p.CarId == position.CarId).First().FastestLapDelta = position.FastestLapDelta;
                }
                updatedSortedPostions.Add(sortedUnplacedDrivers.Key, sortedUnplacedDrivers.Value);
            }
            SortedPositions = updatedSortedPostions;
            
            CachedAllPositions = AllPositions;
        }

        private void GetSpeedAtPosition()
        {
            Trace.WriteLine(FeedTelemetry.LapDist);
            if (LapList.Where(l => l.LapNumber == FeedTelemetry.Lap).Count() == 0)
            {
                LapList.Add(new Lap()
                {
                    TrackId = TrackId,
                    TrackLength = (int)TrackLength,
                    CarPath = AllDrivers.Where(d => d.CarIdx == FeedSessionData.DriverInfo.DriverCarIdx).First().CarPath,
                    LapNumber = FeedTelemetry.Lap,
                    EstLapTime = FeedSessionData.DriverInfo.DriverCarEstLapTime,
                    SpeedData = new Dictionary<int, Tuple<double, double, TrackLocation>>()
                });
            }
            if (SpeedAtPosition == null)
            {
                SpeedAtPosition = new Speed();
                SpeedAtPosition.TrackId = TrackId;
                SpeedAtPosition.CarPath = AllDrivers.Where(d => d.CarIdx == FeedSessionData.DriverInfo.DriverCarIdx).First().CarPath;
                SpeedAtPosition.SpeedStringDictionary = new Dictionary<string, double[]>();
                
            }
            var positionOnTrack = Math.Floor(FeedTelemetry.LapDist);
            var timeAtPosition = (double)FeedTelemetry.LapCurrentLapTime;
            var speed = FeedTelemetry.Speed;
            
            Lap currentLapData = LapList.Where(l => l.LapNumber == FeedTelemetry.Lap).First();
            var currentTrackLocation = FeedTelemetry.CarIdxTrackSurface[FeedSessionData.DriverInfo.DriverCarIdx];
            if (!currentLapData.SpeedData.Keys.Contains((int)positionOnTrack))
            {
                currentLapData.SpeedData.Add((int)positionOnTrack, new Tuple<double, double, TrackLocation>(speed, FeedTelemetry.LapCurrentLapTime, currentTrackLocation));
            }

            if (timeAtPosition >= 0.000 
                && !FeedTelemetry.OnPitRoad 
                && FeedTelemetry.CarIdxTrackSurface[FeedSessionData.DriverInfo.DriverCarIdx] == TrackLocation.OnTrack 
                && timeAtPosition <= FeedSessionData.DriverInfo.DriverCarEstLapTime * FeedTelemetry.LapDistPct + 5
                && (int)_DataSample.LastSample.Telemetry.LapDistPct * TrackLength != positionOnTrack)
            {
                if (!SpeedAtPosition.SpeedStringDictionary.ContainsKey(positionOnTrack.ToString()))
                {
                    SpeedAtPosition.SpeedStringDictionary.Add(positionOnTrack.ToString(), new double[2] { 1, speed });
                }
                else
                {
                    var timeAtPositionCopy = SpeedAtPosition;
                    var totalRecords = SpeedAtPosition.SpeedStringDictionary[positionOnTrack.ToString()][0];
                    var averageSpeed = ((SpeedAtPosition.SpeedStringDictionary[positionOnTrack.ToString()][1] * totalRecords) + speed) / (totalRecords + 1);
                    timeAtPositionCopy.SpeedStringDictionary[positionOnTrack.ToString()] = new double[2] { totalRecords + 1, averageSpeed };
                    SpeedAtPosition = timeAtPositionCopy;
                }
            }
        }

        public double GetRelativeDelta(Driver listedDriver, Driver targetDriver, double trackLength)
        {
            var distanceBetweenDrivers = targetDriver.PosOnTrack - listedDriver.PosOnTrack;
            var averageSpeed = 45.0;
            var delta = 0.0;

            if (Math.Abs(distanceBetweenDrivers) > 1500)
            {
                if (targetDriver.PosOnTrack - trackLength < -1500 && listedDriver.PosOnTrack - trackLength > (-1500 + targetDriver.PosOnTrack))
                {
                    distanceBetweenDrivers = trackLength - listedDriver.PosOnTrack + targetDriver.PosOnTrack;
                    delta = distanceBetweenDrivers / averageSpeed;
                    if (SpeedAtPosition != null && SpeedAtPosition.SpeedIntDictionary != null && SpeedAtPosition.SpeedIntDictionary.Count > 0)
                    {
                        var test = SpeedAtPosition.SpeedIntDictionary.Where(s => s.Key <= Math.Floor(targetDriver.PosOnTrack) || s.Key >= Math.Floor(listedDriver.PosOnTrack)).Select(s => 1 / s.Value[1]).Sum();
                        var recordCount = SpeedAtPosition.SpeedIntDictionary.Where(s => s.Key >= Math.Floor(targetDriver.PosOnTrack) && s.Key <= Math.Floor(listedDriver.PosOnTrack)).Select(s => s.Value[0]).Sum();
                        var speedSum = SpeedAtPosition.SpeedIntDictionary.Where(s => s.Key >= Math.Floor(targetDriver.PosOnTrack) && s.Key <= Math.Floor(listedDriver.PosOnTrack)).Select(s => s.Value[0] * s.Value[1]).Sum();
                        delta = test;//distanceBetweenDrivers / (speedSum / recordCount);
                    }
                }
                else if (listedDriver.PosOnTrack - trackLength < -1500 && targetDriver.PosOnTrack - trackLength > (-1500 + listedDriver.PosOnTrack))
                {
                    distanceBetweenDrivers = trackLength - targetDriver.PosOnTrack + listedDriver.PosOnTrack;
                    delta = distanceBetweenDrivers / averageSpeed;
                    if (SpeedAtPosition != null && SpeedAtPosition.SpeedIntDictionary != null && SpeedAtPosition.SpeedIntDictionary.Count > 0)
                    {
                        var test = SpeedAtPosition.SpeedIntDictionary.Where(s => s.Key <= Math.Floor(listedDriver.PosOnTrack) || s.Key >= Math.Floor(targetDriver.PosOnTrack)).Select(s => 1 / s.Value[1]).Sum();
                        //var recordCount = SpeedAtPosition.SpeedIntDictionary.Where(s => s.Key >= Math.Floor(listedDriver.PosOnTrack) && s.Key <= Math.Floor(targetDriver.PosOnTrack)).Select(s => s.Value[0]).Sum();
                        //var speedSum = SpeedAtPosition.SpeedIntDictionary.Where(s => s.Key >= Math.Floor(listedDriver.PosOnTrack) && s.Key <= Math.Floor(targetDriver.PosOnTrack)).Select(s => s.Value[0] * s.Value[1]).Sum();
                        delta = test * -1;//distanceBetweenDrivers / (speedSum / recordCount) * -1;
                    }
                }
            }
            else
            {
                delta = distanceBetweenDrivers / averageSpeed;
                if (SpeedAtPosition != null && SpeedAtPosition.SpeedIntDictionary != null && SpeedAtPosition.SpeedIntDictionary.Count > 0)
                {
                    if (distanceBetweenDrivers < 0)
                    {
                        var test = SpeedAtPosition.SpeedIntDictionary.Where(s => s.Key >= Math.Floor(targetDriver.PosOnTrack) && s.Key <= Math.Floor(listedDriver.PosOnTrack)).Select(s => 1 / s.Value[1]).Sum();
                        var recordCount = SpeedAtPosition.SpeedIntDictionary.Where(s => s.Key >= Math.Floor(targetDriver.PosOnTrack) && s.Key <= Math.Floor(listedDriver.PosOnTrack)).Select(s => s.Value[0]).Sum();
                        var speedSum = SpeedAtPosition.SpeedIntDictionary.Where(s => s.Key >= Math.Floor(targetDriver.PosOnTrack) && s.Key <= Math.Floor(listedDriver.PosOnTrack)).Select(s => s.Value[0] * s.Value[1]).Sum();
                        delta = test * -1;//distanceBetweenDrivers / (speedSum / recordCount);
                    }
                    else
                    {
                        var test = SpeedAtPosition.SpeedIntDictionary.Where(s => s.Key >= Math.Floor(listedDriver.PosOnTrack) && s.Key <= Math.Floor(targetDriver.PosOnTrack)).Select(s => 1 / s.Value[1]).Sum();
                        var recordCount = SpeedAtPosition.SpeedIntDictionary.Where(s => s.Key >= Math.Floor(listedDriver.PosOnTrack) && s.Key <= Math.Floor(targetDriver.PosOnTrack)).Select(s => s.Value[0]).Sum();
                        var speedSum = SpeedAtPosition.SpeedIntDictionary.Where(s => s.Key >= Math.Floor(listedDriver.PosOnTrack) && s.Key <= Math.Floor(targetDriver.PosOnTrack)).Select(s => s.Value[0] * s.Value[1]).Sum();
                        delta = test;//distanceBetweenDrivers / (speedSum / recordCount);
                    }
                }
            }
            return delta;
        }

        private double GetFastestLapDelta(Driver listedDriver, Driver targetDriver)
        {
            double delta;
            if (targetDriver == null || listedDriver.FastestLap.TotalSeconds < 0 || targetDriver.FastestLap.TotalSeconds < 0)
            {
                delta = -1;
            }
            else
            {
                delta = listedDriver.FastestLap.TotalSeconds - targetDriver.FastestLap.TotalSeconds;
            }
            return delta;
        }

        private KeyValuePair<int, List<Driver>> SortUnplacedDrivers(KeyValuePair<int, List<Driver>> driverClassGroup)
        {
            var driversInClass = driverClassGroup.Value.OrderBy(d => d.ClassPosition).ThenByDescending(d => d.iRating).ToList();
            var lastDriver = driversInClass.First();
            foreach (var driver in driversInClass)
            {
                if (driver.ClassPosition == 99)
                {
                    driver.ClassPosition = lastDriver.ClassPosition + 1;
                }
                lastDriver = driver;
            }
            return new KeyValuePair<int, List<Driver>>(driverClassGroup.Key, driversInClass);
        }
    }
}
