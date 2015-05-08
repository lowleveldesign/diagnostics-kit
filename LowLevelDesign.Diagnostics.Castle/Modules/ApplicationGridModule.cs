using LowLevelDesign.Diagnostics.Commons.Storage;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public class ApplicationGridModule : NancyModule
    {
        public ApplicationGridModule(ILogStore logStore) {
            Get["/", true] = async (x, ct) => {
                var appStats = await logStore.GetApplicationStatuses(DateTime.UtcNow.AddMinutes(-2));

                return View["application-grid.html"];
            };
        }
    }
}