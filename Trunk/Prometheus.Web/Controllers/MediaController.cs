using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Prometheus.Media;
using Prometheus.Media.SocialMedia;
using Prometheus.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Prometheus.Web.Controllers
{
    [Authorize]
    public class MediaController : Controller
    {
        private readonly Lazy<IRssMedia> _rssClient;
        private readonly IFacebookService _facebookService;

        public MediaController(Lazy<IRssMedia> rssClient, IFacebookService facebookService)
        {
            _rssClient = rssClient;
            _facebookService = facebookService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetMediaReport()
        {
            return Json(_rssClient.Value.MediaReport());
            //return Json(new List<int>() { 5, 4, 15, 1, 2, 0, 0, 11 });
        }

        [HttpGet]
        public IActionResult GetSocialMediaReport()
        {
            //return Json(await _facebookService.SocialMediaReport());
            return Json(new List<int>() { 20, 15, 2, 1, 1, 12, 14, 6 });
        }
    }
}
