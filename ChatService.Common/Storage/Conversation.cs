using System;

namespace ChatService.Storage
{
    public class Conversation
    {
        public Conversation(string id, string[] participants, DateTime lastModifiedDateUtc)
        {
            Id = id;
            Participants = participants;
            LastModifiedDateUtc = lastModifiedDateUtc;
        }

        public string Id { get; }
        public string[] Participants { get; }
        public DateTime LastModifiedDateUtc { get; }
    }
}
