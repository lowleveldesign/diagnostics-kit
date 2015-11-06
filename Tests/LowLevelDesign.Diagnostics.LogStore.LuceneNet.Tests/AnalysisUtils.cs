using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLuceneNetAnalysis
{
    public static class AnalysisUtils
    {
        public static void DisplayTokensWithPositions(Analyzer analyzer, String text)
        {
            TokenStream stream = analyzer.TokenStream("content", new StringReader(text));

            var term = stream.AddAttribute<ITermAttribute>();
            var posinc = stream.AddAttribute<IPositionIncrementAttribute>();

            int position = 0;
            while (stream.IncrementToken())
            {
                int increment = posinc.PositionIncrement;
                if (increment > 0)
                {
                    position = position + increment;
                    Console.WriteLine();
                    Console.Write(position + ": ");
                }
                Console.Write("[" + term.Term + "]");
            }
            Console.WriteLine();
        }

        public static void DisplayTokensWithFullDetails(Analyzer analyzer, String text)
        {
            TokenStream stream = analyzer.TokenStream("content", new StringReader(text));

            var term = stream.AddAttribute<ITermAttribute>();
            var posinc = stream.AddAttribute<IPositionIncrementAttribute>();
            var offset = stream.AddAttribute<IOffsetAttribute>();
            var type = stream.AddAttribute<ITypeAttribute>();

            int position = 0;
            while (stream.IncrementToken())
            {
                int increment = posinc.PositionIncrement;
                if (increment > 0)
                {
                    position = position + increment;
                    Console.WriteLine();
                    Console.Write(position + ": ");
                }
                Console.Write("[" + 
                                term.Term + ":" + 
                                offset.StartOffset + "->" + 
                                offset.EndOffset + ":" +
                                type.Type + "]");
            }
            Console.WriteLine();
        }
    }
}
