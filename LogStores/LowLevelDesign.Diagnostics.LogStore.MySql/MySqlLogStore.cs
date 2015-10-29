using Dapper;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Storage;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.MySql
{
    public class MySqlLogStore : ILogStore
    {
        public const String AppLogTablePrefix = "applog_";
        private static readonly ConcurrentDictionary<String, String> applicationMd5Hashes = new ConcurrentDictionary<String, String>(StringComparer.OrdinalIgnoreCase);

        private readonly LogTable logTable;

        public MySqlLogStore() : this(() => DateTime.UtcNow.Date) { }

        internal MySqlLogStore(Func<DateTime> currentUtcDateRetriever)
        {
            logTable = new LogTable(currentUtcDateRetriever);
        }

        public async Task AddLogRecord(LogRecord logrec)
        {
            using (var conn = new MySqlConnection(MySqlLogStoreConfiguration.ConnectionString)) {
                await conn.OpenAsync();
                await AddLogRecord(conn, null, logrec);
            }
        }

        public async Task AddLogRecords(IEnumerable<LogRecord> logrecs)
        {
            using (var conn = new MySqlConnection(MySqlLogStoreConfiguration.ConnectionString)) {
                await conn.OpenAsync();
                foreach (var logrec in logrecs) {
                    await AddLogRecord(conn, null, logrec);
                }
            }
        }

        private async Task AddLogRecord(MySqlConnection conn, MySqlTransaction tran, LogRecord logrec)
        {
            LastApplicationStatus appstat = null;
            var apphash = GetApplicationHash(logrec.ApplicationPath);

            await logTable.SaveLogRecord(conn, tran, AppLogTablePrefix + apphash, logrec);

            if (logrec.LogLevel >= LogRecord.ELogLevel.Error) {
                appstat = new LastApplicationStatus {
                    ApplicationPath = logrec.ApplicationPath,
                    Server = logrec.Server,
                    LastErrorTimeUtc = logrec.TimeUtc,
                    LastErrorType = logrec.ExceptionType
                };
            }

            // performance logs need to be handled differently
            if (logrec.PerformanceData != null && logrec.PerformanceData.Count > 0) {
                if (appstat == null) {
                    appstat = new LastApplicationStatus {
                        ApplicationPath = logrec.ApplicationPath,
                        Server = logrec.Server
                    };
                }
                appstat.LastUpdateTimeUtc = logrec.TimeUtc;
                float v;
                if (logrec.PerformanceData.TryGetValue("CPU", out v)) {
                    appstat.Cpu = v;
                }
                if (logrec.PerformanceData.TryGetValue("Memory", out v)) {
                    appstat.Memory = v;
                }
            }

            if (appstat != null) {
                // we need to update the application statuses table
                await logTable.UpdateApplicationStatus(conn, tran, apphash, appstat);
            }
        }

        public async Task<LogSearchResults> FilterLogs(LogSearchCriteria searchCriteria)
        {
            var hash = GetApplicationHash(searchCriteria.ApplicationPath);
            if (!LogTable.IsLogTableAvailable(AppLogTablePrefix + hash)) {
                return new LogSearchResults {
                    FoundItems = new LogRecord[0]
                };
            }

            if (String.IsNullOrEmpty(searchCriteria.ApplicationPath)) {
                throw new ArgumentException("ApplicationPath is required to filter the logs");
            }
            var whereSql = new StringBuilder("where TimeUtc >= convert(@FromUtc, datetime) and TimeUtc < convert(@ToUtc, datetime)");
            if (!String.IsNullOrEmpty(searchCriteria.Server)) {
                whereSql.Append(" and Server = @Server");
            }
            if (!String.IsNullOrEmpty(searchCriteria.Logger)) {
                whereSql.Append(" and LoggerName = @Logger");
            }
            if (searchCriteria.Levels != null && searchCriteria.Levels.Length > 0) {
                whereSql.Append(" and LogLevel in @Levels");
            }
            whereSql.Append(" order by TimeUtc desc").AppendFormat(" limit {0},{1}", searchCriteria.Offset, searchCriteria.Limit);

            using (var conn = new MySqlConnection(MySqlLogStoreConfiguration.ConnectionString)) {
                await conn.OpenAsync();
                var dbapplogs = (await conn.QueryAsync<DbAppLogRecord>(String.Format("select * from {0}{1} {2}", AppLogTablePrefix, hash, whereSql), searchCriteria)).ToArray();
                var applogs = new LogRecord[dbapplogs.Length];
                for (int i = 0; i < dbapplogs.Length; i++) {
                    var dbapplog = dbapplogs[i];
                    var applog = new LogRecord {
                        LoggerName = dbapplog.LoggerName,
                        LogLevel = dbapplog.LogLevel,
                        TimeUtc = dbapplog.TimeUtc,
                        Message = dbapplog.Message,
                        ExceptionType = dbapplog.ExceptionType,
                        ExceptionMessage = dbapplog.ExceptionMessage,
                        ExceptionAdditionalInfo = dbapplog.ExceptionAdditionalInfo,
                        CorrelationId = dbapplog.CorrelationId,
                        Server = dbapplog.Server,
                        ApplicationPath = dbapplog.ApplicationPath,
                        ProcessId = dbapplog.ProcessId,
                        ThreadId = dbapplog.ThreadId,
                        Identity = dbapplog.Identity,
                        AdditionalFields = new Dictionary<String, Object>()
                    };
                    applog.AdditionalFields.AddIfNotNull("Host", dbapplog.Host);
                    applog.AdditionalFields.AddIfNotNull("LoggedUser", dbapplog.LoggedUser);
                    applog.AdditionalFields.AddIfNotNull("HttpStatusCode", dbapplog.HttpStatusCode);
                    applog.AdditionalFields.AddIfNotNull("Url", dbapplog.Url);
                    applog.AdditionalFields.AddIfNotNull("Referer", dbapplog.Referer);
                    applog.AdditionalFields.AddIfNotNull("ClientIP", dbapplog.ClientIP);
                    applog.AdditionalFields.AddIfNotNull("RequestData", dbapplog.RequestData);
                    applog.AdditionalFields.AddIfNotNull("ResponseData", dbapplog.ResponseData);
                    applog.AdditionalFields.AddIfNotNull("ServiceName", dbapplog.ServiceName);
                    applog.AdditionalFields.AddIfNotNull("ServiceDisplayName", dbapplog.ServiceDisplayName);
                    applog.PerformanceData = dbapplog.PerfData != null ? JsonConvert.DeserializeObject<IDictionary<String, float>>(
                        dbapplog.PerfData) : null;

                    applogs[i] = applog;
                }
                return new LogSearchResults { FoundItems = applogs };
            }
        }

        public async Task<IEnumerable<LastApplicationStatus>> GetApplicationStatuses(DateTime lastDateUtcToQuery)
        {
            using (var conn = new MySqlConnection(MySqlLogStoreConfiguration.ConnectionString)) {
                await conn.OpenAsync();

                return await conn.QueryAsync<LastApplicationStatus>("select * from appstat where LastErrorTimeUtc >= @lastDateUtcToQuery or " +
                    "LastUpdateTimeUtc >= @lastDateUtcToQuery", new { lastDateUtcToQuery });
            }
        }

        public async Task Maintain(TimeSpan logsKeepTime, IDictionary<string, TimeSpan> logsKeepTimePerApplication = null)
        {
            if (logsKeepTime.TotalDays < 0 || logsKeepTimePerApplication != null &&
                logsKeepTimePerApplication.Values.Any(t => t.TotalDays < 0)) {
                throw new ArgumentException("Invalid timespan value for your logs");
            }

            // we need to match application paths with md5 hashes used in table names
            IDictionary<String, TimeSpan> hashesWithTimeouts = null;
            if (logsKeepTimePerApplication != null) {
                hashesWithTimeouts = new Dictionary<String, TimeSpan>(logsKeepTimePerApplication.Count, StringComparer.OrdinalIgnoreCase);
                foreach (var app in logsKeepTimePerApplication) {
                    hashesWithTimeouts.Add(GetApplicationHash(app.Key), app.Value);
                }
            }

            // then iterate through tables and manage their partitions
            using (var conn = new MySqlConnection(MySqlLogStoreConfiguration.ConnectionString)) {
                await conn.OpenAsync();

                var tables = await conn.QueryAsync<String>("select table_name from information_schema.tables where table_schema = @Database", conn);
                // make sure that all tables have current and future partitions created
                foreach (var tbl in tables) {
                    String hash;
                    if (tbl.StartsWith(AppLogTablePrefix, StringComparison.OrdinalIgnoreCase)) {
                        hash = tbl.Substring(AppLogTablePrefix.Length);
                    } else {
                        continue;
                    }

                    var partitions = await conn.QueryAsync<Partition>("select partition_name as Name from information_schema.partitions " +
                        "where table_schema = @Database and table_name = @TableName", new { conn.Database, TableName = tbl });

                    TimeSpan keepTime = logsKeepTime;
                    if (hashesWithTimeouts != null) {
                        if (hashesWithTimeouts.ContainsKey(hash)) {
                            // if not predefined time to keep logs found, use the default one
                            keepTime = hashesWithTimeouts[hash];
                        }
                    }
                    await logTable.ManageTablePartitions(conn, tbl, keepTime, partitions);
                }
            }
        }

        private String GetApplicationHash(String applicationPath)
        {
            String apphash;
            if (!applicationMd5Hashes.TryGetValue(applicationPath, out apphash)) {
                using (var md5 = MD5.Create()) {
                    apphash = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(applicationPath))).Replace("-", String.Empty);
                    applicationMd5Hashes.TryAdd(applicationPath, apphash);
                }
            }
            return apphash;
        }
    }
}
