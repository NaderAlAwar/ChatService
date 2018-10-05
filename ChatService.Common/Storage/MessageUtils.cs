using System;
using ChatService.DataContracts;

namespace ChatService.Storage
{
    public static class MessageUtils 
    {
        public static void Validate(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (string.IsNullOrWhiteSpace(message.SenderUsername))
            {
                throw new ArgumentException($"{nameof(message.SenderUsername)} cannot be null or empty");
            }

            if (string.IsNullOrWhiteSpace(message.Text))
            {
                throw new ArgumentException($"{nameof(message.Text)} cannot be null or empty");
            }

            if (message.UtcTime == null || string.IsNullOrWhiteSpace(message.UtcTime.ToString()))
            {
                throw new ArgumentException($"{nameof(message.UtcTime)} cannot be null or empty");
            }
        }
    }
}