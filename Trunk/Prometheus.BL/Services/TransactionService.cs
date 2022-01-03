using Prometheus.Common;
using Prometheus.Dal.Entities;
using Prometheus.Dal;
using Prometheus.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Prometheus.BL.Interfaces;
using Prometheus.BlockchainAdapter.Models;
using Serilog;

namespace Prometheus.BL.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IPrometheusEntities _entity;
        private readonly IJobDefinitionService _jobDefinitionService;
        private readonly IBlockchainService _blockchainService;
        private readonly IAccountService _accountService;
        private readonly ILogger _logger;

        public TransactionService(IPrometheusEntities entity, IJobDefinitionService jobDefinitionService, IBlockchainService blockchainService, IAccountService accountService, ILogger logger)
        {
            _entity = entity;
            _jobDefinitionService = jobDefinitionService;
            _blockchainService = blockchainService;
            _accountService = accountService;
            _logger = logger;
        }

        public async Task<IResponse<NoValue>> AddToTransaction(TransactionDataModel model)
        {
            var response = new Response<NoValue>();

            var transactions = new List<Transaction>();
            try
            {
                var jobTimeline = _entity.JobTimeline.Find(model.JobId);

                var fromAdapter = jobTimeline.Schedule.JobDefinition.Adapter;
                var toAdapter = jobTimeline.Schedule.JobDefinition.Adapter1;

                var transactionList = model.Data;

                //remove intersecting transactions if job is recurrent
                var newTransactionsResponse = RemoveIntersection(model.Data, jobTimeline);

                if (newTransactionsResponse.Value.Count > 0)
                {
                    foreach (var transaction in newTransactionsResponse.Value)
                    {
                        var transactionTypeResponse = GetTransactionType(transaction);
                        //var accountsResponse = await _accountService.GetAccount(transaction.TransactionAccount, fromAdapter.EnterpriseAdapter.FirstOrDefault().Id, toAdapter.CryptoAdapter.FirstOrDefault().Id);
                        

                        transactions.Add(new Transaction
                        {
                            TransactionId = transaction.TransactionId,
                            AccountId = 1,
                            TransactionAmount = Math.Abs(Convert.ToDecimal(transaction.TransactionAmount)),
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            TransactionTypeId = transactionTypeResponse.Value.Id,
                            TransactionStatusId = (int)Common.Enums.TransactionStatus.Pending,
                            JobTimelineId = model.JobId,
                        });
                    }
                    _entity.Transaction.AddRange(transactions);
                    _entity.SaveChanges();
                }
                response.Status = Common.Enums.StatusEnum.Success;
            }
            catch (Exception ex)
            {
                response.Message = Message.SomethingWentWrong;
                response.Status = Common.Enums.StatusEnum.Error;
                _logger.Information($"TransactionService.AddToTransaction(model: {model})");
                _logger.Error(ex.Message);
            }
            return response;

        }

        public IResponse<List<BlockchainTransactionModel>> GetTransactionsWithoutHash(long jobId)
        {
            var response = new Response<List<BlockchainTransactionModel>>();
            var blockcahinTransactions = new List<BlockchainTransactionModel>();
            try
            {
                var transactions = _entity.Transaction.Where(t => t.JobTimelineId == jobId && t.TransactionHash == null);
                foreach (var transaction in transactions)
                {
                    var accountModel = new AccountModel()
                    {
                        CreditAddress = "ox4",
                        DebitAddress = "ox5"
                    };

                    var blockchainTransaction = new BlockchainTransactionModel()
                    {
                        Id = transaction.Id,
                        Account = accountModel,
                        Statement = (transaction.TransactionTypeId == 1) ? Common.Enums.Statement.Credit : Common.Enums.Statement.Debit,
                        TxStatus = Common.Enums.TxStatus.Pending,
                        Value = transaction.TransactionAmount
                    };
                    blockcahinTransactions.Add(blockchainTransaction);
                }
                response.Value = blockcahinTransactions;
                response.Status = Common.Enums.StatusEnum.Success;
            }
            catch (Exception ex)
            {
                response.Status = Common.Enums.StatusEnum.Error;
                response.Message = Message.SomethingWentWrong;
                _logger.Information($"TransactionService.GetTransactionsWithoutHash(jobId: {jobId})");
                _logger.Error(ex.Message);
            }

            return response;
        }

        public IResponse<List<TransactionViewModel>> GetTransactions(long jobId)
        {
            var response = new Response<List<TransactionViewModel>>
            {
                Value = new List<TransactionViewModel>()
            };

            try
            {
                var transactions = _entity.Transaction.Where(t => t.JobTimelineId == jobId).ToList();

                foreach (var transaction in transactions)
                {
                    response.Value.Add(new TransactionViewModel
                    {
                        TransactionId = transaction.TransactionId,
                        Account = transaction.Account.TransactionAccount,
                        Value = transaction.TransactionAmount,
                        TransactionType = transaction.TransactionType.Name,
                        TransactionHash = transaction.TransactionHash,
                        TransactionStatus = transaction.TransactionStatus.Name
                    });
                }
            }
            catch (Exception ex)
            {
                response.Message = Message.SomethingWentWrong;
                response.Status = Common.Enums.StatusEnum.Error;
                _logger.Information($"TransactionService.GetTransactions(jobId: {jobId})");
                _logger.Error(ex.Message);
            }

            return response;
        }

        public IResponse<NoValue> UpdateTransactions(List<TransactionResponse<BlockchainTransactionModel>> transactionList)
        {
            var response = new Response<NoValue>();
            try
            {
                var transactionIds = transactionList.Where(t => t.Succeeded).Select(t => t.Result.Id);

                var transactions = _entity.Transaction.Where(t => transactionIds.Contains(t.Id)).ToList();

                foreach (var transaction in transactions)
                {
                    transaction.TransactionHash = transactionList.Where(t => t.Result.Id == transaction.Id).Select(h => h.Result.TxHash).FirstOrDefault();
                    transaction.UpdatedAt = DateTime.UtcNow;
                }
                _entity.SaveChanges();
                response.Status = Common.Enums.StatusEnum.Success;
            }
            catch (Exception ex)
            {
                response.Message = Message.SomethingWentWrong;
                response.Status = Common.Enums.StatusEnum.Error;
                _logger.Information($"TransactionService.UpdateTransactions(transactionList: {transactionList})");
                _logger.Error(ex.Message);
            }

            return response;
        }

        public async Task<IResponse<NoValue>> CheckTransactionsStatus()
        {
            var response = new Response<NoValue>();

            try
            {
                var pendingTransactions = _entity.Transaction.Where(t => t.TransactionStatus.Name == Common.Enums.TransactionStatus.Pending.ToString() && t.TransactionHash != null).ToList();
                if (pendingTransactions.Count() > 0)
                {
                    foreach (var transaction in pendingTransactions)
                    {
                        var blockchainTransaction = new BlockchainTransactionModel
                        {
                            TxHash = transaction.TransactionHash
                        };
                        var cryptoAdapterId = transaction.JobTimeline.Schedule.JobDefinition.Adapter1.CryptoAdapter.FirstOrDefault().Id;
                        var getTransactionStatusResponse = await _blockchainService.GetTransactionStatus(blockchainTransaction, (long)cryptoAdapterId);
                        if (getTransactionStatusResponse.Succeeded)
                        {
                            transaction.TransactionStatusId = (int)getTransactionStatusResponse.Result.TxStatus;
                        }
                    }
                }
                _entity.SaveChanges();
            }
            catch (Exception ex)
            {
                response.Message = Message.SomethingWentWrong;
                response.Status = Common.Enums.StatusEnum.Error;
                _logger.Information($"TransactionService.CheckTransactionsStatus()");
                _logger.Error(ex.Message);
            }

            return response;
        }

        #region Helper
        public IResponse<TransactionType> GetTransactionType(GenericModel transaction)
        {
            var response = new Response<TransactionType>();
            var type = new TransactionType();

            try
            {
                if (transaction.TransactionType == null)
                {
                    if (Convert.ToDouble(transaction.TransactionAmount) < 0)
                    {
                        type.Id = (int)Common.Enums.Statement.Credit;

                        response.Status = Common.Enums.StatusEnum.Success;
                        response.Value = type;
                    }
                    else
                    {
                        type.Id = (int)Common.Enums.Statement.Debit;

                        response.Status = Common.Enums.StatusEnum.Success;
                        response.Value = type;
                    }
                }
                else
                {
                    var exist = _entity.TransactionType.Select(t => t.Name.ToLower()).Contains(transaction.TransactionType.ToLower());
                    if (exist)
                    {
                        type.Id = _entity.TransactionType.Where(t => t.Name.ToLower() == transaction.TransactionType.ToLower()).Select(i => i.Id).FirstOrDefault();

                        response.Value = type;
                        response.Status = Common.Enums.StatusEnum.Success;
                    }
                    else
                    {
                        var existInAliasTable = _entity.TransactionTypeAlias.Select(t => t.NormalizedAlias).Contains(transaction.TransactionType.ToLower());
                        if (existInAliasTable)
                        {
                            type.Id = _entity.TransactionTypeAlias.Where(i => i.NormalizedAlias == transaction.TransactionType.ToLower()).Select(t => t.TransactionTypeId).FirstOrDefault();

                            response.Value = type;
                            response.Status = Common.Enums.StatusEnum.Success;
                        }
                        else
                        {
                            type.Id = default(int);

                            response.Value = type;
                            response.Message = Message.SomethingWentWrong;
                            response.Status = Common.Enums.StatusEnum.Error;
                            _logger.Information($"TransactionService.GetTransactionType(transaction: {transaction})");
                            _logger.Error($"Transaction type: {transaction.TransactionType} doesn't exist in alias table.)");

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.Message = Message.SomethingWentWrong;
                response.Status = Common.Enums.StatusEnum.Error;
                _logger.Information($"TransactionService.GetTransactionType(transaction: {transaction})");
                _logger.Error(ex.Message);
            }

            return response;
        }

        public IResponse<List<GenericModel>> RemoveIntersection(List<GenericModel> transactions, JobTimeline jobTimeline)
        {
            var response = new Response<List<GenericModel>>();
            try
            {
                if (jobTimeline.Schedule.RecurrenceRule != null)
                {
                    var transactionsIDs = transactions.Select(t => t.TransactionId).ToList();
                    var intersectingTransactions = _entity.Transaction.Where(t => transactionsIDs.Contains(t.TransactionId) && t.JobTimeline.ScheduleId == jobTimeline.ScheduleId).ToList();
                    transactions.RemoveAll(t => intersectingTransactions.Select(s => s.TransactionId).Contains(t.TransactionId));
                }

                response.Value = transactions;
                response.Status = Common.Enums.StatusEnum.Success;
            }
            catch (Exception ex)
            {
                response.Status = Common.Enums.StatusEnum.Error;
                _logger.Information($"TransactionService.RemoveIntersection(transaction: {transactions}, jobTimeline {jobTimeline.Id})");
                _logger.Error(ex.Message);
                throw;
            }
            return response;
        }
        #endregion 
    }

}
