using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.DbAdapter
{
    public class ConnStringCreator
    {
        private const int Timeout = 10;

        public string Server { get; set; }
        public string Port { get; set; }
        public string Database { get; set; }
        public string Uid { get; set; }
        public string Pwd { get; set; }

        public string MySQLConnString
        {
            get
            {
                return $"server={Server};port={Port};database={Database};uid={Uid};pwd={Pwd};connection timeout={Timeout}";
            }
        }

        public string MSSQLConnString
        {
            get
            {
                return $"server={Server},{Port};database={Database};user id={Uid};password={Pwd};connection timeout={Timeout}";
            }
        }
        public string OracleConnString
        {
            get
            {
                return $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={Server})(PORT={Port}))(CONNECT_DATA=(SERVICE_NAME={Database})));User Id={Uid};Password={Pwd};connection timeout={Timeout}";
            }
        }
        public string MongoDbConnString
        {
            get
            {
                return $"mongodb://{Uid}:{Pwd}@{Server}:{Port}/{Database}?connectTimeoutMS={Timeout * 1000}";
            }
        }
    }   
}
