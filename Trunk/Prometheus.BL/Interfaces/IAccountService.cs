using Prometheus.Common;
using Prometheus.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.BL.Interfaces
{
    public interface IAccountService
    {
        Task<IResponse<Account>>GetAccount(string account, long enterpriseAdapterId, long cryptoAdapterId);
    }
}
