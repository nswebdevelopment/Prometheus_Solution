using MongoDB.Bson.Serialization.Attributes;
using Prometheus.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models
{
    public class EthereumBlockModel
    {
        [BsonIgnore]
        public Guid BlockIdSQL { get; set; }
        public int BlockNumber { get; set; }
        public DateTime TimeStamp { get; set; }
        public List<EthereumBlockTransactionModel> BlockTransactions { get; set; }
    }

    public class EthereumBlockTransactionModel
    {
        [BsonIgnore]
        public Guid TransactionIdSQL { get; set; }
        public string Hash { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public decimal Value { get; set; }
        public TxStatus Status { get; set; }
        [BsonIgnore]
        public Guid ParentBlockId { get; set; }

    }
}
