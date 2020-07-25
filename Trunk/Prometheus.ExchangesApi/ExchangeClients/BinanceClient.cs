using ExchangeSharp;
using Newtonsoft.Json.Linq;
using Prometheus.ExchangesApi.ExchangeClients.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.ExchangesApi.ExchangeClients
{
    public class BinanceClient : ExchangeBinanceAPI, IBinanceClient
    {
        public override IEnumerable<KeyValuePair<string, ExchangeTicker>> GetTickers()
        {
            var result = new List<KeyValuePair<string, ExchangeTicker>>();
            string symbol;
            JToken obj = MakeJsonRequest<JToken>("/ticker/24hr");
            CheckError(obj);
            foreach (JToken child in obj)
            {
                symbol = child["symbol"].ToStringInvariant();
                result.Add(new KeyValuePair<string, ExchangeTicker>(symbol, ParseTicker(symbol, child)));
            }
            return result;
        }

        #region private methods from Base Class BinanceAPI 
        private void CheckError(JToken result)
        {
            if (result != null && !(result is JArray) && result["status"] != null && result["code"] != null)
            {
                throw new APIException(result["code"].ToStringInvariant() + ": " + (result["msg"] != null ? result["msg"].ToStringInvariant() : "Unknown Error"));
            }
        }

        private ExchangeTicker ParseTicker(string symbol, JToken token)
        {
            // {"priceChange":"-0.00192300","priceChangePercent":"-4.735","weightedAvgPrice":"0.03980955","prevClosePrice":"0.04056700","lastPrice":"0.03869000","lastQty":"0.69300000","bidPrice":"0.03858500","bidQty":"38.35000000","askPrice":"0.03869000","askQty":"31.90700000","openPrice":"0.04061300","highPrice":"0.04081900","lowPrice":"0.03842000","volume":"128015.84300000","quoteVolume":"5096.25362239","openTime":1512403353766,"closeTime":1512489753766,"firstId":4793094,"lastId":4921546,"count":128453}
            return new ExchangeTicker
            {
                Ask = token["askPrice"].ConvertInvariant<decimal>(),
                Bid = token["bidPrice"].ConvertInvariant<decimal>(),
                Last = token["lastPrice"].ConvertInvariant<decimal>(),
                Volume = new ExchangeVolume
                {
                    PriceAmount = token["volume"].ConvertInvariant<decimal>(),
                    PriceSymbol = symbol,
                    QuantityAmount = token["quoteVolume"].ConvertInvariant<decimal>(),
                    QuantitySymbol = symbol,
                    Timestamp = CryptoUtility.UnixTimeStampToDateTimeMilliseconds(token["closeTime"].ConvertInvariant<long>())
                }
            };
        }
        #endregion
    }
}
