using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models
{
    public class JobTypeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

    }
    public class JobTypeSelectListModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}
