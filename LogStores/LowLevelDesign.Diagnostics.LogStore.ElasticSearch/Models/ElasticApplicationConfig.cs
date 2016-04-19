using Nest;

namespace LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Models
{
    [ElasticType(Name = "appconfigs")]
    internal sealed class ElasticApplicationConfig
    {
        public string Id { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string Path { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string Server { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string ServerFqdnOrIp { get; set; }

        [ElasticProperty(Index = FieldIndexOption.No, Store = true)]
        public string Binding { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string AppPoolName { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string ServiceName { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string DisplayName { get; set; }

        [ElasticProperty(Index = FieldIndexOption.NotAnalyzed, Store = true)]
        public string AppType { get; set; }
    }

}
