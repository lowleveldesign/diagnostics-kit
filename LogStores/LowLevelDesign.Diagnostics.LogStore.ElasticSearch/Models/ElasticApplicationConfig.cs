using Nest;

namespace LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Models
{
    [ElasticsearchType(Name = "appconfigs")]
    internal sealed class ElasticApplicationConfig
    {
        public string Id { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string Path { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string Server { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string ServerFqdnOrIp { get; set; }

        [String(Index = FieldIndexOption.No, Store = true)]
        public string Binding { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string AppPoolName { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string ServiceName { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string DisplayName { get; set; }

        [String(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string AppType { get; set; }
    }

}
