using Prometheus.BL.Interfaces;
using Prometheus.Common;
using Prometheus.Common.Enums;
using Prometheus.Dal.Entities;
using Prometheus.DbAdapter;
using Prometheus.DbAdapter.Queries;
using Prometheus.Model.Models;
using Prometheus.Model.Models.EnterpriseAdapterModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Prometheus.BL.Services
{
    public class JobDefinitionService : IJobDefinitionService
    {
        private readonly IPrometheusEntities _entity;
        private readonly ILogger _logger;
        private readonly IBusinessAdapterService _businessAdapterService;

        public JobDefinitionService(IPrometheusEntities entity, ILogger logger, IBusinessAdapterService businessAdapterService)
        {
            _entity = entity;
            _logger = logger;
            _businessAdapterService = businessAdapterService;
        }

        public IResponse<NoValue> CreateJobDefinition(JobDefinitionModel model)
        {
            var result = new Response<NoValue>();

            try
            {
                var jobDefinition = new Dal.JobDefinition
                {
                    Name = model.Name,
                    Retry = model.Retry,
                    NumberOfRetry = (model.Retry) ? model.NumberOfRetry : default(int),
                    From = model.From,
                    To = model.To,
                    UserProfileId = model.UserProfileId,
                };

                _entity.JobDefinition.Add(jobDefinition);

                var propertiesDal = new List<Dal.JobDefinitionProperty>();
                var propertiesModel = model.PropertiesGet ?? new List<PropertyModel>();

                foreach (var property in propertiesModel)
                {
                    propertiesDal.Add(
                        new Dal.JobDefinitionProperty
                        {
                            JobDefinitionId = jobDefinition.Id,
                            PropertyId = _entity.Property.FirstOrDefault(p => p.Name == property.Name).Id,
                            Value = property.Value ?? String.Empty
                        });
                }

                _entity.JobDefinitionProperty.AddRange(propertiesDal);
                _entity.SaveChanges();

                result.Message = "Job definition added successfully.";
                result.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                result.Status = StatusEnum.Error;
                result.Message = Message.SomethingWentWrong;
                _logger.Information($"JobDefinitionService.CreateJobDefinition(model: {model})");
                _logger.Error(ex.Message);
            }

            return result;
        }

        public IResponse<JobDefinitionModel> GetJobDefinition(long jobDefinitionId)
        {
            var result = new Response<JobDefinitionModel>();

            try
            {
                var jobDefinition = _entity.JobDefinition.Find(jobDefinitionId);

                result.Value = new JobDefinitionModel
                {
                    Id = jobDefinition.Id,
                    Name = jobDefinition.Name,
                    Retry = jobDefinition.Retry,
                    NumberOfRetry = jobDefinition.NumberOfRetry,
                    From = jobDefinition.From,
                    To = jobDefinition.To,
                    UserProfileId = jobDefinition.UserProfileId,
                    PropertiesGet = jobDefinition.JobDefinitionProperty.Select(jdp => new PropertyModel
                    {
                        Name = jdp.Property.Name,
                        PropertyType = (PropertyTypeEnum)jdp.Property.PropertyTypeId,
                        Value = jdp.Value
                    }).ToList()
                };

                result.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                result.Status = StatusEnum.Error;
                result.Message = Message.SomethingWentWrong;
                _logger.Information($"JobDefinitionService.GetJobDefinition(jobDefinitionId: {jobDefinitionId})");
                _logger.Error(ex.Message);
            }

            return result;
        }

        public IResponse<List<JobDefinitionModel>> GetJobDefinitions(long userProfileId)
        {
            var result = new Response<List<JobDefinitionModel>>();

            try
            {
                result.Value = _entity.JobDefinition.Where(j => j.UserProfileId == userProfileId)
                    .Select(j => new JobDefinitionModel
                    {
                        Id = j.Id,
                        Name = j.Name,
                        Retry = j.Retry,
                        NumberOfRetry = j.NumberOfRetry
                    }).ToList();

                result.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                result.Status = StatusEnum.Error;
                result.Message = Message.SomethingWentWrong;
                _logger.Information($"JobDefinitionService.GetJobDefinitions(userProfileId: {userProfileId})");
                _logger.Error(ex.Message);
            }

            return result;
        }

        public IResponse<JobDefinitionModel> UpdateJobDefinition(JobDefinitionModel model)
        {
            var result = new Response<JobDefinitionModel>();
            try
            {
                var jobDefinition = _entity.JobDefinition.Find(model.Id);

                if (jobDefinition.Schedule.Count != 0)
                {
                    //status should be 'Unable..'....
                    result.Status = StatusEnum.NotFound;
                    result.Message = "Unable to update job definition if the job is scheduled.";
                    return result;
                }

                jobDefinition.Name = model.Name;
                jobDefinition.Retry = model.Retry;
                jobDefinition.NumberOfRetry = (model.Retry) ? model.NumberOfRetry : default(int);

                foreach (var propertyDal in jobDefinition.JobDefinitionProperty.ToList())
                {
                    foreach (var propertyModel in model.PropertiesGet ?? new List<PropertyModel>())
                    {
                        if (propertyModel.Name == propertyDal.Property.Name)
                        {
                            propertyDal.Value = propertyModel.Value ?? String.Empty;
                        }
                    }
                }

                _entity.SaveChanges();
                result.Status = StatusEnum.Success;
                result.Message = Message.ChangesSaved;

            }
            catch (Exception ex)
            {
                result.Status = StatusEnum.Error;
                result.Message = Message.SomethingWentWrong;
                _logger.Information($"JobDefinitionService.UpdateJobDefinition(model: {model})");
                _logger.Error(ex.Message);
            }
            return result;
        }

        public IResponse<NoValue> DeleteJobDefinition(long jobDefinitionId)
        {
            var result = new Response<NoValue>();
            try
            {
                //hard delete until we have StatusId in JobDefinition Table
                var jobDefinition = _entity.JobDefinition.Find(jobDefinitionId);

                if (jobDefinition.Schedule.Count != 0)
                {
                    //status should be 'Unable..'....
                    result.Status = StatusEnum.NotFound;
                    result.Message = "Unable to delete job definition if the job is scheduled.";
                    return result;
                }

                _entity.JobDefinitionProperty.RemoveRange(jobDefinition.JobDefinitionProperty);
                _entity.JobDefinition.Remove(jobDefinition);
                _entity.SaveChanges();
                result.Status = StatusEnum.Success;

            }
            catch (Exception ex)
            {
                result.Status = StatusEnum.Error;
                result.Message = Message.SomethingWentWrong;
                _logger.Information($"JobDefinitionService.DeleteJobDefinition(jobDefinitionId: {jobDefinitionId})");
                _logger.Error(ex.Message);
            }
            return result;
        }

        public IResponse<Config> GetAdapterParameter(long jobDefinitionId)
        {
            var result = new Response<Config>();

            try
            {
                var jobDefinition = _entity.JobDefinition.Find(jobDefinitionId);
                var enterpriseAdapter = jobDefinition.Adapter.EnterpriseAdapter.FirstOrDefault();
                
                result.Value = new Config
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

                result.Value.Adapter = (AdapterTypeItemEnum)enterpriseAdapter.Adapter.AdapterTypeItemId;

                var adapter = result.Value.Adapter;


                var adapterTable = enterpriseAdapter.EnterpriseAdapterTable.FirstOrDefault();
                var adapterTableName = SetDelimeter(adapterTable.TableName, adapter);

                var columns = adapterTable.EnterpriseAdapterTableColumn.Where(ad => ad.ParentId == null);

                string primaryKey = null;
                List<ColumnModel> Columns = new List<ColumnModel>();
                Dictionary<string, string> ForeignKeys = new Dictionary<string, string>();
                List<ChildTableModel> ChildTables = new List<ChildTableModel>();

                foreach (var column in columns)
                {
                    if (column.IsPrimaryKey)
                    {
                        primaryKey = SetDelimeter(column.ColumnName, adapter);
                        Columns.Add(new ColumnModel
                        {
                            ColumnName = SetDelimeter(column.ColumnName, adapter),
                            PropertyName = column.PropertyName?.Name
                        });
                    }
                    else if (column.IsForeignKey)
                    {
                        var childTableModel = new ChildTableModel
                        {
                            Columns = new List<ColumnModel>()
                        };
                        childTableModel.TableName = SetDelimeter(column.RelatedTableName, adapter);
                        var childs = adapterTable.EnterpriseAdapterTableColumn.Where(ad => ad.ParentId == column.Id);

                        foreach (var child in childs)
                        {

                            if (child.IsPrimaryKey)
                            {
                                childTableModel.PrimaryKey = SetDelimeter(child.ColumnName, adapter);
                            }
                            else
                            {
                                childTableModel.Columns.Add(new ColumnModel
                                {
                                    ColumnName = SetDelimeter(child.ColumnName, adapter),
                                    PropertyName = child.PropertyName?.Name
                                });
                            }
                        }
                        ForeignKeys.Add(SetDelimeter(column.RelatedTableName, adapter), SetDelimeter(column.ColumnName, adapter));
                        ChildTables.Add(childTableModel);
                    }
                    else
                    {
                        Columns.Add(new ColumnModel
                        {
                            ColumnName = SetDelimeter(column.ColumnName, adapter),
                            PropertyName = column.PropertyName?.Name
                        });
                    }
                }

                result.Value.QueryRead = new QueryRead()
                {
                    ParentTable = new ParentTableModel
                    {
                        TableName = adapterTableName,
                        Columns = Columns,
                        ForeignKeys = ForeignKeys,
                        PrimaryKey = primaryKey
                    },
                    ChildTables = ChildTables
                };

                result.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                result.Status = StatusEnum.Error;
                result.Message = Message.SomethingWentWrong;
                _logger.Information($"JobDefinitionService.GetAdapterParameter(jobDefinitionId: {jobDefinitionId})");
                _logger.Error(ex.Message);
            }

            return result;
        }

        public IResponse<JobDefinitionModel> GetJobDefinitionByJobId(long jobId)
        {
            var response = new Response<JobDefinitionModel>();
            try
            {
                var jobDefinition = _entity.JobTimeline.Find(jobId).Schedule.JobDefinition;

                var adapter = _entity.CryptoAdapter.FirstOrDefault(ca => ca.AdapterId == jobDefinition.From);

                response.Value = new JobDefinitionModel
                {
                    Id = jobDefinition.Id,
                    Name = jobDefinition.Name,
                    NumberOfRetry = jobDefinition.NumberOfRetry,
                    Retry = jobDefinition.Retry,
                    UserProfileId = jobDefinition.UserProfileId,
                    From = jobDefinition.From,
                    To = jobDefinition.To
                };

                if (adapter != null)
                {
                    response.Value.FromCategory = AdapterType.Crypto;
                }

                response.Status = StatusEnum.Success;
            }
            catch (Exception ex)
            {
                response.Status = StatusEnum.Error;
                response.Message = ex.Message;
                _logger.Information($"JobDefinitionService.GetJobDefinitionByJobId(jobId: {jobId})");
                _logger.Error(ex.Message);
            }
            return response;
        }


        public List<int> CheckAdapterMapping(long adapterId)
        {
            int adapterTypeItemId;

            var adapter = _entity.Adapter.Find(adapterId);
            adapterTypeItemId = adapter.AdapterTypeItemId;

            var mappingList = _entity.AdapterMapping.Where(am => am.From == adapterTypeItemId).Select(am => am.To).ToList();

            return mappingList;
        }


        #region Helper

        private string SetDelimeter(string name, AdapterTypeItemEnum adapter)
        {
            string nameDelimeted = name;

            switch (adapter)
            {
                case (AdapterTypeItemEnum.MSSQL):
                    nameDelimeted = $"[{name}]";
                    break;
                case (AdapterTypeItemEnum.MySQL):
                    nameDelimeted = $"`{name}`";
                    break;
                case (AdapterTypeItemEnum.Oracle):
                    nameDelimeted = $"\"{name}\"";
                    break;
                case (AdapterTypeItemEnum.MongoDB):
                    //check for reserved words in MongoDB
                    break;
            }

            return nameDelimeted;
        }

        #endregion
    }
}
