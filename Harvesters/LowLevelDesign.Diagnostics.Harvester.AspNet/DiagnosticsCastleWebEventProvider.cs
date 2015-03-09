using LowLevelDesign.Diagnostics.Commons.Connectors;
using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Management;

namespace LowLevelDesign.Diagnostics.Harvester.AspNet
{
    public class DiagnosticsCastleWebEventProvider : WebEventProvider
    {
        private HttpCastleConnector connector;

        public override void Initialize(String name, NameValueCollection config) {
            base.Initialize(name, config);

            // read diagnostics url and create a connector
            var url = config.Get("diagnosticsCastleUrl");
            connector = new HttpCastleConnector(new Uri(url));
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
                logrec.LogLevel = "Error";
                this.AddWebRequestInformationDataFields(logrec, ((WebRequestErrorEvent)raisedEvent).RequestInformation);
                this.AddWebThreadInformationDataFields(logrec, ((WebRequestErrorEvent)raisedEvent).ThreadInformation);
            }
            if (raisedEvent is WebErrorEvent) {
                logrec.LogLevel = "Error";
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
                logrec.LogLevel = "Error";
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
            logrec.LogLevel = "Info";
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
