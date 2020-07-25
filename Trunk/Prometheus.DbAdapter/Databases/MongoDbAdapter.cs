using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Core.Operations;
using Prometheus.Common;
using Prometheus.Common.Enums;
using Prometheus.Model.Models;
using Prometheus.Model.Models.BitcoinAdapterModel;
using Prometheus.Model.Models.EnterpriseAdapterModel;
using Prometheus.Model.Models.NeoAdapterModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Prometheus.DbAdapter.Databases
{
    public class MongoDbAdapter
    {
        private readonly ILogger _logger;

        public MongoDbAdapter(ILogger logger)
        {
            _logger = logger;
        }
        
        public Response<NoValue> SendToMongoDb<T>(List<T> blocks, EnterpriseAdapterModel enterpriseAdapter, string connectionString, CryptoAdapterType cryptoAdapter)
        {
            var response = new Response<NoValue>();

            try
            {
                var stopwatch = Stopwatch.StartNew();

                var mongoClient = new MongoClient(connectionString);

                var db = mongoClient.GetDatabase(enterpriseAdapter.DatabaseName);
                var collectionName = enterpriseAdapter.Properties.FirstOrDefault().DestinationProperties.FirstOrDefault(sp => sp.Id == (long)PropertyEnum.CollectionName).Value;

                var collection = db.GetCollection<BsonDocument>($"{cryptoAdapter.ToString()}{collectionName}");

                foreach (var block in blocks)
                {
                    collection.InsertOne(block.ToBsonDocument());
                }

                stopwatch.Stop();
                _logger.Information($"MongoDbAdapter.SendToMongoDb(blocks: {blocks.Count}, enterpriseAdapter: {enterpriseAdapter.Id}). Time elapsed: {stopwatch.Elapsed.TotalSeconds} seconds.");

                response.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                response.Status = StatusEnum.Error;
                response.Message = ex.Message;
                _logger.Information($"MongoDbAdapter.SendToMongoDb(blocks: {blocks.Count}, enterpriseAdapter: {enterpriseAdapter.Id})");
                _logger.Error(ex.Message);
            }

            return response;
        }

        public Response<NoValue> TestConnection(string connectionString, string databaseName)
        {
            var response = new Response<NoValue>();

            try
            {
                var mongoClient = new MongoClient(connectionString);

                var db = mongoClient.GetDatabase(databaseName);

                bool isMongoLive = db.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait(1000);

                if (isMongoLive)
                {
                    //refactor:
                    //check if login credentials are OK!
                    //...

                    response.Status = StatusEnum.Success;
                }
                else
                {
                    response.Status = StatusEnum.Error;
                    response.Message = "Test connection failed. Check input parameters.";
                }

            }
            catch (Exception ex)
            {
                response.Status = StatusEnum.Error;
                response.Message = "Test connection failed. Check input parameters.";
                _logger.Information($"MongoDbAdapter.TestConnection(connectionString: {connectionString}, databaseName: {databaseName})");
                _logger.Error(ex.Message);

            }

            return response;
        }

    }

}
