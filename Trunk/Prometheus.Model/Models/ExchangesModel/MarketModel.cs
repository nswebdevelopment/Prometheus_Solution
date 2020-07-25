using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models.ExchangesModel
{
    public class MarketModel
    {
        public string Name { get; set; }
        public List<string> Pairs { get; set; }
    }
}
