using Nest;
using System;

namespace LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Models
{
    [ElasticType(Name = "appstats")]
    internal sealed class ElasticApplicationStatus
    {
        public string Id { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string ApplicationPath { get; set; }

        [ElasticProperty(Index = FieldIndexOption.No, Store = true, Type = FieldType.Float, 
            NumericType = NumberType.Float)]
        public float Cpu { get; set; }

        [ElasticProperty(Index = FieldIndexOption.No, Store = true, Type = FieldType.Float, 
            NumericType = NumberType.Float)]
        public float Memory { get; set; }

        [ElasticProperty(Index = FieldIndexOption.Analyzed, Type = FieldType.Date, Store = true)]
        public DateTime? LastPerformanceDataUpdateTimeUtc { get; set; }


        [ElasticProperty(Index = FieldIndexOption.Analyzed, Type = FieldType.Date, Store = true)]
        public DateTime? LastErrorTimeUtc { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string LastErrorType { get; set; }

        [ElasticProperty(Index = FieldIndexOption.Analyzed, Type = FieldType.Date, Store = true)]
        public DateTime LastUpdateTimeUtc { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string Server { get; set; }
    }
}
