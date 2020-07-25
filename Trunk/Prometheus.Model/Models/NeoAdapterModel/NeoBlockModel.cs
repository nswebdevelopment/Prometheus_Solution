using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models.NeoAdapterModel
{
    public class NeoBlockModel
    {
        public int BlockNumber { get; set; }
        public DateTime Time { get; set; }
        [BsonIgnore]
        public Guid BlockIdSQL { get; set; }

        public List<NeoBlockTransactionModel> TransactionList { get; set; } = new List<NeoBlockTransactionModel>();
    }
}
