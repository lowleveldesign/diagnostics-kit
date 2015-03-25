using log4net.Appender;
using log4net.Core;
using LowLevelDesign.Diagnostics.Commons;
using LowLevelDesign.Diagnostics.Commons.Connectors;
using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Diagnostics;
using System.Threading;

namespace LowLevelDesign.Diagnostics.Harvester.log4net
{
    public class DiagnosticsKitAppender : AppenderSkeleton
    {
        public String DiagnosticsCastleUrl { get; set; }

        private HttpCastleConnector connector;
        public override void ActivateOptions() {
            base.ActivateOptions();

            connector = new HttpCastleConnector(new Uri(DiagnosticsCastleUrl));
        }

        protected override void Append(LoggingEvent loggingEvent) {
            var process = Process.GetCurrentProcess();
            var thread = Thread.CurrentThread;

            var logrec = new LogRecord {
                TimeUtc = DateTime.UtcNow,
                LoggerName = loggingEvent.LoggerName,
                LogLevel = loggingEvent.Level.Name,
                Message = loggingEvent.RenderedMessage,
                Server = Environment.MachineName,
                ApplicationPath = AppDomain.CurrentDomain.BaseDirectory, // TODO: check if it's a valid approach
                ProcessId = process.Id,
                ProcessName = process.ProcessName,
                ThreadId = thread.ManagedThreadId,
                Identity = Thread.CurrentPrincipal.Identity.Name
            };

            if (loggingEvent.ExceptionObject != null) {
                logrec.ExceptionType = loggingEvent.ExceptionObject.GetType().FullName;
                logrec.ExceptionMessage = loggingEvent.ExceptionObject.Message;
                logrec.ExceptionAdditionalInfo = loggingEvent.ExceptionObject.StackTrace.ShortenIfNecessary(5000);
            }

            connector.SendLogRecord(logrec);
        }

        protected override void OnClose() {
            base.OnClose();

            // dispose the connector
            connector.Dispose();
        }
    }
}
