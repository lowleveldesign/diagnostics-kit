using LowLevelDesign.Diagnostics.Commons.Models;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LowLevelDesign.Diagnostics.Castle.Modules
{
    public sealed class SearchModule : NancyModule
    {
        public SearchModule() {
            Get["/version"] = _ => typeof(LogRecord).Assembly.GetName().Version.ToString();
        }
    }
}