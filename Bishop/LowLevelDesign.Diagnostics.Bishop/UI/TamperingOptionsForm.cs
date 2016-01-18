using LowLevelDesign.Diagnostics.Bishop.Config;
using System;
using System.Windows.Forms;

namespace LowLevelDesign.Diagnostics.Bishop.UI
{
    public partial class TamperingOptionsForm : Form
    {
        private readonly string pluginSettingsFilePath;
        private readonly PluginSettings pluginSettings;

        public TamperingOptionsForm(string pluginSettingsFilePath)
        {
            InitializeComponent();

            this.pluginSettingsFilePath = pluginSettingsFilePath;
            pluginSettings = PluginSettings.Load(pluginSettingsFilePath);
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            var f = new TamperingRuleForm();
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                cblTamperingRules.
                // FIXME add f.GetRequestTransformation();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {

        }

        private void btnRemove_Click(object sender, EventArgs e)
        {

        }

        private void btnUp_Click(object sender, EventArgs e)
        {

        }

        private void btnDown_Click(object sender, EventArgs e)
        {

        }

        private void btnCastleConnection_Click(object sender, EventArgs e)
        {
            using (var dlg = new DiagnosticsCastleForm())
            {
                dlg.TxtCastleUrl.Text = pluginSettings.DiagnosticsUrl != null ? pluginSettings.DiagnosticsUrl.AbsoluteUri : string.Empty;
                dlg.TxtCastleUsername.Text = pluginSettings.UserName;
                dlg.TxtCastlePassword.Text = pluginSettings.GetPassword();
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    pluginSettings.DiagnosticsUrl = new Uri(dlg.TxtCastleUrl.Text);
                    pluginSettings.UserName = dlg.TxtCastleUsername.Text;
                    pluginSettings.SetPassword(dlg.TxtCastlePassword.Text);
                }
            }
        }
    }
}
