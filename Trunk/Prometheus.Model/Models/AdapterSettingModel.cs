using Prometheus.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models
{
    public class AdapterSettingModel
    {
        public long Id { get; set; }
        public string ServerIP { get; set; }
        public string DatabaseName { get; set; }

        [Required]
        public Adapter Adapter { get; set; }

        [Required]
        [Range(1, 65535, ErrorMessage = "Invalid Port!")]
        public int? Port { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string ParentTable { get; set; }

        public List<AdapterTableColumnModel> Columns { get; set; }
    }

    public class AdapterSettingSelectListModel
    {
        public long Id { get; set; }
        public string Desc { get; set; }
    }

}
