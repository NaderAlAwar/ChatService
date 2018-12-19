using System;
using System.Runtime.Serialization;

namespace ChatService.Storage
{
    [Serializable]
    public class MessageNotFoundException : Exception
    {
        public MessageNotFoundException(string message) : base(message)
        {
        }
    }
}