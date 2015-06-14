using LowLevelDesign.Diagnostics.Commons.Connectors;
using LowLevelDesign.Diagnostics.Commons.Models;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestLogReceptionPerformance
{
    class Program
    {
        static void Main(string[] args)
        {
            int numberOfLogs = 10000;
            int batchSize = 0;

            var p = new OptionSet {
                { "n|nlogs=", "number of logs to generate", v => numberOfLogs = Int32.Parse(v) },
                { "b|batchsize=", "send logs in batches", v => batchSize = Int32.Parse(v) },
            };

            List<String> extra;
            try {
                extra = p.Parse(args);
            } catch (Exception ex) {
                Console.Write("testlog: ");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Try `testlog --help' for more information.");
                return;
            }

            if (extra.Count == 0) {
                Console.WriteLine("ERROR: Missing diagnostics url.");
                Console.WriteLine();
                p.WriteOptionDescriptions(Console.Out);
            }

            int errorsCnt = 0;
            int recordsGenerated = 0;
            long ticks = 0;

            using (new Timer((o) => {
                Console.Write("\rProcessed: {0} / {1}               ", recordsGenerated, numberOfLogs);
            }, null, 0, 500)) {
                using (var connector = new HttpCastleConnector(new Uri(extra[0]))) {
                    var opt = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
                    if (batchSize > 0) {
                        Parallel.For(0, numberOfLogs / batchSize, opt, (i) => {
                            var recs = new LogRecord[batchSize];
                            for (int j = 0; j < batchSize; j++) {
                                recs[j] = GenerateRandomLogRecord();
                            }
                            try {
                                var sw = new Stopwatch();
                                sw.Start();
                                connector.SendLogRecords(recs);
                                sw.Stop();

                                Interlocked.Add(ref ticks, sw.ElapsedTicks);
                                Interlocked.Add(ref recordsGenerated, batchSize);
                            } catch (Exception ex) {
                                Interlocked.Increment(ref errorsCnt);
                                Console.WriteLine("Error occured: {0} - {1}", ex.GetType().FullName, ex.Message);
                            }
                        });
                    } else {
                        Parallel.For(0, numberOfLogs, opt, (i) => {
                            try {
                                var sw = new Stopwatch();
                                sw.Start();
                                connector.SendLogRecord(GenerateRandomLogRecord());
                                sw.Stop();

                                Interlocked.Add(ref ticks, sw.ElapsedTicks);
                                Interlocked.Increment(ref recordsGenerated);
                            } catch (Exception ex) {
                                Interlocked.Increment(ref errorsCnt);
                                Console.WriteLine("Error occured: {0} - {1}", ex.GetType().FullName, ex.Message);
                            }
                        });
                    }
                }

                var ts = new TimeSpan(ticks);
                Console.WriteLine("\rRecords generated: {0}, errors: {1}, time: {2:#,#} ms which gives {3:0.000} processed records / sec", recordsGenerated,
                    errorsCnt, ts.TotalMilliseconds, recordsGenerated / ts.TotalSeconds);
            }
        }

        readonly static String[] loggerNames = new[] {
            "Test.Logger1", "Test.Logger2", "Test.Logger3", "Test.Logger4", "Test.Logger5", "Test.Logger6"
        };
        readonly static String[] messages = new[] {
            "Invalid data", "Error occured when performing a read.", "Unauthenticated user trying to access the website.",
            "Bad user credentials provided.", "Invalid number", "Format of the data is invalid."
        };
        readonly static int[] processIds = new[] {
            123, 234, 345, 454, 376, 3345
        };
        readonly static String[] processNames = new[] {
            "app1", "app2", "testapp1", "testapp2", "testapp3"
        };
        readonly static String[] identities = new[] {
            "identity1", "identity2", "identity3", "identity4"
        };
        readonly static String[] paths = new[] {
            @"C:\testapp1\", @"c:\testapp2\", @"c:\testapp3\",
            @"\\share1", @"\\share2"
        };
        readonly static String[] servers = new[] {
            "SRV1", "SRV2", "SRV3"
        };

        static LogRecord GenerateRandomLogRecord()
        {
            var rnd = new Random();

            var logrec = new LogRecord {
                TimeUtc = DateTime.UtcNow,
                LoggerName = loggerNames[rnd.Next() % loggerNames.Length],
                LogLevel = (LogRecord.ELogLevel)(rnd.Next() % 6),
                Message = messages[rnd.Next() % messages.Length],
                Server = servers[rnd.Next() % servers.Length],
                ApplicationPath = paths[rnd.Next() % paths.Length],
                ProcessId = processIds[rnd.Next() % processIds.Length],
                ProcessName = processNames[rnd.Next() % processNames.Length],
                ThreadId = 0,
                Identity = identities[rnd.Next() % identities.Length],
                PerformanceData = new Dictionary<String, float> {
                    { "CPU", (float)(rnd.NextDouble() * 100.0) },
                    { "Memory", (float)(rnd.NextDouble() * 10000.0) }
                }
            };

            if (logrec.LogLevel == LogRecord.ELogLevel.Error ||
                logrec.LogLevel == LogRecord.ELogLevel.Critical) {
                logrec.ExceptionType = typeof(ArgumentException).FullName;
                logrec.ExceptionMessage = "Argument exception occured";
            }
            return logrec;
        }
    }
}
