using LowLevelDesign.Diagnostics.Harvester.AspNet.Mvc;
using System;
using System.Web.Mvc;

namespace ExampleWebApp.Controllers
{
    public class TestController : Controller
    {
        [DiagnosticsKitHandleError]
        public ActionResult Index()
        {
            throw new Exception("test");
        }
    }
}