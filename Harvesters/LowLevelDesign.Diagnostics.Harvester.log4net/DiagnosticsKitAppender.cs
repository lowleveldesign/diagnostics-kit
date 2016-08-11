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
                ApplicationPath = AppDomain.CurrentDomain.BaseDirectory,
                ProcessId = process.Id,
                ProcessName = process.ProcessName,
                ThreadId = thread.ManagedThreadId,
                Identity = Thread.CurrentPrincipal.Identity.Name
            };

            if (loggingEvent.ExceptionObject != null) {
                logrec.ExceptionType = loggingEvent.ExceptionObject.GetType().FullName;
                logrec.ExceptionMessage = loggingEvent.ExceptionObject.Message;
                logrec.ExceptionAdditionalInfo = loggingEvent.ExceptionObject.StackTrace.ShortenIfNecessary(7000);
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
