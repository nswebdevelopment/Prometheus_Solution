
using Microsoft.Extensions.Configuration;
using Prometheus.Dal.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Dal
{
    public partial class PrometheusEntities : IPrometheusEntities
    {
        public PrometheusEntities(IConfiguration config) : base(config["ConnectionStrings:PrometheusEntities"])
        {        
        }
    }
}
