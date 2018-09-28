using System;

namespace ChatService.Storage
{
    public class StorageErrorException : Exception
    {
        public StorageErrorException(string message) : base(message)
        {
        }

        public StorageErrorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
