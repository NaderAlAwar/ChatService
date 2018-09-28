using System;

namespace ChatService.Storage
{
    public class DuplicateProfileException : Exception
    {
        public DuplicateProfileException(string message) : base(message)
        {
        }
    }
}
