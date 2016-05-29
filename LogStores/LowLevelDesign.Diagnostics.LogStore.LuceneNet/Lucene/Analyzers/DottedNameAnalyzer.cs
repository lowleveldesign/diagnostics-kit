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
using Lucene.Net.Analysis.Ext;
using System;
using System.IO;

namespace LowLevelDesign.Diagnostics.LogStore.LuceneNet.Lucene.Analyzers
{
    internal sealed class DottedNameAnalyzer : Analyzer
    {
        private sealed class SavedStreams
        {
            internal LetterOrDigitTokenizer tokenStream;
            internal TokenStream filteredTokenStream;
        }

        public override TokenStream TokenStream(String fieldName, TextReader reader) {
            // FIXME LoggerName should be split by dots
            // some specific rules for ProcessName and ApplicationPath, ThreadIdentity
            // and maybe CorrelationId - think what it should be

            return new ASCIIFoldingFilter(new LowerCaseFilter(new LetterOrDigitTokenizer(reader)));
        }

        public override TokenStream ReusableTokenStream(String fieldName, TextReader reader) {
            var streams = (SavedStreams)PreviousTokenStream;
            if (streams == null) {
                streams = new SavedStreams();
                PreviousTokenStream = streams;
                streams.tokenStream = new LetterOrDigitTokenizer(reader);
                streams.filteredTokenStream = new LowerCaseFilter(streams.tokenStream);
                streams.filteredTokenStream = new ASCIIFoldingFilter(streams.filteredTokenStream);
            } else {
                streams.tokenStream.Reset(reader);
            }
            return streams.filteredTokenStream;
        }
    }
}
