using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models.BitcoinAdapterModel
{
    public class TransactionInModel
    {
        public string Address { get; set; }
        [BsonIgnore]
        public Guid TransactionInIdSQL { get; set; }
        [BsonIgnore]
        public Guid ParentTransactionIdSQL { get; set; }


    }
}
