using LowLevelDesign.Diagnostics.Commons.Connectors;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.Commons;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Diagnostics;
using System.Threading;

namespace LowLevelDesign.Diagnostics.Harvester.NLog
{
    [Target("DiagnosticsCastle")]
    public class DiagnosticsCastleTarget : TargetWithLayout
    {
        [RequiredParameter]
        public String DiagnosticsCastleUrl { get; set; }


        private HttpCastleConnector connector;
        protected override void InitializeTarget() {
            base.InitializeTarget();

            connector = new HttpCastleConnector(new Uri(DiagnosticsCastleUrl));
        }

        protected override void Write(LogEventInfo logEvent) {
            var process = Process.GetCurrentProcess();
            var thread = Thread.CurrentThread;

            var logrec = new LogRecord {
                TimeUtc = DateTime.UtcNow,
                LoggerName = logEvent.LoggerName,
                LogLevel = logEvent.Level.Name,
                Message = logEvent.Message,
                Server = Environment.MachineName,
                ApplicationPath = AppDomain.CurrentDomain.BaseDirectory, // TODO: check if it's a valid approach
                ProcessId = process.Id,
                ProcessName = process.ProcessName,
                ThreadId = thread.ManagedThreadId,
                Identity = Thread.CurrentPrincipal.Identity.Name
            };

            if (logEvent.Exception != null) {
                logrec.ExceptionType = logEvent.Exception.GetType().FullName;
                logrec.ExceptionMessage = logEvent.Exception.Message;
                logrec.ExceptionAdditionalInfo = logEvent.Exception.StackTrace.ShortenIfNecessary(5000);
            }

            connector.SendLogRecord(logrec);
        }

        protected override void CloseTarget() {
            base.CloseTarget();

            // dispose the connector
            connector.Dispose();
        }
    }
}
