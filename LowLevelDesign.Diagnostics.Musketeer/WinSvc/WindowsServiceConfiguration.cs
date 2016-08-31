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
using LowLevelDesign.Diagnostics.Musketeer.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;

namespace LowLevelDesign.Diagnostics.Musketeer.WinSvc
{
    public class WindowsServiceConfiguration : IComponentConfiguration
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

        private readonly List<ApplicationServerConfig> configs = new List<ApplicationServerConfig>();
        private readonly List<AppInfo> apps = new List<AppInfo>();

        public WindowsServiceConfiguration()
        {
            using (var c = new ManagementClass("Win32_Service")) {
                var alreadyProcessedPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (ManagementObject o in c.GetInstances().OfType<ManagementObject>()) {
                    try {
                        var name = (string)o["Name"];
                        var path = (string)o["PathName"];
                        foreach (char invalidchr in InvalidPathChars) {
                            path = path.Replace(invalidchr, ' ');
                        }

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
                                ServerFqdnOrIp = MusketeerConfiguration.ServerFqdnOrIp
                            });

                            if ("Running".Equals((string)o["State"], StringComparison.OrdinalIgnoreCase)) {
                                var pid = Convert.ToInt32(o["ProcessId"]);
                                logger.Info("Service '{0}' running with PID: {1} seems interesting", name, pid);
                                apps.Add(new AppInfo { Path = path, ProcessIds = new[] { pid },
                                    ApplicationType = EAppType.WindowsService });
                            }
                            alreadyProcessedPaths.Add(path, name);
                        }
                    } finally {
                        o.Dispose();
                    }
                }
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

        public void Dispose()
        {
        }
    }
}
