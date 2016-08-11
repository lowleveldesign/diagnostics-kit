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

using LowLevelDesign.Diagnostics.Commons.Connectors;
using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Diagnostics;
using System.Text;
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

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            SendLogRecord(source, eventType, id, data != null ? data.ToString() : null, eventCache);
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            var message = new StringBuilder();
            if (data != null) {
                message.Append(data).Append(',');
            }
            SendLogRecord(source, eventType, id, message.ToString(), eventCache);
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
                ApplicationPath = AppDomain.CurrentDomain.BaseDirectory,
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
