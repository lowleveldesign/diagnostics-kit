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
using Lucene.Net.Analysis.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLuceneNetAnalysis
{
    class Program
    {
        static void Main(string[] args) {
            //String[] logs = { "Test.Logger.MyLogger", "System.ArgumentException", "Invalid argument was passed to the method.", 
            //                           "http://www.test.pl/test?s=123&sdf=1231&logger=test" };

            //Analyzer[] analyzers = new Analyzer[] { new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30),
            //                               new LogRecordAnalyzer() };

            //foreach (var analyzer in analyzers) {
            //    foreach (var log in logs) {
            //        Console.WriteLine("analyzer: {0}, sentence: {1}", analyzer.GetType().Name, log);
            //        AnalysisUtils.DisplayTokensWithFullDetails(analyzer, log);
            //        Console.WriteLine("----------------------------------");
            //    }
            //}
        }
    }
}
