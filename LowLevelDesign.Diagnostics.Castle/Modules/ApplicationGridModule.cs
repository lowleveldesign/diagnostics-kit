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
                    var appstats = await logStore.GetApplicationStatuses(DateTime.UtcNow.AddMinutes(-15));
                    var allapps = await appconf.GetAppsAsync();

                    var servers = new SortedSet<String>();
                    var apps = new Dictionary<String, Application>();
                    var extendedAppStats = new SortedDictionary<String, IDictionary<String, LastApplicationStatus>>();
                    foreach (var appstat in appstats)
                    {
                        servers.Add(appstat.Server);

                        var app = allapps.FirstOrDefault(a => String.Equals(a.Path, appstat.ApplicationPath,
                            StringComparison.InvariantCultureIgnoreCase));
                        if (app != null && !app.IsExcluded)
                        {
                            String key = String.Format("{0}:{1}", app.Name, app.Path);
                            if (!apps.ContainsKey(key))
                            {
                                apps.Add(key, app);
                            }
                            IDictionary<String, LastApplicationStatus> srvstat;
                            if (!extendedAppStats.TryGetValue(key, out srvstat))
                            {
                                srvstat = new Dictionary<String, LastApplicationStatus>(StringComparer.OrdinalIgnoreCase);
                                extendedAppStats.Add(key, srvstat);
                            }
                            srvstat.Add(appstat.Server, appstat);
                        }
                    }
                    // for the rest of the applications
                    foreach (var app in allapps)
                    {
                        if (!app.IsExcluded)
                        {
                            String key = String.Format("{0}:{1}", app.Name, app.Path);
                            if (!apps.ContainsKey(key))
                            {
                                apps.Add(key, app);
                            }
                            if (!extendedAppStats.ContainsKey(key))
                            {
                                extendedAppStats.Add(key, new Dictionary<String, LastApplicationStatus>(StringComparer.OrdinalIgnoreCase));
                            }
                        }
                    }

                    model = new ApplicationGridModel {
                        LastUpdateTime = DateTime.Now,
                        Servers = servers.ToArray(),
                        Applications = apps,
                        ApplicationStatuses = extendedAppStats
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