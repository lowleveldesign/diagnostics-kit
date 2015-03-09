using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LowLevelDesign.Diagnostics.Commons
{
    public static class Utils
    {
        /// <summary>
        /// Shortens string if its length is greater than the given maximum.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static String ShortenIfNecessary(this String str, int maxLength) {
            if (str == null) {
                return null;
            }
            if (str.Length > maxLength) {
                return str.Substring(0, maxLength);
            }
            return str;
        }
    }
}
