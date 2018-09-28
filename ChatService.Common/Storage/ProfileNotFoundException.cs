using System;

namespace ChatService.Storage
{
    public class ProfileNotFoundException : Exception
    {
        public ProfileNotFoundException(string message) : base(message)
        {
        }
    }
}
