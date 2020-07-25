using Prometheus.ExchangesApi.ExchangeClients.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.ExchangesApi.Uow
{
    public interface IDataClient
    {
        IBinanceClient Binance { get; set; }
        IBitfinexClient Bitfinex { get; set; }
        IPoloniexClient Poloniex { get; set; }
        IBitstampClient Bitstamp { get; set; }
        IGdaxClient Gdax { get; set; }
        IGeminiClient Gemini { get; set; }
        IKrakenClient Kraken { get; set; }
        IOkexClient Okex { get; set; }        
    }
}
