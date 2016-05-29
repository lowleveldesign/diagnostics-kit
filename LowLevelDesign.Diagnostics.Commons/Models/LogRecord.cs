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

namespace LowLevelDesign.Diagnostics.Commons.Models
{
    public class LogRecord
    {
        public enum ELogLevel : short
        {
            Trace = 0,
            Debug,
            Info,
            Warning,
            Error,
            Critical
        }

        public String LoggerName;

        public ELogLevel LogLevel;

        public DateTime TimeUtc;

        public int ProcessId;

        public String ProcessName;

        public int ThreadId;

        public String Server;

        public String ApplicationPath;

        public String Identity;

        public String CorrelationId;

        public String Message;

        public String ExceptionType;

        public String ExceptionMessage;

        public String ExceptionAdditionalInfo;

        /// <summary>
        /// Additional field which describe the log being collected, such as for web logs:
        /// url, referer or exception data.
        /// </summary>
        public IDictionary<String, Object> AdditionalFields { get; set; }

        /// <summary>
        /// This field will be used only for logs generated from Performance Counters data.
        /// </summary>
        public IDictionary<String, float> PerformanceData { get; set; }
    }
}
