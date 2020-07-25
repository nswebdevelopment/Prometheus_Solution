using System.Collections.Generic;
using Prometheus.Common;
using Prometheus.Model.Models.EnterpriseAdapterModel;

namespace Prometheus.BL.Interfaces
{
    public interface IEnterpriseAdapterService
    {
        IResponse<NoValue> Create(EnterpriseAdapterModel model, long userProfileID);
        IResponse<List<EnterpriseAdapterModel>> GetEnterpriseAdapters(long userProfileId);
        IResponse<EnterpriseAdapterModel> GetEnterpriseAdapter(long enterpriseAdapterId);
        IResponse<EnterpriseAdapterModel> UpdateEnterpriseAdapter(EnterpriseAdapterModel model);
        IResponse<NoValue> DeleteEnterpriseAdapter(long enterpriseAdapterId);
        IResponse<NoValue> CreateXML(string fileAsString, long userProfileId);
        EnterpriseAdapterModel GetInitModel();
    }
}
