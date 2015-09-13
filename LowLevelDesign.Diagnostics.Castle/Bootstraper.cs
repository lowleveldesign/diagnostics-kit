using FluentValidation;
using LowLevelDesign.Diagnostics.Castle.Logs;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.Commons.Validators;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Diagnostics;
using Nancy.TinyIoc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using Nancy.Conventions;
using Nancy.Responses;
using LowLevelDesign.Diagnostics.LogStore.Commons.Storage;
using LowLevelDesign.Diagnostics.LogStore.Commons.Validators;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Config;

namespace LowLevelDesign.Diagnostics.Castle
{
    public class Bootstraper : DefaultNancyBootstrapper
    {
        private const String LogStoreKey = "diag:logstore";
        private const String ConfMgrKey = "diag:confmgr";
        // Diagnostics defaults - we should use them only if other options are not available
        private const String defaultTypesNamespace = "LowLevelDesign.Diagnostics.LogStore.Defaults";

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
#if !DEBUG
            DiagnosticsHook.Disable(pipelines);
#endif
            // Logging configuration
            Log.Logger = new LoggerConfiguration().WriteTo.Trace().CreateLogger();

            pipelines.OnError += (ctx, err) => {
                Log.Error(err, "Global application error occurred when serving request: {0}", ctx.Request.Url);
                return null;
            };

            // make sure that we have partitions to store the coming logs
            var logmaintain = container.Resolve<ILogMaintenance>();
            logmaintain.PerformMaintenanceIfNecessaryAsync().Wait();
        }

        private Type FindSingleTypeInLowLevelDesignAssemblies(Type typeToImplement, String confkey)
        {
            var implementers = new List<Type>();
            var bindir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
            foreach (var asmpath in Directory.GetFiles(bindir, "LowLevelDesign.*.dll")) {
                try {
                    var asm = Assembly.LoadFrom(asmpath);
                    implementers.AddRange(asm.GetExportedTypes().Where(t => t.GetInterfaces().Contains(typeToImplement)));
                } catch (Exception ex) {
                    // just swallow and check the next one
                    Log.Debug(ex, "Failure while loading assembly from '{0}'", asmpath);
                }
                if (implementers.Count > 1) {
                    // we may skip the default one if present
                    var ind = implementers.FindIndex(t => t.Namespace.StartsWith(defaultTypesNamespace, StringComparison.Ordinal));
                    if (ind >= 0) {
                        implementers.RemoveAt(ind);
                    }
                    if (implementers.Count > 1) {
                        throw new ConfigurationErrorsException("More than one class implementing " + typeToImplement.FullName +
                            ". Please specify which one should be used by adding '" + confkey + " ' " +
                            "key in the appsettings. Please check documentation if in doubt.");
                    }
                }
            }
            if (implementers.Count == 0) {
                // no log store found
                throw new ConfigurationErrorsException("No class implementing " + typeToImplement.FullName + " found. Please add at least one " +
                    "assembly where we coud find such a class. Please check documentation if in doubt.");
            }
            return implementers[0];
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            /* LOG STORAGE */
            var logstoreTypeName = ConfigurationManager.AppSettings[LogStoreKey];
            Type logstoreType;
            if (logstoreTypeName == null) {
                logstoreType = FindSingleTypeInLowLevelDesignAssemblies(typeof(ILogStore), LogStoreKey);
            } else {
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
            if (confMgrTypeName == null) {
                confMgrType = FindSingleTypeInLowLevelDesignAssemblies(typeof(IAppConfigurationManager), ConfMgrKey);
            } else {
                confMgrType = Type.GetType(confMgrTypeName);
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

        protected override void ConfigureConventions(NancyConventions conventions)
        {
            base.ConfigureConventions(conventions);
        }
    }
}