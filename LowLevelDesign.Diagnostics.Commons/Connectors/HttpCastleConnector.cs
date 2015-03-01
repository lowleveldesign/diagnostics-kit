using LowLevelDesign.Diagnostics.Commons.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LowLevelDesign.Diagnostics.Commons.Connectors
{
    public sealed class HttpCastleConnector : IDisposable
    {
        private readonly SyncModel diagnosticsMasterAddress;

        public HttpCastleConnector(Uri uri) {
            if (uri == null) {
                throw new ArgumentException("uri");
            }

            // make ping request to recognize the master application
            var path = uri.AbsolutePath ?? String.Empty;
            path += path.Length > 0 && path[path.Length - 1] == '/' ? "ping" : "/ping";
            diagnosticsMasterAddress = JsonConvert.DeserializeObject<SyncModel>(MakeGetRequest(new Uri(uri, path).ToString()));
        }

        public void SendLogRecord(LogRecord logrec) {
            // connects to master and sends there log record information
            MakePostRequest(String.Format("{0}/collect", diagnosticsMasterAddress.ToUrl()),
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
            var request = WebRequest.Create(url);
            if (host != null) {
                request.Headers["Host"] = host;
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
