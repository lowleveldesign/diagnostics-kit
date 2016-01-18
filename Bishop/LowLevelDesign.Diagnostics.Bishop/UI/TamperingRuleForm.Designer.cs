namespace LowLevelDesign.Diagnostics.Bishop.UI
{
    partial class TamperingRuleForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txtRuleName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtHostRegex = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtPathAndQueryRegex = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtDestinationHost = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtDestinationPathAndQuery = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.txtDestinationPorts = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtDestinationIPs = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.lnkHelp = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Rule name:";
            // 
            // txtRuleName
            // 
            this.txtRuleName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRuleName.Location = new System.Drawing.Point(160, 10);
            this.txtRuleName.Name = "txtRuleName";
            this.txtRuleName.Size = new System.Drawing.Size(278, 20);
            this.txtRuleName.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Host regex:";
            // 
            // txtHostRegex
            // 
            this.txtHostRegex.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHostRegex.Location = new System.Drawing.Point(160, 33);
            this.txtHostRegex.Name = "txtHostRegex";
            this.txtHostRegex.Size = new System.Drawing.Size(278, 20);
            this.txtHostRegex.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(111, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Path and query regex:";
            // 
            // txtPathAndQueryRegex
            // 
            this.txtPathAndQueryRegex.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPathAndQueryRegex.Location = new System.Drawing.Point(160, 55);
            this.txtPathAndQueryRegex.Name = "txtPathAndQueryRegex";
            this.txtPathAndQueryRegex.Size = new System.Drawing.Size(278, 20);
            this.txtPathAndQueryRegex.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 80);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(86, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Destination host:";
            // 
            // txtDestinationHost
            // 
            this.txtDestinationHost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDestinationHost.Location = new System.Drawing.Point(160, 77);
            this.txtDestinationHost.Name = "txtDestinationHost";
            this.txtDestinationHost.Size = new System.Drawing.Size(278, 20);
            this.txtDestinationHost.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 103);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(137, 13);
            this.label5.TabIndex = 8;
            this.label5.Text = "Destination path and query:";
            // 
            // txtDestinationPathAndQuery
            // 
            this.txtDestinationPathAndQuery.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDestinationPathAndQuery.Location = new System.Drawing.Point(160, 100);
            this.txtDestinationPathAndQuery.Name = "txtDestinationPathAndQuery";
            this.txtDestinationPathAndQuery.Size = new System.Drawing.Size(278, 20);
            this.txtDestinationPathAndQuery.TabIndex = 9;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.CausesValidation = false;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(363, 251);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(282, 251);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 16;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // txtDestinationPorts
            // 
            this.txtDestinationPorts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDestinationPorts.Location = new System.Drawing.Point(160, 182);
            this.txtDestinationPorts.Multiline = true;
            this.txtDestinationPorts.Name = "txtDestinationPorts";
            this.txtDestinationPorts.Size = new System.Drawing.Size(278, 63);
            this.txtDestinationPorts.TabIndex = 15;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(13, 182);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(116, 30);
            this.label6.TabIndex = 14;
            this.label6.Text = "Destination ports (comma seperated):";
            // 
            // txtDestinationIPs
            // 
            this.txtDestinationIPs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDestinationIPs.Location = new System.Drawing.Point(160, 123);
            this.txtDestinationIPs.Multiline = true;
            this.txtDestinationIPs.Name = "txtDestinationIPs";
            this.txtDestinationIPs.Size = new System.Drawing.Size(278, 54);
            this.txtDestinationIPs.TabIndex = 13;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(13, 123);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(153, 35);
            this.label7.TabIndex = 12;
            this.label7.Text = "Destination IP addresses (comma separated):";
            // 
            // lnkHelp
            // 
            this.lnkHelp.AutoSize = true;
            this.lnkHelp.Location = new System.Drawing.Point(16, 260);
            this.lnkHelp.Name = "lnkHelp";
            this.lnkHelp.Size = new System.Drawing.Size(62, 13);
            this.lnkHelp.TabIndex = 18;
            this.lnkHelp.TabStop = true;
            this.lnkHelp.Text = "Need help?";
            this.lnkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkHelp_LinkClicked);
            // 
            // TamperingRuleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(443, 282);
            this.Controls.Add(this.lnkHelp);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtDestinationPorts);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtDestinationIPs);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtDestinationPathAndQuery);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtDestinationHost);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtPathAndQueryRegex);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtHostRegex);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtRuleName);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TamperingRuleForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tempering Rule Editor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtRuleName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtHostRegex;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPathAndQueryRegex;
        private System.Windows.Forms.TextBox txtDestinationHost;
        private System.Windows.Forms.TextBox txtDestinationPathAndQuery;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtDestinationIPs;
        private System.Windows.Forms.TextBox txtDestinationPorts;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.LinkLabel lnkHelp;
    }
}