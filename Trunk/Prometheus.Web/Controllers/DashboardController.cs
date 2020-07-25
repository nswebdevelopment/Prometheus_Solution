using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prometheus.BL.Interfaces;
using Prometheus.Common.Enums;
using Prometheus.Common.Extensiosns;

namespace Prometheus.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var result = _dashboardService.GetDashboardData(User.Identity.GetUserProfileId() ?? default(long));
            if (result.Status != StatusEnum.Success)
            {
                return BadRequest();
            }
            return View(result.Value);
        }
    }
}
