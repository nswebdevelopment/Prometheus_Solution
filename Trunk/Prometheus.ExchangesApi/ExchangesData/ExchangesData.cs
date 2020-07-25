using ExchangeSharp;
using Microsoft.Extensions.Configuration;
using Prometheus.ExchangesApi.Uow;
using Prometheus.Common.Enums;
using Prometheus.Model.Models.ExchangesModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prometheus.ExchangesApi.ExchangeClients.Interfaces;
using Prometheus.ExchangesApi.ExchangeClients;

namespace Prometheus.ExchangesApi.ExchangesData
{
    public class ExchangesData : BaseExchangesData, IExchangesData
    {        
        private readonly string updateIntervalInSecondsStr;
        private readonly TimeSpan _updateInterval;

        private readonly IDataClient _dataClient;
        private AvailableExchanges _exchangeName;      
        private List<string> _symbolsOfMarket;
        private IList<KeyValuePair<string, ExchangeTicker>> dataToQuery = new List<KeyValuePair<string, ExchangeTicker>>();

 
        public ExchangesData(IConfiguration configuration, IDataClient dataClient)
        {            
            updateIntervalInSecondsStr = configuration["UpdateInterval:InSeconds"];
            _updateInterval = TimeSpan.FromSeconds(Convert.ToDouble(Int32.Parse(updateIntervalInSecondsStr)));
            _dataClient = dataClient;
        }

        #region Interface implementations
        public List<TickerModel> GetTickersFor(AvailableExchanges exchangeName, List<string> symbolsOfMarket)
        {
            _exchangeName = exchangeName;
            _symbolsOfMarket = symbolsOfMarket;

            if (!DataLoaded || DataOutdated)
            {
                FetchData();
            }
            SelectDataToQuery();
            return PopulateTickerModelResult();
        }

        public List<TickerModel> MockTickersFor(AvailableExchanges exchangeName, List<string> symbolsOfMarket)
        {
            var result = new List<TickerModel>();
            result = symbolsOfMarket.Select(symbol => new TickerModel
            {
                Symbol = symbol,
                Price = 0,
                Bid = 0,
                Ask = 0,
                Volume = new VolumeModel()
            }).ToList();
            return result;
        }

        public IBaseExchangeClient GetClientInstance(AvailableExchanges exchangeName)
        {
            switch (exchangeName)
            {
                case AvailableExchanges.Binance:
                    return _dataClient.Binance;
                case AvailableExchanges.Bitfinex:
                    return _dataClient.Bitfinex;
                case AvailableExchanges.Bitstamp:
                    return _dataClient.Bitstamp;
                case AvailableExchanges.Gdax:
                    return _dataClient.Gdax;
                case AvailableExchanges.Gemini:
                    return _dataClient.Gemini;
                case AvailableExchanges.Kraken:
                    return _dataClient.Kraken;
                case AvailableExchanges.Okex:
                    return _dataClient.Okex;
                case AvailableExchanges.Poloniex:
                    return _dataClient.Poloniex;
                default:
                    return null;
            }
        } 
        #endregion

        private bool DataLoaded
        {
            get
            {
                switch (_exchangeName)
                {
                    case AvailableExchanges.Binance:
                        return BinanceTickers.Count() > 0 ? true : false;
                    case AvailableExchanges.Bitfinex:
                        return BitfinexTickers.Count() > 0 ? true : false;
                    case AvailableExchanges.Bitstamp:
                        return BitstampTickers.Count(c => _symbolsOfMarket.Contains(c.Key)) == _symbolsOfMarket.Count ? true : false;
                    case AvailableExchanges.Gdax:
                        return GdaxTickers.Count(c => _symbolsOfMarket.Contains(c.Key)) == _symbolsOfMarket.Count ? true : false;
                    case AvailableExchanges.Gemini:
                        return GeminiTickers.Count(c => _symbolsOfMarket.Contains(c.Key)) == _symbolsOfMarket.Count ? true : false;
                    case AvailableExchanges.Kraken:
                        return KrakenTickers.Count(c => _symbolsOfMarket.Contains(c.Key)) == _symbolsOfMarket.Count ? true : false;
                    case AvailableExchanges.Okex:
                        return OkexTickers.Count(c => _symbolsOfMarket.Contains(c.Key)) == _symbolsOfMarket.Count ? true : false;
                    case AvailableExchanges.Poloniex:
                        return PoloniexTickers.Count() > 0 ? true : false;
                    default:
                        return false;
                }
            }            
        }

