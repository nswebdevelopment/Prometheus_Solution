using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models
{
    public class TransactionViewModel
    {
        public string TransactionId { get; set; }
        public string Account { get; set; }
        public decimal Value { get; set; }
        public string TransactionType { get; set; }
        public string TransactionHash { get; set; }
        public string TransactionStatus { get; set; }


    }
}
