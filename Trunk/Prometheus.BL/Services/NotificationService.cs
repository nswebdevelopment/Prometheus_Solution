using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Prometheus.BL.Interfaces;
using Prometheus.Common.Enums;
using Prometheus.Common.Hubs;
using Prometheus.Dal;
using Prometheus.Dal.Entities;
using Prometheus.Model.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.BL.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IPrometheusEntities _entity;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly IConfiguration _configuration;

        public NotificationService(IPrometheusEntities entity, IHubContext<NotificationHub> hub, IConfiguration configuration)
        {
            _entity = entity;
            _hub = hub;
            _configuration = configuration;
        }

        public List<NotificationModel> GetNotifications(long userProfileId)
        {
            var notificationListModel = new List<NotificationModel>();

            foreach (var item in _entity.Notification.Where(u => u.UserProfileId == userProfileId))
            {
                var notification = new NotificationModel
                {
                    CreatedAt = item.CreatedAt,
                    Message = item.Message,
                    UpdatedAt = item.UpdatedAt,
                    StatusId = item.StatusId,
                    UserId = item.UserProfileId
                };

                notificationListModel.Add(notification);
                item.StatusId = (int)Statuses.Notified;
                item.UpdatedAt = DateTime.Now;
            }

            _entity.SaveChanges();

            return notificationListModel.OrderByDescending(ca => ca.CreatedAt).Take(10).ToList();
        }

        public long GetNotificationCount(long userProfileId)
        {
            return _entity.Notification.Where(n => n.UserProfileId == userProfileId && n.StatusId == (int)Statuses.Unnotified).Count();
        }

        public void RegisterNotification()
        {
            var connString = _configuration.GetConnectionString("Prometheus");
            string sqlCommand = @"SELECT Message FROM [dbo].[Notification]";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sqlCommand, conn))
                {
                    cmd.Notification = null;
                    var sqlDep = new SqlDependency(cmd);
                    sqlDep.OnChange += new OnChangeEventHandler(SqlDep_OnChange);
                    SqlDependency.Start(connString);
                    cmd.ExecuteReader();
                }
            }
        }

        private void SqlDep_OnChange(object sender, SqlNotificationEventArgs e)
        {            
            if (e.Type == SqlNotificationType.Change && e.Info == SqlNotificationInfo.Insert)
            {
                var sqlDep = sender as SqlDependency;
                sqlDep.OnChange -= new OnChangeEventHandler(SqlDep_OnChange);
                using (var context = new PrometheusEntities(_configuration))
                {
                    var userId = context.Notification.OrderByDescending(ca => ca.CreatedAt).FirstOrDefault().UserId;
                    _hub.Clients.User(userId).SendAsync("ReceiveNotification");
                }
            }
            RegisterNotification();
        }
    }
}
