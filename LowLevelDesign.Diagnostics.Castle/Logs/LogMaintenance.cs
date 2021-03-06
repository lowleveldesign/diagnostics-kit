﻿/**
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

using LowLevelDesign.Diagnostics.Castle.Config;
using LowLevelDesign.Diagnostics.LogStore.Commons.Config;
using LowLevelDesign.Diagnostics.LogStore.Commons.Storage;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Castle.Logs
{
    public interface ILogMaintenance
    {
        Task<bool> PerformMaintenanceIfNecessaryAsync(bool force = false);
    }

    public class LogMaintenance : ILogMaintenance
    {
        private static readonly TraceSource logger = new TraceSource("LowLevelDesign.Diagnostics.Castle");

        private const string lastMaintenanceKey = "diag:last-logs-maintenance";
        private static readonly MemoryCache cache = MemoryCache.Default;

        private readonly IAppConfigurationManager config;
        private readonly ILogStore logStore;

        public LogMaintenance(IAppConfigurationManager config, ILogStore logStore)
        {
            this.config = config;
            this.logStore = logStore;
        }

        public async Task<bool> PerformMaintenanceIfNecessaryAsync(bool force = false)
        {
            if (force || !cache.Contains(lastMaintenanceKey)) {
                logger.TraceEvent(TraceEventType.Information, 0, "Performing logs maintenance, force: {0}", force);

                // we need to perform the maintenance
                var appmaintenance = (await config.GetAppsAsync()).Where(app => app.DaysToKeepLogs.HasValue).ToDictionary(
                    app => app.Path, app => TimeSpan.FromDays(app.DaysToKeepLogs.Value));
                
                await logStore.MaintainAsync(TimeSpan.FromDays(AppSettings.DefaultNoOfDaysToKeepLogs), appmaintenance);

                // the maintenance can't be performed often then on daily basis 
                // so we shouldn't call it more often then once a day (I will leave
                // 12h in case the idea changes :))
                cache.Set(lastMaintenanceKey, DateTime.UtcNow, new CacheItemPolicy {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(12)
                });

                return true;
            }
            return false;
        }
    }
}
