using Nest;

namespace LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Models
{
    [ElasticType(Name = "apps")]
    internal sealed class ElasticApplication
    {
        public string Id { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string Path { get; set; }

        [ElasticProperty(Index = FieldIndexOption.No, Type = FieldType.Integer, Store = true)]
        public byte? DaysToKeepLogs { get; set; }

        [ElasticProperty(Index = FieldIndexOption.No, Type = FieldType.Boolean, Store = true)]
        public bool IsExcluded { get; set; }

        [ElasticProperty(Index = FieldIndexOption.No, Type = FieldType.Boolean, Store = true)]
        public bool IsHidden { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string Name { get; set; }
    }
}
