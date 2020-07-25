using Prometheus.BL.Interfaces;
using Prometheus.BlockchainAdapter.Models;
using Prometheus.BlockchainAdapter.Nodes;
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
using System.Threading.Tasks;

namespace Prometheus.BL.Services
{
    public class BlockchainService : IBlockchainService
    {
        private readonly IPrometheusEntities _entities;
        private readonly ILogger _logger;
        private readonly IJobDefinitionService _jobDefinitionService;
        private readonly ICryptoAdapterService _cryptoAdapterService;

        public BlockchainService(IPrometheusEntities entities, ILogger logger, IJobDefinitionService jobDefinitionService, ICryptoAdapterService cryptoAdapterService)
        {
            _entities = entities;
            _logger = logger;
            _jobDefinitionService = jobDefinitionService;
            _cryptoAdapterService = cryptoAdapterService;
        }

        public async Task<Response<List<TransactionResponse<BlockchainTransactionModel>>>> SendToBlockchain(BlockchainDataModel blockchainDataModel)
        {
            var response = new Response<List<TransactionResponse<BlockchainTransactionModel>>>()
            {
                Value = new List<TransactionResponse<BlockchainTransactionModel>>()
            };

            try
            {
                var jobTimeLine = _entities.JobTimeline.FirstOrDefault(jt => jt.Id == blockchainDataModel.JobId);

                var cryptoAdapterId = jobTimeLine.Schedule.JobDefinition.Adapter1.CryptoAdapter.FirstOrDefault().Id;

                var cryptoAdapter = _cryptoAdapterService.GetCryptoAdapter(cryptoAdapterId);

                if (cryptoAdapter.Status != StatusEnum.Success)
                {
                    return response;
                }

                switch (cryptoAdapter.Value.NodeType)
                {
                    case AdapterTypeItemEnum.Cardano:
                        var cardano = new CardanoAdapter();

                        break;
                    case AdapterTypeItemEnum.EOS:
                        var eos = new EOSAdapter();

                        break;
                    case AdapterTypeItemEnum.Ethereum:
                        var ethAdapter = new EthereumAdapter(_logger);

                        response = await ethAdapter.SendTransactions(blockchainDataModel.Transactions, cryptoAdapter.Value);

                        break;
                    case AdapterTypeItemEnum.NEO:
                        var neo = new NEOAdapter(_logger);

                        break;
                    case AdapterTypeItemEnum.Bitcoin:
                        var btcAdapter = new BitcoinAdapter(_logger);

                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Information($"BlockchainService.SendToBlockchain(blockchainDataModel: {blockchainDataModel})");
                _logger.Error(ex.Message);
                response.Status = StatusEnum.Error;
            }

            return response;

        }

        public async Task<IResponse<AccountModel>> NewAccount(long cryptoAdapterId)
        {
            var response = new Response<AccountModel>();

            try
            {
                var cryptoAdapter = _entities.CryptoAdapter.Find(cryptoAdapterId);

                var cryptoAdapterModel = new CryptoAdapterModel
                {
                    RpcAddr = cryptoAdapter.RpcAddr,
                };

                if (cryptoAdapter.RpcPort != null)
                {
                    cryptoAdapterModel.RpcPort = UInt16.Parse(cryptoAdapter.RpcPort);
                }

                var newAccountPassword = cryptoAdapter.CryptoAdapterProperty.FirstOrDefault(cap => cap.PropertyId == (long)PropertyEnum.NewAccountPassword).Value;
                var nodeType = (AdapterTypeItemEnum)cryptoAdapter.Adapter.AdapterTypeItemId;

                switch (nodeType)
                {
                    case AdapterTypeItemEnum.Ethereum:
                        var adapter = new EthereumAdapter(_logger);

                        response = await adapter.NewAccount(cryptoAdapterModel, newAccountPassword);

                        break;
                    case AdapterTypeItemEnum.Cardano:

                        break;
                    case AdapterTypeItemEnum.EOS:

                        break;
                    case AdapterTypeItemEnum.NEO:

                        break;
                    default:
                        //add logger
                        throw new Exception();
                }
            }
            catch (Exception ex)
            {
                response.Status = StatusEnum.Error;
                response.Message = Message.SomethingWentWrong;
                _logger.Information($"BlockchainService.NewAccount(cryptoAdapterId: {cryptoAdapterId})");
                _logger.Error(ex.Message);
            }

            return response;
        }

        public async Task<TransactionResponse<BlockchainTransactionModel>> GetTransactionStatus(BlockchainTransactionModel transaction, long cryptoAdapterId)
        {
            var response = new TransactionResponse<BlockchainTransactionModel>();
            try
            {
                var cryptoAdapter = _entities.CryptoAdapter.Find(cryptoAdapterId);

                var cryptoAdapterModel = new CryptoAdapterModel
                {
                    RpcAddr = cryptoAdapter.RpcAddr
                };

                if (cryptoAdapter.RpcPort != null)
                {
                    cryptoAdapterModel.RpcPort = UInt16.Parse(cryptoAdapter.RpcPort);
                }

                var nodeType = (AdapterTypeItemEnum)cryptoAdapter.Adapter.AdapterTypeItemId;

                switch (nodeType)
                {
                    case AdapterTypeItemEnum.Ethereum:
                        var adapter = new EthereumAdapter(_logger);

                        response = await adapter.GetTransactionStatus(transaction, cryptoAdapterModel);

                        break;
                    case AdapterTypeItemEnum.Cardano:

                        break;
                    case AdapterTypeItemEnum.EOS:

                        break;
                    case AdapterTypeItemEnum.NEO:

                        break;
                    default:
                        //add logger
                        throw new Exception();
                }
            }
            catch (Exception ex)
            {
                response.Succeeded = false;
                response.Message = Message.SomethingWentWrong;
                _logger.Information($"BlockchainService.GetTransactionStatus(transaction: {transaction}, cryptoAdapterId: {cryptoAdapterId})");
                _logger.Error(ex.Message);
            }

            return response;
        }

        public async Task<Response<int>> GetCurrentBlockNumber(long adapterId, AdapterTypeItemEnum adapterType)
        {
            var response = new Response<int>();

            try
            {
                var cryptoAdapter = _entities.Adapter.Find(adapterId).CryptoAdapter.FirstOrDefault();

                var cryptoAdapterModel = new CryptoAdapterModel
                {
                    RpcAddr = cryptoAdapter.RpcAddr
                };

                if (cryptoAdapter.RpcPort != null)
                {
                    cryptoAdapterModel.RpcPort = UInt16.Parse(cryptoAdapter.RpcPort);
                }

                switch (adapterType)
                {
                    case AdapterTypeItemEnum.Ethereum:

                        var ethAdapter = new EthereumAdapter(_logger);

                        response = await ethAdapter.GetCurrentBlockNumber(cryptoAdapterModel);

                        break;
                    case AdapterTypeItemEnum.Bitcoin:

                        var btcUsername = cryptoAdapter.CryptoAdapterProperty.FirstOrDefault(cap => cap.PropertyId == (long)PropertyEnum.RpcUsername).Value;
                        var btcPassword = cryptoAdapter.CryptoAdapterProperty.FirstOrDefault(cap => cap.PropertyId == (long)PropertyEnum.RpcPassword).Value;

                        var btcAdapter = new BitcoinAdapter(_logger);

                        response = btcAdapter.GetCurrentBlockNumber(cryptoAdapterModel, btcUsername, btcPassword);

                        break;
                    case AdapterTypeItemEnum.NEO:

                        var neoAdapter = new NEOAdapter(_logger);

                        response = await neoAdapter.GetCurrentBlockNumber(cryptoAdapterModel);

                        break;
                    case AdapterTypeItemEnum.Litecoin:

                        var ltcUsername = cryptoAdapter.CryptoAdapterProperty.FirstOrDefault(cap => cap.PropertyId == (long)PropertyEnum.RpcUsername).Value;
                        var ltcPassword = cryptoAdapter.CryptoAdapterProperty.FirstOrDefault(cap => cap.PropertyId == (long)PropertyEnum.RpcPassword).Value;

                        var ltcAdapter = new LitecoinAdapter(_logger);

                        response = ltcAdapter.GetCurrentBlockNumber(cryptoAdapterModel, ltcUsername, ltcPassword);

                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                response.Status = StatusEnum.Error;
                response.Message = Message.SomethingWentWrong;
                _logger.Information($"BlockchainService.GetCurrentBlockNumber(adapterId: {adapterId}, adapterType: {adapterType}");
                _logger.Error(ex.Message);

            }

            return response;
        }

        public async Task<Response<List<T>>> GetBlocksWithTransactions<T>(long adapterId, int fromBlock, int toBlock, string address)
        {
            var response = new Response<List<T>>()
            {
                Value = new List<T>()
            };

            try
            {
                var cryptoAdapter = _entities.Adapter.Find(adapterId).CryptoAdapter.FirstOrDefault();

                var cryptoAdapterModel = new CryptoAdapterModel
                {
                    Id = cryptoAdapter.Id,
                    RpcAddr = cryptoAdapter.RpcAddr,
                    RpcPort = Convert.ToUInt16(cryptoAdapter.RpcPort)
                };

                if (typeof(T) == typeof(EthereumBlockModel))
                {
                    var ethereumResponse = new Response<List<EthereumBlockModel>>()
                    {
                        Value = new List<EthereumBlockModel>()
                    };

                    var adapter = new EthereumAdapter(_logger);

                    ethereumResponse = await adapter.GetBlocksWithTransactions(cryptoAdapterModel, fromBlock, toBlock, address);

                    response = (Response<List<T>>)Convert.ChangeType(ethereumResponse, typeof(Response<List<T>>));
                }
                else if (typeof(T) == typeof(BitcoinBlockModel))
                {
                    var bitcoinResponse = new Response<List<BitcoinBlockModel>>()
                    {
                        Value = new List<BitcoinBlockModel>()
                    };

                    var username = cryptoAdapter.CryptoAdapterProperty.FirstOrDefault(cap => cap.PropertyId == (long)PropertyEnum.RpcUsername).Value;
                    var password = cryptoAdapter.CryptoAdapterProperty.FirstOrDefault(cap => cap.PropertyId == (long)PropertyEnum.RpcPassword).Value;

                    var adapter = new BitcoinAdapter(_logger);

                    bitcoinResponse = adapter.GetBlocksWithTransactions(cryptoAdapterModel, username, password, fromBlock, toBlock, address);

                    response = (Response<List<T>>)Convert.ChangeType(bitcoinResponse, typeof(Response<List<T>>));

                }
                else if (typeof(T) == typeof(LitecoinBlockModel))
                {
                    var litecoinResponse = new Response<List<LitecoinBlockModel>>()
                    {
                        Value = new List<LitecoinBlockModel>()
                    };

                    var username = cryptoAdapter.CryptoAdapterProperty.FirstOrDefault(cap => cap.PropertyId == (long)PropertyEnum.RpcUsername).Value;
                    var password = cryptoAdapter.CryptoAdapterProperty.FirstOrDefault(cap => cap.PropertyId == (long)PropertyEnum.RpcPassword).Value;

                    var adapter = new LitecoinAdapter(_logger);

                    litecoinResponse = adapter.GetBlocksWithTransactions(cryptoAdapterModel, username, password, fromBlock, toBlock, address);

                    response = (Response<List<T>>)Convert.ChangeType(litecoinResponse, typeof(Response<List<T>>));
                }
                else if (typeof(T) == typeof(NeoBlockModel))
                {
                    var neoResponse = new Response<List<NeoBlockModel>>()
                    {
                        Value = new List<NeoBlockModel>()
                    };

                    var adapter = new NEOAdapter(_logger);

                    neoResponse = await adapter.GetBlocksWithTransactions(cryptoAdapterModel, fromBlock, toBlock, address);

                    response = (Response<List<T>>)Convert.ChangeType(neoResponse, typeof(Response<List<T>>));
                }
            }
            catch (Exception ex)
            {
                response.Status = StatusEnum.Error;
                response.Message = Message.SomethingWentWrong;
                _logger.Information($"BlockchainService.GetBlocksWithTransactions(adapterId: {adapterId}, fromBlock: {fromBlock}), toBlock: {toBlock}, address: {address}");
                _logger.Error(ex.Message);
            }

            return response;
        }

    }
}
