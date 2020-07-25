using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Media.SocialMedia
{
    class FacebookResponsePageFeeds
    {
        public Feed[] data { get; set; }
        public Paging paging { get; set; }
    }

    public class Paging
    {
        public Cursors cursors { get; set; }
        public string next { get; set; }
    }

    public class Cursors
    {
        public string before { get; set; }
        public string after { get; set; }
    }

    public class Feed
    {
        public DateTime created_time { get; set; }
        public string message { get; set; }
        public string id { get; set; }
    }
}
