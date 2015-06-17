using LowLevelDesign.Diagnostics.Castle.Models;
using LowLevelDesign.Diagnostics.Commons.Storage;
using Nancy;
using Nancy.ModelBinding;
using System;
using System.Linq;
using System.Collections.Generic;
using LowLevelDesign.Diagnostics.Commons;
using LowLevelDesign.Diagnostics.Commons.Config;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.Castle.Config;
using FluentValidation;
using System.Threading.Tasks;
using System.Runtime.Caching;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public class ApplicationGridModule : NancyModule
    {
        private readonly static int DefaultCacheTimeInSeconds = AppSettingsWrapper.DefaultGridCacheTimeInSeconds;
        private MemoryCache cache = MemoryCache.Default;

        public ApplicationGridModule(IAppConfigurationManager appconf, ILogStore logStore, IValidator<Application> appvalidator)
        {
            Get["/", true] = async (x, ct) => {
                ApplicationGridModel model = null;
                if (DefaultCacheTimeInSeconds != 0) {
                    model = cache["appstats"] as ApplicationGridModel;
                }

                if (model == null) {
                    var appstats = await logStore.GetApplicationStatuses(DateTime.UtcNow.AddMinutes(-15));

                    var servers = new SortedSet<String>();
                    var apps = new Dictionary<String, Application>();
                    var extendedAppStats = new SortedDictionary<String, IDictionary<String, LastApplicationStatus>>();
                    foreach (var appstat in appstats) {
                        servers.Add(appstat.Server);

                        var app = await appconf.FindAppAsync(appstat.ApplicationPath);

                        if (app != null && !app.IsExcluded) {
                            String key = String.Format("{0}:{1}", app.Name, app.Path);
                            if (!apps.ContainsKey(key)) {
                                apps.Add(key, app);
                            }
                            IDictionary<String, LastApplicationStatus> srvstat;
                            if (!extendedAppStats.TryGetValue(key, out srvstat)) {
                                srvstat = new Dictionary<String, LastApplicationStatus>(StringComparer.OrdinalIgnoreCase);
                                extendedAppStats.Add(key, srvstat);
                            }
                            srvstat.Add(appstat.Server, appstat);
                        }
                    }

                    model = new ApplicationGridModel {
                        LastUpdateTime = DateTime.Now,
                        Servers = servers.ToArray(),
                        Applications = apps,
                        ApplicationStatuses = extendedAppStats
                    };
                    if (DefaultCacheTimeInSeconds != 0) {
                        cache.Set("appstats", model, new CacheItemPolicy() {
                            AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(DefaultCacheTimeInSeconds)
                        });
                    }
                }

                return View["ApplicationGrid.cshtml", model];
            };

            Get["/apps", true] = async (x, ct) => {
                // gets applications for which we have received the logs
                return View["Applications", (await appconf.GetAppsAsync()).Select(app => {
                    app.DaysToKeepLogs = app.DaysToKeepLogs ?? AppSettingsWrapper.DefaultNoOfDaysToKeepLogs;
                    return app;
                })];
            };
            Post["conf/appname", true] = async (x, ct) => {
                return await UpdateAppPropertyAsync(appconf, appvalidator, this.Bind<Application>(), "Name");
            };
            Post["conf/appmaintenance", true] = async (x, ct) => {
                return await UpdateAppPropertyAsync(appconf, appvalidator, this.Bind<Application>(), "DaysToKeepLogs");
            };
            Post["conf/appexclusion", true] = async (x, ct) => {
                return await UpdateAppPropertyAsync(appconf, appvalidator, this.Bind<Application>(), "IsExcluded");
            };
        }

        private static async Task<String> UpdateAppPropertyAsync(IAppConfigurationManager appconf,
            IValidator<Application> validator, Application app, String property)
        {
            var validationResult = validator.Validate(app, "Hash", property);
            if (!validationResult.IsValid) {
                return "ERR_INVALID";
            }
            await appconf.UpdateAppPropertiesAsync(app, new[] { property });

            return "OK";
        }
    }
}