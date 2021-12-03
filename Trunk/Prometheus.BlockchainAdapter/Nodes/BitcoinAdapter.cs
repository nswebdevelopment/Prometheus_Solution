﻿using NBitcoin;
using NBitcoin.RPC;
using Prometheus.Common;
using Prometheus.Common.Enums;
using Prometheus.Model.Models;
using Prometheus.Model.Models.BitcoinAdapterModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Prometheus.BlockchainAdapter.Nodes
{
    /// <summary>
    /// Class for manipulating data from Bitcoin network
    /// </summary>
    public class BitcoinAdapter
    {
        // TestConnectiontDestination method ?
        // Name property ?

        private readonly Serilog.ILogger _logger;

        public BitcoinAdapter(Serilog.ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets the Bitcoin blocks in the specified range
        /// </summary>
        /// <param name="cryptoAdapter">Information about Bitcoin network</param>
        /// <param name="username">Account username</param>
        /// <param name="password">Account password</param>
        /// <param name="fromBlock">First block to get</param>
        /// <param name="toBlock">Last block to get</param>
        /// <param name="address">Address to filter transactions</param>
        /// <returns>List of Bitcoin blocks</returns>
        public Response<List<BitcoinBlockModel>> GetBlocksWithTransactions(CryptoAdapterModel cryptoAdapter, string username, string password, int fromBlock, int toBlock, string address)
        {
            var response = new Response<List<BitcoinBlockModel>>()
            {
                Value = new List<BitcoinBlockModel>()
            };

            try
            {
                var client = InstantiateRpcClient(cryptoAdapter, username, password);

                if (toBlock < fromBlock)
                {
                    _logger.Information($"BitcoinAdapter.GetBlocksWithTransactions(cryptoAdapter: {cryptoAdapter.Id}, username:{username}, password:{password}, fromBlock: {fromBlock}, toBlock: {toBlock}, address: {address}");
                    _logger.Error($"FromBlock value {fromBlock} is greater than ToBlock value {toBlock}");
                    response.Status = StatusEnum.Error;
                    return response;
                }

                var stopwatch = Stopwatch.StartNew();

                for (int i = fromBlock; i <= toBlock; i++)
                {
                    // returns a block matching the block number
                    var block = client.GetBlock(i);

                    // block information of importance
                    var blockModel = new BitcoinBlockModel
                    {
                        BlockNumber = i,
                        Time = UnixTimeStampToDateTime(Convert.ToDouble(block.Header.BlockTime.ToUnixTimeSeconds()))
                    };

                    // transaction information of importance in the current block
                    foreach (var transaction in block.Transactions)
                    {
                        var blockTransactionModel = new BitcoinBlockTransactionModel
                        {
                            TransactionHash = transaction.GetHash().ToString(),
                            TotalOutValue = transaction.TotalOut.ToDecimal(MoneyUnit.BTC) // same as 'FromWei'
                        };

                        foreach (var transactionInput in transaction.Inputs)
                        {
                            var transactionInModel = new TransactionInModel();

                            if (transactionInput.ScriptSig.GetSignerAddress(Network.Main) != null)
                            {
                                transactionInModel.Address = transactionInput.ScriptSig.GetSignerAddress(Network.Main).ToString();
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
                            var transactionOutModel = new TransactionOutModel
                            {
                                Value = transactionOutput.Value.ToDecimal(MoneyUnit.BTC)
                            };

                            if (transactionOutput.ScriptPubKey.GetDestinationAddress(Network.Main) != null)
                            {
                                transactionOutModel.Address = transactionOutput.ScriptPubKey.GetDestinationAddress(Network.Main).ToString();
                            }
                            else
                            {
                                transactionOutModel.Address = "Address could not be retrieved.";
                            }

                            //transactionOutModel.Address = (transactionOutput.ScriptPubKey.GetDestinationAddress(Network.Main) != null) ? transactionOutput.ScriptPubKey.GetDestinationAddress(Network.Main).ToString() : "Address could not be retrieved.";

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
                _logger.Information($"BitcoinAdapter.GetBlocksWithTransactions(cryptoAdapter: {cryptoAdapter.Id}, fromBlock: {fromBlock}, toBlock: {toBlock}, address: {address}). Time elapsed: {stopwatch.Elapsed.TotalSeconds} seconds.");

                response.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                response.Status = StatusEnum.Error;
                response.Message = ex.Message;
                _logger.Information($"BitcoinAdapter.GetBlocksWithTransactions(cryptoAdapter: {cryptoAdapter.Id}, username:{username}, password:{password}, fromBlock: {fromBlock}, toBlock: {toBlock}, address: {address}");
                _logger.Error(ex.Message);
            }

            return response;
        }

        /// <summary>
        ///  Gets current block number
        /// </summary>
        /// <param name="cryptoAdapter">Information about Bitcoin network</param>
        /// <param name="username">Account username</param>
        /// <param name="password">Account password</param>
        /// <returns> Current block number</returns>
        public Response<int> GetCurrentBlockNumber(CryptoAdapterModel cryptoAdapter, string username, string password)
        {
            var response = new Response<int>();

            try
            {
                var client = InstantiateRpcClient(cryptoAdapter, username, password);

                // get the current block number
                var blockNumber = client.GetBlockCount();

                response.Value = blockNumber;
                response.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {

                response.Status = StatusEnum.Error;
                response.Message = ex.Message;
                _logger.Information($"BitcoinAdapter.GetCurrentBlockNumber(cryptoAdapter: {cryptoAdapter.Id}, username:{username}, password:{password}");
                _logger.Error(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Tests the connection to the source
        /// </summary>
        /// <param name="cryptoAdapter">Information about Bitcoin network</param>
        /// <param name="username">Account username</param>
        /// <param name="password">Account password</param>
        /// <returns>Information about test connection to source</returns>
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
                _logger.Information($"BitcoinAdapter.TestConnectionSource(cryptoAdapter: {cryptoAdapter.Id}, username:{username}, password:{password}");
                _logger.Error(ex.Message);
            }

            return response;
        }

        #region Helpers

        /// <summary>
        /// Create a new RPCClient instance
        /// </summary>
        /// <param name="cryptoAdapter">Information about Bitcoin network</param>
        /// <param name="username">Account username</param>
        /// <param name="password">Account password</param>
        /// <returns>RPCClient instance for provided crypto adapter and credentials</returns>
        private static RPCClient InstantiateRpcClient(CryptoAdapterModel cryptoAdapter, string username, string password)
        {
            return new RPCClient($"{username}:{password}", $"{cryptoAdapter.RpcAddr}:{cryptoAdapter.RpcPort}", Network.Main);
        }

        /// <summary>
        /// Converts UnixTimeStamp to DateTime form
        /// The unix time stamp is a way to track time as a running total of seconds
        /// </summary>
        /// <param name="unixTimeStamp">UnixTimeStamp to be converted</param>
        /// <returns>DateTime value</returns>
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
