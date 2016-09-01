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

using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.Musketeer.Config;
using LowLevelDesign.Diagnostics.Musketeer.Connectors;
using LowLevelDesign.Diagnostics.Musketeer.Models;
using NLog;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LowLevelDesign.Diagnostics.Musketeer.Jobs
{
    [DisallowConcurrentExecution]
    public sealed class ServiceMonitorJob : IJob
    {
        private const int TheNumberOfExecutionsBeforeReload = 10;

        private static readonly IDictionary<Tuple<string, string>, string> perfCountersWithFriendlyNames = new Dictionary<Tuple<string, string>, string> {
            { new Tuple<string, string>("Process", "% Processor Time"), "CPU" },
            { new Tuple<string, string>("Process", "Working Set"), "Memory" },
            { new Tuple<string, string>("Process", "IO Read Bytes/sec"), "IOReadBytesPerSec" },
            { new Tuple<string, string>("Process", "IO Write Bytes/sec"), "IOWriteBytesPerSec" },
            { new Tuple<string, string>(".NET CLR Memory", "# Gen 0 Collections"), "DotNetGen0Collections" },
            { new Tuple<string, string>(".NET CLR Memory", "# Gen 1 Collections"), "DotNetGen1Collections" },
            { new Tuple<string, string>(".NET CLR Memory", "# Gen 2 Collections"), "DotNetGen2Collections" },
            { new Tuple<string, string>(".NET CLR Memory", "Gen 0 heap size"), "DotNetGen0HeapSize" },
            { new Tuple<string, string>(".NET CLR Memory", "Gen 1 heap size"), "DotNetGen1HeapSize" },
            { new Tuple<string, string>(".NET CLR Memory", "Gen 2 heap size"), "DotNetGen2HeapSize" },
            { new Tuple<string, string>(".NET CLR Memory", "% Time in GC"), "DotNetCpuTimeInGc" },
            { new Tuple<string, string>(".NET CLR Exceptions", "# of Exceps Thrown"), "DotNetExceptionsThrown" },
            { new Tuple<string, string>(".NET CLR Exceptions", "# of Exceps Thrown / sec"), "DotNetExceptionsThrownPerSec" },
            { new Tuple<string, string>("ASP.NET Applications", "Errors Total"), "AspNetErrorsTotal" },
            { new Tuple<string, string>("ASP.NET Applications", "Requests Executing"), "AspNetRequestExecuting" },
            { new Tuple<string, string>("ASP.NET Applications", "Requests Failed"), "AspNetRequestsFailed" },
            { new Tuple<string, string>("ASP.NET Applications", "Requests Not Found"), "AspNetRequestsNotFound" },
            { new Tuple<string, string>("ASP.NET Applications", "Requests Not Authorized"), "AspNetRequestsNotAuthorized" },
            { new Tuple<string, string>("ASP.NET Applications", "Requests In Application Queue"), "AspNetRequestsInApplicationQueue" },
            { new Tuple<string, string>("ASP.NET Applications", "Requests Timed Out"), "AspNetRequestsTimedOut" },
            { new Tuple<string, string>("ASP.NET Applications", "Requests Total"), "AspNetRequestsTotal" },
            { new Tuple<string, string>("ASP.NET Applications", "Requests/Sec"), "AspNetRequestsPerSec" },
            { new Tuple<string, string>("ASP.NET Applications", "Request Execution Time"), "AspNetRequestExecutionTime" },
            { new Tuple<string, string>("ASP.NET Applications", "Request Wait Time"), "AspNetRequestWaitTime" }
        };

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static int reloadCount = TheNumberOfExecutionsBeforeReload;
        private static Dictionary<int, PerformanceCounter[]> processCounters;
        private static Dictionary<string, PerformanceCounter[]> applicationCounters;

        private readonly ISharedInfoAboutApps sharedAppsInfo;
        private readonly IMusketeerConnector connector;

        public ServiceMonitorJob(ISharedInfoAboutApps sharedAppsInfo, IMusketeerConnectorFactory connectorFactory)
        {
            this.sharedAppsInfo = sharedAppsInfo;
            connector = connectorFactory.CreateConnector();
        }

        private struct PerformanceCounterSnapshot
        {
            public string Name;

            public float Value;
        }

        public void Execute(IJobExecutionContext context)
        {
            if (reloadCount++ >= TheNumberOfExecutionsBeforeReload) {
                reloadCount = 0;
                ReinitializeProcessCounters();
                ReinitializeApplicationCounters();
            }

            var processCountersSnapshots = CollectProcessCountersSnapshots();
            var apps = sharedAppsInfo.GetApps();
            var logs = new List<LogRecord>(apps.Count);

            foreach (var app in apps) {
                var perfData = new Dictionary<string, float>();

                foreach (var pid in app.ProcessIds) {
                    PerformanceCounterSnapshot[] snapshots;
                    if (processCountersSnapshots.TryGetValue(pid, out snapshots)) {
                        foreach (var snapshot in snapshots) {
                            perfData.Add(snapshot.Name, snapshot.Value);
                        }
                    }
                }

                // collect application performance counters
                PerformanceCounter[] perfCounters;
                if (applicationCounters.TryGetValue(app.Path, out perfCounters)) {
                    foreach (var perfCounter in perfCounters) {
                        var snapshot = CollectPerformanceCounterSnapshot(perfCounter);
                        perfData.Add(snapshot.Name, snapshot.Value);
                    }
                }

                var log = new LogRecord {
                    TimeUtc = DateTime.UtcNow,
                    ApplicationPath = app.Path,
                    LoggerName = GetLoggerName(app),
                    LogLevel = LogRecord.ELogLevel.Trace,
                    ProcessId = app.ProcessIds.FirstOrDefault(), // TODO: there might be few PIDs assigned to the same app - for now I leave it
                    Server = SharedInfoAboutApps.MachineName,
                    PerformanceData = perfData
                };
                logs.Add(log);
            }

            if (logs.Count > 0) {
                connector.SendLogRecords(logs);
            }
        }

        private Dictionary<int, PerformanceCounterSnapshot[]> CollectProcessCountersSnapshots()
        {
            var result = new Dictionary<int, PerformanceCounterSnapshot[]>();
            // collect process counters snapshots
            foreach (var proccnt in processCounters) {
                var data = new PerformanceCounterSnapshot[proccnt.Value.Length];
                for (int i = 0; i < proccnt.Value.Length; i++) {
                    data[i] = CollectPerformanceCounterSnapshot(proccnt.Value[i]);
                }
                result.Add(proccnt.Key, data);
            }
            return result;
        }

        private PerformanceCounterSnapshot CollectPerformanceCounterSnapshot(PerformanceCounter perfCounter)
        {
            string friendlyName;
            float value = -1.0f;
            if (perfCountersWithFriendlyNames.TryGetValue(new Tuple<string, string>(perfCounter.CategoryName,
                perfCounter.CounterName), out friendlyName)) {
                try {
                    // for CPU counter we need to divide the value by the number of cores
                    if (friendlyName.Equals("CPU", StringComparison.Ordinal)) {
                        value = perfCounter.NextValue() / Environment.ProcessorCount;
                    } else {
                        value = perfCounter.NextValue();
                    }
                } catch (InvalidOperationException ex) {
                    // this unfortunately happens quite frequently
                    logger.Info("Performance counter " + GetPerfCounterPath(perfCounter) + " didn't send any value.", ex);
                }
            }
            return new PerformanceCounterSnapshot { Name = friendlyName ?? perfCounter.CounterName, Value = value };
        }

        private string GetLoggerName(AppInfo app)
        {
            if (app.ApplicationType == EAppType.WebApplication) {
                return "PerfCounter.WebApp";
            }
            return "PerfCounter.WinSvc";
        }

        private void ReinitializeProcessCounters()
        {
            var counters = new Dictionary<int, PerformanceCounter[]>();

            // load the list of PIDs from the shared info about apps and get
            // the corresponding perf counters
            var pids = MatchProcessPidsWithCounterInstances("Process", "ID Process");
            var managedPids = MatchProcessPidsWithCounterInstances(".NET CLR Memory", "Process ID");

            var processIds = sharedAppsInfo.GetProcessIds();
            foreach (var pid in processIds) {
                var perfCounters = new List<PerformanceCounter>();
                string inst;
                if (pids.TryGetValue((uint)pid, out inst)) {
                    logger.Debug("Adding performance counters for PID: {0}.", pid);
                    foreach (var counter in perfCountersWithFriendlyNames.Keys.Where(k => k.Item1.Equals("Process", StringComparison.Ordinal))) {
                        perfCounters.Add(new PerformanceCounter(counter.Item1, counter.Item2, inst, true));
                    }
                }
                if (managedPids.TryGetValue((uint)pid, out inst)) {
                    logger.Debug("Adding managed performance counters for PID: {0}.", pid);
                    foreach (var counter in perfCountersWithFriendlyNames.Keys.Where(k => k.Item1.StartsWith(".NET", StringComparison.Ordinal))) {
                        perfCounters.Add(new PerformanceCounter(counter.Item1, counter.Item2, inst, true));
                    }
                }

                if (perfCounters.Count > 0) {
                    counters.Add(pid, perfCounters.ToArray());
                }
            }

            // close previous counters
            if (processCounters != null) {
                foreach (var sc in processCounters.Values) {
                    foreach (var c in sc) {
                        c.Close();
                    }
                }
            }
            processCounters = counters;
        }

        private void ReinitializeApplicationCounters()
        {
            var counters = new Dictionary<string, PerformanceCounter[]>();
            var aspNetPerfCounterInstances = new PerformanceCounterCategory("ASP.NET Applications").GetInstanceNames();

            foreach (var app in sharedAppsInfo.GetApps()) {
                if (app.AppDomains != null) {
                    var perfCounters = new Dictionary<string, PerformanceCounter>();
                    foreach (var appDomain in app.AppDomains) {
                        foreach (var aspNetPerfCounterInstance in aspNetPerfCounterInstances) {
                            if (appDomain.Name.Replace('/', '_').StartsWith(aspNetPerfCounterInstance, StringComparison.Ordinal)) {
                                logger.Debug("Adding ASP.NET performance counters for appdomain: {0}, path: '{1}'", appDomain.Name, app.Path);
                                foreach (var counter in perfCountersWithFriendlyNames.Keys.Where(k => k.Item1.Equals("ASP.NET Applications",
                                    StringComparison.Ordinal))) {
                                    PerformanceCounter prevCounter;
                                    if (perfCounters.TryGetValue(counter.Item2, out prevCounter) 
                                        && prevCounter.InstanceName.Length < aspNetPerfCounterInstance.Length) {
                                        prevCounter.Close();
                                        perfCounters.Remove(counter.Item2);
                                        logger.Debug("Found better ASP.NET performance counter ({0}) for '{1}', instance: '{2}'", counter.Item2,
                                            app.Path, aspNetPerfCounterInstance);
                                    }
                                    logger.Debug("Adding ASP.NET performance counter ({0}) for  '{1}', instance: '{2}'", counter.Item2,
                                        app.Path, aspNetPerfCounterInstance);
                                    perfCounters.Add(counter.Item2, new PerformanceCounter(counter.Item1, counter.Item2, aspNetPerfCounterInstance, true));
                                }
                            }
                        }
                    }

                    if (perfCounters.Count > 0) {
                        counters.Add(app.Path, perfCounters.Values.ToArray());
                    }
                }
            }

            // close previous counters
            if (applicationCounters != null) {
                foreach (var sc in applicationCounters.Values) {
                    foreach (var c in sc) {
                        c.Close();
                    }
                }
            }

            applicationCounters = counters;
        }

        private static Dictionary<uint, string> MatchProcessPidsWithCounterInstances(string categoryName, string counterName)
        {
            var pids = new Dictionary<uint, string>(); // pid <-> instance name
            var perfcat = new PerformanceCounterCategory(categoryName);
            string[] instances = perfcat.GetInstanceNames();

            // now for each instance let's figure out its pid
            foreach (var inst in instances) {
                try {
                    using (var counter = new PerformanceCounter(categoryName, counterName, inst, true)) {
                        var ppid = (uint)counter.NextValue();
                        // _Total and Idle have PID = 0 - we don't need them
                        if (ppid > 0 && !pids.ContainsKey(ppid)) {
                            pids.Add(ppid, inst);
                            logger.Debug("Matched PID: {0} with instance: {1} ({2})", ppid, inst, categoryName);
                        }
                    }
                } catch (InvalidOperationException ex) {
                    logger.Warn(string.Format("Performance counter '{0}' didn't send any value.", inst), ex);
                }
            }

            return pids;
        }

        private static string GetPerfCounterPath(PerformanceCounter cnt)
        {
            return string.Format(@"{0}\{1}\{2}\{3}", cnt.MachineName, cnt.CategoryName, cnt.CounterName, cnt.InstanceName);
        }
    }
}
