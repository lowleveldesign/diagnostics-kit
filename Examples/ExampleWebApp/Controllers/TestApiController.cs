using LowLevelDesign.Diagnostics.Harvester.AspNet.WebAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ExampleWebApp.Controllers
{
    public class TestApiController : ApiController
    {
        [DiagnosticsKitExceptionFilter]
        public String Get() {
            throw new Exception("test");
        }
    }
}
