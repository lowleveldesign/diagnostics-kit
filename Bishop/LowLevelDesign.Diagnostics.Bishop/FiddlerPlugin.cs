using Fiddler;
using LowLevelDesign.Diagnostics.Bishop.Common;
using LowLevelDesign.Diagnostics.Bishop.Config;
using LowLevelDesign.Diagnostics.Bishop.Tampering;
using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Diagnostics;
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

        private PluginSettings settings;
        private CustomTamperingRulesContainer tamperer;
        private ServerRedirectionRulesContainer serverRedirector;
        private bool isLoaded;
        private bool shouldInterceptHttps;
        private string selectedServer;
        private bool isRedirectionToOneHostEnabled;
        private bool tamperRequests;

        public void OnLoad()
        {
            try {
                settings = PluginSettings.Load(configurationFilePath);
                tamperer = new CustomTamperingRulesContainer(settings);

                if (!IsDiagnosticsCastleConfigured()) {
                    serverRedirector = new ServerRedirectionRulesContainer(new ApplicationServerConfig[0]);
                } else {
                    // FIXME connect with the Diagnostics Castle
                }

                // load menu items for Bishop
                FiddlerApplication.UI.Menu.MenuItems.Add(PrepareMenu());

                isLoaded = true;
            } catch (Exception ex) {
                MessageBox.Show("There was a problem while loading the Bishop plugin. Please check the Fiddler log for details", 
                    "Bishop is dead.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogFormat("Bishop error: {0}", ex);
            }
        }

        private bool IsDiagnosticsCastleConfigured()
        {
            return settings.DiagnosticsUrl != null;
        }

        private MenuItem PrepareMenu()
        {
            var bishopMenu = new MenuItem("&Bishop");
            MenuItem it;

            // Castle
            if (!IsDiagnosticsCastleConfigured()) {
                it = new MenuItem("Configure &Castle connection...") {
                    Name = "miConfigureCastle"
                };
                // FIXME configuration dialog
                bishopMenu.MenuItems.Add(it);
                it = new MenuItem("-");
                bishopMenu.MenuItems.Add(it);
            }

            // HTTPS -> HTTP emulator
            it = new MenuItem("&Emulate HTTPS (localhost)") {
                Name = "miEnableTlsEdgeRouter",
                RadioCheck = false,
                Checked = false
            };
            it.Click += (o, ev) => {
                var mi = (MenuItem)o;
                shouldInterceptHttps = mi.Checked = !mi.Checked;
            };
            bishopMenu.MenuItems.Add(it);

            it = new MenuItem("-");
            bishopMenu.MenuItems.Add(it);

            // FIXME redirect to one host - (print its address if available)

            // options dialog
            it = new MenuItem("Tampering &options...");
            // FIXME it.Click += Options_Click;
            bishopMenu.MenuItems.Add(it);

            it = new MenuItem("-");
            bishopMenu.MenuItems.Add(it);

            // about dialog
            it = new MenuItem("About Bishop...");
            it.Click += (o, ev) => MessageBox.Show("Version: " + GetType().Assembly.GetName().Version, 
                "Bishop", MessageBoxButtons.OK, MessageBoxIcon.Information);
            bishopMenu.MenuItems.Add(it);

            // help
            it = new MenuItem("Help...");
            it.Click += (o, ev) => Process.Start(new ProcessStartInfo(
                "https://github.com/lowleveldesign/diagnostics-kit/wiki/5.1.bishop"));
            bishopMenu.MenuItems.Add(it);

            return bishopMenu;
        }

        public void AutoTamperRequestBefore(Session oSession)
        {
            if (!isLoaded) {
                return;
            }
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
                tamperingContext.ServerTcpAddressWithPort = settings.OneHostForRedirectionTcpAddressWithPort;
            } else if (IsApplicationServerSelected()) {
                serverRedirector.ApplyMatchingTamperingRules(request, tamperingContext, selectedServer);
            }

            if (tamperingContext.ShouldTamperRequest)
            {
                TamperRequest(oSession, tamperingContext);
            }
        }

        private void Log(string message)
        {
            FiddlerApplication.Log.LogFormat("[Bishop][Thread: {0}] {1}", Thread.CurrentThread.ManagedThreadId, message);
        }

        private void LogFormat(string format, params object[] args)
        {
            Log(string.Format(format, args));
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
            LogFormat("Tampering request: {1}", fiddlerSession.url);

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
            LogFormat("IP changed to {1}", tamperingContext.ServerTcpAddressWithPort);
            LogFormat("Url set to {1}", fullUrl);
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

    }
}
