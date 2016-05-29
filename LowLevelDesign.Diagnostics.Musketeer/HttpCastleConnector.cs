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

using LowLevelDesign.Diagnostics.Commons.Connectors;
using LowLevelDesign.Diagnostics.Commons.Models;
using LowLevelDesign.Diagnostics.Musketeer.Config;
using LowLevelDesign.Diagnostics.Musketeer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace LowLevelDesign.Diagnostics.Musketeer
{
    public interface IMusketeerHttpCastleConnectorFactory
    {
        IMusketeerHttpCastleConnector CreateCastleConnector();
    }

    public sealed class MusketeerHttpCastleConnectorFactory : IMusketeerHttpCastleConnectorFactory
    {
        private sealed class MusketeerHttpCastleConnector : HttpCastleConnector, IMusketeerHttpCastleConnector
        {
            private const int MaxLogBatchSize = 100;

            public MusketeerHttpCastleConnector() : base(MusketeerConfiguration.DiagnosticsCastleUrl)
            {
            }

            public override void SendLogRecords(IEnumerable<LogRecord> l)
            {
                var logrecs = l.ToList();
                int offset = 0;
                while (offset < logrecs.Count) {
                    base.SendLogRecords(logrecs.GetRange(offset, Math.Min(MaxLogBatchSize, logrecs.Count - offset)));
                    offset += MaxLogBatchSize;
                }
            }

            public string[] SendApplicationConfigs(IEnumerable<ApplicationServerConfig> configs)
            {
                return JsonConvert.DeserializeObject<string[]>(MakePostRequest(string.Format("{0}/conf/appsrvconfig", diagnosticsAddress),
                    JsonConvert.SerializeObject(configs, Formatting.None, JsonSettings)), JsonSettings);
            }

            public ApplicationUpdate GetInformationAboutMusketeerUpdates()
            {
                return JsonConvert.DeserializeObject<ApplicationUpdate>(MakeGetRequest(string.Format("{0}/about-musketeer",
                    diagnosticsAddress)));
            }

            public void DownloadFile(string url, string outputFilePath)
            {
                using (var webClient = new WebClient()) {
                    webClient.DownloadFile(url, outputFilePath);
                }
            }
        }

        public IMusketeerHttpCastleConnector CreateCastleConnector()
        {
            return new MusketeerHttpCastleConnector();
        }
    }

    public interface IMusketeerHttpCastleConnector : IDisposable
    {
        void SendLogRecord(LogRecord logrec);

        void SendLogRecords(IEnumerable<LogRecord> logrecs);

        string[] SendApplicationConfigs(IEnumerable<ApplicationServerConfig> configs);

        ApplicationUpdate GetInformationAboutMusketeerUpdates();

        void DownloadFile(string url, string outputFilePath);
    }

}
