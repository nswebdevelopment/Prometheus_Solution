using MongoDB.Bson.Serialization.Attributes;
using static Prometheus.SolanaAPI.Enums.Enums;

namespace Prometheus.SolanaAPI.Models
{
        public class SolanaBlockModel
        {
            [BsonIgnore]
            public Guid BlockIdSQL { get; set; }
            public int BlockNumber { get; set; }
            public DateTime TimeStamp { get; set; }
            public List<SolanaBlockTransactionModel> BlockTransactions { get; set; }
        }

        public class SolanaBlockTransactionModel
        {
            [BsonIgnore]
            public Guid TransactionIdSQL { get; set; }
            public string Hash { get; set; }
            public decimal Value { get; set; }
            public TxStatus Status { get; set; }
            [BsonIgnore]
            public Guid ParentBlockId { get; set; }

        }
    }

