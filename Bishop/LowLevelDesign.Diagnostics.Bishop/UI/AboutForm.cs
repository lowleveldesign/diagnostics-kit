using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace LowLevelDesign.Diagnostics.Bishop.UI
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            lblVersion.Text = "v" + GetType().Assembly.GetName().Version;
        }

        private void lnkDesc_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.lowleveldesign.org/diagnosticskit");
        }

        private void lnkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/lowleveldesign/diagnostics-kit/wiki/5.1.bishop");
        }
    }
}
