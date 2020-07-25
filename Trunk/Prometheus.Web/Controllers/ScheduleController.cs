using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prometheus.BL.Interfaces;
using Prometheus.BL.Services;
using Prometheus.Common.Extensiosns;
using Prometheus.Model.Models;
using Prometheus.Model.Models.ScheduleModel;
using Prometheus.Web.SchedulerRecurrence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;


namespace Prometheus.Web.Controllers
{
    [Authorize]
    public class ScheduleController : Controller
    {
        private readonly IScheduleService _scheduleService;
        private readonly ISelectListService _selectList;

        public ScheduleController(IScheduleService scheduleService, ISelectListService selectList)
        {
            _scheduleService = scheduleService;
            _selectList = selectList;
        }
        
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Get(string startDate, string endDate)
        {
            var start = DateTime.Parse(startDate);
            var end = DateTime.Parse(endDate);
            var result = _scheduleService.GetSchedule((User.Identity.GetUserProfileId() ?? default(long)), start, end);
            if (result.Status == Common.Enums.StatusEnum.Success)
            {
                return Json(result.Value);
            }
            else
            {
                return Json(result.Message);
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            var userProfileId = User.Identity.GetUserProfileId();
            var model = new ScheduleModel
            {
                JobDefinitions = _selectList.GetList<JobDefinitionSelectListModel>(userProfileId ?? default(long))
            };

            return PartialView("_CreatePartial", model);
        }

        [HttpPost]
        public IActionResult Post(ScheduleModel model)
        {
            model.UserProfileId = User.Identity.GetUserProfileId() ?? default(long);

            if (ModelState.IsValid)
            {
                var dates = model.RecurrenceRule != null ? RecurrenceHelper.GetRecurrenceDateTimeCollection(model.RecurrenceRule, model.StartDate) : null;
                var result = _scheduleService.CreateSchedule(model, dates);

                return Json(result);
            }
            
            return PartialView("_CreatePartial", model);
        }


        [HttpGet]
        public IActionResult GetScheduleById(long scheduleId, long jobId)
        {
            var userProfileId = User.Identity.GetUserProfileId();
            var result = _scheduleService.GetScheduleById(scheduleId, jobId);

            if(result.Status == Common.Enums.StatusEnum.Success)
            {
                result.Value.JobDefinitions = _selectList.GetList<JobDefinitionSelectListModel>(userProfileId ?? default(long));
            }
            var model = result.Value;
            return PartialView("_EditPartial", model);
           
        }

        [HttpPost]
        public IActionResult Put(ScheduleModel model)
        {
            model.UserProfileId = User.Identity.GetUserProfileId() ?? default(long);
            if (ModelState.IsValid)
            {
                var dates = model.RecurrenceRule != null ? RecurrenceHelper.GetRecurrenceDateTimeCollection(model.RecurrenceRule, model.StartDate) : null;
                var result = _scheduleService.EditSchedule(model, dates);
                
                return Json(result);
            }
            return PartialView("_EditPartial", model);
        }

        [HttpPost]
        public IActionResult DeleteSchedule(long id)
        {
            var result = _scheduleService.DeleteSchedule(id);
            return Json(result.Message);
        }

        [HttpPost]
        public IActionResult DeleteJob(long id)
        {
            var result = _scheduleService.DeleteJob(id);
            return Json(result);
        }
    }
}
