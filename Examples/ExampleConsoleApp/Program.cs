using log4net;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleConsoleApp
{
    class Program
    {
        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly ILog logger2 = log4net.LogManager.GetLogger(typeof(Program));
        private static readonly TraceSource logger3 = new TraceSource("TestSource");

        static void Main(string[] args) {
            log4net.Config.XmlConfigurator.Configure();

            logger.Info("test");
            logger.Info("test2");
            logger.Info("test3");
            logger2.Info("test-log4net");
            logger3.TraceEvent(TraceEventType.Information, 0, "test-system.diagnostics-tracesource");
            Trace.WriteLine("### test-system.diagnostics-trace");
        }
    }
}
