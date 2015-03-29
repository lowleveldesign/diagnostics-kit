using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LuceneNetLogStore.Lucene.Analyzers;
using LowLevelDesign.Diagnostics.LuceneNetLogStore.Lucene;
using Lucene.Net.Documents;
using System;
using LowLevelDesign.Diagnostics.Commons.Storage;
using NLog;

namespace LowLevelDesign.Diagnostics.LuceneNetLogStore
{
    public class LogStore : ILogStore
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly SearchEngine<LogRecordAnalyzer> searchEngine;

        public LogStore(String indexPath, String logPath = null) {
            this.searchEngine = new SearchEngine<LogRecordAnalyzer>(indexPath, logPath);
        }

        public void AddLogRecord(LogRecord logrec) {
            var doc = new Document();

            doc.AddSearchableStringField("LoggerName", logrec.LoggerName);
            doc.AddSearchableNumericField("LogLevel", (int)logrec.LogLevel);
            doc.AddSearchableDateTimeField("TimeUtc", logrec.TimeUtc);
            doc.AddSearchableNumericField("ProcessId", logrec.ProcessId);
            doc.AddSearchableStringField("ProcessName", logrec.ProcessName);
            doc.AddSearchableNumericField("ThreadId", logrec.ThreadId);
            doc.AddSearchableStringField("Server", logrec.Server);
            doc.AddSearchableStringField("ApplicationPath", logrec.ApplicationPath);
            doc.AddSearchableStringField("ThreadIdentity", logrec.Identity);
            doc.AddSearchableStringField("CorrelationId", logrec.CorrelationId);
            doc.AddSearchableStringField("Message", logrec.Message);

            // iterate through the additional fields and store them according to the value types
            if (logrec.AdditionalFields != null) {
                foreach (var f in logrec.AdditionalFields) {
                    if (f.Value != null) {
                        if (typeof(int).Equals(f.Value.GetType())) {
                            doc.AddSearchableNumericField(f.Key, (int)f.Value);
                        } else if (typeof(long).Equals(f.Value.GetType())) {
                            doc.AddSearchableNumericField(f.Key, (long)f.Value);
                        } else if (typeof(float).Equals(f.Value.GetType())) {
                            doc.AddSearchableNumericField(f.Key, (float)f.Value);
                        } else if (typeof(double).Equals(f.Value.GetType())) {
                            doc.AddSearchableNumericField(f.Key, (double)f.Value);
                        } else if (typeof(DateTime).Equals(f.Value.GetType())) {
                            doc.AddSearchableDateTimeField(f.Key, (DateTime)f.Value);
                        } else if (typeof(String).Equals(f.Value.GetType())) {
                            doc.AddSearchableStringField(f.Key, (String)f.Value);
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
    }
}
