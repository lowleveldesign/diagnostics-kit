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