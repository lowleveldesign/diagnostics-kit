using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

        public LogRecord.ELogLevel LevelFrom { get; set; }

        public LogRecord.ELogLevel LevelTo { get; set; }

        public String Keywords { get; set; }
    }
}