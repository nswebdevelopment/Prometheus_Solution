using Prometheus.BL.Interfaces;
using Prometheus.Common.Enums;
using Prometheus.Dal.Entities;
using Prometheus.Model.Models;
using Prometheus.Model.Models.EnterpriseAdapterModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Prometheus.BL.Services
{
    public class SelectListService : ISelectListService
    {
        private readonly IPrometheusEntities _entity;
        private readonly ILogger _logger;

        public SelectListService(IPrometheusEntities entity, ILogger logger)
        {
            _entity = entity;
            _logger = logger;
        }

        public List<T> GetList<T>(long userProfileId)
        {
            try
            {
                if (typeof(T) == typeof(EnterpriseAdapterSelectListModel))
                {
                    var result = _entity.Adapter.Where(a => a.UserProfileId == userProfileId && a.StatusId == (int)Statuses.Active && a.AdapterTypeItem.AdapterTypeId == (int)AdapterType.Enterprise && a.DirectionId == (int)DirectionEnum.Source).Select(a => new EnterpriseAdapterSelectListModel
                    {
                        Id = a.Id,
                        Name = a.Name + "/" + a.AdapterTypeItemId
                    }).ToList();

                    return result as List<T>;
                }
                else if (typeof(T) == typeof(CryptoAdapterSelectListModel))
                {
                    var result = _entity.Adapter.Where(a => a.UserProfileId == userProfileId && a.StatusId == (int)Statuses.Active && a.AdapterTypeItem.AdapterTypeId == (int)AdapterType.Crypto && a.DirectionId == (int)DirectionEnum.Source).Select(b => new CryptoAdapterSelectListModel
                    {
                        Id = b.Id,
                        Name = b.Name + "/" + b.AdapterTypeItemId
                    }).ToList();

                    return result as List<T>;
                }
                else if (typeof(T) == typeof(BusinessAdapterSelectListModel))
                {
                    var result = _entity.Adapter.Where(a => a.UserProfileId == userProfileId && a.StatusId == (int)Statuses.Active && a.AdapterTypeItem.AdapterTypeId == (int)AdapterType.Business && a.DirectionId == (int)DirectionEnum.Source).Select(b => new BusinessAdapterSelectListModel
                    {
                        Id = b.Id,
                        Name = b.Name + "/" + b.AdapterTypeItemId
                    }).ToList();

                    return result as List<T>;
                }
                else if (typeof(T) == typeof(JobDefinitionSelectListModel))
                {
                    var result = _entity.JobDefinition.Where(j => j.UserProfileId == userProfileId).Select(j => new JobDefinitionSelectListModel
                    {
                        Id = j.Id,
                        Name = j.Name
                    }).ToList();

                    return result as List<T>;
                }
                else if(typeof(T) == typeof(AdapterSelectListModel))
                {
                    var result = _entity.Adapter.Where(a => a.UserProfileId == userProfileId && a.StatusId == (int)Statuses.Active).Select(a => new AdapterSelectListModel
                    {
                        Id = a.Id,
                        Name = a.Name
                    }).ToList();

                    return result as List<T>;
                }
            }
            catch (Exception ex)
            {
                _logger.Information($"SelectListService.GetList<T>(userProfileId: {userProfileId})");
                _logger.Error(ex.Message);
            }
            return null;
        }

        public List<T> GetMappedList<T>(long userProfileId, List<int> typeItems)
        {
            try
            {
                if (typeof(T) == typeof(EnterpriseAdapterSelectListModel))
                {
                    var result = new List<EnterpriseAdapterSelectListModel>();
                    var adapters = _entity.Adapter.Where(a => a.UserProfileId == userProfileId && a.StatusId == (int)Statuses.Active && a.AdapterTypeItem.AdapterTypeId == (int)AdapterType.Enterprise && a.DirectionId == (int)DirectionEnum.Destination).ToList();
                    
                    foreach(var typeItem in typeItems)
                    {
                        result.AddRange(adapters.Where(a => a.AdapterTypeItemId == typeItem).Select(a => new EnterpriseAdapterSelectListModel
                        {
                            Id = a.Id,
                            Name = a.Name + "/" + a.AdapterTypeItemId
                        }).ToList());
                    }
                    
                    return result as List<T>;
                }
                else if (typeof(T) == typeof(CryptoAdapterSelectListModel))
                {
                    var result = new List<CryptoAdapterSelectListModel>();
                    var adapters = _entity.Adapter.Where(a => a.UserProfileId == userProfileId && a.StatusId == (int)Statuses.Active && a.AdapterTypeItem.AdapterTypeId == (int)AdapterType.Crypto &&  a.DirectionId == (int)DirectionEnum.Destination).ToList();

                    foreach (var typeItem in typeItems)
                    {
                        result.AddRange(adapters.Where(a => a.AdapterTypeItemId == typeItem).Select(a => new CryptoAdapterSelectListModel
                        {
                            Id = a.Id,
                            Name = a.Name + "/" + a.AdapterTypeItemId
                        }).ToList());
                    }

                    return result as List<T>;
                }
                else if (typeof(T) == typeof(BusinessAdapterSelectListModel))
                {
                    var result = new List<BusinessAdapterSelectListModel>();
                    var adapters = _entity.Adapter.Where(a => a.UserProfileId == userProfileId && a.StatusId == (int)Statuses.Active && a.AdapterTypeItem.AdapterTypeId == (int)AdapterType.Business && a.DirectionId == (int)DirectionEnum.Destination).ToList();

                    foreach (var typeItem in typeItems)
                    {
                        result.AddRange(adapters.Where(a => a.AdapterTypeItemId == typeItem).Select(a => new BusinessAdapterSelectListModel
                        {
                            Id = a.Id,
                            Name = a.Name + "/" + a.AdapterTypeItemId
                        }).ToList());
                    }

                    return result as List<T>;
                }
            }
            catch (Exception ex)
            {
                _logger.Information($"SelectListService.GetMappedList<T>(userProfileId: {userProfileId})");
                _logger.Error(ex.Message);
            }
            return null;
        }

        public List<T> GetPropertiesList<T>(long userProfileId)
        {
            try
            {
                if (typeof(T) == typeof(JobDefinitionPropertyModel))
                {
                    var result = new List<JobDefinitionPropertyModel>();
                    var jobDefinitionProperties = _entity.JobDefinitionAdapterTypeItemProperty.ToList();

                    var adapterTypeItems = _entity.Adapter.Where(a => a.UserProfileId == userProfileId && a.StatusId == (int)Statuses.Active).Select(a => a.AdapterTypeItemId).Distinct().ToList();

                    foreach(var adapterTypeItem in adapterTypeItems)
                    {
                        result.Add(new JobDefinitionPropertyModel
                        {
                            AdapterTypeItem = (AdapterTypeItemEnum)adapterTypeItem,
                            FromProperties = jobDefinitionProperties.Where(jdp => jdp.AdapterTypeItemId == adapterTypeItem && jdp.DirectionId == (int)DirectionEnum.Source).Select(jdp => new PropertyModel
                            {
                                Name = jdp.Property.Name,
                                PropertyType = (PropertyTypeEnum)jdp.Property.PropertyTypeId
                            }).ToList(),
                            ToProperties = jobDefinitionProperties.Where(jdp => jdp.AdapterTypeItemId == adapterTypeItem && jdp.DirectionId == (int)DirectionEnum.Destination).Select(jdp => new PropertyModel
                            {
                                Name = jdp.Property.Name,
                                PropertyType = (PropertyTypeEnum)jdp.Property.PropertyTypeId
                            }).ToList(),
                        });
                    }
                    return result as List<T>;
                }
            }
            catch (Exception ex)
            {
                _logger.Information($"SelectListService.GetPropertiesList<T>()");
                _logger.Error(ex.Message);
            }
            return null;
        }
    }
}
