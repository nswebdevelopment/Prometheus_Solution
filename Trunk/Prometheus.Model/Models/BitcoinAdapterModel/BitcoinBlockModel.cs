using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models.BitcoinAdapterModel
{
    public class BitcoinBlockModel
    {
        public int BlockNumber { get; set; }
        public DateTime Time { get; set; }
        [BsonIgnore]
        public Guid BlockIdSQL { get; set; }

        public List<BitcoinBlockTransactionModel> TransactionList { get; set; } = new List<BitcoinBlockTransactionModel>();
    }
}
