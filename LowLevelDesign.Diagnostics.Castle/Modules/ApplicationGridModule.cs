using LowLevelDesign.Diagnostics.Castle.Config;
using LowLevelDesign.Diagnostics.Castle.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Config;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Storage;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public class ApplicationGridModule : NancyModule
    {
        private readonly static int DefaultCacheTimeInSeconds = AppSettings.DefaultGridCacheTimeInSeconds;
        private MemoryCache cache = MemoryCache.Default;

        public ApplicationGridModule(GlobalConfig globals, IAppConfigurationManager appconf, ILogStore logStore)
        {
            if (globals.IsAuthenticationEnabled())
            {
                this.RequiresAuthentication();
            }

            Get["/", true] = async (x, ct) => {
                ApplicationGridModel model = null;
                if (DefaultCacheTimeInSeconds != 0)
                {
                    model = cache["appstats"] as ApplicationGridModel;
                }

                if (model == null)
                {
                    var applicationStatuses = await logStore.GetApplicationStatusesAsync(DateTime.UtcNow.AddMinutes(-15));
                    var allApplications = await appconf.GetAppsAsync();

                    var activeServers = new SortedSet<string>();
                    var activeApplications = new Dictionary<string, Application>();
                    var activeApplicationStatuses = new SortedDictionary<string, IDictionary<string, LastApplicationStatus>>();
                    foreach (var appStatus in applicationStatuses)
                    {
                        activeServers.Add(appStatus.Server);

                        var app = allApplications.FirstOrDefault(a => string.Equals(a.Path, appStatus.ApplicationPath,
                            StringComparison.InvariantCultureIgnoreCase));
                        if (app != null && !app.IsExcluded)
                        {
                            if (!activeApplications.ContainsKey(app.Path))
                            {
                                activeApplications.Add(app.Path, app);
                            }
                            IDictionary<string, LastApplicationStatus> applicationStatusPerServer;
                            if (!activeApplicationStatuses.TryGetValue(app.Path, out applicationStatusPerServer))
                            {
                                applicationStatusPerServer = new Dictionary<string, LastApplicationStatus>(StringComparer.OrdinalIgnoreCase);
                                activeApplicationStatuses.Add(app.Path, applicationStatusPerServer);
                            }
                            applicationStatusPerServer.Add(appStatus.Server, appStatus);
                        }
                    }
                    // for the rest of the applications
                    foreach (var app in allApplications)
                    {
                        if (!app.IsExcluded)
                        {
                            if (!activeApplications.ContainsKey(app.Path))
                            {
                                activeApplications.Add(app.Path, app);
                            }
                            if (!activeApplicationStatuses.ContainsKey(app.Path))
                            {
                                activeApplicationStatuses.Add(app.Path, new Dictionary<string, LastApplicationStatus>(StringComparer.OrdinalIgnoreCase));
                            }
                        }
                    }

                    model = new ApplicationGridModel {
                        LastUpdateTime = DateTime.Now,
                        Servers = activeServers.ToArray(),
                        Applications = activeApplications,
                        ApplicationStatuses = activeApplicationStatuses
                    };
                    if (DefaultCacheTimeInSeconds != 0)
                    {
                        cache.Set("appstats", model, new CacheItemPolicy() {
                            AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(DefaultCacheTimeInSeconds)
                        });
                    }
                }
                return View["ApplicationGrid.cshtml", model];
            };

            Get["/apps", true] = async (x, ct) => {
                // gets applications for which we have received the logs
                return View["Applications", (await appconf.GetAppsAsync()).Where(app => !app.IsHidden).Select(
                    app => {
                        app.DaysToKeepLogs = app.DaysToKeepLogs ?? AppSettings.DefaultNoOfDaysToKeepLogs;
                        return app;
                    })];
            };
        }
    }
}