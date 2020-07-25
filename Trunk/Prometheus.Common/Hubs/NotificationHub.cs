using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Prometheus.Common.Hubs
{
    public class NotificationHub:Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.Caller.SendAsync("ReceiveNotification", message);
        }
    }
}
