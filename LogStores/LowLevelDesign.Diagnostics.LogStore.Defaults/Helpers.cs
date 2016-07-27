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

namespace LowLevelDesign.Diagnostics.LogStore.Defaults
{
    public static class Helpers
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
