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

using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LowLevelDesign.Diagnostics.Castle.Models
{
    public class ApplicationLogFilterModel
    {
        public String appname { get; set; }

        public String apppath { get; set; }

        public String server { get; set; }

        public DateTime? dfrom { get; set; }

        public DateTime? dto { get; set; }

        public String logger { get; set; }

        public short? lfrom { get; set; }

        public short? lto {get; set; }

        public String keywords { get; set; }

        public int off { get; set; }

        public static Tuple<short, String>[] Levels = new[] {
            new Tuple<short, String>((short)LogRecord.ELogLevel.Trace, "Trace"),
            new Tuple<short, String>((short)LogRecord.ELogLevel.Debug, "Debug"),
            new Tuple<short, String>((short)LogRecord.ELogLevel.Info, "Info"),
            new Tuple<short, String>((short)LogRecord.ELogLevel.Warning, "Warning"),
            new Tuple<short, String>((short)LogRecord.ELogLevel.Error, "Error"),
            new Tuple<short, String>((short)LogRecord.ELogLevel.Critical, "Critical")
        };
    }

    public class ApplicationLogSearchResults
    {
        public LogRecord[] FoundItems { get; set; }

        public int Limit { get; set; }

        public int Offset { get; set; }

        public bool IsLastPage { get; set; }
    }
}
