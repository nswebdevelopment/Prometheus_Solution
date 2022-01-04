using Hangfire;
using Microsoft.Extensions.Configuration;
using Prometheus.BL.Interfaces;
using Prometheus.Common;
using Prometheus.Common.Enums;
using Prometheus.Dal.Entities;
using Prometheus.Model.Models;
using Prometheus.Model.Models.BitcoinAdapterModel;
using Prometheus.Model.Models.LitecoinBlockModel;
using Prometheus.Model.Models.NeoAdapterModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.BL.Services
{
    public class JobService : IJobService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IPrometheusEntities _entity;
        private readonly IDbAdapterService _dbAdapterService;
        private readonly ITransactionService _transactionService;
        private readonly IBlockchainService _blockchainService;
        private readonly IJobHistoryService _jobHistoryService;
        private readonly IJobTimelineService _jobTimelineService;
        private readonly IBlockTransactionService _blockTransactionService;
        private readonly IEnterpriseAdapterService _enterpriseAdapterService;
        private readonly ICryptoAdapterService _cryptoAdapterService;
        private readonly IBusinessAdapterService _businessAdapterService;

        public JobService(ILogger logger, IDbAdapterService dbAdapterService, ITransactionService transactionService, IBlockchainService blockchainService,
            IJobHistoryService jobHistoryService, IJobTimelineService jobTimelineService, IBlockTransactionService blockTransactionService,
            IEnterpriseAdapterService enterpriseAdapterService, ICryptoAdapterService cryptoAdapterService, IPrometheusEntities entity,
            IBusinessAdapterService businessAdapterService, IConfiguration configuration)
        {
            _logger = logger;
            _dbAdapterService = dbAdapterService;
            _transactionService = transactionService;
            _blockchainService = blockchainService;
            _jobHistoryService = jobHistoryService;
            _jobTimelineService = jobTimelineService;
            _blockTransactionService = blockTransactionService;
            _enterpriseAdapterService = enterpriseAdapterService;
            _cryptoAdapterService = cryptoAdapterService;
            _entity = entity;
            _businessAdapterService = businessAdapterService;
            _configuration = configuration;
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task ExecuteJob(long jobId)
        {

            try
            {
                _jobHistoryService.ChangeJobStatus(JobStatus.Executing, jobId);
                var status = new StatusEnum();

                var jobTimeline = _entity.JobTimeline.Find(jobId);
                var jobDefinitionId = jobTimeline.Schedule.JobDefinition.Id;
                var fromAdapter = jobTimeline.Schedule.JobDefinition.Adapter;

                string fromBlock;
                string toBlock;
                string address;

                int ethLimit = Int32.Parse(_configuration["Limits:EthereumBlocks"]);
                int btcLimit = Int32.Parse(_configuration["Limits:BitcoinBlocks"]);
                int neoLimit = Int32.Parse(_configuration["Limits:NEOBlocks"]);
                int ltcLimit = Int32.Parse(_configuration["Limits:LitecoinBlocks"]);

                switch ((AdapterTypeItemEnum)fromAdapter.AdapterTypeItemId)
                {
                    case AdapterTypeItemEnum.MSSQL:
                    case AdapterTypeItemEnum.MySQL:
                    case AdapterTypeItemEnum.Oracle:

                        var data = _dbAdapterService.GetAdapterData(jobDefinitionId);

                        if (data.Status == StatusEnum.Success && data.Value.Count > 0)
                        {
                            var transactionData = new TransactionDataModel
                            {
                                JobId = jobId,
                                Data = data.Value
                            };

                            await _transactionService.AddToTransaction(transactionData);

                            var transactions = _transactionService.GetTransactionsWithoutHash(jobId);

                            if (transactions.Status == StatusEnum.Success)
                            {
                                var blockchainDataList = new List<BlockchainDataModel>()
                                {
                                    new BlockchainDataModel
                                    {
                                        JobId = jobId,
                                        Transactions = transactions.Value
                                    }
                                };

                                status = await SendData(jobId, blockchainDataList);
                                break;
                            }
                        }
                        status = StatusEnum.Error;
                        break;
                    case AdapterTypeItemEnum.MongoDB:
                        break;
                    case AdapterTypeItemEnum.Ethereum:

                        fromBlock = jobTimeline.Schedule.JobDefinition.JobDefinitionProperty.FirstOrDefault(jdp => jdp.PropertyId == (long)PropertyEnum.FromBlock).Value;
                        toBlock = jobTimeline.Schedule.JobDefinition.JobDefinitionProperty.FirstOrDefault(jdp => jdp.PropertyId == (long)PropertyEnum.ToBlock).Value;
                        address = jobTimeline.Schedule.JobDefinition.JobDefinitionProperty.FirstOrDefault(jdp => jdp.PropertyId == (long)PropertyEnum.EthereumAccount).Value;

                        int fromBlockConverted;
                        int toBlockConverted;

                        Int32.TryParse(fromBlock, out fromBlockConverted);
                        Int32.TryParse(toBlock, out toBlockConverted);

                        var ethereumCurrentBlock = await _blockchainService.GetCurrentBlockNumber(fromAdapter.Id, AdapterTypeItemEnum.Ethereum);

                        if (ethereumCurrentBlock.Status == StatusEnum.Success)
                        {
                            if (fromBlockConverted == 0)
                            {
                                fromBlockConverted = 1;
                            }

                            if (toBlockConverted == 0)
                            {
                                toBlockConverted = ethereumCurrentBlock.Value;
                            }

                            if (Math.Abs(toBlockConverted - fromBlockConverted) < ethLimit && fromBlockConverted < ethereumCurrentBlock.Value && toBlockConverted <= ethereumCurrentBlock.Value)
                            {
                                var blocks = await _blockchainService.GetBlocksWithTransactions<EthereumBlockModel>(fromAdapter.Id, fromBlockConverted, toBlockConverted, address);

                                if (blocks.Status == StatusEnum.Success)
                                {
                                    status = await SendData(jobId, blocks.Value);
                                    if (status != StatusEnum.Error)
                                    {
                                        if (jobTimeline.Schedule.RecurrenceRule != null)
                                        {
                                            _blockTransactionService.AddBlocksWithTransactions(jobId, blocks.Value);
                                        }
                                        break;
                                    }
                                }
                                status = StatusEnum.Error;
                                break;
                            }
                            _jobHistoryService.ChangeJobStatus(JobStatus.Failed, jobId, limitExceeded: true);
                            break;
                        }

                        status = StatusEnum.Error;
                        break;
                    case AdapterTypeItemEnum.Cardano:
                    case AdapterTypeItemEnum.EOS:
                        break;
                    case AdapterTypeItemEnum.NEO:

                        fromBlock = jobTimeline.Schedule.JobDefinition.JobDefinitionProperty.FirstOrDefault(jdp => jdp.PropertyId == (long)PropertyEnum.FromBlock).Value;
                        toBlock = jobTimeline.Schedule.JobDefinition.JobDefinitionProperty.FirstOrDefault(jdp => jdp.PropertyId == (long)PropertyEnum.ToBlock).Value;
                        address = jobTimeline.Schedule.JobDefinition.JobDefinitionProperty.FirstOrDefault(jdp => jdp.PropertyId == (long)PropertyEnum.NeoAddress).Value;

                        Int32.TryParse(fromBlock, out fromBlockConverted);
                        Int32.TryParse(toBlock, out toBlockConverted);

                        var neoCurrentBlock = await _blockchainService.GetCurrentBlockNumber(fromAdapter.Id, AdapterTypeItemEnum.NEO);

                        if (neoCurrentBlock.Status == StatusEnum.Success)
                        {
                            if (fromBlockConverted == 0)
                            {
                                fromBlockConverted = 1;
                            }

                            if (toBlockConverted == 0)
                            {
                                toBlockConverted = neoCurrentBlock.Value;

                            }

                            if (Math.Abs(toBlockConverted - fromBlockConverted) < neoLimit && fromBlockConverted < neoCurrentBlock.Value && toBlockConverted <= neoCurrentBlock.Value)
                            {
                                var neoBlocks = await _blockchainService.GetBlocksWithTransactions<NeoBlockModel>(fromAdapter.Id, fromBlockConverted, toBlockConverted, address);

                                if (neoBlocks.Status == StatusEnum.Success)
                                {
                                    status = await SendData(jobId, neoBlocks.Value);
                                    if (status != StatusEnum.Error)
                                    {
                                        if (jobTimeline.Schedule.RecurrenceRule != null)
                                        {
                                            _blockTransactionService.AddBlocksWithTransactions(jobId, neoBlocks.Value);
                                        }
                                        break;
                                    }
                                }
                                status = StatusEnum.Error;
                                break;
                            }
                            _jobHistoryService.ChangeJobStatus(JobStatus.Failed, jobId, limitExceeded: true);
                            break;

                        }
                        status = StatusEnum.Error;
                        break;
                    case AdapterTypeItemEnum.Bitcoin:

                        fromBlock = jobTimeline.Schedule.JobDefinition.JobDefinitionProperty.FirstOrDefault(jdp => jdp.PropertyId == (long)PropertyEnum.FromBlock).Value;
                        toBlock = jobTimeline.Schedule.JobDefinition.JobDefinitionProperty.FirstOrDefault(jdp => jdp.PropertyId == (long)PropertyEnum.ToBlock).Value;
                        address = jobTimeline.Schedule.JobDefinition.JobDefinitionProperty.FirstOrDefault(jdp => jdp.PropertyId == (long)PropertyEnum.BitcoinAddress).Value;

                        Int32.TryParse(fromBlock, out fromBlockConverted);
                        Int32.TryParse(toBlock, out toBlockConverted);

                        var bitcoinCurrentBlock = await _blockchainService.GetCurrentBlockNumber(fromAdapter.Id, AdapterTypeItemEnum.Bitcoin);

                        if (bitcoinCurrentBlock.Status == StatusEnum.Success)
                        {

                            if (fromBlockConverted == 0)
                            {
                                fromBlockConverted = 1;
                            }

                            if (toBlockConverted == 0)
                            {
                                toBlockConverted = bitcoinCurrentBlock.Value;
                            }

                            if (Math.Abs(toBlockConverted - fromBlockConverted) < btcLimit && fromBlockConverted < bitcoinCurrentBlock.Value && toBlockConverted <= bitcoinCurrentBlock.Value)
                            {
                                var bitcoinBlocks = await _blockchainService.GetBlocksWithTransactions<BitcoinBlockModel>(fromAdapter.Id, fromBlockConverted, toBlockConverted, address);

                                if (bitcoinBlocks.Status == StatusEnum.Success)
                                {
                                    status = await SendData(jobId, bitcoinBlocks.Value);
                                    if (status != StatusEnum.Error)
                                    {
                                        if (jobTimeline.Schedule.RecurrenceRule != null)
                                        {
                                            _blockTransactionService.AddBlocksWithTransactions(jobId, bitcoinBlocks.Value);
                                        }
                                        break;
                                    }
                                }
                                status = StatusEnum.Error;
                                break;
                            }
                            _jobHistoryService.ChangeJobStatus(JobStatus.Failed, jobId, limitExceeded: true);
                            break;

                        }
                        status = StatusEnum.Error;

                        break;
                    case AdapterTypeItemEnum.Litecoin:

                        fromBlock = jobTimeline.Schedule.JobDefinition.JobDefinitionProperty.FirstOrDefault(jdp => jdp.PropertyId == (long)PropertyEnum.FromBlock).Value;
                        toBlock = jobTimeline.Schedule.JobDefinition.JobDefinitionProperty.FirstOrDefault(jdp => jdp.PropertyId == (long)PropertyEnum.ToBlock).Value;
                        address = jobTimeline.Schedule.JobDefinition.JobDefinitionProperty.FirstOrDefault(jdp => jdp.PropertyId == (long)PropertyEnum.LitecoinAddress).Value;

                        Int32.TryParse(fromBlock, out fromBlockConverted);
                        Int32.TryParse(toBlock, out toBlockConverted);

                        var litecoinCurrentBlock = await _blockchainService.GetCurrentBlockNumber(fromAdapter.Id, AdapterTypeItemEnum.Litecoin);

                        if (litecoinCurrentBlock.Status == StatusEnum.Success)
                        {

                            if (fromBlockConverted == 0)
                            {
                                fromBlockConverted = 1;
                            }

                            if (toBlockConverted == 0)
                            {
                                toBlockConverted = litecoinCurrentBlock.Value;
                            }

                            if (Math.Abs(toBlockConverted - fromBlockConverted) < ltcLimit && fromBlockConverted < litecoinCurrentBlock.Value && toBlockConverted <= litecoinCurrentBlock.Value)
                            {
                                var litecoinBlocks = await _blockchainService.GetBlocksWithTransactions<LitecoinBlockModel>(fromAdapter.Id, fromBlockConverted, toBlockConverted, address);

                                if (litecoinBlocks.Status == StatusEnum.Success)
                                {
                                    status = await SendData(jobId, litecoinBlocks.Value);
                                    if (status != StatusEnum.Error)
                                    {
                                        if (jobTimeline.Schedule.RecurrenceRule != null)
                                        {
                                            _blockTransactionService.AddBlocksWithTransactions(jobId, litecoinBlocks.Value);
                                        }
                                        break;
                                    }
                                }
                                status = StatusEnum.Error;
                                break;
                            }
                            _jobHistoryService.ChangeJobStatus(JobStatus.Failed, jobId, limitExceeded: true);
                            break;

                        }
                        status = StatusEnum.Error;

                        break;
                    case AdapterTypeItemEnum.Excel:
                    case AdapterTypeItemEnum.MATLAB:
                        break;
                    case AdapterTypeItemEnum.Solana:

                        fromBlock = jobTimeline.Schedule.JobDefinition.JobDefinitionProperty.FirstOrDefault(jdp => jdp.PropertyId == (long)PropertyEnum.FromBlock).Value;
                        toBlock = jobTimeline.Schedule.JobDefinition.JobDefinitionProperty.FirstOrDefault(jdp => jdp.PropertyId == (long)PropertyEnum.ToBlock).Value;
                        address = jobTimeline.Schedule.JobDefinition.JobDefinitionProperty.FirstOrDefault(jdp => jdp.PropertyId == (long)PropertyEnum.SolanaAddress).Value;

                        var solBlocks = await _blockchainService.GetBlocksWithTransactions<SolanaBlockModel>(fromAdapter.Id, int.Parse(fromBlock), int.Parse(toBlock), address);

                                if (solBlocks.Status == StatusEnum.Success)
                                {
                                    status = await SendData(jobId, solBlocks.Value);
                                    if (status != StatusEnum.Error)
                                    {
                                        if (jobTimeline.Schedule.RecurrenceRule != null)
                                        {
                                            _blockTransactionService.AddBlocksWithTransactions(jobId, solBlocks.Value);
                                        }
                                        break;
                                    }
                                }
                             
                            _jobHistoryService.ChangeJobStatus(JobStatus.Failed, jobId, limitExceeded: true);
                            break;
                  
                    default:
                        status = StatusEnum.Error;
                        break;
                }

                if (status == StatusEnum.Error)
                {
                    _jobHistoryService.ChangeJobStatus(JobStatus.Failed, jobId);
                }
            }
            catch (Exception ex)
            {
                _jobHistoryService.ChangeJobStatus(JobStatus.Failed, jobId);
                _logger.Information($"JobService.ExecuteJob(jobId: {jobId})");
                _logger.Error(ex.Message);
            }
        }


        private async Task<StatusEnum> SendData<T>(long jobId, List<T> list)
        {
            var transactionCount = 0;
            var blockCount = 0;

            try
            {
                var jobTimeline = _entity.JobTimeline.Find(jobId);
                var toAdapter = jobTimeline.Schedule.JobDefinition.Adapter1;

                switch ((AdapterTypeItemEnum)toAdapter.AdapterTypeItemId)
                {
                    case AdapterTypeItemEnum.MSSQL:
                    case AdapterTypeItemEnum.MySQL:
                    case AdapterTypeItemEnum.Oracle:
                        if (typeof(T) == typeof(EthereumBlockModel) || typeof(T) == typeof(BitcoinBlockModel) || typeof(T) == typeof(NeoBlockModel) || typeof(T) == typeof(LitecoinBlockModel))
                        {
                            var result = _dbAdapterService.SendToRelationalDb(jobId, list);

                            if (result.Status != StatusEnum.Success)
                            {
                                return StatusEnum.Error;
                            }

                            blockCount = list.Count();

                            if (typeof(T) == typeof(EthereumBlockModel))
                            {
                                var model = (List<EthereumBlockModel>)Convert.ChangeType(list, typeof(List<EthereumBlockModel>));
                                transactionCount = model.Select(m => m.BlockTransactions.Count).Sum();
                            }
                            else if (typeof(T) == typeof(BitcoinBlockModel))
                            {
                                var model = (List<BitcoinBlockModel>)Convert.ChangeType(list, typeof(List<BitcoinBlockModel>));
                                transactionCount = model.Select(m => m.TransactionList.Count).Sum();
                            }
                            else if (typeof(T) == typeof(NeoBlockModel))
                            {
                                var model = (List<NeoBlockModel>)Convert.ChangeType(list, typeof(List<NeoBlockModel>));
                                transactionCount = model.Select(m => m.TransactionList.Count).Sum();
                            }
                            else if (typeof(T) == typeof(LitecoinBlockModel))
                            {
                                var model = (List<LitecoinBlockModel>)Convert.ChangeType(list, typeof(List<LitecoinBlockModel>));
                                transactionCount = model.Select(m => m.TransactionList.Count).Sum();
                            }
                        }

                        break;
                    case AdapterTypeItemEnum.MongoDB:

                        if (typeof(T) == typeof(EthereumBlockModel) || typeof(T) == typeof(BitcoinBlockModel) || typeof(T) == typeof(NeoBlockModel) || typeof(T) == typeof(LitecoinBlockModel))
                        {
                            var result = _dbAdapterService.SendToMongoDb(toAdapter.EnterpriseAdapter.FirstOrDefault().Id, list);

                            if (result.Status != StatusEnum.Success)
                            {
                                return StatusEnum.Error;
                            }

                            blockCount = list.Count();

                            if (typeof(T) == typeof(EthereumBlockModel))
                            {
                                var model = (List<EthereumBlockModel>)Convert.ChangeType(list, typeof(List<EthereumBlockModel>));
                                transactionCount = model.Select(m => m.BlockTransactions.Count).Sum();
                            }
                            else if (typeof(T) == typeof(BitcoinBlockModel))
                            {
                                var model = (List<BitcoinBlockModel>)Convert.ChangeType(list, typeof(List<BitcoinBlockModel>));
                                transactionCount = model.Select(m => m.TransactionList.Count).Sum();
                            }
                            else if (typeof(T) == typeof(NeoBlockModel))
                            {
                                var model = (List<NeoBlockModel>)Convert.ChangeType(list, typeof(List<NeoBlockModel>));
                                transactionCount = model.Select(m => m.TransactionList.Count).Sum();
                            }
                            else if (typeof(T) == typeof(LitecoinBlockModel))
                            {
                                var model = (List<LitecoinBlockModel>)Convert.ChangeType(list, typeof(List<LitecoinBlockModel>));
                                transactionCount = model.Select(m => m.TransactionList.Count).Sum();
                            }
                        }

                        break;
                    case AdapterTypeItemEnum.Ethereum:
                        if (typeof(T) == typeof(BlockchainDataModel))
                        {
                            var model = (BlockchainDataModel)Convert.ChangeType(list[0], typeof(BlockchainDataModel));

                            var blockchainTransactions = await _blockchainService.SendToBlockchain(model);

                            if (blockchainTransactions.Status == StatusEnum.Error)
                            {
                                return StatusEnum.Error;
                            }

                            if (blockchainTransactions.Value.Count > 0)
                            {
                                _transactionService.UpdateTransactions(blockchainTransactions.Value);
                                transactionCount = blockchainTransactions.Value.Count;
                            }
                        }
                        break;
                    case AdapterTypeItemEnum.Cardano:
                    case AdapterTypeItemEnum.EOS:
                    case AdapterTypeItemEnum.NEO:
                    case AdapterTypeItemEnum.Bitcoin:
                        break;
                    case AdapterTypeItemEnum.Excel:
                        if (typeof(T) == typeof(EthereumBlockModel))
                        {
                            var model = (List<EthereumBlockModel>)Convert.ChangeType(list, typeof(List<EthereumBlockModel>));

                            var numberOfTransactions = model.SelectMany(b => b.BlockTransactions).Count();

                            if (numberOfTransactions > 0)
                            {
                                var result = _businessAdapterService.CreateXlsxFile(model, jobId);

                                if (result.Status != StatusEnum.Success)
                                {
                                    return StatusEnum.Error;
                                }

                                blockCount = model.Count();
                                transactionCount = numberOfTransactions;
                            }
                            else
                            {
                                blockCount = model.Count();
                            }
                        }
                        else if (typeof(T) == typeof(BitcoinBlockModel))
                        {
                            var model = (List<BitcoinBlockModel>)Convert.ChangeType(list, typeof(List<BitcoinBlockModel>));

                            var numberOfTransactions = model.SelectMany(b => b.TransactionList).Count();

                            if (numberOfTransactions > 0)
                            {
                                var result = _businessAdapterService.CreateXlsxFile(model, jobId);

                                if (result.Status != StatusEnum.Success)
                                {
                                    return StatusEnum.Error;
                                }

                                blockCount = model.Count();
                                transactionCount = numberOfTransactions;
                            }
                            else
                            {
                                blockCount = model.Count();
                            }

                        }
                        else if (typeof(T) == typeof(NeoBlockModel))
                        {
                            var model = (List<NeoBlockModel>)Convert.ChangeType(list, typeof(List<NeoBlockModel>));

                            var numberOfTransactions = model.SelectMany(b => b.TransactionList).Count();

                            if (numberOfTransactions > 0)
                            {
                                var result = _businessAdapterService.CreateXlsxFile(model, jobId);

                                if (result.Status != StatusEnum.Success)
                                {
                                    return StatusEnum.Error;
                                }

                                blockCount = model.Count();
                                transactionCount = numberOfTransactions;
                            }
                            else
                            {
                                blockCount = model.Count();
                            }
                        }
                        else if (typeof(T) == typeof(LitecoinBlockModel))
                        {
                            var model = (List<LitecoinBlockModel>)Convert.ChangeType(list, typeof(List<LitecoinBlockModel>));

                            var numberOfTransactions = model.SelectMany(b => b.TransactionList).Count();

                            if (numberOfTransactions > 0)
                            {
                                var result = _businessAdapterService.CreateXlsxFile(model, jobId);
                                if (result.Status != StatusEnum.Success)
                                {
                                    return StatusEnum.Error;
                                }

                                blockCount = model.Count();
                                transactionCount = numberOfTransactions;
                            }
                            else
                            {
                                blockCount = model.Count();
                            }
                        }
                        else if (typeof(T) == typeof(SolanaBlockModel))
                        {
                            var model = (List<SolanaBlockModel>)Convert.ChangeType(list, typeof(List<SolanaBlockModel>));

                            var numberOfTransactions = model.SelectMany(b => b.BlockTransactions).Count();

                            if (numberOfTransactions > 0)
                            {
                                var result = _businessAdapterService.CreateXlsxFile(model, jobId);
                                if (result.Status != StatusEnum.Success)
                                {
                                    return StatusEnum.Error;
                                }

                                blockCount = model.Count();
                                transactionCount = numberOfTransactions;
                            }
                            else
                            {
                                blockCount = model.Count();
                            }
                        }
                        break;
                    case AdapterTypeItemEnum.MATLAB:
                        if (typeof(T) == typeof(EthereumBlockModel))
                        {
                            var model = (List<EthereumBlockModel>)Convert.ChangeType(list, typeof(List<EthereumBlockModel>));

                            var numberOfTransactions = model.SelectMany(b => b.BlockTransactions).Count();

                            if (numberOfTransactions > 0)
                            {
                                var result = _businessAdapterService.CreateCsvFile(model, jobId);

                                if (result.Status != StatusEnum.Success)
                                {
                                    return StatusEnum.Error;
                                }

                                blockCount = model.Count();
                                transactionCount = numberOfTransactions;
                            }
                            else
                            {
                                blockCount = model.Count();
                            }
                        }
                        break;
                    case AdapterTypeItemEnum.Solana:
                        if (typeof(T) == typeof(BlockchainDataModel))
                        {
                            var model = (BlockchainDataModel)Convert.ChangeType(list[0], typeof(BlockchainDataModel));

                            var blockchainTransactions = await _blockchainService.SendToBlockchain(model);

                            if (blockchainTransactions.Status == StatusEnum.Error)
                            {
                                return StatusEnum.Error;
                            }

                            if (blockchainTransactions.Value.Count > 0)
                            {
                                _transactionService.UpdateTransactions(blockchainTransactions.Value);
                                transactionCount = blockchainTransactions.Value.Count;
                            }
                        }
                        break;
                    default:
                        return StatusEnum.Error;
                }

                _jobHistoryService.ChangeJobStatus(JobStatus.Done, jobId, transactionCount, blockCount);
                _jobTimelineService.WriteNumberOfTransactions(jobId, transactionCount);
                return StatusEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.Information($"JobService.SendData(jobId: {jobId}, list: {list})");
                _logger.Error(ex.Message);
                return StatusEnum.Error;
            }
        }

        public Task JobCleaner()
        {
            BusinessFileCleaner();
            TransactionAndAccountCleaner();
            BlockCleaner();

            return Task.CompletedTask;
        }

        private void BusinessFileCleaner()
        {
            try
            {
                var leaseTime = Convert.ToInt32(_configuration["LeaseTime:BusinessFile"]);
                var businessFile = _entity.BusinessFile;
                var schedules = _entity.BusinessFile.Select(jt => jt.JobTimeline.Schedule).Distinct().ToList();

                foreach (var schedule in schedules)
                {
                    var jobtimeline = _entity.JobTimeline.Where(s => s.ScheduleId == schedule.Id).OrderByDescending(s => s.StartTime).FirstOrDefault();
                    if (jobtimeline.JobStatusId == (int)JobStatus.Done && (DateTime.UtcNow - jobtimeline.StartTime).TotalDays > leaseTime)
                        _entity.BusinessFile.RemoveRange(businessFile.Where(j => j.JobTimeline.ScheduleId == jobtimeline.ScheduleId));
                }

                _entity.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Information($"JobService.BusinessFileCleaner()");
                _logger.Error(ex.Message);
            }
        }


        private void BlockCleaner()
        {
            try
            {
                var blockList = new List<Dal.Block>();
                var blockTransactionList = new List<Dal.BlockTransaction>();

                var schedules = _entity.Block.Where(j => j.JobTimeline.JobStatusId == (int)JobStatus.Done).Select(s => s.JobTimeline.Schedule).Distinct().ToList();

                foreach (var schedule in schedules)
                {
                    if (schedule.JobTimeline.Count(c => c.JobStatusId == (int)JobStatus.Done) == schedule.JobTimeline.Count())
                    {
                        foreach (var job in schedule.JobTimeline)
                        {
                            var blocks = _entity.Block.Where(b => b.JobTimeline.Id == job.Id);
                            foreach (var block in blocks)
                            {
                                var blockTransactions = _entity.BlockTransaction.Where(b => b.BlockId == block.Id);
                                blockTransactionList.AddRange(blockTransactions);
                            }
                            blockList.AddRange(blocks);
                        }
                    }
                }

                _entity.BlockTransaction.RemoveRange(blockTransactionList);
                _entity.Block.RemoveRange(blockList);

                _entity.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Information($"JobService.BlockCleaner()");
                _logger.Error(ex.Message);
            }
        }

        private void TransactionAndAccountCleaner()
        {
            try
            {
                var accounts = new List<Dal.Account>();
                var schedules = _entity.Transaction.Select(t => t.JobTimeline.Schedule)?.GroupBy(s => s.Id).Select(s => s.FirstOrDefault());

                foreach (var schedule in schedules)
                {
                    if (schedule.JobTimeline.All(jt => jt.JobStatusId == (int)JobStatus.Done) && schedule.JobTimeline.LastOrDefault().StartTime.AddHours(24) <= DateTime.UtcNow)
                    {
                        foreach (var jobTimeline in schedule.JobTimeline)
                        {
                            accounts.AddRange(jobTimeline.Transaction.Select(t => t.Account));
                            _entity.Transaction.RemoveRange(jobTimeline.Transaction);
                            accounts.RemoveAll(a => a.Transaction.Count > 0);
                        }
                    }
                }

                _entity.Account.RemoveRange(accounts);
                _entity.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Information($"JobService.TransactionAndAccountCleaner()");
                _logger.Error(ex.Message);
            }
        }

        #region Helpers
        
        #endregion
    }
}


