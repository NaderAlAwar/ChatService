using System;

namespace ChatService.Utils
{
    public static class DateTimeUtils
    {
        public static long InvertTicks(long ticks)
        {
            return DateTime.MaxValue.Ticks - ticks;
        }
    }
}