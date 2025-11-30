using iRacingSDK;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace RacingOverlay
{
    public class TelemetryData
    {
        Thread thread;
        public bool IsConnected { get; set; }
        public DataSample _DataSample { get; set; }
        public TelemetryData LastSample { get; set; }

        private Telemetry _feedTelemetry;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public Telemetry FeedTelemetry
        {
            get
            {
#if SAMPLE
                return _feedTelemetry;
#endif
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
                _feedSessionData = value;
            }
        }
        public List<SessionData._SessionInfo._Sessions._ResultsPositions> AllResultsPositions
        {
            get
            {
                var allResultsPositions = new List<SessionData._SessionInfo._Sessions._ResultsPositions>();
                if (CurrentSession?.ResultsPositions?.Length > 0)
                {
                    allResultsPositions = CurrentSession.ResultsPositions.ToList();
                }

                if (allResultsPositions.Count == 0 && CurrentSession?.IsRace == true && FeedSessionData.SessionInfo.Sessions.FirstOrDefault(s => s.SessionType.ToUpper().Contains("QUALI")) != null)
                {
                    //TODO fix null reference error
                    allResultsPositions = FeedSessionData.SessionInfo?.Sessions?.Where(s => s.SessionType.ToUpper().Contains("QUALI")).First().ResultsPositions.ToList();
                }
                return allResultsPositions;
            }
        }
        public List<SessionData._DriverInfo._Drivers> AllDrivers
        {
            get
            {
                var allDrivers = new List<SessionData._DriverInfo._Drivers>();
                if (FeedSessionData.DriverInfo.Drivers.Length > 0)
                {
                    allDrivers = FeedSessionData.DriverInfo.Drivers.ToList();
                }
                return allDrivers;
            }
        }

        public List<Driver> AllPositions { get; set; }
        public Dictionary<int, List<Driver>> SortedPositions { get; set; }

        private Dictionary<int, Dictionary<string, List<Speed>>> _savedSpeedData;
        public Dictionary<int, Dictionary<string, List<Speed>>> SavedSpeedData
        {
            get
            {
                if (_savedSpeedData != null)
                {
                    return _savedSpeedData;
                }

                if (FeedTelemetry != null && FeedSessionData != null && TrackId > 0)
                {
                    var carClasses = FeedSessionData.DriverInfo.Drivers.Select(d => d.CarClassID).Distinct().ToList();

                    var carClassSpeedDict = new Dictionary<int, Dictionary<string, List<Speed>>>();
                    foreach (var carClass in carClasses)
                    {
                        var carSpeedData = FeedSessionData.DriverInfo.Drivers.Where(d => d.CarClassID == carClass).Select(d => d.CarPath).Distinct().ToDictionary(d => d, d => Lap.GetSpeedData(TrackId, TrackName, d));
                        carClassSpeedDict.Add((int)carClass, carSpeedData);
                    }
                    _savedSpeedData = carClassSpeedDict;
                }
                else
                {
                    _savedSpeedData = new Dictionary<int, Dictionary<string, List<Speed>>>();
                }
                return _savedSpeedData;
            }
            set
            {
                _savedSpeedData = value;
            }
        }
        private int _trackId;
        public int TrackId
        {
            get
            {
#if SAMPLE
                return _trackId;
#endif
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

        private string _trackName;
        public string TrackName
        {
            get
            {
#if SAMPLE
                return _trackName;
#endif
                if (_trackName != null && _trackName == FeedSessionData?.WeekendInfo?.TrackDisplayShortName)
                {
                    return _trackName;
                }

                if (FeedSessionData != null && _DataSample.SessionData != null)
                {
                    _trackName = FeedSessionData.WeekendInfo.TrackDisplayShortName;
                }
                return _trackName;
            }
            set
            {
                _trackName = value;
            }
        }

        public double TrackLength
        {
            get
            {
                var trackLength = 0.0;
                if (FeedSessionData?.WeekendInfo?.TrackLength != null)
                {
                    trackLength = double.Parse(FeedSessionData.WeekendInfo.TrackLength.Substring(0, FeedSessionData.WeekendInfo.TrackLength.Length - 3)) * 1000;
                }
                return trackLength;
            }
        }
        public bool IsRace { get; set; }
        public SessionData._SessionInfo._Sessions CurrentSession
        {
            get
            {
#if SAMPLE
                return FeedSessionData.SessionInfo.Sessions[0];
#endif
                var currentSession = new SessionData._SessionInfo._Sessions();
                if (FeedSessionData.SessionInfo.Sessions.Length >= (FeedTelemetry.Session?.SessionNum ?? 99))
                {
                    currentSession = FeedSessionData.SessionInfo.Sessions[FeedTelemetry.Session.SessionNum];
                }
                return currentSession;
            }
        }

        public List<Lap> LapList { get; set; }
        Stopwatch StopWatch;
        public bool IsReady
        {
            get
            {
                return SortedPositions != null && AllPositions != null;
            }
        }
        private DateTime LastValidConnection { get; set; }

        public TelemetryData()
        {

        }

        public TelemetryData(TelemetryData original)
        {
            if (original != null)
            {
                _DataSample = original._DataSample;
                AllPositions = original.AllPositions;
                SortedPositions = original.SortedPositions;
                TrackId = original.TrackId;
                TrackName = original.TrackName;
                IsRace = original.IsRace;
                FeedTelemetry = original.FeedTelemetry;
                FeedSessionData = original.FeedSessionData;
            }
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
                foreach (var data in iRacing.GetDataFeed().WithLastSample())
                {
                    if (data != null)
                    {
                        IsConnected = data.IsConnected;
                        if (IsConnected)
                        {
                            LastValidConnection = DateTime.UtcNow;
                            CollectData(data);
                            CollectPositions();
                            RecordSpeedData();
                        }
                        else
                        {
                            if (LastValidConnection < DateTime.UtcNow.AddSeconds(-30))
                            {
                                ClearData();
                            }

                            Logger.Info("No connection to IRacing. Retrying in 5 seconds...");
                            Thread.Sleep(5000);
                        }
                    }
                }
            }
        }

        private void ClearData()
        {
            AllPositions = null;
            SortedPositions = null;
            LapList = null;
        }

        private void CollectData(DataSample data)
        {
            _DataSample = data;
        }

        public void CollectPositions()
        {
            IsRace = CurrentSession.IsRace;
            CollectAllPositions();
            CollectSortedPositions();
        }

        private void CollectAllPositions()
        {
            var preCorrectedPositions = AllResultsPositions.Join(AllDrivers, p => p.CarIdx, d => d.CarIdx, (p, d) => new Driver()
            {
                CarId = (int)d.CarIdx,
                CarNumber = d.CarNumber,
                CarPath = d.CarPath,
                ClassId = (int)d.CarClassID,
                ClassColor = d.CarClassColor,
                ClassPosition = (int)p.ClassPosition + 1,
                OverallPosition = (int)p.Position,
                Name = d.UserName,
                iRating = (int)d.IRating,
                SafetyRating = new Tuple<string, string>(d.LicString, d.LicColor),
                FastestLap = GetFastestLap(p),
                LastLap = ((float[])FeedTelemetry["CarIdxLastLapTime"])[d.CarIdx] > 0 ? (double?)((float[])FeedTelemetry["CarIdxLastLapTime"])[d.CarIdx] : null,
                LapsComplete = (int)FeedTelemetry.CarIdxLap[d.CarIdx],
                PosOnTrack = FeedTelemetry.CarIdxLapDistPct[d.CarIdx] * TrackLength,
                Distance = FeedTelemetry.CarIdxDistance[d.CarIdx] * TrackLength,
                InPit = FeedTelemetry.CarIdxOnPitRoad[d.CarIdx]
            }).ToList();

            foreach (var driver in AllDrivers.OrderByDescending(d => d.IRating).Where(d => !d.IsPaceCar && d.CarIsPaceCar <= 0))
            {
                if (!preCorrectedPositions.Select(p => p.CarId).Contains((int)driver.CarIdx))
                {
                    preCorrectedPositions.Add(new Driver
                    {
                        CarId = (int)driver.CarIdx,
                        CarNumber = driver.CarNumber,
                        CarPath = driver.CarPath,
                        ClassColor = driver.CarClassColor,
                        ClassId = (int)driver.CarClassID,
                        ClassPosition = FeedTelemetry.CarIdxClassPosition[driver.CarIdx] == 0 ? null : (int?)FeedTelemetry.CarIdxClassPosition[driver.CarIdx],
                        OverallPosition = FeedTelemetry.CarIdxPosition[driver.CarIdx] == 0 ? null : (int?)FeedTelemetry.CarIdxPosition[driver.CarIdx],
                        Name = driver.UserName,
                        iRating = (int)driver.IRating,
                        SafetyRating = new Tuple<string, string>(driver.LicString, driver.LicColor),
                        FastestLap = null,
                        LastLap = null,
                        LapsComplete = null,
                        PosOnTrack = FeedTelemetry.CarIdxLapDistPct[driver.CarIdx] * TrackLength,
                        Distance = FeedTelemetry.CarIdxDistance[driver.CarIdx] * TrackLength,
                        InPit = FeedTelemetry.CarIdxOnPitRoad[driver.CarIdx]
                    });
                }
            }

            var correctedPositions = new List<Driver>(preCorrectedPositions);
            foreach (var position in correctedPositions)
            {
                if (AllPositions != null)
                {
                    var lastPosition = AllPositions.FirstOrDefault(c => c.CarId == position.CarId);
                    if (position.LastLap == null || lastPosition?.LastLap != position?.LastLap)
                    {
                        position.LapChangeTime = DateTime.UtcNow;
                    }
                    else
                    {
                        position.LapChangeTime = lastPosition?.LapChangeTime ?? DateTime.MinValue;
                    }
                }
            }

            AllPositions = correctedPositions;
            StopWatch.Restart();
        }

        private void CollectSortedPositions()
        {
            var preCorrectedSortedPositions = AllPositions
                                  .GroupBy(a => a.ClassId)
                                  .OrderBy(a => a.First().FastestLap)
                                  .ToDictionary(a => a.Key, a => a.OrderBy(p => p.ClassPosition).ToList());

            Dictionary<int, List<Driver>> correctedSortedPostions = new Dictionary<int, List<Driver>>();
            foreach (var positionGroup in preCorrectedSortedPositions)
            {

                var sortedUnplacedDrivers = new KeyValuePair<int, List<Driver>>(positionGroup.Key, positionGroup.Value.OrderBy(s => s.ClassPosition).ToList());

                if (IsRace)
                {
                    sortedUnplacedDrivers = new KeyValuePair<int, List<Driver>>(positionGroup.Key, positionGroup.Value.OrderByDescending(s => s.Distance).ToList());
                }
                sortedUnplacedDrivers = SortUnplacedDrivers(positionGroup);

                var i = 1;
                foreach (var position in sortedUnplacedDrivers.Value)
                {
                    if (FeedTelemetry.IsReplayPlaying)
                    {
                        position.ClassPosition = IsRace ? i : position.ClassPosition == 0 ? i : position.ClassPosition;
                    }
                    position.TimeBehindLeader = FeedTelemetry.CarIdxF2Time[position.CarId];
                    position.FastestLapDelta = GetFastestLapDelta(position, positionGroup.Value.Where(p => p.PosOnTrack > 0).FirstOrDefault());
                    i++;

                    AllPositions.Where(p => p.CarId == position.CarId).First().TimeBehindLeader = position.TimeBehindLeader;
                    AllPositions.Where(p => p.CarId == position.CarId).First().FastestLapDelta = position.FastestLapDelta;
                }
                correctedSortedPostions.Add(sortedUnplacedDrivers.Key, sortedUnplacedDrivers.Value);
            }
            SortedPositions = correctedSortedPostions;
        }

        private void RecordSpeedData()
        {
            var carPath = AllDrivers.First(d => d.CarIdx == FeedSessionData.DriverInfo.DriverCarIdx).CarPath;
            if (LapList == null)
            {
                LapList = new List<Lap>();
            }
            else if (LapList.Count > 0 && (LapList.First()?.TrackId != TrackId || LapList.First()?.CarPath != carPath))
            {
                LapList = new List<Lap>();
            }


            if (LapList.Where(l => l.LapNumber == FeedTelemetry.Lap).Count() == 0)
            {
                LapList.Add(new Lap()
                {
                    TrackId = TrackId,
                    TrackName = TrackName,
                    TrackLength = (int)TrackLength,
                    CarPath = carPath,
                    LapNumber = FeedTelemetry.Lap,
                    EstLapTime = FeedSessionData.DriverInfo.DriverCarEstLapTime,
                    SpeedData = new List<Speed>()
                });

            }
            var positionOnTrack = Math.Floor(FeedTelemetry.LapDist);
            var timeAtPosition = (double)FeedTelemetry.LapCurrentLapTime;
            var speed = FeedTelemetry.Speed;

            Lap currentLapData = LapList.Where(l => l.LapNumber == FeedTelemetry.Lap).First();
            var currentTrackLocation = FeedTelemetry.CarIdxTrackSurface[FeedSessionData.DriverInfo.DriverCarIdx];
            if (!currentLapData.SpeedData.Select(s => s.Meter).Contains((int)positionOnTrack))
            {
                var speedRecord = new Speed()
                {
                    Meter = (int)positionOnTrack,
                    SpeedMS = speed,
                    TimeInSeconds = FeedTelemetry.LapCurrentLapTime,
                    Location = currentTrackLocation
                };
                currentLapData.SpeedData.Add(speedRecord);
            }
        }

        public double GetRelativeDelta(Driver viewedDriver, Driver targetDriver, double trackLength)
        {
            var distanceBetweenDrivers = targetDriver.PosOnTrack - viewedDriver.PosOnTrack;
            var targetDriverAngle = targetDriver.PosOnTrack / trackLength * 360;
            var viewedDriverAngle = viewedDriver.PosOnTrack / trackLength * 360;

            var originalDifferenceAngle = viewedDriverAngle - targetDriverAngle;
            var differenceAngle = viewedDriverAngle - targetDriverAngle;

            if (Math.Abs(differenceAngle) > 180)
                differenceAngle = differenceAngle > 180 ? differenceAngle - 360 : differenceAngle + 360;

            distanceBetweenDrivers = differenceAngle / 360 * trackLength * -1;

            var averageSpeed = 45.0;
            var delta = 0.0;
            var listedCarSpeedData = SavedSpeedData.Where(s => s.Key == viewedDriver.ClassId).First().Value.Where(s => s.Key == viewedDriver.CarPath).First().Value;
            if (listedCarSpeedData == null)
            {
                listedCarSpeedData = SavedSpeedData.Where(s => s.Key == viewedDriver.ClassId).First().Value.FirstOrDefault(s => s.Value != null).Value;
            }
            var targetCarSpeedData = SavedSpeedData.Where(s => s.Key == targetDriver.ClassId).First().Value.Where(s => s.Key == targetDriver.CarPath).First().Value;
            if (targetCarSpeedData == null)
            {
                targetCarSpeedData = SavedSpeedData.Where(s => s.Key == targetDriver.ClassId).First().Value.FirstOrDefault(s => s.Value != null).Value;
            }

            // Set default delta if no speed data is present
            delta = distanceBetweenDrivers / averageSpeed;

            if (distanceBetweenDrivers >= 0)
            {
                if (originalDifferenceAngle > 180)
                {
                    if (listedCarSpeedData != null && listedCarSpeedData.Count > 0)
                        delta = listedCarSpeedData.Where(s => s.Meter <= Math.Floor(targetDriver.PosOnTrack) || s.Meter >= Math.Floor(viewedDriver.PosOnTrack)).Select(s => 1 / s.SpeedMS).Sum();
                    else if (targetCarSpeedData != null && targetCarSpeedData.Count > 0)
                        delta = targetCarSpeedData.Where(s => s.Meter <= Math.Floor(targetDriver.PosOnTrack) || s.Meter >= Math.Floor(viewedDriver.PosOnTrack)).Select(s => 1 / s.SpeedMS).Sum();
                }
                else
                {
                    if (listedCarSpeedData != null && listedCarSpeedData.Count > 0)
                        delta = listedCarSpeedData.Where(s => s.Meter <= Math.Floor(targetDriver.PosOnTrack) && s.Meter >= Math.Floor(viewedDriver.PosOnTrack)).Select(s => 1 / s.SpeedMS).Sum();
                    else if (targetCarSpeedData != null && targetCarSpeedData.Count > 0)
                        delta = targetCarSpeedData.Where(s => s.Meter <= Math.Floor(targetDriver.PosOnTrack) && s.Meter >= Math.Floor(viewedDriver.PosOnTrack)).Select(s => 1 / s.SpeedMS).Sum();
                }

            }
            else
            {
                if (originalDifferenceAngle < -180)
                {
                    if (targetCarSpeedData != null && targetCarSpeedData.Count > 0)
                        delta = targetCarSpeedData.Where(s => s.Meter >= Math.Floor(targetDriver.PosOnTrack) || s.Meter <= Math.Floor(viewedDriver.PosOnTrack)).Select(s => 1 / s.SpeedMS).Sum() * -1;
                    else if (listedCarSpeedData != null && listedCarSpeedData.Count > 0)
                        delta = listedCarSpeedData.Where(s => s.Meter >= Math.Floor(targetDriver.PosOnTrack) || s.Meter <= Math.Floor(viewedDriver.PosOnTrack)).Select(s => 1 / s.SpeedMS).Sum() * -1;
                }
                else
                {
                    if (targetCarSpeedData != null && targetCarSpeedData.Count > 0)
                        delta = targetCarSpeedData.Where(s => s.Meter >= Math.Floor(targetDriver.PosOnTrack) && s.Meter <= Math.Floor(viewedDriver.PosOnTrack)).Select(s => 1 / s.SpeedMS).Sum() * -1;
                    else if (listedCarSpeedData != null && listedCarSpeedData.Count > 0)
                        delta = listedCarSpeedData.Where(s => s.Meter >= Math.Floor(targetDriver.PosOnTrack) && s.Meter <= Math.Floor(viewedDriver.PosOnTrack)).Select(s => 1 / s.SpeedMS).Sum() * -1;
                }
            }

            return delta;
        }

        private double GetFastestLapDelta(Driver listedDriver, Driver targetDriver)
        {
            double delta;
            if (listedDriver?.FastestLap == null || targetDriver?.FastestLap == null)
            {
                delta = -1;
            }
            else
            {
                delta = listedDriver.FastestLap.Value - targetDriver.FastestLap.Value;
            }
            return delta;
        }

        private double? GetFastestLap(SessionData._SessionInfo._Sessions._ResultsPositions driver)
        {
            var fastestLap = ((float[])FeedTelemetry["CarIdxBestLapTime"])[driver.CarIdx];

            return fastestLap > 0 ? fastestLap : driver.FastestTime > 0 ? (double?)driver.FastestTime : null;
        }

        private KeyValuePair<int, List<Driver>> SortUnplacedDrivers(KeyValuePair<int, List<Driver>> driverClassGroup)
        {
            var unplacedDriversInClass = driverClassGroup.Value.Where(d => d.ClassPosition == null).OrderByDescending(d => d.iRating).ToList();
            var placedDriversInClass = driverClassGroup.Value.Where(d => d.ClassPosition != null).OrderBy(d => d.ClassPosition).ToList();

            if (IsRace && FeedTelemetry.RaceLaps > 0)
            {
                placedDriversInClass = placedDriversInClass.OrderByDescending(d => d.Distance).ToList();
            }

            var driversInClass = new List<Driver>();

            driversInClass.AddRange(placedDriversInClass);
            var lastClassPosition = 0;

            if (driversInClass.Count > 0)
            {
                lastClassPosition = driversInClass.Last().ClassPosition ?? 0;
            }

            foreach (var driver in unplacedDriversInClass)
            {
                if (driver.ClassPosition == null)
                {
                    driver.ClassPosition = lastClassPosition + 1;
                }
                lastClassPosition++;
            }
            driversInClass.AddRange(unplacedDriversInClass);

            return new KeyValuePair<int, List<Driver>>(driverClassGroup.Key, driversInClass);
        }
    }
}
