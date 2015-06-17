using LowLevelDesign.Diagnostics.Castle.Config;
using LowLevelDesign.Diagnostics.Castle.Logs;
using LowLevelDesign.Diagnostics.Commons.Config;
using LowLevelDesign.Diagnostics.Commons.Storage;
using Nancy;
using System;
using System.Linq;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public class MaintenanceModule : NancyModule
    {

        public MaintenanceModule(ILogMaintenance logmaintain)
        {
            Get["/maintain", true] = async (x, ct) => {
                await logmaintain.PerformMaintenanceIfNecessaryAsync(true);
                return "DONE";
            };
        }
    }
}