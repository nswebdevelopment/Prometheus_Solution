using Prometheus.Common.Enums;
using Prometheus.DbAdapter;
using Prometheus.DbAdapter.Queries;


namespace Prometheus
{
    public class Config
    {
        public QueryRead QueryRead { get; set; }
        public QueryWrite QueryWrite { get; set; }
        public ConnStringCreator ConnString { get; set; }
        public AdapterTypeItemEnum Adapter { get; set; }
    }
}