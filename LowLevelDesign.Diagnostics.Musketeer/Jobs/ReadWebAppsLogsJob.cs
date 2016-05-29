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

using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.Musketeer.Config;
using LowLevelDesign.Diagnostics.Musketeer.IIS;
using LowLevelDesign.Diagnostics.Musketeer.Models;
using NLog;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace LowLevelDesign.Diagnostics.Musketeer.Jobs
{
    [DisallowConcurrentExecution]
    public sealed class ReadWebAppsLogsJob : IJob
    {
        private const int NumberOfLogsPerApplicationToTriggerWarning = 50;

        private static readonly TimeSpan LogReloadInterval = TimeSpan.FromSeconds(Int32.Parse(ConfigurationManager.AppSettings["log-reload-interval-in-seconds"] ?? "60"));
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly Dictionary<string, W3CLogStream> logPathToAppLogStreamInfo = new Dictionary<string, W3CLogStream>(StringComparer.Ordinal);

        private readonly ISharedInfoAboutApps sharedAppsInfo;
        private readonly IMusketeerHttpCastleConnectorFactory castleConnectorFactory;

        public ReadWebAppsLogsJob(ISharedInfoAboutApps sharedAppsInfo, IMusketeerHttpCastleConnectorFactory castleConnectorFactory)
        {
            this.sharedAppsInfo = sharedAppsInfo;
            this.castleConnectorFactory = castleConnectorFactory;
        }

        public void Execute(IJobExecutionContext context)
        {
            // main loop for reading logs
            // reload streams from log files
            ReloadStreams();

            // read and send new logs to the castle
            ProcessNewLogRecords();
        }

        private void ReloadStreams()
        {
            var apps = sharedAppsInfo.GetApps();
            var unusedLogPaths = new HashSet<string>(logPathToAppLogStreamInfo.Keys, StringComparer.Ordinal);

            // for each app with a log we need to make sure that we have an opened log stream
            foreach (var app in apps) {
                W3CLogStream si;
                if (app.LogEnabled) {
                    if (!logPathToAppLogStreamInfo.TryGetValue(app.LogsPath, out si)) {
                        logger.Info("No log stream found for path: '{0}', creating one.", app.LogsPath);
                        // we need to add the new stream info object
                        logPathToAppLogStreamInfo.Add(app.LogsPath, new W3CLogStream(app.LogsPath));
                    } else {
                        unusedLogPaths.Remove(app.LogsPath);
                    }
                }
            }

            // dispose unused logs (this should be rare)
            foreach (var logPath in unusedLogPaths) {
                logger.Info("Found unused log file: '{0}' - closing it.", logPath);
                var l = logPathToAppLogStreamInfo[logPath];
                logPathToAppLogStreamInfo.Remove(logPath);
                l.Dispose();
            }
        }

        private void ProcessNewLogRecords()
        {
            var logrecs = new List<LogRecord>(40);
            foreach (var logStream in logPathToAppLogStreamInfo.Values) {
                // for each of them filter the acquired lines and send them to the Castle
                var newLogRecords = logStream.ReadLogRecords().ToList();
                logger.Debug("Read {0} new records from log: '{1}'", newLogRecords.Count, logStream.CurrentlyProcessedLogFilePath);

                if (newLogRecords.Count > 0) {
                    // get all apps for a given log path
                    var apps = sharedAppsInfo.FindAppsByLogsPath(logStream.LogsFolderPath);
                    foreach (var app in apps) {
                        logrecs.AddRange(FilterAndConvertLogs(app, newLogRecords));
                    }
                }
            }

            if (logrecs.Count > 0) {
                // send collected logs to Diagnostics Castle
                using (var castleConnector = castleConnectorFactory.CreateCastleConnector()) {
                    castleConnector.SendLogRecords(logrecs);
                }
            }
        }

        private IEnumerable<LogRecord> FilterAndConvertLogs(AppInfo app, IEnumerable<W3CEvent> logs)
        {
            if (app.LogFilter == null) {
                return logs.Select(l => Map(app, l));
            }
            var recs = new List<LogRecord>(100);
            int numberOfLogsToSend = 0;
            foreach (var log in logs) {
                if (log.cs_uri_stem != null && log.cs_uri_stem.StartsWith(app.LogFilter, StringComparison.OrdinalIgnoreCase)) {
                    if (!MusketeerConfiguration.ShouldSendSuccessHttpLogs && IsSuccessHttpStatus(log.sc_status)) {
                        continue;
                    }
                    recs.Add(Map(app, log));
                    if (++numberOfLogsToSend == NumberOfLogsPerApplicationToTriggerWarning) {
                        logger.Warn("The number of log records for application '{0}' is higher than the allowed maximum and will be truncated to {1} before " +
                            "sending to the Castle. To avoid this situation please make the job run more often (job:iis-logs-read-cron parameter) or exclude success " +
                            "status responses (include-http-success-logs parameter).", app.Path, NumberOfLogsPerApplicationToTriggerWarning);
                        break;
                    }
                }
            }
            if (recs.Count > 0) {
                logger.Debug("Found {0} log records for application: '{1}'", recs.Count, app.Path);
            }
            return recs;
        }

        private bool IsSuccessHttpStatus(string httpStatus)
        {
            if (httpStatus == null) {
                return false;
            }
            return httpStatus.StartsWith("20", StringComparison.Ordinal) || httpStatus.StartsWith("30", StringComparison.Ordinal);
        }

        private LogRecord Map(AppInfo app, W3CEvent ev)
        {
            var lvl = LogRecord.ELogLevel.Info;
            if (ev.sc_status.StartsWith("4", StringComparison.Ordinal)) {
                lvl = LogRecord.ELogLevel.Warning;
            } else if (ev.sc_status.StartsWith("5", StringComparison.Ordinal)) {
                lvl = LogRecord.ELogLevel.Error;
            }

            return new LogRecord {
                TimeUtc = ev.dateTime,
                ApplicationPath = app.Path,
                LoggerName = "IISLog",
                LogLevel = lvl,
                ProcessId = app.ProcessIds.FirstOrDefault(),
                Server = SharedInfoAboutApps.MachineName,
                AdditionalFields = new Dictionary<string, object>() {
                    { "HttpStatusCode", string.Format("{0}{1}{2}", ev.sc_status, string.IsNullOrEmpty(ev.sc_substatus) ? string.Empty : "." + ev.sc_substatus, 
                                string.IsNullOrEmpty(ev.sc_win32_status) ? string.Empty : "." + ev.sc_win32_status) },
                    { "ClientIP", ev.c_ip },
                    { "Url", string.Format("{0}{1}{2}", ev.cs_uri_stem, ev.cs_uri_query != null ? "?" : string.Empty, ev.cs_uri_query) }
                }
            };
        }

        public static void CleanupStreams()
        {
            logger.Info("Disposing streams and finishing work...");
            foreach (var logStream in logPathToAppLogStreamInfo.Values) {
                try {
                    logStream.Dispose();
                } catch (Exception ex) {
                    logger.Warn(ex, "Exception occured while dispoing the stream for a path: '{0}'", logStream.LogsFolderPath);
                }
            }
        }
    }
}
