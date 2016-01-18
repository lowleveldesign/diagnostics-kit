namespace LowLevelDesign.Diagnostics.Bishop.UI
{
    partial class AboutForm
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
            this.lblVersion = new System.Windows.Forms.Label();
            this.lnkDesc = new System.Windows.Forms.LinkLabel();
            this.lnkHelp = new System.Windows.Forms.LinkLabel();
            this.btnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "Bishop";
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblVersion.Location = new System.Drawing.Point(86, 9);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(79, 24);
            this.lblVersion.TabIndex = 1;
            this.lblVersion.Text = "v.1.0.0.0";
            // 
            // lnkDesc
            // 
            this.lnkDesc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkDesc.LinkArea = new System.Windows.Forms.LinkArea(24, 14);
            this.lnkDesc.Location = new System.Drawing.Point(16, 44);
            this.lnkDesc.Name = "lnkDesc";
            this.lnkDesc.Size = new System.Drawing.Size(367, 30);
            this.lnkDesc.TabIndex = 2;
            this.lnkDesc.TabStop = true;
            this.lnkDesc.Text = "Bishop is a part of the DiagnosticsKit toolkit and provides functionalities for t" +
    "esting various network scenarios.";
            this.lnkDesc.UseCompatibleTextRendering = true;
            this.lnkDesc.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkDesc_LinkClicked);
            // 
            // lnkHelp
            // 
            this.lnkHelp.AutoSize = true;
            this.lnkHelp.LinkArea = new System.Windows.Forms.LinkArea(30, 23);
            this.lnkHelp.Location = new System.Drawing.Point(16, 84);
            this.lnkHelp.Name = "lnkHelp";
            this.lnkHelp.Size = new System.Drawing.Size(272, 17);
            this.lnkHelp.TabIndex = 3;
            this.lnkHelp.TabStop = true;
            this.lnkHelp.Text = "Detailed help can be found on the project Github wiki.";
            this.lnkHelp.UseCompatibleTextRendering = true;
            this.lnkHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkHelp_LinkClicked);
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(308, 112);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // AboutForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(395, 147);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lnkHelp);
            this.Controls.Add(this.lnkDesc);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About Bishop";
            this.Load += new System.EventHandler(this.AboutForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.LinkLabel lnkDesc;
        private System.Windows.Forms.LinkLabel lnkHelp;
        private System.Windows.Forms.Button btnOK;
    }
}