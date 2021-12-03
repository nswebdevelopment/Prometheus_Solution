using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Prometheus.BlockchainAdapter.Models;
using Prometheus.Common;
using Prometheus.Common.Enums;
using Prometheus.Model.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Prometheus.BlockchainAdapter.Nodes
{
    /// <summary>
    /// Class for manipulating data from Ethereum network
    /// </summary>
    public class EthereumAdapter
    {
        private readonly Serilog.ILogger _logger;

        public EthereumAdapter(Serilog.ILogger logger)
        {
            _logger = logger;
        }

        public string Name
        {
            get
            {
                return "Ethereum";
            }
        }

        /// <summary>
        /// Sends transaction to the blockchain network
        /// </summary>
        /// <param name="transaction">Information about transaction that will be sent</param>
        /// <param name="cryptoAdapter">Information about Ethereum network</param>
        /// <returns>Transaction results</returns>
        public async Task<TransactionResponse<BlockchainTransactionModel>> SendTransaction(BlockchainTransactionModel transaction, CryptoAdapterModel cryptoAdapter)
        {
            var response = new TransactionResponse<BlockchainTransactionModel>();

            // url to the appropriate node
            var web3 = InstantiateWeb3(cryptoAdapter);

            // credentials for platform for buying/selling digital currency (f.i. coinbase.com)
            var coinbase = cryptoAdapter.Properties.FirstOrDefault().DestinationProperties.FirstOrDefault(dp => dp.Id == (long)PropertyEnum.Coinbase).Value;
            var password = cryptoAdapter.Properties.FirstOrDefault().DestinationProperties.FirstOrDefault(dp => dp.Id == (long)PropertyEnum.CoinbasePassword).Value;

            try
            {
                // if coinbase is unknown, get the coinbase (address to which mining rewards will go) from Ethereum blockchain
                if (String.IsNullOrEmpty(coinbase))
                {
                    coinbase = await web3.Eth.CoinBase.SendRequestAsync();
                }

                // signs data using a specific account
                // Sending your account password over an unsecured HTTP RPC connection is highly unsecure ***
                var unlockAccount = await web3.Personal.UnlockAccount.SendRequestAsync(coinbase, password, 0);

                // credit or debit
                var statement = transaction.Statement;

                var address = String.Empty;

                // takes Credit or Debit address from given transaction
                switch (statement)
                {
                    case Statement.Credit:

                        address = transaction.Account.CreditAddress;
                        break;
                    case Statement.Debit:

                        address = transaction.Account.DebitAddress;
                        break;
                }

                // takes transaction value from the given transaction
                var value = transaction.Value;

                // converts transaction value to Wei (= base unit of ether)
                var weiValue = Web3.Convert.ToWei(value, Nethereum.Util.UnitConversion.EthUnit.Ether);
                var hexValue = new HexBigInteger(weiValue);

                // gas refers to the cost necessary to perform a transaction on the network
                var gasPrice = await web3.Eth.GasPrice.SendRequestAsync();

                CallInput callInput = new CallInput
                {
                    From = coinbase,
                    To = address,
                    Value = hexValue
                };

                // the used gas for the simulated call/transaction
                var gas = await web3.Eth.Transactions.EstimateGas.SendRequestAsync(callInput);

                web3.TransactionManager.DefaultGasPrice = gasPrice;
                web3.TransactionManager.DefaultGas = gas;

                // transaction to be sent
                TransactionInput transactionInput = new TransactionInput
                {
                    From = coinbase,
                    To = address,
                    Value = hexValue
                };

                // Sends a transaction to the network
                var transactionHash = await web3.Eth.Transactions.SendTransaction.SendRequestAsync(transactionInput);

                response.Succeeded = true; // it will always be true, unless we get an exception
                response.Result = new BlockchainTransactionModel
                {
                    Account = new AccountModel
                    {
                        CreditAddress = transaction.Account.CreditAddress,
                        DebitAddress = transaction.Account.DebitAddress
                    },
                    Id = transaction.Id,
                    Value = transaction.Value,
                    Statement = transaction.Statement,
                    TxHash = transactionHash // WHAT ABOUT TxStatus ???
                };

                // locks the given account, which was previously unlocked
                var lockAccount = await web3.Personal.LockAccount.SendRequestAsync(coinbase);
            }
            catch (Exception ex)
            {
                response.Succeeded = false;
                response.Message = ex.Message;
                _logger.Information($"EthereumAdapter.SendTransaction(TransactionId: {transaction.Id}, CryptoAdapterId: {cryptoAdapter.Id})");
                _logger.Error(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Sends multiple transactions to the blockchain network
        /// </summary>
        /// <param name="transactions">Information about transactions that will be sent</param>
        /// <param name="cryptoAdapter">Information about Ethereum network</param>
        /// <returns>List of transaction results</returns>
        public async Task<Response<List<TransactionResponse<BlockchainTransactionModel>>>> SendTransactions(List<BlockchainTransactionModel> transactions, CryptoAdapterModel cryptoAdapter)
        {
            // url to the appropriate node
            var web3 = InstantiateWeb3(cryptoAdapter);

            var response = new Response<List<TransactionResponse<BlockchainTransactionModel>>>
            {
                Value = new List<TransactionResponse<BlockchainTransactionModel>>()
            };

            // credentials for coinbase.com (platform for buying/selling digital currency) ?
            var coinbase = cryptoAdapter.Properties.FirstOrDefault().DestinationProperties.FirstOrDefault(dp => dp.Id == (long)PropertyEnum.Coinbase).Value;
            var password = cryptoAdapter.Properties.FirstOrDefault().DestinationProperties.FirstOrDefault(dp => dp.Id == (long)PropertyEnum.CoinbasePassword).Value;

            try
            {
                // signs data using a specific account
                var unlockAccount = await web3.Personal.UnlockAccount.SendRequestAsync(coinbase, password, 0);

                foreach (var transaction in transactions)
                {
                    var responseItem = new TransactionResponse<BlockchainTransactionModel>();

                    try
                    {
                        // credit or debit
                        var statement = transaction.Statement;

                        var address = String.Empty;

                        // takes Credit or Debit address from given transaction
                        switch (statement)
                        {
                            case Statement.Credit:
                                address = transaction.Account.CreditAddress;
                                break;
                            case Statement.Debit:
                                address = transaction.Account.DebitAddress;
                                break;
                        }

                        // takes transaction value from the given transaction
                        var value = transaction.Value;

                        // converts transaction value to Wei (= base unit of ether)
                        var weiValue = Web3.Convert.ToWei(value, Nethereum.Util.UnitConversion.EthUnit.Ether);
                        var hexValue = new HexBigInteger(weiValue);

                        // gas refers to the cost necessary to perform a transaction on the network
                        var gasPrice = await web3.Eth.GasPrice.SendRequestAsync();

                        CallInput callInput = new CallInput
                        {
                            From = coinbase,
                            To = address,
                            Value = hexValue
                        };

                        // the used gas for the simulated call/transaction
                        var gas = await web3.Eth.Transactions.EstimateGas.SendRequestAsync(callInput);

                        web3.TransactionManager.DefaultGas = gas;
                        web3.TransactionManager.DefaultGasPrice = gasPrice;

                        // transaction to be sent
                        TransactionInput transactionInput = new TransactionInput
                        {
                            From = coinbase,
                            To = address,
                            Value = hexValue
                        };

                        // Sends a transaction to the network
                        var transactionHash = await web3.Eth.Transactions.SendTransaction.SendRequestAsync(transactionInput);

                        responseItem.Succeeded = true;
                        responseItem.Result = new BlockchainTransactionModel
                        {
                            Id = transaction.Id,
                            Account = new AccountModel
                            {
                                DebitAddress = transaction.Account.DebitAddress,
                                CreditAddress = transaction.Account.CreditAddress
                            },
                            Value = value,
                            Statement = statement,
                            TxHash = transactionHash,
                            TxStatus = TxStatus.Pending
                        };
                    }
                    catch (Exception ex)
                    {
                        responseItem.Succeeded = false;
                        response.Message = ex.Message;
                        _logger.Information($"EthereumAdapter.SendTransactions(TransactionId: {transaction.Id}, CryptoAdapterId: {cryptoAdapter.Id})");
                        _logger.Error(ex.Message);
                    }

                    response.Value.Add(responseItem);
                }

                // locks the given account, which was previously unlocked
                var lockAccount = await web3.Personal.LockAccount.SendRequestAsync(coinbase);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;

                if (response.Value.Count() >= 1)
                {
                    response.Status = StatusEnum.Success;
                }
                else
                {
                    response.Status = StatusEnum.Error;
                }

                _logger.Information($"EthereumAdapter.SendTransactions(CryptoAdapterId: {cryptoAdapter.Id})");
                _logger.Error(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Gets the most recent result for transaction status
        /// </summary>
        /// <param name="transaction">Transaction information</param>
        /// <param name="cryptoAdapter">Information about Ethereum network</param>
        /// <returns>Transaction information with updated transaction status</returns>
        public async Task<TransactionResponse<BlockchainTransactionModel>> GetTransactionStatus(BlockchainTransactionModel transaction, CryptoAdapterModel cryptoAdapter)
        {

            var web3 = InstantiateWeb3(cryptoAdapter);

            var response = new TransactionResponse<BlockchainTransactionModel>();

            try
            {
                // the receipt of a transaction by transaction hash (or null if no receipt was found)
                var transactionReceipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transaction.TxHash);

                // gets the status of the transaction
                var transactionStatus = transactionReceipt.Status?.Value;

                response.Succeeded = true;
                var transactionStatusConverted = ConvertStatus(transactionStatus ?? default(BigInteger));

                response.Result = new BlockchainTransactionModel
                {
                    TxHash = transaction.TxHash,
                    TxStatus = transactionStatusConverted
                };
            }
            catch (Exception ex)
            {
                response.Succeeded = false;
                response.Message = ex.Message;

                response.Result = new BlockchainTransactionModel
                {
                    TxHash = transaction.TxHash,
                    TxStatus = TxStatus.None
                };

                _logger.Information($"EthereumAdapter.GetTransactionStatus(TransactionId: {transaction.Id}, CryptoAdapterId: {cryptoAdapter.Id})");
                _logger.Error(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Gets the most recent result for status of all transactions in the list
        /// </summary>
        /// <param name="transactions">Transactions information</param>
        /// <param name="cryptoAdapter">Information about Ethereum network</param>
        /// <returns>Transaction information with updated status value for each transaction in the list</returns>
        public async Task<List<TransactionResponse<BlockchainTransactionModel>>> GetTransactionsStatus(List<BlockchainTransactionModel> transactions, CryptoAdapterModel cryptoAdapter)
        {
            var web3 = InstantiateWeb3(cryptoAdapter);

            var response = new List<TransactionResponse<BlockchainTransactionModel>>();

            foreach (var transaction in transactions)
            {
                var responseItem = new TransactionResponse<BlockchainTransactionModel>();

                try
                {
                    // the receipt of a transaction by transaction hash (or null if no receipt was found)
                    var transactionReceipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transaction.TxHash);

                    // gets the status of the transaction
                    var transactionStatus = transactionReceipt.Status?.Value;

                    responseItem.Succeeded = true;
                    var transactionStatusConverted = ConvertStatus(transactionStatus ?? default(BigInteger));

                    responseItem.Result = new BlockchainTransactionModel
                    {
                        TxHash = transaction.TxHash,
                        TxStatus = transactionStatusConverted
                    };
                }
                catch (Exception ex)
                {
                    responseItem.Succeeded = false;
                    responseItem.Message = ex.Message;
                    responseItem.Result = new BlockchainTransactionModel
                    {
                        TxHash = transaction.TxHash,
                        TxStatus = TxStatus.None
                    };

                    _logger.Information($"EthereumAdapter.GetTransactionsStatus(TransactionId: {transaction.Id}, CryptoAdapterId: {cryptoAdapter.Id})");
                    _logger.Error(ex.Message);
                }

                response.Add(responseItem);
            }

            return response;
        }

        /// <summary>
        /// Creates a new account in the Ethereum network
        /// </summary>
        /// <param name="cryptoAdapter">Information about Ethereum network</param>
        /// <param name="newAccountPassword">The password to encrypt new account with</param>
        /// <returns>New account</returns>
        public async Task<Response<AccountModel>> NewAccount(CryptoAdapterModel cryptoAdapter, string newAccountPassword)
        {
            var web3 = InstantiateWeb3(cryptoAdapter);

            var response = new Response<AccountModel>();

            try
            {
                // gets addresses of newly created accounts
                var newDebitAccount = await web3.Personal.NewAccount.SendRequestAsync(newAccountPassword);
                var newCreditAccount = await web3.Personal.NewAccount.SendRequestAsync(newAccountPassword);

                response.Status = StatusEnum.Success;
                response.Value = new AccountModel
                {
                    DebitAddress = newDebitAccount,
                    CreditAddress = newCreditAccount
                };

            }
            catch (Exception ex)
            {
                response.Status = StatusEnum.Error;
                response.Message = Message.SomethingWentWrong;
                _logger.Information($"EthereumAdapter.NewAccount(CryptoAdapterId: {cryptoAdapter.Id}, newAccountPassword: {newAccountPassword})");
                _logger.Error(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Tests the connection with the provided credentials in the cryptoAdapter
        /// </summary>
        /// <param name="cryptoAdapter">Information about Ethereum network</param>
        /// <returns>Information about test connection to destination</returns>
        public async Task<Response<NoValue>> TestConnectionDestination(CryptoAdapterModel cryptoAdapter)
        {
            var response = new Response<NoValue>();
            Web3 web3;

            var coinbase = cryptoAdapter.Properties.FirstOrDefault().DestinationProperties.FirstOrDefault(dp => dp.Id == (long)PropertyEnum.Coinbase).Value;
            var password = cryptoAdapter.Properties.FirstOrDefault().DestinationProperties.FirstOrDefault(dp => dp.Id == (long)PropertyEnum.CoinbasePassword).Value;

            web3 = InstantiateWeb3(cryptoAdapter);

            try
            {
                // WHY there is no LockAccount anywhere afterwards ???
                var unlockAccount = await web3.Personal.UnlockAccount.SendRequestAsync(coinbase, password, 1);

                response.Status = StatusEnum.Success;
            }
            // WHERE to find all possible exceptions ?
            catch (Exception ex)
            {
                _logger.Information($"EthereumAdapter.TestConnection(Ip: {cryptoAdapter.RpcAddr}, Port: {cryptoAdapter.RpcPort}, Coinbase: {coinbase}, Password: {password})");

                if (ex.Message.Equals("could not decrypt key with given passphrase", StringComparison.InvariantCultureIgnoreCase))
                {
                    response.Message = "Test connection failed. Reason: Incorrect address or password.";
                    _logger.Error(ex.Message);
                }
                else if (ex.Message.Equals("invalid argument 0: json: cannot unmarshal hex string of odd length into Go value of type common.Address", StringComparison.InvariantCultureIgnoreCase) || ex.Message.Equals("no key for given address or file", StringComparison.InvariantCultureIgnoreCase))
                {
                    response.Message = "Test connection failed. Reason: Incorrect coinbase address.";
                    _logger.Error(ex.Message);
                }

                else if (ex.InnerException != null)
                {
                    if (ex.InnerException.InnerException != null)
                    {
                        if (ex.InnerException.InnerException.Message.Equals("Unable to connect to the remote server", StringComparison.InvariantCultureIgnoreCase))
                        {
                            response.Message = "Test connection failed. Reason: Incorrect IP address or port.";
                            _logger.Error(ex.InnerException.InnerException.Message);
                        }
                        else
                        {
                            response.Message = Message.TestConnectionFailed;
                            _logger.Error(ex.InnerException.InnerException.Message);
                        }
                    }
                    else
                    {
                        response.Message = Message.TestConnectionFailed;
                        _logger.Error(ex.InnerException.Message);
                    }
                }
                else
                {
                    response.Message = Message.TestConnectionFailed;
                    _logger.Error(ex.Message);
                }

                response.Status = StatusEnum.Error;
            }

            return response;
        }

        /// <summary>
        /// Tests the connection to the source
        /// </summary>
        /// <param name="cryptoAdapter">Information about Ethereum network</param>
        /// <returns>Information about test connection to source</returns>
        public async Task<Response<NoValue>> TestConnectionSource(CryptoAdapterModel cryptoAdapter)
        {
            // WHAT is source?

            var response = new Response<NoValue>();

            var web3 = InstantiateWeb3(cryptoAdapter);

            try
            {
                // get the current block number
                var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();

                response.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                response.Message = Message.TestConnectionFailed;
                response.Status = StatusEnum.Error;
                _logger.Information($"EthereumAdapter.TestConnectionSource(CryptoAdapterId: {cryptoAdapter.Id})");
                _logger.Error(ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Gets the Ethereum blocks in the specified range
        /// </summary>
        /// <param name="cryptoAdapter">Information about Ethereum network</param>
        /// <param name="fromBlock">First block to get</param>
        /// <param name="toBlock">Last block to get</param>
        /// <param name="address">Address to filter transactions ???</param>
        /// <returns>List of Ethereum blocks</returns>
        public async Task<Response<List<EthereumBlockModel>>> GetBlocksWithTransactions(CryptoAdapterModel cryptoAdapter, int fromBlock, int toBlock, string address)
        {
            var response = new Response<List<EthereumBlockModel>>()
            {
                Value = new List<EthereumBlockModel>()
            };

            var web3 = InstantiateWeb3(cryptoAdapter);

            try
            {
                if (fromBlock > toBlock)
                {
                    _logger.Information($"EthereumAdapter.GetBlocksWithTransactions(cryptoAdapter: {cryptoAdapter.Id}, fromBlock: {fromBlock}, toBlock: {toBlock}, address: {address}");
                    _logger.Error($"FromBlock value {fromBlock} is greater than ToBlock value {toBlock}");
                    response.Status = StatusEnum.Error;
                    return response;
                }

                var stopwatch = Stopwatch.StartNew();

                for (int i = fromBlock; i <= toBlock; i++)
                {
                    // returns a block matching the block number
                    var block = await web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(new HexBigInteger(i));

                    // block information of importance
                    var blockModel = new EthereumBlockModel
                    {
                        BlockNumber = (int)block.Number.Value,
                        TimeStamp = UnixTimeStampToDateTime((double)block.Timestamp.Value),
                        BlockTransactions = new List<EthereumBlockTransactionModel>()
                    };

                    // transaction information of importance in the current block
                    foreach (var transaction in block.Transactions)
                    {
                        var blockTransactionModel = new EthereumBlockTransactionModel
                        {
                            Hash = transaction.TransactionHash,
                            Value = Web3.Convert.FromWei(transaction.Value), // converts transaction value from Wei (= base unit of ether)
                            From = transaction.From,
                            To = transaction.To
                        };

                        address = address.ToLower();

                        if (address == String.Empty || (String.Equals(transaction.From.ToLower(), address) || String.Equals(transaction.To?.ToLower(), address)))
                        {
                            // gets the receipt of a transaction by transaction hash
                            var transactionReceipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transaction.TransactionHash);
                            var status = transactionReceipt.Status?.Value;

                            blockTransactionModel.Status = ConvertStatus(status);

                            blockModel.BlockTransactions.Add(blockTransactionModel);
                        }
                    }
                    response.Value.Add(blockModel);
                }

                stopwatch.Stop();
                _logger.Information($"EthereumAdapter.GetBlocksWithTransactions(cryptoAdapter: {cryptoAdapter.Id}, fromBlock: {fromBlock}, toBlock: {toBlock}, address: {address}). Time elapsed: {stopwatch.Elapsed.TotalSeconds} seconds.");

                response.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                _logger.Information($"EthereumAdapter.GetBlocksWithTransactions(cryptoAdapter: {cryptoAdapter.Id}, fromBlock: {fromBlock}, toBlock: {toBlock}, address: {address}");
                _logger.Error(ex.Message);
                response.Status = StatusEnum.Error;
                response.Message = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Gets current block number
        /// </summary>
        /// <param name="cryptoAdapter">Information about Ethereum network</param>
        /// <returns>Current block number</returns>
        public async Task<Response<int>> GetCurrentBlockNumber(CryptoAdapterModel cryptoAdapter)
        {
            var response = new Response<int>();

            var web3 = InstantiateWeb3(cryptoAdapter);

            try
            {
                // get the current block number
                var blockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();

                response.Value = (int)(blockNumber.Value);
                response.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                response.Message = Message.SomethingWentWrong;
                response.Status = StatusEnum.Error;
                _logger.Information($"EthereumAdapter.GetCurrentBlockNumber(CryptoAdapterId: {cryptoAdapter.Id})");
                _logger.Error(ex.Message);
            }

            return response;
        }

        public bool StartMining()
        {
            return true;
        }

        public bool StopMining()
        {
            return true;
        }

        #region Helpers

        /// <summary>
        /// Provides a simple interaction wrapper to access the RPC methods provided by the Ethereum
        /// </summary>
        /// <param name="cryptoAdapter">Information about Ethereum network</param>
        /// <returns>Web3 instance for provided crypto adapter</returns>
        private static Web3 InstantiateWeb3(CryptoAdapterModel cryptoAdapter)
        {
            Web3 web3;

            web3 = new Web3("http://" + cryptoAdapter.RpcAddr + ":" + cryptoAdapter.RpcPort + "/");

            return web3;
        }

        /// <summary>
        /// Converts BigInteger value to the enum TxStatus value
        /// </summary>
        /// <param name="txStatus">Transaction status represented as BigInteger</param>
        /// <returns>Corresponding TxStatus enum value</returns>
        private static TxStatus ConvertStatus(BigInteger? txStatus)
        {
            var status = TxStatus.None;

            if (txStatus == null)
            {
                return status;
            }
            else
            {
                var txStatusInt = (int)txStatus;

                switch (txStatusInt)
                {
                    case 0:
                        status = TxStatus.Fail;
                        break;

                    case 1:
                        status = TxStatus.Success;
                        break;
                }

                return status;
            }
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
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc); // count starts at the Unix Epoch on January 1st, 1970 at UTC
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }

        #endregion

    }
}
