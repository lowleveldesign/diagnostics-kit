using LowLevelDesign.Diagnostics.Castle.Config;
using LowLevelDesign.Diagnostics.Castle.Models;
using LowLevelDesign.Diagnostics.Commons.Models;
using Nancy;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public class InfoModule : NancyModule
    {
        public InfoModule(GlobalConfig globalSettings)
        {
            Get["/about"] = _ => {
                var result = new DiagnosticsKitInformation {
                    Version = SelfInformation.ApplicationVersion,
                    UsedLogStore = AppSettings.GetLogStore().GetType().AssemblyQualifiedName,
                    UsedAppConfigurationManager = AppSettings.GetAppConfigurationManager().GetType().AssemblyQualifiedName,
                    UsedAppUserManager = AppSettings.GetAppUserManager().GetType().AssemblyQualifiedName,
                    IsAuthenticationEnabled = globalSettings.IsAuthenticationEnabled()
                };
                return Negotiate.WithView("About.cshtml").WithModel(result);
            };
        }
    }
}