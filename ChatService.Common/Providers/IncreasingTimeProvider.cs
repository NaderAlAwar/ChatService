using System;

namespace ChatService.Providers {
    public class IncreasingTimeProvider : ITimeProvider {
        private int offset = 0;
        public DateTime GetCurrentTime() {
            return DateTime.UtcNow.AddSeconds(offset++);
        }
    }
}