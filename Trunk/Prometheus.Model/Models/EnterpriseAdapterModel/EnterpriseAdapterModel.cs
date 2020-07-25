using Prometheus.Common.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prometheus.Model.Models.EnterpriseAdapterModel
{
    public class EnterpriseAdapterModel
    {
        public long Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string ServerIP { get; set; }

        [Required]
        public string DatabaseName { get; set; }

        [Required]
        public DirectionEnum Direction { get; set; }

        [Required]
        public AdapterTypeItemEnum EnterpriseAdapter { get; set; }

        [Required]
        [Range(1, 65535, ErrorMessage = "Invalid Port!")]
        public int? Port { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string Username { get; set; }

        public string ParentTable { get; set; }

        public List<EnterpriseAdapterTableColumnModel> Columns { get; set; }

        public List<EnterpriseModel> Properties { get; set; }
    }
    public class EnterpriseAdapterSelectListModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
