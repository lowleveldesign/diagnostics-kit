using System;

namespace LowLevelDesign.Diagnostics.Commons.Models
{
    /// <summary>
    /// Criterias used to search for logs.
    /// </summary>
    public class LogSearchCriteria
    {
        public DateTime FromUtc { get; set; }

        public DateTime ToUtc { get; set; }

        public String Logger { get; set; }

        public LogRecord.ELogLevel[] Levels { get; set; }

        public String ApplicationPath { get; set; }

        public String Server { get; set; }

        public String Keywords { get; set; }

        public int Limit { get; set; }

        public int Offset { get; set; }
    }
}