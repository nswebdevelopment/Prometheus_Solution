using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models
{
    public class TransactionDataModel
    {
        public long JobId { get; set; }
        public List<GenericModel> Data{ get; set; }
    }
}
