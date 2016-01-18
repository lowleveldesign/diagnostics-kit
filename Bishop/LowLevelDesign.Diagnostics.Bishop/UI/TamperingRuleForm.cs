using LowLevelDesign.Diagnostics.Bishop.Config;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace LowLevelDesign.Diagnostics.Bishop.UI
{
    public partial class TamperingRuleForm : Form
    {
        public TamperingRuleForm()
        {
            InitializeComponent();
        }

        public TamperingRuleForm(RequestTransformation transformation) : base()
        {
            txtRuleName.Text = transformation.Name;
            txtHostRegex.Text = transformation.RegexToMatchAgainstHost;
            txtPathAndQueryRegex.Text = transformation.RegexToMatchAgainstPathAndQuery;
            txtDestinationHost.Text = transformation.DestinationHostHeader;
            txtDestinationPathAndQuery.Text = transformation.DestinationPathAndQuery;
            txtDestinationIPs.Text = string.Join(", ", transformation.DestinationIpAddresses);
            txtDestinationPorts.Text = string.Join(", ", transformation.DestinationPorts.Select(p => p.ToString()));
        }

        private void lnkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/lowleveldesign/diagnostics-kit/wiki/5.1.bishop");
        }

        public RequestTransformation GetRequestTransformation()
        {
            return new RequestTransformation {
                Name = RuleName,
                RegexToMatchAgainstHost = RegexToMatchAgainstHost,
                RegexToMatchAgainstPathAndQuery = RegexToMatchAgainstPathAndQuery,
                DestinationHostHeader = DestinationHost,
                DestinationPathAndQuery = DestinationPathAndQuery,
                DestinationIpAddresses = DestinationIPs,
                DestinationPorts = DestinationPorts
            };
        }

        public string RuleName { get { return txtRuleName.Text.Trim(); } }

        public string RegexToMatchAgainstHost { get { return txtHostRegex.Text.Trim(); } }

        public string RegexToMatchAgainstPathAndQuery { get { return txtPathAndQueryRegex.Text.Trim(); } }

        public string DestinationHost { get { return txtDestinationHost.Text.Trim(); } }

        public string DestinationPathAndQuery { get { return txtDestinationPathAndQuery.Text.Trim(); } }

        public string[] DestinationIPs {
            get
            {
                return txtDestinationIPs.Text.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public ushort[] DestinationPorts
        {
            get
            {
                var v = txtDestinationPorts.Text.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var ports = new List<ushort>(v.Length);
                for (int i = 0; i < v.Length; i++)
                {
                    ushort p;
                    if (ushort.TryParse(v[i], out p))
                    {
                        ports.Add(p);
                    }
                }
                return ports.ToArray();
            }
        }

        private bool IsValid()
        {
            return !(string.IsNullOrEmpty(RuleName) ||
                (string.IsNullOrEmpty(RegexToMatchAgainstHost) && string.IsNullOrEmpty(RegexToMatchAgainstPathAndQuery)) ||
                (string.IsNullOrEmpty(DestinationHost) && string.IsNullOrEmpty(DestinationPathAndQuery) &&
                    DestinationIPs.Length == 0 && DestinationPorts.Length == 0));
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (IsValid()) {
                DialogResult = DialogResult.OK;
            } else {
                MessageBox.Show(this, "You must provide the rule name, either host regex or path query regex and any of the destinations.",
                    "Invalid data", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
