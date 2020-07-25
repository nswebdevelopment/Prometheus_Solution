using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Prometheus.Media.SocialMedia
{
    public class FacebookClient : IFacebookClient
    {
        private readonly HttpClient _facebookClient;

        public FacebookClient()
        {
            var httpClient = new HttpClient();

            httpClient.BaseAddress = new Uri(FacebookSettings.BaseAddress);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(FacebookSettings.RequestHeader));

            _facebookClient = httpClient;
        }

        public async Task<T> GetResponse<T>(string facebookEndpoint)
        {
            var httpResponse = await _facebookClient.GetAsync(facebookEndpoint.FacebookEndpointBuilder());
            if (!httpResponse.IsSuccessStatusCode)
                return default(T);

            var result = await httpResponse.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(result);
        }
    }
}
