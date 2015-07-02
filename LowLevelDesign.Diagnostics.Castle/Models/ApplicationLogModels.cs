using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LowLevelDesign.Diagnostics.Castle.Models
{
    public class ApplicationLogFilterModel
    {
        public DateTime FromUtc { get; set; }

        public DateTime ToUtc { get; set; }

        public String Logger { get; set; }

        // FIXME: public LogRecord.ELogLevel[] Levels { get; set; }

        public String ApplicationPath { get; set; }

        public String Server { get; set; }

        public String Keywords { get; set; }


    }

    public class ApplicationLogSearchResult
    {
        // FIXME what should be in the results?
    }
}