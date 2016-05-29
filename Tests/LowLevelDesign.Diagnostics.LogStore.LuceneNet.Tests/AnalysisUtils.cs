/**
 *  Part of the Diagnostics Kit
 *
 *  Copyright (C) 2016  Sebastian Solnica
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 */

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
