using System.Diagnostics;
using LowLevelDesign.Diagnostics.Castle.Config;
using LowLevelDesign.Diagnostics.Castle.Models;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.Commons.Storage;
using Nancy;
using System;
using System.Collections.Generic;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public class ApplicationGridModule : NancyModule
    {
        public ApplicationGridModule(ILogStore logStore, IAppConfigurationManager configurationManager)
        {
            Get["/"] = _ => {
                return View["application-grid.html"];
            };
            Get["/grid/{tab}", true] = async (x, ct) => {
                var model = new ApplicationGrid {
                    ApplicationStatuses = new Dictionary<String, Dictionary<String, LastApplicationStatus>>(StringComparer.OrdinalIgnoreCase),
                    Servers = new SortedSet<String>(StringComparer.OrdinalIgnoreCase)
                };
                foreach (var appst in await logStore.GetApplicationStatuses(DateTime.UtcNow.AddMinutes(-2))) {
                    var app = await configurationManager.FindApp(appst.ApplicationPath);
                    if (!app.IsExcluded) {
                        Dictionary<String, LastApplicationStatus> d;
                        if (!model.ApplicationStatuses.TryGetValue(app.Name, out d)) {
                            d = new Dictionary<String, LastApplicationStatus>();
                            model.ApplicationStatuses.Add(app.Name, d);
                        }
                        model.Servers.Add(appst.Server);
                        if (!d.ContainsKey(appst.Server)) {
                            d.Add(appst.Server, appst);
                        } else {
                            Debug.Fail("Multiple records for the same server.");
                        }
                    }
                }
                return model;
            };
        }
    }
}