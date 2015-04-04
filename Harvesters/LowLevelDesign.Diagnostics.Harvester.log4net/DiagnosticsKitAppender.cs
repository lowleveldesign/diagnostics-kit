using log4net.Appender;
using log4net.Core;
using LowLevelDesign.Diagnostics.Commons;
using LowLevelDesign.Diagnostics.Commons.Connectors;
using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace LowLevelDesign.Diagnostics.Harvester.log4net
{
    public class DiagnosticsKitAppender : AppenderSkeleton
    {
        public String DiagnosticsCastleUrl { get; set; }

        private readonly Process process = Process.GetCurrentProcess();
        private HttpCastleConnector connector;
        public override void ActivateOptions() {
            base.ActivateOptions();

            connector = new HttpCastleConnector(new Uri(DiagnosticsCastleUrl));
        }

        protected override void Append(LoggingEvent[] loggingEvents) {
            var logrecs = loggingEvents.Select(ConvertLogEventToLogRecord);
            connector.SendLogRecords(logrecs);
        }

        protected override void Append(LoggingEvent loggingEvent) {
            connector.SendLogRecord(ConvertLogEventToLogRecord(loggingEvent));
        }

        private LogRecord ConvertLogEventToLogRecord(LoggingEvent loggingEvent) {
            var thread = Thread.CurrentThread;

            var logrec = new LogRecord {
                TimeUtc = DateTime.UtcNow,
                LoggerName = loggingEvent.LoggerName,
                LogLevel = ConvertToLogRecordLevel(loggingEvent.Level),
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

            return logrec;
        }

        private static LogRecord.ELogLevel ConvertToLogRecordLevel(Level lvl) {
            if (lvl != null) {
                if (lvl >= Level.Critical) {
                    return LogRecord.ELogLevel.Critical;
                }
                if (lvl >= Level.Error) {
                    return LogRecord.ELogLevel.Error;
                }
                if (lvl >= Level.Warn) {
                    return LogRecord.ELogLevel.Warning;
                }
                if (lvl >= Level.Info) {
                    return LogRecord.ELogLevel.Info;
                }
                if (lvl >= Level.Debug) {
                    return LogRecord.ELogLevel.Debug;
                }
            }
            return LogRecord.ELogLevel.Trace;
        }

        protected override void OnClose() {
            base.OnClose();

            // dispose the connector
            connector.Dispose();
        }
    }
}
