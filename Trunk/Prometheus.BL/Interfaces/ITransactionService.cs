using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prometheus.BlockchainAdapter.Models;
using Prometheus.Common;
using Prometheus.Model.Models;

namespace Prometheus.BL.Interfaces
{
    public interface ITransactionService
    {
        Task<IResponse<NoValue>> AddToTransaction(TransactionDataModel model);
        IResponse<List<BlockchainTransactionModel>> GetTransactionsWithoutHash(long jobId);
        IResponse<NoValue> UpdateTransactions(List<TransactionResponse<BlockchainTransactionModel>> transactionList);
        IResponse<List<TransactionViewModel>> GetTransactions(long jobDefinitionId);
        Task<IResponse<NoValue>> CheckTransactionsStatus();
    }
}
