using System;
using System.Collections.Generic;
using Centipede.Objects.Enums;

namespace Centipede.Objects
{
    public class CrawlJob
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CrawlJob" /> class.
        /// </summary>
        /// <param name="url">The web URL.</param>
        /// <param name="maxExecutions">The maximum executions.</param>
        /// <param name="delay">The delay.</param>
        /// <exception cref="System.Exception">URL required for Job.</exception>
        public CrawlJob(string url, string parentUrl = "", int priority = 0, int maxExecutions = 3, int delay = 100)
        {
            if (string.IsNullOrEmpty(url)) throw new Exception("URL required for Job.");
            Guid = Guid.NewGuid();
            State = JobState.Submitted;
            Url = url;
            ParentUrl = parentUrl;
            Priority = priority;
            MaxExecutions = maxExecutions;
            TotalExecutions = 0;
            Delay = delay;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CrawlJob" /> class.
        /// </summary>
        public CrawlJob()
        {
        }

        /// <summary>
        ///     Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        ///     The unique identifier.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        ///     Gets or sets the maximum executions.
        /// </summary>
        /// <value>
        ///     The maximum executions.
        /// </value>
        public int MaxExecutions { get; set; }

        /// <summary>
        ///     Gets or sets the total executions.
        /// </summary>
        /// <value>
        ///     The total executions.
        /// </value>
        public int TotalExecutions { get; set; }

        /// <summary>
        ///     Gets or sets the total duration of job execution including delay.
        /// </summary>
        /// <value>
        ///     The total duration of the execution.
        /// </value>
        public int TotalExecutionDuration { get; set; }

        /// <summary>
        ///     Gets or sets the duration of request execution excluding delay.
        /// </summary>
        /// <value>
        ///     The duration of the request execution.
        /// </value>
        public int RequestExecutionDuration => TotalExecutionDuration - Delay;

        /// <summary>
        ///     Gets or sets the state.
        /// </summary>
        /// <value>
        ///     The state.
        /// </value>
        public JobState State { get; set; }

        /// <summary>
        ///     Gets or sets the delay.
        /// </summary>
        /// <value>
        ///     The delay.
        /// </value>
        public int Delay { get; set; }

        /// <summary>
        ///     Gets or sets the web URL.
        /// </summary>
        /// <value>
        ///     The web URL.
        /// </value>
        public string Url { get; set; }


        /// <summary>
        ///     Gets or sets the parent URL.
        /// </summary>
        /// <value>
        ///     The parent URL.
        /// </value>
        public string ParentUrl { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is parent.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is parent; otherwise, <c>false</c>.
        /// </value>
        public bool IsParent => string.IsNullOrEmpty(ParentUrl);

        /// <summary>
        ///     Gets or sets the priority of the job. Higher priority jobs are executed first.
        /// </summary>
        /// <value>
        ///     The priority.
        /// </value>
        public int Priority { get; set; }


        /// <summary>
        ///     URLs that were discovered during the crawl job.
        /// </summary>
        /// <value>
        ///     The urls.
        /// </value>
        public List<string> Urls { get; set; }
    }
}