        private bool DataOutdated
        {            
            get
            {
                switch (_exchangeName)
                {
                    case AvailableExchanges.Binance:
                        return BinanceTickers.First().Value.Volume.Timestamp < DateTime.UtcNow - _updateInterval ? true : false;
                    case AvailableExchanges.Bitfinex:
                        return BitfinexTickers.First().Value.Volume.Timestamp < DateTime.UtcNow - _updateInterval ? true : false;
                    case AvailableExchanges.Bitstamp:
                        return BitstampTickers.First().Value.Volume.Timestamp < DateTime.UtcNow - _updateInterval ? true : false;
                    case AvailableExchanges.Gdax:
                        return GdaxTickers.First().Value.Volume.Timestamp < DateTime.UtcNow - _updateInterval ? true : false;
                    case AvailableExchanges.Gemini:
                        return GeminiTickers.First().Value.Volume.Timestamp < DateTime.UtcNow - _updateInterval ? true : false;
                    case AvailableExchanges.Kraken:
                        return KrakenTickers.First().Value.Volume.Timestamp < DateTime.UtcNow - _updateInterval ? true : false;
                    case AvailableExchanges.Okex:
                        return OkexTickers.First().Value.Volume.Timestamp < DateTime.UtcNow - _updateInterval ? true : false;
                    case AvailableExchanges.Poloniex:
                        return PoloniexTickers.First().Value.Volume.Timestamp < DateTime.UtcNow - _updateInterval ? true : false;
                    default:
                        return false;
                }
            }
        }

        private void FetchData()
        {            
            switch (_exchangeName)
            {
                case AvailableExchanges.Binance:
                    BinanceTickers = _dataClient.Binance.GetTickers().ToList();
                    break;
                case AvailableExchanges.Bitfinex:
                    BitfinexTickers = _dataClient.Bitfinex.GetTickers().ToList();
                    break;
                case AvailableExchanges.Bitstamp:
                    ManageFetchedData(BitstampTickers, _dataClient.Bitstamp.GetTickers(_symbolsOfMarket));                    
                    break;
                case AvailableExchanges.Gdax:
                    ManageFetchedData(GdaxTickers, _dataClient.Gdax.GetTickers(_symbolsOfMarket));
                    break;
                case AvailableExchanges.Gemini:
                    ManageFetchedData(GeminiTickers, _dataClient.Gemini.GetTickers(_symbolsOfMarket));
                    break;
                case AvailableExchanges.Kraken:
                    ManageFetchedData(KrakenTickers, _dataClient.Kraken.GetTickers(_symbolsOfMarket));
                    break;
                case AvailableExchanges.Okex:
                    ManageFetchedData(OkexTickers, _dataClient.Okex.GetTickers(_symbolsOfMarket));
                    break;
                case AvailableExchanges.Poloniex:
                    PoloniexTickers = _dataClient.Poloniex.GetTickers().ToList();
                    break;
                default:
                    break;
            }
        }

        private void SelectDataToQuery()
        {
            switch (_exchangeName)
            {
                case AvailableExchanges.Binance:
                    dataToQuery = BinanceTickers;
                    break;
                case AvailableExchanges.Bitfinex:
                    dataToQuery = BitfinexTickers;
                    break;
                case AvailableExchanges.Bitstamp:
                    dataToQuery = BitstampTickers;
                    break;
                case AvailableExchanges.Gdax:
                    dataToQuery = GdaxTickers;
                    break;
                case AvailableExchanges.Gemini:
                    dataToQuery = GeminiTickers;
                    break;
                case AvailableExchanges.Kraken:
                    dataToQuery = KrakenTickers;
                    break;
                case AvailableExchanges.Okex:
                    dataToQuery = OkexTickers;
                    break;                   
                case AvailableExchanges.Poloniex:
                    dataToQuery = PoloniexTickers;                
                    break;                
                default:
                    break;
            }           
        }

        private List<TickerModel> PopulateTickerModelResult()
        {
            var result = new List<TickerModel>();

            result = dataToQuery.Where(w => _symbolsOfMarket.Contains(w.Key)).Select(ticker => new TickerModel
            {
                Symbol = ticker.Key,
                Price = ticker.Value.Last,
                Bid = ticker.Value.Bid,
                Ask = ticker.Value.Ask,
                Volume = new VolumeModel
                {
                    Timestamp = ticker.Value.Volume.Timestamp,
                    PriceSymbol = ticker.Value.Volume.PriceSymbol,
                    PriceAmount = ticker.Value.Volume.PriceAmount,
                    QuantitySymbol = ticker.Value.Volume.QuantitySymbol,
                    QuantityAmount = ticker.Value.Volume.QuantityAmount
                }
            }).ToList();
            return result;
        }

        private void ManageFetchedData(IList<KeyValuePair<string, ExchangeTicker>> oldTickers, IEnumerable<KeyValuePair<string, ExchangeTicker>> fetchedTickers)
        {
            foreach (var ticker in fetchedTickers)
            {
                if (oldTickers.Any(a => a.Key == ticker.Key))
                {
                    oldTickers.Remove(oldTickers.FirstOrDefault(f => f.Key == ticker.Key));
                }
                oldTickers.Add(new KeyValuePair<string, ExchangeTicker>(ticker.Key, ticker.Value));
            }
        }        
    }
}
