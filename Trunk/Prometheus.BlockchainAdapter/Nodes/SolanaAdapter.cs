using Newtonsoft.Json;
using Prometheus.BlockchainAdapter.Models;
using Prometheus.Common;
using Prometheus.Common.Enums;
using Prometheus.Model.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.BlockchainAdapter.Nodes
{
    public class SolanaAdapter
    {
        private readonly Serilog.ILogger _logger;

        public SolanaAdapter(Serilog.ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Sends multiple transactions to the blockchain network
        /// </summary>
        /// <param name="transactions">Information about transactions that will be sent</param>
        /// <param name="cryptoAdapter">Information about Ethereum network</param>
        /// <returns>List of transaction results</returns>
        public async Task<Response<List<TransactionResponse<BlockchainTransactionModel>>>> SendTransactions(List<BlockchainTransactionModel> transactions, CryptoAdapterModel cryptoAdapter)
        {
            

            var response = new Response<List<TransactionResponse<BlockchainTransactionModel>>>
            {
                Value = new List<TransactionResponse<BlockchainTransactionModel>>()
            };

            

            try
            {
                

            }
            catch (Exception ex)
            {

                response.Message = ex.Message;
                _logger.Information($"SolanaAdapter.SendTransactions(TransactionId:, CryptoAdapterId: {cryptoAdapter.Id})");
                _logger.Error(ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Tests the connection to the source
        /// </summary>
        /// <param name="cryptoAdapter">Information about Solana network</param>
        /// <returns>Information about test connection to source</returns>
        public async Task<Response<NoValue>> TestConnectionSource(CryptoAdapterModel cryptoAdapter)
        {
            // WHAT is source?

            var response = new Response<NoValue>();

       
            try
            {
                HttpClient httpClient = new HttpClient();
                var result = httpClient.GetAsync("http://localhost:5208/Solana/TestConnection").Result.Content.ReadAsStringAsync().Result;
                if (result.Equals("OK"))
                {
                    response.Status = StatusEnum.Success;
                }
            }
            catch (Exception ex)
            {
                response.Message = Message.TestConnectionFailed;
                response.Status = StatusEnum.Error;
                _logger.Information($"SolanaAdapter.TestConnectionSource(CryptoAdapterId: {cryptoAdapter.Id})");
                _logger.Error(ex.Message);
            }

            return response;
        }

        public async Task<Response<List<SolanaBlockModel>>> GetBlocksWithTransactions(CryptoAdapterModel cryptoAdapter, int fromBlock, int toBlock, string address)
        {
            var response = new Response<List<SolanaBlockModel>>()
            {
                Value = new List<SolanaBlockModel>()
            };

           
            try
            {
                HttpClient httpClient = new HttpClient();
                var result = httpClient.GetAsync("http://localhost:5208/Solana/GetBlocksWithTransactions/" + fromBlock + "/" + toBlock).Result.Content.ReadAsStringAsync().Result;

                response.Status = StatusEnum.Success;
                response.Value = JsonConvert.DeserializeObject<List<SolanaBlockModel>>(result);
            }
            catch (Exception ex)
            {
                _logger.Information($"SolanaAdapter.GetBlocksWithTransactions(cryptoAdapter: {cryptoAdapter.Id}, fromBlock: {fromBlock}, toBlock: {toBlock}, address: {address}");
                _logger.Error(ex.Message);
                response.Status = StatusEnum.Error;
                response.Message = ex.Message;
            }

            return response;
        }

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc); // count starts at the Unix Epoch on January 1st, 1970 at UTC
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }
    }
}
