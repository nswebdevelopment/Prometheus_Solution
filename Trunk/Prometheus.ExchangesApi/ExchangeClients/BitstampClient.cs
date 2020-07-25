using ExchangeSharp;
using Prometheus.ExchangesApi.ExchangeClients.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.ExchangesApi.ExchangeClients
{
    public class BitstampClient : ExchangeBitstampAPI, IBitstampClient
    {
        public IEnumerable<KeyValuePair<string, ExchangeTicker>> GetTickers(List<string> symbolsOfMarket)
        {
            var fetchedResult = new List<KeyValuePair<string, ExchangeTicker>>();
            foreach (var symbol in symbolsOfMarket)
            {
                fetchedResult.Add(new KeyValuePair<string, ExchangeTicker>(symbol, base.GetTicker(symbol)));               
            }
            return fetchedResult;
        }
    }
}
