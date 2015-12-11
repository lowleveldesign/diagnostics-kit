using LowLevelDesign.Diagnostics.Bishop.Config;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace LowLevelDesign.Diagnostics.Bishop.UI
{
    public partial class DiagnosticsCastleForm : Form
    {
        public DiagnosticsCastleForm()
        {
            InitializeComponent();
        }

        private void TxtCastleUrl_Validating(object sender, CancelEventArgs e)
        {
            Uri url;
            if (!Uri.TryCreate(TxtCastleUrl.Text, UriKind.Absolute, out url)) {
                errorProvider.SetError((Control)sender, "Invalid url");
                e.Cancel = true;
            } else {
                errorProvider.Clear();
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            var testSettings = new PluginSettings {
                DiagnosticsUrl = new Uri(TxtCastleUrl.Text),
                UserName = TxtCastleUsername.Text
            };
            testSettings.SetPassword(TxtCastlePassword.Text);
            var connector = new BishopHttpCastleConnector(testSettings);
            if (connector.AreSettingsValid()) {
                MessageBox.Show(this, "Connection successful", "Test connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } else {
                MessageBox.Show(this, "Connection failed", "Test connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
