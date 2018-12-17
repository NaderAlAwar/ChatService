using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ChatService.DataContracts;
using Polly;

namespace ChatService.Notifications
{
    public class NotificationServiceFaultToleranceDecorator : INotificationService
    {
        private readonly INotificationService notificationService;
        private readonly ISyncPolicy faultTolerancePolicy;

        public NotificationServiceFaultToleranceDecorator(INotificationService notificationService,
            ISyncPolicy faultTolerancePolicy)
        {
            this.notificationService = notificationService;
            this.faultTolerancePolicy = faultTolerancePolicy;
        }
        public Task SendNotificationAsync(NotificationPayload payload)
        {
            return faultTolerancePolicy.Execute(
                async () => await notificationService.SendNotificationAsync(payload)
            );
        }
    }
}
