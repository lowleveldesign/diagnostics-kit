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

            var path = uri.AbsolutePath ?? string.Empty;
            diagnosticsAddress = uri;
        }

        /// <summary>
        /// Sends a log record to the diagnostics castle.
        /// </summary>
        /// <param name="logrec"></param>
        public virtual void SendLogRecord(LogRecord logrec) {
            MakePostRequest(string.Format("{0}/collect", diagnosticsAddress),
                JsonConvert.SerializeObject(logrec, Formatting.None, JsonSettings));
        }

        /// <summary>
        /// Sends a batch of log records to the diagnostics castle.
        /// </summary>
        /// <param name="logrecs"></param>
        public virtual void SendLogRecords(IEnumerable<LogRecord> logrecs) {
            MakePostRequest(string.Format("{0}/collectall", diagnosticsAddress),
                JsonConvert.SerializeObject(logrecs, Formatting.None, JsonSettings));
        }

        protected string MakeGetRequest(string url) {
            var request = WebRequest.Create(url);
            using (var reader = new StreamReader(request.GetResponse().GetResponseStream())) {
                return reader.ReadToEnd();
            }
        }

        protected string MakePostRequest(string url, string postData) {
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

        public virtual void Dispose() {
        }
    }
}
