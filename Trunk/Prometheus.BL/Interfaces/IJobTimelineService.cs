using Prometheus.Common;
using Prometheus.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.BL.Interfaces
{
    public interface IJobTimelineService
    {
        IResponse<List<JobModel>> GetJobTimeline(long userProfileId);
        IResponse<NoValue> WriteNumberOfTransactions(long jobId, int numberOfTransactions);

    }
}
