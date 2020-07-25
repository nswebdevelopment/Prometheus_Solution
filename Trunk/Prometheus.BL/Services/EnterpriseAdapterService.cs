using Microsoft.Extensions.Configuration;
using Prometheus.BL.Interfaces;
using Prometheus.Common;
using Prometheus.Common.Enums;
using Prometheus.Dal;
using Prometheus.Dal.Entities;
using Prometheus.DbAdapter;
using Prometheus.DbAdapter.Databases;
using Prometheus.DbAdapter.Queries;
using Prometheus.Model.Models;
using Prometheus.Model.Models.EnterpriseAdapterModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Prometheus.BL.Services
{

    public class EnterpriseAdapterService : IEnterpriseAdapterService
    {
        private readonly IPrometheusEntities _entity;
        private readonly Serilog.ILogger _logger;

        public EnterpriseAdapterService(IPrometheusEntities entity, Serilog.ILogger logger)
        {
            _entity = entity;
            _logger = logger;
        }

        public IResponse<List<EnterpriseAdapterModel>> GetEnterpriseAdapters(long userProfileId)
        {
            var result = new Response<List<EnterpriseAdapterModel>>();

            try
            {
                result.Value = _entity.EnterpriseAdapter.Where(a => a.Adapter.UserProfileId == userProfileId && a.Adapter.StatusId == (int)Statuses.Active)
                    .Select(a => new EnterpriseAdapterModel
                    {
                        Id = a.Id,
                        Name = a.Adapter.Name,
                        EnterpriseAdapter = (AdapterTypeItemEnum)a.Adapter.AdapterTypeItemId,
                        Direction = (DirectionEnum)a.Adapter.DirectionId
                    }).ToList();

                result.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                result.Status = StatusEnum.Error;
                result.Message = Message.SomethingWentWrong;
                _logger.Information($"EnterpriseAdapterService.GetEnterpriseAdapters(userProfileId: {userProfileId})");
                _logger.Error(ex.Message);
            }

            return result;
        }

        public IResponse<EnterpriseAdapterModel> GetEnterpriseAdapter(long enterpriseAdapterId)
        {
            var response = new Response<EnterpriseAdapterModel>();
            try
            {

                var enterpriseAdapter = _entity.EnterpriseAdapter.Find(enterpriseAdapterId);

                var enterpriseAdapterModel = new EnterpriseAdapterModel
                {
                    Id = enterpriseAdapter.Id,
                    Name = enterpriseAdapter.Adapter.Name,
                    ServerIP = enterpriseAdapter.ServerIP,
                    Port = enterpriseAdapter.Port,
                    Username = enterpriseAdapter.Username,
                    Password = enterpriseAdapter.Password,
                    DatabaseName = enterpriseAdapter.DatabaseName,
                    EnterpriseAdapter = (AdapterTypeItemEnum)enterpriseAdapter.Adapter.AdapterTypeItemId,
                    Properties = new List<EnterpriseModel>(),
                    Direction = (DirectionEnum)enterpriseAdapter.Adapter.DirectionId
                };
                enterpriseAdapterModel.Properties.Add(new EnterpriseModel()
                {
                    EnterpriseType = (EnterpriseAdapterType)enterpriseAdapter.Adapter.AdapterTypeItem.AdapterType.Id,
                    SourceProperties = new List<PropertyModel>(),
                    DestinationProperties = new List<PropertyModel>()
                });

                var properties = _entity.EnterpriseAdapterProperty
                    .Where(eap => eap.EnterpriseAdapterId == enterpriseAdapter.Id)
                    .Select(s => new PropertyModel
                    {
                        Id = s.Property.Id,
                        Name = s.Property.Name,
                        PropertyType = (PropertyTypeEnum)s.Property.PropertyTypeId,
                        Value = s.Value
                    })
                    .ToList();
                
                switch (enterpriseAdapterModel.Direction)
                {
                    case DirectionEnum.Source:
                        enterpriseAdapterModel.Properties.First().SourceProperties = properties;
                        if ((int)enterpriseAdapterModel.EnterpriseAdapter < 4)
                        {
                            var adapterTable = enterpriseAdapter.EnterpriseAdapterTable.FirstOrDefault();
                            var adapterColumn = adapterTable.EnterpriseAdapterTableColumn.ToList();
                            var ParentColumns = adapterColumn.Where(x => x.ParentId == null);
                            var list = new List<EnterpriseAdapterTableColumnModel>();
                            foreach (var item in ParentColumns)
                            {
                                GetRecursive(item, list);
                            }
                            enterpriseAdapterModel.ParentTable = adapterTable.TableName;
                            enterpriseAdapterModel.Columns = list;
                        }
                        break;
                    case DirectionEnum.Destination:
                        enterpriseAdapterModel.Properties.First().DestinationProperties = properties;
                        break;
                    default:
                        break;
                }

                response.Status = StatusEnum.Success;
                response.Value = enterpriseAdapterModel;
            }

            catch (Exception ex)
            {
                response.Status = StatusEnum.Error;
                _logger.Information($"EnterpriseAdapterService.GetEnterpriseAdapter(enterpriseAdapterId: {enterpriseAdapterId})");
                _logger.Error(ex.Message);
            }
            return response;
        }

        public IResponse<NoValue> Create(EnterpriseAdapterModel model, long userProfileID)
        {
            var result = new Response<NoValue>();
            try
            {
                #region connectivity
                var connection = TestConnection(model);
                if (connection.Status != StatusEnum.Success)
                {
                    result.Status = StatusEnum.Error;
                    result.Message = connection.Message;
                    return result;
                }
                #endregion

                var enterpriseAdapter = new EnterpriseAdapter();
                var newAdapter = new Adapter
                {
                    Name = model.Name,
                    AdapterTypeItemId = (int)model.EnterpriseAdapter,
                    UserProfileId = userProfileID,
                    StatusId = (int)Statuses.Active,
                    DirectionId = (int)model.Direction
                };
                _entity.Adapter.Add(newAdapter);

                enterpriseAdapter.ServerIP = model.ServerIP;
                enterpriseAdapter.Port = (int)model.Port;
                enterpriseAdapter.Username = model.Username;
                enterpriseAdapter.Password = model.Password;
                enterpriseAdapter.DatabaseName = model.DatabaseName;
                enterpriseAdapter.AdapterId = newAdapter.Id;
                _entity.EnterpriseAdapter.Add(enterpriseAdapter);

                switch (model.Direction)
                {
                    case DirectionEnum.Destination:
                        foreach (var prop in model.Properties.First().DestinationProperties ?? new List<PropertyModel>())
                        {
                            var enterpriseAdapterProperty = new EnterpriseAdapterProperty
                            {
                                EnterpriseAdapter = enterpriseAdapter,
                                PropertyId = prop.Id,
                                Value = prop.Value ?? String.Empty
                            };
                            _entity.EnterpriseAdapterProperty.Add(enterpriseAdapterProperty);
                        }
                        break;


                    case DirectionEnum.Source:
                        if ((int)model.EnterpriseAdapter < 4)
                        {
                            var adapterTable = new EnterpriseAdapterTable
                            {
                                TableName = model.ParentTable,
                                EnterpriseAdapterId = enterpriseAdapter.Id
                            };
                            _entity.EnterpriseAdapterTable.Add(adapterTable);

                            var y = _entity.EnterpriseAdapterTableColumn.Select(x => x.Id).ToList().LastOrDefault();
                            if (model.Columns != null)
                            {
                                for (int i = 0; i < model.Columns.Count; i++)
                                {

                                    var child = new EnterpriseAdapterTableColumn
                                    {
                                        Id = (int)model.Columns[i].Id,
                                        ParentId = model.Columns[i].ParentId,
                                        IsForeignKey = model.Columns[i].IsForeignKey,
                                        IsPrimaryKey = model.Columns[i].IsPrimaryKey,
                                        PropertyNameId = (int?)model.Columns[i].PropertyNameId,
                                        RelatedTableName = model.Columns[i].RelatedTableName,
                                        DataTypeId = 1,
                                        ColumnName = model.Columns[i].ColumnName,
                                        EnterpriseAdapterTableId = adapterTable.Id
                                    };
                                    if (model.Columns[i].PropertyNameId == 0)
                                        child.PropertyNameId = null;

                                    if (model.Columns[i].ParentId == 0)
                                        child.ParentId = null;
                                    else
                                        child.ParentId = model.Columns[i].ParentId;
                                    _entity.EnterpriseAdapterTableColumn.Add(child);

                                }
                            }
                        }
                        foreach (var prop in model.Properties.First().SourceProperties ?? new List<PropertyModel>())
                        {
                            var enterpriseAdapterProperty = new EnterpriseAdapterProperty
                            {
                                EnterpriseAdapter = enterpriseAdapter,
                                PropertyId = prop.Id,
                                Value = prop.Value ?? String.Empty
                            };
                            _entity.EnterpriseAdapterProperty.Add(enterpriseAdapterProperty);
                        }
                        break;

                    default:
                        break;

                }

                _entity.SaveChanges();
                result.Status = StatusEnum.Success;
                result.Message = "Connection test succeeded. Enterprise adapter added successfully.";
            }

            catch (Exception ex)
            {
                result.Status = StatusEnum.Error;
                result.Message = Message.SomethingWentWrong;
                _logger.Information($"EnterpriseAdapterService.Create(model: {model}, userProfileID: {userProfileID})");
                _logger.Error(ex.Message);
            }
            return result;
        }

        public IResponse<NoValue> CreateXML(string fileAsString, long userProfileId)
        {
            //refacor DataType after delete from DB

            var result = new Response<NoValue>();

            var xml = XDocument.Parse(fileAsString);
            var files = xml.Root.Elements().ToList();

            try
            {
                var enumConvertSuccess = Enum.TryParse(files[0].Value, true, out AdapterTypeItemEnum adapterType);

                if (!enumConvertSuccess)
                {
                    result.Status = StatusEnum.Error;
                    result.Message = "Adapter type can be: MSSQL, MySQL or Oracle.";
                    return result;
                }


                #region connectivity
                var enterpriseAdapterModel = new EnterpriseAdapterModel
                {
                    EnterpriseAdapter = adapterType,
                    ServerIP = files[1].Value,
                    Port = Int32.Parse(files[2].Value),
                    Username = files[4].Value,
                    Password = files[5].Value,
                    DatabaseName = files[3].Value,
                    Direction = (DirectionEnum)Enum.Parse(typeof(DirectionEnum), files[6].Value, true)
                };

                var tableElements = new List<XElement>();

                if (enterpriseAdapterModel.Direction == DirectionEnum.Source)
                {
                    tableElements = files[7].Elements().ToList();
                    enterpriseAdapterModel.ParentTable = tableElements[0].Value;
                }

                var connection = TestConnection(enterpriseAdapterModel);
                if (connection.Status != StatusEnum.Success)
                {
                    result.Status = StatusEnum.Error;
                    result.Message = connection.Message;
                    return result;
                }
                #endregion

                var newAdapter = new Adapter
                {
                    Name = files[0].Value + " Adapter",
                    UserProfileId = userProfileId,
                    StatusId = (int)Statuses.Active,
                    AdapterTypeItemId = (int)adapterType,
                    DirectionId = (int)enterpriseAdapterModel.Direction
                };
                _entity.Adapter.Add(newAdapter);

                EnterpriseAdapter enterpriseAdapter = new EnterpriseAdapter
                {

                    ServerIP = enterpriseAdapterModel.ServerIP,
                    Port = enterpriseAdapterModel.Port.GetValueOrDefault(),
                    Username = enterpriseAdapterModel.Username,
                    Password = enterpriseAdapterModel.Password,
                    DatabaseName = enterpriseAdapterModel.DatabaseName,
                    AdapterId = newAdapter.Id
                };

                _entity.EnterpriseAdapter.Add(enterpriseAdapter);


                if (enterpriseAdapterModel.Direction == DirectionEnum.Source)
                {
                    EnterpriseAdapterTable adapterTable = new EnterpriseAdapterTable
                    {
                        TableName = tableElements[0].Value,
                        EnterpriseAdapterId = enterpriseAdapter.Id
                    };
                    _entity.EnterpriseAdapterTable.Add(adapterTable);


                    var columns = tableElements[1].Elements().ToList();

                    foreach (var column in columns)
                    {
                        var adapterPropertyId = adapterTable.Id;
                        var isForeignKey = Convert.ToBoolean(column.Attribute("FK").Value);
                        var isPrimaryKey = Convert.ToBoolean(column.Attribute("PK").Value);

                        Common.Enums.PropertyName property = new Common.Enums.PropertyName();
                        var propertyName = column.Attribute("property").Value;


                        if (!isForeignKey)
                        {
                            var foreignKeyElementsExist = column.HasElements;

                            if (foreignKeyElementsExist)
                            {
                                result.Status = StatusEnum.Error;
                                result.Message = "There was an error in defining the foreign key field. Check the XML Example file.";
                                return result;
                            }


                            EnterpriseAdapterTableColumn adapterTableColumn = new EnterpriseAdapterTableColumn
                            {
                                EnterpriseAdapterTableId = adapterPropertyId,
                                ColumnName = column.Value,
                                IsForeignKey = isForeignKey,
                                IsPrimaryKey = isPrimaryKey,
                                DataTypeId = 1
                            };

                            switch (propertyName.ToLower())
                            {
                                case "transactionid":
                                case "transactionaccount":
                                case "transactionamount":
                                case "transactiontype":
                                    property = (Common.Enums.PropertyName)Enum.Parse(typeof(Common.Enums.PropertyName), propertyName, true);
                                    adapterTableColumn.PropertyNameId = (int)property;
                                    break;
                            }

                            _entity.EnterpriseAdapterTableColumn.Add(adapterTableColumn);
                        }
                        else
                        {
                            var columnsChild = column.Elements().ToList();

                            if (columnsChild.Count < 4)
                            {
                                result.Status = StatusEnum.Error;
                                result.Message = "You have to provide aditional data if the foreign key attribute is set to \"true\". Check the XML Example file.";
                                return result;
                            }

                            EnterpriseAdapterTableColumn adapterTableColumnParent = new EnterpriseAdapterTableColumn
                            {
                                EnterpriseAdapterTableId = adapterPropertyId,
                                ColumnName = columnsChild[0].Value,
                                IsForeignKey = isForeignKey,
                                IsPrimaryKey = isPrimaryKey,
                                DataTypeId = 1,
                                RelatedTableName = columnsChild[1].Value
                            };


                            for (var i = 2; i < columnsChild.Count; i++)
                            {
                                EnterpriseAdapterTableColumn adapterTableColumnChild = new EnterpriseAdapterTableColumn
                                {
                                    EnterpriseAdapterTableId = adapterTable.Id,
                                    ColumnName = columnsChild[i].Value,
                                    IsForeignKey = false,
                                    IsPrimaryKey = false,
                                    DataTypeId = 1,
                                    ParentId = adapterTableColumnParent.Id,
                                    EnterpriseAdapterTableColumn2 = adapterTableColumnParent
                                };


                                Common.Enums.PropertyName propertyChild = new Common.Enums.PropertyName();
                                var propertyNameFK = columnsChild[i].Attribute("property").Value;

                                switch (propertyNameFK.ToLower())
                                {
                                    case "transactionaccount":
                                    case "transactionamount":
                                    case "transactiontype":
                                        propertyChild = (Common.Enums.PropertyName)Enum.Parse(typeof(Common.Enums.PropertyName), propertyNameFK, true);
                                        adapterTableColumnChild.PropertyNameId = (int)propertyChild;
                                        break;
                                }


                                if (columnsChild[i].Name == "primaryKey")
                                {
                                    adapterTableColumnChild.IsPrimaryKey = true;
                                }

                                adapterTableColumnParent.EnterpriseAdapterTableColumn1.Add(adapterTableColumnChild);
                            }
                            _entity.EnterpriseAdapterTableColumn.Add(adapterTableColumnParent);
                        }
                    }
                }
                else if (enterpriseAdapterModel.Direction == DirectionEnum.Destination)
                {
                    _entity.EnterpriseAdapterProperty.Add(new EnterpriseAdapterProperty
                    {
                        PropertyId = (long)PropertyEnum.TableNamePrefix,
                        EnterpriseAdapterId = newAdapter.Id,
                        Value = files[7].Value
                    });
                }

                _entity.SaveChanges();

                result.Status = StatusEnum.Success;
                result.Message = "Connection test succeeded. Enterprise adapter added successfully.";
            }
            catch (Exception ex)
            {
                result.Status = StatusEnum.Error;
                result.Message = ex.Message;
                _logger.Information($"EnterpriseAdapterService.CreateXML(fileAsString: {fileAsString}, userProfileId: {userProfileId})");
                _logger.Error(ex.Message);
            }

            return result;
        }

        private IResponse<NoValue> TestConnection(EnterpriseAdapterModel enterpriseAdapter)
        {
            var connectionResponse = new Response<NoValue>();

            var config = new Config
            {
                ConnString = new ConnStringCreator
                {
                    Server = enterpriseAdapter.ServerIP,
                    Port = enterpriseAdapter.Port.ToString(),
                    Database = enterpriseAdapter.DatabaseName,
                    Pwd = enterpriseAdapter.Password,
                    Uid = enterpriseAdapter.Username
                }
            };

            IAdapter adapter;
            IResponse<NoValue> connectionStatus = null;
            switch (enterpriseAdapter.EnterpriseAdapter)
            {
                case AdapterTypeItemEnum.MSSQL:
                    adapter = new MSSQLAdapter(_logger);

                    if (enterpriseAdapter.Direction == DirectionEnum.Source)
                    {
                        connectionStatus = adapter.TestConnectivity(config.ConnString.MSSQLConnString, enterpriseAdapter.ParentTable);
                    }
                    else
                    {
                        connectionStatus = adapter.TestConnectivity(config.ConnString.MSSQLConnString);
                    }
                    break;

                case AdapterTypeItemEnum.MySQL:
                    adapter = new MySQLAdapter(_logger);

                    if (enterpriseAdapter.Direction == DirectionEnum.Source)
                    {
                        connectionStatus = adapter.TestConnectivity(config.ConnString.MySQLConnString, enterpriseAdapter.ParentTable);
                    }
                    else
                    {
                        connectionStatus = adapter.TestConnectivity(config.ConnString.MySQLConnString);
                    }
                    break;

                case AdapterTypeItemEnum.Oracle:
                    adapter = new OracleAdapter(_logger);

                    if (enterpriseAdapter.Direction == DirectionEnum.Source)
                    {
                        connectionStatus = adapter.TestConnectivity(config.ConnString.OracleConnString, enterpriseAdapter.ParentTable);
                    }
                    else
                    {
                        connectionStatus = adapter.TestConnectivity(config.ConnString.OracleConnString);
                    }
                    break;

                case AdapterTypeItemEnum.MongoDB:
                    var mongoAdapter = new MongoDbAdapter(_logger);
                    connectionStatus = mongoAdapter.TestConnection(config.ConnString.MongoDbConnString, enterpriseAdapter.DatabaseName);
                    break;
            }

            if (connectionStatus.Status != StatusEnum.Success)
            {
                connectionResponse.Status = StatusEnum.Error;
                connectionResponse.Message = connectionStatus.Message;
            }
            else
            {
                connectionResponse.Status = StatusEnum.Success;
            }
            return connectionResponse;
        }

        public IResponse<EnterpriseAdapterModel> UpdateEnterpriseAdapter(EnterpriseAdapterModel model)
        {
            var result = new Response<EnterpriseAdapterModel>();
            try
            {
                #region connectivity
                var connection = TestConnection(model);
                if (connection.Status != StatusEnum.Success)
                {
                    result.Status = StatusEnum.Error;
                    result.Message = connection.Message;
                    return result;
                }
                #endregion


                var enterpriseAdapter = _entity.EnterpriseAdapter.Find(model.Id);

                enterpriseAdapter.Adapter.Name = model.Name;
                enterpriseAdapter.ServerIP = model.ServerIP;
                enterpriseAdapter.Port = (int)model.Port;
                enterpriseAdapter.Username = model.Username;
                enterpriseAdapter.Password = model.Password;
                enterpriseAdapter.Adapter.AdapterTypeItemId = (int)model.EnterpriseAdapter;
                enterpriseAdapter.DatabaseName = model.DatabaseName;
                switch (model.Direction)
                {
                    case DirectionEnum.Source:
                        if ((int)model.EnterpriseAdapter < 4)
                        {
                            var adapterTable = enterpriseAdapter.EnterpriseAdapterTable.FirstOrDefault();
                            var tableColumn = adapterTable.EnterpriseAdapterTableColumn.Where(x => x.ParentId == null).ToList();
                            foreach (var child in tableColumn)
                            {
                                DeleteRecursive(child);
                            }
                            var y = _entity.EnterpriseAdapterTableColumn.Select(x => x.Id).ToList().LastOrDefault();
                            if (model.Columns != null)
                            {
                                for (int i = 0; i < model.Columns.Count; i++)
                                {

                                    var child = new EnterpriseAdapterTableColumn
                                    {
                                        Id = (int)model.Columns[i].Id,
                                        ParentId = model.Columns[i].ParentId,
                                        IsForeignKey = model.Columns[i].IsForeignKey,
                                        IsPrimaryKey = model.Columns[i].IsPrimaryKey,
                                        PropertyNameId = (int?)model.Columns[i].PropertyNameId,
                                        RelatedTableName = model.Columns[i].RelatedTableName,
                                        DataTypeId = 1,
                                        ColumnName = model.Columns[i].ColumnName,
                                        EnterpriseAdapterTableId = adapterTable.Id
                                    };
                                    if (model.Columns[i].PropertyNameId == 0)
                                        child.PropertyNameId = null;

                                    if (model.Columns[i].ParentId == 0)
                                        child.ParentId = null;
                                    else
                                        child.ParentId = model.Columns[i].ParentId;
                                    _entity.EnterpriseAdapterTableColumn.Add(child);

                                }
                            }
                        }
                        foreach (var prop in model.Properties.First().SourceProperties ?? new List<PropertyModel>())
                        {
                            var cryptoAdapterProperty = _entity.EnterpriseAdapterProperty.Where(eap => eap.EnterpriseAdapterId == enterpriseAdapter.Id && eap.PropertyId == prop.Id).SingleOrDefault();

                            cryptoAdapterProperty.Value = prop.Value ?? String.Empty;
                        }
                        break;
                    case DirectionEnum.Destination:
                        foreach (var prop in model.Properties.First().DestinationProperties ?? new List<PropertyModel>())
                        {
                            var cryptoAdapterProperty = _entity.EnterpriseAdapterProperty.Where(eap => eap.EnterpriseAdapterId == enterpriseAdapter.Id && eap.PropertyId == prop.Id).SingleOrDefault();

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
                _logger.Information($"EnterpriseAdapterService.UpdateEnterpriseAdapter(model: {model})");
                _logger.Error(ex.Message);
            }
            return result;
        }

        public IResponse<NoValue> DeleteEnterpriseAdapter(long enterpriseAdapterId)
        {
            var result = new Response<NoValue>();
            try
            {
                var enterpriseAdapter = _entity.EnterpriseAdapter.Find(enterpriseAdapterId);

                var adapter = enterpriseAdapter.Adapter;
                var jobDefinitions = _entity.JobDefinition.FirstOrDefault(jd => jd.From == adapter.Id || jd.To == adapter.Id);

                if (jobDefinitions != null)
                {
                    result.Status = StatusEnum.Error;
                    result.Message = "Unable to delete enterprise adapter if it is used in job definition.";
                    return result;
                }

                enterpriseAdapter.Adapter.StatusId = (int)Statuses.Deleted;

                //also delete other related tables (JobDefinition, Account, AccountAddress...)
                _entity.SaveChanges();

                result.Status = StatusEnum.Success;

            }
            catch (Exception ex)
            {
                result.Status = StatusEnum.Error;
                result.Message = Message.SomethingWentWrong;
                _logger.Information($"EnterpriseAdapterService.DeleteEnterpriseAdapter(enterpriseAdapterId: {enterpriseAdapterId})");
                _logger.Error(ex.Message);
            }
            return result;
        }

        public List<EnterpriseAdapterTableColumnModel> GetRecursive(EnterpriseAdapterTableColumn tree, List<EnterpriseAdapterTableColumnModel> list)
        {
            var newAdapter = new EnterpriseAdapterTableColumnModel
            {
                Id = tree.Id,
                ColumnName = tree.ColumnName,
                DataType = (DataTypes)tree.DataTypeId,
                IsForeignKey = tree.IsForeignKey,
                IsPrimaryKey = tree.IsPrimaryKey,
                RelatedTableName = tree.RelatedTableName ?? null,
                ParentId = tree.ParentId ?? null,
                PropertyNameId = (Common.Enums.PropertyName?)tree.PropertyNameId ?? null,
                Children = new List<EnterpriseAdapterTableColumnModel>()

            };
            if (tree.EnterpriseAdapterTableColumn1.Count != 0)
            {

                foreach (var child in tree.EnterpriseAdapterTableColumn1)
                {
                    GetRecursive(child, newAdapter.Children);
                    if (newAdapter.Id != child.ParentId) { 
                        newAdapter.Children.Add(newAdapter);
                }
                }
                
            }
            list.Add(newAdapter);
            return list;
        }

        public void DeleteRecursive(EnterpriseAdapterTableColumn tableColumn)
        {
            foreach (var child in tableColumn.EnterpriseAdapterTableColumn1.ToList())
            {
                DeleteRecursive(child);
            }
            _entity.EnterpriseAdapterTableColumn.Remove(tableColumn);
        }

        private List<PropertyModel> GetProperties(EnterpriseAdapterType enterpriseAdapterType, DirectionEnum direction)
        {
            var result = new List<PropertyModel>();

            result = _entity.AdapterTypeItemProperty
                .Where(atip => atip.AdapterTypeItemId == (int)enterpriseAdapterType && atip.DirectionId == (int)direction)
                .Select(s => new PropertyModel
                {
                    Id = s.Property.Id,
                    Name = s.Property.Name,
                    PropertyType = (PropertyTypeEnum)s.Property.PropertyTypeId
                })
                .ToList();

            return result;
        }

        public EnterpriseAdapterModel GetInitModel()
        {
            var result = new EnterpriseAdapterModel { Properties = new List<EnterpriseModel>() };
            foreach (var adapterType in Enum.GetValues(typeof(EnterpriseAdapterType)).Cast<EnterpriseAdapterType>())
            {
                result.Properties.Add(new EnterpriseModel
                {
                    EnterpriseType = adapterType,
                    SourceProperties = GetProperties(adapterType, DirectionEnum.Source),
                    DestinationProperties = GetProperties(adapterType, DirectionEnum.Destination)
                });
            }

            return result;

        }        

    }
}