using MongoDB.Bson.Serialization.Attributes;
using Prometheus.Model.Models.BitcoinAdapterModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models.LitecoinBlockModel
{
    public class LitecoinBlockModel
    {
        public int BlockNumber { get; set; }
        public DateTime Time { get; set; }
        [BsonIgnore]
        public Guid BlockIdSQL { get; set; }

        public List<LitecoinBlockTransactionModel> TransactionList { get; set; } = new List<LitecoinBlockTransactionModel>();
    }
}
