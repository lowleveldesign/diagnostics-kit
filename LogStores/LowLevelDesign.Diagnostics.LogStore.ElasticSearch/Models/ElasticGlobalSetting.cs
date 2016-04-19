using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.ElasticSearch.Models
{
    [ElasticType(Name = "globals")]
    internal sealed class ElasticGlobalSetting
    {
        public string Id { get; set; }

        [ElasticProperty(Index = FieldIndexOption.No, Store = true)]
        public string ConfValue { get; set; }
    }
}
