using FluentValidation;
using LowLevelDesign.Diagnostics.Castle.Config;
using LowLevelDesign.Diagnostics.Castle.Logs;
using LowLevelDesign.Diagnostics.Commons.Config;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.Commons.Storage;
using LowLevelDesign.Diagnostics.Commons.Validators;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Diagnostics;
using Nancy.TinyIoc;
using NLog;
using System;
using System.Configuration;
using System.IO;

namespace LowLevelDesign.Diagnostics.Castle
{
    public class Bootstraper : DefaultNancyBootstrapper
    {
        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
#if !DEBUG
            DiagnosticsHook.Disable(pipelines);
#endif
            // log errors to NLog
            pipelines.OnError += (ctx, err) => {
                logger.Error(err, "Global application error occurred when serving request: {0}", ctx.Request.Url);
                return null;
            };

            // make sure that we have partitions to store the coming logs
            var logmaintain = container.Resolve<ILogMaintenance>();
            logmaintain.PerformMaintenanceIfNecessaryAsync().Wait();
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            /* LOG STORAGE */
            var logstoreType = ConfigurationManager.AppSettings["diag:logstore"];
            try {
                var t = Type.GetType(logstoreType);
                var logstore = (ILogStore)Activator.CreateInstance(t);
                container.Register(logstore);
            } catch (Exception ex) {
                throw new ConfigurationErrorsException(String.Format(
                    "LogStore of type: '{0}' could not be initialized. Make sure you specified a valid type in the appsettings diag:logstore key. Error: {1}",
                    logstoreType, ex));
            }

            /* LOGS MAINTENANCE */
            container.Register<ILogMaintenance, LogMaintenance>();

            /* VALIDATORS */
            container.Register<IValidator<LogRecord>, LogRecordValidator>();
            container.Register<IValidator<Application>, ApplicationValidator>();

            /* CONFIGURATION */
            var confMgr = ConfigurationManager.AppSettings["diag:confmgr"];
            Type confMgrType;
            if (String.IsNullOrEmpty(confMgr)) {
                confMgrType = typeof(DefaultAppConfigurationManager);
            } else {
                confMgrType = Type.GetType(confMgr);
            }
            container.Register(typeof(IAppConfigurationManager), confMgrType);
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            // configure request-lifetime objects
        }

        protected override Nancy.Diagnostics.DiagnosticsConfiguration DiagnosticsConfiguration
        {
            get { return new DiagnosticsConfiguration { Password = "n4ncyBoard" }; }
        }
    }
}