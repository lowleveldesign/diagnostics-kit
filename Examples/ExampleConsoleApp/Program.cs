using log4net;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleConsoleApp
{
    class Program
    {
        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly ILog logger2 = log4net.LogManager.GetLogger(typeof(Program));

        static void Main(string[] args) {
            log4net.Config.XmlConfigurator.Configure();

            logger.Info("test");
            logger2.Info("test-log4net");
        }
    }
}
