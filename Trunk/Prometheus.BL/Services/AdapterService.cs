using AutoMapper;
using Microsoft.Extensions.Options;
using Prometheus.BL.Interfaces;
using Prometheus.Dal;
using Prometheus.Dal.Entities;
using Prometheus.Model;
using Serilog;
using System;
using System.Collections.Generic;

namespace Prometheus.BL.Services
{
    public class AdapterService : IAdapterService
    {
        public AdapterService(IOptions<AdapterSeed> seed, IMapper mapper, IPrometheusEntities entity, ILogger logger)
        {
            _seed = seed;
            _mapper = mapper;
            _entity = entity;
            _logger = logger;
        }

        private readonly IPrometheusEntities _entity;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IOptions<AdapterSeed> _seed;

        public void AdapterSeed(long userProfileId)
        {
            try
            {
                foreach (var node in _seed.Value.CryptoAdapters ?? new List<Cryptoadapter>().ToArray())
                {
                    var adapter = _mapper.Map<Adapter>(node);
                    adapter.UserProfileId = userProfileId;
                    _entity.Adapter.Add(adapter);

                    var cryptoAdapter = _mapper.Map<CryptoAdapter>(node);
                    cryptoAdapter.Adapter = adapter;
                    _entity.CryptoAdapter.Add(cryptoAdapter);

                    foreach (var property in node.Properties ?? new List<Model.Property>().ToArray())
                    {
                        var cryptoAdapterProperty = _mapper.Map<CryptoAdapterProperty>(property);
                        cryptoAdapterProperty.CryptoAdapter = cryptoAdapter;
                        _entity.CryptoAdapterProperty.Add(cryptoAdapterProperty);
                    }
                }

                foreach (var business in _seed.Value.BusinessAdapters ?? new List<Businessadapter>().ToArray())
                {
                    var adapter = _mapper.Map<Adapter>(business);
                    adapter.UserProfileId = userProfileId;
                    _entity.Adapter.Add(adapter);

                    var businessAdapter = _mapper.Map<BusinessAdapter>(business);
                    businessAdapter.Adapter = adapter;
                    _entity.BusinessAdapter.Add(businessAdapter);
                }

                _entity.SaveChanges();

            }
            catch (Exception ex)
            {
                _logger.Information($"AdapterService.AdapterSeed(userProfileId: {userProfileId})");
                _logger.Error(ex.Message);
            }
        }
    }
}
