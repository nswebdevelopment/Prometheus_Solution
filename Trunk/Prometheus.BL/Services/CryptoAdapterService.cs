using Prometheus.BL.Interfaces;
using Prometheus.Common;
using Prometheus.Common.Enums;
using Prometheus.Dal;
using Prometheus.Dal.Entities;
using Prometheus.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prometheus.BlockchainAdapter.Nodes;
using Serilog;

namespace Prometheus.BL.Services
{
    public class CryptoAdapterService : ICryptoAdapterService
    {
        private readonly IPrometheusEntities _entity;
        private readonly ILogger _logger;

        public CryptoAdapterService(IPrometheusEntities entity, ILogger logger)
        {
            _entity = entity;
            _logger = logger;
        }

        public CryptoAdapterModel GetInitModel()
        {
            var result = new CryptoAdapterModel { Properties = new List<NodeModel>() };

            foreach (var node in Enum.GetValues(typeof(CryptoAdapterType)).Cast<CryptoAdapterType>())
            {
                result.Properties.Add(new NodeModel
                {
                    NodeType = node,
                    SourceProperties = GetProperties(node, DirectionEnum.Source),
                    DestinationProperties = GetProperties(node, DirectionEnum.Destination)
                });
            }

            return result;
        }

        public IResponse<List<CryptoAdapterModel>> GetCryptoAdapters(long userProfileId)
        {
            var result = new Response<List<CryptoAdapterModel>>();

            try
            {
                result.Value = _entity.CryptoAdapter.Where(a => a.Adapter.UserProfileId == userProfileId && a.Adapter.StatusId == (int)Statuses.Active)
                    .Select(a => new CryptoAdapterModel
                    {
                        Id = a.Id,
                        Name = a.Adapter.Name,
                        NodeType = (AdapterTypeItemEnum)a.Adapter.AdapterTypeItemId,
                        Direction = (DirectionEnum)a.Adapter.DirectionId
                    }).ToList();

                result.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                result.Status = StatusEnum.Error;
                result.Message = Message.SomethingWentWrong;
                _logger.Information($"CryptoAdapterService.GetCryptoAdapters(userProfileId: {userProfileId})");
                _logger.Error(ex.Message);
            }

            return result;
        }

        public IResponse<CryptoAdapterModel> GetCryptoAdapter(long cryptoAdapterId)
        {
            var response = new Response<CryptoAdapterModel>();

            try
            {
                var cryptoAdapter = _entity.CryptoAdapter.Find(cryptoAdapterId);

                var cryptoAdapterModel = new CryptoAdapterModel
                {
                    Id = cryptoAdapter.Id,
                    Direction = (DirectionEnum)cryptoAdapter.Adapter.DirectionId,
                    Name = cryptoAdapter.Adapter.Name,
                    NodeType = (AdapterTypeItemEnum)cryptoAdapter.Adapter.AdapterTypeItemId,
                    RpcAddr = cryptoAdapter.RpcAddr,
                    RpcPort = UInt16.Parse(cryptoAdapter.RpcPort),
                    Properties = new List<NodeModel>()
                };

                cryptoAdapterModel.Properties.Add(new NodeModel()
                {
                    NodeType = (CryptoAdapterType)cryptoAdapter.Adapter.AdapterTypeItem.AdapterType.Id,
                    SourceProperties = new List<PropertyModel>(),
                    DestinationProperties = new List<PropertyModel>()
                });


                var properties = _entity.CryptoAdapterProperty
                    .Where(cap => cap.CryptoAdapterId == cryptoAdapter.Id)
                    .Select(s => new PropertyModel
                    {
                        Id = s.Property.Id,
                        Name = s.Property.Name,
                        PropertyType = (PropertyTypeEnum)s.Property.PropertyTypeId,
                        Value = s.Property.PropertyTypeId == (int)PropertyTypeEnum.Password ? String.Empty : s.Value
                    })
                    .ToList();

                switch (cryptoAdapterModel.Direction)
                {
                    case DirectionEnum.Source:
                        cryptoAdapterModel.Properties.First().SourceProperties = properties;
                        break;
                    case DirectionEnum.Destination:
                        cryptoAdapterModel.Properties.First().DestinationProperties = properties;
                        break;
                    default:
                        break;
                }

                response.Status = StatusEnum.Success;
                response.Value = cryptoAdapterModel;
            }
            catch (Exception ex)
            {
                response.Message = Message.SomethingWentWrong;
                response.Status = StatusEnum.Error;
                _logger.Information($"CryptoAdapterService.GetCryptoAdapter(cryptoAdapterId: {cryptoAdapterId})");
                _logger.Error(ex.Message);
            }

            return response;
        }

