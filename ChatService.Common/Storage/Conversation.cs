using System;
using System.Collections.Generic;

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

        public override bool Equals(object obj)
        {
            var conversation = obj as Conversation;
            return conversation != null &&
                   Id == conversation.Id &&
                   string.Join(",", Participants) == string.Join(",", conversation.Participants) &&
                   LastModifiedDateUtc == conversation.LastModifiedDateUtc;
        }
    }
}
