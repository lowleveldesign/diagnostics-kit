using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LuceneNetLogStore.Lucene.Analyzers;
using LowLevelDesign.Diagnostics.LuceneNetLogStore.Lucene;
using Lucene.Net.Documents;
using System;
using LowLevelDesign.Diagnostics.Commons.Storage;
using NLog;
using System.Collections.Generic;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;

namespace LowLevelDesign.Diagnostics.LuceneNetLogStore
{
    public class LogStore : ILogStore
    {
        readonly static ISet<String> searchableAdditionalFields = new HashSet<String> {
            "Host", "LoggedUser", "Url", "Referer", "ClientIP", "RequestData", "ResponseData", "ServiceName", "ServiceDisplayName"
        };

        public void AddLogRecords(IEnumerable<LogRecord> logrecs)
        {
            // FIXME implement :)
            throw new NotImplementedException();
        }

        public void AddLogRecord(LogRecord logrec) {
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
                        if (typeof(int).Equals(f.Value.GetType())) {
                            doc.AddField(f.Key, (int)f.Value, searchableAdditionalFields.Contains(f.Key));
                        } else if (typeof(long).Equals(f.Value.GetType())) {
                            doc.AddField(f.Key, (long)f.Value, searchableAdditionalFields.Contains(f.Key));
                        } else if (typeof(float).Equals(f.Value.GetType())) {
                            doc.AddField(f.Key, (float)f.Value, searchableAdditionalFields.Contains(f.Key));
                        } else if (typeof(double).Equals(f.Value.GetType())) {
                            doc.AddField(f.Key, (double)f.Value, searchableAdditionalFields.Contains(f.Key));
                        } else if (typeof(DateTime).Equals(f.Value.GetType())) {
                            doc.AddField(f.Key, (DateTime)f.Value, searchableAdditionalFields.Contains(f.Key));
                        } else if (typeof(String).Equals(f.Value.GetType())) {
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

        public LogStore(String indexPath, String logPath = null) {
            searchEngine = new SearchEngine(indexPath, CreateAnalyzer, logPath);
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
    }
}
