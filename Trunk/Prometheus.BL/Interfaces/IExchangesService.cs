using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prometheus.Common.Enums;
using Prometheus.Model.Models.ExchangesModel;

namespace Prometheus.BL.Interfaces
{
    public interface IExchangesService
    {
        List<BasicInfoModel> GetAll();
        ExtendedInfoModel GetDetails(AvailableExchanges name);        
        bool SaveSymbolsForExchange(AvailableExchanges name);
        bool DeleteAllSymbolsForExchange(AvailableExchanges name);
        List<TickerModel> GetOverview(AvailableExchanges exchangeName, string market);        
    }
}
