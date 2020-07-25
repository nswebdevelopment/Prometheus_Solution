using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models.NeoAdapterModel
{
    public class NeoBlockTransactionModel
    {
        public string TransactionId { get; set; }
        public string TransactionType { get; set; }
        public List<NeoTransactionInputModel> TransactionInputs { get; set; } = new List<NeoTransactionInputModel>();
        public List<NeoTransactionOutputModel> TransactionOutputs { get; set; } = new List<NeoTransactionOutputModel>();
        [BsonIgnore]
        public Guid TransactionIdSQL { get; set; }
        [BsonIgnore]
        public Guid ParentBlockIdSQL { get; set; }
    }
}
