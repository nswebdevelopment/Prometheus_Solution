using Prometheus.Common;
using Prometheus.Model.Models;
using Prometheus.Model.Models.BitcoinAdapterModel;
using Prometheus.Model.Models.LitecoinBlockModel;
using Prometheus.Model.Models.NeoAdapterModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.BL.Interfaces
{
    public interface IBlockTransactionService
    {
        IResponse<int> GetMaxBlockNumber(long jobId);
        IResponse<NoValue> AddBlocksWithTransactions(long jobId, List<EthereumBlockModel> blocks);
        IResponse<NoValue> AddBlocksWithTransactions(long jobId, List<BitcoinBlockModel> blocks);
        IResponse<NoValue> AddBlocksWithTransactions(long jobId, List<NeoBlockModel> blocks);
        IResponse<NoValue> AddBlocksWithTransactions(long jobId, List<LitecoinBlockModel> blocks);
        IResponse<List<EthereumBlockModel>> GetBlocksWithTransactions(long jobId, int maxBlockNumber);
        IResponse<List<BlockTransactionViewModel>> ShowBlockTransactions(long jobId);

    }
}
