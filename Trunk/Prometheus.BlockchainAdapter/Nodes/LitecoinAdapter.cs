using NBitcoin;
using NBitcoin.Altcoins;
using NBitcoin.RPC;
using Prometheus.Common;
using Prometheus.Common.Enums;
using Prometheus.Model.Models;
using Prometheus.Model.Models.BitcoinAdapterModel;
using Prometheus.Model.Models.LitecoinBlockModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.BlockchainAdapter.Nodes
{
    public class LitecoinAdapter
    {
        private readonly Serilog.ILogger _logger;

        public LitecoinAdapter(Serilog.ILogger logger)
        {
            _logger = logger;
        }

        public Response<List<LitecoinBlockModel>> GetBlocksWithTransactions(CryptoAdapterModel cryptoAdapter, string username, string password, int fromBlock, int toBlock, string address)
        {
            var response = new Response<List<LitecoinBlockModel>>()
            {
                Value = new List<LitecoinBlockModel>()
            };

            try
            {
                var client = InstantiateRpcClient(cryptoAdapter, username, password);

                if (toBlock < fromBlock)
                {
                    _logger.Information($"LitecoinAdapter.GetBlocksWithTransactions(cryptoAdapter: {cryptoAdapter.Id}, username:{username}, password:{password}, fromBlock: {fromBlock}, toBlock: {toBlock}, address: {address}");
                    _logger.Error($"FromBlock value {fromBlock} is greater than ToBlock value {toBlock}");
                    response.Status = Common.Enums.StatusEnum.Error;
                    return response;
                }

                var stopwatch = Stopwatch.StartNew();

                for (int i = fromBlock; i <= toBlock; i++)
                {
                    var block = client.GetBlock(i);

                    var blockModel = new LitecoinBlockModel()
                    {
                        BlockNumber = i,
                        Time = UnixTimeStampToDateTime(Convert.ToDouble(block.Header.BlockTime.ToUnixTimeSeconds()))
                    };

                    foreach (var transaction in block.Transactions)
                    {
                        var blockTransactionModel = new LitecoinBlockTransactionModel
                        {
                            TransactionHash = transaction.GetHash().ToString(),
                            TotalOutValue = transaction.TotalOut.ToDecimal(MoneyUnit.BTC)
                        };

                        foreach (var transactionInput in transaction.Inputs)
                        {
                            var transactionInModel = new LitecoinTransactionInModel();

                            if (transactionInput.ScriptSig.GetSignerAddress(Litecoin.Instance.Mainnet) != null)
                            {
                                transactionInModel.Address = transactionInput.ScriptSig.GetSignerAddress(Litecoin.Instance.Mainnet).ToString();
                            }
                            else
                            {
                                transactionInModel.Address = "Address could not be retrieved.";
                            }

                            if (address == String.Empty || (String.Equals(address, transactionInModel.Address)))
                            {
                                blockTransactionModel.TransactionInputs.Add(transactionInModel);
                            }
                        }

                        blockTransactionModel.TransactionInputs = blockTransactionModel.TransactionInputs.GroupBy(ti => ti.Address).Select(ti => ti.FirstOrDefault()).ToList();

                        foreach (var transactionOutput in transaction.Outputs)
                        {
                            var transactionOutModel = new LitecoinTransactionOutModel
                            {
                                Value = transactionOutput.Value.ToDecimal(MoneyUnit.BTC)
                            };

                            if (transactionOutput.ScriptPubKey.GetDestinationAddress(Litecoin.Instance.Mainnet) != null)
                            {
                                transactionOutModel.Address = transactionOutput.ScriptPubKey.GetDestinationAddress(Litecoin.Instance.Mainnet).ToString();
                            }
                            else
                            {
                                transactionOutModel.Address = "Address could not be retrieved.";
                            }

                            if (address == String.Empty || (String.Equals(address, transactionOutModel.Address)))
                            {
                                blockTransactionModel.TransactionOutputs.Add(transactionOutModel);
                            }
                        }

                        if (address == String.Empty || blockTransactionModel.TransactionInputs.Exists(ti => String.Equals(ti.Address, address)) || blockTransactionModel.TransactionOutputs.Exists(to => String.Equals(to.Address, address)))
                        {
                            blockModel.TransactionList.Add(blockTransactionModel);
                        }
                    }

                    response.Value.Add(blockModel);
                }

                stopwatch.Stop();
                _logger.Information($"LitecoinAdapter.GetBlocksWithTransactions(cryptoAdapter: {cryptoAdapter.Id}, username:{username}, password:{password}, fromBlock: {fromBlock}, toBlock: {toBlock}, address: {address}). Time elapsed: {stopwatch.Elapsed.TotalSeconds} seconds.");

                response.Status = Common.Enums.StatusEnum.Success;
            }
            catch (Exception ex)
            {
                response.Status = Common.Enums.StatusEnum.Error;
                response.Message = ex.Message;
                _logger.Information($"LitecoinAdapter.GetBlocksWithTransactions(cryptoAdapter: {cryptoAdapter.Id}, username:{username}, password:{password}, fromBlock: {fromBlock}, toBlock: {toBlock}, address: {address}");
                _logger.Error(ex.Message);

            }

            return response;
        }

        public Response<int> GetCurrentBlockNumber(CryptoAdapterModel cryptoAdapter, string username, string password)
        {
            var response = new Response<int>();

            try
            {
                var client = InstantiateRpcClient(cryptoAdapter, username, password);

                var blockNumber = client.GetBlockCount();

                response.Value = blockNumber;
                response.Status = Common.Enums.StatusEnum.Success;
            }
            catch (Exception ex)
            {

                response.Status = Common.Enums.StatusEnum.Error;
                response.Message = ex.Message;
                _logger.Information($"LiteAdapter.GetCurrentBlockNumber(cryptoAdapter: {cryptoAdapter.Id}, username:{username}, password:{password}");
                _logger.Error(ex.Message);
            }

            return response;
        }

        public Response<NoValue> TestConnectionSource(CryptoAdapterModel cryptoAdapter, string username, string password)
        {
            var response = new Response<NoValue>();

            try
            {
                var client = InstantiateRpcClient(cryptoAdapter, username, password);

                client.GetBlockCount();

                response.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                response.Status = StatusEnum.Error;
                response.Message = Message.TestConnectionFailed;
                _logger.Information($"LitecoinAdapter.TestConnectionSource(cryptoAdapter: {cryptoAdapter.Id}, username:{username}, password:{password}");
                _logger.Error(ex.Message);
            }

            return response;
        }

        #region Helper
        private static RPCClient InstantiateRpcClient(CryptoAdapterModel cryptoAdapter, string username, string password) =>
            new RPCClient($"{username}:{password}", $"{cryptoAdapter.RpcAddr}:{cryptoAdapter.RpcPort}", Litecoin.Instance.Mainnet);

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }
        #endregion

    }
}
