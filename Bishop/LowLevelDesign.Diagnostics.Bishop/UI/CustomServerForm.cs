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
