using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Prometheus.BL.Interfaces;
using Prometheus.Common.Extensiosns;
using Prometheus.BL.Services;
using Prometheus.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Prometheus.Common.Enums;

namespace Prometheus.Web.Controllers
{
    [Authorize]
    public class CryptoAdapterController : Controller
    {

        private readonly ICryptoAdapterService _cryptoAdapterService;

        public CryptoAdapterController(ICryptoAdapterService cryptoAdapterService)
        {
            _cryptoAdapterService = cryptoAdapterService;
        }

        public IActionResult Index()
        {
            var result = _cryptoAdapterService.GetCryptoAdapters(User.Identity.GetUserProfileId() ?? default(long));

            if (result.Status != Common.Enums.StatusEnum.Success)
            {
                return BadRequest();
            }

            return View(result.Value);
        }

        public IActionResult Create()
        {
            var model = _cryptoAdapterService.GetInitModel();

            return PartialView("_CreatePartial", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CryptoAdapterModel model)
        {
            var userProfileId = User.Identity.GetUserProfileId();

            if (ModelState.IsValid)
            {
                var response = await _cryptoAdapterService.CreateCryptoAdapter(model, (long)userProfileId);
                return Json(response);
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(long Id)
        {
            var response = _cryptoAdapterService.GetCryptoAdapter(Id);

            if (response.Status == StatusEnum.Error)
            {
                return BadRequest();
            }

            return PartialView("_EditPartial", response.Value);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CryptoAdapterModel model)
        {
            if (ModelState.IsValid)
            {
                var response = await _cryptoAdapterService.UpdateCryptoAdapter(model);
                return Json(response);
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult Delete(long cryptoAdapterId)
        {
            var response = _cryptoAdapterService.DeleteCryptoAdapter(cryptoAdapterId);
            
            return Json(response);
        }


        [HttpGet]
        public IActionResult Details(long Id)
        {
            var response = _cryptoAdapterService.GetCryptoAdapter(Id);

            if (response.Status == StatusEnum.Error)
            {
                return BadRequest();
            }

            return PartialView("_DetailsPartial", response.Value);
        }
    }
}
