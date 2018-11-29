using System;
using System.Collections.Generic;
using System.Text;

namespace ChatService.Notifications
{
    public class NotificationServiceSettings
    {
        public string ServiceBusConnectionString { get; set; }
        public string QueueName { get; set; }
    }
}
