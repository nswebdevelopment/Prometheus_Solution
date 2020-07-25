using Prometheus.Common;
using Prometheus.Common.Enums;
using Prometheus.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.BL.Interfaces
{
    public interface IJobDefinitionService
    {
        IResponse<NoValue> CreateJobDefinition(JobDefinitionModel model);
        IResponse<JobDefinitionModel> GetJobDefinition(long jobDefinitionId);
        IResponse<JobDefinitionModel> GetJobDefinitionByJobId(long jobId);
        IResponse<List<JobDefinitionModel>> GetJobDefinitions(long userProfileId);
        IResponse<JobDefinitionModel> UpdateJobDefinition(JobDefinitionModel model);
        IResponse<NoValue> DeleteJobDefinition(long jobDefinitionId);
        IResponse<Config> GetAdapterParameter(long jobDefinitionId);
        List<int> CheckAdapterMapping(long adapterId);
    }
}
