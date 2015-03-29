using LowLevelDesign.Diagnostics.Commons.Models;
using Nancy;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public class InfoModule : NancyModule
    {
        public InfoModule() {
            Get["/version"] = _ => typeof(LogRecord).Assembly.GetName().Version.ToString();
        }
    }
}