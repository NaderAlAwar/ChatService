using System;

namespace ChatService.Storage
{
    public class MessageNotFoundException : Exception
    {
        public MessageNotFoundException(string message) : base(message)
        {
        }
    }
}