        public async Task<IResponse<NoValue>> CreateCryptoAdapter(CryptoAdapterModel model, long userProfileId)
        {
            var result = new Response<NoValue>();

            try
            {
                var connection = await TestConnection(model);

                if (connection.Status != StatusEnum.Success)
                {
                    result.Status = StatusEnum.Error;
                    result.Message = connection.Message;
                    return result;
                }

                Adapter newAdapter = new Adapter
                {
                    Name = model.Name,
                    AdapterTypeItemId = (int)model.NodeType,
                    DirectionId = (int)model.Direction,
                    UserProfileId = userProfileId,
                    StatusId = (int)Statuses.Active
                };
                _entity.Adapter.Add(newAdapter);

                CryptoAdapter cryptoAdapter = new CryptoAdapter
                {
                    RpcAddr = model.RpcAddr,
                    RpcPort = model.RpcPort.ToString(),
                    AdapterId = newAdapter.Id
                };
                _entity.CryptoAdapter.Add(cryptoAdapter);

                switch (model.Direction)
                {
                    case DirectionEnum.Source:
                        foreach (var prop in model.Properties.First().SourceProperties ?? new List<PropertyModel>())
                        {
                            var cryptoAdapterProperty = new CryptoAdapterProperty
                            {
                                CryptoAdapter = cryptoAdapter,
                                PropertyId = prop.Id,
                                Value = prop.Value ?? String.Empty
                            };
                            _entity.CryptoAdapterProperty.Add(cryptoAdapterProperty);
                        }
                        break;
                    case DirectionEnum.Destination:
                        foreach (var prop in model.Properties.First().DestinationProperties ?? new List<PropertyModel>())
                        {
                            var cryptoAdapterProperty = new CryptoAdapterProperty
                            {
                                CryptoAdapter = cryptoAdapter,
                                PropertyId = prop.Id,
                                Value = prop.Value ?? String.Empty
                            };
                            _entity.CryptoAdapterProperty.Add(cryptoAdapterProperty);
                        }
                        break;
                    default:
                        break;
                }
                _entity.SaveChanges();

                result.Status = StatusEnum.Success;
                result.Message = "Connection test succeeded. Crypto adapter added successfully.";
            }
            catch (Exception ex)
            {
                result.Status = StatusEnum.Error;
                result.Message = Message.SomethingWentWrong;
                _logger.Information($"CryptoAdapterService.CreateCryptoAdapter(model: {model}, userProfileId: {userProfileId})");
                _logger.Error(ex.Message);
            }

            return result;
        }

        public async Task<IResponse<CryptoAdapterModel>> UpdateCryptoAdapter(CryptoAdapterModel model)
        {
            var result = new Response<CryptoAdapterModel>();
            try
            {
                var connection = await TestConnection(model);

                if (connection.Status != StatusEnum.Success)
                {
                    result.Status = StatusEnum.Error;
                    result.Message = connection.Message;
                    return result;
                }
                var cryptoAdapter = _entity.CryptoAdapter.Find(model.Id);

                var jobDefinition = _entity.JobDefinition.Any(jd => jd.From == cryptoAdapter.Adapter.Id || jd.To == cryptoAdapter.Adapter.Id);

                if (jobDefinition)
                {
                    result.Status = StatusEnum.Error;
                    result.Message = "Unable to update crypto adapter if it is used in job definition.";
                    return result;
                }

                var rpcPort = (model.RpcPort).ToString();

                if (rpcPort == "")
                {
                    rpcPort = null;
                }

                cryptoAdapter.Adapter.Name = model.Name;
                cryptoAdapter.Adapter.DirectionId = (int)model.Direction;
                cryptoAdapter.RpcAddr = model.RpcAddr;
                cryptoAdapter.Adapter.AdapterTypeItemId = (int)model.NodeType;
                cryptoAdapter.RpcPort = rpcPort;

                switch (model.Direction)
                {
                    case DirectionEnum.Source:
                        foreach (var prop in model.Properties.First().SourceProperties ?? new List<PropertyModel>())
                        {
                            if (String.IsNullOrEmpty(prop.Value) && prop.PropertyType == PropertyTypeEnum.Password) continue;

                            var cryptoAdapterProperty = _entity.CryptoAdapterProperty.Where(cap => cap.CryptoAdapterId == cryptoAdapter.Id && cap.PropertyId == prop.Id).SingleOrDefault();

                            cryptoAdapterProperty.Value = prop.Value ?? String.Empty;
                        }
                        break;
                    case DirectionEnum.Destination:
                        foreach (var prop in model.Properties.First().DestinationProperties ?? new List<PropertyModel>())
                        {
                            if (String.IsNullOrEmpty(prop.Value) && prop.PropertyType == PropertyTypeEnum.Password) continue;

                            var cryptoAdapterProperty = _entity.CryptoAdapterProperty.Where(cap => cap.CryptoAdapterId == cryptoAdapter.Id && cap.PropertyId == prop.Id).SingleOrDefault();

                            cryptoAdapterProperty.Value = prop.Value ?? String.Empty;
                        }
                        break;
                    default:
                        break;
                }

                _entity.SaveChanges();
                result.Status = StatusEnum.Success;
                result.Message = Message.ChangesSaved;

            }
            catch (Exception ex)
            {
                result.Status = StatusEnum.Error;
                result.Message = Message.SomethingWentWrong;
                _logger.Information($"CryptoAdapterService.UpdateCryptoAdapter(model: {model})");
                _logger.Error(ex.Message);
            }
            return result;
        }

