
using Prometheus.ExchangesApi.ExchangeClients;
using Prometheus.ExchangesApi.ExchangeClients.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.ExchangesApi.Uow
{
    public class DataClient : IDataClient
    {
        public DataClient()
        {
            Binance = new BinanceClient();
            Bitfinex = new BitfinexClient();
            Poloniex = new PoloniexClient();
            Bitstamp = new BitstampClient();
            Gdax = new GdaxClient();
            Gemini = new GeminiClient();
            Kraken = new KrakenClient();
            Okex = new OkexClient();
        }
        public IBinanceClient Binance { get; set; }
        public IBitfinexClient Bitfinex { get; set; }
        public IPoloniexClient Poloniex { get; set; }
        public IBitstampClient Bitstamp { get; set; }
        public IGdaxClient Gdax { get; set; }
        public IGeminiClient Gemini { get; set; }
        public IKrakenClient Kraken { get; set; }
        public IOkexClient Okex { get; set; }       
    }
}
