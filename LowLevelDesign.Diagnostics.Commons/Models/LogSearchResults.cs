using System.Collections.Generic;

namespace LowLevelDesign.Diagnostics.Commons.Storage
{
    public sealed class LogSearchResults
    {
        public IEnumerable<LogSearchResults> FoundItems { get; set; }
    }
}
