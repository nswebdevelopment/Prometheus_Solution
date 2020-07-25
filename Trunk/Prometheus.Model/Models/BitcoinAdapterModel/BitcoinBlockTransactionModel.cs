using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models.BitcoinAdapterModel
{
    public class BitcoinBlockTransactionModel
    {

        public string TransactionHash { get; set; }
        public List<TransactionInModel> TransactionInputs { get; set; } = new List<TransactionInModel>();
        public List<TransactionOutModel> TransactionOutputs { get; set; } = new List<TransactionOutModel>();
        [BsonIgnore]
        public Guid TransactionIdSQL { get; set; }
        [BsonIgnore]
        public Guid ParentBlockIdSQL { get; set; }
        public decimal TotalOutValue { get; set; }


    }
}
