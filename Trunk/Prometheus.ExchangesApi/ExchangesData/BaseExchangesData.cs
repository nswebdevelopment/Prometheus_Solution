using ExchangeSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.ExchangesApi.ExchangesData
{
    public abstract class BaseExchangesData
    {
        //apis support bulk result
        public IList<KeyValuePair<string, ExchangeTicker>> BinanceTickers = new List<KeyValuePair<string, ExchangeTicker>>();
        public IList<KeyValuePair<string, ExchangeTicker>> BitfinexTickers = new List<KeyValuePair<string, ExchangeTicker>>();
        public IList<KeyValuePair<string, ExchangeTicker>> PoloniexTickers = new List<KeyValuePair<string, ExchangeTicker>>();
        //apis don't support bulk result
        public IList<KeyValuePair<string, ExchangeTicker>> BitstampTickers = new List<KeyValuePair<string, ExchangeTicker>>();
        public IList<KeyValuePair<string, ExchangeTicker>> GdaxTickers = new List<KeyValuePair<string, ExchangeTicker>>();
        public IList<KeyValuePair<string, ExchangeTicker>> GeminiTickers = new List<KeyValuePair<string, ExchangeTicker>>();
        public IList<KeyValuePair<string, ExchangeTicker>> KrakenTickers = new List<KeyValuePair<string, ExchangeTicker>>();
        public IList<KeyValuePair<string, ExchangeTicker>> OkexTickers = new List<KeyValuePair<string, ExchangeTicker>>();
    }
}
