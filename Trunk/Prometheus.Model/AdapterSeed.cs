using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Model
{
    public class AdapterSeed
    {
        public Enterpriseadapters[] EnterpriseAdapters { get; set; }
        public Cryptoadapter[] CryptoAdapters { get; set; }
        public Businessadapter[] BusinessAdapters { get; set; }
    }

    public class Cryptoadapter
    {
        public string Direction { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string RpcAddress { get; set; }
        public string RpcPort { get; set; }
        public Property[] Properties { get; set; }
    }

    public class Property
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Businessadapter
    {
        public string Direction { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Filename { get; set; }
    }

    public class Enterpriseadapters
    {
    }


}
