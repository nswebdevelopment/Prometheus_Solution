using System;
using System.Collections.Generic;
using System.Text;

namespace Prometheus.ExchangesApi.ExchangeClients.Interfaces
{
    public interface IBaseExchangeClient
    {
        IEnumerable<string> GetSymbols();
    }
}
