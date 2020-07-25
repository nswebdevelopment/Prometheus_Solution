using Prometheus.BlockchainAdapter.Models;
using Prometheus.Common;
using Prometheus.Common.Enums;
using Prometheus.Model.Models;
using Prometheus.Model.Models.BitcoinAdapterModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.BL.Interfaces
{
    public interface IBlockchainService
    {
        Task<Response<List<TransactionResponse<BlockchainTransactionModel>>>> SendToBlockchain(BlockchainDataModel blockchainDataModel);
        Task<IResponse<AccountModel>> NewAccount (long cryptoAdapterId);
        Task<TransactionResponse<BlockchainTransactionModel>> GetTransactionStatus(BlockchainTransactionModel transaction, long cryptoAdapterId);
        Task<Response<int>> GetCurrentBlockNumber(long cryptoAdapterId, Common.Enums.AdapterTypeItemEnum adapterType);
        Task<Response<List<T>>> GetBlocksWithTransactions<T>(long cryptoAdapterId, int fromBlock, int toBlock, string address);
    }
}   
