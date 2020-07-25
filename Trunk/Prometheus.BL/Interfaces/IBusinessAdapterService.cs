using Prometheus.Common;
using Prometheus.Common.Enums;
using Prometheus.Model.Models;
using Prometheus.Model.Models.BitcoinAdapterModel;
using Prometheus.Model.Models.LitecoinBlockModel;
using Prometheus.Model.Models.NeoAdapterModel;
using System.Collections.Generic;

namespace Prometheus.BL.Interfaces
{
    public interface IBusinessAdapterService
    {
        IResponse<NoValue> CreateBusinessAdapter(BusinessAdapterModel businessAdapter, long userProfileId);
        IResponse<List<BusinessAdapterModel>> GetBusinessAdapters(long userProfileId);
        IResponse<BusinessAdapterModel> GetBusinessAdapter(long businessAdapterId);
        IResponse<BusinessAdapterModel> UpdateBusinessAdapter(BusinessAdapterModel model);
        IResponse<NoValue> DeleteBusinessAdapter(long businessAdapterId);
        IResponse<NoValue> CreateXlsxFile(List<EthereumBlockModel> list, long jobId);
        IResponse<NoValue> CreateXlsxFile(List<BitcoinBlockModel> list, long jobId);
        IResponse<NoValue> CreateXlsxFile(List<NeoBlockModel> list, long jobId);
        IResponse<NoValue> CreateXlsxFile(List<LitecoinBlockModel> list, long jobId);
        IResponse<BusinessFileModel> GetExcelFile(long jobId);
        IResponse<NoValue> CreateCsvFile(List<EthereumBlockModel> list, long jobId);
    }
}
