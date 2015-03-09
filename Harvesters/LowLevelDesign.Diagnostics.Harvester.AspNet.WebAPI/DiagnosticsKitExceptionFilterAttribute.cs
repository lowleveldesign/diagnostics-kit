using LowLevelDesign.Diagnostics.Commons.Connectors;
using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace LowLevelDesign.Diagnostics.Harvester.AspNet.WebAPI
{
    public sealed class DiagnosticsKitExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private static readonly HttpCastleConnector connector;
        private static readonly Process process;

        static DiagnosticsKitExceptionFilterAttribute() {
            var diagurl = ConfigurationManager.AppSettings["lowleveldesign.diagnostics.url"];
            Uri uri;
            if (!Uri.TryCreate(diagurl, UriKind.Absolute, out uri)) {
                throw new ConfigurationErrorsException("Please check lowleveldesign.diagnostics.url key in the appSettings - its value should contain an url " + 
                    "pointing to the diagnostics main application (Castle).");
            }
            connector = new HttpCastleConnector(uri);
            process = Process.GetCurrentProcess();
        }

        public override void OnException(HttpActionExecutedContext context)
        {
            var logrec = new LogRecord {
                ApplicationPath = AppDomain.CurrentDomain.BaseDirectory,
                ExceptionType = context.Exception.GetType().FullName,
                ExceptionMessage = context.Exception.Message,
                ExceptionAdditionalInfo = context.Exception.StackTrace,
                Identity = Thread.CurrentPrincipal.Identity.Name,
                LoggerName = String.Format("WebAPI.{0}.{1}", context.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName, 
                                                context.ActionContext.ActionDescriptor.ActionName),
                LogLevel = "Error",
                Message = null,
                ProcessId = process.Id,
                ProcessName = process.ProcessName,
                Server = Environment.MachineName,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                TimeUtc = DateTime.UtcNow,
                AdditionalFields = new Dictionary<String, Object> {
                    { "Url", context.Request.RequestUri.AbsoluteUri },
                    { "Host", context.Request.Headers.Host },
                    { "HttpStatusCode", context.Response.StatusCode },
                }
            };
            connector.SendLogRecord(logrec);
        }
    }
}
