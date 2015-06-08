using LowLevelDesign.Diagnostics.Commons.Connectors;
using LowLevelDesign.Diagnostics.Commons.Models;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLogReceptionPerformance
{
    class Program
    {
        static void Main(string[] args)
        {
            int numberOfLogs = 1000;
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

            var opt = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
            if (batchSize > 0) {
                Parallel.For(0, numberOfLogs / batchSize, opt, (i) => {
                    using (var connector = new HttpCastleConnector(new Uri(extra[0]))) {
                        var recs = new LogRecord[batchSize];
                        for (int j = 0; j < batchSize; j++) {
                            recs[j] = GenerateRandomLogRecord(i);
                        }
                        connector.SendLogRecords(recs);
                    }
                });
            } else {
                Parallel.For(0, numberOfLogs, opt, (i) => {
                    using (var connector = new HttpCastleConnector(new Uri(extra[0]))) {
                        connector.SendLogRecord(GenerateRandomLogRecord(i));
                    }
                });
            }
        }

        readonly static String[] loggerNames = new [] {
            "Test.Logger1", "Test.Logger2", "Test.Logger3", "Test.Logger4", "Test.Logger5", "Test.Logger6"
        };
        readonly static String[] messages = new [] {
            "Invalid data", "Error occured when performing a read.", "Unauthenticated user trying to access the website.",
            "Bad user credentials provided.", "Invalid number", "Format of the data is invalid."
        };
        readonly static int[] processIds = new [] {
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

        static LogRecord GenerateRandomLogRecord(int i)
        {
            var logrec = new LogRecord {
                TimeUtc = DateTime.UtcNow,
                LoggerName = loggerNames[i % loggerNames.Length],
                LogLevel = (LogRecord.ELogLevel)(i % 6),
                Message = messages[i % messages.Length],
                Server = Environment.MachineName,
                ApplicationPath = paths[i % paths.Length],
                ProcessId = processIds[i % processIds.Length],
                ProcessName = processNames[i % processNames.Length],
                ThreadId = 0,
                Identity = identities[i % identities.Length]
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
