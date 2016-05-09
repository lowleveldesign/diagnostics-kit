using Nest;
using System;
using System.Collections.Generic;

namespace LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Models
{
    [ElasticsearchType(Name = "logrecords")]
    internal sealed class ElasticLogRecord
    {
        [String(Index = FieldIndexOption.Analyzed, Analyzer = "loggername", Store = true)]
        public string LoggerName { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string ApplicationPath { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string LogLevel { get; set; }

        [Date(Store = true)]
        public DateTime TimeUtc { get; set; }

        [Number(Store = true)]
        public int ProcessId { get; set; }

        [Number(Store = true)]
        public int ThreadId { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string Server { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string Identity { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string CorrelationId { get; set; }

        [String(Index = FieldIndexOption.Analyzed, Analyzer = "standard", Store = true)]
        public string Message { get; set; }
       
        [String(Index = FieldIndexOption.Analyzed, Analyzer = "standard", Store = true)]
        public string ExceptionMessage { get; set; }

        [String(Index = FieldIndexOption.Analyzed, Analyzer = "standard", Store = true)]
        public string ExceptionType { get; set; }

        [String(Index = FieldIndexOption.Analyzed, Analyzer = "standard", Store = true)]
        public string ExceptionAdditionalInfo { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string Host { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string LoggedUser { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string HttpStatusCode { get; set; }

        [String(Index = FieldIndexOption.Analyzed, Analyzer = "standard", Store = true)]
        public string Url { get; set; }

        [String(Index = FieldIndexOption.Analyzed, Analyzer = "standard", Store = true)]
        public string Referer { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string ClientIP { get; set; }

        [String(Index = FieldIndexOption.Analyzed, Analyzer = "standard", Store = true)]
        public string RequestData { get; set; }
        
        [String(Index = FieldIndexOption.Analyzed, Analyzer = "standard", Store = true)]
        public string ResponseData { get; set; }

        [String(Index = FieldIndexOption.Analyzed, Analyzer = "standard", Store = true)]
        public string ServiceName { get; set; }

        [String(Index = FieldIndexOption.Analyzed, Analyzer = "standard", Store = true)]
        public string ServiceDisplayName { get; set; }
        
        [Nested(IncludeInParent = true)]
        public IDictionary<string, float> PerfData { get; set; }
    }
}
