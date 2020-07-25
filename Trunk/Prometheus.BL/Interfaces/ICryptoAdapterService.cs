using System.Collections.Generic;
using System.Threading.Tasks;
using Prometheus.Common;
using Prometheus.Model.Models;

namespace Prometheus.BL.Interfaces
{
    public interface ICryptoAdapterService
    {
        CryptoAdapterModel GetInitModel();
        Task<IResponse<NoValue>> CreateCryptoAdapter(CryptoAdapterModel model, long userProfileID);
        IResponse<List<CryptoAdapterModel>> GetCryptoAdapters(long userProfileId);
        IResponse<CryptoAdapterModel> GetCryptoAdapter(long cryptoAdapterId);
        Task<IResponse<CryptoAdapterModel>> UpdateCryptoAdapter(CryptoAdapterModel model);
        IResponse<NoValue> DeleteCryptoAdapter(long cryptoAdapterId);
    }
}
