using System.Collections.Generic;

namespace LowLevelDesign.Diagnostics.Commons.LogStore
{
    public sealed class SearchResults<T>
    {
        public IEnumerable<T> FoundItems { get; set; }

        public int ItemsToReturn { get; set; }

        public int Offset { get; set; }

        public int MaxItemsNumber { get; set; }
    }
}
