using MongoDB.Bson.Serialization.Attributes;
using Prometheus.Model.Models.BitcoinAdapterModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models.LitecoinBlockModel
{ 
    public class LitecoinTransactionInModel
    {
        public string Address { get; set; }
        [BsonIgnore]
        public Guid TransactionInIdSQL { get; set; }
        [BsonIgnore]
        public Guid ParentTransactionIdSQL { get; set; }
    }
}
