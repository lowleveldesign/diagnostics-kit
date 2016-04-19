using Nest;

namespace LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Models
{
    [ElasticsearchType(Name = "apps")]
    internal sealed class ElasticApplication
    {
        public string Id { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string Path { get; set; }

        [Number(Index = NonStringIndexOption.No, Store = true)]
        public byte? DaysToKeepLogs { get; set; }

        [Boolean(Index = NonStringIndexOption.No, Store = true)]
        public bool IsExcluded { get; set; }

        [Boolean(Index = NonStringIndexOption.No, Store = true)]
        public bool IsHidden { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string Name { get; set; }
    }
}
