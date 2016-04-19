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
