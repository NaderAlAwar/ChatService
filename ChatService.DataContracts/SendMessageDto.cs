using System;
using System.Collections.Generic;

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

        public override bool Equals(object obj)
        {
            var dto = obj as SendMessageDto;
            return dto != null &&
                   Text == dto.Text &&
                   SenderUsername == dto.SenderUsername;
        }

        public override int GetHashCode()
        {
            var hashCode = 928530866;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Text);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SenderUsername);
            return hashCode;
        }
    }
}
