using System;

namespace ChatService.DataContracts
{
    public class ListMessagesItemDto
    {
        public ListMessagesItemDto(string text, string senderUsername, DateTime utcTime)
        {
            Text = text;
            SenderUsername = senderUsername;
            UtcTime = utcTime;
        }

        public string Text { get; }
        public string SenderUsername { get; }
        public DateTime UtcTime { get; }
    }
}