using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Prometheus.BL.Interfaces;
using Prometheus.BL.Services;
using Prometheus.Common.Enums;
using Prometheus.Model.Models.ExchangesModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Prometheus.Web.Controllers
{
    [Authorize]
    public class ExchangeController : Controller
    {
        private readonly IExchangesService _exchangesService;        

        public ExchangeController(IExchangesService exchangesService)
        {
            _exchangesService = exchangesService;            
        }
        [Route("/exchanges/overview")]
        public IActionResult Index()
        {
            var model = _exchangesService.GetAll();
            return View(model);
        }
        [Route("/exchanges/{name}")]
        public IActionResult Details(AvailableExchanges name)
        {
            var model = _exchangesService.GetDetails(name);            
            return View(model);
        }

        [Route("/exchanges/settings")]
        public IActionResult Settings()
        {
            var model = _exchangesService.GetAll();
            return View(model);
        }        

        [HttpPost]
        public JsonResult SaveSymbols(AvailableExchanges name)
        {
            var result = _exchangesService.SaveSymbolsForExchange(name);
            return Json(result);
        }

        [HttpPost]
        public JsonResult DeleteSymbols(AvailableExchanges name)
        {
            var result = _exchangesService.DeleteAllSymbolsForExchange(name);
            return Json(result);
        }

        [HttpGet]
        public IList<TickerModel> Overview(AvailableExchanges exchangeName, string market)
        {
            var result = _exchangesService.GetOverview(exchangeName, market);
            return result;
        }
    }
}
