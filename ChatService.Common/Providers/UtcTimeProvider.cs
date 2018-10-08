using System;

namespace ChatService.Providers {
    public class UtcTimeProvider : ITimeProvider {
        public DateTime GetCurrentTimeUtc() {
            return DateTime.UtcNow;
        }
    }
}