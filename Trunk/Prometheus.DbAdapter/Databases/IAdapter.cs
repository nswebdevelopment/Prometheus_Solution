using Prometheus.DbAdapter.Queries;
using Prometheus.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prometheus.Common;


namespace Prometheus.DbAdapter.Databases
{
    public interface IAdapter
    {
        Response<List<GenericModel>> ConnectAndRead(QueryRead query, string connString);
        Response<NoValue> ConnectAndWrite(QueryWrite query, string connString);
        Response<NoValue> TestConnectivity(string connString, string parentTableName = default(String));

    }
}
