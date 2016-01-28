using Fiddler;
using LowLevelDesign.Diagnostics.Bishop.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace LowLevelDesign.Diagnostics.Bishop.UI
{
    class PluginMenu
    {
        private readonly IBishop plugin;
        private readonly MenuItem bishopMenu;
        private MenuItem selectedServerMenuItem;
        private string lastSelectedCustomServerAddress = string.Empty;
        private ushort lastSelectedCustomServerPort = 80;

        public PluginMenu(IBishop plugin)
        {
            this.plugin = plugin;
            bishopMenu = new MenuItem("&Bishop");

            MenuItem it;
            // Castle
            PrepareServerMenu();
            it = new MenuItem("-");
            bishopMenu.MenuItems.Add(it);

            // HTTPS -> HTTP emulator
            it = new MenuItem("&Emulate HTTPS (localhost)") {
                Name = "miEnableTlsEdgeRouter",
                RadioCheck = false,
                Checked = false
            };
            it.Click += (o, ev) => {
                var mi = (MenuItem)o;
                mi.Checked = !mi.Checked;
                plugin.SetHttpsLocalInterception(mi.Checked);
            };
            bishopMenu.MenuItems.Add(it);

            it = new MenuItem("-");
            bishopMenu.MenuItems.Add(it);

            // options dialog
            it = new MenuItem("Tampering &options...");
            it.Click += TamperingOptions_Click;
            bishopMenu.MenuItems.Add(it);

            it = new MenuItem("-");
            bishopMenu.MenuItems.Add(it);

            // about dialog
            it = new MenuItem("About Bishop...");
            it.Click += (o, ev) => {
                new AboutForm().ShowDialog(FiddlerApplication.UI);
            };
            bishopMenu.MenuItems.Add(it);
        }

        public void PrepareServerMenu()
        {
            const string ConfigureCastleMenuItemKey = "miConfigureCastle";

            if (!plugin.IsDiagnosticsCastleConfigured() &&
                !bishopMenu.MenuItems.ContainsKey(ConfigureCastleMenuItemKey)) {
                var it = new MenuItem("Configure &Castle connection...") {
                    Name = ConfigureCastleMenuItemKey
                };
                it.Click += CastleConnection_Click;
                bishopMenu.MenuItems.Add(0, it);
            }

            if (!bishopMenu.MenuItems.ContainsKey("miNoServer")) {
                var it = new MenuItem("Off") {
                    Name = "miNoServer",
                    Checked = true
                };
                selectedServerMenuItem = it;
                it.Click += NoServer_Click;
                bishopMenu.MenuItems.Add(it);
            }

            var keysToRemove = new HashSet<string>(StringComparer.Ordinal);
            foreach (MenuItem it in bishopMenu.MenuItems) {
                if (it.Name.StartsWith("miServer", StringComparison.Ordinal)) {
                    keysToRemove.Add(it.Name);
                }
            }
            if (plugin.IsDiagnosticsCastleConfigured()) {
                if (bishopMenu.MenuItems.ContainsKey(ConfigureCastleMenuItemKey)) {
                    bishopMenu.MenuItems.RemoveByKey(ConfigureCastleMenuItemKey);
                }
                int offset = 1;
                foreach (var srv in plugin.AvailableServers) {
                    var itemName = "miServer" + srv;
                    if (!keysToRemove.Contains(itemName)) {
                        var it = new MenuItem(srv) { Name = itemName };
                        it.Click += Server_Click;
                        bishopMenu.MenuItems.Add(offset, it);
                        offset += 1;
                    } else {
                        keysToRemove.Remove(itemName);
                    }
                }
            }
            foreach (var key in keysToRemove) {
                var it = bishopMenu.MenuItems[key];
                if (it.Checked) {
                    plugin.TurnOffServerRedirection();
                    it.Checked = false;
                    bishopMenu.MenuItems["miNoServer"].Checked = true;
                }
                bishopMenu.MenuItems.Remove(it);
            }

            if (!bishopMenu.MenuItems.ContainsKey("miCustomServer")) {
                var it = new MenuItem("Custom...") {
                    Name = "miCustomServer"
                };
                it.Click += CustomServer_Click;
                bishopMenu.MenuItems.Add(it);
            }
        }

        public MenuItem BishopMenu { get { return bishopMenu; } }

        private void TamperingOptions_Click(object sender, EventArgs e)
        {
            using (var dlg = new TamperingOptionsForm(plugin.PluginConfigurationFilePath))
            {
                if (dlg.ShowDialog(FiddlerApplication.UI) == DialogResult.OK)
                {
                    var newSettings = dlg.GetNewPluginSettings();
                    newSettings.Save(plugin.PluginConfigurationFilePath);
                    plugin.ReloadSettings(newSettings);

                    PrepareServerMenu();
                }
            }
        }

        private void CastleConnection_Click(object sender, EventArgs e)
        {
            using (var dlg = new DiagnosticsCastleForm())
            {
                var settings = PluginSettings.Load(plugin.PluginConfigurationFilePath);
                dlg.TxtCastleUrl.Text = settings.DiagnosticsUrl != null ? settings.DiagnosticsUrl.AbsoluteUri : string.Empty;
                dlg.TxtCastleUsername.Text = settings.UserName;
                dlg.TxtCastlePassword.Text = settings.GetPassword();
                if (dlg.ShowDialog(FiddlerApplication.UI) == DialogResult.OK)
                {
                    settings.DiagnosticsUrl = new Uri(dlg.TxtCastleUrl.Text);
                    settings.UserName = dlg.TxtCastleUsername.Text;
                    settings.SetPassword(dlg.TxtCastlePassword.Text);
                    settings.Save(plugin.PluginConfigurationFilePath);

                    plugin.ReloadSettings(settings);

                    PrepareServerMenu();
                }
            }
        }

        private void Server_Click(object sender, EventArgs e)
        {
            Debug.Assert(selectedServerMenuItem != null);
            ((MenuItem)sender).Checked = true;
            selectedServerMenuItem.Checked = false;
            selectedServerMenuItem = (MenuItem)sender;
            plugin.SelectApplicationServer(selectedServerMenuItem.Text);
        }

        private void CustomServer_Click(object sender, EventArgs e)
        {
            Debug.Assert(selectedServerMenuItem != null);
            var dlg = new CustomServerForm();
            dlg.TxtServerAddress.Text = lastSelectedCustomServerAddress;
            dlg.NumericServerPort.Value = lastSelectedCustomServerPort;
            if (dlg.ShowDialog(FiddlerApplication.UI) == DialogResult.OK) {
                ((MenuItem)sender).Checked = true;
                selectedServerMenuItem.Checked = false;
                selectedServerMenuItem = (MenuItem)sender;
                lastSelectedCustomServerPort = (ushort)dlg.NumericServerPort.Value;
                lastSelectedCustomServerAddress = dlg.TxtServerAddress.Text;

                var fullAddress = string.Format("{0}:{1}", lastSelectedCustomServerAddress,
                    lastSelectedCustomServerPort);
                ((MenuItem)sender).Text = fullAddress;
                plugin.SelectCustomServer(fullAddress);
            }
            dlg.Dispose();
        }

        private void NoServer_Click(object sender, EventArgs e)
        {
            plugin.TurnOffServerRedirection();
            selectedServerMenuItem.Checked = false;
            ((MenuItem)sender).Checked = true;
            selectedServerMenuItem = (MenuItem)sender;
        }

    }
}
