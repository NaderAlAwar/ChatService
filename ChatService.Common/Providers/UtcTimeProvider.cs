using System;

namespace ChatService.Providers {
    public class UtcTimeProvider : ITimeProvider {
        public DateTime GetCurrentTime() {
            return DateTime.UtcNow;
        }
    }
}