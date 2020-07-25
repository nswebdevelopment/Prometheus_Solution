using Prometheus.Common;
using Prometheus.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.BL.Interfaces
{
    public interface IJobHistoryService
    {
        IResponse<List<JobModel>> GetJobHistory(long Id);
        IResponse<List<JobModel>> GetJobHistoryForUser(long userProfileId);
        IResponse<NoValue> ChangeJobStatus(Common.Enums.JobStatus jobStatus, long Id, int transactionCount = default(int), int blockCount = default(int), bool limitExceeded = default(bool));
    }
}
