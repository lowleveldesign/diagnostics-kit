using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.ElasticSearch
{
    internal static class Helpers
    {
        public static void AddIfNotNull(this IDictionary<string, Object> dict, string key, Object v)
        {
            if (v != null)
            {
                dict.Add(key, v);
            }
        }
    }
}
