using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExchangeSharp;
using Prometheus.ExchangesApi.ExchangeClients.Interfaces;

namespace Prometheus.ExchangesApi.ExchangeClients
{
    public class GeminiClient : ExchangeGeminiAPI, IGeminiClient
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
