using System;
using System.Linq;
using System.Timers;
using Centipede.Objects;
using Centipede.Objects.Enums;
using Centipede.Objects.Interfaces;
using Newtonsoft.Json;
using NLog;

namespace Centipede.Scheduler
{
    internal class SchedulerService
    {
        private readonly Timer _schedulerTimer;

        public SchedulerService(IDatastore datastore)
        {
            _schedulerTimer = new Timer(500) {AutoReset = true};
            _schedulerTimer.Elapsed += SchedulerHandler;
            Datastore = datastore;
            Logger.Debug($"Centipede.Scheduler {SchedulerGuid} started!");
        }

        private IDatastore Datastore { get; }
        private Guid SchedulerGuid { get; } = Guid.NewGuid();
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        internal void SchedulerHandler(object sender, EventArgs e)
        {
            SubmittedJobHandler();

            ExcecutedJobHandler();
        }

        /// <summary>
        ///     Checks the submitted jobs list for new jobs that may have been submitted via the API.
        /// </summary>
        private void SubmittedJobHandler()
        {
            var value = Datastore.ListRightPop($"centipede:submittedjobs");

            if (string.IsNullOrEmpty(value)) return;

            var crawlJob = JsonConvert.DeserializeObject<CrawlJob>(value);

            ScheduleCrawlJob(crawlJob);
        }

        /// <summary>
        ///     Checks the executed jobs list for any jobs that have been finished by workers.
        ///     Reschedules jobs that were added to the executed jobs list without being completed or erroring out.
        /// </summary>
        private void ExcecutedJobHandler()
        {
            var value = Datastore.ListRightPop($"centipede:executedjobs");

            if (string.IsNullOrEmpty(value)) return;

            var crawlJob = JsonConvert.DeserializeObject<CrawlJob>(value);

            // Jobs in the executed list that haven't been completed are pushed back to the scheduled jobs list.
            if (crawlJob.State != JobState.Completed && crawlJob.State != JobState.Error)
            {
                ScheduleCrawlJob(crawlJob, false);
                return;
            }

            // Jobs that have are in an Error state are removed.
            if (crawlJob.State == JobState.Error)
                Logger.Debug($"Crawl Job ({crawlJob.Guid} finished with an Error.");


            if (crawlJob.Urls.Any())
                foreach (var url in crawlJob.Urls)
                    if (url.Contains(crawlJob.Url))
                    {
                        var parentUrl = "";
                        if (crawlJob.IsParent)
                            parentUrl = crawlJob.Url;

                        ScheduleCrawlJob(new CrawlJob(url, parentUrl, 0, 2, 1000));
                    }
        }

        /// <summary>
        ///     Schedules a Crawl Job by LPUSHing it into the scheduled jobs list.
        /// </summary>
        /// <param name="crawlJob">The crawl job to be scheduled.</param>
        /// <param name="unique">Whether the crawl jb</param>
        private void ScheduleCrawlJob(CrawlJob crawlJob, bool unique = true)
        {
            // If the key is in the url keyspace, it has already been crawled during this crawl timespan.
            if (unique)
                if (Datastore.KeyExists($"centipede:urls:{crawlJob.Url}")) return;

            // Add the crawl job to the scheduled jobs list.
            Datastore.ListLeftPush($"centipede:scheduledjobs", JsonConvert.SerializeObject(crawlJob));

            // Add an entry to the URLs keyspace with scheduled expiry.
            Datastore.StringSet($"centipede:urls:{crawlJob.Url}", crawlJob.Guid.ToString(), TimeSpan.FromSeconds(7200));

            Logger.Debug($"Scheduled Crawl Job {crawlJob.Guid} for URL '{crawlJob.Url}'");
        }

        internal void Start()
        {
            _schedulerTimer.Start();
        }

        internal void Stop()
        {
            _schedulerTimer.Stop();
        }
    }
}