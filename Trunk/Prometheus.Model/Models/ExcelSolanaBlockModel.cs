using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models
{
    public class ExcelSolanaBlockModel
    {
        public int BlockNumber { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Hash { get; set; }
        public decimal Value { get; set; }
        public string Status { get; set; }
    }
}
