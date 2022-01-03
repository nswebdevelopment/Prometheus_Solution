using Prometheus.BL.Interfaces;
using Prometheus.Common;
using Prometheus.Common.Enums;
using Prometheus.Dal.Entities;
using Prometheus.DbAdapter.Databases;
using Prometheus.DbAdapter.Queries;
using Prometheus.Model.Models;
using Prometheus.Model.Models.BitcoinAdapterModel;
using Prometheus.Model.Models.LitecoinBlockModel;
using Prometheus.Model.Models.NeoAdapterModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Prometheus.BL.Services
{
    public class DbAdapterService : IDbAdapterService
    {
        private readonly ILogger _logger;
        private readonly IJobDefinitionService _jobDefinitionService;
        private readonly IEnterpriseAdapterService _enterpriseAdapterService;
        private readonly IPrometheusEntities _entity;

        public DbAdapterService(ILogger logger, IJobDefinitionService jobDefinitionService, IEnterpriseAdapterService enterpriseAdapterService, IPrometheusEntities entity)
        {
            _logger = logger;
            _jobDefinitionService = jobDefinitionService;
            _enterpriseAdapterService = enterpriseAdapterService;
            _entity = entity;
        }

        public IResponse<List<GenericModel>> GetAdapterData(long jobDefinitionId)
        {
            var response = new Response<List<GenericModel>>
            {
                Value = new List<GenericModel>()
            };

            try
            {
                var adapterParameters = _jobDefinitionService.GetAdapterParameter(jobDefinitionId);
                if (adapterParameters.Status != StatusEnum.Success)
                {
                    response.Status = StatusEnum.Error;
                    return response;
                }

                IAdapter adapter;
                switch (adapterParameters.Value.Adapter)
                {
                    case AdapterTypeItemEnum.MSSQL:
                        adapter = new MSSQLAdapter(_logger);
                        var connString = "Data Source=NSWD-LT054\\SQLEXPRESS;Initial Catalog=Retail_Bank_Dev;Integrated Security=True";
                        response = adapter.ConnectAndRead(adapterParameters.Value.QueryRead, connString);

                        break;
                    case AdapterTypeItemEnum.MySQL:
                        adapter = new MySQLAdapter(_logger);
                        response = adapter.ConnectAndRead(adapterParameters.Value.QueryRead, adapterParameters.Value.ConnString.MySQLConnString);

                        break;
                    case AdapterTypeItemEnum.Oracle:
                        adapter = new OracleAdapter(_logger);
                        response = adapter.ConnectAndRead(adapterParameters.Value.QueryRead, adapterParameters.Value.ConnString.OracleConnString);

                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Information($"DbAdapterService.GetAdapterData(jobId: {jobDefinitionId})");
                _logger.Error(ex.Message);
                response.Status = StatusEnum.Error;
            }

            return response;
        }

        public IResponse<NoValue> SendToRelationalDb<T>(long jobId, List<T> blocks)
        {
            var response = new Response<NoValue>();

            try
            {
                if (blocks.Count == 0)
                {
                    response.Status = StatusEnum.Success;
                    return response;
                }

                var enterpriseAdapter = _entity.JobTimeline.Find(jobId).Schedule.JobDefinition.Adapter1.EnterpriseAdapter.FirstOrDefault();

                var query = new QueryWrite
                {
                    TableNamePrefix = enterpriseAdapter.EnterpriseAdapterProperty.FirstOrDefault(eap => eap.PropertyId == (int)PropertyEnum.TableNamePrefix).Value
                };
                
                if (typeof(T) == typeof(EthereumBlockModel))
                {
                    var model = (List<EthereumBlockModel>)Convert.ChangeType(blocks, typeof(List<EthereumBlockModel>));
                    query.CryptoAdapterType = CryptoAdapterType.Ethereum;

                    for (var i = 0; i < model.Count; i++)
                    {
                        model[i].BlockIdSQL = Guid.NewGuid();

                        for (var j = 0; j < model[i].BlockTransactions.Count; j++)
                        {
                            model[i].BlockTransactions[j].ParentBlockId = model[i].BlockIdSQL;
                            model[i].BlockTransactions[j].TransactionIdSQL = Guid.NewGuid();
                        }
                    }
                    query.EthereumBlockModel = model;
                }
                else
                {
                    foreach (T block in blocks)
                    {
                        var blockIdSQL = typeof(T).GetProperty("BlockIdSQL");
                        blockIdSQL.SetValue(block, Guid.NewGuid());

                        var transactionList = (IEnumerable)typeof(T).GetProperty("TransactionList").GetValue(block);

                        foreach (var transaction in transactionList)
                        {
                            var transactionType = transaction.GetType();
                            var transactionIdSQL = transactionType.GetProperty("TransactionIdSQL");
                            transactionIdSQL.SetValue(transaction, Guid.NewGuid());

                            var parentBlockIdSQL = transactionType.GetProperty("ParentBlockIdSQL");
                            parentBlockIdSQL.SetValue(transaction, blockIdSQL.GetValue(block));

                            var transactionInputList = (IEnumerable)transactionType.GetProperty("TransactionInputs").GetValue(transaction);

                            foreach (var input in transactionInputList)
                            {
                                var transactionInIdSQL = input.GetType().GetProperty("TransactionInIdSQL");
                                transactionInIdSQL.SetValue(input, Guid.NewGuid());

                                var parentTransactionIdSQL = input.GetType().GetProperty("ParentTransactionIdSQL");
                                parentTransactionIdSQL.SetValue(input, transactionIdSQL.GetValue(transaction));
                            }

                            var transactionOutputList = (IEnumerable)transactionType.GetProperty("TransactionOutputs").GetValue(transaction);

                            foreach (var output in transactionOutputList)
                            {
                                var transactionOutIdSQL = output.GetType().GetProperty("TransactionOutIdSQL");
                                transactionOutIdSQL.SetValue(output, Guid.NewGuid());

                                var parentTransactionIdSQL = output.GetType().GetProperty("ParentTransactionIdSQL");
                                parentTransactionIdSQL.SetValue(output, transactionIdSQL.GetValue(transaction));
                            }
                        }
                    }

                    if(typeof(T) == typeof(BitcoinBlockModel))
                    {
                        var model = (List<BitcoinBlockModel>)Convert.ChangeType(blocks, typeof(List<BitcoinBlockModel>));
                        query.CryptoAdapterType = CryptoAdapterType.Bitcoin;
                        query.BitcoinBlockModel = model;
                    }
                    else if (typeof(T) == typeof(NeoBlockModel))
                    {
                        var model = (List<NeoBlockModel>)Convert.ChangeType(blocks, typeof(List<NeoBlockModel>));
                        query.CryptoAdapterType = CryptoAdapterType.NEO;
                        query.NEOBlockModel = model;
                    }
                    else if (typeof(T) == typeof(LitecoinBlockModel))
                    {
                        var model = (List<LitecoinBlockModel>)Convert.ChangeType(blocks, typeof(List<LitecoinBlockModel>));
                        query.CryptoAdapterType = CryptoAdapterType.Litecoin;
                        query.LitecoinBlockModel = model;
                    }
                }
                
                var config = new Config
                {
                    ConnString = new DbAdapter.ConnStringCreator
                    {
                        Server = enterpriseAdapter.ServerIP,
                        Port = enterpriseAdapter.Port.ToString(),
                        Uid = enterpriseAdapter.Username,
                        Pwd = enterpriseAdapter.Password,
                        Database = enterpriseAdapter.DatabaseName
                    },
                    Adapter = (AdapterTypeItemEnum)enterpriseAdapter.Adapter.AdapterTypeItemId,
                    QueryWrite = query
                };
                
                IAdapter adapter;
                switch (config.Adapter)
                {
                    case AdapterTypeItemEnum.MSSQL:
                        adapter = new MSSQLAdapter(_logger);
                        response = adapter.ConnectAndWrite(config.QueryWrite, config.ConnString.MSSQLConnString);
                        break;
                    case AdapterTypeItemEnum.MySQL:
                        adapter = new MySQLAdapter(_logger);
                        response = adapter.ConnectAndWrite(config.QueryWrite, config.ConnString.MySQLConnString);
                        break;
                    case AdapterTypeItemEnum.Oracle:
                        adapter = new OracleAdapter(_logger);
                        response = adapter.ConnectAndWrite(config.QueryWrite, config.ConnString.OracleConnString);
                        break;
                }
            }
            catch (Exception ex)
            {
                response.Status = StatusEnum.Error;
                response.Message = ex.Message;
                _logger.Information($"DbAdapterService.SendToRelationalDb(jobid: {jobId}, block:{blocks})");
                _logger.Error(ex.Message);
            }

            return response;
        }
        
        public IResponse<NoValue> SendToMongoDb<T>(long enterpriseAdapterId, List<T> blocks)
        {
            var response = new Response<NoValue>();

            try
            {
                var enterpriseAdapter = _enterpriseAdapterService.GetEnterpriseAdapter(enterpriseAdapterId);

                if (enterpriseAdapter.Status != StatusEnum.Success)
                {
                    response.Status = StatusEnum.Error;
                    return response;
                }

                var config = new Config
                {
                    ConnString = new DbAdapter.ConnStringCreator
                    {
                        Server = enterpriseAdapter.Value.ServerIP,
                        Port = enterpriseAdapter.Value.Port.ToString(),
                        Uid = enterpriseAdapter.Value.Username,
                        Pwd = enterpriseAdapter.Value.Password,
                        Database = enterpriseAdapter.Value.DatabaseName
                    }
                };

                var adapter = new MongoDbAdapter(_logger);

                if (typeof(T) == typeof(EthereumBlockModel))
                {
                    response = adapter.SendToMongoDb(blocks, enterpriseAdapter.Value, config.ConnString.MongoDbConnString, CryptoAdapterType.Ethereum);
                }
                else if (typeof(T) == typeof(BitcoinBlockModel))
                {
                    response = adapter.SendToMongoDb(blocks, enterpriseAdapter.Value, config.ConnString.MongoDbConnString, CryptoAdapterType.Bitcoin);
                }
                else if (typeof(T) == typeof(NeoBlockModel))
                {
                    response = adapter.SendToMongoDb(blocks, enterpriseAdapter.Value, config.ConnString.MongoDbConnString, CryptoAdapterType.NEO);
                }
                else if (typeof(T) == typeof(LitecoinBlockModel))
                {
                    response = adapter.SendToMongoDb(blocks, enterpriseAdapter.Value, config.ConnString.MongoDbConnString, CryptoAdapterType.Litecoin);
                }

            }
            catch (Exception ex)
            {
                response.Status = StatusEnum.Error;
                response.Message = ex.Message;
                _logger.Information($"DbAdapterService.SendToMongoDb(jobid: {enterpriseAdapterId}, block:{blocks})");
                _logger.Error(ex.Message);
            }

            return response;

        }

    }
}
