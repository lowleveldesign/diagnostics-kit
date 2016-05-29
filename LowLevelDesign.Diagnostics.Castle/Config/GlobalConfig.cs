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

using LowLevelDesign.Diagnostics.LogStore.Commons.Config;
using System;
using System.Runtime.Caching;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Castle.Config
{
    public class GlobalConfig
    {
        public static readonly String AdminRoleClaim = ClaimTypes.Role + ":admin";

        private const String AuthSettingKey = "auth:enabled";
        private const int ConfigCacheInvalidationInMinutes = 5;

        private const String CachePrefix = "conf:";
        private MemoryCache cache = MemoryCache.Default;

        private readonly IAppConfigurationManager confManager;

        public GlobalConfig(IAppConfigurationManager confManager)
        {
            this.confManager = confManager;
        }

        public bool IsAuthenticationEnabled()
        {
            var o = cache.Get(CachePrefix + AuthSettingKey);
            if (o == null) {
                bool flag;
                Boolean.TryParse(confManager.GetGlobalSetting(AuthSettingKey), out flag);
                cache.Set(CachePrefix + AuthSettingKey, flag, new CacheItemPolicy {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(ConfigCacheInvalidationInMinutes)
                });
                return flag;
            }
            return (bool)o;
        }

        public async Task ToggleAuthentication(bool enabled)
        {
            await confManager.SetGlobalSettingAsync(AuthSettingKey, enabled.ToString());
            cache.Remove(CachePrefix + AuthSettingKey);
        }
    }
}
