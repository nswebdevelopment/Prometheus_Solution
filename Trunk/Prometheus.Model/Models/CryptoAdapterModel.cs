using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Prometheus.Common.Enums;

namespace Prometheus.Model.Models
{
    public class CryptoAdapterModel
    {
        public long Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [DisplayName("Node Type")]
        public AdapterTypeItemEnum NodeType { get; set; }

        [Required]
        public DirectionEnum Direction { get; set; }

        [Required]
        [DisplayName("Rpc Address")]

        public string RpcAddr { get; set; }

        [Required]
        [DisplayName("Rpc Port")]

        [Range(1, 65535, ErrorMessage = "Invalid Port.")]
        public ushort? RpcPort { get; set; }
        
        public List<NodeModel> Properties { get; set; }

    }

    public class CryptoAdapterSelectListModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
