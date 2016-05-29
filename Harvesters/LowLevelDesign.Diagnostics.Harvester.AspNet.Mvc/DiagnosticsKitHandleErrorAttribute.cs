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
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace LowLevelDesign.Diagnostics.Harvester.AspNet.Mvc
{
    public sealed class DiagnosticsKitHandleErrorAttribute : HandleErrorAttribute
    {
        private const int maxHttpDataLength = 1024;

        private static readonly HttpCastleConnector connector;
        private static readonly Process process;

        static DiagnosticsKitHandleErrorAttribute() {
            var diagurl = ConfigurationManager.AppSettings["lowleveldesign.diagnostics.url"];
            Uri uri;
            if (!Uri.TryCreate(diagurl, UriKind.Absolute, out uri)) {
                throw new ConfigurationErrorsException("Please check lowleveldesign.diagnostics.url key in the appSettings - its value should contain an url " +
                    "pointing to the diagnostics main application (Castle).");
            }
            connector = new HttpCastleConnector(uri);
            process = Process.GetCurrentProcess();
        }

        private String ExtractHeaders(StringBuilder buffer, NameValueCollection headers) {
            buffer.Clear();
            try {
                foreach (String header in headers) {
                    buffer.AppendFormat("{0}={1}", header, headers[header]).AppendLine();
                }
            } catch (ArgumentOutOfRangeException) { /* it's normal if we exceed http data limit */ }

            return buffer.ToString();
        }

        public override void OnException(ExceptionContext filterContext) {
            if (filterContext.ExceptionHandled) {
                return;
            }
            const String duplicateKey = "__llddiag_alreadyserved";
            if (filterContext.HttpContext.Items.Contains(duplicateKey)) {
                return;
            }
            filterContext.HttpContext.Items.Add(duplicateKey, true);

            var request = filterContext.HttpContext.Request;
            var response = filterContext.HttpContext.Response;
            var ex = filterContext.Exception;

            var buffer = new StringBuilder(maxHttpDataLength, maxHttpDataLength);

            var logrec = new LogRecord {
                ExceptionType = ex.GetType().FullName,
                ExceptionMessage = ex.Message,
                ExceptionAdditionalInfo = ex.StackTrace,
                Identity = Thread.CurrentPrincipal.Identity.Name,
                LoggerName = String.Format("MVC.{0}.{1}", filterContext.RouteData.Values["controller"], filterContext.RouteData.Values["action"]),
                LogLevel = LogRecord.ELogLevel.Error,
                ProcessId = process.Id,
                ProcessName = process.ProcessName,
                Server = Environment.MachineName,
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                TimeUtc = DateTime.UtcNow,
                AdditionalFields = new Dictionary<String, Object> {
                    { "Url", request.RawUrl },
                    { "HttpStatusCode", response.StatusCode },
                    { "ClientIP", request.UserHostAddress },
                    { "RequestData", ExtractHeaders(buffer, request.Headers) },
                    { "ResponseData", ExtractHeaders(buffer, response.Headers) }
                }
            };

            try {
                logrec.ApplicationPath = HttpRuntime.AppDomainAppPath;
            } catch {
                logrec.ApplicationPath = AppDomain.CurrentDomain.BaseDirectory;
            }

            connector.SendLogRecord(logrec);
        }
    }
}
