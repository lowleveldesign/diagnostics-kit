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

namespace LowLevelDesign.Diagnostics.LogStore.GenericDb
{
    internal class DbAppLogRecord
    {
        public Int64 Id { get; set; }
        public string LoggerName { get; set; }
        public string ApplicationPath { get; set; }
        public LogRecord.ELogLevel LogLevel { get; set; }
        public DateTime TimeUtc { get; set; }
        public int ProcessId { get; set; }
        public int ThreadId { get; set; }
        public string Server { get; set; }
        public string Identity { get; set; }
        public string CorrelationId { get; set; }
        public string Message { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionAdditionalInfo { get; set; }
        public string Host { get; set; }
        public string LoggedUser { get; set; }
        public string HttpStatusCode { get; set; }
        public string Url { get; set; }
        public string Referer { get; set; }
        public string ClientIP { get; set; }
        public string RequestData { get; set; }
        public string ResponseData { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDisplayName { get; set; }
        public string PerfData { get; set; }
    }
}
