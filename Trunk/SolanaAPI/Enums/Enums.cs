namespace Prometheus.SolanaAPI.Enums
{
    public class Enums
    {
        /// <summary>
        /// Represents status of the transaction
        /// </summary>
        public enum TxStatus
        {
            Fail = 0,
            Success = 1,
            Pending = 2,
            None = 3
        }
    }
}
