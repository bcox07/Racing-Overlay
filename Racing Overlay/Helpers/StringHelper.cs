using System;

namespace IRacing_Standings
{
    public class StringHelper
    {
        public static string GetTimeString(double timeInSeconds, bool includeMilliseconds)
        {
            var timespanInSeconds = TimeSpan.FromSeconds(timeInSeconds);
            var timeStringFormat = @"hh\:mm\:ss\.fff";

            if (timespanInSeconds.TotalSeconds >= 3600)
                timeStringFormat = @"hh\:mm\:ss\.fff";
            else if (timespanInSeconds.TotalSeconds >= 60)
                timeStringFormat = @"mm\:ss\.fff";
            else
                timeStringFormat = @"ss\.fff";


            if (!includeMilliseconds)
            {
                timeStringFormat = timeStringFormat.Substring(0, timeStringFormat.Length - 5);
            }

            return timespanInSeconds.ToString(timeStringFormat).TrimStart('h', 'm', 's', '0');
        }
    }
}
