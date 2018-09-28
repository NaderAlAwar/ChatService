using System;

namespace ChatService.DataContracts
{
    public class SendMessageDto
    {
        public SendMessageDto(string text, string senderUsername)
        {
            Text = text;
            SenderUsername = senderUsername;
        }

        public string Text { get; }
        public string SenderUsername { get; }
    }
}
