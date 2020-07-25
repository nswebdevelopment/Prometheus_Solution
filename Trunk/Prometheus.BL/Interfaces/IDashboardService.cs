using Prometheus.Common;
using Prometheus.Model.Models;

namespace Prometheus.BL.Interfaces
{
    public interface IDashboardService
    {
        IResponse<DashboardModel> GetDashboardData(long userProfileId);
    }
}
