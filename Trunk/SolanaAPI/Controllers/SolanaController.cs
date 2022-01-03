using Microsoft.AspNetCore.Mvc;
using Prometheus.SolanaAPI.Models;
using Solnet.Rpc;
using static Prometheus.SolanaAPI.Enums.Enums;

namespace SolanaAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SolanaController : ControllerBase
    {
       
        private readonly ILogger<SolanaController> _logger;

        public SolanaController(ILogger<SolanaController> logger)
        {
            _logger = logger;
        }

        [HttpGet("GetBlocksWithTransactions/{fromBlock}/{toBlock}")]
        public List<SolanaBlockModel> GetBlocksWithTransactions(int fromBlock, int toBlock)
        {
            var rpcClient = ClientFactory.GetClient(Cluster.DevNet);
            var response = new List<SolanaBlockModel>();

            for (int i = fromBlock; i <= toBlock; i++)
            {
                var block = rpcClient.GetBlock(Convert.ToUInt64(i));
                

                var blockModel = new SolanaBlockModel
                {
                    BlockNumber = i,
                    
                    TimeStamp = UnixTimeStampToDateTime(block.Result.BlockTime),
                    BlockTransactions = new List<SolanaBlockTransactionModel>()
                };

                foreach (var transaction in block.Result.Transactions)
                {
                    var blockTransactionModel = new SolanaBlockTransactionModel
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