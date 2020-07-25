using System.Collections.Generic;
using System.Threading.Tasks;

namespace Prometheus.Media.SocialMedia
{
    public interface IFacebookService
    {
        Task<IEnumerable<int>> SocialMediaReport();
    }
}