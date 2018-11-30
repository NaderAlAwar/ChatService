using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ChatService.DataContracts;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace ChatService.Notifications
{
    public class ServiceBusNotificationServiceClient : INotificationService
    {
        private readonly IQueueClient queueClient;

        public ServiceBusNotificationServiceClient(IQueueClient queueClient)
        {
            this.queueClient = queueClient;
        }

        public async Task SendNotificationAsync(NotificationPayload payload)
        {
            CheckArguments(payload);

            var jsonPayload = JsonConvert.SerializeObject(payload);
            var message = new Message(Encoding.UTF8.GetBytes(jsonPayload));
            await queueClient.SendAsync(message);
        }

        private void CheckArguments(NotificationPayload payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (payload.Users == null || payload.Users.Length == 0)
            {
                throw new ArgumentNullException(nameof(payload.Users));
            }

            foreach (var username in payload.Users)
            {
                if (string.IsNullOrEmpty(username))
                {
                    throw new ArgumentNullException(nameof(username));
                }
            }

            if (string.IsNullOrWhiteSpace(payload.ConversationId))
            {
                throw new ArgumentNullException(nameof(payload.ConversationId));
            }

            if (string.IsNullOrWhiteSpace(payload.Type))
            {
                throw new ArgumentNullException(nameof(payload.Type));
            }

            if (payload.UtcTime == null)
            {
                throw new ArgumentNullException(nameof(payload.UtcTime));
            }
        }
    }
}
