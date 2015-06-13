using LowLevelDesign.Diagnostics.Castle.Models;
using LowLevelDesign.Diagnostics.Commons.Storage;
using Nancy;
using System;
using System.Linq;
using System.Collections.Generic;
using LowLevelDesign.Diagnostics.Commons;
using LowLevelDesign.Diagnostics.Commons.Config;
using LowLevelDesign.Diagnostics.Commons.Models;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public class ApplicationGridModule : NancyModule
    {
        public ApplicationGridModule(IAppConfigurationManager appconf, ILogStore logStore) {
            Get["/", true] = async (x, ct) => {
                var appstats = await logStore.GetApplicationStatuses(DateTime.UtcNow.AddMinutes(-2));

                var servers = new SortedSet<String>();
                var extendedAppStats = new SortedDictionary<String, IDictionary<String, LastApplicationStatus>>(StringComparer.Ordinal);
                foreach (var appstat in appstats) {
                    servers.Add(appstat.Server);

                    var app = await appconf.FindAppAsync(appstat.ApplicationPath);
                    if (app != null && !app.IsExcluded) {
                        IDictionary<String, LastApplicationStatus> srvstat;
                        if (!extendedAppStats.TryGetValue(app.Name, out srvstat)) {
                            srvstat = new Dictionary<String, LastApplicationStatus>(StringComparer.Ordinal);
                            extendedAppStats.Add(app.Name, srvstat);
                        }
                        srvstat.Add(appstat.Server, appstat);
                    }
                }

                return View["ApplicationGrid.cshtml", new ApplicationGridModel {
                    Servers = servers.ToArray(),
                    ApplicationStatuses = extendedAppStats
                }];
            };

            Get["/apps", true] = async (x, ct) => {
                // gets applications for which we have received the logs
                return View["Applications", await appconf.GetAppsAsync()];
            };
        }
    }
}