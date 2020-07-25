using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prometheus.BL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prometheus.Common.Extensiosns;
using Prometheus.Model.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Prometheus.Model.Models.EnterpriseAdapterModel;
using Prometheus.Common.Enums;

namespace Prometheus.Web.Controllers
{
    [Authorize]
    public class EnterpriseAdapterController : Controller
    {
        private readonly IEnterpriseAdapterService _enterpriseAdapterService;

        public EnterpriseAdapterController(IEnterpriseAdapterService enterpriseAdapterService)
        {
            _enterpriseAdapterService = enterpriseAdapterService;
        }


        public IActionResult Index()
        {
            var result = _enterpriseAdapterService.GetEnterpriseAdapters(User.Identity.GetUserProfileId() ?? default(long));

            if (result.Status != StatusEnum.Success)
            {
                return BadRequest();
            }
            return View(result.Value);
        }


        [HttpGet]
        public IActionResult Edit(long id)
        {
            var result = _enterpriseAdapterService.GetEnterpriseAdapter(id);
            if (result.Status == StatusEnum.Error)
            {
                return BadRequest();
            }
            return View(result.Value);

        }

        [HttpPost]
        public IActionResult Edit(EnterpriseAdapterModel model)
        {         
            if (ModelState.IsValid) {

                var result = _enterpriseAdapterService.UpdateEnterpriseAdapter(model);
                return Json(result);
            }
            return View(model);
        }


        public IActionResult Create()
        {
            var model = _enterpriseAdapterService.GetInitModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult Create(EnterpriseAdapterModel model)
        {
            var userProfileId = User.Identity.GetUserProfileId();
            if (ModelState.IsValid)
            {
                var result = _enterpriseAdapterService.Create(model, (long)userProfileId);
                return Json(result);
            }
            return View(model);
        }


        [HttpPost]
        public IActionResult Delete(long enterpriseAdapterId)
        {
            var response = _enterpriseAdapterService.DeleteEnterpriseAdapter(enterpriseAdapterId);

            return Json(response);
        }

        [HttpGet]
        public IActionResult Details(long? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }

            var result = _enterpriseAdapterService.GetEnterpriseAdapter(Convert.ToInt64(Id));

            return PartialView("_DetailsPartial", result.Value);
        }

        [HttpGet]
        public IActionResult CreateXML()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateXMLPost()
        {
            var userProfileId = User.Identity.GetUserProfileId();

            var fileUpload = Request.Form.Files;
            var file = fileUpload.GetFile(fileUpload[0].Name);

            var fileAsString = String.Empty;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                fileAsString = reader.ReadToEnd();
            }

            if (string.IsNullOrEmpty(fileAsString) || userProfileId == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var response = _enterpriseAdapterService.CreateXML(fileAsString, (long)userProfileId);
            return Json(response);
        }
    }
}

