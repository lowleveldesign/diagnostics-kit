using LowLevelDesign.Diagnostics.Musketeer.Config;
using LowLevelDesign.Diagnostics.Musketeer.Jobs;
using NLog;
using Quartz;
using Quartz.Impl;
using SimpleInjector;
using System;
using System.Diagnostics;
using Topshelf;

namespace LowLevelDesign.Diagnostics.Musketeer
{
    sealed class MusketeerService : ServiceControl
    {
        public readonly static string ServiceName = typeof(MusketeerService).Namespace;
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private IScheduler sched;

        public bool Start(HostControl hostControl)
        {
            logger.Info("Starting service...");

            logger.Info("Starting scheduler...");

            Container container = new Container();

            // shared class registration
            container.Register<ISharedInfoAboutApps, SharedInfoAboutApps>(Lifestyle.Singleton);
            container.Register<IMusketeerHttpCastleConnectorFactory, MusketeerHttpCastleConnectorFactory>(Lifestyle.Singleton);

            container.Verify();

            sched = StdSchedulerFactory.GetDefaultScheduler();
            sched.JobFactory = new SimpleInjectorJobFactory(container); 

            /* schedule all the jobs */
            // the configuration should be loaded just after the service is started
            ScheduleCronJob<ServerConfigRefreshJob>("once");
            ScheduleCronJob<ServerConfigRefreshJob>(MusketeerConfiguration.IISConfigurationRefreshCron);

            ScheduleCronJob<ServiceMonitorJob>(MusketeerConfiguration.PerformanceMonitorJobCron);

            ScheduleCronJob<ReadWebAppsLogsJob>(MusketeerConfiguration.IISLogsReadCron);

            ScheduleCronJob<MusketeerUpdateJob>(MusketeerConfiguration.CheckUpdateCron);

            sched.Start();

            logger.Info("Service succesfully started");

            return true;
        }

        private void ScheduleCronJob<T>(string cronExp) where T : IJob
        {
            if (string.Equals(cronExp, "never", StringComparison.OrdinalIgnoreCase))
                return;
            var job = JobBuilder.Create<T>().WithIdentity(typeof(T).Name + "-" + cronExp).Build();

            ITrigger trigger;
            if (string.Equals(cronExp, "once", StringComparison.OrdinalIgnoreCase))
                trigger = TriggerBuilder.Create().WithSimpleSchedule().Build();
            else
                trigger = TriggerBuilder.Create().WithCronSchedule(cronExp).Build();

            var startdt = sched.ScheduleJob(job, trigger);

            logger.Info("{0} has been scheduled to run at: {1} and repeat based on expression: {2}",
                    job.Key.Name, startdt, cronExp);
        }

        public bool Stop(HostControl hostControl)
        {
            logger.Info("Stopping service...");

            // stop all running jobs
            if (sched != null)
            {
                sched.Shutdown(true);
            }
            // finally close opened streams for the logs reader
            ReadWebAppsLogsJob.CleanupStreams();

            return true;
        }
    }

    class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static void Main()
        {
            HostFactory.Run(hc => {
                hc.BeforeInstall(() => {
                    if (EventLog.SourceExists("Musketeer")) {
                        EventLog.DeleteEventSource("Musketeer");
                    }
                    EventLog.CreateEventSource("Musketeer", "LowLevelDesign-Diagnostics");
                });
                hc.AfterUninstall(() => {
                    EventLog.DeleteEventSource("Musketeer");
                    EventLog.Delete("LowLevelDesign-Diagnostics");
                });

                hc.UseNLog();
                hc.DependsOnEventLog();
                // service is constructed using its default constructor
                hc.Service<MusketeerService>();
                // sets service properties
                hc.SetServiceName(MusketeerService.ServiceName);
                hc.SetDisplayName(typeof(MusketeerService).Namespace);
                hc.SetDescription("Musketeer service - a part of LowLevelDesign DiagnosticsKit.");
            });
        }

    }
}
