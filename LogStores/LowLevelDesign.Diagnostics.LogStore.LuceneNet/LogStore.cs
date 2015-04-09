using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LogStore.LuceneNet.Lucene.Analyzers;
using LowLevelDesign.Diagnostics.LogStore.LuceneNet.Lucene;
using Lucene.Net.Documents;
using System;
using LowLevelDesign.Diagnostics.Commons.Storage;
using NLog;
using System.Collections.Generic;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.LuceneNet
{
    public class LuceneNetLogStore : ILogStore
    {
        readonly static ISet<String> searchableAdditionalFields = new HashSet<String> {
            "Host", "LoggedUser", "Url", "Referer", "ClientIP", "RequestData", "ResponseData", "ServiceName", "ServiceDisplayName"
        };

        public async Task AddLogRecords(IEnumerable<LogRecord> logrecs)
        {
            foreach (var logrec in logrecs) {
                await AddLogRecord(logrec);
            }
        }

        public async Task AddLogRecord(LogRecord logrec) {
            var doc = new Document();

            doc.AddField("LoggerName", logrec.LoggerName, true);
            doc.AddField("LogLevel", (int)logrec.LogLevel, true);
            doc.AddField("TimeUtc", logrec.TimeUtc, true);
            doc.AddField("ProcessId", logrec.ProcessId, false);
            doc.AddField("ProcessName", logrec.ProcessName, false);
            doc.AddField("ThreadId", logrec.ThreadId, false);
            doc.AddField("Server", logrec.Server, true);
            doc.AddField("ApplicationPath", logrec.ApplicationPath, false);
            doc.AddField("Identity", logrec.Identity, true);
            doc.AddField("CorrelationId", logrec.CorrelationId, true);
            doc.AddField("Message", logrec.Message, true);

            // iterate through the additional fields and store them according to the value types
            if (logrec.AdditionalFields != null) {
                foreach (var f in logrec.AdditionalFields) {
                    if (f.Value != null) {
                        if (f.Value is int) {
                            doc.AddField(f.Key, (int)f.Value, searchableAdditionalFields.Contains(f.Key));
                        } else if (f.Value is long) {
                            doc.AddField(f.Key, (long)f.Value, searchableAdditionalFields.Contains(f.Key));
                        } else if (f.Value is float) {
                            doc.AddField(f.Key, (float)f.Value, searchableAdditionalFields.Contains(f.Key));
                        } else if (f.Value is double) {
                            doc.AddField(f.Key, (double)f.Value, searchableAdditionalFields.Contains(f.Key));
                        } else if (f.Value is DateTime) {
                            doc.AddField(f.Key, (DateTime)f.Value, searchableAdditionalFields.Contains(f.Key));
                        } else if (f.Value is String) {
                            doc.AddField(f.Key, (String)f.Value, searchableAdditionalFields.Contains(f.Key));
                        } else {
                            logger.Warn("Field {0} from additional fields won't be saved because its type: {1} is not supported.", f.Key, f.Value);
                        }
                    }
                }
            }

            // iterate through performance data coming with the log
            if (logrec.PerformanceData != null) {
                foreach (var f in logrec.PerformanceData) {
                    doc.Add(new NumericField(f.Key, Field.Store.YES, true).SetFloatValue(f.Value));
                }
            }

            searchEngine.SaveDocumentInIndex(doc);
        }

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly SearchEngine searchEngine;

        public LuceneNetLogStore() {
            var logPath = LogStoreConfiguration.LuceneNetConfigSection.LogEnabled ? LogStoreConfiguration.LuceneNetConfigSection.LogPath : null;
            searchEngine = new SearchEngine(LogStoreConfiguration.LuceneNetConfigSection.IndexPath, CreateAnalyzer, logPath);
        }

        private void AddFieldsAnalyzer(PerFieldAnalyzerWrapper analyzer, Analyzer fanalyzer, String[] fields) {
            foreach (var f in fields) {
                analyzer.AddAnalyzer(f, fanalyzer);
            }
        }

        private Analyzer CreateAnalyzer() {
            var analyzer = new PerFieldAnalyzerWrapper(new StandardAnalyzer(global::Lucene.Net.Util.Version.LUCENE_30));

            AddFieldsAnalyzer(analyzer, new KeywordAnalyzer(), new[] { "Server", "Host", "LoggedUser", 
                "ClientIP", "Identity", "CorrelationId" });
            AddFieldsAnalyzer(analyzer, new DottedNameAnalyzer(), new[] { "LoggerName", "ExceptionType", "ServiceName" });
            // FIXME: something better for urls
            AddFieldsAnalyzer(analyzer, new KeywordAnalyzer(), new[] { "Url", "Referer" });

            /* Other fields, such as: Message, ExceptionMessage, ExceptionAdditionalInfo,
             * RequestData, ResponseData, ServiceDisplayName will be parsed with the StandardAnalyzer */

            return analyzer;
        }


        public async Task<IEnumerable<LogRecord>> SearchLogs(LogSearchCriteria searchCriteria) {
            throw new NotImplementedException(); // FIXME we need to implement searching in Lucene index
        }


        public async Task Maintain(TimeSpan logsKeepTime, IDictionary<string, DateTime> logsKeepTimePerApplication = null) {
            throw new NotImplementedException();
        }
    }
}
