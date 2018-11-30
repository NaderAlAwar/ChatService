using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ChatService.DataContracts;
using Microsoft.Extensions.Logging.Metrics;

namespace ChatService.Notifications
{
    public class NotificationServiceMetricsDecorator : INotificationService
    {
        private readonly INotificationService notificationService;
        private readonly AggregateMetric sendNotificationMetric;

        public NotificationServiceMetricsDecorator(INotificationService notificationService, IMetricsClient metricsClient)
        {
            this.notificationService = notificationService;

            sendNotificationMetric = metricsClient.CreateAggregateMetric("SendNotificationTime");
        }

        public Task SendNotificationAsync(NotificationPayload payload)
        {
            return sendNotificationMetric.TrackTime(() => notificationService.SendNotificationAsync(payload));
        }
    }
}
