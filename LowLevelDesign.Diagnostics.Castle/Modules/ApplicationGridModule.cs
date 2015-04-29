using LowLevelDesign.Diagnostics.Commons.Storage;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public class ApplicationGridModule : NancyModule
    {
        public ApplicationGridModule(ILogStore logStore) {
            Get["/"] = _ => {
                return View["application-grid.html"];
            };
        }
    }
}