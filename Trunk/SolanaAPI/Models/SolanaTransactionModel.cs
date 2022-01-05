using Prometheus.Common.Enums;
using Prometheus.Model.Models;

namespace Prometheus.SolanaAPI.Models
{
    public class SolanaTransactionModel
    {
        public long Id { get; set; }
        public decimal Value { get; set; }
        public string TxHash { get; set; }
        public TxStatus TxStatus { get; set; }
    }
}
