using System;
using System.Collections.Generic;
using System.Net;
using System.Timers;
using Centipede.Objects;
using Centipede.Objects.Enums;
using Centipede.Objects.Interfaces;
using Newtonsoft.Json;
using NLog;
using RestSharp;
using StackExchange.Redis;

namespace Centipede.Worker
{
    internal class WorkerService
    {
        private readonly Timer _workerTimer;

        internal WorkerService(IDatastore datastore)
        {
            _workerTimer = new Timer(500) {AutoReset = true};
            _workerTimer.Elapsed += Worker;
            Datastore = datastore;
            Logger.Debug($"Centipede.Worker {WorkerGuid} started!");
        }

        private IDatastore Datastore { get; }
        private Guid WorkerGuid { get; } = Guid.NewGuid();
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        internal void Worker(object sender, EventArgs e)
        {
            ScheduledJobHandler();
        }

        /// <summary>
        ///     Gets the next Crawl Job to be executed from the Scheduled Jobs list, executes it, and adds it to the Executed Jobs
        ///     list.
        /// </summary>
        private void ScheduledJobHandler()
        {
            var value = GetCrawlJob();

            if (string.IsNullOrEmpty(value)) return;

            var crawlJob = JsonConvert.DeserializeObject<CrawlJob>(value);

            if (crawlJob.TotalExecutions == crawlJob.MaxExecutions && crawlJob.State != JobState.Error)
                crawlJob.State = JobState.Error;
            else
                crawlJob.TotalExecutions++;

            if (crawlJob.State != JobState.Completed && crawlJob.State != JobState.Error)
            {
                var crawlResult = Crawler.Crawl(crawlJob);
                Logger.Debug(
                    $"Executed Crawl Job {crawlJob.Guid} in {crawlJob.RequestExecutionDuration}. Job State: {crawlJob.State}");

                if (crawlResult != null)
                    IndexCrawlJobResult(crawlResult);
            }

            UpdateCrawlJob(crawlJob);
        }

        /// <summary>
        ///     Indexes the crawl job result in Solr.
        /// </summary>
        /// <param name="crawlResult">The crawl result.</param>
        private static void IndexCrawlJobResult(CrawlResult crawlResult)
        {
            var client = new RestClient("http://172.16.1.201:8983");
            var request =
                new RestRequest("/solr/centipede/update?commit=true", Method.POST) {RequestFormat = DataFormat.Json};
            request.AddBody(new List<CrawlResult> {crawlResult});

            var response = client.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
                Logger.Debug($"Crawl Result for Crawl Job {crawlResult.CrawlJobGuid} indexed succesfully.");
        }

        /// <summary>
        ///     LPushes the crawl job onto the executed jobs list.
        /// </summary>
        /// <param name="crawlJob">The crawl job.</param>
        private void UpdateCrawlJob(CrawlJob crawlJob)
        {
            Datastore.ListLeftPush("centipede:executedjobs", JsonConvert.SerializeObject(crawlJob));
        }

        /// <summary>
        ///     Gets the next available crawl job.
        /// </summary>
        /// <returns></returns>
        private RedisValue GetCrawlJob()
        {
            var value = Datastore.ListRightPop("centipede:scheduledjobs");
            return value;
        }

        internal void Start()
        {
            _workerTimer.Start();
        }

        internal void Stop()
        {
            _workerTimer.Stop();
        }
    }
}