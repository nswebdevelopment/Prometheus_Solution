using Prometheus.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.BL.Interfaces
{
    public interface INotificationService
    {
        List<NotificationModel> GetNotifications(long userProfileId);
        long GetNotificationCount(long userProfileId);
        void RegisterNotification();
    }
}
