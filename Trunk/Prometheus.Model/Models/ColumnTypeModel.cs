using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models
{
    public class ColumnTypeModel
    {

        public EnterpriseAdapterTableColumnModel ParentColumn { get; set; }

        public EnterpriseAdapterTableColumnModel ChildIdColumn { get; set; }

        public EnterpriseAdapterTableColumnModel ChildDescColumn { get; set; }

    }
}
