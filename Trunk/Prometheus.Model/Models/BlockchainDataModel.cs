using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models
{
    public class BlockchainDataModel
    {
        public long JobId { get; set; }
        public List<BlockchainTransactionModel> Transactions { get; set; }

    }
}
