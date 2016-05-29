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
using LowLevelDesign.Diagnostics.Commons.Models;

namespace LowLevelDesign.Diagnostics.LogStore.MySql
{
    internal class DbAppLogRecord
    {
        public Int64 Id { get; set; }
        public String LoggerName { get; set; }
        public String ApplicationPath { get; set; }
        public LogRecord.ELogLevel LogLevel { get; set; }
        public DateTime TimeUtc { get; set; }
        public int ProcessId { get; set; }
        public int ThreadId { get; set; }
        public String Server { get; set; }
        public String Identity { get; set; }
        public String CorrelationId { get; set; }
        public String Message { get; set; }
        public String ExceptionMessage { get; set; }
        public String ExceptionType { get; set; }
        public String ExceptionAdditionalInfo { get; set; }
        public String Host { get; set; }
        public String LoggedUser { get; set; }
        public String HttpStatusCode { get; set; }
        public String Url { get; set; }
        public String Referer { get; set; }
        public String ClientIP { get; set; }
        public String RequestData { get; set; }
        public String ResponseData { get; set; }
        public String ServiceName { get; set; }
        public String ServiceDisplayName { get; set; }
        public String PerfData { get; set; }
    }
}
