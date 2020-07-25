using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prometheus.Common.Enums;

namespace Prometheus.Common
{
    public class Response<T> : IResponse<T>
    {
        public T Value { get; set; }
        public string Message { get; set; }
        public StatusEnum Status { get; set; }
    }

    public class NoValue
    {

    }
}
