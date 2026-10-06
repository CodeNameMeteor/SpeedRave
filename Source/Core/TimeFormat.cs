using System;
using System.Globalization;

namespace SpeedRave.Core
{
    // Pure time formatting (no Unity dependencies, so it can be unit tested).
    public static class TimeFormat
    {
        // On-screen timer: "mm:ss.ff", or "h:mm:ss.ff" from one hour.
        public static string Timer(TimeSpan time)
        {
            if (time < TimeSpan.Zero) time = TimeSpan.Zero;
            int hundredths = time.Milliseconds / 10;

            if (time.TotalHours >= 1)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}:{1:D2}:{2:D2}.{3:D2}", (int)time.TotalHours, time.Minutes, time.Seconds, hundredths);
            }
            return string.Format(CultureInfo.InvariantCulture, "{0:D2}:{1:D2}.{2:D2}", time.Minutes, time.Seconds, hundredths);
        }

        // LiveSplit Server's setgametime argument: "h:mm:ss.ff".
        public static string LiveSplit(TimeSpan time)
        {
            if (time < TimeSpan.Zero) time = TimeSpan.Zero;
            return string.Format(CultureInfo.InvariantCulture, "{0}:{1:00}:{2:00}.{3:00}",
                (int)time.TotalHours, time.Minutes, time.Seconds, time.Milliseconds / 10);
        }
    }
}
