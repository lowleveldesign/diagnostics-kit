﻿using LowLevelDesign.Diagnostics.Commons.Connectors;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.Commons;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Diagnostics;
using System.Threading;
using NLog.Common;

namespace LowLevelDesign.Diagnostics.Harvester.NLog
{
    [Target("DiagnosticsKit")]
    public class DiagnosticsKitTarget : TargetWithLayout
    {
        [RequiredParameter]
        public String DiagnosticsCastleUrl { get; set; }

        private readonly Process process = Process.GetCurrentProcess();
        private HttpCastleConnector connector;

        protected override void InitializeTarget() {
            base.InitializeTarget();

            connector = new HttpCastleConnector(new Uri(DiagnosticsCastleUrl));
        }

        protected override void Write(LogEventInfo logEvent) {
            var thread = Thread.CurrentThread;

            var logrec = new LogRecord {
                TimeUtc = DateTime.UtcNow,
                LoggerName = logEvent.LoggerName,
                LogLevel = ConvertToLogRecordLevel(logEvent.Level),
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

        private static LogRecord.ELogLevel ConvertToLogRecordLevel(LogLevel lvl) {
            if (lvl != null) {
                if (lvl >= LogLevel.Fatal) {
                    return LogRecord.ELogLevel.Critical;
                }
                if (lvl >= LogLevel.Error) {
                    return LogRecord.ELogLevel.Error;
                }
                if (lvl >= LogLevel.Warn) {
                    return LogRecord.ELogLevel.Warning;
                }
                if (lvl >= LogLevel.Info) {
                    return LogRecord.ELogLevel.Info;
                }
                if (lvl >= LogLevel.Debug) {
                    return LogRecord.ELogLevel.Debug;
                }
            }
            return LogRecord.ELogLevel.Trace;
        }

        protected override void CloseTarget() {
            base.CloseTarget();

            // dispose the connector
            connector.Dispose();
        }
    }
}