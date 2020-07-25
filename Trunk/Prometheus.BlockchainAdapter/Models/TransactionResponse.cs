using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.BlockchainAdapter.Models
{
    public class TransactionResponse<T>
    {
        //
        // Summary:
        //     Flag indicating whether if the operation succeeded or not.
        public bool Succeeded { get; set; }
        //
        // Summary:
        //     Error message.
        public string Message { get; set; }
        //
        // Summary:
        //     Return result
        public T Result { get; set; }
    }
}
