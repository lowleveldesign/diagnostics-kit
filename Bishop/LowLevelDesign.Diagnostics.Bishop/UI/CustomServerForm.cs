using System.ComponentModel;
using System.Windows.Forms;

namespace LowLevelDesign.Diagnostics.Bishop.UI
{
    public partial class CustomServerForm : Form
    {
        public CustomServerForm()
        {
            InitializeComponent();
        }

        private void TxtServerAddress_Validating(object sender, CancelEventArgs e)
        {
            TxtServerAddress.Text = TxtServerAddress.Text.Trim();
            if (string.IsNullOrEmpty(TxtServerAddress.Text)) {
                e.Cancel = true;
                addressErrorProvider.SetError((Control)sender, "You must provide a valid hostname or IP address.");
            } else {
                addressErrorProvider.Clear();
            }
        }
    }
}
