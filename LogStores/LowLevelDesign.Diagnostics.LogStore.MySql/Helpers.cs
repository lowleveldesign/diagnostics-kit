using System;
using System.Collections.Generic;

namespace LowLevelDesign.Diagnostics.LogStore.MySql
{
    internal static class Helpers
    {
        public static void AddIfNotNull(this IDictionary<String, Object> dict, String key, Object v)
        {
            if (v != null)
            {
                dict.Add(key, v);
            }
        }

    }
}
