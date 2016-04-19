using Nest;
using System;

namespace LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Models
{
    [ElasticsearchType(Name = "globals")]
    internal sealed class ElasticGlobalSetting
    {
        public string Id { get; set; }

        [String(Index = FieldIndexOption.No, Store = true)]
        public string ConfValue { get; set; }
    }
}
