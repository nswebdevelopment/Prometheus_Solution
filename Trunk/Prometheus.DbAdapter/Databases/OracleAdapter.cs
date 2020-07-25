using Oracle.ManagedDataAccess.Client;
using Prometheus.DbAdapter.Queries;
using Prometheus.Model.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Prometheus.Common;
using Prometheus.Common.Enums;
using Serilog;

namespace Prometheus.DbAdapter.Databases
{
    public class OracleAdapter : IAdapter
    {
        private readonly ILogger _logger;

        public OracleAdapter(ILogger logger)
        {
            _logger = logger;
        }

        public Response<List<GenericModel>> ConnectAndRead(QueryRead query, string connString)
        {
            var response = new Response<List<GenericModel>>
            {
                Value = new List<GenericModel>()
            };

            using (var conn = new OracleConnection(connString))
            {
                try
                {
                    conn.Open();

                    var adapter = new OracleDataAdapter(query.SelectQuery, conn);
                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    dataTable.ToModel(response);
                }
                catch (Exception ex)
                {
                    response.Message = ex.Message;
                    _logger.Information($"OracleAdapter.ConnectAndRead(query: {query}, connString: {connString})");
                    _logger.Error(ex.Message);
                }
                finally
                {
                    conn.Close();
                }

            }

            return response;
        }


        public Response<NoValue> ConnectAndWrite(QueryWrite query, string connString)
        {
            var response = new Response<NoValue>();

            using (var conn = new OracleConnection(connString))
            {
                try
                {
                    conn.Open();

                    var selectTableCommand = new OracleCommand($"SELECT COUNT (*) FROM ALL_OBJECTS WHERE OBJECT_TYPE ='TABLE' AND OBJECT_NAME='{query.TableNamePrefix.ToUpper()}_{query.CryptoAdapterType.ToString().ToUpper()}_BLOCK'", conn);
                    var tableExists = selectTableCommand.ExecuteScalar();

                    if (Convert.ToInt32(tableExists) == 0)
                    {
                        var createCommand = new OracleCommand(query.CreateTableQueryOracle, conn);
                        createCommand.ExecuteNonQuery();
                    }

                    int insertQueryNo = 0;
                    switch (query.CryptoAdapterType)
                    {
                        case CryptoAdapterType.Ethereum:
                            insertQueryNo = 2;
                            break;
                        case CryptoAdapterType.Bitcoin:
                        case CryptoAdapterType.NEO:
                        case CryptoAdapterType.Litecoin:
                            insertQueryNo = 4;
                            break;
                    }

                    var pageNumber = 0;
                    var continuePaging = true;
                    var queryString = string.Empty;
                    
                    for (var i = 1; i <= insertQueryNo; i++)
                    {
                        pageNumber = 0;
                        do
                        {
                            var resultTuple = query.InsertDataToOracleDb(i, pageNumber);
                            queryString = resultTuple.Item1;
                            continuePaging = resultTuple.Item2;

                            if (queryString != String.Empty)
                            {
                                var insertCommand = new OracleCommand(queryString, conn);
                                insertCommand.ExecuteNonQuery();
                            }
                            pageNumber++;
                        }
                        while (continuePaging == true);
                    }
                    
                    response.Status = StatusEnum.Success;
                }
                catch (Exception ex)
                {
                    response.Status = StatusEnum.Error;
                    response.Message = ex.Message;
                    _logger.Information($"OracleAdapter.ConnectAndWrite(query: {query}, connString: {connString})");
                    _logger.Error(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }

            return response;
        }
        public Response<NoValue> TestConnectivity(string connString, string parentTableName = default(String))
        {
            var response = new Response<NoValue>();
            using (var conn = new OracleConnection(connString))
            {
                try
                {
                    conn.Open();

                    if(parentTableName != default(String))
                    {
                        var adapter = new OracleDataAdapter($"SELECT * FROM \"{parentTableName}\" WHERE ROWNUM <= 1", conn);
                        var data = new DataTable();
                        adapter.Fill(data);

                        if (data.Rows.Count == 0)
                        {
                            response.Status = StatusEnum.Error;
                            response.Message = "Connection test failed. We could not find any data in the table.";
                        }
                    }
                    
                    response.Status = StatusEnum.Success;
                }
                catch (Exception ex)
                {
                    response.Status = StatusEnum.Error;
                    response.Message = "Connection test failed. " + ex.Message;
                    _logger.Information($"OracleAdapter.TestConnectivity(connString: {connString}, parentTableName: {parentTableName})");
                    _logger.Error(ex.Message);
                }
                finally
                {
                    conn.Close();
                }

            }
            return response;
        }
    }
}
