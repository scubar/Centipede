using System;
using System.Threading;
using Centipede.Objects;
using NLog;
using Topshelf;

namespace Centipede.Scheduler
{
    public class Program
    {
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            var redisDriver = new RedisDriver();

            // Check/Prepare external service dependencies (Databases, Services, etc)
            // before proceeding with service start.
            try
            {
                Logger.Debug("Testing external dependencies.");

                Logger.Debug("Testing redis connectivity");
                redisDriver.Ping();
                Logger.Debug("redis connected succesfully!");

                Logger.Info("External resources available, proceeding with service start.");
            }
            catch (Exception ex)
            {
                Logger.Fatal("Error during external dependency check.");
                Logger.Fatal(ex.Message);

                Logger.Warn("Exiting due to external dependency failure!");
                Thread.Sleep(1000);
                Environment.Exit(0);
            }

            // Construct the Service
            HostFactory.Run(x =>
            {
                // Main Service Configuration
                x.Service<SchedulerService>(sc =>
                {
                    sc.ConstructUsing(name => new SchedulerService(redisDriver));
                    sc.WhenStarted(service => service.Start());
                    sc.WhenStopped(service => service.Stop());
                });

                // Name and Description
                x.SetDescription("Schedules Web Crawling operations and manages Centipede.Worker instances.");
                x.SetDisplayName("Centipede.Scheduler");
                x.SetServiceName("Centipede.Scheduler");

                // Recovery
                x.EnableServiceRecovery(r =>
                {
                    r.RestartService(0);
                    r.RestartService(0);
                    r.RestartService(0);
                    r.OnCrashOnly();
                    r.SetResetPeriod(1);
                });

                // RunAs
                x.RunAsLocalSystem();

                // Logging
                x.UseNLog();
            });
        }
    }
}