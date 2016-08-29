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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

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

            try {
                logger.Info("test");
                logger.Info("test2");
                logger.Info("test3");
                logger2.Info("test-log4net");
                logger3.TraceEvent(TraceEventType.Information, 0, "test-system.diagnostics-tracesource");
                Trace.WriteLine("### test-system.diagnostics-trace");
            } catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }

        //static void TestLogStash()
        //{
        //    using (var beats = new Beats("logstash", 5044, true, "786A70526AFBC407A3DB699BBBE567891689F921")) {
        //        for (var i = 0; i < 35; i++) {
        //            beats.SendEvent("musketeer", "mprocess", DateTime.UtcNow, new Dictionary<string, object> {
        //                { "cpu", 20.0f },
        //                { "machine", "mylaptop" }
        //            });
        //        }
        //    }
        //}

        static void Main(string[] args)
        {
            SendTestLogs();

            Console.ReadKey();
        }

        /** SharpDevelop
         * 
         * 
            var publish = new Debugger.Core.Wrappers.CorPub.ICorPublish();
            var process = publish.GetProcess(pid);
            Debugger.Interop.CorPub.ICorPublishAppDomainEnum appDomainEnum;

            try {
                process.WrappedObject.EnumAppDomains(out appDomainEnum);

                if (appDomainEnum != null) {
                    uint count;
                    Debugger.Interop.CorPub.ICorPublishAppDomain appDomain;

                    try {
                        while (true) {
                            appDomainEnum.Next(1, out appDomain, out count);

                            if (count == 0)
                                break;

                            try {
                                StringBuilder sb = new StringBuilder(0x100);
                                uint strCount;

                                appDomain.GetName((uint)sb.Capacity, out strCount, sb);
                                Console.WriteLine(sb.ToString(0, (int)strCount - 1));
                            } finally {
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(appDomain);
                            }
                        }
                    } finally {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(appDomainEnum);
                    }
                }
            } finally {
            }
         */

    /**
     *  CLRMD:
        using (var target = DataTarget.AttachToProcess(pid, 1000, AttachFlag.Passive)) {
            if (target.ClrVersions.Count > 0) {
                var clrver = target.ClrVersions[0];
                var dac = clrver.TryDownloadDac();
                if (dac == null) {
                    return;
                }
                // managed process
                var runtime = target.CreateRuntime(dac);
                foreach (var appdomain in runtime.AppDomains) {
                    Console.WriteLine("Appdomain name: {0}", appdomain.Name);
                }
            }
        }
     */
}
}
