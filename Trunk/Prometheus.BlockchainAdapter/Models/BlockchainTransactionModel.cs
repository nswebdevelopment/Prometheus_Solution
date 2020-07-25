using Prometheus.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.BlockchainAdapter.Models
{
    public class BlockchainTransactionModel
    {
        public long Id { get; set; }
        public AccountModel Account { get; set; }
        public decimal Value { get; set; }
        public Statement Statement { get; set; }
        public string TxHash { get; set; }

    }
}
