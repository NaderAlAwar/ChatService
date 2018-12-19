using System;
using System.Runtime.Serialization;

namespace ChatService.Storage
{
    [Serializable]
    public class ConversationNotFoundException : Exception
    {
        public ConversationNotFoundException(string message) : base(message)
        {
        }
    }
}