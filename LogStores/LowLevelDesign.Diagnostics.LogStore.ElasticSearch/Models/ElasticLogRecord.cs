using Nest;
using System;
using System.Collections.Generic;

namespace LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Models
{
    [ElasticType(Name = "logrecords")]
    internal sealed class ElasticLogRecord
    {
        [ElasticProperty(Index = FieldIndexOption.Analyzed, Analyzer = "loggername", Store = true)]
        public string LoggerName { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string ApplicationPath { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string LogLevel { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Type = FieldType.Date, Store = true)]
        public DateTime TimeUtc { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Type = FieldType.Integer, 
            NumericType = NumberType.Integer, Store = true)]
        public int ProcessId { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Type = FieldType.Integer, 
            NumericType = NumberType.Integer, Store = true)]
        public int ThreadId { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string Server { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string Identity { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string CorrelationId { get; set; }

        [ElasticProperty(Index = FieldIndexOption.Analyzed, Analyzer = "standard", Store = true)]
        public string Message { get; set; }
       
        [ElasticProperty(Index = FieldIndexOption.Analyzed, Analyzer = "standard", Store = true)]
        public string ExceptionMessage { get; set; }

        [ElasticProperty(Index = FieldIndexOption.Analyzed, Analyzer = "standard", Store = true)]
        public string ExceptionType { get; set; }

        [ElasticProperty(Index = FieldIndexOption.Analyzed, Analyzer = "standard", Store = true)]
        public string ExceptionAdditionalInfo { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string Host { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string LoggedUser { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string HttpStatusCode { get; set; }

        [ElasticProperty(Index = FieldIndexOption.Analyzed, Analyzer = "standard", Store = true)]
        public string Url { get; set; }

        [ElasticProperty(Index = FieldIndexOption.Analyzed, Analyzer = "standard", Store = true)]
        public string Referer { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string ClientIP { get; set; }

        [ElasticProperty(Index = FieldIndexOption.Analyzed, Analyzer = "standard", Store = true)]
        public string RequestData { get; set; }
        
        [ElasticProperty(Index = FieldIndexOption.Analyzed, Analyzer = "standard", Store = true)]
        public string ResponseData { get; set; }

        [ElasticProperty(Index = FieldIndexOption.Analyzed, Analyzer = "standard", Store = true)]
        public string ServiceName { get; set; }

        [ElasticProperty(Index = FieldIndexOption.Analyzed, Analyzer = "standard", Store = true)]
        public string ServiceDisplayName { get; set; }
        
        [ElasticProperty(Index = FieldIndexOption.No, Type = FieldType.Object, Store = true)]
        public IDictionary<string, float> PerfData { get; set; }
    }
}
