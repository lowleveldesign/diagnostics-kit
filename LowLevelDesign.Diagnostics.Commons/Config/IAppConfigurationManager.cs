using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Commons.Config
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
        /// Removes application from the cache and the configuration database - it won't
        /// be monitored any longer.
        /// </summary>
        /// <param name="path">A path to the application - it's used as an application identifier.</param>
        /// <returns></returns>
        Task RemoveAppAsync(string path);

        /// <summary>
        /// Retruns a sorted list of applications for which we have already received logs.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Application>> GetAppsAsync();
    }
}
