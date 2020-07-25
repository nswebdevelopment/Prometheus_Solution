using System.Threading.Tasks;

namespace Prometheus.Media.SocialMedia
{
    public interface IFacebookClient
    {
        Task<T> GetResponse<T>(string facebookEndpoint);
    }
}