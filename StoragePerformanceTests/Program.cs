using Dapper;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoragePerformanceTests
{
    class Program
    {
        static String luceneIndex = @"D:\data\lucene\perftest";
        static String dbpath = @"D:\data\sqlite\mydb.db";
        static int eventLogBatch = 300;

        class MyEventLogEntry
        {
            public String Category { get; set; }
            public int Index { get; set; }
            public String MachineName { get; set; }
            public String Source { get; set; }
            public String Message { get; set; }
            public int TimeGenerated { get; set; }
            public int TimeWritten { get; set; }
        }

        static void Main(string[] args) {
            FeedSQLiteWithData();
            FeedLuceneWithData();

            SearchWithSQLite();
            SearchWithLucene();
        }

        private static void SearchWithLucene() {
            var searchTerm = "windows";

            //var dateValue = DateTools.DateToString(DateTime.Today.AddDays(-7), DateTools.Resolution.MILLISECOND);
            //var filter = FieldCacheRangeFilter.NewStringRange("TimeGenerated",
            //                 lowerVal: dateValue, includeLower: true,
            //                 upperVal: null, includeUpper: false);

            var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "Message", new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30));

            using (var indexSearcher = new IndexSearcher(IndexReader.Open(FSDirectory.Open(luceneIndex), true))) {
                var sw = new Stopwatch();
                sw.Start();

                var hits = indexSearcher.Search(parser.Parse(searchTerm), 100);

                sw.Stop();
                Console.WriteLine("LUCENE: Found {0} docs in {1:0.0} ms", hits.TotalHits, sw.ElapsedMilliseconds);
                sw.Restart();

                var docs = new Document[hits.ScoreDocs.Length];
                for (int i = 0; i < docs.Length; i++) {
                    docs[i] = indexSearcher.Doc(hits.ScoreDocs[i].Doc);
                }

                sw.Stop();
                Console.WriteLine("LUCENE: Populated {0} docs in {1:0.0} ms", docs.Length, sw.ElapsedMilliseconds);
            }
        }

        private static void SearchWithSQLite() {
            var searchTerm = "windows";
            var dateValue = DateTools.DateToString(DateTime.Today.AddDays(-7), DateTools.Resolution.MILLISECOND);

            using (var conn = new SQLiteConnection(@"Data Source=" + dbpath + ";Version=3")) {
                conn.Open();

                var sw = new Stopwatch();
                sw.Start();

                var ids = conn.Query<MyEventLogEntry>("select * from logsFTS where Message MATCH @searchTerm limit 0,100", new { searchTerm }).Select(l => l.Index).ToArray();

                sw.Stop();
                Console.WriteLine("SQLITE: Found {0} docs in {1:0.0} ms", ids.Length, sw.ElapsedMilliseconds);
                sw.Restart();

                var docs = conn.Query<MyEventLogEntry>("select * from logs where [Index] in @ids", new { ids }).ToList();
                
                sw.Stop();
                Console.WriteLine("SQLITE: Populated {0} docs in {1:0.0} ms", docs.Count, sw.ElapsedMilliseconds);
            }
        }

        static void FeedLuceneWithData() {
            if (System.IO.Directory.Exists(luceneIndex)) {
                System.IO.Directory.Delete(luceneIndex, true);
            }
            var eventLogEntries = new EventLog("Application").Entries.OfType<EventLogEntry>().Reverse().Take(eventLogBatch).ToList();

            using (var iw = new IndexWriter(FSDirectory.Open(new DirectoryInfo(luceneIndex), new SimpleFSLockFactory()),
                                            new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30), IndexWriter.MaxFieldLength.UNLIMITED)) {

                var sw = new Stopwatch();
                sw.Start();

                var tasks = new Task[Math.Min(eventLogEntries.Count, 20)];
                bool passed = false;
                int freetask = 0, count = 0;
                foreach (EventLogEntry ele in eventLogEntries) {
                    var eventLogEntry = ele;
                    if (!passed && freetask < tasks.Length) {
                        tasks[freetask++] = Task.Run(() => {
                            var doc = new Document();
                            doc.Add(new Field("Category", eventLogEntry.Category, Field.Store.YES, Field.Index.ANALYZED_NO_NORMS));
                            doc.Add(new NumericField("Index", 4, Field.Store.YES, true).SetIntValue(eventLogEntry.Index));
                            doc.Add(new Field("MachineName", eventLogEntry.MachineName, Field.Store.YES, Field.Index.ANALYZED_NO_NORMS));
                            doc.Add(new Field("Message", eventLogEntry.Message, Field.Store.YES, Field.Index.ANALYZED));
                            doc.Add(new Field("Source", eventLogEntry.Source, Field.Store.YES, Field.Index.ANALYZED_NO_NORMS));
                            doc.Add(new Field("TimeGenerated", DateTools.DateToString(eventLogEntry.TimeGenerated, DateTools.Resolution.MILLISECOND), Field.Store.YES, Field.Index.ANALYZED));
                            doc.Add(new Field("TimeWritten", DateTools.DateToString(eventLogEntry.TimeWritten, DateTools.Resolution.MILLISECOND), Field.Store.YES, Field.Index.ANALYZED));

                            iw.AddDocument(doc);
                            iw.Commit();
                        });
                    } else {
                        passed = true;
                        freetask = Task.WaitAny(tasks);
                        tasks[freetask] = Task.Run(() => {
                            var doc = new Document();
                            doc.Add(new Field("Category", eventLogEntry.Category, Field.Store.YES, Field.Index.ANALYZED_NO_NORMS));
                            doc.Add(new NumericField("Index", 4, Field.Store.YES, true).SetIntValue(eventLogEntry.Index));
                            doc.Add(new Field("MachineName", eventLogEntry.MachineName, Field.Store.YES, Field.Index.ANALYZED_NO_NORMS));
                            doc.Add(new Field("Message", eventLogEntry.Message, Field.Store.YES, Field.Index.ANALYZED));
                            doc.Add(new Field("Source", eventLogEntry.Source, Field.Store.YES, Field.Index.ANALYZED_NO_NORMS));
                            doc.Add(new Field("TimeGenerated", DateTools.DateToString(eventLogEntry.TimeGenerated, DateTools.Resolution.MILLISECOND), Field.Store.YES, Field.Index.ANALYZED));
                            doc.Add(new Field("TimeWritten", DateTools.DateToString(eventLogEntry.TimeWritten, DateTools.Resolution.MILLISECOND), Field.Store.YES, Field.Index.ANALYZED));

                            iw.AddDocument(doc);
                            iw.Commit();
                        });
                    }
                    count++;
                    Console.Write("Processed entry number: {0}.\r", count);
                }
                Console.WriteLine();
                Task.WaitAll(tasks);
                sw.Stop();

                Console.WriteLine("LUCENE: {0} records inserted in {1:0,0} ms which gives {2:0.##} records / sec", count, sw.ElapsedMilliseconds, count / sw.Elapsed.TotalSeconds);
            }
        }

        static void FeedSQLiteWithData() {
            // delete the database file if exists
            if (File.Exists(dbpath)) {
                File.Delete(dbpath);
            }

            var eventLogEntries = new EventLog("Application").Entries.OfType<EventLogEntry>().Reverse().Take(eventLogBatch).ToList();

            using (var conn = new SQLiteConnection(@"Data Source=" + dbpath + ";Version=3")) {
                conn.Open();

                conn.Execute("create table logs ([Index] integer primary key, Category text, MachineName text, Message text, Source text, TimeGenerated integer, TimeWritten integer)");
                conn.Execute("create index NCIX_logs_TimeGenerated on logs (TimeGenerated)");
                conn.Execute("create virtual table logsFTS using fts4 ([Index] integer, Message text)");

                var sw = new Stopwatch();
                sw.Start();

                var tasks = new Task[Math.Min(eventLogEntries.Count, 20)];
                bool passed = false;
                int freetask = 0, count = 0;
                foreach (EventLogEntry ele in eventLogEntries) {
                    var eventLogEntry = ele;
                    if (!passed && freetask < tasks.Length) {
                        tasks[freetask++] = Task.Run(() => {
                            conn.Execute("insert into logs (Category, [Index], MachineName, Message, Source, TimeGenerated, TimeWritten) values " +
                                "(@Category, @Index, @MachineName, @Message, @Source, @TimeGenerated, @TimeWritten)", eventLogEntry);
                            conn.Execute("insert into logsFTS (TimeGenerated, Message) values (@TimeGenerated, @Message)", eventLogEntry);
                        });
                    } else {
                        passed = true;
                        freetask = Task.WaitAny(tasks);
                        tasks[freetask] = Task.Run(() => {
                            conn.Execute("insert into logs (Category, [Index], MachineName, Message, Source, TimeGenerated, TimeWritten) values " +
                                "(@Category, @Index, @MachineName, @Message, @Source, @TimeGenerated, @TimeWritten)", eventLogEntry);
                            conn.Execute("insert into logsFTS ([Index], Message) values (@Index, @Message)", eventLogEntry);
                        });
                    }
                    count++;
                    Console.Write("Processed entry number: {0}.\r", count);
                }
                Console.WriteLine();
                Task.WaitAll(tasks);
                sw.Stop();

                Console.WriteLine("SQLITE: {0} records inserted in {1:0,0} ms which gives {2:0.##} records / sec", count, sw.ElapsedMilliseconds, count / sw.Elapsed.TotalSeconds);
            }
        }
    }
}
