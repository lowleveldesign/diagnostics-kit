using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LowLevelDesign.Diagnostics.Castle
{
    public static class Extensions
    {
        /// <summary>
        /// Shortens string if its length is greater than the given maximum.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static String ShortenAndDotIfNecessary(this String str, int maxLength) {
            if (str == null) {
                return null;
            }
            if (str.Length > maxLength) {
                return str.Substring(0, maxLength - 3) + "...";
            }
            return str;
        }

        /// <summary>
        /// Returns a value of a given counter from the performance data 
        /// dictionary.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="counter"></param>
        /// <returns></returns>
        public static String GetCounterValueIfAvailable(this IDictionary<String, float> data, String counter)
        {
            float res;
            if (data == null || !data.TryGetValue(counter, out res)) {
                return "-";
            }
            return res.ToString("#,0.00");
        }
    }
}