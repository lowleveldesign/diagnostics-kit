using Lucene.Net.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.LuceneNetLogStore.Lucene.Analyzers
{
    internal sealed class LogRecordAnalyzer : Analyzer
    {
        public override TokenStream TokenStream(string fieldName, System.IO.TextReader reader) {
            // FIXME LoggerName should be split by dots
            // some specific rules for ProcessName and ApplicationPath, ThreadIdentity
            // and maybe CorrelationId - think what it should be

            throw new NotImplementedException();
        }
    }
}
