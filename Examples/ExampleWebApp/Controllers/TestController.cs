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
        // GET: Test
        public ActionResult Index()
        {
            var asm = Assembly.GetExecutingAssembly();
            var process = Process.GetCurrentProcess();

            return Content("test");
        }
    }
}