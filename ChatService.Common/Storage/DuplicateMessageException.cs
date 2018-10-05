using System;

namespace ChatService.Storage
{
    public class DuplicateMessageException : Exception
    {
        public DuplicateMessageException(string message) : base(message)
        {
        }
    }
}
