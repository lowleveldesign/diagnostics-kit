using Nest;
using System;

namespace LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Models
{
    [ElasticsearchType(Name = "appstats")]
    internal sealed class ElasticApplicationStatus
    {
        public string Id { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string ApplicationPath { get; set; }

        [Number(Index = NonStringIndexOption.No, Store = true)]
        public float Cpu { get; set; }

        [Number(Index = NonStringIndexOption.No, Store = true)]
        public float Memory { get; set; }

        [Date(Store = true)]
        public DateTime? LastPerformanceDataUpdateTimeUtc { get; set; }


        [Date(Store = true)]
        public DateTime? LastErrorTimeUtc { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string LastErrorType { get; set; }

        [Date(Store = true)]
        public DateTime LastUpdateTimeUtc { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string Server { get; set; }
    }
}
