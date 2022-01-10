using MongoDB.Bson.Serialization.Attributes;
using static Prometheus.SolanaAPI.Enums.Enums;

namespace Prometheus.SolanaAPI.Models
{
    /// <summary>
    /// Model containing information about the Solana block
    /// </summary>
    public class SolanaBlockModel
    {
        /// <summary>
        /// Block Id
        /// </summary>
        [BsonIgnore]
        public Guid BlockIdSQL { get; set; }
        /// <summary>
        /// Number of the block
        /// </summary>
        public int BlockNumber { get; set; }
        /// <summary>
        /// Time stamp
        /// </summary>
        public DateTime TimeStamp { get; set; }
        /// <summary>
        /// List of transactions
        /// </summary>
        public List<SolanaBlockTransactionModel> BlockTransactions { get; set; }
    }

    /// <summary>
    /// Model containing information used for Solana transactions
    /// </summary>
    public class SolanaBlockTransactionModel
    {
        /// <summary>
        /// Transaction id
        /// </summary>
        [BsonIgnore]
        public Guid TransactionIdSQL { get; set; }
        /// <summary>
        /// Transaction hash
        /// </summary>
        public string Hash { get; set; }
        /// <summary>
        /// Transaction amount
        /// </summary>
        public decimal Value { get; set; }
        /// <summary>
        /// Transaction status
        /// </summary>
        public TxStatus Status { get; set; }
        /// <summary>
        /// Id of the parent block 
        /// </summary>
        [BsonIgnore]
        public Guid ParentBlockId { get; set; }

    }
}

