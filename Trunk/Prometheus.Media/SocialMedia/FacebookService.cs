using Prometheus.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus.Media.SocialMedia
{
    public class FacebookService : IFacebookService
    {
        private readonly IFacebookClient _client;
        public FacebookService(IFacebookClient client)
        {
            _client = client;
        }

        public async Task<IEnumerable<int>> SocialMediaReport()
        {
            var coins = Enum.GetNames(typeof(Cryptocurrency));
            var fbPages = new FacebookPages();

            Dictionary<string, int> mentions = new Dictionary<string, int>();

            foreach (var coin in coins)
            {
                mentions[coin.ToLower()] = 0;
            }

            foreach (var page in fbPages.FacebookPagesList)
            {
                var result = await _client.GetResponse<FacebookResponsePageFeeds>(page);
                if (result != null)
                {
                    var feeds = result.data.ToList();
                    foreach (var feed in feeds)
                    {
                        if (feed.message != null)
                        {
                            var words = feed.message.ToLower().Split();

                            foreach (var word in words)
                            {
                                if (mentions.ContainsKey(word))
                                {
                                    mentions[word] += 1;
                                }
                            }
                        }
                    }
                }


            }
            var list = mentions.Values.ToList();
            return mentions.Values.ToList();
        }

    }
}
