using System;

namespace ChatService.Providers {
    public interface ITimeProvider {
        DateTime GetCurrentTime();
    }
}