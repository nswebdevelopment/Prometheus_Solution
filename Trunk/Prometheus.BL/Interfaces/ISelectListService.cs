using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.BL.Interfaces
{
    public interface ISelectListService
    {
        List<T> GetList<T>(long userProfileId);
        List<T> GetMappedList<T>(long userProfileId, List<int> typeItems);
        List<T> GetPropertiesList<T>(long userProfileId);
    }
}
