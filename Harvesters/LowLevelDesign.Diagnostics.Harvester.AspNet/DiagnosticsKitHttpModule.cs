using LowLevelDesign.Diagnostics.Commons.Connectors;
using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Configuration;

namespace LowLevelDesign.Diagnostics.Harvester.AspNet
{
    public class DiagnosticsKitHttpModule : IHttpModule
    {
        private const int maxHttpDataLength = 1024;

        private static readonly HttpCastleConnector connector;
        private static readonly Process process = Process.GetCurrentProcess();

        static DiagnosticsKitHttpModule() {
            var diagurl = WebConfigurationManager.AppSettings["lowleveldesign.diagnostics.url"];
            Uri uri;
            if (!Uri.TryCreate(diagurl, UriKind.Absolute, out uri)) {
                throw new ConfigurationErrorsException("Please check lowleveldesign.diagnostics.url key in the appSettings - its value should contain an url " + 
                    "pointing to the diagnostics main application (Castle).");
            }
            connector = new HttpCastleConnector(uri);
        }

        public void Dispose() {
        }

        public void Init(HttpApplication context) {
            context.Error += context_Error;
        }

        private String ExtractHeaders(StringBuilder buffer, NameValueCollection headers) {
            buffer.Clear();
            try {
                foreach (String header in headers) {
                    buffer.AppendFormat("{0}={1}", header, headers[header]).AppendLine();
                }
            } catch (ArgumentOutOfRangeException) { /* it's normal if we exceed http data limit */ }

            return buffer.ToString();
        }

        void context_Error(object sender, EventArgs e) {
            var ctx = HttpContext.Current;
            var request = ctx.Request;
            var response = ctx.Response;
            var ex = ctx.Server.GetLastError();
            
            const String duplicateKey = "__llddiag_alreadyserved";
            if (ctx.Items.Contains(duplicateKey)) {
                return;
            }
            ctx.Items.Add(duplicateKey, true);


            var buffer = new StringBuilder(maxHttpDataLength, maxHttpDataLength);

            var logrec = new LogRecord {
                Identity = Thread.CurrentPrincipal.Identity.Name,
                LoggerName = "ASP.NET",
                LogLevel = LogRecord.ELogLevel.Error,
                ProcessId = process.Id,
                ProcessName = process.ProcessName,
                Server = Environment.MachineName,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                TimeUtc = DateTime.UtcNow,
                AdditionalFields = new Dictionary<String, Object> {
                    { "Url", request.RawUrl },
                    { "HttpStatusCode", response.StatusCode },
                    { "ClientIP", request.UserHostAddress },
                    { "RequestData", ExtractHeaders(buffer, request.Headers) },
                    { "ResponseData", ExtractHeaders(buffer, response.Headers) }
                }
            };

            try {
                logrec.ApplicationPath = HttpRuntime.AppDomainAppPath;
            } catch {
                logrec.ApplicationPath = AppDomain.CurrentDomain.BaseDirectory;
            }

            if (ex != null) {
                logrec.ExceptionType = ex.GetType().FullName;
                logrec.ExceptionMessage = ex.Message;
                logrec.ExceptionAdditionalInfo = ex.StackTrace;
            }

            connector.SendLogRecord(logrec);
        }
    }
}
