using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.BL.Interfaces
{
    public interface IAdapterService
    {
        void AdapterSeed(long userProfileId);
    }
}
