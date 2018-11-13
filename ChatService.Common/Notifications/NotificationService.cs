using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ChatService.DataContracts;
using Newtonsoft.Json;

namespace ChatService.Notifications
{
    public class NotificationService : INotificationsService
    {
        private readonly HttpClient httpClient;
        private readonly string baseUri;

        public NotificationService(string baseUri)
        {
            httpClient = new HttpClient();
            this.baseUri = baseUri;
        }

        public async Task SendNotificationAsync(string user, NotificationPayload payload)
        {
            CheckArguments(user, payload);

            string uri = $"{baseUri}/api/Notifications/{user}";
            var jsonPayload = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            await httpClient.PostAsync(uri, httpContent);
        }

        private void CheckArguments(string user, NotificationPayload payload)
        {
            if (string.IsNullOrWhiteSpace(user))
            {
                throw new ArgumentNullException(nameof(user));
            }
            else if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }
            else if (string.IsNullOrWhiteSpace(payload.ConversationId))
            {
                throw new ArgumentNullException(nameof(payload.ConversationId));
            }
            else if (string.IsNullOrWhiteSpace(payload.Type))
            {
                throw new ArgumentNullException(nameof(payload.Type));
            }
            else if (payload.UtcTime == null)
            {
                throw new ArgumentNullException(nameof(payload.UtcTime));
            }
        }
    }
}
