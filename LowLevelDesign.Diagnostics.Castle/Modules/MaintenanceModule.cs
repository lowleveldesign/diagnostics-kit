using LowLevelDesign.Diagnostics.Castle.Logs;
using Nancy;

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