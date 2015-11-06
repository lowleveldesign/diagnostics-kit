using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestLuceneNetAnalysis.Analyzers;

namespace TestLuceneNetAnalysis
{
    class Program
    {
        static void Main(string[] args) {
            String[] logs = { "Test.Logger.MyLogger", "System.ArgumentException", "Invalid argument was passed to the method.", 
                                       "http://www.test.pl/test?s=123&sdf=1231&logger=test" };

            Analyzer[] analyzers = new Analyzer[] { new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30),
                                           new LogRecordAnalyzer() };

            foreach (var analyzer in analyzers) {
                foreach (var log in logs) {
                    Console.WriteLine("analyzer: {0}, sentence: {1}", analyzer.GetType().Name, log);
                    AnalysisUtils.DisplayTokensWithFullDetails(analyzer, log);
                    Console.WriteLine("----------------------------------");
                }
            }
        }
    }
}
