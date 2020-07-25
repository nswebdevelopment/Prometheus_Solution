using Microsoft.AspNetCore.Mvc;
using Prometheus.BL.Interfaces;
using Prometheus.Common.Extensiosns;

namespace Prometheus.Web.Controllers
{
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public IActionResult GetNotifications()
        {
            var response = _notificationService.GetNotifications(User.Identity.GetUserProfileId() ?? default(long));
            return Json(response);
        }
    }
}