using System;
using System.Runtime.Serialization;

namespace ChatService.Storage
{
    [Serializable]
    internal class ConversationNotFoundException : Exception
    {
        public ConversationNotFoundException(string message) : base(message)
        {
        }

        public ConversationNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ConversationNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}