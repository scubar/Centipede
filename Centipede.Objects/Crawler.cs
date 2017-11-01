using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Centipede.Objects.Enums;
using HtmlAgilityPack;
using NLog;

namespace Centipede.Objects
{
    public static class Crawler
    {
        public static Logger Logger = LogManager.GetCurrentClassLogger();

        public static CrawlResult Crawl(CrawlJob crawlJob)
        {
            if (crawlJob == null) throw new NullReferenceException();

            try
            {
                var stopwatch = Stopwatch.StartNew();
                if (crawlJob.Delay > 0)
                    Thread.Sleep(crawlJob.Delay);

                var hw = new HtmlWeb();
                var doc = hw.Load(crawlJob.Url);

                crawlJob.Urls = GetBodyUrls(crawlJob, doc);

                var bodyText = GetBodyText(crawlJob, doc);

                crawlJob.TotalExecutionDuration = Convert.ToInt32(stopwatch.ElapsedMilliseconds);

                var crawlResult = new CrawlResult(crawlJob.Url, bodyText, DateTime.UtcNow, crawlJob.Guid);

                crawlJob.State = JobState.Completed;

                return crawlResult;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex.Message);

                // Set Crawl Job to Error state if it exceeds max executions.
                if (crawlJob.TotalExecutions == crawlJob.MaxExecutions)
                    crawlJob.State = JobState.Error;
                // Set Crawl Job to Retry state if it fails but hasn't exceeded max executions.
                else if (crawlJob.TotalExecutions < crawlJob.MaxExecutions)
                    crawlJob.State = JobState.Retry;

                return null;
            }
        }

        private static List<string> GetBodyUrls(CrawlJob crawlJob, HtmlDocument doc)
        {
            var linkedPages = doc.DocumentNode.Descendants("a")
                .Select(a => a.GetAttributeValue("href", null))
                .Where(u => !string.IsNullOrEmpty(u));
            return linkedPages.ToList();
        }

        private static string GetBodyText(CrawlJob crawlJob, HtmlDocument doc)
        {
            var bodyText = "";
            foreach (var node in doc.DocumentNode.SelectNodes("//text()"))
                bodyText += node.InnerText;
            return bodyText;
        }
    }
}