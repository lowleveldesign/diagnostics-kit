using LowLevelDesign.Diagnostics.LogStore.Commons.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using System.Threading.Tasks;
using System.Runtime.Caching;

namespace LowLevelDesign.Diagnostics.Castle.Caching
{
    public class AppConfigurationManagerWrapper : IAppConfigurationManager
    {
        private const string CachePrefixForApplications = "app:";

        private readonly IAppConfigurationManager wrappedInstance;
        private readonly MemoryCache cache = new MemoryCache("appconf-cache");

        public AppConfigurationManagerWrapper(IAppConfigurationManager wrappedInstance)
        {
            this.wrappedInstance = wrappedInstance;
        }

        public Task AddOrUpdateAppAsync(Application app)
        {
            cache.Remove(CachePrefixForApplications + app.Path);
            return wrappedInstance.AddOrUpdateAppAsync(app);
        }

        public Task AddOrUpdateAppServerConfigAsync(ApplicationServerConfig config)
        {
            return wrappedInstance.AddOrUpdateAppServerConfigAsync(config);
        }

        public async Task<Application> FindAppAsync(string path)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ApplicationServerConfig>> GetAppConfigsAsync(string[] appPaths, string server = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Application>> GetAppsAsync()
        {
            throw new NotImplementedException();
        }

        public string GetGlobalSetting(string key)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetGlobalSettingAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task SetGlobalSettingAsync(string key, string value)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAppPropertiesAsync(Application app, string[] propertiesToUpdate)
        {
            throw new NotImplementedException();
        }
    }
}