using System;
using System.Collections.Generic;
using System.Text;

namespace ChatService.DataContracts
{
    public class NotificationPayload
    {
        public NotificationPayload(DateTime utcTime, string type, string conversationId, string[] users)
        {
            this.utcTime = utcTime;
            this.type = type;
            this.conversationId = conversationId;
            this.users = users;
        }
        public DateTime utcTime { get; set; }
        public string type { get; set; }
        public string conversationId { get; set; }
        public string[] users { get; set; }
    }
}
