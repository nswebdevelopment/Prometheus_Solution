using System.Collections.Generic;
using Prometheus.Common.Enums;

namespace Prometheus.Model.Models
{
    public class JobDefinitionPropertyModel
    {
        public AdapterTypeItemEnum AdapterTypeItem { get; set; }

        public List<PropertyModel> FromProperties { get; set; }

        public List<PropertyModel> ToProperties { get; set; }
    }
}
