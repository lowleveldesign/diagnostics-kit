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

using Fiddler;
using LowLevelDesign.Diagnostics.Bishop.Common;
using LowLevelDesign.Diagnostics.Bishop.Config;
using LowLevelDesign.Diagnostics.Bishop.Tampering;
using LowLevelDesign.Diagnostics.Bishop.UI;
using LowLevelDesign.Diagnostics.Commons.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace LowLevelDesign.Diagnostics.Bishop
{
    public sealed class FiddlerPlugin : IAutoTamper, IBishop
    {
        private readonly string configurationFilePath = Path.Combine(Path.GetDirectoryName(
            Assembly.GetExecutingAssembly().Location), "bishop.conf");
        private readonly ReaderWriterLockSlim lck = new ReaderWriterLockSlim();

        private PluginSettings settings;
        private CustomTamperingRulesContainer tamperer;
        private ServerRedirectionRulesContainer serverRedirector;
        private PluginMenu pluginMenu;
        private bool isLoaded;
        private bool shouldInterceptHttps;
        private string selectedServer;
        private string customServerAddressWithPort;
        private bool isRedirectionToOneHostEnabled;

        public void OnLoad()
        {
            try {
                ReloadSettings(PluginSettings.Load(configurationFilePath));

                pluginMenu = new PluginMenu(this);
                FiddlerApplication.UI.Menu.MenuItems.Add(pluginMenu.BishopMenu);
                isLoaded = true;
            } catch (Exception ex) {
                MessageBox.Show("There was a problem while loading the Bishop plugin. Please check the Fiddler log for details", 
                    "Bishop is dead.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogFormat("Bishop error: {0}", ex);
            }
        }

        public void ReloadSettings(PluginSettings newSettings)
        {
            var needToReloadApplicationServerConfigs = settings == null
                || newSettings.DiagnosticsUrl != null &&
                    !newSettings.DiagnosticsUrl.Equals(settings.DiagnosticsUrl)
                || !string.Equals(settings.UserName, newSettings.UserName, StringComparison.Ordinal)
                || newSettings.EncryptedPassword != null && 
                    !newSettings.EncryptedPassword.SequenceEqual(settings.EncryptedPassword);

            lck.EnterWriteLock();
            try {
                settings = newSettings;
                tamperer = new CustomTamperingRulesContainer(newSettings);

                if (needToReloadApplicationServerConfigs) {
                    serverRedirector = new ServerRedirectionRulesContainer(RetrieveApplicationServerConfigs());
                }
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
                LogFormat("ERROR: Timeout when acquiring lock.");
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
                } else {
                    tamperer.ApplyMatchingTamperingRules(request, tamperingContext);
                    if (isRedirectionToOneHostEnabled) {
                        tamperingContext.ServerTcpAddressWithPort = customServerAddressWithPort;
                    }
                    else if (IsApplicationServerSelected()) {
                        serverRedirector.ApplyMatchingTamperingRules(request, tamperingContext, selectedServer);
                    }
                }

                if (tamperingContext.ShouldTamperRequest) {
                    TamperRequest(request, oSession, tamperingContext);
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
                tamperParams.Protocol = "http";
            }
        }

        private bool IsApplicationServerSelected()
        {
            return selectedServer != null;
        }

        private void TamperRequest(IRequest request, Session fiddlerSession, TamperingContext tamperingContext)
        {
            LogFormat("Tampering request: {0}", fiddlerSession.url);

            var fullUrl = fiddlerSession.fullUrl;
            var host = fiddlerSession.host;

            if (!string.IsNullOrEmpty(tamperingContext.ServerTcpAddressWithPort)) {
                fiddlerSession["X-OverrideHost"] = tamperingContext.ServerTcpAddressWithPort;
                LogFormat("IP changed to {0}", tamperingContext.ServerTcpAddressWithPort);
            }
            if (!string.IsNullOrEmpty(tamperingContext.HostHeader)) {
                fullUrl = request.Protocol + "://" + tamperingContext.HostHeader + fiddlerSession.PathAndQuery;
                host = tamperingContext.HostHeader;
                LogFormat("HostName changed to {0}", host);
            }
            if (!string.IsNullOrEmpty(tamperingContext.PathAndQuery)) {
                fullUrl = request.Protocol + "://" + host + tamperingContext.PathAndQuery;
            }
            if (!string.IsNullOrEmpty(tamperingContext.Protocol) && !string.Equals(
                    tamperingContext.Protocol, request.Protocol, StringComparison.OrdinalIgnoreCase)) {
                fullUrl = fullUrl.Replace(request.Protocol + "://", tamperingContext.Protocol + "://");
            }
            fiddlerSession.fullUrl = fullUrl;

            fiddlerSession.bypassGateway = true;
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
