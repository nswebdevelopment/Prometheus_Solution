using Prometheus.Common;
using Prometheus.Model.Models;
using Prometheus.Model.Models.BitcoinAdapterModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.BL.Interfaces
{
    public interface IDbAdapterService
    {
        IResponse<List<GenericModel>> GetAdapterData(long jobDefinitionId);

        IResponse<NoValue> SendToRelationalDb<T>(long jobId, List<T> blocks);

        IResponse<NoValue> SendToMongoDb<T>(long enterpriseId, List<T> blocks);
    }
}
