using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Media.SocialMedia
{
    public static class FacebookSettings
    {
        public static string BaseAddress = "https://graph.facebook.com/v2.12/";
        public static string RequestHeader = "application/json";
        public static string AccessToken = "EAAcnx1IMg88BAHql1ucgsdZAMnvbhHVqL1vONiMoZC8VcMx7yJnAZAFq2xcTipPQPvdCZC4C8y0BTVuaEVVgSLLJDH5UXWaXL8qFbxJl3LAZBnje8ZAlOS75Nplre2tKZCNHtTvVfa2JQadJqtJh4uco6IwHQbYW0X7grhpMwPVwgZDZD";

        public static string FacebookEndpointBuilder(this string endpoint)
        {
            return $"{endpoint}?access_token={AccessToken}";
        }
    }
}
