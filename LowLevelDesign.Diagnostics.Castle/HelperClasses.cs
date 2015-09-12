using Nancy;
using Nancy.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LowLevelDesign.Diagnostics.Castle
{
    public static class Extensions
    {
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

        /// <summary>
        /// Replaces a parameter in the query string with a new value. If it does not exist
        /// it will be added.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pname"></param>
        /// <param name="pvalue"></param>
        /// <returns></returns>
        public static String ReplaceQueryParameterValue(this Url url, String pname, Object pvalue)
        {
            var query = url.Query;
            if (query == null) {
                return String.Format("?{0}={1}", pname, pvalue);
            }
            if (pvalue == null) {
                pvalue = String.Empty;
            }
            var parsedQuery = HttpUtility.ParseQueryString(query);
            parsedQuery.Set(pname, pvalue.ToString());
            var queryString = new StringBuilder("?");
            for (int i = 0; i < parsedQuery.Count; i++) {
                if (i > 0) {
                    queryString.Append("&");
                }
                queryString.AppendFormat("{0}={1}", parsedQuery.GetKey(i), HttpUtility.UrlEncode(parsedQuery.GetValues(i).First()));
            }
            return queryString.ToString();
        }
    }
}