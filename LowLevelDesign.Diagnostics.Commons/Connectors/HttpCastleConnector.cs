using LowLevelDesign.Diagnostics.Commons.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace LowLevelDesign.Diagnostics.Commons.Connectors
{
    public class HttpCastleConnector : IDisposable
    {
        protected static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings {
            NullValueHandling = NullValueHandling.Ignore,
            /* HACK: for some reason the ISO format did not work with the collector. It converted
             * this value to local time, completely skipping timezone settings. */
            DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
        };
        protected readonly Uri diagnosticsAddress;

        public HttpCastleConnector(Uri uri) {
            if (uri == null) {
                throw new ArgumentException("uri");
            }

            var path = uri.AbsolutePath ?? String.Empty;
            diagnosticsAddress = uri;
        }

        /// <summary>
        /// Sends a log record to the diagnostics castle.
        /// </summary>
        /// <param name="logrec"></param>
        public void SendLogRecord(LogRecord logrec) {
            MakePostRequest(String.Format("{0}/collect", diagnosticsAddress),
                JsonConvert.SerializeObject(logrec, Formatting.None, JsonSettings));
        }

        /// <summary>
        /// Sends a batch of log records to the diagnostics castle.
        /// </summary>
        /// <param name="logrecs"></param>
        public void SendLogRecords(IEnumerable<LogRecord> logrecs) {
            MakePostRequest(String.Format("{0}/collectall", diagnosticsAddress),
                JsonConvert.SerializeObject(logrecs, Formatting.None, JsonSettings));
        }

        protected String MakeGetRequest(String url) {
            var request = WebRequest.Create(url);
            using (var reader = new StreamReader(request.GetResponse().GetResponseStream())) {
                return reader.ReadToEnd();
            }
        }

        protected String MakePostRequest(String url, String postData) {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            using (var writer = new StreamWriter(request.GetRequestStream())) {
                writer.Write(postData);
            }
            using (var reader = new StreamReader(request.GetResponse().GetResponseStream())) {
                return reader.ReadToEnd();
            }
        }

        public void Dispose() {
        }
    }
}
