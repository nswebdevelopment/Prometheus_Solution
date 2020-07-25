using Prometheus.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Common
{
    public interface IResponse<T>
    {
        T Value { get; set; }
        StatusEnum Status { get; set; }
        string Message { get; set; }
    }
}
