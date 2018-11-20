using System;

namespace ChatService.Utils
{
    public static class DateTimeUtils
    {
        public static long InvertTicks(long ticks)
        {
            return DateTime.MaxValue.Ticks - ticks;
        }

        public static string FromDateTimeToInvertedString(DateTime utcTime) {
            return InvertTicks(utcTime.Ticks).ToString("d19");
        }
    }
}