using ExchangeSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.ExchangesApi.ExchangeClients.Interfaces
{
    /// <summary>
    /// For Exchanges that support multiple ticker results per request
    /// </summary>
    public interface IExchangeClient : IBaseExchangeClient
    {
        /// <summary>
        /// Get All Tickers From This Exchange
        /// </summary>
        /// <returns>Symbol Name and its exchange Ticker</returns>
        IEnumerable<KeyValuePair<string, ExchangeTicker>> GetTickers();        
    }
}
