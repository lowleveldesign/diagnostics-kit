using LowLevelDesign.Diagnostics.Commons.Connectors;
using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Diagnostics;
using System.Threading;

namespace LowLevelDesign.Diagnostics.Harvester.SystemDiagnostics
{
    public class DiagnosticsKitTraceListener : TraceListener
    {
        private readonly Process process = Process.GetCurrentProcess();
        private readonly HttpCastleConnector connector;

        public DiagnosticsKitTraceListener(String diagnosticsCastleUri) {
            connector = new HttpCastleConnector(new Uri(diagnosticsCastleUri));
        }

        public override void Fail(string message, string detailMessage) {
            SendLogRecord("Trace", TraceEventType.Critical, 0, message, null);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id) {
            SendLogRecord(source, eventType, id, null, eventCache);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args) {
            SendLogRecord(source, eventType, id, String.Format(format, args), eventCache);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message) {
            SendLogRecord(source, eventType, id, message, eventCache);
        }

        public override void Write(string message) {
            SendLogRecord("Trace", TraceEventType.Information, 0, message, null);
        }

        public override void WriteLine(string message) {
            SendLogRecord("Trace", TraceEventType.Information, 0, message, null);
        }

        private void SendLogRecord(String source, TraceEventType eventType, int id, String message, TraceEventCache eventCache) {
            var logrec = new LogRecord {
                TimeUtc = DateTime.UtcNow,
                LoggerName = source,
                Message = message,
                Server = Environment.MachineName,
                ApplicationPath = AppDomain.CurrentDomain.BaseDirectory, // TODO: check if it's a valid approach
                ProcessId = process.Id,
                ProcessName = process.ProcessName,
                Identity = Thread.CurrentPrincipal.Identity.Name
            };

            if (id != 0) {
                logrec.Message = String.Format(":{0} {1}", id, message);
            }

            if (eventCache == null) {
                var thread = Thread.CurrentThread;
                logrec.ThreadId = thread.ManagedThreadId;
            } else {
                int threadId;
                if (Int32.TryParse(eventCache.ThreadId, out threadId)) {
                    logrec.ThreadId = threadId;
                }
            }

            if (eventType <= TraceEventType.Critical) {
                logrec.LogLevel = LogRecord.ELogLevel.Critical;
            } else if (eventType <= TraceEventType.Error) {
                logrec.LogLevel = LogRecord.ELogLevel.Error;
            } else if (eventType <= TraceEventType.Warning) {
                logrec.LogLevel = LogRecord.ELogLevel.Warning;
            } else if (eventType <= TraceEventType.Information) {
                logrec.LogLevel = LogRecord.ELogLevel.Info;
            } else if (eventType <= TraceEventType.Verbose) {
                logrec.LogLevel = LogRecord.ELogLevel.Debug;
            } else {
                logrec.LogLevel = LogRecord.ELogLevel.Trace;
            }

            connector.SendLogRecord(logrec);
        }

        public override void Close() {
            base.Close();

            connector.Dispose();
        }

    }
}
