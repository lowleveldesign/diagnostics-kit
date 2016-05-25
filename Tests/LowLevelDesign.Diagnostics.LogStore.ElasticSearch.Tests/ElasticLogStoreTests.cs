using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Models;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Tests
{
    public class ElasticLogStoreTests : IDisposable
    {
        private readonly ElasticClient client;

        public ElasticLogStoreTests()
        {
            client = ElasticSearchClientConfiguration.CreateClient(null);
        }

        [Fact]
        public async Task TestAddLogRecord()
        {
            var utcnow = DateTime.UtcNow.Date;
            var elasticLogStore = new ElasticSearchLogStore(() => utcnow);
            const string appPath = "c:\\###rather_not_existing_application_path###";
            var hash = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(appPath))).Replace("-", string.Empty);

            var logrec = new LogRecord {
                LoggerName = "TestLogger",
                ApplicationPath = appPath,
                LogLevel = LogRecord.ELogLevel.Error,
                TimeUtc = DateTime.UtcNow,
                ProcessId = -1,
                ThreadId = 456,
                Server = "TestServer",
                Identity = "TestIdentity",
                CorrelationId = Guid.NewGuid().ToString(),
                Message = "Test log message to store in the log",
                ExceptionMessage = "Test exception log message",
                ExceptionType = "TestException",
                ExceptionAdditionalInfo = "Additinal info for the test exception",
                AdditionalFields = new Dictionary<string, Object>
                {
                    { "Host", "testhost.com" },
                    { "LoggedUser", "testloggeduser" },
                    { "HttpStatusCode", "200.1" },
                    { "Url", "http://testhost.com" },
                    { "Referer", "http://prevtesthost.com" },
                    { "ClientIP", null },
                    { "RequestData", "test test test" },
                    { "ResponseData", null },
                    { "ServiceName", "TestService" },
                    { "ServiceDisplayName", "Test service generating logs" },
                    { "NotExisting", null }
                },
                PerformanceData = new Dictionary<string, float>
                {
                    { "CPU", 2.0f },
                    { "Memory", 20000000f }
                }
            };

            // add log
            await elasticLogStore.AddLogRecordAsync(logrec);

            var lim = new LogIndexManager(client, () => utcnow);

            // check if index was created
            var ir = await client.IndexExistsAsync(lim.GetCurrentIndexName());
            Assert.Equal(true, ir.Exists);

            // give it 2s to index
            await Task.Delay(2000);

            var res = await client.SearchAsync<ElasticLogRecord>(s => s.Query(f => f.Term(lr => lr.ProcessId, -1))); 
            Assert.Equal(1L, res.Total);
            var dbLogRec = res.Hits.First().Source;


            // check logs content
            Assert.Equal(logrec.LoggerName, dbLogRec.LoggerName);
            Assert.Equal(logrec.ApplicationPath, dbLogRec.ApplicationPath);
            Assert.Equal(Enum.GetName(typeof(LogRecord.ELogLevel), logrec.LogLevel), dbLogRec.LogLevel);
            Assert.Equal(logrec.TimeUtc.ToShortDateString(), dbLogRec.TimeUtc.ToShortDateString());
            Assert.Equal(logrec.ProcessId, dbLogRec.ProcessId);
            Assert.Equal(logrec.ThreadId, dbLogRec.ThreadId);
            Assert.Equal(logrec.Server, dbLogRec.Server);
            Assert.Equal(logrec.Identity, dbLogRec.Identity);
            Assert.Equal(logrec.CorrelationId, dbLogRec.CorrelationId);
            Assert.Equal(logrec.Message, dbLogRec.Message);
            Assert.Equal(logrec.ExceptionMessage, dbLogRec.ExceptionMessage);
            Assert.Equal(logrec.ExceptionType, dbLogRec.ExceptionType);
            Assert.Equal(logrec.ExceptionAdditionalInfo, dbLogRec.ExceptionAdditionalInfo);
            Assert.Equal((string)logrec.AdditionalFields["Host"], dbLogRec.Host);
            Assert.Equal((string)logrec.AdditionalFields["LoggedUser"], dbLogRec.LoggedUser);
            Assert.Equal((string)logrec.AdditionalFields["HttpStatusCode"], dbLogRec.HttpStatusCode);
            Assert.Equal((string)logrec.AdditionalFields["Url"], dbLogRec.Url);
            Assert.Equal((string)logrec.AdditionalFields["Referer"], dbLogRec.Referer);
            Assert.Equal((string)logrec.AdditionalFields["ClientIP"], dbLogRec.ClientIP);
            Assert.Equal((string)logrec.AdditionalFields["RequestData"], dbLogRec.RequestData);
            Assert.Equal((string)logrec.AdditionalFields["ResponseData"], dbLogRec.ResponseData);
            Assert.Equal((string)logrec.AdditionalFields["ServiceName"], dbLogRec.ServiceName);
            Assert.Equal((string)logrec.AdditionalFields["ServiceDisplayName"], dbLogRec.ServiceDisplayName);

            var dbPerfLogs = dbLogRec.PerfData;
            Assert.True(dbPerfLogs.Count == 2);

            float r;
            Assert.True(dbPerfLogs.TryGetValue("CPU", out r));
            Assert.Equal(r, logrec.PerformanceData["CPU"]);

            Assert.True(dbPerfLogs.TryGetValue("Memory", out r));
            Assert.Equal(r, logrec.PerformanceData["Memory"]);


            res = await client.SearchAsync<ElasticLogRecord>(s => s.Query(f => f.Term(lr => lr.ExceptionType, "test"))); 
            Assert.Equal(1L, res.Total);
            dbLogRec = res.Hits.First().Source;

            Assert.Equal("TestException", dbLogRec.ExceptionType);

        }

        [Fact]
        public async Task ApplicationStatusTest()
        {
            var utcnow = DateTime.UtcNow.Date;
            var elasticLogStore = new ElasticSearchLogStore(() => utcnow);
            const string appPath = "c:\\###rather_not_existing_application_path###";
            var hash = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(appPath))).Replace("-", string.Empty);

            var startDate = DateTime.UtcNow.AddMinutes(-1);

            var expectedAppStatus = new LastApplicationStatus {
                ApplicationPath = appPath,
                Server = "SRV1",
                LastUpdateTimeUtc = DateTime.UtcNow
            };
            await elasticLogStore.UpdateApplicationStatusAsync(expectedAppStatus);
            await Task.Delay(2000);

            var actualAppStatus = (await elasticLogStore.GetApplicationStatusesAsync(startDate)).FirstOrDefault(
                st => string.Equals(st.ApplicationPath, expectedAppStatus.ApplicationPath, StringComparison.Ordinal));
            Assert.NotNull(actualAppStatus);
            Assert.Equal(expectedAppStatus.ApplicationPath, actualAppStatus.ApplicationPath);
            Assert.Equal(expectedAppStatus.Cpu, 0);
            Assert.Equal(expectedAppStatus.Memory, 0);
            Assert.Null(actualAppStatus.LastErrorTimeUtc);
            Assert.Null(actualAppStatus.LastPerformanceDataUpdateTimeUtc);

            expectedAppStatus.LastErrorType = "TestException";
            expectedAppStatus.LastErrorTimeUtc = DateTime.UtcNow;
            await elasticLogStore.UpdateApplicationStatusAsync(expectedAppStatus);
            await Task.Delay(2000);

            actualAppStatus = (await elasticLogStore.GetApplicationStatusesAsync(startDate)).FirstOrDefault(
                st => string.Equals(st.ApplicationPath, expectedAppStatus.ApplicationPath, StringComparison.Ordinal));
            Assert.NotNull(actualAppStatus);
            Assert.Equal(expectedAppStatus.ApplicationPath, actualAppStatus.ApplicationPath);
            Assert.Equal(expectedAppStatus.Cpu, 0);
            Assert.Equal(expectedAppStatus.Memory, 0);
            Assert.Equal(expectedAppStatus.LastErrorType, actualAppStatus.LastErrorType);
            Assert.NotNull(actualAppStatus.LastErrorTimeUtc);
            Assert.Null(actualAppStatus.LastPerformanceDataUpdateTimeUtc);

            expectedAppStatus.Cpu = 10f;
            expectedAppStatus.Memory = 1000f;
            expectedAppStatus.LastPerformanceDataUpdateTimeUtc = DateTime.UtcNow;
            await elasticLogStore.UpdateApplicationStatusAsync(expectedAppStatus);
            await Task.Delay(2000);

            actualAppStatus = (await elasticLogStore.GetApplicationStatusesAsync(startDate)).FirstOrDefault(
                st => string.Equals(st.ApplicationPath, expectedAppStatus.ApplicationPath, StringComparison.Ordinal));
            Assert.NotNull(actualAppStatus);
            Assert.Equal(expectedAppStatus.ApplicationPath, actualAppStatus.ApplicationPath);
            Assert.Equal(expectedAppStatus.LastErrorType, actualAppStatus.LastErrorType);
            Assert.NotNull(actualAppStatus.LastErrorTimeUtc);
            Assert.Equal(expectedAppStatus.Cpu, 10f);
            Assert.Equal(expectedAppStatus.Memory, 1000f);
            Assert.NotNull(actualAppStatus.LastPerformanceDataUpdateTimeUtc);
        }

        [Fact]
        public async Task ApplicationStatusesTest()
        {
            var utcnow = DateTime.UtcNow.Date;
            var elasticLogStore = new ElasticSearchLogStore(() => utcnow);
            const string appPath = "c:\\###rather_not_existing_application_path###";
            var hash = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(appPath))).Replace("-", string.Empty);

            var startDate = DateTime.UtcNow.AddMinutes(-1);

            var expectedAppStatus = new LastApplicationStatus {
                ApplicationPath = appPath,
                Server = "SRV1",
                LastUpdateTimeUtc = DateTime.UtcNow
            };
            await elasticLogStore.UpdateApplicationStatusesAsync(new[] { expectedAppStatus });
            await Task.Delay(2000);

            var actualAppStatus = (await elasticLogStore.GetApplicationStatusesAsync(startDate)).FirstOrDefault(
                st => string.Equals(st.ApplicationPath, expectedAppStatus.ApplicationPath, StringComparison.Ordinal));
            Assert.NotNull(actualAppStatus);
            Assert.Equal(expectedAppStatus.ApplicationPath, actualAppStatus.ApplicationPath);
            Assert.Equal(expectedAppStatus.Cpu, 0);
            Assert.Equal(expectedAppStatus.Memory, 0);
            Assert.Null(actualAppStatus.LastErrorTimeUtc);
            Assert.Null(actualAppStatus.LastPerformanceDataUpdateTimeUtc);

            expectedAppStatus.LastErrorType = "TestException";
            expectedAppStatus.LastErrorTimeUtc = DateTime.UtcNow;
            await elasticLogStore.UpdateApplicationStatusesAsync(new[] { expectedAppStatus });
            await Task.Delay(2000);

            actualAppStatus = (await elasticLogStore.GetApplicationStatusesAsync(startDate)).FirstOrDefault(
                st => string.Equals(st.ApplicationPath, expectedAppStatus.ApplicationPath, StringComparison.Ordinal));
            Assert.NotNull(actualAppStatus);
            Assert.Equal(expectedAppStatus.ApplicationPath, actualAppStatus.ApplicationPath);
            Assert.Equal(expectedAppStatus.Cpu, 0);
            Assert.Equal(expectedAppStatus.Memory, 0);
            Assert.Equal(expectedAppStatus.LastErrorType, actualAppStatus.LastErrorType);
            Assert.NotNull(actualAppStatus.LastErrorTimeUtc);
            Assert.Null(actualAppStatus.LastPerformanceDataUpdateTimeUtc);

            expectedAppStatus.Cpu = 10f;
            expectedAppStatus.Memory = 1000f;
            expectedAppStatus.LastPerformanceDataUpdateTimeUtc = DateTime.UtcNow;
            await elasticLogStore.UpdateApplicationStatusesAsync(new[] { expectedAppStatus });
            await Task.Delay(2000);

            actualAppStatus = (await elasticLogStore.GetApplicationStatusesAsync(startDate)).FirstOrDefault(
                st => string.Equals(st.ApplicationPath, expectedAppStatus.ApplicationPath, StringComparison.Ordinal));
            Assert.NotNull(actualAppStatus);
            Assert.Equal(expectedAppStatus.ApplicationPath, actualAppStatus.ApplicationPath);
            Assert.Equal(expectedAppStatus.LastErrorType, actualAppStatus.LastErrorType);
            Assert.NotNull(actualAppStatus.LastErrorTimeUtc);
            Assert.Equal(expectedAppStatus.Cpu, 10f);
            Assert.Equal(expectedAppStatus.Memory, 1000f);
            Assert.NotNull(actualAppStatus.LastPerformanceDataUpdateTimeUtc);
        }

        [Fact]
        public async Task LogFilteringTest()
        {
            // add a test log record
            var utcnow = DateTime.UtcNow.Date;
            var elasticLogStore = new ElasticSearchLogStore(() => utcnow);
            const string appPath = "c:\\###rather_not_existing_application_path2###";

            var logrec = new LogRecord {
                LoggerName = "TestLogger",
                ApplicationPath = appPath,
                LogLevel = LogRecord.ELogLevel.Error,
                TimeUtc = DateTime.UtcNow,
                ProcessId = -1,
                ThreadId = 456,
                Server = "TestServer",
                Identity = "TestIdentity",
                CorrelationId = Guid.NewGuid().ToString(),
                Message = "Test log message to store in the log",
                ExceptionMessage = "Test exception log message",
                ExceptionType = "TestException",
                ExceptionAdditionalInfo = "Additinal info for the test exception",
                AdditionalFields = new Dictionary<string, Object>
                {
                    { "Host", "testhost.com" },
                    { "LoggedUser", "testloggeduser" },
                    { "HttpStatusCode", "200.1" },
                    { "Url", "http://testhost.com" },
                    { "Referer", "http://prevtesthost.com" },
                    { "ClientIP", null },
                    { "RequestData", "test test test" },
                    { "ResponseData", null },
                    { "ServiceName", "TestService" },
                    { "ServiceDisplayName", "Test service generating logs" },
                    { "NotExisting", null }
                },
                PerformanceData = new Dictionary<string, float>
                {
                    { "CPU", 2.0f },
                    { "Memory", 20000000f }
                }
            };

            // add log
            await elasticLogStore.AddLogRecordAsync(logrec);

            // give it 2s to index
            await Task.Delay(2000);

            // check content
            var searchResults = await elasticLogStore.FilterLogsAsync(new LogSearchCriteria {
                FromUtc = DateTime.UtcNow.AddMinutes(-10),
                ToUtc = DateTime.UtcNow.AddMinutes(10),
                ApplicationPath = appPath,
                Levels = new[] { LogRecord.ELogLevel.Error, LogRecord.ELogLevel.Info },
                Limit = 10,
                Offset = 0,
                Server = "TestServer",
                Keywords = new KeywordsParsed { FreeText = "test exception" }
            });

            Assert.NotNull(searchResults.FoundItems);
            var foundItems = searchResults.FoundItems.ToArray();
            Assert.True(foundItems.Length == 1);
            var logrec2 = foundItems[0];
            Assert.Equal(logrec.LoggerName, logrec2.LoggerName);
            Assert.Equal(logrec.ApplicationPath, logrec2.ApplicationPath);
            Assert.Equal(logrec.LogLevel, logrec2.LogLevel);
            Assert.Equal(logrec.TimeUtc.ToShortDateString(), logrec2.TimeUtc.ToShortDateString());
            Assert.Equal(logrec.ProcessId, logrec2.ProcessId);
            Assert.Equal(logrec.ThreadId, logrec2.ThreadId);
            Assert.Equal(logrec.Server, logrec2.Server);
            Assert.Equal(logrec.Identity, logrec2.Identity);
            Assert.Equal(logrec.CorrelationId, logrec2.CorrelationId);
            Assert.Equal(logrec.Message, logrec2.Message);
            Assert.Equal(logrec.ExceptionMessage, logrec2.ExceptionMessage);
            Assert.Equal(logrec.ExceptionType, logrec2.ExceptionType);
            Assert.Equal(logrec.ExceptionAdditionalInfo, logrec2.ExceptionAdditionalInfo);
            Assert.Equal(logrec.AdditionalFields["Host"], logrec2.AdditionalFields["Host"]);
            Assert.Equal(logrec.AdditionalFields["LoggedUser"], logrec2.AdditionalFields["LoggedUser"]);
            Assert.Equal(logrec.AdditionalFields["HttpStatusCode"], logrec2.AdditionalFields["HttpStatusCode"]);
            Assert.Equal(logrec.AdditionalFields["Url"], logrec2.AdditionalFields["Url"]);
            Assert.Equal(logrec.AdditionalFields["Referer"], logrec2.AdditionalFields["Referer"]);
            Assert.Equal(logrec.AdditionalFields["RequestData"], logrec2.AdditionalFields["RequestData"]);
            Assert.Equal(logrec.AdditionalFields["ServiceName"], logrec2.AdditionalFields["ServiceName"]);
            Assert.Equal(logrec.AdditionalFields["ServiceDisplayName"], logrec2.AdditionalFields["ServiceDisplayName"]);
        }

        public void Dispose()
        {
            var indexName = new LogIndexManager(null, () => DateTime.UtcNow).GetCurrentIndexName();

            var searchResults = client.Search<ElasticLogRecord>(s => s.Index(indexName).Query(q => q.Term(
                t => t.Field(log => log.ProcessId).Value(-1))));
            foreach (var hit in searchResults.Hits) {
                client.Delete(new DocumentPath<ElasticLogRecord>(hit.Id).Index(indexName));
            }

            var docPath = new DocumentPath<ElasticApplicationStatus>(GenerateElasticApplicationStatusId(
                "c:\\###rather_not_existing_application_path###", "SRV1")).Index("lldconf");
            var req = client.Get(docPath);
            if (req.Found) {
                client.Delete(docPath);
            }
            Thread.Sleep(2000);
        }
        private string GenerateElasticApplicationStatusId(string path, string server)
        {
            return BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(
                            path + server))).Replace("-", string.Empty);
        }
    }
}
