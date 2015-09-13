using LowLevelDesign.Diagnostics.Commons.Models;
using System.Collections.Generic;

namespace LowLevelDesign.Diagnostics.LogStore.Commons.Models
{
    public sealed class LogSearchResults
    {
        public IEnumerable<LogRecord> FoundItems { get; set; }
    }
}
