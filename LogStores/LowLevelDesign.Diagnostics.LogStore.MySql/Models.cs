using System;
using LowLevelDesign.Diagnostics.Commons.Models;

namespace LowLevelDesign.Diagnostics.LogStore.MySql
{
    internal class DbAppLogRecord
    {
        public Int64 Id { get; set; }
        public String LoggerName { get; set; }
        public String ApplicationPath { get; set; }
        public LogRecord.ELogLevel LogLevel { get; set; }
        public DateTime TimeUtc { get; set; }
        public int ProcessId { get; set; }
        public int ThreadId { get; set; }
        public String Server { get; set; }
        public String Identity { get; set; }
        public String CorrelationId { get; set; }
        public String Message { get; set; }
        public String ExceptionMessage { get; set; }
        public String ExceptionType { get; set; }
        public String ExceptionAdditionalInfo { get; set; }
        public String Host { get; set; }
        public String LoggedUser { get; set; }
        public String HttpStatusCode { get; set; }
        public String Url { get; set; }
        public String Referer { get; set; }
        public String ClientIP { get; set; }
        public String RequestData { get; set; }
        public String ResponseData { get; set; }
        public String ServiceName { get; set; }
        public String ServiceDisplayName { get; set; }
        public String PerfData { get; set; }
    }

    internal class DbPerfLogRecord
    {
        public Int64 LogRecordId { get; set; }
        public DateTime TimeUtc { get; set; }
        public String CounterName { get; set; }
        public float CounterValue { get; set; }
    }

}
