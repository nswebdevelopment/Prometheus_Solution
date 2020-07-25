using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models.ExchangesModel
{
    public class BasicInfoModel
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Country { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public int PairsCount { get; set; }
    }    

    public class ExtendedInfoModel : BasicInfoModel
    {
        public List<string> Markets { get; set; }
    }         
}
