using MongoDB.Bson.Serialization.Attributes;
using Prometheus.Model.Models.BitcoinAdapterModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models.LitecoinBlockModel
{
    public class LitecoinBlockTransactionModel
    {
        public string TransactionHash { get; set; }
        public List<LitecoinTransactionInModel> TransactionInputs { get; set; } = new List<LitecoinTransactionInModel>();
        public List<LitecoinTransactionOutModel> TransactionOutputs { get; set; } = new List<LitecoinTransactionOutModel>();
        [BsonIgnore]
        public Guid TransactionIdSQL { get; set; }
        [BsonIgnore]
        public Guid ParentBlockIdSQL { get; set; }
        public decimal TotalOutValue { get; set; }
    }
}
