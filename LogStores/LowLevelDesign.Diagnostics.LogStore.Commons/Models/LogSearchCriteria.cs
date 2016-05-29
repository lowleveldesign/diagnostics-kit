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

namespace LowLevelDesign.Diagnostics.LogStore.Commons.Models
{
    /// <summary>
    /// Criterias used to search for logs.
    /// </summary>
    public sealed class LogSearchCriteria
    {
        public DateTime FromUtc { get; set; }

        public DateTime ToUtc { get; set; }

        public string Logger { get; set; }

        public LogRecord.ELogLevel[] Levels { get; set; }

        public string ApplicationPath { get; set; }

        public string Server { get; set; }

        public KeywordsParsed Keywords { get; set; }

        public int Limit { get; set; }

        public int Offset { get; set; }
    }

    public sealed class KeywordsParsed
    {
        public string HttpStatus { get; set; }

        public string ClientIp { get; set; }

        public string Url { get; set; }

        public string Service { get; set; }

        public string FreeText { get; set; }
    }
}
