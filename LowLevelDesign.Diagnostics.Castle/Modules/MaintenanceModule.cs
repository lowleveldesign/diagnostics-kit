using LowLevelDesign.Diagnostics.Commons.Storage;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public class MaintenanceModule : NancyModule
    {
        public MaintenanceModule(ILogStore logStore)
        {
            Get["/maintain", true] = async (x, ct) => {
                // FIXME prepare configuration for maintenance
                await logStore.Maintain(TimeSpan.FromDays(2));

                return "DONE";
            };
        }
    }
}