using FluentValidation;
using LowLevelDesign.Diagnostics.Castle.Config;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.Commons.Storage;
using LowLevelDesign.Diagnostics.Commons.Validators;
using LowLevelDesign.Diagnostics.LuceneNetLogStore;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Diagnostics;
using Nancy.TinyIoc;
using System.Configuration;
using System.IO;
using System.Web.Configuration;

namespace LowLevelDesign.Diagnostics.Castle
{
    public class Bootstraper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines) {
            // configure application hooks

            // FIXME for release builds: DiagnosticsHook.Disable(pipelines);
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container) {
            // configure application singletons and multinstance classes

            /* LOG STORAGE */
            // FIXME we should inject here the configured logstore
            var logstorePath = WebConfigurationManager.AppSettings["logstore:path"];
            if (!Directory.Exists(logstorePath)) {
                throw new ConfigurationErrorsException(Resource.InvalidLogstorePath);
            }
            container.Register<ILogStore, LogStore>(new LogStore(logstorePath, WebConfigurationManager.AppSettings["logstore:log"]));

            /* VALIDATORS */
            container.Register<IValidator<LogRecord>, LogRecordValidator>();

            /* CONFIGURATION */
            container.Register<IAppConfigurationManager, AppConfigurationManager>();
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context) {
            // configure request-lifetime objects
        }

        protected override Nancy.Diagnostics.DiagnosticsConfiguration DiagnosticsConfiguration {
            get { return new DiagnosticsConfiguration { Password = "n4ncyBoard" }; }
        }
    }
}