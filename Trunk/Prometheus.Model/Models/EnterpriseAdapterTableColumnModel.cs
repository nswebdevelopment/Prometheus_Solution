using Prometheus.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model.Models
{
    public class EnterpriseAdapterTableColumnModel
    {
        public long? Id { get; set; }

        public long? ParentId { get; set; }

        public string ColumnName { get; set; }

        public DataTypes DataType { get; set; }

        public bool IsForeignKey { get; set; }

        public bool IsPrimaryKey { get; set; }

        public string RelatedTableName { get; set; }

        public PropertyName? PropertyNameId { get; set; }

        public List<EnterpriseAdapterTableColumnModel> Children { get; set; }
    }
}
