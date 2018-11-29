using System;
using System.Collections.Generic;
using System.Text;

namespace ChatService.DataContracts
{
    public class NotificationPayload
    {
        public NotificationPayload(DateTime utcTime, string type, string conversationId, string[] users)
        {
            UtcTime = utcTime;
            Type = type;
            ConversationId = conversationId;
            Users = users;
        }
        public DateTime UtcTime { get; set; }
        public string Type { get; set; }
        public string ConversationId { get; set; }
        public string[] Users { get; set; }
    }
}
