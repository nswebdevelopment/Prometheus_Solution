using Prometheus.Common.Enums;
using Prometheus.ExchangesApi.ExchangeClients.Interfaces;
using Prometheus.Model.Models.ExchangesModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.ExchangesApi.ExchangesData
{
    public interface IExchangesData
    {
        List<TickerModel> GetTickersFor(AvailableExchanges exchangeName, List<string> symbolsOfMarket);
        /// <summary>
        /// For Development only
        /// </summary>
        /// <param name="exchangeName"></param>
        /// <param name="symbolsOfMarket"></param>
        /// <returns></returns>
        List<TickerModel> MockTickersFor(AvailableExchanges exchangeName, List<string> symbolsOfMarket);
        IBaseExchangeClient GetClientInstance(AvailableExchanges exchangeName);
    }
}
