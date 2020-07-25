using MongoDB.Bson.Serialization.Attributes;
using Prometheus.Model.Models.BitcoinAdapterModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models.LitecoinBlockModel
{
    public class LitecoinTransactionOutModel
    {
        public string Address { get; set; }
        public decimal Value { get; set; }
        [BsonIgnore]
        public Guid TransactionOutIdSQL { get; set; }
        [BsonIgnore]
        public Guid ParentTransactionIdSQL { get; set; }
    }
}
