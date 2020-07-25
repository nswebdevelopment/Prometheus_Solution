using Microsoft.Extensions.Options;
using Prometheus.Common.Enums;
using Prometheus.Common.Extensiosns;
using Prometheus.Model;
using Prometheus.Model.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Prometheus.Media
{
    public class RssMedia : IRssMedia
    {
        public RssMedia(IOptions<RSSUrlSeed> rss, ILogger logger)
        {
            _rss = rss;
            _logger = logger;
        }

        private readonly ILogger _logger;
        private readonly IOptions<RSSUrlSeed> _rss;

        public IEnumerable<int> MediaReport()
        {
            var coins = Enum.GetNames(typeof(Cryptocurrency));
            var mentions = new int[coins.Length];

            foreach (var url in _rss.Value.RssUrl)
            {
                try
                {
                    var reader = XmlReader.Create(url);
                    var feed = SyndicationFeed.Load(reader);
                    reader.Close();

                    for (int index = 0; index < coins.Length; index++)
                    {
                        var items = feed.Items.Where(i => (i.PublishDate.Date == DateTimeOffset.UtcNow.Date.AddDays(-1) || i.LastUpdatedTime.Date == DateTimeOffset.UtcNow.Date.AddDays(-1)) && i.Title.Text.ToLower().ExactMatch(coins[index].ToLower())).ToList();

                        mentions[index] += items.Count;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Information($"RssMedia.MediaReport(url: {url})");
                    _logger.Warning(ex.Message);
                }
            }

            return mentions.ToList();
        }
    }
}
