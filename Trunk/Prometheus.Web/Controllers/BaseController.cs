using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Prometheus.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Prometheus.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly IUserHandler _user;
        public BaseController(IUserHandler user)
        {
            _user = user;
        }
    }
}
