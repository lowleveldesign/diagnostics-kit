using LowLevelDesign.Diagnostics.Castle.Config;
using LowLevelDesign.Diagnostics.Castle.Models;
using LowLevelDesign.Diagnostics.Commons.Storage;
using Nancy;
using System;
using System.Linq;
using System.Collections.Generic;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public class ApplicationGridModule : NancyModule
    {
        public ApplicationGridModule(IAppConfigurationManager appconf, ILogStore logStore) {
            Get["/", true] = async (x, ct) => {
                var appstats = await logStore.GetApplicationStatuses(DateTime.UtcNow.AddMinutes(-2));

                var servers = new SortedSet<String>();
                var extendedAppStats = new SortedDictionary<String, ExtendedApplicationStatus>();
                foreach (var appstat in appstats) {
                    servers.Add(appstat.Server);

                    var app = await appconf.FindAppAsync(appstat.ApplicationPath);
                    if (app != null && !app.IsExcluded) {
                        extendedAppStats.Add(app.Name, new ExtendedApplicationStatus {
                            ApplicationName = app.Name,
                            ApplicationStatus = appstat
                        });
                    }
                }

                return View["ApplicationGrid.cshtml", new ApplicationGridModel {
                    Servers = servers.ToArray(),
                    ApplicationStatuses = extendedAppStats.Values.ToArray()
                }];
            };
        }
    }
}