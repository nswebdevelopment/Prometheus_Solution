using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Prometheus.Model.Models;
using Prometheus.Common.Enums;
using Prometheus.Common;
using NeoModules.JsonRpc.Client;
using Prometheus.Model.Models.NeoAdapterModel;
using NeoModules.RPC;
using System.Globalization;
using System.Diagnostics;

namespace Prometheus.BlockchainAdapter.Nodes
{
    public class NEOAdapter
    {
        private readonly Serilog.ILogger _logger;

        public NEOAdapter(Serilog.ILogger logger)
        {
            _logger = logger;
        }

        public string Name
        {
            get
            {
                return "NEO";
            }
        }

        public async Task<Response<List<NeoBlockModel>>> GetBlocksWithTransactions(CryptoAdapterModel cryptoAdapter, int fromBlock, int toBlock, string address)
        {
            var response = new Response<List<NeoBlockModel>>()
            {
                Value = new List<NeoBlockModel>()
            };

            try
            {
                var client = InstantiateRpcClient(cryptoAdapter);

                var neoApiService = new NeoApiService(client);

                if (toBlock < fromBlock)
                {
                    _logger.Information($"NEOAdapter.GetBlocksWithTransactions(cryptoAdapter: {cryptoAdapter.Id}, fromBlock: {fromBlock}, toBlock: {toBlock}, address: {address}");
                    _logger.Error($"FromBlock value {fromBlock} is greater then ToBlock value {toBlock}");
                    response.Status = StatusEnum.Error;
                    return response;
                }

                var stopwatch = Stopwatch.StartNew();

                for (int i = fromBlock; i <= toBlock; i++)
                {
                    var block = await neoApiService.Blocks.GetBlock.SendRequestAsync(i);

                    var blockModel = new NeoBlockModel()
                    {
                        BlockNumber = i,
                        Time = DateTimeOffset.FromUnixTimeSeconds(block.Time).LocalDateTime
                    };

                    foreach (var transaction in block.Transactions)
                    {
                        var blockTransactionModel = new NeoBlockTransactionModel()
                        {
                            TransactionId = transaction.Txid,
                            TransactionType = transaction.Type
                        };

                        foreach (var transactionInput in transaction.Vin)
                        {
                            var transactionInModel = new NeoTransactionInputModel()
                            {
                                TransactionId = transactionInput.TransactionId
                            };

                            blockTransactionModel.TransactionInputs.Add(transactionInModel);
                        }

                        foreach (var transactionOutput in transaction.Vout)
                        {
                            var transactionOutModel = new NeoTransactionOutputModel()
                            {
                                Address = transactionOutput.Address,
                                Asset = transactionOutput.Asset,
                                Value = Decimal.Parse(transactionOutput.Value, CultureInfo.InvariantCulture)
                            };

                            if (address == String.Empty || (String.Equals(address, transactionOutModel.Address)))
                            {
                                blockTransactionModel.TransactionOutputs.Add(transactionOutModel);
                            }
                        }

                        if (address == String.Empty || blockTransactionModel.TransactionOutputs.Exists(to => String.Equals(to.Address, address)))
                        {
                            blockModel.TransactionList.Add(blockTransactionModel);
                        }
                    }

                    response.Value.Add(blockModel);
                }

                stopwatch.Stop();
                _logger.Information($"NEOAdapter.GetBlocksWithTransactions(cryptoAdapter: {cryptoAdapter.Id}, fromBlock: {fromBlock}, toBlock: {toBlock}, address: {address}). Time elapsed: {stopwatch.Elapsed.TotalSeconds} seconds.");

                response.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                response.Status = StatusEnum.Error;
                response.Message = ex.Message;
                _logger.Information($"NEOAdapter.GetBlocksWithTransactions(cryptoAdapter: {cryptoAdapter.Id}, fromBlock: {fromBlock}, toBlock: {toBlock}, address: {address}");
                _logger.Error(ex.Message);
            }

            return response;
        }

        public async Task<Response<int>> GetCurrentBlockNumber(CryptoAdapterModel cryptoAdapter)
        {
            var response = new Response<int>();

            try
            {
                var client = InstantiateRpcClient(cryptoAdapter);

                var neoApiService = new NeoApiService(client);

                var blockNumber = await neoApiService.Blocks.GetBlockCount.SendRequestAsync();

                response.Status = StatusEnum.Success;
                response.Value = blockNumber;

            }
            catch (Exception ex)
            {
                response.Status = StatusEnum.Error;
                response.Message = ex.Message;
                _logger.Information($"NEOAdapter.GetCurrentBlockNumber(cryptoAdapter: {cryptoAdapter.Id}");
                _logger.Error(ex.Message);
            }

            return response;
        }

        public async Task<Response<NoValue>> TestConnectionSource(CryptoAdapterModel cryptoAdapter)
        {
            var response = new Response<NoValue>();

            try
            {
                var client = InstantiateRpcClient(cryptoAdapter);

                var neoApiService = new NeoApiService(client);

                await neoApiService.Blocks.GetBlockCount.SendRequestAsync();

                response.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                response.Message = Message.TestConnectionFailed;
                response.Status = StatusEnum.Error;
                _logger.Information($"NEOAdapter.TestConnectionSource(CryptoAdapterId: {cryptoAdapter.Id})");
                _logger.Error(ex.Message);
            }

            return response;
        }

        #region Helpers

        private static RpcClient InstantiateRpcClient(CryptoAdapterModel cryptoAdapter) => new RpcClient(new Uri($"http://{cryptoAdapter.RpcAddr}:{cryptoAdapter.RpcPort}"));

        #endregion

    }
}
