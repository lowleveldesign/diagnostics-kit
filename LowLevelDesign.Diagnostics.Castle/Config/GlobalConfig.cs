using LowLevelDesign.Diagnostics.LogStore.Commons.Config;
using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Castle.Config
{
    public class GlobalConfig
    {
        private const String AuthSettingKey = "auth:enabled";

        private const String CachePrefix = "conf:";
        private MemoryCache cache = MemoryCache.Default;

        private readonly IAppConfigurationManager confManager;

        public GlobalConfig(IAppConfigurationManager confManager)
        {
            this.confManager = confManager;
        }

        public async Task<bool> IsAuthenticationEnabled()
        {
            var o = cache.Get(CachePrefix + AuthSettingKey);
            if (o == null) {
                bool flag;
                Boolean.TryParse(await confManager.GetGlobalSettingAsync(AuthSettingKey), out flag);
                cache.Set(CachePrefix + AuthSettingKey, flag, new CacheItemPolicy {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(5)
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