using Prometheus.BL.Interfaces;
using Prometheus.Common.Enums;
using Prometheus.Dal;
using Prometheus.Dal.Entities;
using Prometheus.ExchangesApi.ExchangeClients.Interfaces;
using Prometheus.ExchangesApi.ExchangesData;
using Prometheus.Model.Models.ExchangesModel;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.BL.Services
{
    public class ExchangesService : IExchangesService
    {
        private readonly IPrometheusEntities _context;
        private readonly IExchangesData _exchangesData;
        private readonly ILogger _logger;
        public ExchangesService(IPrometheusEntities context, IExchangesData exchangesData, ILogger logger)
        {
            _exchangesData = exchangesData;
            _context = context;
            _logger = logger;
        }

        #region Public - interface implementations
        public List<BasicInfoModel> GetAll()
        {
            var result = new List<BasicInfoModel>();
            try
            {
                result = _context.Exchange.Select(exchange => new BasicInfoModel
                {
                    Name = exchange.Name,
                    ImageUrl = exchange.ImageUrl,
                    Country = exchange.Country,
                    Type = exchange.Type,
                    Description = exchange.Description,
                    PairsCount = exchange.Market.Any() ? exchange.Market.Sum(c => c.Symbol.Count) : 0
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.Information("ExcangesService.GetAll()");
                _logger.Error(ex.Message);
                return result;

            }
            return result;
        }

        public ExtendedInfoModel GetDetails(AvailableExchanges name)
        {
            var result = new ExtendedInfoModel();
            try
            {
                var exchange = _context.Exchange.FirstOrDefault(f => f.Name == name.ToString());
                result = new ExtendedInfoModel
                {
                    Name = exchange.Name,
                    ImageUrl = exchange.ImageUrl,
                    Country = exchange.Country,
                    Type = exchange.Type,
                    Description = exchange.Description,
                    Markets = exchange.Market.Where(w => w.Symbol.Any()).Select(m => m.Name).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.Information($"ExcangesService.GetDetails(name: {name})");
                _logger.Error(ex.Message);
                return result;
            }
            return result;
        }

        public bool SaveSymbolsForExchange(AvailableExchanges name)
        {
            try
            {
                IBaseExchangeClient exchangeClient = _exchangesData.GetClientInstance(name);
                var symbols = exchangeClient.GetSymbols().ToList();
                List<MarketModel> markets = FindAndPopulateDistinctMarketsForExchange(symbols);

                #region For Testing
                ////test if all symbols are distributed to different markets
                //var totalSimbolCount = symbols.Count;
                //var simbolCountInMarkets = 0;
                //foreach (var market in markets)
                //{
                //    simbolCountInMarkets += market.CurrencyPairs.Count;
                //}                
                //if (simbolCountInMarkets == totalSimbolCount)
                //{
                //    //success
                //    totalSimbolCount = 0;
                //} 
                #endregion

                var exchange = _context.Exchange.FirstOrDefault(e => e.Name == name.ToString());
                if (exchange.Market.Any())
                {
                    return false;
                }
                foreach (var market in markets)
                {
                    var marketToSave = new Market
                    {
                        Exchange = exchange,
                        Name = market.Name,
                        StatusId = (int)Statuses.Active,
                    };
                    _context.Market.Add(marketToSave);

                    foreach (var pair in market.Pairs)
                    {
                        var symbolToSave = new Symbol
                        {
                            Market = marketToSave,
                            Name = pair,
                            StatusId = (int)Statuses.Active
                        };
                        _context.Symbol.Add(symbolToSave);
                    }
                }
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Information($"ExchangesService.SaveSymbolsForExchange(name: {name})");
                _logger.Error(ex.Message);
                return false;
            }
            return true;
        }

        public bool DeleteAllSymbolsForExchange(AvailableExchanges name)
        {
            try
            {
                var exchange = _context.Exchange.FirstOrDefault(e => e.Name == name.ToString());

                _context.Symbol.RemoveRange(exchange.Market.SelectMany(s => s.Symbol));
                _context.Market.RemoveRange(exchange.Market);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Information($"ExchangesService.DeleteAllSymbolsForExchange(name: {name})");
                _logger.Error(ex.Message);
                return false;
            }
            return true;
        }

        public List<TickerModel> GetOverview(AvailableExchanges exchangeName, string market)
        {
            var result = new List<TickerModel>();
            try
            {
                var symbolsOfMarket = new List<string>();

                //TODO paging sorting etc
                var exchangeMarket = _context.Exchange
                                                .FirstOrDefault(f => f.Id == (int)exchangeName)
                                                    .Market.FirstOrDefault(f => !string.IsNullOrWhiteSpace(market) ? f.Name == market : true);
                if (exchangeMarket == null)
                {
                    return result;
                }

                symbolsOfMarket = exchangeMarket.Symbol.Select(s => s.Name).ToList();

                if (string.IsNullOrWhiteSpace(symbolsOfMarket.FirstOrDefault()))
                {
                    return result;
                }

                result = _exchangesData.GetTickersFor(exchangeName, symbolsOfMarket);
            }
            catch (Exception ex)
            {
                _logger.Information($"ExchangesService.GetOverview(exchangeName: {exchangeName}, market: {market})");
                _logger.Error(ex.Message);
                return result;
            }
            return result;
        }
        #endregion

        #region Private - helper methods
        private List<MarketModel> FindAndPopulateDistinctMarketsForExchange(List<string> symbols)
        {
            var result = new List<MarketModel>();

            List<string> distinctMarkets = FindDistinctMarketsOnly(symbols);

            var marketsIterated = new List<string>();

            foreach (var market in distinctMarkets.OrderByDescending(o => o.Length))
            {
                if (marketsIterated.Count == 0)
                {
                    result.Add(new MarketModel
                    {
                        Name = market,
                        Pairs = symbols.Where(w => w.EndsWith(market)).ToList()
                    });
                }
                else
                {
                    marketsIterated.OrderByDescending(o => o.Length);
                    var remainingSymbols = symbols.Where(s => !symbols.Where(w => marketsIterated.Any(me => w.EndsWith(me))).ToList().Contains(s)).ToList();

                    result.Add(new MarketModel
                    {
                        Name = market,
                        Pairs = remainingSymbols.Where(w => w.EndsWith(market)).ToList()
                    });
                }
                marketsIterated.Add(market);
            }
            return result;
        }

        private List<string> FindDistinctMarketsOnly(List<string> symbols)
        {
            //todo some ref
            var distinctMarkets = new List<string>();

            var initialLength8 = symbols.Where(w => w.Length >= 8).Select(s => s.Replace("-", string.Empty).Replace("_", string.Empty).Replace("/", string.Empty)).ToList();
            var initialLength7 = symbols.Where(w => w.Length == 7).Select(s => s.Replace("-", string.Empty).Replace("_", string.Empty).Replace("/", string.Empty)).ToList();
            var initialLength6 = symbols.Where(w => w.Length == 6).Select(s => s.Replace("-", string.Empty).Replace("_", string.Empty).Replace("/", string.Empty)).ToList();
            var initialLength5 = symbols.Where(w => w.Length == 5).ToList();

            var currentLength5 = new List<string>();
            currentLength5.AddRange(initialLength5);
            currentLength5.AddRange(initialLength6.Where(w => w.Length == 5).ToList());

            var currentLengt6 = new List<string>();
            currentLengt6.AddRange(initialLength6.Where(w => w.Length == 6).ToList());
            currentLengt6.AddRange(initialLength7.Where(w => w.Length == 6).ToList());

            var currentLength7 = new List<string>();
            currentLength7.AddRange(initialLength7.Where(w => w.Length == 7).ToList());
            currentLength7.AddRange(initialLength8.Where(w => w.Length == 7).ToList());

            var currentLengt8 = new List<string>();
            currentLengt8.AddRange(initialLength8.Where(w => w.Length == 8).ToList());

            //eg BTC_USD ---- BTC is base asset and USD i quote asset
            var baseAssets = new List<string>();
            var quoteAssets = new List<string>();

            foreach (var pair in currentLengt6)
            {
                AddPairToBaseAndQuoteAssets(GetBaseAndQouteAssetFromString(pair, 3, 3), baseAssets, quoteAssets);
            }

            foreach (var pair in currentLength7)
            {
                if (
                    (baseAssets.Any(w => w == pair.Substring(0, 3)) || quoteAssets.Any(w => w == pair.Substring(0, 3)))
                    &&
                    (!baseAssets.Any(w => w == pair.Substring(4, 3)) && !quoteAssets.Any(w => w == pair.Substring(4, 3)))
                    )
                {
                    AddPairToBaseAndQuoteAssets(GetBaseAndQouteAssetFromString(pair, 3, 4), baseAssets, quoteAssets);
                }
                else
                {
                    AddPairToBaseAndQuoteAssets(GetBaseAndQouteAssetFromString(pair, 4, 3), baseAssets, quoteAssets);
                }
            }

            foreach (var pair in currentLengt8)
            {
                if (
                    (baseAssets.Any(w => w == pair.Substring(0, 3)) || quoteAssets.Any(w => w == pair.Substring(0, 3)))
                    &&
                    (!baseAssets.Any(w => w == pair.Substring(5, 3)) && !quoteAssets.Any(w => w == pair.Substring(5, 3)))
                    )
                {
                    AddPairToBaseAndQuoteAssets(GetBaseAndQouteAssetFromString(pair, 3, 5), baseAssets, quoteAssets);
                }
                else
                {
                    AddPairToBaseAndQuoteAssets(GetBaseAndQouteAssetFromString(pair, 5, 3), baseAssets, quoteAssets);
                }
            }

            foreach (var pair in currentLength5)
            {
                if (
                    (baseAssets.Any(w => w == pair.Substring(0, 3)) || quoteAssets.Any(w => w == pair.Substring(0, 3)))
                    &&
                    (!baseAssets.Any(w => w == pair.Substring(2, 3)) || !quoteAssets.Any(w => w == pair.Substring(2, 3)))
                    )
                {
                    AddPairToBaseAndQuoteAssets(GetBaseAndQouteAssetFromString(pair, 3, 2), baseAssets, quoteAssets);
                }
                else
                {
                    AddPairToBaseAndQuoteAssets(GetBaseAndQouteAssetFromString(pair, 2, 3), baseAssets, quoteAssets);
                }
            }

            distinctMarkets = quoteAssets.Distinct().OrderByDescending(o => o.Length).ToList();
            return distinctMarkets;
        }

        private KeyValuePair<string, string> GetBaseAndQouteAssetFromString(string pair, int firstAssetNameLength, int secondAssetNameLength)
        {
            return new KeyValuePair<string, string>(pair.Substring(0, firstAssetNameLength), pair.Substring(firstAssetNameLength, secondAssetNameLength));
        }

        private void AddPairToBaseAndQuoteAssets(KeyValuePair<string, string> baseQuotePair, List<string> baseAssets, List<string> quoteAssets)
        {
            baseAssets.Add(baseQuotePair.Key);
            quoteAssets.Add(baseQuotePair.Value);
        }
        #endregion
    }
}
