using Prometheus.BL.Interfaces;
using Prometheus.Common;
using Prometheus.Dal;
using Prometheus.Dal.Entities;
using Prometheus.Model.Models;
using Prometheus.Model.Models.BitcoinAdapterModel;
using Prometheus.Model.Models.LitecoinBlockModel;
using Prometheus.Model.Models.NeoAdapterModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.BL.Services
{
    public class BlockTransactionService : IBlockTransactionService
    {
        private readonly IJobDefinitionService _jobDefinitionService;
        private readonly IPrometheusEntities _entity;
        private readonly ILogger _logger;

        public BlockTransactionService(IJobDefinitionService jobDefinitionService, IPrometheusEntities entity, ILogger logger)
        {
            _jobDefinitionService = jobDefinitionService;
            _entity = entity;
            _logger = logger;
        }

        public IResponse<NoValue> AddBlocksWithTransactions(long jobId, List<EthereumBlockModel> blocks)
        {
            var response = new Response<NoValue>();

            try
            {
                var stopwatch = Stopwatch.StartNew();

                foreach (var block in blocks)
                {
                    Block blockDb = new Block
                    {
                        BlockNumber = block.BlockNumber,
                        JobTimelineId = jobId,
                        BlockTime = block.TimeStamp,
                        CreatedAt = DateTime.UtcNow
                    };

                    _entity.Block.Add(blockDb);

                    var transactions = new List<BlockTransaction>();

                    foreach (var transaction in block.BlockTransactions)
                    {
                        transactions.Add(new BlockTransaction
                        {
                            Block = blockDb,
                            TxHash = transaction.Hash
                        });
                    }

                    _entity.BlockTransaction.AddRange(transactions);
                }

                _entity.SaveChanges();

                stopwatch.Stop();
                _logger.Information($"BlockTransactionService.AddBlocksWithTransactions(jobId: {jobId}). Time elapsed: {stopwatch.Elapsed.TotalSeconds} seconds.");

                response.Status = Common.Enums.StatusEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.Information($"BlockTransactionService.AddBlocksWithTransactions(jobId: {jobId})");
                _logger.Error(ex.Message);
                response.Status = Common.Enums.StatusEnum.Error;
                response.Message = ex.Message;
            }

            return response;
        }

        public IResponse<NoValue> AddBlocksWithTransactions(long jobId, List<BitcoinBlockModel> blocks)
        {
            var response = new Response<NoValue>();

            try
            {
                var stopwatch = Stopwatch.StartNew();

                foreach (var block in blocks)
                {
                    Block blockDb = new Block
                    {
                        BlockNumber = block.BlockNumber,
                        JobTimelineId = jobId,
                        BlockTime = block.Time,
                        CreatedAt = DateTime.UtcNow
                    };

                    _entity.Block.Add(blockDb);

                    var transactions = new List<BlockTransaction>();

                    foreach (var transaction in block.TransactionList)
                    {
                        transactions.Add(new BlockTransaction
                        {
                            Block = blockDb,
                            TxHash = transaction.TransactionHash
                        });
                    }
                    _entity.BlockTransaction.AddRange(transactions);
                }

                _entity.SaveChanges();

                stopwatch.Stop();
                _logger.Information($"BlockTransactionService.AddBlocksWithTransactions(jobId: {jobId}). Time elapsed: {stopwatch.Elapsed.TotalSeconds} seconds.");

                response.Status = Common.Enums.StatusEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.Information($"BlockTransactionService.AddBlocksWithTransactions(jobId: {jobId})");
                _logger.Error(ex.Message);
                response.Status = Common.Enums.StatusEnum.Error;
                response.Message = ex.Message;
            }

            return response;
        }

        public IResponse<NoValue> AddBlocksWithTransactions(long jobId, List<NeoBlockModel> blocks)
        {
            var response = new Response<NoValue>();

            try
            {
                foreach (var block in blocks)
                {
                    Block blockDb = new Block
                    {
                        BlockNumber = block.BlockNumber,
                        JobTimelineId = jobId,
                        BlockTime = block.Time,
                        CreatedAt = DateTime.UtcNow
                    };

                    _entity.Block.Add(blockDb);

                    var transactions = new List<BlockTransaction>();

                    foreach (var transaction in block.TransactionList)
                    {
                        transactions.Add(new BlockTransaction
                        {
                            Block = blockDb,
                            TxHash = transaction.TransactionId
                        });

                    }
                    _entity.BlockTransaction.AddRange(transactions);
                }

                _entity.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Information($"BlockTransactionService.AddBlocksWithTransactions(jobId: {jobId})");
                _logger.Error(ex.Message);
                response.Status = Common.Enums.StatusEnum.Error;
                response.Message = ex.Message;
            }

            return response;
        }

        public IResponse<NoValue> AddBlocksWithTransactions(long jobId, List<LitecoinBlockModel> blocks)
        {
            var response = new Response<NoValue>();

            try
            {
                var stopwatch = Stopwatch.StartNew();

                foreach (var block in blocks)
                {
                    Block blockDb = new Block
                    {
                        BlockNumber = block.BlockNumber,
                        JobTimelineId = jobId,
                        BlockTime = block.Time,
                        CreatedAt = DateTime.UtcNow
                    };

                    _entity.Block.Add(blockDb);

                    var transactions = new List<BlockTransaction>();

                    foreach (var transaction in block.TransactionList)
                    {
                        transactions.Add(new BlockTransaction
                        {
                            Block = blockDb,
                            TxHash = transaction.TransactionHash
                        });
                    }
                    _entity.BlockTransaction.AddRange(transactions);
                }

                _entity.SaveChanges();

                stopwatch.Stop();
                _logger.Information($"BlockTransactionService.AddBlocksWithTransactions(jobId: {jobId}). Time elapsed: {stopwatch.Elapsed.TotalSeconds} seconds.");

                response.Status = Common.Enums.StatusEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.Information($"BlockTransactionService.AddBlocksWithTransactions(jobId: {jobId})");
                _logger.Error(ex.Message);
                response.Status = Common.Enums.StatusEnum.Error;
                response.Message = ex.Message;
            }

            return response;
        }

        public IResponse<List<EthereumBlockModel>> GetBlocksWithTransactions(long jobId, int maxBlockNumber)
        {
            var response = new Response<List<EthereumBlockModel>>
            {
                Value = new List<EthereumBlockModel>()
            };

            try
            {

                var blocks = _entity.Block.Where(b => b.JobTimelineId == jobId && b.BlockNumber > maxBlockNumber).ToList();

                foreach (var block in blocks)
                {

                    var blockToAdd = new EthereumBlockModel
                    {
                        BlockNumber = (int)block.BlockNumber,
                        TimeStamp = block.BlockTime,
                        BlockTransactions = new List<EthereumBlockTransactionModel>()
                    };

                    foreach (var transaction in block.BlockTransaction)
                    {
                        var transactionToAdd = new EthereumBlockTransactionModel
                        {
                            Hash = transaction.TxHash
                        };

                        blockToAdd.BlockTransactions.Add(transactionToAdd);
                    }

                    response.Value.Add(blockToAdd);
                }

                response.Status = Common.Enums.StatusEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.Information($"BlockTransactionService.GetBlocksWithTransactions(jobId: {jobId}, maxBlockNumber: {maxBlockNumber})");
                _logger.Error(ex.Message);
                response.Status = Common.Enums.StatusEnum.Error;
                response.Message = ex.Message;
            }

            return response;
        }

        public IResponse<int> GetMaxBlockNumber(long jobId)
        {
            var response = new Response<int>();

            try
            {
                var jobDefinition = _jobDefinitionService.GetJobDefinitionByJobId(jobId);

                if (jobDefinition.Status == Common.Enums.StatusEnum.Success)
                {
                    var fromAdapterId = jobDefinition.Value.From;
                    var toAdapterId = jobDefinition.Value.To;


                    var blockCount = _entity.Block.Where(j => j.JobTimeline.Schedule.JobDefinition.Adapter1.Id == toAdapterId && j.JobTimeline.Schedule.JobDefinition.Adapter.Id == fromAdapterId).Count();

                    if (blockCount > 0)
                    {
                        response.Value = blockCount;
                    }
                    else
                    {
                        response.Value = 1;
                    }

                    response.Status = Common.Enums.StatusEnum.Success;
                }
            }
            catch (Exception ex)
            {
                _logger.Information($"BlockTransactionService.GetMaxBlockNumber(jobId: {jobId})");
                _logger.Error(ex.Message);
                response.Status = Common.Enums.StatusEnum.Error;
                response.Message = ex.Message;
            }

            return response;
        }

        public IResponse<List<BlockTransactionViewModel>> ShowBlockTransactions(long jobId)
        {
            var response = new Response<List<BlockTransactionViewModel>>
            {
                Value = new List<BlockTransactionViewModel>()
            };

            try
            {
                var blockTransactions = _entity.BlockTransaction.Where(b => b.Block.JobTimelineId == jobId).ToList();

                response.Value = blockTransactions.Select(b => new BlockTransactionViewModel
                {
                    BlockNumber = b.Block.BlockNumber,
                    TransactionHash = b.TxHash,

                }).ToList();

                response.Status = Common.Enums.StatusEnum.Success;

            }
            catch (Exception ex)
            {
                _logger.Information($"BlockTransactionService.ShowBlockTransactions(jobId: {jobId})");
                _logger.Error(ex.Message);
                response.Status = Common.Enums.StatusEnum.Error;
                response.Message = ex.Message;
            }

            return response;
        }


    }
}
