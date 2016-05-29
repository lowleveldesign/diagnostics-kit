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
using LowLevelDesign.Diagnostics.LogStore.Commons.Config;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Castle.Caching
{
    public class AppConfigurationManagerWrapper : IAppConfigurationManager
    {
        private readonly static TimeSpan TimeToKeepApplicationInCache = TimeSpan.FromSeconds(
            Int32.Parse(ConfigurationManager.AppSettings["diag:appcacheInSeconds"] ?? "120"));
        private readonly static bool IsCachingEnabled = TimeToKeepApplicationInCache > TimeSpan.Zero;
        private const string CachePrefixForApplications = "app:";

        private readonly IAppConfigurationManager wrappedInstance;
        private readonly MemoryCache cache = new MemoryCache("appconf-cache");

        public AppConfigurationManagerWrapper(IAppConfigurationManager wrappedInstance)
        {
            this.wrappedInstance = wrappedInstance;
        }

        public Task AddOrUpdateAppAsync(Application app)
        {
            if (IsCachingEnabled) {
                cache.Remove(CachePrefixForApplications + app.Path);
            }
            return wrappedInstance.AddOrUpdateAppAsync(app);
        }

        public Task AddOrUpdateAppServerConfigAsync(ApplicationServerConfig config)
        {
            return wrappedInstance.AddOrUpdateAppServerConfigAsync(config);
        }

        public async Task<Application> FindAppAsync(string path)
        {
            var cacheKey = CachePrefixForApplications + path;
            if (IsCachingEnabled && cache.Contains(cacheKey)) {
                return (Application)cache[cacheKey];
            }
            var app = await wrappedInstance.FindAppAsync(path);
            if (IsCachingEnabled && app != null) {
                cache.Add(cacheKey, app, new CacheItemPolicy {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.Add(TimeToKeepApplicationInCache)
                });
            }
            return app;
        }

        public Task<IEnumerable<ApplicationServerConfig>> GetAppConfigsAsync(string[] appPaths, string server = null)
        {
            return wrappedInstance.GetAppConfigsAsync(appPaths, server);
        }

        public Task<IEnumerable<Application>> GetAppsAsync()
        {
            return wrappedInstance.GetAppsAsync();
        }

        public string GetGlobalSetting(string key)
        {
            return wrappedInstance.GetGlobalSetting(key);
        }

        public Task<string> GetGlobalSettingAsync(string key)
        {
            return wrappedInstance.GetGlobalSettingAsync(key);
        }

        public Task SetGlobalSettingAsync(string key, string value)
        {
            return wrappedInstance.SetGlobalSettingAsync(key, value);
        }

        public Task UpdateAppPropertiesAsync(Application app, string[] propertiesToUpdate)
        {
            if (IsCachingEnabled) {
                cache.Remove(CachePrefixForApplications + app.Path);
            }
            return wrappedInstance.UpdateAppPropertiesAsync(app, propertiesToUpdate);
        }
    }
}
