using Prometheus.Common.Enums;
using Prometheus.Model.Models;

namespace Prometheus.SolanaAPI.Models
{
    /// <summary>
    /// Model containing information necessary for Solana transaction
    /// </summary>
    public class SolanaTransactionModel
    {
        /// <summary>
        /// Transaction Id
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Transaction amount
        /// </summary>
        public decimal Value { get; set; }
        /// <summary>
        /// Hash code of transaction
        /// </summary>
        public string TxHash { get; set; }
        /// <summary>
        /// Transaction status
        /// </summary>
        public TxStatus TxStatus { get; set; }
    }
}
