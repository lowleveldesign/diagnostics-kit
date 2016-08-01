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

using log4net;
using LowLevelDesign.Diagnostics.LogStash;
using NLog;
using System.Collections.Generic;
using System.Diagnostics;

namespace ExampleConsoleApp
{
    class Program
    {
        private static readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly ILog logger2 = log4net.LogManager.GetLogger(typeof(Program));
        private static readonly TraceSource logger3 = new TraceSource("TestSource");

        static void SendTestLogs()
        {
            log4net.Config.XmlConfigurator.Configure();

            logger.Info("test");
            logger.Info("test2");
            logger.Info("test3");
            logger2.Info("test-log4net");
            logger3.TraceEvent(TraceEventType.Information, 0, "test-system.diagnostics-tracesource");
            Trace.WriteLine("### test-system.diagnostics-trace");
        }

        static void Main(string[] args)
        {
            using (var beats = new Beats("localhost", 5044)) {
                for (var i = 0; i < 35; i++) {
                    beats.SendEvent("musketeer", "mprocess", new Dictionary<string, object> {
                        { "cpu", 20.0f },
                        { "machine", "mylaptop" }
                    });
                }
            }
        }
    }
}
