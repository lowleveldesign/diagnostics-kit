using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LowLevelDesign.Diagnostics.Castle
{
    public class Bootstraper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines) {
            // configure application hooks
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container) {
            // configure application singletons and multinstance classes
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context) {
            // configure request-lifetime objects
        }
    }
}