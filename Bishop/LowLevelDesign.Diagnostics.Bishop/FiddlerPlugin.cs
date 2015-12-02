using Fiddler;
using LowLevelDesign.Diagnostics.Bishop.Common;
using LowLevelDesign.Diagnostics.Bishop.Config;
using LowLevelDesign.Diagnostics.Bishop.Tampering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LowLevelDesign.Diagnostics.Bishop
{
    public class FiddlerPlugin : IAutoTamper
    {
        private readonly string configurationFilePath = Path.Combine(Path.GetDirectoryName(
            Assembly.GetExecutingAssembly().Location), "bishop.conf");

        private PluginSettings settings;
        private CustomTamperingRulesContainer tamperer;
        private bool isLoaded;
        private bool shouldInterceptHttps;

        public void OnLoad()
        {
            try {
                settings = PluginSettings.Load(configurationFilePath);
                tamperer = new CustomTamperingRulesContainer(settings);

                // FIXME ask for Diagnostics Kit url
                // FIXME connect with the Diagnostics Castle

                // load menu items for Bishop
                FiddlerApplication.UI.Menu.MenuItems.Add(PrepareMenu());

                isLoaded = true;
            } catch (Exception ex) {
                MessageBox.Show("There was a problem while loading the Bishop plugin. Please check the Fiddler log for details", 
                    "Bishop is dead.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                FiddlerApplication.Log.LogFormat("Bishop error: {0}", ex);
            }
        }

        private MenuItem PrepareMenu()
        {
            var bishopMenu = new MenuItem("&Bishop");

            // HTTPS -> HTTP emulator
            var it = new MenuItem("&Emulate HTTPS (localhost)") {
                Name = "miEnableTlsEdgeRouter",
                RadioCheck = false,
                Checked = false
            };
            it.Click += (o, ev) => {
                var mi = (MenuItem)o;
                shouldInterceptHttps = mi.Checked = !mi.Checked;
            };
            bishopMenu.MenuItems.Add(it);

            bishopMenu.MenuItems.Add(it);
            it = new MenuItem("-");
            bishopMenu.MenuItems.Add(it);

            // options dialog
            //it = new MenuItem("&Options...");
            //it.Click += Options_Click;
            //bishopMenu.MenuItems.Add(it);

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

            if (request.IsHttpsConnect)
            {
                HandleHttpsConnect(request);
                return;
            }

            var tamperParams = new TamperParameters();
            ApplyHttpsRedirectionIfEnabled(request, tamperParams);
            ApplyTamperingRules(request, tamperParams);
            RedirectToASpecificServerIfSelected(request, tamperParams);

            if (tamperParams.ShouldTamperRequest) {
                TamperRequest(request, tamperParams);
            }
        }

        private void HandleHttpsConnect(IRequest request)
        {
            if (request.IsLocal && shouldInterceptHttps) {
                FiddlerApplication.Log.LogFormat("[Bishop][Thread: {0}] Performing handshake on behalf of the application.", 
                    Thread.CurrentThread.ManagedThreadId);
                //FIXME: request.FiddlerSession.oFlags["x-replywithtunnel"] = "true";
            } else {
                FiddlerApplication.Log.LogFormat("[Bishop][Thread: {0}] Forwarding handshake.", 
                    Thread.CurrentThread.ManagedThreadId);
            }
        }

        private void ApplyHttpsRedirectionIfEnabled(IRequest request, TamperParameters tamperParams)
        {
            if (request.IsLocal && request.IsHttps && shouldInterceptHttps)
            {
                var localPort = settings.FindLocalPortForHttpsRedirection(request.Port);
                if (localPort > 0) {
                    // FIXME: request.FiddlerSession.oRequest.headers.Add("X-OriginalBaseUri", string.Format("https://{0}", request.FiddlerSession.host));
                    tamperParams.ServerTcpAddressWithPort = string.Format("localhost:{0}", localPort);
                }
            }
        }

        private void ApplyTamperingRules(IRequest request, TamperParameters tamperParams)
        {
            // FIXME loop through the rules and find the matching one
        }

        private void RedirectToASpecificServerIfSelected(Request request, TamperParameters tamperParams)
        {
            // FIXME redirect to a specific server
        }

        private void TamperRequest(IRequest request, TamperParameters tamperParams)
        {
            // FIXME
            /*
            var fiddlerSession = request.FiddlerSession;
            FiddlerApplication.Log.LogFormat("[Bishop][Thread: {0}] Tampering request: {1}", Thread.CurrentThread.ManagedThreadId, fiddlerSession.url);

            var fullUrl = fiddlerSession.fullUrl.Replace(fiddlerSession.host, tamperParams.ServerTcpAddressWithPort);
            if (!string.IsNullOrEmpty(tamperParams.HostHeader))
            {
                fiddlerSession.fullUrl = "http://" + tamperParams.HostHeader + fiddlerSession.PathAndQuery;
                // fullUrl won't be a host but rather host header with different IP
                fullUrl = fiddlerSession.fullUrl;
            }
            else if (fiddlerSession.isHTTPS)
            {
                fiddlerSession.fullUrl = fiddlerSession.fullUrl.Replace("https://", "http://");
            }
            fiddlerSession["X-OverrideHost"] = tamperParams.ServerTcpAddressWithPort;
            fiddlerSession.bypassGateway = true;
            FiddlerApplication.Log.LogFormat("[Bishop][Thread: {0}] IP changed to {1}", Thread.CurrentThread.ManagedThreadId, 
                tamperParams.ServerTcpAddressWithPort);
            FiddlerApplication.Log.LogFormat("[Bishop][Thread: {0}] Url set to {1}", Thread.CurrentThread.ManagedThreadId, fullUrl);*/
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
