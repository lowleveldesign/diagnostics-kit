using Dapper;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.Defaults
{
    public abstract class DefaultLogStore : ILogStore
    {
        public const string AppLogTablePrefix = "ApplicationLog";

        private static readonly ConcurrentDictionary<string, string> applicationMd5Hashes = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        protected readonly ILogTable logTable;

        protected DefaultLogStore(ILogTable logTable)
        {
            this.logTable = logTable;
        }

        protected abstract IDbConnection CreateConnection();

        public virtual async Task AddLogRecordAsync(LogRecord logrec)
        {
            using (var conn = CreateConnection()) {
                conn.Open();
                await AddLogRecordAsync(conn, null, logrec);
            }
        }

        public virtual async Task AddLogRecordsAsync(IEnumerable<LogRecord> logrecs)
        {
            using (var conn = CreateConnection()) {
                conn.Open();
                foreach (var logrec in logrecs) {
                    await AddLogRecordAsync(conn, null, logrec);
                }
            }
        }

        private async Task AddLogRecordAsync(IDbConnection conn, IDbTransaction tran, LogRecord logrec)
        {
            var apphash = GetApplicationHash(logrec.ApplicationPath);
            await logTable.SaveLogRecord(conn, tran, AppLogTablePrefix + apphash, logrec);
        }


        public abstract Task<LogSearchResults> FilterLogsAsync(LogSearchCriteria searchCriteria);

        protected string PrepareWhereSectionOfTheQuery(LogSearchCriteria searchCriteria)
        {
            Debug.Assert(searchCriteria.Keywords != null);
            var whereSql = new StringBuilder("where TimeUtc >= cast(@FromUtc as datetime) and TimeUtc < cast(@ToUtc as datetime)");
            if (!string.IsNullOrEmpty(searchCriteria.Server)) {
                whereSql.Append(" and Server = @Server");
            }
            if (!string.IsNullOrEmpty(searchCriteria.Logger)) {
                whereSql.Append(" and LoggerName like @Logger");
            }
            if (searchCriteria.Levels != null && searchCriteria.Levels.Length > 0) {
                whereSql.Append(" and LogLevel in @Levels");
            }
            if (!string.IsNullOrEmpty(searchCriteria.Keywords.HttpStatus)) {
                whereSql.Append(" and HttpStatusCode like @HttpStatus");
            }
            if (!string.IsNullOrEmpty(searchCriteria.Keywords.Url)) {
                whereSql.Append(" and Url like @Url");
            }
            if (!string.IsNullOrEmpty(searchCriteria.Keywords.ClientIp)) {
                whereSql.Append(" and ClientIP like @ClientIp");
            }
            if (!string.IsNullOrEmpty(searchCriteria.Keywords.Service)) {
                whereSql.Append(" and ServiceName like @ServiceName");
            }
            return whereSql.ToString();
        }

        protected LogRecord[] ConvertDbLogRecordToPublicModel(DbAppLogRecord[] dbapplogs)
        {
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
                    AdditionalFields = new Dictionary<string, Object>()
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
                applog.PerformanceData = dbapplog.PerfData != null ? JsonConvert.DeserializeObject<IDictionary<string, float>>(
                    dbapplog.PerfData) : null;

                applogs[i] = applog;
            }
            return applogs;
        }

        protected string GetApplicationHash(string applicationPath)
        {
            string apphash;
            if (!applicationMd5Hashes.TryGetValue(applicationPath, out apphash)) {
                using (var md5 = MD5.Create()) {
                    apphash = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(applicationPath))).Replace("-", string.Empty);
                    applicationMd5Hashes.TryAdd(applicationPath, apphash);
                }
            }
            return apphash;
        }

        public virtual async Task<IEnumerable<LastApplicationStatus>> GetApplicationStatusesAsync(DateTime lastDateUtcToQuery)
        {
            using (var conn = CreateConnection()) {
                conn.Open();

                return await conn.QueryAsync<LastApplicationStatus>("select * from ApplicationStatus where LastErrorTimeUtc >= @lastDateUtcToQuery or " +
                    "LastUpdateTimeUtc >= @lastDateUtcToQuery", new { lastDateUtcToQuery });
            }
        }

        public virtual async Task UpdateApplicationStatusAsync(LastApplicationStatus appStatus)
        {
            var apphash = GetApplicationHash(appStatus.ApplicationPath);
            using (var conn = CreateConnection()) {
                conn.Open();
                await logTable.UpdateApplicationStatus(conn, null, apphash, appStatus);
            }
        }

        public virtual async Task UpdateApplicationStatusesAsync(IEnumerable<LastApplicationStatus> appStatuses)
        {
            using (var conn = CreateConnection()) {
                conn.Open();
                foreach (var appStatus in appStatuses) {
                    var apphash = GetApplicationHash(appStatus.ApplicationPath);
                    await logTable.UpdateApplicationStatus(conn, null, apphash, appStatus);
                }
            }
        }

        public virtual async Task MaintainAsync(TimeSpan logsKeepTime, IDictionary<string, TimeSpan> logsKeepTimePerApplication = null)
        {
            if (logsKeepTime.TotalDays < 0 || logsKeepTimePerApplication != null &&
                logsKeepTimePerApplication.Values.Any(t => t.TotalDays < 0)) {
                throw new ArgumentException("Invalid timespan value for your logs");
            }

            // we need to match application paths with md5 hashes used in table names
            IDictionary<string, TimeSpan> hashesWithTimeouts = null;
            if (logsKeepTimePerApplication != null) {
                hashesWithTimeouts = new Dictionary<string, TimeSpan>(logsKeepTimePerApplication.Count, StringComparer.OrdinalIgnoreCase);
                foreach (var app in logsKeepTimePerApplication) {
                    hashesWithTimeouts.Add(GetApplicationHash(app.Key), app.Value);
                }
            }

            // then iterate through tables and manage their partitions
            using (var conn = CreateConnection()) {
                conn.Open();

                // table_schema is a database name on mysql, when on sql server the database name is stored in table_catalog
                var tables = await conn.QueryAsync<string>("select table_name from information_schema.tables where table_schema = @Database or table_catalog = @Database", conn);
                // make sure that all tables have current and future partitions created
                foreach (var tbl in tables) {
                    string hash;
                    if (tbl.StartsWith(AppLogTablePrefix, StringComparison.OrdinalIgnoreCase)) {
                        hash = tbl.Substring(AppLogTablePrefix.Length);
                    } else {
                        continue;
                    }

                    TimeSpan keepTime = logsKeepTime;
                    if (hashesWithTimeouts != null) {
                        if (hashesWithTimeouts.ContainsKey(hash)) {
                            // if not predefined time to keep logs found, use the default one
                            keepTime = hashesWithTimeouts[hash];
                        }
                    }

                    await logTable.ManageTablePartitions(conn, tbl, keepTime);
                }
            }
        }
    }
}
