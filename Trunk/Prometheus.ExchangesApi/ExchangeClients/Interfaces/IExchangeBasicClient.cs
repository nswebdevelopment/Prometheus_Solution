using ExchangeSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.ExchangesApi.ExchangeClients.Interfaces
{
    /// <summary>
    /// For Exchanges that don't support multiple ticker results per request
    /// </summary>
    public interface IExchangeBasicClient : IBaseExchangeClient
    {
        /// <summary>
        /// Get Tickers From Tihs Exchange for given Symbols
        /// </summary>
        /// <param name="symbolsOfMarket"></param>
        /// <returns>Symbol Name and its exchange Ticker</returns>
        IEnumerable<KeyValuePair<string, ExchangeTicker>> GetTickers(List<string> symbolsOfMarket);
    }
}
