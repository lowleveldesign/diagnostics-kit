using FluentValidation;
using LowLevelDesign.Diagnostics.Castle.Config;
using LowLevelDesign.Diagnostics.Castle.Logs;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.Commons.Validators;
using LowLevelDesign.Diagnostics.LogStore.Commons.Auth;
using LowLevelDesign.Diagnostics.LogStore.Commons.Config;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Storage;
using LowLevelDesign.Diagnostics.LogStore.Commons.Validators;
using Microsoft.AspNet.Identity;
using Nancy;
using Nancy.Owin;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.Diagnostics;
using Nancy.TinyIoc;
using Serilog;
using System;
using System.Configuration;

namespace LowLevelDesign.Diagnostics.Castle
{
    public class Bootstraper : DefaultNancyBootstrapper
    {
        private const String LogStoreKey = "diag:logstore";
        private const String ConfMgrKey = "diag:confmgr";

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
#if !DEBUG
            StaticConfiguration.DisableErrorTraces = false;
            DiagnosticsHook.Disable(pipelines);
#endif
            pipelines.OnError += (ctx, err) => {
                Log.Error(err, "Global application error occurred when serving request: {0}", ctx.Request.Url);
                return null;
            };

            // make sure that we have partitions to store the coming logs
            var logmaintain = container.Resolve<ILogMaintenance>();
            logmaintain.PerformMaintenanceIfNecessaryAsync().Wait();
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            /* LOG STORAGE */
            var logstoreTypeName = ConfigurationManager.AppSettings[LogStoreKey];
            Type logstoreType;
            if (logstoreTypeName == null)
            {
                logstoreType = AppSettings.FindSingleTypeInLowLevelDesignAssemblies(typeof(ILogStore), LogStoreKey);
            }
            else
            {
                logstoreType = Type.GetType(logstoreTypeName);
            }
            container.Register(typeof(ILogStore), logstoreType);

            /* LOGS MAINTENANCE */
            container.Register<ILogMaintenance, LogMaintenance>();

            /* VALIDATORS */
            container.Register<IValidator<LogRecord>, LogRecordValidator>();
            container.Register<IValidator<Application>, ApplicationValidator>();
            container.Register<IValidator<ApplicationServerConfig>, ApplicationServerConfigValidator>();

            /* CONFIGURATION */
            var confMgrTypeName = ConfigurationManager.AppSettings[ConfMgrKey];
            Type confMgrType;
            if (confMgrTypeName == null)
            {
                confMgrType = AppSettings.FindSingleTypeInLowLevelDesignAssemblies(typeof(IAppConfigurationManager), ConfMgrKey);
            }
            else
            {
                confMgrType = Type.GetType(confMgrTypeName);
            }
            container.Register(typeof(IAppConfigurationManager), confMgrType);
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            // configure request-lifetime objects
            /* SECURITY */
            container.Register<IAppUserManager>((c, p) => {
                return (IAppUserManager)context.GetOwinEnvironment()["AspNet.Identity.Owin:" + 
                    AuthSettings.UserManagerType.AssemblyQualifiedName]; // get by recompiling Microsoft.Aspnet.Identity.Owin
            });
        }

        protected override Nancy.Diagnostics.DiagnosticsConfiguration DiagnosticsConfiguration
        {
            get { return new DiagnosticsConfiguration { Password = "n4ncyBoard" }; }
        }

        protected override void ConfigureConventions(NancyConventions conventions)
        {
            base.ConfigureConventions(conventions);
        }
    }
}