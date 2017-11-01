using System;

namespace Centipede.Objects
{
    public class CrawlResult
    {
        public CrawlResult(string url, string body, DateTime crawledAt, Guid crawlJobGuid)
        {
            Url = url;
            Body = body;
            CrawledAt = crawledAt;
            CrawlJobGuid = crawlJobGuid;
        }

        public string Url { get; set; }
        public string Body { get; set; }
        public DateTime CrawledAt { get; set; }
        public Guid CrawlJobGuid { get; set; }
    }
}