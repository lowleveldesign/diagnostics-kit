using LowLevelDesign.Diagnostics.Commons;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Storage;
using LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Models;
using Nest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.ElasticSearch
{
    /// <summary>
    /// LogStore implementation based on Elastic Search.
    /// 
    /// Thread-safe and may be used as a singleton.
    /// </summary>
    public class ElasticSearchLogStore : ILogStore
    {
        private const int MaxNumberOfReturnedStatuses = 1000;
        private const string AppStatsIndexName = ElasticSearchClientConfiguration.MainConfigIndex;

        private readonly static string[] AllLogLevelNames = Enum.GetNames(typeof(LogRecord.ELogLevel));

        private readonly Func<DateTime> currentDateRetriever;
        private readonly LogIndexManager logIndexManager;
        private readonly ElasticClient eclient;

        private DateTime lastIndexCheckDate;

        public ElasticSearchLogStore() : this(() => DateTime.UtcNow.Date) { }

        internal ElasticSearchLogStore(Func<DateTime> currentUtcDateRetriever)
        {
            this.eclient = ElasticSearchClientConfiguration.CreateClient(AppStatsIndexName);
            this.logIndexManager = new LogIndexManager(this.eclient, currentUtcDateRetriever);
            this.currentDateRetriever = currentUtcDateRetriever;
        }

        public async Task AddLogRecordAsync(LogRecord logrec)
        {
            await MakeSureCurrentIndexExistsAsync();
            await eclient.IndexAsync(MapLogRecord(logrec), ind => ind.Index(logIndexManager.GetCurrentIndexName()));
        }

        public async Task AddLogRecordsAsync(IEnumerable<LogRecord> logrecs)
        {
            await MakeSureCurrentIndexExistsAsync();
            await eclient.IndexManyAsync(logrecs.Select(MapLogRecord), logIndexManager.GetCurrentIndexName());
        }

        private async Task MakeSureCurrentIndexExistsAsync()
        {
            var today = currentDateRetriever();
            if (!today.Equals(lastIndexCheckDate)) {
                // if day changes we need to make sure that the index exists
                await logIndexManager.MakeSureCurrentIndexExistsAsync();
                lastIndexCheckDate = today;
            }
        }

        private ElasticLogRecord MapLogRecord(LogRecord logrec)
        {
            Object v;
            var res = new ElasticLogRecord {
                LoggerName = logrec.LoggerName,
                LogLevel = Enum.GetName(typeof(LogRecord.ELogLevel), logrec.LogLevel),
                TimeUtc = logrec.TimeUtc,
                Message = logrec.Message,
                ExceptionType = logrec.ExceptionType,
                ExceptionMessage = logrec.ExceptionMessage,
                ExceptionAdditionalInfo = logrec.ExceptionAdditionalInfo,
                CorrelationId = logrec.CorrelationId,
                Server = logrec.Server,
                ApplicationPath = logrec.ApplicationPath,
                ProcessId = logrec.ProcessId,
                ThreadId = logrec.ThreadId,
                Identity = logrec.Identity,
                PerfData = logrec.PerformanceData
            };

            if (logrec.AdditionalFields != null) {
                res.Host = logrec.AdditionalFields.TryGetValue("Host", out v) ? ((string)v).ShortenIfNecessary(100) : null;
                res.LoggedUser = logrec.AdditionalFields.TryGetValue("LoggedUser", out v) ? ((string)v).ShortenIfNecessary(200) : null;
                res.HttpStatusCode = logrec.AdditionalFields.TryGetValue("HttpStatusCode", out v) ? ((string)v).ShortenIfNecessary(15) : null;
                res.Url = logrec.AdditionalFields.TryGetValue("Url", out v) ? ((string)v).ShortenIfNecessary(2000) : null;
                res.Referer = logrec.AdditionalFields.TryGetValue("Referer", out v) ? ((string)v).ShortenIfNecessary(2000) : null;
                res.ClientIP = logrec.AdditionalFields.TryGetValue("ClientIP", out v) ? ((string)v).ShortenIfNecessary(50) : null;
                res.RequestData = logrec.AdditionalFields.TryGetValue("RequestData", out v) ? ((string)v).ShortenIfNecessary(2000) : null;
                res.ResponseData = logrec.AdditionalFields.TryGetValue("ResponseData", out v) ? ((string)v).ShortenIfNecessary(2000) : null;
                res.ServiceName = logrec.AdditionalFields.TryGetValue("ServiceName", out v) ? ((string)v).ShortenIfNecessary(100) : null;
                res.ServiceDisplayName = logrec.AdditionalFields.TryGetValue("ServiceDisplayName", out v) ? ((string)v).ShortenIfNecessary(200) : null;
            }

            return res;
        }

        public async Task UpdateApplicationStatusAsync(LastApplicationStatus status)
        {
            if (status == null) {
                throw new ArgumentException("status == null");
            }

            var elasticApplicationStatus = (await eclient.GetAsync<ElasticApplicationStatus>(
                GenerateElasticApplicationStatusId(status), AppStatsIndexName)).Source;

            elasticApplicationStatus = UpdateOrCreateElasticApplicationStatus(elasticApplicationStatus, status);
            await eclient.IndexAsync(elasticApplicationStatus, ind => ind.Index(AppStatsIndexName));
        }

        public async Task UpdateApplicationStatusesAsync(IEnumerable<LastApplicationStatus> statuses)
        {
            if (statuses == null) {
                throw new ArgumentException("statuses == null");
            }
            var statusesWithIds = statuses.ToDictionary(GenerateElasticApplicationStatusId);
            var elasticApplicationStatusSearchResultsWithIds = (await eclient.GetManyAsync<ElasticApplicationStatus>(statusesWithIds.Keys,
                AppStatsIndexName)).ToDictionary(s => s.Id);

            var elasticApplicationStatuses = new List<ElasticApplicationStatus>(statusesWithIds.Count);
            foreach (var statusWithId in statusesWithIds) {
                elasticApplicationStatuses.Add(UpdateOrCreateElasticApplicationStatus(
                    elasticApplicationStatusSearchResultsWithIds[statusWithId.Key].Source, statusWithId.Value));
            }

            if (elasticApplicationStatuses.Count > 0) {
                await eclient.IndexManyAsync(elasticApplicationStatuses, AppStatsIndexName);
            }
        }

        private string GenerateElasticApplicationStatusId(LastApplicationStatus appStatus)
        {
            return BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(
                            appStatus.ApplicationPath + appStatus.Server))).Replace("-", string.Empty);
        }

        private ElasticApplicationStatus UpdateOrCreateElasticApplicationStatus(ElasticApplicationStatus status, LastApplicationStatus appStatus)
        {
            if (status == null) {
                status = new ElasticApplicationStatus {
                    Id = GenerateElasticApplicationStatusId(appStatus),
                    ApplicationPath = appStatus.ApplicationPath,
                    Server = appStatus.Server
                };
            }
            status.LastUpdateTimeUtc = appStatus.LastUpdateTimeUtc;

            if (appStatus.ContainsErrorInformation()) {
                status.LastErrorTimeUtc = appStatus.LastErrorTimeUtc;
                status.LastErrorType = appStatus.LastErrorType;
            }

            if (appStatus.ContainsPerformanceData()) {
                status.Cpu = appStatus.Cpu;
                status.Memory = appStatus.Memory;
                status.LastPerformanceDataUpdateTimeUtc = appStatus.LastPerformanceDataUpdateTimeUtc;
            }
            return status;
        }

        public async Task<LogSearchResults> FilterLogsAsync(LogSearchCriteria searchCriteria)
        {
            var srchreq = new SearchRequest(
                await logIndexManager.GetQueryIndicesOrAliasAsync(searchCriteria.FromUtc, searchCriteria.ToUtc, 6), // max indices to query is 6 - otherwise use alias
                typeof(ElasticLogRecord)) {
                From = searchCriteria.Offset,
                Size = searchCriteria.Limit,
                Filter = PrepareFilterForTheLogRecordsSearch(searchCriteria),
                Sort = new List<KeyValuePair<PropertyPathMarker, ISort>> {
                    new KeyValuePair<PropertyPathMarker, ISort>(Property.Path<ElasticLogRecord>(l => l.TimeUtc), new Sort { Order = SortOrder.Descending })
                }
            };

            var queryResult = await eclient.SearchAsync<ElasticLogRecord>(srchreq);
            return new LogSearchResults {
                FoundItems = ConvertElasticLogSearchResultsToLogRecords(queryResult)
            };
        }

        private FilterContainer PrepareFilterForTheLogRecordsSearch(LogSearchCriteria searchCriteria)
        {
            Debug.Assert(searchCriteria.Keywords != null);
            var filters = new List<Func<FilterDescriptor<ElasticLogRecord>, FilterContainer>>();

            filters.Add(f => f.Term(log => log.ApplicationPath, searchCriteria.ApplicationPath));
            filters.Add(f => f.Range(r => r.OnField(log => log.TimeUtc).GreaterOrEquals(searchCriteria.FromUtc).Lower(searchCriteria.ToUtc)));

            if (searchCriteria.Levels != null) {
                var levelNames = searchCriteria.Levels.Select(lvl => Enum.GetName(typeof(LogRecord.ELogLevel), lvl)).ToArray();
                // query only in case not all log levels are selected
                if (levelNames.Length > 0 && levelNames.Length != AllLogLevelNames.Length) {
                    filters.Add(f => f.Terms(log => log.LogLevel, levelNames));
                }
            }
            if (!string.IsNullOrEmpty(searchCriteria.Server)) {
                filters.Add(f => f.Term(log => log.Server, searchCriteria.Server));
            }
            if (!string.IsNullOrEmpty(searchCriteria.Logger)) {
                filters.Add(f => f.Query(q => q.Match(m => m.OnField(log => log.LoggerName).Query(searchCriteria.Logger))));
            }
            if (!string.IsNullOrEmpty(searchCriteria.Keywords.HttpStatus)) {
                filters.Add(f => f.Query(q => q.Wildcard(log => log.HttpStatusCode, 
                    searchCriteria.Keywords.HttpStatus + "*")));
            }
            if (!string.IsNullOrEmpty(searchCriteria.Keywords.Url)) {
                filters.Add(f => f.Query(q => q.Match(m => m.OnField(log => log.Url).Query(searchCriteria.Keywords.Url))));
            }
            if (!string.IsNullOrEmpty(searchCriteria.Keywords.Service)) {
                filters.Add(f => f.Query(q => q.Match(m => m.OnField(log => log.ServiceName).Query(searchCriteria.Keywords.Service))));
            }

            // Keywords are used in a query 
            if (!string.IsNullOrEmpty(searchCriteria.Keywords.FreeText)) {
                filters.Add(f => f.Query(q => q.MultiMatch(m => m.Analyzer("standard").OnFieldsWithBoost(d => {
                    d.Add(log => log.Message, 1.5);
                    d.Add(log => log.ExceptionType, 2.0);
                    d.Add(log => log.ExceptionMessage, 2.0);
                    d.Add(log => log.ExceptionAdditionalInfo, 2.0);
                    d.Add(log => log.Identity, null);
                    d.Add(log => log.LoggedUser, null);
                    d.Add(log => log.Host, null);
                    d.Add(log => log.Url, null);
                    d.Add(log => log.Referer, null);
                    d.Add(log => log.HttpStatusCode, null);
                    d.Add(log => log.RequestData, null);
                    d.Add(log => log.ResponseData, null);
                    d.Add(log => log.ServiceName, null);
                    d.Add(log => log.ServiceDisplayName, null);
                }).Query(searchCriteria.Keywords.FreeText))));
            }
            return ElasticLogFilter.And(filters.ToArray());
        }

        private IEnumerable<LogRecord> ConvertElasticLogSearchResultsToLogRecords(ISearchResponse<ElasticLogRecord> queryResult)
        {
            return queryResult.Hits.Select(hit => {
                var elogrec = hit.Source;
                var logrec = new LogRecord {
                    LoggerName = elogrec.LoggerName,
                    LogLevel = (LogRecord.ELogLevel)Enum.Parse(typeof(LogRecord.ELogLevel), elogrec.LogLevel),
                    TimeUtc = elogrec.TimeUtc,
                    Message = elogrec.Message,
                    ExceptionType = elogrec.ExceptionType,
                    ExceptionMessage = elogrec.ExceptionMessage,
                    ExceptionAdditionalInfo = elogrec.ExceptionAdditionalInfo,
                    CorrelationId = elogrec.CorrelationId,
                    Server = elogrec.Server,
                    ApplicationPath = elogrec.ApplicationPath,
                    ProcessId = elogrec.ProcessId,
                    ThreadId = elogrec.ThreadId,
                    Identity = elogrec.Identity,
                    PerformanceData = elogrec.PerfData,
                    AdditionalFields = new Dictionary<string, Object>()
                };
                logrec.AdditionalFields.AddIfNotNull("Host", elogrec.Host);
                logrec.AdditionalFields.AddIfNotNull("LoggedUser", elogrec.LoggedUser);
                logrec.AdditionalFields.AddIfNotNull("HttpStatusCode", elogrec.HttpStatusCode);
                logrec.AdditionalFields.AddIfNotNull("Url", elogrec.Url);
                logrec.AdditionalFields.AddIfNotNull("Referer", elogrec.Referer);
                logrec.AdditionalFields.AddIfNotNull("ClientIP", elogrec.ClientIP);
                logrec.AdditionalFields.AddIfNotNull("RequestData", elogrec.RequestData);
                logrec.AdditionalFields.AddIfNotNull("ResponseData", elogrec.ResponseData);
                logrec.AdditionalFields.AddIfNotNull("ServiceName", elogrec.ServiceName);
                logrec.AdditionalFields.AddIfNotNull("ServiceDisplayName", elogrec.ServiceDisplayName);

                return logrec;
            });
        }

        public async Task<IEnumerable<LastApplicationStatus>> GetApplicationStatusesAsync(DateTime lastDateTimeUtcToQuery)
        {
            var result = await eclient.SearchAsync<ElasticApplicationStatus>(s => s.Take(MaxNumberOfReturnedStatuses).Index(
                AppStatsIndexName).Filter(q => q.Range(r => r.OnField(st => st.LastUpdateTimeUtc)
                .GreaterOrEquals(lastDateTimeUtcToQuery))));
            return result.Hits.Select(hit => new LastApplicationStatus {
                ApplicationPath = hit.Source.ApplicationPath,
                Cpu = hit.Source.Cpu,
                Memory = hit.Source.Memory,
                Server = hit.Source.Server,
                LastErrorTimeUtc = hit.Source.LastErrorTimeUtc,
                LastErrorType = hit.Source.LastErrorType,
                LastUpdateTimeUtc = hit.Source.LastUpdateTimeUtc,
                LastPerformanceDataUpdateTimeUtc = hit.Source.LastPerformanceDataUpdateTimeUtc
            });
        }

        public async Task MaintainAsync(TimeSpan logsKeepTime, IDictionary<string, TimeSpan> logsKeepTimePerApplication = null)
        {
            // find max logs keep time
            if (logsKeepTimePerApplication != null) {
                foreach (var ts in logsKeepTimePerApplication.Values) {
                    if (ts.CompareTo(logsKeepTime) > 0) {
                        logsKeepTime = ts;
                    }
                }
            }
            await logIndexManager.ManageIndicesAsync(logsKeepTime);
        }
    }
}
