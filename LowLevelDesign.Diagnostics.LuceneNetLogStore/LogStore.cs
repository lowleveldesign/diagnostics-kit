using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.LuceneNetLogStore.Lucene.Analyzers;
using LowLevelDesign.Diagnostics.LuceneNetLogStore.Lucene;
using LowLevelDesign.Diagnostics.Models;
using Lucene.Net.Documents;
using Rzekoznawca.Commons.Search;
using System;
using Rzekoznawca.Commons.Search.Lucene;

namespace LowLevelDesign.Diagnostics.LuceneNetLogStore
{
    public class LogStore : ILogStore
    {
        private readonly SearchEngine<LogRecordAnalyzer> searchEngine;

        public LogStore(String indexPath, String logPath = null) {
            this.searchEngine = new SearchEngine<LogRecordAnalyzer>(indexPath, logPath);
        }

        public void AddLogRecord(LogRecord logrec) {
            var doc = new Document();

            doc.AddSearchableStringField("LoggerName", logrec.LoggerName);
            doc.AddSearchableStringField("LogLevel", logrec.LogLevel);
            doc.AddSearchableDateTimeField("TimeUtc", logrec.TimeUtc);
            doc.AddSearchableNumericField("ProcessId", logrec.ProcessId);
            doc.AddSearchableStringField("ProcessName", logrec.ProcessName);
            doc.AddSearchableNumericField("ThreadId", logrec.ThreadId);
            doc.AddSearchableStringField("Server", logrec.Server);
            doc.AddSearchableStringField("ApplicationPath", logrec.ApplicationPath);
            doc.AddSearchableStringField("ThreadIdentity", logrec.ThreadIdentity);
            doc.AddSearchableStringField("CorrelationId", logrec.CorrelationId);
        }
    }
}
