using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ChatService.DataContracts;
using Newtonsoft.Json;

namespace ChatService.Notifications
{
    public class SignalRNotificationService : INotificationsService
    {
        private readonly HttpClient httpClient;
        private readonly string baseUri;

        public SignalRNotificationService(string baseUri)
        {
            httpClient = new HttpClient();
            this.baseUri = baseUri;
        }

        public void SendNotification(string user, NotificationPayload payload)
        {
            CheckArguments(user, payload);

            string uri = $"{baseUri}/api/Notifications/{user}";
            var jsonPayload = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            httpClient.PostAsync(uri, httpContent);
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
            else if (string.IsNullOrWhiteSpace(payload.conversationId))
            {
                throw new ArgumentNullException(nameof(payload.conversationId));
            }
            else if (string.IsNullOrWhiteSpace(payload.type))
            {
                throw new ArgumentNullException(nameof(payload.type));
            }
            else if (payload.utcTime == null)
            {
                throw new ArgumentNullException(nameof(payload.utcTime));
            }
        }
    }
}
