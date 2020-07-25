using Prometheus.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models
{
    public class NodeModel
    {
        public CryptoAdapterType NodeType { get; set; }

        public List<PropertyModel> SourceProperties { get; set; }

        public List<PropertyModel> DestinationProperties { get; set; }
    }
}
