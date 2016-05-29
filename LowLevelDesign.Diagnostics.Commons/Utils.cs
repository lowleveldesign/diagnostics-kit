/**
 *  Part of the Diagnostics Kit
 *
 *  Copyright (C) 2016  Sebastian Solnica
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 */

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
