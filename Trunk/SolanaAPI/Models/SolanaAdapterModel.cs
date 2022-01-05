using Prometheus.Model.Models;
using Solnet.Wallet;

namespace Prometheus.SolanaAPI.Models
{
    public class SolanaAdapterModel : CryptoAdapterModel
    {
        public Wallet Wallet { get; set; }
    }
}
