using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prometheus.BL.Interfaces;
using Prometheus.Common.Extensiosns;
using Prometheus.Model.Models;
using Prometheus.Model.Models.EnterpriseAdapterModel;
using Prometheus.Common.Enums;
using System;
using Prometheus.Common;

namespace Prometheus.Web.Controllers
{
    [Authorize]
    public class JobDefinitionController : Controller
    {
        private readonly ISelectListService _selectList;
        private readonly IJobDefinitionService _jobDefinitionService;

        public JobDefinitionController(ISelectListService selectList, IJobDefinitionService jobDefinitionService)
        {
            _selectList = selectList;
            _jobDefinitionService = jobDefinitionService;
        }
        [HttpGet]
        public IActionResult Index()
        {
            var result = _jobDefinitionService.GetJobDefinitions(User.Identity.GetUserProfileId() ?? default(long));
            if(result.Status != StatusEnum.Success)
            {
                return BadRequest();
            }
            return View(result.Value);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var userProfileId = User.Identity.GetUserProfileId();
            var model = new JobDefinitionModel
            {
                Properties = _selectList.GetPropertiesList<JobDefinitionPropertyModel>(userProfileId ?? default(long)),
                EnterpriseAdapters = _selectList.GetList<EnterpriseAdapterSelectListModel>(userProfileId ?? default(long)),
                CryptoAdapters = _selectList.GetList<CryptoAdapterSelectListModel>(userProfileId ?? default(long)),
                BusinessAdapters = _selectList.GetList<BusinessAdapterSelectListModel>(userProfileId ?? default(long)),
            };
            
            return PartialView("_CreatePartial", model);
        }
        
        [HttpPost]
        public IActionResult Create(JobDefinitionModel model)
        {
            if (ModelState.IsValid)
            {
                model.UserProfileId = User.Identity.GetUserProfileId() ?? default(long);
                var response = _jobDefinitionService.CreateJobDefinition(model);
                return Json(response);
            }
            else
            {
                var response = new Response<NoValue>
                {
                    Status = StatusEnum.Error,
                    Message = Message.SomethingWentWrong
                };
                return Json(response);
            }
        }

        [HttpGet]
        public IActionResult Edit(long Id)
        {
            var response = _jobDefinitionService.GetJobDefinition(Id);

            var userProfileId = User.Identity.GetUserProfileId();

            response.Value.Adapters = _selectList.GetList<AdapterSelectListModel>(userProfileId ?? default(long));

            if (response.Status == StatusEnum.Error)
            {
                return BadRequest();
            }

            var model = response.Value;
            return PartialView("_EditPartial", model);
        }

        [HttpPost]
        public IActionResult Edit(JobDefinitionModel model)
        {
            if (ModelState.IsValid)
            {
                model.UserProfileId = User.Identity.GetUserProfileId() ?? default(long);
                var response = _jobDefinitionService.UpdateJobDefinition(model);
                
                return Json(response);
            }
            else
            {
                var response = new Response<NoValue>
                {
                    Status = StatusEnum.Error,
                    Message = Message.SomethingWentWrong
                };
                return Json(response);
            } 
        }

        [HttpPost]
        public IActionResult Delete(long jobDefinitionId)
        {
            var response = _jobDefinitionService.DeleteJobDefinition(jobDefinitionId);

            return Json(response);
        }
        
        [HttpGet]
        public IActionResult Details(long? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            
            var result = _jobDefinitionService.GetJobDefinition(Convert.ToInt64(Id));
            var userProfileId = User.Identity.GetUserProfileId();

            result.Value.Adapters = _selectList.GetList<AdapterSelectListModel>(userProfileId ?? default(long));
            var model = result.Value;
            
            return PartialView("_DetailsPartial", model);
        }

        [HttpGet]
        public IActionResult CheckAdapterMapping(long adapterId)
        {
            var userProfileId = User.Identity.GetUserProfileId();
            var typeItems = _jobDefinitionService.CheckAdapterMapping(adapterId);
            
            var adapters = new JobDefinitionModel
            {
                EnterpriseAdapters = _selectList.GetMappedList<EnterpriseAdapterSelectListModel>(userProfileId ?? default(long), typeItems),
                CryptoAdapters = _selectList.GetMappedList<CryptoAdapterSelectListModel>(userProfileId ?? default(long), typeItems),
                BusinessAdapters = _selectList.GetMappedList<BusinessAdapterSelectListModel>(userProfileId ?? default(long), typeItems)
            };
            
            return Json(adapters);
        }
    }
}
