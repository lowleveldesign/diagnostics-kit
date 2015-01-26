using Dapper;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
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
        static void Main(string[] args) {
            FeedSqlLiteWithData();
            FeedLuceneWithData();
        }

        static void FeedLuceneWithData() {
            String luceneIndex = @"D:\data\lucene\perftest";
            if (System.IO.Directory.Exists(luceneIndex)) {
                System.IO.Directory.Delete(luceneIndex, true);
            }

            using (var iw = new IndexWriter(FSDirectory.Open(new DirectoryInfo(luceneIndex), new SimpleFSLockFactory()),
                                            new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30), IndexWriter.MaxFieldLength.UNLIMITED)) {

                var sw = new Stopwatch();
                sw.Start();

                var eventLog = new EventLog("Application");
                var tasks = new Task[Math.Min(eventLog.Entries.Count, 20)];
                bool passed = false;
                int freetask = 0, count = 0;
                foreach (EventLogEntry ele in eventLog.Entries) {
                    if (!passed && freetask < tasks.Length) {
                        var eventLogEntry = ele;
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
                    }
                    count++;
                }
                Task.WaitAll(tasks);
                sw.Stop();

                Console.WriteLine("LUCENE: {0} records inserted in {1:0,0} ms which gives {2:0.##} records / sec", count, sw.ElapsedMilliseconds, count / sw.Elapsed.TotalSeconds);
            }
        }

        static void FeedSqlLiteWithData() {
            using (var conn = new SQLiteConnection(@"Data Source=d:\data\sqlite\mydb.db;Version=3")) {
                conn.Open();

                conn.Execute("drop table if exists logs");
                conn.Execute("create table logs (Category text, [Index] integer, MachineName text, Message text, Source text, TimeGenerated integer, TimeWritten integer)");

                var sw = new Stopwatch();
                sw.Start();

                var eventLog = new EventLog("Application");
                var tasks = new Task[Math.Min(eventLog.Entries.Count, 20)];
                bool passed = false;
                int freetask = 0, count = 0;
                foreach (EventLogEntry ele in eventLog.Entries) {
                    if (!passed && freetask < tasks.Length) {
                        var eventLogEntry = ele;
                        tasks[freetask++] = conn.ExecuteAsync("insert into logs (Category, [Index], MachineName, Message, Source, TimeGenerated, TimeWritten) values " +
                            "(@Category, @Index, @MachineName, @Message, @Source, @TimeGenerated, @TimeWritten)", eventLogEntry);
                    } else {
                        passed = true;
                        freetask = Task.WaitAny(tasks);
                    }
                    count++;
                }
                Task.WaitAll(tasks);
                sw.Stop();

                Console.WriteLine("SQLITE: {0} records inserted in {1:0,0} ms which gives {2:0.##} records / sec", count, sw.ElapsedMilliseconds, count / sw.Elapsed.TotalSeconds);
            }
        }
    }
}
