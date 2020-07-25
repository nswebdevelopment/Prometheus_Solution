using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models
{
    public class BlockTransactionViewModel
    {
        public long BlockNumber { get; set; }
        public string TransactionHash { get; set; }
        
    }
}
