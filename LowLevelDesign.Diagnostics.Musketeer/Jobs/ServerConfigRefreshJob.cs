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
using LowLevelDesign.Diagnostics.Musketeer.IIS;
using LowLevelDesign.Diagnostics.Musketeer.Models;
using LowLevelDesign.Diagnostics.Musketeer.WinSvc;
using NLog;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;

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
        private readonly ISharedInfoAboutApps sharedAppsInfo;
        private readonly IMusketeerConnector connector;


        public ServerConfigRefreshJob(ISharedInfoAboutApps sharedAppsInfo, IMusketeerConnectorFactory connectorFactory)
        {
            this.sharedAppsInfo = sharedAppsInfo;
            connector = connectorFactory.CreateConnector();
        }

        public void Execute(IJobExecutionContext context)
        {
            var configs = new List<ApplicationServerConfig>();
            var applications = new List<AppInfo>();

            foreach (var component in LoadComponents()) {
                configs.AddRange(component.ApplicationConfigurations);
                applications.AddRange(component.Applications);
            }

            var activeApps = connector.SupportsApplicationConfigs ?
                connector.SendApplicationConfigs(configs) : configs.Select(c => c.AppPath).ToArray();

            // store the configs in the shared storage
            var map = new Dictionary<string, AppInfo>(activeApps.Length, StringComparer.Ordinal);
            foreach (var path in activeApps) {
                if (map.ContainsKey(path)) {
                    logger.Warn("Duplicate application path found: '{0}' - it will appear once in the log.", path);
                    continue;
                }
                AppInfo appinfo = applications.FirstOrDefault(a => a.Path.Equals(path, StringComparison.Ordinal));
                if (appinfo != null) {
                    logger.Debug("Application '{0}' will be monitored by the Musketeer.", path);
                    map.Add(path, appinfo);
                }
            }
            sharedAppsInfo.UpdateAppWorkerProcessesMap(map);
        }

        public ICollection<IComponentConfiguration> LoadComponents()
        {
            var components = new List<IComponentConfiguration>(2);
            try {
                components.Add(new IISConfiguration());
            } catch (Exception ex) {
                logger.Error(ex, "Problem when querying information about the IIS.");
            } 
            try {
                components.Add(new WindowsServiceConfiguration());
            } catch (Exception ex) {
                logger.Error(ex, "Problem when querying information about the Windows services.");
            }
            return components;
        }
    }
}
