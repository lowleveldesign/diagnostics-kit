using LowLevelDesign.Diagnostics.Castle.Config;
using LowLevelDesign.Diagnostics.Commons.Config;
using LowLevelDesign.Diagnostics.Commons.Storage;
using Nancy;
using System;
using System.Linq;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public class MaintenanceModule : NancyModule
    {

        public MaintenanceModule(ILogStore logStore, IAppConfigurationManager config)
        {
            Get["/maintain", true] = async (x, ct) => {
                var appmaintenance = (await config.GetAppsAsync()).Where(app => app.DaysToKeepLogs.HasValue).ToDictionary(
                    app => app.Path, app => TimeSpan.FromDays(app.DaysToKeepLogs.Value));
                
                await logStore.Maintain(TimeSpan.FromDays(AppSettingsWrapper.DefaultNoOfDaysToKeepLogs), appmaintenance);

                return "DONE";
            };
        }
    }
}