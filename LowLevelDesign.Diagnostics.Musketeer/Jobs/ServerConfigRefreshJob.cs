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
using Microsoft.Web.Administration;
using NLog;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;

namespace LowLevelDesign.Diagnostics.Musketeer.Jobs
{
    /// <summary>
    /// Job responsible for reading IIS and Windows Services
    /// configuration. It will collect the data and share it
    /// with the Diagnostics Castle.
    /// </summary>
    [DisallowConcurrentExecution]
    public sealed class ServerConfigRefreshJob : IJob
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();
        private static readonly string serverFqdnOrIp;
        private readonly ISharedInfoAboutApps sharedAppsInfo;
        private readonly IMusketeerConnectorFactory connectorFactory;

        static ServerConfigRefreshJob()
        {
            try {
                var hostNetworkConfiguration = Dns.GetHostEntry(Dns.GetHostName());
                if (string.IsNullOrEmpty(hostNetworkConfiguration.HostName)) {
                    serverFqdnOrIp = hostNetworkConfiguration.AddressList.Length > 0 ?
                       hostNetworkConfiguration.AddressList[0].ToString() : string.Empty;
                } else {
                    serverFqdnOrIp = hostNetworkConfiguration.HostName;
                }
            } catch {
                serverFqdnOrIp = string.Empty;
            }
        }

        public ServerConfigRefreshJob(ISharedInfoAboutApps sharedAppsInfo, IMusketeerConnectorFactory connectorFactory)
        {
            this.sharedAppsInfo = sharedAppsInfo;
            this.connectorFactory = connectorFactory;
        }

        public void Execute(IJobExecutionContext context)
        {
            var configs = new List<ApplicationServerConfig>();
            var appinfo = new Dictionary<string, AppInfo>();

            // load IIS configuration items
            LoadIISAppConfigs(configs, appinfo);
            // load Win services configuration items
            LoadWinServicesConfigs(configs, appinfo);

            using (var connector = connectorFactory.GetConnector()) {
                var activeApps = connector.SupportsApplicationConfigs ?
                    connector.SendApplicationConfigs(configs) : configs.Select(c => c.AppPath).ToArray();

                // store the configs in the shared storage
                var map = new Dictionary<string, AppInfo>(activeApps.Length, StringComparer.Ordinal);
                foreach (var path in activeApps) {
                    if (map.ContainsKey(path)) {
                        logger.Warn("Duplicate application path found: '{0}' - it will appear once in the log.", path);
                        continue;
                    }
                    AppInfo ai;
                    if (appinfo.TryGetValue(path, out ai)) {
                        logger.Debug("Application '{0}' will be monitored by the Musketeer.", path);
                        map.Add(path, ai);
                    }
                }
                sharedAppsInfo.UpdateAppWorkerProcessesMap(map);
            }
        }

