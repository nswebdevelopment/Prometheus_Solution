 using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prometheus.BL.Interfaces;
using Prometheus.Common.Extensiosns;

namespace Prometheus.Web.Controllers
{
    [Authorize]
    public class JobHistoryController : Controller
    {
        private readonly IJobHistoryService _jobHistoryService;
        private readonly IJobTimelineService _jobTimelineService;

        public JobHistoryController(IJobHistoryService jobHistoryService, IJobTimelineService jobTimelineService)
        {
            _jobHistoryService = jobHistoryService;
            _jobTimelineService = jobTimelineService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ViewAll()
        {
            var result = _jobHistoryService.GetJobHistoryForUser(User.Identity.GetUserProfileId() ?? default(long));
            if (result.Status != Common.Enums.StatusEnum.Success)
            {
                return BadRequest();
            }
            var model = result.Value;
            return PartialView("_ViewAllPartial", model);
        }

        [HttpGet]
        public IActionResult ViewGrouped()
        {
            var result = _jobTimelineService.GetJobTimeline(User.Identity.GetUserProfileId() ?? default(long));
            if (result.Status != Common.Enums.StatusEnum.Success)
            {
                return BadRequest();
            }
            var model = result.Value;
            return PartialView("_ViewGroupedPartial", model);
        }

        [HttpGet]
        public IActionResult HistoryDetails(long Id)
        {
            var result = _jobHistoryService.GetJobHistory(Id);
            if (result.Status != Common.Enums.StatusEnum.Success)
            {
                return BadRequest();
            }
            var model = result.Value;

            return PartialView("_JobHistoryDetailsPartial", model);
        }
    }
}
