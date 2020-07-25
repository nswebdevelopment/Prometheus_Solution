using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models.EnterpriseAdapterModel
{
    public class ParentTableModel
    {
        public string TableName { get; set; }
        public List<ColumnModel> Columns { get; set; }
        public string PrimaryKey { get; set; }
        public Dictionary<string, string> ForeignKeys { get; set; }
    }
}
