using Dapper;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Storage;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.MySql
{
    public class MySqlLogStore : ILogStore
    {
        public const string AppLogTablePrefix = "applog_";
        private static readonly ConcurrentDictionary<string, string> applicationMd5Hashes = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly LogTable logTable;

        public MySqlLogStore() : this(() => DateTime.UtcNow.Date) { }

        internal MySqlLogStore(Func<DateTime> currentUtcDateRetriever)
        {
            logTable = new LogTable(currentUtcDateRetriever);
        }

        public async Task AddLogRecordAsync(LogRecord logrec)
        {
            using (var conn = new MySqlConnection(MySqlLogStoreConfiguration.ConnectionString)) {
                await conn.OpenAsync();
                await AddLogRecordAsync(conn, null, logrec);
            }
        }

        public async Task AddLogRecordsAsync(IEnumerable<LogRecord> logrecs)
        {
            using (var conn = new MySqlConnection(MySqlLogStoreConfiguration.ConnectionString)) {
                await conn.OpenAsync();
                foreach (var logrec in logrecs) {
                    await AddLogRecordAsync(conn, null, logrec);
                }
            }
        }

        private async Task AddLogRecordAsync(MySqlConnection conn, MySqlTransaction tran, LogRecord logrec)
        {
            var apphash = GetApplicationHash(logrec.ApplicationPath);
            await logTable.SaveLogRecord(conn, tran, AppLogTablePrefix + apphash, logrec);
        }

        public async Task UpdateApplicationStatusAsync(LastApplicationStatus appStatus)
        {
            var apphash = GetApplicationHash(appStatus.ApplicationPath);
            using (var conn = new MySqlConnection(MySqlLogStoreConfiguration.ConnectionString)) {
                await conn.OpenAsync();
                await logTable.UpdateApplicationStatus(conn, null, apphash, appStatus);
            }
        }

        public async Task UpdateApplicationStatusesAsync(IEnumerable<LastApplicationStatus> appStatuses)
        {
            using (var conn = new MySqlConnection(MySqlLogStoreConfiguration.ConnectionString)) {
                await conn.OpenAsync();
                foreach (var appStatus in appStatuses) {
                    var apphash = GetApplicationHash(appStatus.ApplicationPath);
                    await logTable.UpdateApplicationStatus(conn, null, apphash, appStatus);
                }
            }
        }

        public async Task<LogSearchResults> FilterLogsAsync(LogSearchCriteria searchCriteria)
        {
            var hash = GetApplicationHash(searchCriteria.ApplicationPath);
            if (!LogTable.IsLogTableAvailable(AppLogTablePrefix + hash)) {
                return new LogSearchResults {
                    FoundItems = new LogRecord[0]
                };
            }

            if (string.IsNullOrEmpty(searchCriteria.ApplicationPath)) {
                throw new ArgumentException("ApplicationPath is required to filter the logs");
            }
            var whereSql = PrepareWhereSectionOfTheQuery(searchCriteria);
            var orderBySql = string.Format("order by TimeUtc desc limit {0},{1}", searchCriteria.Offset, searchCriteria.Limit);

            using (var conn = new MySqlConnection(MySqlLogStoreConfiguration.ConnectionString)) {
                await conn.OpenAsync();
                var dbapplogs = (await conn.QueryAsync<DbAppLogRecord>(string.Format("select * from {0}{1} {2} {3}", AppLogTablePrefix, hash, whereSql, orderBySql), new {
                    searchCriteria.FromUtc,
                    searchCriteria.ToUtc,
                    searchCriteria.Server,
                    Logger = searchCriteria.Logger + "%",
                    searchCriteria.Levels,
                    HttpStatusCode = searchCriteria.Keywords.HttpStatus + "%",
                    Url = searchCriteria.Keywords.Url + "%",
                    ClientIp = searchCriteria.Keywords.ClientIp + "%",
                    ServiceName = searchCriteria.Keywords.Service + "%"
                })).ToArray();

                return new LogSearchResults { FoundItems = ConvertDbLogRecordToPublicModel(dbapplogs) };
            }
        }

        private string PrepareWhereSectionOfTheQuery(LogSearchCriteria searchCriteria)
        {
            Debug.Assert(searchCriteria.Keywords != null);
            var whereSql = new StringBuilder("where TimeUtc >= convert(@FromUtc, datetime) and TimeUtc < convert(@ToUtc, datetime)");
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
        private LogRecord[] ConvertDbLogRecordToPublicModel(DbAppLogRecord[] dbapplogs)
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

        public async Task<IEnumerable<LastApplicationStatus>> GetApplicationStatusesAsync(DateTime lastDateUtcToQuery)
        {
            using (var conn = new MySqlConnection(MySqlLogStoreConfiguration.ConnectionString)) {
                await conn.OpenAsync();

                return await conn.QueryAsync<LastApplicationStatus>("select * from appstat where LastErrorTimeUtc >= @lastDateUtcToQuery or " +
                    "LastUpdateTimeUtc >= @lastDateUtcToQuery", new { lastDateUtcToQuery });
            }
        }

        public async Task MaintainAsync(TimeSpan logsKeepTime, IDictionary<string, TimeSpan> logsKeepTimePerApplication = null)
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
            using (var conn = new MySqlConnection(MySqlLogStoreConfiguration.ConnectionString)) {
                await conn.OpenAsync();

                var tables = await conn.QueryAsync<string>("select table_name from information_schema.tables where table_schema = @Database", conn);
                // make sure that all tables have current and future partitions created
                foreach (var tbl in tables) {
                    string hash;
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

        private string GetApplicationHash(string applicationPath)
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
    }
}
