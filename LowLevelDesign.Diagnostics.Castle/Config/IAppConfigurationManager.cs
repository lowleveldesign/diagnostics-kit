using LowLevelDesign.Diagnostics.Castle.Models;
using System;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Castle.Config
{
    interface IAppConfigurationManager
    {
        /// <summary>
        /// Adds or updates application in the cache and in the configuration database.
        /// </summary>
        /// <param name="app">Application to insert or update.</param>
        /// <returns></returns>
        Task AddOrUpdateApp(Application app);

        /// <summary>
        /// Finds application based on its path - it may return null if
        /// application was not defined.
        /// </summary>
        /// <param name="path">A path to the application - it's used as an application identifier.</param>
        /// <returns></returns>
        Task<Application> FindApp(string path);

        /// <summary>
        /// Removes application from the cache and the configuration database - it won't
        /// be monitored any longer.
        /// </summary>
        /// <param name="path">A path to the application - it's used as an application identifier.</param>
        /// <returns></returns>
        Task RemoveApp(string path);
    }
}
