using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LowLevelDesign.Diagnostics.Castle.Models
{
    public class ApplicationLogFilterModel
    {
        public String ApplicationName { get; set; }

        public String ApplicationPath { get; set; }

        public String Server { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime DateTo { get; set; }

        public String Logger { get; set; }

        public short LevelFrom { get; set; }

        public short LevelTo {get; set; }

        public String Keywords { get; set; }

        public Tuple<short, String>[] Levels = new[] {
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
    }
}