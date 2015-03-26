using LowLevelDesign.Diagnostics.Commons.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;

namespace LowLevelDesign.Diagnostics.Commons.Connectors
{
    public sealed class HttpCastleConnector : IDisposable
    {
        private readonly Uri diagnosticsMasterAddress;

        /// <summary>
        /// Makes a request to the diagnostics url to gather
        /// information about the master node.
        /// </summary>
        /// <param name="uri"></param>
        public HttpCastleConnector(Uri uri) {
            if (uri == null) {
                throw new ArgumentException("uri");
            }

            // make ping request to recognize the master application
            var path = uri.AbsolutePath ?? String.Empty;
            diagnosticsMasterAddress = uri;
        }

        public void SendLogRecord(LogRecord logrec) {
            // connects to master and sends there log record information
            MakePostRequest(String.Format("{0}/collect", diagnosticsMasterAddress),
                JsonConvert.SerializeObject(logrec), diagnosticsMasterAddress.Host);
        }

        private String MakeGetRequest(String url, String host = null) {
            var request = WebRequest.Create(url);
            if (host != null) {
                request.Headers["Host"] = host;
            }
            using (var reader = new StreamReader(request.GetResponse().GetResponseStream())) {
                return reader.ReadToEnd();
            }
        }

        private String MakePostRequest(String url, String postData, String host = null) {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            if (host != null) {
                request.Host = host;
            }
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
