using Fiddler;
using LowLevelDesign.Diagnostics.Bishop.Config;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace LowLevelDesign.Diagnostics.Bishop.UI
{
    class PluginMenu
    {
        private readonly FiddlerPlugin plugin;
        private MenuItem selectedServerMenuItem;
        private string lastSelectedCustomServerAddress = string.Empty;
        private ushort lastSelectedCustomServerPort = 80;

        public PluginMenu(FiddlerPlugin plugin)
        {
            this.plugin = plugin;
        }

        public MenuItem PrepareMenu()
        {
            var bishopMenu = new MenuItem("&Bishop");
            MenuItem it;

            // Castle
            if (!plugin.IsDiagnosticsCastleConfigured()) {
                it = new MenuItem("Configure &Castle connection...") {
                    Name = "miConfigureCastle"
                };
                it.Click += CastleConnection_Click;
                bishopMenu.MenuItems.Add(it);
            }
            it = new MenuItem("Off") {
                Name = "miNoServer",
                Checked = true
            };
            selectedServerMenuItem = it;
            it.Click += NoServer_Click;
            bishopMenu.MenuItems.Add(it);
            if (plugin.IsDiagnosticsCastleConfigured()) {
                foreach (var srv in plugin.AvailableServers) {
                    it = new MenuItem(srv) {
                        Name = "miServer" + srv
                    };
                    it.Click += Server_Click;
                    bishopMenu.MenuItems.Add(it);
                }
            }

            it = new MenuItem("Custom...") {
                Name = "miCustomServer"
            };
            it.Click += CustomServer_Click;
            bishopMenu.MenuItems.Add(it);

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

            return bishopMenu;
        }

        private void TamperingOptions_Click(object sender, EventArgs e)
        {
            using (var dlg = new TamperingOptionsForm(plugin.PluginConfigurationFilePath))
            {
                if (dlg.ShowDialog(FiddlerApplication.UI) == DialogResult.OK)
                {
                    plugin.ReloadSettings();
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

                    plugin.ReloadSettings();
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
