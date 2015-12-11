namespace LowLevelDesign.Diagnostics.Bishop.UI
{
    partial class CustomServerForm
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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.TxtServerAddress = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.addressErrorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.NumericServerPort = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.addressErrorProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericServerPort)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(299, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "All request intercepted by Fiddler will be redirected to this host.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Server address:";
            // 
            // TxtServerAddress
            // 
            this.TxtServerAddress.Location = new System.Drawing.Point(100, 37);
            this.TxtServerAddress.Name = "TxtServerAddress";
            this.TxtServerAddress.Size = new System.Drawing.Size(212, 20);
            this.TxtServerAddress.TabIndex = 2;
            this.TxtServerAddress.Validating += new System.ComponentModel.CancelEventHandler(this.TxtServerAddress_Validating);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Port:";
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(175, 87);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.CausesValidation = false;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(256, 87);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // addressErrorProvider
            // 
            this.addressErrorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            this.addressErrorProvider.ContainerControl = this;
            // 
            // NumericServerPort
            // 
            this.NumericServerPort.Location = new System.Drawing.Point(100, 62);
            this.NumericServerPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.NumericServerPort.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.NumericServerPort.Name = "NumericServerPort";
            this.NumericServerPort.Size = new System.Drawing.Size(87, 20);
            this.NumericServerPort.TabIndex = 4;
            this.NumericServerPort.Value = new decimal(new int[] {
            80,
            0,
            0,
            0});
            // 
            // CustomServerForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(343, 118);
            this.ControlBox = false;
            this.Controls.Add(this.NumericServerPort);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.TxtServerAddress);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "CustomServerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Set Custom Server for redirection";
            ((System.ComponentModel.ISupportInitialize)(this.addressErrorProvider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericServerPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.TextBox TxtServerAddress;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ErrorProvider addressErrorProvider;
        public System.Windows.Forms.NumericUpDown NumericServerPort;
    }
}