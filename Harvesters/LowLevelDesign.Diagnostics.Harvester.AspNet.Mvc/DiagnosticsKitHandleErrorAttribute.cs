using LowLevelDesign.Diagnostics.Commons.Connectors;
using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace LowLevelDesign.Diagnostics.Harvester.AspNet.Mvc
{
    public sealed class DiagnosticsKitHandleErrorAttribute : HandleErrorAttribute
    {
        private static readonly HttpCastleConnector connector;
        private static readonly Process process;

        static DiagnosticsKitHandleErrorAttribute() {
            var diagurl = ConfigurationManager.AppSettings["lowleveldesign.diagnostics.url"];
            Uri uri;
            if (!Uri.TryCreate(diagurl, UriKind.Absolute, out uri)) {
                throw new ConfigurationErrorsException("Please check lowleveldesign.diagnostics.url key in the appSettings - its value should contain an url " + 
                    "pointing to the diagnostics main application (Castle).");
            }
            connector = new HttpCastleConnector(uri);
            process = Process.GetCurrentProcess();
        }

        public override void OnException(ExceptionContext filterContext) {
            if (filterContext.ExceptionHandled) {
                return;
            }
            var logrec = new LogRecord {
                ExceptionType = filterContext.Exception.GetType().FullName,
                ExceptionMessage = filterContext.Exception.Message,
                ExceptionAdditionalInfo = filterContext.Exception.StackTrace,
                Identity = Thread.CurrentPrincipal.Identity.Name,
                LoggerName = String.Format("MVC.{0}.{1}", filterContext.RouteData.Values["controller"], filterContext.RouteData.Values["action"]),
                LogLevel = "Error",
                ProcessId = process.Id,
                ProcessName = process.ProcessName,
                Server = Environment.MachineName,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                TimeUtc = DateTime.UtcNow,
                AdditionalFields = new Dictionary<String, Object> {
                    { "Url", filterContext.HttpContext.Request.RawUrl },
                    { "HttpStatusCode", filterContext.HttpContext.Response.StatusCode },
                    { "ClientIP", filterContext.HttpContext.Request.UserHostAddress },
                }
            };

            try {
                logrec.ApplicationPath = HttpRuntime.AppDomainAppPath;
            } catch {
                logrec.ApplicationPath = AppDomain.CurrentDomain.BaseDirectory;
            }

            connector.SendLogRecord(logrec);
        }
    }
}
