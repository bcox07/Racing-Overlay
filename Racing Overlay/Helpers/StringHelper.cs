using System;

namespace RacingOverlay
{
    public class StringHelper
    {
        public static string GetTimeString(double timeInSeconds, bool includeMilliseconds)
        {
            var timespanInSeconds = TimeSpan.FromSeconds(timeInSeconds);
            var timeStringFormat = @"hh\:mm\:ss\.fff";

            if (Math.Abs(timespanInSeconds.TotalSeconds) >= 3600)
                timeStringFormat = @"hh\:mm\:ss\.fff";
            else if (Math.Abs(timespanInSeconds.TotalSeconds) >= 60)
                timeStringFormat = @"mm\:ss\.fff";
            else
                timeStringFormat = @"ss\.fff";


            if (!includeMilliseconds)
            {
                timeStringFormat = timeStringFormat.Substring(0, timeStringFormat.Length - 5);
            }

            var timeString = timespanInSeconds.ToString(timeStringFormat).TrimStart('h', 'm', 's', '0');
            if (timeInSeconds < 0)
                timeString = $"-{timeString}";
            return timeString;
        }
    }
}
