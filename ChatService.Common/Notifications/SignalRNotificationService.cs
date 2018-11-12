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
            this.baseUri = baseUri;
        }

        public Task SendNotification(string user, NotificationPayload payload)
        {
            string uri = $"{baseUri} + /api/Notifications/{user}";
            var jsonPayload = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            httpClient.PostAsync(uri, httpContent);
            return Task.CompletedTask;
        }
    }
}
