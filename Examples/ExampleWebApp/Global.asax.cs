using LowLevelDesign.Diagnostics.Harvester.AspNet.Mvc;
using LowLevelDesign.Diagnostics.Harvester.AspNet.WebAPI;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace ExampleWebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            GlobalConfiguration.Configure(WebApiConfig);

            GlobalFilters.Filters.Add(new DiagnosticsKitHandleErrorAttribute());
        }

        private void WebApiConfig(HttpConfiguration config) {
            config.Filters.Add(new DiagnosticsKitExceptionFilterAttribute());
        }
    }
}
