using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ChatService.DataContracts;

namespace ChatService.Notifications
{
    public interface INotificationsService
    {
        Task SendNotification(string user, NotificationPayload payload);
    }
}
