/**
 *  Part of the Diagnostics Kit
 *
 *  Copyright (C) 2016  Sebastian Solnica
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 */

using FluentValidation;
using LowLevelDesign.Diagnostics.Castle.Caching;
using LowLevelDesign.Diagnostics.Castle.Config;
using LowLevelDesign.Diagnostics.Castle.Logs;
using LowLevelDesign.Diagnostics.Castle.Models;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.Commons.Validators;
using LowLevelDesign.Diagnostics.LogStore.Commons.Config;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Validators;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.Diagnostics;
using Nancy.Json;
using Nancy.TinyIoc;
using System.Diagnostics;

namespace LowLevelDesign.Diagnostics.Castle
{
    public class Bootstraper : DefaultNancyBootstrapper
    {
        private static readonly TraceSource logger = new TraceSource("LowLevelDesign.Diagnostics.Castle");

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            // We will send big JSON payloads
            JsonSettings.MaxJsonLength = int.MaxValue;

            // Authentication
            var authconf = new StatelessAuthenticationConfiguration(ctx => {
                var principal = ctx.GetFromOwinContext<ApplicationSignInManager>().AuthenticationManager.User;
                return principal.Identity.IsAuthenticated ? new AuthenticatedUser(principal) : null;
            });
            StatelessAuthentication.Enable(pipelines, authconf);

#if !DEBUG
            // disable diagnostics in release version
            DiagnosticsHook.Disable(pipelines);
            container.Register<IDiagnostics, DisabledDiagnostics>();

            StaticConfiguration.DisableErrorTraces = true;
#endif

            pipelines.OnError += (ctx, err) => {
                logger.TraceEvent(TraceEventType.Error, 0, "Global application error occurred when serving request: {0}, ex: {1}", ctx.Request.Url, err);
                return null;
            };

            // make sure that we have partitions to store the coming logs
            var logmaintain = container.Resolve<ILogMaintenance>();
            logmaintain.PerformMaintenanceIfNecessaryAsync().Wait();
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            /* LOG STORE */
            container.Register(AppSettings.GetLogStore());

            /* LOGS MAINTENANCE */
            container.Register<ILogMaintenance, LogMaintenance>();

            /* VALIDATORS */
            container.Register<IValidator<LogRecord>, LogRecordValidator>();
            container.Register<IValidator<Application>, ApplicationValidator>();
            container.Register<IValidator<ApplicationServerConfig>, ApplicationServerConfigValidator>();

            /* CONFIGURATION */
            container.Register<IAppConfigurationManager>(new AppConfigurationManagerWrapper(AppSettings.GetAppConfigurationManager()));
            container.Register<GlobalConfig>();

            /* SECURITY */
            container.Register(AppSettings.GetAppUserManager());
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            // configure request-lifetime objects
        }

        protected override DiagnosticsConfiguration DiagnosticsConfiguration
        {
            get { return new DiagnosticsConfiguration { Password = "n4ncyBoard" }; }
        }

        protected override void ConfigureConventions(NancyConventions conventions)
        {
            base.ConfigureConventions(conventions);

            conventions.StaticContentsConventions.AddFile("/favicon.png", "/Content/favicon.png");
        }
    }
}