        private void LoadIISAppConfigs(IList<ApplicationServerConfig> configs, IDictionary<string, AppInfo> appinfo)
        {
            Debug.Assert(appinfo != null);
            Debug.Assert(configs != null);

            ServerManager mgr = null;
            try { 
                mgr = ServerManager.OpenRemote("localhost");
                var poolToApps = new Dictionary<string, List<string>>();
                // information about application logs (if any) - Item1 is logpath, Item2 is filter
                var applogs = new Dictionary<string, Tuple<string, string>>(StringComparer.Ordinal);

                foreach (var site in mgr.Sites) {
                    var bindings = site.Bindings.Select(b => string.Format("{0}://{1}", b.Protocol, b.BindingInformation)).ToArray();
                    if (logger.IsDebugEnabled) {
                        logger.Debug("Found site: {0}, binding: {1}", site.Name, string.Join(", ", bindings));
                    }
                    // log configuration (we currently support only W3C logs
                    string logpath = null;
                    if (site.LogFile.Enabled && site.LogFile.LogFormat == LogFormat.W3c
                        && !string.IsNullOrEmpty(site.LogFile.Directory)) {
                        logpath = Path.Combine(site.LogFile.Directory, "W3SVC" + site.Id);
                    }

                    foreach (var app in site.Applications) {
                        try {
                            logger.Debug("Found application: {0}, pool: {1}", app.Path, app.ApplicationPoolName);
                            // get the default virtual directory - we will treat it as the application path
                            var defaultVirtDir = app.VirtualDirectories["/"];
                            var c = new ApplicationServerConfig {
                                AppType = ApplicationServerConfig.WebAppType,
                                AppPath = defaultVirtDir.PhysicalPath,
                                Server = Environment.MachineName,
                                ServerFqdnOrIp = serverFqdnOrIp,
                                AppPoolName = app.ApplicationPoolName,
                                Bindings = bindings
                            };
                            configs.Add(c);
                            if (logpath != null && !applogs.ContainsKey(c.AppPath)) {
                                applogs.Add(c.AppPath, new Tuple<string, string>(logpath, app.Path));
                            }

                            List<string> poolApps;
                            if (!poolToApps.TryGetValue(c.AppPoolName, out poolApps)) {
                                poolApps = new List<string>();
                                poolToApps.Add(c.AppPoolName, poolApps);
                            }
                            poolApps.Add(c.AppPath);
                        } catch (Exception ex) {
                            logger.Error(ex, "Failed when querying information about application: '{0}'", app.Path);
                        }
                    }
                }

                // now it's time to fill the workers table
                foreach (var pool in mgr.ApplicationPools) {
                    try {
                        if (pool.WorkerProcesses != null && pool.WorkerProcesses.Count > 0) {
                            List<string> appPaths;
                            if (poolToApps.TryGetValue(pool.Name, out appPaths)) {
                                var pids = pool.WorkerProcesses.Select(w => w.ProcessId).ToArray();
                                if (logger.IsDebugEnabled) {
                                    logger.Debug("Found worker processes: {0} for the pool: '{1}'", string.Join(",", pids), pool.Name);
                                }
                                foreach (var path in appPaths) {
                                    if (!appinfo.ContainsKey(path))
                                    {
                                        var ai = new AppInfo {
                                            Path = path,
                                            ProcessIds = pids
                                        };
                                        Tuple<string, string> li;
                                        if (applogs.TryGetValue(path, out li))
                                        {
                                            ai.LogType = ELogType.W3SVC;
                                            ai.LogEnabled = true;
                                            ai.LogsPath = li.Item1;
                                            ai.LogFilter = li.Item2;
                                        }
                                        appinfo.Add(path, ai);
                                    }
                                }
                            }
                        }
                    } catch (COMException ex) {
                        logger.Warn(ex, "Problem when querying information about a pool: '{0}'", pool.Name);
                    }
                }
            } catch (Exception ex) {
                logger.Error(ex, "Problem when querying information about the IIS.");
            } finally {
                if (mgr != null) {
                    mgr.Dispose();
                }
            }
        }

        private void LoadWinServicesConfigs(IList<ApplicationServerConfig> configs, IDictionary<string, AppInfo> appinfo)
        {
            using (var c = new ManagementClass("Win32_Service")) {
                var alreadyProcessedPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (ManagementObject o in c.GetInstances()) {
                    try {
                        var name = (string)o["Name"];
                        var path = (string)o["PathName"];
                        foreach (char invalidchr in InvalidPathChars) {
                            path = path.Replace(invalidchr, ' ');
                        }
                        path = Path.GetDirectoryName(path);

                        if (MusketeerConfiguration.ShouldServiceBeIncluded(name)) {
                            if (MusketeerConfiguration.ShouldServiceBeExcluded(name)) {
                                logger.Debug("Skipping service: '{0}' (path: '{1}') - is in the exclusion list.", name, path);
                                continue;
                            }
                            string alreadyProcessedService;
                            if (alreadyProcessedPaths.TryGetValue(path, out alreadyProcessedService)) {
                                logger.Warn("The path: '{0}' for service '{1}' was already used by service: '{2}'. Either " +
                                    "remove one of the services from monitoring or move any of them to some different location.",
                                    o["PathName"], o["Name"], alreadyProcessedService);
                                continue;
                            }
                            logger.Debug("Including service: '{0}' (path: '{1}') - is in the inclusion list.", name, path);

                            configs.Add(new ApplicationServerConfig {
                                AppType = ApplicationServerConfig.WinSvcType,
                                AppPath = path,
                                DisplayName = (string)o["DisplayName"],
                                ServiceName = name,
                                Server = Environment.MachineName,
                                ServerFqdnOrIp = serverFqdnOrIp
                            });

                            if ("Running".Equals((string)o["State"], StringComparison.OrdinalIgnoreCase)) {
                                var pid = Convert.ToInt32(o["ProcessId"]);
                                logger.Info("Service '{0}' running with PID: {1} seems interesting", name, pid);
                                appinfo.Add(path, new AppInfo {
                                    Path = path,
                                    ProcessIds = new[] { pid }
                                });
                            }
                        }
                    } finally {
                        o.Dispose();
                    }
                }
            }
        }
    }
}
