using Nancy;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LowLevelDesign.Diagnostics.Castle.LogsCollection
{
    public class CollectorModule : NancyModule
    {
        public CollectorModule() : base("/collect") {
            Get["/aspnet"] = args => {
                return "Hello";
            };
        }
    }
}