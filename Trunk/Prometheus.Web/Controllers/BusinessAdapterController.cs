using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prometheus.BL.Interfaces;
using Prometheus.Common.Enums;
using Prometheus.Common.Extensiosns;
using Prometheus.Model.Models;

namespace Prometheus.Web.Controllers
{
    [Authorize]
    public class BusinessAdapterController : Controller
    {
        private readonly IBusinessAdapterService _businessAdapterService;

        public BusinessAdapterController(IBusinessAdapterService businessAdapterService)
        {
            _businessAdapterService = businessAdapterService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var result = _businessAdapterService.GetBusinessAdapters(User.Identity.GetUserProfileId() ?? default(long));

            if (result.Status != StatusEnum.Success)
            {
                return BadRequest();
            }

            return View(result.Value);
        }

        [HttpGet]
        public IActionResult Create()
        {
            //var model = _businessAdapterService.GetInitModel();

            return PartialView("_CreatePartial"/*, model*/);
        }

        [HttpPost]
        public IActionResult Create(BusinessAdapterModel model)
        {
            var userProfileId = User.Identity.GetUserProfileId();

            if (ModelState.IsValid)
            {
                var response = _businessAdapterService.CreateBusinessAdapter(model, (long)userProfileId);
                return Json(response);
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(long Id)
        {
            var response = _businessAdapterService.GetBusinessAdapter(Id);

            if (response.Status == StatusEnum.Error)
            {
                return BadRequest();
            }

            var model = response.Value;

            return PartialView("_EditPartial", model);
        }

        [HttpPost]
        public IActionResult Edit(BusinessAdapterModel model)
        {
            if (ModelState.IsValid)
            {
                var response = _businessAdapterService.UpdateBusinessAdapter(model);
                return Json(response);
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(long businessAdapterId)
        {
            var response = _businessAdapterService.DeleteBusinessAdapter(businessAdapterId);

            return Json(response);
        }


        [HttpGet]
        public IActionResult Details(long? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }

            var result = _businessAdapterService.GetBusinessAdapter(Convert.ToInt64(Id));
            var model = result.Value;

            return PartialView("_DetailsPartial", model);
        }
    }
}
