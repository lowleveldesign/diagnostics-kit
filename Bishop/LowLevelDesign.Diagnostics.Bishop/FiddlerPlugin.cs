using Fiddler;
using LowLevelDesign.Diagnostics.Bishop.Common;
using LowLevelDesign.Diagnostics.Bishop.Config;
using LowLevelDesign.Diagnostics.Bishop.Tampering;
using LowLevelDesign.Diagnostics.Bishop.UI;
using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace LowLevelDesign.Diagnostics.Bishop
{
    public class FiddlerPlugin : IAutoTamper
    {
        private readonly string configurationFilePath = Path.Combine(Path.GetDirectoryName(
            Assembly.GetExecutingAssembly().Location), "bishop.conf");
        private readonly ReaderWriterLockSlim lck = new ReaderWriterLockSlim();

        private PluginSettings settings;
        private CustomTamperingRulesContainer tamperer;
        private ServerRedirectionRulesContainer serverRedirector;
        private bool isLoaded;
        private bool shouldInterceptHttps;
        private string selectedServer;
        private string customServerAddressWithPort;
        private bool isRedirectionToOneHostEnabled;

        public void OnLoad()
        {
            try {
                ReloadSettings();

                FiddlerApplication.UI.Menu.MenuItems.Add(new PluginMenu(this).PrepareMenu());
                isLoaded = true;
            } catch (Exception ex) {
                MessageBox.Show("There was a problem while loading the Bishop plugin. Please check the Fiddler log for details", 
                    "Bishop is dead.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogFormat("Bishop error: {0}", ex);
            }
        }

        public void ReloadSettings()
        {

            lck.EnterWriteLock();
            try {
                settings = PluginSettings.Load(configurationFilePath);
                // FIXME make sure that settings did change
                tamperer = new CustomTamperingRulesContainer(settings);

                serverRedirector = new ServerRedirectionRulesContainer(RetrieveApplicationServerConfigs());
            } finally { 
                lck.ExitWriteLock();
            }
        }

        private IEnumerable<ApplicationServerConfig> RetrieveApplicationServerConfigs()
        {
            if (IsDiagnosticsCastleConfigured()) {
                try {
                    return new BishopHttpCastleConnector(settings).ReadApplicationConfigs();
                } catch (Exception ex) {
                    LogFormat("Bishop error: {0}", ex);
                    MessageBox.Show("There was a problem while connecting with the Diagnostics Castle - please review the settings. " +
                        "Probably something is wrong with them.", "Error in Bishop.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return new ApplicationServerConfig[0];
        }

        public bool IsDiagnosticsCastleConfigured()
        {
            return settings.DiagnosticsUrl != null;
        }

        public void SetHttpsLocalInterception(bool enabled) {
            shouldInterceptHttps = enabled;
        }

        private void Log(string message)
        {
            FiddlerApplication.Log.LogFormat("[Bishop][Thread: {0}] {1}", Thread.CurrentThread.ManagedThreadId, message);
        }

        private void LogFormat(string format, params object[] args)
        {
            Log(string.Format(format, args));
        }

        public void AutoTamperRequestBefore(Session oSession)
        {
            if (!isLoaded) {
                return;
            }
            if (!lck.TryEnterReadLock(TimeSpan.FromSeconds(1))) {
                return;
            }
            try {
                var request = new Request(oSession);

                if (request.IsHttpsConnect) {
                    if (request.IsLocal && shouldInterceptHttps) {
                        PerformHttpsHandshake(oSession);
                    }
                    return;
                }

                var tamperingContext = new TamperingContext();
                if (request.IsLocal && request.IsHttps && shouldInterceptHttps) {
                    ApplyHttpsRedirection(request, tamperingContext);
                }
                tamperer.ApplyMatchingTamperingRules(request, tamperingContext);
                if (isRedirectionToOneHostEnabled) {
                    tamperingContext.ServerTcpAddressWithPort = customServerAddressWithPort;
                } else if (IsApplicationServerSelected()) {
                    serverRedirector.ApplyMatchingTamperingRules(request, tamperingContext, selectedServer);
                }

                if (tamperingContext.ShouldTamperRequest)
                {
                    TamperRequest(oSession, tamperingContext);
                }
            } finally {
                lck.ExitReadLock();
            }
        }

        private void PerformHttpsHandshake(Session fiddlerSession)
        {
            Log("Performing handshake on behalf of the application.");
            fiddlerSession.oFlags["x-replywithtunnel"] = "true";
        }

        private void ApplyHttpsRedirection(IRequest request, TamperingContext tamperParams)
        {
            var localPort = settings.FindLocalPortForHttpsRedirection(request.Port);
            if (localPort > 0) {
                Log(string.Format("Redirecting local https to http :{0}->:{1}.", request.Port, localPort));
                request.SetHeader("X-OriginalBaseUri", string.Format("https://{0}", request.Host));
                tamperParams.ServerTcpAddressWithPort = string.Format("localhost:{0}", localPort);
            }
        }

        private bool IsApplicationServerSelected()
        {
            return selectedServer != null;
        }

        private void TamperRequest(Session fiddlerSession, TamperingContext tamperingContext)
        {
            LogFormat("Tampering request: {0}", fiddlerSession.url);

            var fullUrl = fiddlerSession.fullUrl;

            if (!string.IsNullOrEmpty(tamperingContext.ServerTcpAddressWithPort)) {
                fullUrl = fullUrl.Replace(fiddlerSession.host, tamperingContext.ServerTcpAddressWithPort);
            }
            if (!string.IsNullOrEmpty(tamperingContext.HostHeader))
            {
                fiddlerSession.fullUrl = "http://" + tamperingContext.HostHeader + fiddlerSession.PathAndQuery;
                // fullUrl won't be a host but rather host header with different IP
                fullUrl = fiddlerSession.fullUrl;
            }
            // FIXME redirect from https to http
            // fiddlerSession.fullUrl = fiddlerSession.fullUrl.Replace("https://", "http://");

            fiddlerSession["X-OverrideHost"] = tamperingContext.ServerTcpAddressWithPort;
            fiddlerSession.bypassGateway = true;
            LogFormat("IP changed to {0}", tamperingContext.ServerTcpAddressWithPort);
            LogFormat("Url set to {0}", fullUrl);
        }

        public void AutoTamperRequestAfter(Session oSession)
        {
        }

        public void AutoTamperResponseAfter(Session oSession)
        {
        }

        public void AutoTamperResponseBefore(Session oSession)
        {
        }

        public void OnBeforeReturningError(Session oSession)
        {
        }

        public void OnBeforeUnload()
        {
        }

        public void SelectCustomServer(string customServerAddressWithPort)
        {
            lck.EnterWriteLock();
            try {
                isRedirectionToOneHostEnabled = true;
                this.customServerAddressWithPort = customServerAddressWithPort;
            } finally {
                lck.ExitWriteLock();
            }
        }

        public void SelectApplicationServer(string srv)
        {
            lck.EnterWriteLock();
            try {
                isRedirectionToOneHostEnabled = false;
                selectedServer = srv;
            } finally {
                lck.ExitWriteLock();
            }
        }

        public void TurnOffServerRedirection()
        {
            lck.EnterWriteLock();
            try {
                customServerAddressWithPort = null;
                selectedServer = null;
                isRedirectionToOneHostEnabled = false;
            } finally {
                lck.ExitWriteLock();
            }
        }

        public string PluginConfigurationFilePath { get { return configurationFilePath; } }

        public IEnumerable<string> AvailableServers { get { return serverRedirector.AvailableServers; } }
    }
}
