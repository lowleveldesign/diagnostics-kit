/**
 *  Part of the Diagnostics Kit
 *
 *  Copyright (C) 2016  Sebastian Solnica
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 */

using LowLevelDesign.Diagnostics.Musketeer.Config;
using LowLevelDesign.Diagnostics.Musketeer.Connectors;
using LowLevelDesign.Diagnostics.Musketeer.Jobs;
using NLog;
using Quartz;
using Quartz.Impl;
using SimpleInjector;
using System;
using System.Diagnostics;
using System.Linq;
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
            container.Register<IMusketeerConnectorFactory, MusketeerConnectorFactory>(Lifestyle.Singleton);
            foreach (var jobType in typeof(MusketeerService).Assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IJob)))) {
                container.Register(jobType, jobType, Lifestyle.Singleton);
            }

            container.Verify();

            sched = StdSchedulerFactory.GetDefaultScheduler();
            sched.JobFactory = new SimpleInjectorJobFactory(container);

            /* schedule all the jobs */
            // the configuration should be loaded just after the service is started
            ScheduleJob<ServerConfigRefreshJob>(TriggerBuilder.Create().StartNow().WithSimpleSchedule(
                x => x.WithIntervalInMinutes(30).RepeatForever()).Build());

            ScheduleJob<ServiceMonitorJob>(TriggerBuilder.Create().WithCronSchedule(
                MusketeerConfiguration.PerformanceMonitorJobCron).Build());

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
            if (string.Equals(cronExp, "once", StringComparison.OrdinalIgnoreCase)) {
                ScheduleJob<T>(TriggerBuilder.Create().WithSimpleSchedule().Build());
                return;
            }
            ScheduleJob<T>(TriggerBuilder.Create().WithCronSchedule(cronExp).Build());
        }

        private void ScheduleJob<T>(ITrigger trigger) where T : IJob
        {
            var job = JobBuilder.Create<T>().WithIdentity(typeof(T).Name + "-" + trigger.Key).Build();

            var startdt = sched.ScheduleJob(job, trigger);

            logger.Info("{0} has been scheduled to run at: {1}", job.Key.Name, startdt);
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

    static class Program
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
