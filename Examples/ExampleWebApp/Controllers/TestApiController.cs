using LowLevelDesign.Diagnostics.Harvester.AspNet.WebAPI;
using System;
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
