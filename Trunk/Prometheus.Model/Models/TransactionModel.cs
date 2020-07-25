using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models
{
    public class TransactionModel
    {
        public long Id { get; set; }
        public string TransactionId { get; set; }
        public long AccountId { get; set; }
        public decimal TransactionAmount { get; set; }
        public string TransactionAccount { get; set; }
        public long EnterpriseAdapterId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string TransactionHash { get; set; }
        public string TransactionType { get; set; }
        public int? TransactionTypeId { get; set; }
        public int TransactionStatusId { get; set; }
    }
}
