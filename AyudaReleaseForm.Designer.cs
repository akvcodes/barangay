namespace BarangayManagementSystem
{
    partial class QuickAyudaReleaseForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblInstructions = new System.Windows.Forms.Label();
            this.txtBarcodeScanner = new System.Windows.Forms.TextBox();
            this.lblScannerLabel = new System.Windows.Forms.Label();
            this.groupBoxResident = new System.Windows.Forms.GroupBox();
            this.lblResidentContact = new System.Windows.Forms.Label();
            this.lblResidentAddress = new System.Windows.Forms.Label();
            this.lblResidentName = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.numAmount = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.cboPurpose = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtReleasedBy = new System.Windows.Forms.TextBox();
            this.btnRelease = new System.Windows.Forms.Button();
            this.panelTop.SuspendLayout();
            this.groupBoxResident.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAmount)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.panelTop.Controls.Add(this.btnClose);
            this.panelTop.Controls.Add(this.lblTitle);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Margin = new System.Windows.Forms.Padding(4);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1067, 74);
            this.panelTop.TabIndex = 0;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(1000, 12);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(53, 49);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "✕";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(27, 18);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(376, 37);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "AYUDA RELEASE (SCANNER)";
            // 
            // lblInstructions
            // 
            this.lblInstructions.AutoSize = true;
            this.lblInstructions.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblInstructions.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.lblInstructions.Location = new System.Drawing.Point(267, 111);
            this.lblInstructions.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblInstructions.Name = "lblInstructions";
            this.lblInstructions.Size = new System.Drawing.Size(465, 32);
            this.lblInstructions.TabIndex = 1;
            this.lblInstructions.Text = "SCAN RESIDENT BARCODE OR ID CARD";
            this.lblInstructions.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtBarcodeScanner
            // 
            this.txtBarcodeScanner.Font = new System.Drawing.Font("Segoe UI", 18F);
            this.txtBarcodeScanner.Location = new System.Drawing.Point(293, 185);
            this.txtBarcodeScanner.Margin = new System.Windows.Forms.Padding(4);
            this.txtBarcodeScanner.Name = "txtBarcodeScanner";
            this.txtBarcodeScanner.Size = new System.Drawing.Size(532, 47);
            this.txtBarcodeScanner.TabIndex = 0;
            this.txtBarcodeScanner.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtBarcodeScanner.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtBarcodeScanner_KeyPress);
            // 
            // lblScannerLabel
            // 
            this.lblScannerLabel.AutoSize = true;
            this.lblScannerLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblScannerLabel.Location = new System.Drawing.Point(427, 154);
            this.lblScannerLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblScannerLabel.Name = "lblScannerLabel";
            this.lblScannerLabel.Size = new System.Drawing.Size(266, 23);
            this.lblScannerLabel.TabIndex = 3;
            this.lblScannerLabel.Text = "Scanner Input (Keep this focused)";
            // 
            // groupBoxResident
            // 
            this.groupBoxResident.Controls.Add(this.lblResidentContact);
            this.groupBoxResident.Controls.Add(this.lblResidentAddress);
            this.groupBoxResident.Controls.Add(this.lblResidentName);
            this.groupBoxResident.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.groupBoxResident.Location = new System.Drawing.Point(67, 271);
            this.groupBoxResident.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxResident.Name = "groupBoxResident";
            this.groupBoxResident.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxResident.Size = new System.Drawing.Size(933, 148);
            this.groupBoxResident.TabIndex = 4;
            this.groupBoxResident.TabStop = false;
            this.groupBoxResident.Text = "Scanned Resident";
            // 
            // lblResidentContact
            // 
            this.lblResidentContact.AutoSize = true;
            this.lblResidentContact.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblResidentContact.Location = new System.Drawing.Point(27, 105);
            this.lblResidentContact.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblResidentContact.Name = "lblResidentContact";
            this.lblResidentContact.Size = new System.Drawing.Size(0, 23);
            this.lblResidentContact.TabIndex = 2;
            // 
            // lblResidentAddress
            // 
            this.lblResidentAddress.AutoSize = true;
            this.lblResidentAddress.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblResidentAddress.Location = new System.Drawing.Point(27, 74);
            this.lblResidentAddress.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblResidentAddress.Name = "lblResidentAddress";
            this.lblResidentAddress.Size = new System.Drawing.Size(0, 23);
            this.lblResidentAddress.TabIndex = 1;
            // 
            // lblResidentName
            // 
            this.lblResidentName.AutoSize = true;
            this.lblResidentName.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblResidentName.ForeColor = System.Drawing.Color.Gray;
            this.lblResidentName.Location = new System.Drawing.Point(27, 37);
            this.lblResidentName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblResidentName.Name = "lblResidentName";
            this.lblResidentName.Size = new System.Drawing.Size(249, 32);
            this.lblResidentName.TabIndex = 0;
            this.lblResidentName.Text = "No resident scanned";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.label1.Location = new System.Drawing.Point(67, 455);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 25);
            this.label1.TabIndex = 5;
            this.label1.Text = "Amount:";
            // 
            // numAmount
            // 
            this.numAmount.DecimalPlaces = 2;
            this.numAmount.Font = new System.Drawing.Font("Segoe UI", 14F);
            this.numAmount.Location = new System.Drawing.Point(240, 449);
            this.numAmount.Margin = new System.Windows.Forms.Padding(4);
            this.numAmount.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.numAmount.Name = "numAmount";
            this.numAmount.Size = new System.Drawing.Size(333, 39);
            this.numAmount.TabIndex = 1;
            this.numAmount.ThousandsSeparator = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.label2.Location = new System.Drawing.Point(67, 517);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 25);
            this.label2.TabIndex = 7;
            this.label2.Text = "Purpose:";
            // 
            // cboPurpose
            // 
            this.cboPurpose.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboPurpose.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.cboPurpose.FormattingEnabled = true;
            this.cboPurpose.Location = new System.Drawing.Point(240, 513);
            this.cboPurpose.Margin = new System.Windows.Forms.Padding(4);
            this.cboPurpose.Name = "cboPurpose";
            this.cboPurpose.Size = new System.Drawing.Size(532, 33);
            this.cboPurpose.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.label3.Location = new System.Drawing.Point(67, 578);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 25);
            this.label3.TabIndex = 9;
            this.label3.Text = "Released By:";
            // 
            // txtReleasedBy
            // 
            this.txtReleasedBy.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtReleasedBy.Location = new System.Drawing.Point(240, 575);
            this.txtReleasedBy.Margin = new System.Windows.Forms.Padding(4);
            this.txtReleasedBy.Name = "txtReleasedBy";
            this.txtReleasedBy.Size = new System.Drawing.Size(399, 32);
            this.txtReleasedBy.TabIndex = 3;
            // 
            // btnRelease
            // 
            this.btnRelease.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnRelease.Enabled = false;
            this.btnRelease.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRelease.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.btnRelease.ForeColor = System.Drawing.Color.White;
            this.btnRelease.Location = new System.Drawing.Point(400, 652);
            this.btnRelease.Margin = new System.Windows.Forms.Padding(4);
            this.btnRelease.Name = "btnRelease";
            this.btnRelease.Size = new System.Drawing.Size(267, 62);
            this.btnRelease.TabIndex = 4;
            this.btnRelease.Text = "RELEASE AYUDA";
            this.btnRelease.UseVisualStyleBackColor = false;
            this.btnRelease.Click += new System.EventHandler(this.btnRelease_Click);
            // 
            // QuickAyudaReleaseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1067, 738);
            this.Controls.Add(this.btnRelease);
            this.Controls.Add(this.txtReleasedBy);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cboPurpose);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numAmount);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBoxResident);
            this.Controls.Add(this.lblScannerLabel);
            this.Controls.Add(this.txtBarcodeScanner);
            this.Controls.Add(this.lblInstructions);
            this.Controls.Add(this.panelTop);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "QuickAyudaReleaseForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Quick Ayuda Release";
            this.Load += new System.EventHandler(this.QuickAyudaReleaseForm_Load_1);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.groupBoxResident.ResumeLayout(false);
            this.groupBoxResident.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAmount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblInstructions;
        private System.Windows.Forms.TextBox txtBarcodeScanner;
        private System.Windows.Forms.Label lblScannerLabel;
        private System.Windows.Forms.GroupBox groupBoxResident;
        private System.Windows.Forms.Label lblResidentContact;
        private System.Windows.Forms.Label lblResidentAddress;
        private System.Windows.Forms.Label lblResidentName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numAmount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboPurpose;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtReleasedBy;
        private System.Windows.Forms.Button btnRelease;
    }
}