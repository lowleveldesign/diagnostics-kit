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

using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.Musketeer.Config;
using LowLevelDesign.Diagnostics.Musketeer.Connectors;
using LowLevelDesign.Diagnostics.Musketeer.Jobs;
using LowLevelDesign.Diagnostics.Musketeer.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace LowLevelDesign.Diagnostics.Musketeer.Tests
{
    public class LogReaderTests : IDisposable
    {
        private readonly string logFolder;
        private readonly string appPath;
        private readonly SharedInfoAboutApps sharedInfoAboutApps;

        public LogReaderTests()
        {
            logFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(logFolder);

            appPath = "##not_existing_sample_path##";
            sharedInfoAboutApps = new SharedInfoAboutApps();
            sharedInfoAboutApps.UpdateAppWorkerProcessesMap(new Dictionary<string, Models.AppInfo>() {
                { appPath, new AppInfo {
                        LogEnabled = true,
                        Path = appPath,
                        LogsPath = logFolder,
                        ProcessIds = new [] { 123 }
                } }
            });
        }

        [Fact]
        public void TestReadLogsJob()
        {
            var connectorMock = new Mock<IMusketeerConnector>();
            var factoryMock = new Mock<IMusketeerConnectorFactory>();

            // setup log folders
            var fileName = Path.Combine(logFolder, "u_ex151006.log");
            var lines = new[] {
"#Software: Microsoft Internet Information Services 7.5",
"#Version: 1.0",
"#Date: 2015-02-06 00:00:01",
"#Fields: date time s-ip cs-method cs-uri-stem cs-uri-query s-port cs-username c-ip cs(User-Agent) sc-status sc-substatus sc-win32-status time-taken",
"2015-02-06 00:00:01 172.20.1.2 GET /czesci-samochodowe-do-mitsubishi-77/czesci-wiazki-elektryczne-51106 - 80 - 172.20.11.91 Mozilla/5.0+(compatible;+Googlebot/2.1;++http://www.google.com/bot.html) 200 0 0 312",
            };
            File.WriteAllLines(fileName, lines);

            int recordsSent = 0;
            connectorMock.Setup(c => c.SendLogRecords(It.IsAny<IEnumerable<LogRecord>>()))
                .Callback<IEnumerable<LogRecord>>((ls) => {
                    var logs = ls.ToList();
                    recordsSent = logs.Count;
                    Assert.True(recordsSent > 0);
                    var l = logs[0];

                    Assert.Equal(new DateTime(2015, 02, 06, 00, 00, 02), l.TimeUtc);
                    Assert.Equal(LogRecord.ELogLevel.Info, l.LogLevel);
                    Assert.Equal("IISLog", l.LoggerName);
                    Assert.Equal(appPath, l.ApplicationPath);
                    Object o;
                    Assert.True(l.AdditionalFields.TryGetValue("HttpStatusCode", out o));
                    Assert.Equal(o, "200.0.0");
                    Assert.True(l.AdditionalFields.TryGetValue("ClientIP", out o));
                    Assert.Equal(o, "172.20.1.4");
                    Assert.True(l.AdditionalFields.TryGetValue("Url", out o));
                    Assert.Equal(o, "/czesci-samochodowe-do-mercedesbenz-74/czesci-tloki-71901");
                });

            factoryMock.Setup(f => f.GetConnector()).Returns(connectorMock.Object);

            var job = new ReadWebAppsLogsJob(sharedInfoAboutApps, factoryMock.Object);
            job.Execute(null);

            // add new line
            lines = new[] {
"2015-02-06 00:00:02 172.20.1.2 GET /czesci-samochodowe-do-mercedesbenz-74/czesci-tloki-71901 - 80 - 172.20.1.4 Mozilla/5.0+(compatible;+Googlebot/2.1;++http://www.google.com/bot.html) 200 0 0 312"
            };
            File.AppendAllLines(fileName, lines);
            job.Execute(null);
            Assert.True(recordsSent == 1);

            // try creating a new log
            // but first append something to the old file
            lines = new[] {
"2015-07-06 12:01:03 194.14.1.4 POST /transcode.svc/customer/1/job/1/_startRegistered testv=2 80 - 194.14.1.2 - 500 0 0 62"
            };
            File.AppendAllLines(fileName, lines);

            fileName = Path.Combine(logFolder, "u_ex151007.log");

            lines = new[] {
"#Software: Microsoft Internet Information Services 7.5",
"#Version: 1.0",
"#Date: 2015-07-06 00:00:03",
"#Fields: date time s-ip cs-method cs-uri-stem cs-uri-query s-port cs-username c-ip cs(User-Agent) sc-status sc-substatus sc-win32-status time-taken",
"2015-07-06 00:00:03 194.14.1.4 POST /transcode.svc/customer/1/job/1/_startRegistered testv=1 80 - 194.14.1.2 - 500 0 0 62",
"2015-07-06 00:00:33 194.14.1.4 POST /transcode.svc/customer/2/job/1/_startRegistered - 80 - 194.14.1.2 - 500 0 0 15"
            };

            recordsSent = 0;
            connectorMock.Setup(c => c.SendLogRecords(It.IsAny<IEnumerable<LogRecord>>()))
                .Callback<IEnumerable<LogRecord>>((ls) => {
                    var logs = ls.ToList();
                    recordsSent = logs.Count;
                    Assert.True(recordsSent > 2);

                    var l = logs[0];
                    Assert.Equal(new DateTime(2015, 07, 06, 12, 01, 03), l.TimeUtc);
                    Assert.Equal(LogRecord.ELogLevel.Error, l.LogLevel);
                    Assert.Equal("IISLog", l.LoggerName);
                    Assert.Equal(appPath, l.ApplicationPath);
                    Object o;
                    Assert.True(l.AdditionalFields.TryGetValue("HttpStatusCode", out o));
                    Assert.Equal(o, "500.0.0");
                    Assert.True(l.AdditionalFields.TryGetValue("ClientIP", out o));
                    Assert.Equal(o, "194.14.1.2");
                    Assert.True(l.AdditionalFields.TryGetValue("Url", out o));
                    Assert.Equal(o, "/transcode.svc/customer/1/job/1/_startRegistered?testv=2");

                    l = logs[1];
                    Assert.Equal(new DateTime(2015, 07, 06, 00, 00, 03), l.TimeUtc);
                    Assert.Equal(LogRecord.ELogLevel.Error, l.LogLevel);
                    Assert.Equal("IISLog", l.LoggerName);
                    Assert.Equal(appPath, l.ApplicationPath);
                    Assert.True(l.AdditionalFields.TryGetValue("HttpStatusCode", out o));
                    Assert.Equal(o, "500.0.0");
                    Assert.True(l.AdditionalFields.TryGetValue("ClientIP", out o));
                    Assert.Equal(o, "194.14.1.2");
                    Assert.True(l.AdditionalFields.TryGetValue("Url", out o));
                    Assert.Equal(o, "/transcode.svc/customer/1/job/1/_startRegistered?testv=1");

                    l = logs[2];
                    Assert.Equal(new DateTime(2015, 07, 06, 00, 00, 33), l.TimeUtc);
                    Assert.Equal(LogRecord.ELogLevel.Error, l.LogLevel);
                    Assert.Equal("IISLog", l.LoggerName);
                    Assert.Equal(appPath, l.ApplicationPath);
                    Assert.True(l.AdditionalFields.TryGetValue("HttpStatusCode", out o));
                    Assert.Equal(o, "500.0.0");
                    Assert.True(l.AdditionalFields.TryGetValue("ClientIP", out o));
                    Assert.Equal(o, "194.14.1.2");
                    Assert.True(l.AdditionalFields.TryGetValue("Url", out o));
                    Assert.Equal(o, "/transcode.svc/customer/2/job/1/_startRegistered");
                });


            File.WriteAllLines(fileName, lines);

            job.Execute(null);

            Assert.Equal(recordsSent, 3);

            // remove lines and see if the parser does not break totally
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
            {
                fs.SetLength(fs.Length - 100); // we truncate last 100 bytes
            }
            recordsSent = 0;
            connectorMock.Setup(c => c.SendLogRecords(It.IsAny<IEnumerable<LogRecord>>())).Callback(() => {
                recordsSent++;
            });

            job.Execute(null); // no exception should be thrown and the file reopened

            lines = new[] {
"2015-07-06 12:01:03 194.14.1.4 POST /transcode.svc/customer/1/job/1/_startRegistered testv=2 80 - 194.14.1.2 - 500 0 0 62"
            };
            File.AppendAllLines(fileName, lines);

            job.Execute(null); // no exception should be thrown and the file reopened
            job.Execute(null); // reset of the stream

            File.AppendAllLines(fileName, lines);

            job.Execute(null); // new lines should be read

            Assert.Equal(1, recordsSent);
        }

        public void Dispose()
        {
            ReadWebAppsLogsJob.CleanupStreams();
            Directory.Delete(logFolder, true);
        }

    }
}
