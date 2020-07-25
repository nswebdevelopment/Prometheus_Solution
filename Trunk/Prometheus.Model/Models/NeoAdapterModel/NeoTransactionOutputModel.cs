using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models.NeoAdapterModel
{
    public class NeoTransactionOutputModel
    {
        public string Address { get; set; }
        public string Asset { get; set; }
        public decimal Value { get; set; }
        [BsonIgnore]
        public Guid TransactionOutIdSQL { get; set; }
        [BsonIgnore]
        public Guid ParentTransactionIdSQL { get; set; }

    }
}
