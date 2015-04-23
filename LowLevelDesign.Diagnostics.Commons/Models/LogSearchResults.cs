using System.Collections.Generic;

namespace LowLevelDesign.Diagnostics.Commons.Models
{
    public sealed class LogSearchResults
    {
        public IEnumerable<LogRecord> FoundItems { get; set; }
    }
}
