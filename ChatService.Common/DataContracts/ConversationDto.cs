using System;

namespace ChatService.DataContracts
{
    public class ConversationDto
    {
        public string Id { get; set; }
        public string[] Participants { get; set; }
        public DateTime LastModifiedDateUtc { get; set; }
    }
}
