using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.Commons.Config
{
    public interface IAppConfigurationManager
    {
        /// <summary>
        /// Adds or updates application in the cache and in the configuration database.
        /// </summary>
        /// <param name="app">Application to insert or update.</param>
        /// <returns></returns>
        Task AddOrUpdateAppAsync(Application app);

        /// <summary>
        /// Finds application based on its path - it may return null if
        /// application was not defined.
        /// </summary>
        /// <param name="path">A path to the application - it's used as an application identifier.</param>
        /// <returns></returns>
        Task<Application> FindAppAsync(string path);

        /// <summary>
        /// Updates selected properties for an app. Path must be provided 
        /// in the app object.
        /// </summary>
        Task UpdateAppPropertiesAsync(Application app, String[] propertiesToUpdate);

        /// <summary>
        /// Retruns an ordered by path list of applications for which we have already received logs
        /// and which are not hidden.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Application>> GetAppsAsync();

        /// <summary>
        /// Returns application configurations on all servers (null) or 
        /// on a given server.
        /// </summary>
        /// <param name="appPaths">Applications for which we need configuration settings</param>
        /// <param name="server">Server which interests us (or null if we want to return configurations on all servers)</param>
        /// <returns></returns>
        Task<IEnumerable<ApplicationServerConfig>> GetAppConfigsAsync(String[] appPaths, String server = null);

        /// <summary>
        /// Adds or updates application server configuration.
        /// </summary>
        /// <param name="config">Configuration settings</param>
        /// <returns></returns>
        Task AddOrUpdateAppServerConfigAsync(ApplicationServerConfig config);

        /// <summary>
        /// Sets global setting value. If value for a given key exists
        /// it will be overwritten. 
        /// </summary>
        Task SetGlobalSettingAsync(String key, String value);

        /// <summary>
        /// Gets value of the global setting. Returns null 
        /// if the key does not exist.
        /// </summary>
        Task<String> GetGlobalSettingAsync(String key);

        /// <summary>
        /// Gets value of the global setting synchronously. 
        /// Returns null if the key does not exist.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        String GetGlobalSetting(String key);
    }
}