        public IResponse<NoValue> DeleteCryptoAdapter(long cryptoAdapterId)
        {
            var result = new Response<NoValue>();
            try
            {
                var cryptoAdapter = _entity.CryptoAdapter.Find(cryptoAdapterId);
                var jobDefinition = _entity.JobDefinition.Any(jd => jd.From == cryptoAdapter.Adapter.Id || jd.To == cryptoAdapter.Adapter.Id);

                if (jobDefinition)
                {
                    result.Status = StatusEnum.Error;
                    result.Message = "Unable to delete crypto adapter if it is used in job definition.";
                    return result;
                }

                cryptoAdapter.Adapter.StatusId = (int)Statuses.Deleted;

                _entity.SaveChanges();

                result.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                result.Status = StatusEnum.Error;
                result.Message = Message.SomethingWentWrong;
                _logger.Information($"CryptoAdapterService.DeleteCryptoAdapter(cryptoAdapterId: {cryptoAdapterId})");
                _logger.Error(ex.Message);
            }
            return result;
        }


        #region Helper
        private async Task<IResponse<NoValue>> TestConnection(CryptoAdapterModel cryptoAdapter)
        {
            var response = new Response<NoValue>();

            var nodeType = cryptoAdapter.NodeType;

            switch (nodeType)
            {
                case AdapterTypeItemEnum.Ethereum:
                    var adapter = new EthereumAdapter(_logger);

                    if (cryptoAdapter.Direction == DirectionEnum.Destination)
                    {
                        response = await adapter.TestConnectionDestination(cryptoAdapter);
                    }
                    else
                    {
                        response = await adapter.TestConnectionSource(cryptoAdapter);
                    }

                    break;
                case AdapterTypeItemEnum.Cardano:

                    break;
                case AdapterTypeItemEnum.EOS:

                    break;
                case AdapterTypeItemEnum.NEO:
                    var neoAdapter = new NEOAdapter(_logger);

                    response = await neoAdapter.TestConnectionSource(cryptoAdapter);
                    break;
                case AdapterTypeItemEnum.Bitcoin:
                    var btcAdapter = new BitcoinAdapter(_logger);

                    var btcUsername = cryptoAdapter.Properties.SelectMany(p => p.SourceProperties).FirstOrDefault(sp => sp.Id == (long)PropertyEnum.RpcUsername).Value;
                    var btcPassword = cryptoAdapter.Properties.SelectMany(p => p.SourceProperties).FirstOrDefault(sp => sp.Id == (long)PropertyEnum.RpcPassword).Value;

                    response = btcAdapter.TestConnectionSource(cryptoAdapter, btcUsername, btcPassword); 

                    break;
                case AdapterTypeItemEnum.Litecoin:
                    var ltcAdapter = new LitecoinAdapter(_logger);

                    var ltcUsername = cryptoAdapter.Properties.SelectMany(p => p.SourceProperties).FirstOrDefault(sp => sp.Id == (long)PropertyEnum.RpcUsername).Value;
                    var ltcPassword = cryptoAdapter.Properties.SelectMany(p => p.SourceProperties).FirstOrDefault(sp => sp.Id == (long)PropertyEnum.RpcPassword).Value;

                    response = ltcAdapter.TestConnectionSource(cryptoAdapter, ltcUsername, ltcPassword);

                    break;
                case AdapterTypeItemEnum.Solana:
                    var solAdapter = new SolanaAdapter(_logger);
                    response = await solAdapter.TestConnectionSource(cryptoAdapter);
                   
                    break;
            }

            return response;

        }

        private List<PropertyModel> GetProperties(CryptoAdapterType cryptoAdapterType, DirectionEnum direction)
        {
            var result = new List<PropertyModel>();

            result = _entity.AdapterTypeItemProperty
                .Where(atip => atip.AdapterTypeItemId == (int)cryptoAdapterType && atip.DirectionId == (int)direction)
                .Select(s => new PropertyModel
                {
                    Id = s.Property.Id,
                    Name = s.Property.Name,
                    PropertyType = (PropertyTypeEnum)s.Property.PropertyTypeId
                })
                .ToList();

            return result;
        }
        #endregion
    }
}
