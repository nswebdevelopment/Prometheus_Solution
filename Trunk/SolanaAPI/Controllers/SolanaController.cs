using Microsoft.AspNetCore.Mvc;
using Prometheus.BlockchainAdapter.Models;
using Prometheus.Model.Models;
using Prometheus.SolanaAPI.Helpers;
using Solnet.Rpc;
using Solnet.Wallet;
using Solnet.Wallet.Bip39;
using static Prometheus.SolanaAPI.Enums.Enums;

namespace SolanaAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SolanaController : ControllerBase
    {

        private readonly Serilog.ILogger _logger;

        public SolanaController(Serilog.ILogger logger)
        {
            _logger = logger;
        }

        [HttpGet("GetBlocksWithTransactions/{fromBlock}/{toBlock}")]
        public List<Prometheus.SolanaAPI.Models.SolanaBlockModel> GetBlocksWithTransactions(int fromBlock, int toBlock)
        {
            var rpcClient = ClientFactory.GetClient(Cluster.DevNet);
            var response = new List<Prometheus.SolanaAPI.Models.SolanaBlockModel>();

            for (int i = fromBlock; i <= toBlock; i++)
            {
                var block = rpcClient.GetBlock(Convert.ToUInt64(i));


                var blockModel = new Prometheus.SolanaAPI.Models.SolanaBlockModel
                {
                    BlockNumber = i,

                    TimeStamp = UnixTimeStampToDateTime(block.Result.BlockTime),
                    BlockTransactions = new List<Prometheus.SolanaAPI.Models.SolanaBlockTransactionModel>()
                };

                foreach (var transaction in block.Result.Transactions)
                {
                    var blockTransactionModel = new Prometheus.SolanaAPI.Models.SolanaBlockTransactionModel
                    {
                        Hash = transaction.Transaction.Signatures[0],
                        Value = transaction.Meta.Fee,
                        Status = ConvertStatus(transaction.Meta.Error)
                    };

                    blockModel.BlockTransactions.Add(blockTransactionModel);
                }
                response.Add(blockModel);
            }

            return response;
        }

        [HttpGet("SendTransactions")]
        public List<TransactionResponse<BlockchainTransactionModel>> SendTransactions()
        {
            var rpcClient = ClientFactory.GetClient(Cluster.DevNet);
            var wallet = new Wallet("void exchange behind cousin online funny genius sun clip address nephew purity neck margin boy economy wagon destroy choice churn kid bone dress desk", WordList.English);
            var result = new List<TransactionResponse<BlockchainTransactionModel>>();

            try
            {
                var txList = SolanaHelper.ConnectAndRead();

                foreach (var t in txList)
                {
                    var resultItem = new TransactionResponse<BlockchainTransactionModel>();
                    var amount = ulong.Parse(t.TransactionAmount);
                    var transaction = SolanaHelper.SendTransaction(wallet, rpcClient, amount);
                    BlockchainTransactionModel transactionModel = new BlockchainTransactionModel
                    {
                        Id = long.Parse(t.TransactionId),
                        TxHash = transaction.Result.GetHashCode().ToString(),
                        Value = amount,
                        TxStatus = transaction.WasSuccessful ? Prometheus.Common.Enums.TxStatus.Success : Prometheus.Common.Enums.TxStatus.Fail
                    };
                    resultItem.Succeeded = transaction.WasSuccessful ? true : false;
                    resultItem.Result = transactionModel;
                    result.Add(resultItem);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in {SendTransactions} method: ",ex.Message);
                return null;
            }
        }

        [HttpGet("TestConnection")]
        public string TestConnection()
        {
            var rpcClient = ClientFactory.GetClient(Cluster.DevNet);

            var genesisCheck = rpcClient.GetGenesisHash().HttpStatusCode.ToString();
            if (genesisCheck.Equals("OK"))
            {
                return "OK";
            }
            else
            {
                return "Error";
            }
        }
        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc); // count starts at the Unix Epoch on January 1st, 1970 at UTC
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }

        private static TxStatus ConvertStatus(object error)
        {
            var status = TxStatus.None;

            if (error == null)
            {
                status = TxStatus.Success;
            }
            else
            {
                status = TxStatus.Fail;
            }

            return status;
        }
    }
}