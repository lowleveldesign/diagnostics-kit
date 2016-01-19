using LowLevelDesign.Diagnostics.Bishop.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace LowLevelDesign.Diagnostics.Bishop.UI
{
    public partial class TamperingOptionsForm : Form
    {
        private readonly string pluginSettingsFilePath;
        private readonly PluginSettings pluginSettings;
        private readonly Dictionary<string, RequestTransformation> transformations;

        public TamperingOptionsForm(string pluginSettingsFilePath)
        {
            InitializeComponent();

            this.pluginSettingsFilePath = pluginSettingsFilePath;
            pluginSettings = PluginSettings.Load(pluginSettingsFilePath);

            transformations = new Dictionary<string, RequestTransformation>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var rt in pluginSettings.UserDefinedTransformations) {
                lbTamperingRules.Items.Add(rt.Name);
                transformations.Add(rt.Name, rt);
            }

            txtLocalHttpsRedirects.Text = string.Join(", ", pluginSettings.HttpsRedirects.Select(
                r => string.Format("{0}:{1}", r.RemoteHttpsPort, r.LocalHttpPort)));
        }

        private bool isRuleNameUsed(string ruleName)
        {
            return transformations.ContainsKey(ruleName);
        }

        private IEnumerable<HttpsLocalRedirect> GetHttpsRedirects()
        {
            var txt = txtLocalHttpsRedirects.Text.Trim();
            var redirectsSerialized = txt.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            var redirects = new List<HttpsLocalRedirect>();
            foreach (var redirect in redirectsSerialized) {
                var ports = redirect.Split(':');
                if (ports.Length != 2) {
                    throw new ArgumentException();
                }
                redirects.Add(new HttpsLocalRedirect {
                    RemoteHttpsPort = ushort.Parse(ports[0]),
                    LocalHttpPort = ushort.Parse(ports[1])
                });
            }
            return redirects;
        }

        public PluginSettings GetNewPluginSettings()
        {
            var l = new RequestTransformation[lbTamperingRules.Items.Count];
            for (int i = 0; i < lbTamperingRules.Items.Count; i++) {
                var ruleName = (string)lbTamperingRules.Items[i];
                Debug.Assert(transformations.ContainsKey(ruleName));
                l[i] = transformations[ruleName];
            }
            pluginSettings.UserDefinedTransformations = l;

            pluginSettings.HttpsRedirects = GetHttpsRedirects();

            return pluginSettings;
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            var f = new TamperingRuleForm(isRuleNameUsed);
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                var requestTransformation = f.GetRequestTransformation();
                Debug.Assert(!transformations.ContainsKey(requestTransformation.Name));
                lbTamperingRules.Items.Add(requestTransformation.Name);
                transformations.Add(requestTransformation.Name, requestTransformation);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            var selectedIndex = lbTamperingRules.SelectedIndex;
            Debug.Assert(selectedIndex >= 0 && selectedIndex < lbTamperingRules.Items.Count);
            var f = new TamperingRuleForm(transformations[(string)lbTamperingRules.Items[selectedIndex]]);
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                var requestTransformation = f.GetRequestTransformation();
                Debug.Assert(transformations.ContainsKey(requestTransformation.Name));
                transformations[requestTransformation.Name] = requestTransformation;
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            var selectedIndex = lbTamperingRules.SelectedIndex;
            Debug.Assert(selectedIndex >= 0 && selectedIndex < lbTamperingRules.Items.Count);
            transformations.Remove((string)lbTamperingRules.Items[selectedIndex]);
            lbTamperingRules.Items.RemoveAt(selectedIndex);
            if (lbTamperingRules.Items.Count > 0) {
                lbTamperingRules.SelectedIndex = selectedIndex == lbTamperingRules.Items.Count ? 
                    selectedIndex - 1 : selectedIndex;
            }
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            var selectedIndex = lbTamperingRules.SelectedIndex;
            Debug.Assert(selectedIndex > 0);
            var item = lbTamperingRules.Items[selectedIndex];
            lbTamperingRules.Items.RemoveAt(selectedIndex);
            lbTamperingRules.Items.Insert(selectedIndex - 1, item);
            lbTamperingRules.SelectedIndex = selectedIndex - 1;
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            var selectedIndex = lbTamperingRules.SelectedIndex;
            Debug.Assert(selectedIndex < lbTamperingRules.Items.Count - 1);
            var item = lbTamperingRules.Items[selectedIndex];
            lbTamperingRules.Items.RemoveAt(selectedIndex);
            lbTamperingRules.Items.Insert(selectedIndex + 1, item);
            lbTamperingRules.SelectedIndex = selectedIndex + 1;
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

        private void lbTamperingRules_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedIndex = lbTamperingRules.SelectedIndex; 
            if (selectedIndex >= 0) {
                btnEdit.Enabled = true;
                btnRemove.Enabled = true;
                btnUp.Enabled = selectedIndex > 0;
                btnDown.Enabled = selectedIndex < lbTamperingRules.Items.Count - 1;
            } else {
                btnEdit.Enabled = btnRemove.Enabled = false;
                btnUp.Enabled = btnDown.Enabled = false;  
            }
        }

        private void txtLocalHttpsRedirects_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool isValid = true;

            var txt = txtLocalHttpsRedirects.Text.Trim();
            var redirectsSerialized = txt.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var redirect in redirectsSerialized) {
                if (!isValid) {
                    break;
                }
                var ports = redirect.Split(':');
                if (ports.Length != 2) {
                    isValid = false;
                } else {
                    ushort p;
                    isValid = ushort.TryParse(ports[0], out p) && ushort.TryParse(ports[1], out p);
                }
            }

            if (isValid) {
                httpsRedirectionErrorProvider.Clear();
            } else {
                e.Cancel = true;
                httpsRedirectionErrorProvider.SetError((Control)sender, "Invalid format of the https redirections.");
            }
        }
    }
}
