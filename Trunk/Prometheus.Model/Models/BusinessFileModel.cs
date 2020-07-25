using Prometheus.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models
{
    public class BusinessFileModel
    {
        public byte[] File { get; set; }
        public AdapterTypeItemEnum BusinessAdapterType { get; set; }
        public string FileName { get; set; }

    }
}
