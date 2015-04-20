using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LogStore.LuceneNet.Lucene
{
    public class SearchResults<T>
    {
        public IEnumerable<T> FoundItems { get; set; }

        public int Offset { get; set; }

        public int ItemsToReturn { get; set; }

        public int MaxItemsNumber { get; set; }
    }
}
