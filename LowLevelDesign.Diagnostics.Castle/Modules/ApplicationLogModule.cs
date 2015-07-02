using LowLevelDesign.Diagnostics.Castle.Models;
using LowLevelDesign.Diagnostics.Commons.Storage;
using Nancy;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public sealed class ApplicationLogModule : NancyModule
    {
        public ApplicationLogModule(ILogStore logStore) {
            Get["/", true] = async (x, ct) => {
                var model = new ApplicationLogSearchResult();

                return View["ApplicationLogs.cshtml", model];
            };
        }
    }
}