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
            { new Tuple<string, string>(".NET CLR Memory", "# Gen 0 Collections"), "DotNetGen0Collections" },
            { new Tuple<string, string>(".NET CLR Memory", "# Gen 1 Collections"), "DotNetGen1Collections" },
            { new Tuple<string, string>(".NET CLR Memory", "# Gen 2 Collections"), "DotNetGen2Collections" },
            { new Tuple<string, string>(".NET CLR Memory", "Gen 0 heap size"), "DotNetGen0HeapSize" },
            { new Tuple<string, string>(".NET CLR Memory", "Gen 1 heap size"), "DotNetGen1HeapSize" },
            { new Tuple<string, string>(".NET CLR Memory", "Gen 2 heap size"), "DotNetGen2HeapSize" },
            { new Tuple<string, string>(".NET CLR Memory", "% Time in GC"), "DotNetCpuTimeInGc" },
            { new Tuple<string, string>(".NET CLR Exceptions", "# of Exceps Thrown"), "DotNetExceptionsThrown" },
            { new Tuple<string, string>(".NET CLR Exceptions", "# of Exceps Thrown / sec"), "DotNetExceptionsThrownPerSec" },
        };

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static int reloadCount = TheNumberOfExecutionsBeforeReload;
        private static IList<Tuple<int, PerformanceCounter[], IEnumerable<AppInfo>>> serviceCounters;

        private readonly ISharedInfoAboutApps sharedAppsInfo;
        private readonly IMusketeerConnectorFactory castleConnectorFactory;

        public ServiceMonitorJob(ISharedInfoAboutApps sharedAppsInfo, IMusketeerConnectorFactory castleConnectorFactory)
        {
            this.sharedAppsInfo = sharedAppsInfo;
            this.castleConnectorFactory = castleConnectorFactory;
        }

        public void Execute(IJobExecutionContext context)
        {
            if (reloadCount++ >= TheNumberOfExecutionsBeforeReload) {
                reloadCount = 0;
                Reinitialize();
            }
            var snapshots = new List<LogRecord>(serviceCounters.Count);

            // collect snapshots
            foreach (var svccnt in serviceCounters) {
                var perfData = new Dictionary<string, float>();
                foreach (var perfCounter in svccnt.Item2) {
                    try {
                        // for CPU counter we need to divide the value by the number of cores
                        string friendlyName;
                        if (perfCountersWithFriendlyNames.TryGetValue(new Tuple<string, string>(perfCounter.CategoryName, 
                            perfCounter.CounterName), out friendlyName)) {
                            if (friendlyName.Equals("CPU", StringComparison.Ordinal)) {
                                perfData.Add(friendlyName, perfCounter.NextValue() / Environment.ProcessorCount);
                                continue;
                            }
                            perfData.Add(friendlyName, perfCounter.NextValue());
                        }
                    } catch (InvalidOperationException ex) {
                        // this unfortunately happens quite frequently
                        logger.Info("Performance counter " + GetPerfCounterPath(perfCounter) + " didn't send any value.", ex);
                    }
                }

                foreach (var app in svccnt.Item3) {
                    var snapshot = new LogRecord {
                        TimeUtc = DateTime.UtcNow,
                        ApplicationPath = app.Path,
                        LoggerName = "PerfCounter",
                        LogLevel = LogRecord.ELogLevel.Trace,
                        ProcessId = svccnt.Item1,
                        Server = SharedInfoAboutApps.MachineName,
                        PerformanceData = perfData
                    };
                    snapshots.Add(snapshot);
                }
            }

            if (snapshots.Count > 0) {
                // FIXME call asynchronously graphite (if configured at the same time)
                using (var castleConnector = castleConnectorFactory.GetConnector()) {
                    castleConnector.SendLogRecords(snapshots);
                }
            }
        }

        private void Reinitialize()
        {
            var counters = new List<Tuple<int, PerformanceCounter[], IEnumerable<AppInfo>>>();

            // load the list of PIDs from the shared info about apps and get
            // the corresponding perf counters
            var pids = MatchProcessPidsWithCounterInstances("Process", "ID Process");
            var managedPids = MatchProcessPidsWithCounterInstances(".NET CLR Memory", "Process ID");

            var processIds = sharedAppsInfo.GetProcessIds();
            foreach (var pid in processIds) {
                var perfCounters = new List<PerformanceCounter>(perfCountersWithFriendlyNames.Count);
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
                    counters.Add(new Tuple<int, PerformanceCounter[], IEnumerable<AppInfo>>(pid, 
                        perfCounters.ToArray(), sharedAppsInfo.FindAppsByProcessId(pid)));
                }
            }
            // close previous counters
            if (serviceCounters != null) {
                foreach (var sc in serviceCounters) {
                    foreach (var c in sc.Item2) { c.Close(); }
                }
            }
            // new counters now become valid
            serviceCounters = counters;
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
