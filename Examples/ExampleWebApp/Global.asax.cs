/**
 *  Part of the Diagnostics Kit
 *
 *  Copyright (C) 2016  Sebastian Solnica
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 */

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
