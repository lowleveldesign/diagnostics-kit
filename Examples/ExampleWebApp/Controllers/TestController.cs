using LowLevelDesign.Diagnostics.Harvester.AspNet.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
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