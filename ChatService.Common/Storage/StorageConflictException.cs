using System;

namespace ChatService.Storage
{
    public class StorageConflictException : Exception
    {
        public StorageConflictException(string message) : base(message)
        {
        }
    }
}
