using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models
{
    //In the future this class will be changed
    public class GenericModel
    {
        public string TransactionId { get; set; }
        public string TransactionAccount { get; set; }
        public string TransactionAmount { get; set; }
        public string TransactionType { get; set; }
    }
}
