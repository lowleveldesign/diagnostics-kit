using LowLevelDesign.Diagnostics.Commons.Connectors;
using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace LowLevelDesign.Diagnostics.Harvester.AspNet.WebAPI
{
    public sealed class DiagnosticsKitExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private const int maxHttpDataLength = 1024;

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

        private String ExtractHeaders(StringBuilder buffer, HttpHeaders headers) {
            if (headers == null) {
                return String.Empty;
            }

            buffer.Clear();
            try {
                foreach (var header in headers) {
                    buffer.AppendFormat("{0}={1}", header.Key, header.Value.FirstOrDefault()).AppendLine();
                }
            } catch (ArgumentOutOfRangeException) { /* it's normal if we exceed http data limit */ }

            return buffer.ToString();
        }

        public override void OnException(HttpActionExecutedContext context)
        {
            const String duplicateKey = "__llddiag_alreadyserved";
            if (context.Request.Properties.ContainsKey(duplicateKey)) {
                return;
            }
            context.Request.Properties.Add(duplicateKey, true);

            var request = context.Request;
            var ex = context.Exception;

            var buffer = new StringBuilder(maxHttpDataLength, maxHttpDataLength);

            var logrec = new LogRecord {
                ApplicationPath = AppDomain.CurrentDomain.BaseDirectory,
                ExceptionType = ex.GetType().FullName,
                ExceptionMessage = ex.Message,
                ExceptionAdditionalInfo = ex.StackTrace,
                Identity = Thread.CurrentPrincipal.Identity.Name,
                LoggerName = String.Format("WebAPI.{0}.{1}", context.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName, 
                                                context.ActionContext.ActionDescriptor.ActionName),
                LogLevel = LogRecord.ELogLevel.Error,
                Message = null,
                ProcessId = process.Id,
                ProcessName = process.ProcessName,
                Server = Environment.MachineName,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                TimeUtc = DateTime.UtcNow,
                AdditionalFields = new Dictionary<String, Object> {
                    { "Url", request.RequestUri.AbsoluteUri },
                    { "Host", request.Headers.Host },
                    { "RequestData", ExtractHeaders(buffer, request.Headers) },
                }
            };

            var response = context.Response;
            if (response != null) {
                logrec.AdditionalFields.Add("HttpStatusCode", response.StatusCode);
                logrec.AdditionalFields.Add("ResponseData", ExtractHeaders(buffer, response.Headers));
            }

            connector.SendLogRecord(logrec);
        }
    }
}
