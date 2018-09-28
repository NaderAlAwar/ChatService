using System;
using Newtonsoft.Json;

namespace ChatService.Storage
{
    public class Message
    {
        public Message(string text, string senderUsername, DateTime utcTime)
        {
            Text = text;
            SenderUsername = senderUsername;
            UtcTime = utcTime;
        }

        public string Text { get; }
        public string SenderUsername { get; }
        public DateTime UtcTime { get; }

        [JsonIgnore] public DateTime LocalTime => UtcTime.ToLocalTime();
    }
}
