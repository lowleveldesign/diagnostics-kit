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
using System.Web.Configuration;
using System.Web.Management;

namespace LowLevelDesign.Diagnostics.Harvester.AspNet
{
    public class DiagnosticsKitWebEventProvider : WebEventProvider
    {
        private static readonly HttpCastleConnector connector;

        static DiagnosticsKitWebEventProvider() {
            var diagurl = WebConfigurationManager.AppSettings["lowleveldesign.diagnostics.url"];
            Uri uri;
            if (!Uri.TryCreate(diagurl, UriKind.Absolute, out uri)) {
                throw new ConfigurationErrorsException("Please check lowleveldesign.diagnostics.url key in the appSettings - its value should contain an url " + 
                    "pointing to the diagnostics main application (Castle).");
            }
            connector = new HttpCastleConnector(uri);
        }

        public override void Initialize(String name, NameValueCollection config) {
            base.Initialize(name, config);
        }

        public override void Flush() {
            // not used
        }

        public override void ProcessEvent(WebBaseEvent raisedEvent) {
            var logrec = new LogRecord { AdditionalFields = new Dictionary<String, Object>() };

            this.AddBasicDataFields(logrec, raisedEvent);
            if (raisedEvent is WebManagementEvent) {
                this.AddWebProcessInformationDataFields(logrec, ((WebManagementEvent)raisedEvent).ProcessInformation);
            }
            if (raisedEvent is WebRequestEvent) {
                this.AddWebRequestInformationDataFields(logrec, ((WebRequestEvent)raisedEvent).RequestInformation);
            }
            if (raisedEvent is WebBaseErrorEvent) {
                this.AddExceptionDataFields(logrec, ((WebBaseErrorEvent)raisedEvent).ErrorException);
            }
            if (raisedEvent is WebAuditEvent) {
                this.AddWebRequestInformationDataFields(logrec, ((WebAuditEvent)raisedEvent).RequestInformation);
            }
            if (raisedEvent is WebRequestErrorEvent) {
                logrec.LogLevel = LogRecord.ELogLevel.Error;
                this.AddWebRequestInformationDataFields(logrec, ((WebRequestErrorEvent)raisedEvent).RequestInformation);
                this.AddWebThreadInformationDataFields(logrec, ((WebRequestErrorEvent)raisedEvent).ThreadInformation);
            }
            if (raisedEvent is WebErrorEvent) {
                logrec.LogLevel = LogRecord.ELogLevel.Error;
                this.AddWebRequestInformationDataFields(logrec, ((WebErrorEvent)raisedEvent).RequestInformation);
                this.AddWebThreadInformationDataFields(logrec, ((WebErrorEvent)raisedEvent).ThreadInformation);
            }
            if (raisedEvent is WebAuthenticationSuccessAuditEvent) {
                logrec.AdditionalFields.Add("UserToSignIn", ((WebAuthenticationSuccessAuditEvent)raisedEvent).NameToAuthenticate);
            }
            if (raisedEvent is WebAuthenticationFailureAuditEvent) {
                logrec.AdditionalFields.Add("UserToSignIn", ((WebAuthenticationFailureAuditEvent)raisedEvent).NameToAuthenticate);
            }
            if (raisedEvent is WebViewStateFailureAuditEvent) {
                logrec.LogLevel = LogRecord.ELogLevel.Error;
                this.AddExceptionDataFields(logrec, ((WebViewStateFailureAuditEvent)raisedEvent).ViewStateException);
            }

            connector.SendLogRecord(logrec);
        }

        private void AddBasicDataFields(LogRecord logrec, WebBaseEvent raisedEvent) {
            var applicationInformation = WebBaseEvent.ApplicationInformation;

            logrec.Server = applicationInformation.ApplicationPath;
            logrec.Server = applicationInformation.MachineName;
            logrec.TimeUtc = raisedEvent.EventTimeUtc;
            logrec.Message = raisedEvent.Message;
            logrec.LogLevel = LogRecord.ELogLevel.Info;
            logrec.LoggerName = String.Format("ASP.NET Health: {0}.{1}", raisedEvent.EventCode, raisedEvent.EventDetailCode);
        }

        private void AddWebProcessInformationDataFields(LogRecord logrec, WebProcessInformation procinfo) {
            logrec.ProcessId = procinfo.ProcessID;
            logrec.ProcessName = procinfo.ProcessName;
            logrec.Identity = procinfo.AccountName;
        }

        private void AddWebRequestInformationDataFields(LogRecord logrec, WebRequestInformation reqinfo) {
            logrec.Identity = reqinfo.ThreadAccountName;
            logrec.AdditionalFields.Add("ClientIP", reqinfo.UserHostAddress);
            logrec.AdditionalFields.Add("Url", reqinfo.RequestUrl);
            if (reqinfo.Principal != null) {
                logrec.AdditionalFields.Add("LoggedUser", reqinfo.Principal.Identity.Name);
            }
        }

        private void AddExceptionDataFields(LogRecord logrec, Exception ex) {
            if (ex != null) {
                logrec.ExceptionType = ex.GetType().FullName;
                logrec.ExceptionMessage = ex.Message;
                logrec.ExceptionAdditionalInfo = ex.StackTrace;
            }
        }

        private void AddWebThreadInformationDataFields(LogRecord logrec, WebThreadInformation threadinfo) {
            logrec.ThreadId = threadinfo.ThreadID;
            logrec.Identity = threadinfo.ThreadAccountName;
        }

        public override void Shutdown() {
            connector.Dispose();
        }
    }
}
