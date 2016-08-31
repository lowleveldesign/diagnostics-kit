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
using LowLevelDesign.Diagnostics.Musketeer.CLR;
using LowLevelDesign.Diagnostics.Musketeer.Config;
using LowLevelDesign.Diagnostics.Musketeer.Models;
using Microsoft.Web.Administration;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace LowLevelDesign.Diagnostics.Musketeer.IIS
{
    public class IISConfiguration : IComponentConfiguration, IDisposable
    {
        private struct AppLogInfo
        {
            public string LogsPath;
            public string LogFilter;
        }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly List<ApplicationServerConfig> configs = new List<ApplicationServerConfig>();
        private readonly List<AppInfo> apps = new List<AppInfo>();

        private readonly ServerManager serverManager;
        private readonly Dictionary<string, AppLogInfo> applicationLogs = new Dictionary<string, AppLogInfo>(StringComparer.Ordinal);
        private readonly Dictionary<string, AppDomainInfo[]> appAppDomainsMap = new Dictionary<string, AppDomainInfo[]>(StringComparer.Ordinal);

        public IISConfiguration()
        {
            serverManager = ServerManager.OpenRemote("localhost");

            LoadApplicationConfigurations();

            LoadApplicationLogsInformation();

            LoadApplicationsAppDomains();

            LoadApplications();
        }

        private void LoadApplicationConfigurations()
        {
            foreach (var site in serverManager.Sites) {
                var bindings = site.Bindings.Select(b => string.Format("{0}://{1}", b.Protocol, b.BindingInformation)).ToArray();
                if (logger.IsDebugEnabled) {
                    logger.Debug("Found site: {0}, binding: {1}", site.Name, string.Join(", ", bindings));
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
                            ServerFqdnOrIp = MusketeerConfiguration.ServerFqdnOrIp,
                            AppPoolName = app.ApplicationPoolName,
                            Bindings = bindings
                        };
                        configs.Add(c);
                    } catch (Exception ex) {
                        logger.Error(ex, "Failed when querying information about application: '{0}'", app.Path);
                    }
                }
            }
        }

        private void LoadApplicationLogsInformation()
        {
            foreach (var site in serverManager.Sites) {
                // log configuration (we currently support only W3C logs
                string logpath = null;
                if (site.LogFile.Enabled && site.LogFile.LogFormat == LogFormat.W3c
                    && !string.IsNullOrEmpty(site.LogFile.Directory)) {
                    logpath = Path.Combine(site.LogFile.Directory, "W3SVC" + site.Id);
                }

                foreach (var app in site.Applications) {
                    try {
                        var appPath = app.VirtualDirectories["/"].PhysicalPath;
                        if (logpath != null && !applicationLogs.ContainsKey(appPath)) {
                            applicationLogs.Add(appPath, new AppLogInfo { LogsPath = logpath, LogFilter = app.Path });
                        }
                    } catch (Exception ex) {
                        logger.Error(ex, "Failed when querying information about application: '{0}'", app.Path);
                    }
                }
            }
        }

        private void LoadApplicationsAppDomains()
        {
            using (var clrDac = new ClrDac()) {
                clrDac.CollectAppDomainInfo();

                Dictionary<string, string> appToAppDomainGuessedName = new Dictionary<string, string>();
                foreach (var site in serverManager.Sites) {
                    foreach (var app in site.Applications) {
                        try {
                            var appPath = app.VirtualDirectories["/"].PhysicalPath;
                            if (!appToAppDomainGuessedName.ContainsKey(appPath)) {
                                appToAppDomainGuessedName.Add(appPath, string.Format(
                                    "/LM/W3SVC/{0}/ROOT{1}", site.Id, app.Path).TrimEnd('/'));
                            }
                        } catch (Exception ex) {
                            logger.Error(ex, "Failed when querying information about application: '{0}'", app.Path);
                        }
                    }
                }

                foreach (var pool in serverManager.ApplicationPools) {
                    try {
                        if (pool.WorkerProcesses != null && pool.WorkerProcesses.Count > 0) {
                            var pids = pool.WorkerProcesses.Select(w => w.ProcessId).ToArray();

                            // for each app that runs in this apppool
                            foreach (var path in configs.Where(c => string.Equals(c.AppPoolName, pool.Name,
                                StringComparison.Ordinal)).Select(c => c.AppPath)) {
                                if (appAppDomainsMap.ContainsKey(path)) {
                                    logger.Warn("Skipping application '{0}' - duplicate and AppDomains were already added.", path);
                                }
                                var appDomains = new List<AppDomainInfo>(2);
                                string appDomainGuessedName;
                                if (appToAppDomainGuessedName.TryGetValue(path, out appDomainGuessedName)) {
                                    foreach (var pid in pids) {
                                        AppDomainInfo[] processAppDomains = clrDac.GetAppDomainsForProcess(pid);
                                        foreach (var ad in processAppDomains) {
                                            if (ad.Name.StartsWith(appDomainGuessedName, StringComparison.Ordinal)) {
                                                logger.Debug("Found appdomain: {0} for application: '{1}'", ad.Name, path);
                                                appDomains.Add(ad);
                                            }
                                        }
                                    }
                                }
                                appAppDomainsMap.Add(path, appDomains.ToArray());
                            }
                        }
                    } catch (COMException ex) {
                        logger.Warn(ex, "Problem when querying information about a pool: '{0}'", pool.Name);
                    }
                }
            }
        }

        private void LoadApplications()
        {
            foreach (var pool in serverManager.ApplicationPools) {
                try {
                    if (pool.WorkerProcesses != null && pool.WorkerProcesses.Count > 0) {
                        var pids = pool.WorkerProcesses.Select(w => w.ProcessId).ToArray();
                        // for each app that runs in this apppool
                        foreach (var path in configs.Where(c => string.Equals(c.AppPoolName, pool.Name,
                            StringComparison.Ordinal)).Select(c => c.AppPath)) {
                            var appInfo = new AppInfo {
                                Path = path,
                                ProcessIds = pids
                            };
                            AppLogInfo logInfo;
                            if (applicationLogs.TryGetValue(path, out logInfo)) {
                                appInfo.ApplicationType = EAppType.WebApplication;
                                appInfo.LogEnabled = true;
                                appInfo.LogsPath = logInfo.LogsPath;
                                appInfo.LogFilter = logInfo.LogFilter;
                            }
                            AppDomainInfo[] appDomains;
                            if (appAppDomainsMap.TryGetValue(path, out appDomains)) {
                                appInfo.AppDomains = appDomains;
                            }
                            apps.Add(appInfo);
                        }
                    }
                } catch (COMException ex) {
                    logger.Warn(ex, "Problem when querying information about a pool: '{0}'", pool.Name);
                }
            }
        }

        public void Dispose()
        {
            if (serverManager != null) {
                serverManager.Dispose();
            }
        }

        public IReadOnlyCollection<AppInfo> Applications
        {
            get { return apps; }
        }

        public IReadOnlyCollection<ApplicationServerConfig> ApplicationConfigurations
        {
            get { return configs; }
        }
    }
}
