using System;
using System.Collections.Generic;
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

        public override bool Equals(object obj)
        {
            var message = obj as Message;
            return message != null &&
                   Text == message.Text &&
                   SenderUsername == message.SenderUsername &&
                   DateTime.Compare(UtcTime, message.UtcTime) == 0;
        }

        public override int GetHashCode()
        {
            var hashCode = -1525092123;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Text);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SenderUsername);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(UtcTime.ToString());
            return hashCode;
        }

    }
}
