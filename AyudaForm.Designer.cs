namespace BarangayManagementSystem
{
    partial class AyudaForm
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
            this.panelMain = new System.Windows.Forms.Panel();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.groupBoxAyuda = new System.Windows.Forms.GroupBox();
            this.txtRemarks = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.cboStatus = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtReleasedBy = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cboPurpose = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.dtpDateReleased = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.numAmount = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.txtBarcodeID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBoxResident = new System.Windows.Forms.GroupBox();
            this.lblResidentContact = new System.Windows.Forms.Label();
            this.lblResidentAddress = new System.Windows.Forms.Label();
            this.lblResidentName = new System.Windows.Forms.Label();
            this.panelTop.SuspendLayout();
            this.panelMain.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.groupBoxAyuda.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAmount)).BeginInit();
            this.groupBoxResident.SuspendLayout();
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
            this.panelTop.Size = new System.Drawing.Size(933, 74);
            this.panelTop.TabIndex = 0;
            this.panelTop.Paint += new System.Windows.Forms.PaintEventHandler(this.panelTop_Paint);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(867, 12);
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
            this.lblTitle.Size = new System.Drawing.Size(408, 37);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "RELEASE ASSISTANCE / AYUDA";
            // 
            // panelMain
            // 
            this.panelMain.BackColor = System.Drawing.Color.White;
            this.panelMain.Controls.Add(this.panelButtons);
            this.panelMain.Controls.Add(this.groupBoxAyuda);
            this.panelMain.Controls.Add(this.groupBoxResident);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 74);
            this.panelMain.Margin = new System.Windows.Forms.Padding(4);
            this.panelMain.Name = "panelMain";
            this.panelMain.Padding = new System.Windows.Forms.Padding(27, 25, 27, 25);
            this.panelMain.Size = new System.Drawing.Size(933, 788);
            this.panelMain.TabIndex = 1;
            this.panelMain.Paint += new System.Windows.Forms.PaintEventHandler(this.panelMain_Paint);
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.btnClear);
            this.panelButtons.Controls.Add(this.btnAdd);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelButtons.Location = new System.Drawing.Point(27, 690);
            this.panelButtons.Margin = new System.Windows.Forms.Padding(4);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(879, 73);
            this.panelButtons.TabIndex = 2;
            // 
            // btnClear
            // 
            this.btnClear.BackColor = System.Drawing.Color.Gray;
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClear.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnClear.ForeColor = System.Drawing.Color.White;
            this.btnClear.Location = new System.Drawing.Point(240, 12);
            this.btnClear.Margin = new System.Windows.Forms.Padding(4);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(200, 49);
            this.btnClear.TabIndex = 1;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnAdd.ForeColor = System.Drawing.Color.White;
            this.btnAdd.Location = new System.Drawing.Point(27, 12);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(4);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(200, 49);
            this.btnAdd.TabIndex = 0;
            this.btnAdd.Text = "Release Ayuda";
            this.btnAdd.UseVisualStyleBackColor = false;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // groupBoxAyuda
            // 
            this.groupBoxAyuda.Controls.Add(this.txtRemarks);
            this.groupBoxAyuda.Controls.Add(this.label8);
            this.groupBoxAyuda.Controls.Add(this.cboStatus);
            this.groupBoxAyuda.Controls.Add(this.label7);
            this.groupBoxAyuda.Controls.Add(this.txtReleasedBy);
            this.groupBoxAyuda.Controls.Add(this.label6);
            this.groupBoxAyuda.Controls.Add(this.cboPurpose);
            this.groupBoxAyuda.Controls.Add(this.label5);
            this.groupBoxAyuda.Controls.Add(this.dtpDateReleased);
            this.groupBoxAyuda.Controls.Add(this.label4);
            this.groupBoxAyuda.Controls.Add(this.numAmount);
            this.groupBoxAyuda.Controls.Add(this.label3);
            this.groupBoxAyuda.Controls.Add(this.txtBarcodeID);
            this.groupBoxAyuda.Controls.Add(this.label2);
            this.groupBoxAyuda.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxAyuda.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.groupBoxAyuda.Location = new System.Drawing.Point(27, 173);
            this.groupBoxAyuda.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxAyuda.Name = "groupBoxAyuda";
            this.groupBoxAyuda.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxAyuda.Size = new System.Drawing.Size(879, 517);
            this.groupBoxAyuda.TabIndex = 1;
            this.groupBoxAyuda.TabStop = false;
            this.groupBoxAyuda.Text = "Assistance Details";
            // 
            // txtRemarks
            // 
            this.txtRemarks.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtRemarks.Location = new System.Drawing.Point(240, 394);
            this.txtRemarks.Margin = new System.Windows.Forms.Padding(4);
            this.txtRemarks.Multiline = true;
            this.txtRemarks.Name = "txtRemarks";
            this.txtRemarks.Size = new System.Drawing.Size(599, 98);
            this.txtRemarks.TabIndex = 13;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label8.Location = new System.Drawing.Point(27, 394);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(159, 23);
            this.label8.TabIndex = 12;
            this.label8.Text = "Remarks (Optional):";
            // 
            // cboStatus
            // 
            this.cboStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStatus.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboStatus.FormattingEnabled = true;
            this.cboStatus.Location = new System.Drawing.Point(240, 338);
            this.cboStatus.Margin = new System.Windows.Forms.Padding(4);
            this.cboStatus.Name = "cboStatus";
            this.cboStatus.Size = new System.Drawing.Size(332, 31);
            this.cboStatus.TabIndex = 11;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label7.Location = new System.Drawing.Point(27, 342);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(60, 23);
            this.label7.TabIndex = 10;
            this.label7.Text = "Status:";
            // 
            // txtReleasedBy
            // 
            this.txtReleasedBy.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtReleasedBy.Location = new System.Drawing.Point(240, 283);
            this.txtReleasedBy.Margin = new System.Windows.Forms.Padding(4);
            this.txtReleasedBy.Name = "txtReleasedBy";
            this.txtReleasedBy.Size = new System.Drawing.Size(465, 30);
            this.txtReleasedBy.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label6.Location = new System.Drawing.Point(27, 287);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(104, 23);
            this.label6.TabIndex = 8;
            this.label6.Text = "Released By:";
            // 
            // cboPurpose
            // 
            this.cboPurpose.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cboPurpose.FormattingEnabled = true;
            this.cboPurpose.Location = new System.Drawing.Point(240, 172);
            this.cboPurpose.Margin = new System.Windows.Forms.Padding(4);
            this.cboPurpose.Name = "cboPurpose";
            this.cboPurpose.Size = new System.Drawing.Size(599, 31);
            this.cboPurpose.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label5.Location = new System.Drawing.Point(27, 176);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(128, 23);
            this.label5.TabIndex = 6;
            this.label5.Text = "Purpose / Type:";
            // 
            // dtpDateReleased
            // 
            this.dtpDateReleased.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dtpDateReleased.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDateReleased.Location = new System.Drawing.Point(240, 228);
            this.dtpDateReleased.Margin = new System.Windows.Forms.Padding(4);
            this.dtpDateReleased.Name = "dtpDateReleased";
            this.dtpDateReleased.Size = new System.Drawing.Size(332, 30);
            this.dtpDateReleased.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label4.Location = new System.Drawing.Point(27, 231);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(122, 23);
            this.label4.TabIndex = 4;
            this.label4.Text = "Date Released:";
            // 
            // numAmount
            // 
            this.numAmount.DecimalPlaces = 2;
            this.numAmount.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.numAmount.Location = new System.Drawing.Point(240, 117);
            this.numAmount.Margin = new System.Windows.Forms.Padding(4);
            this.numAmount.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.numAmount.Name = "numAmount";
            this.numAmount.Size = new System.Drawing.Size(333, 30);
            this.numAmount.TabIndex = 3;
            this.numAmount.ThousandsSeparator = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label3.Location = new System.Drawing.Point(27, 121);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 23);
            this.label3.TabIndex = 2;
            this.label3.Text = "Amount:";
            // 
            // txtBarcodeID
            // 
            this.txtBarcodeID.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtBarcodeID.Location = new System.Drawing.Point(240, 62);
            this.txtBarcodeID.Margin = new System.Windows.Forms.Padding(4);
            this.txtBarcodeID.Name = "txtBarcodeID";
            this.txtBarcodeID.Size = new System.Drawing.Size(465, 30);
            this.txtBarcodeID.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label2.Location = new System.Drawing.Point(27, 65);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(179, 23);
            this.label2.TabIndex = 0;
            this.label2.Text = "Barcode ID (Optional):";
            // 
            // groupBoxResident
            // 
            this.groupBoxResident.Controls.Add(this.lblResidentContact);
            this.groupBoxResident.Controls.Add(this.lblResidentAddress);
            this.groupBoxResident.Controls.Add(this.lblResidentName);
            this.groupBoxResident.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxResident.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.groupBoxResident.Location = new System.Drawing.Point(27, 25);
            this.groupBoxResident.Margin = new System.Windows.Forms.Padding(4);
            this.groupBoxResident.Name = "groupBoxResident";
            this.groupBoxResident.Padding = new System.Windows.Forms.Padding(4);
            this.groupBoxResident.Size = new System.Drawing.Size(879, 148);
            this.groupBoxResident.TabIndex = 0;
            this.groupBoxResident.TabStop = false;
            this.groupBoxResident.Text = "Resident Information";
            // 
            // lblResidentContact
            // 
            this.lblResidentContact.AutoSize = true;
            this.lblResidentContact.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblResidentContact.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblResidentContact.Location = new System.Drawing.Point(27, 105);
            this.lblResidentContact.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblResidentContact.Name = "lblResidentContact";
            this.lblResidentContact.Size = new System.Drawing.Size(74, 23);
            this.lblResidentContact.TabIndex = 2;
            this.lblResidentContact.Text = "Contact:";
            // 
            // lblResidentAddress
            // 
            this.lblResidentAddress.AutoSize = true;
            this.lblResidentAddress.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblResidentAddress.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblResidentAddress.Location = new System.Drawing.Point(27, 74);
            this.lblResidentAddress.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblResidentAddress.Name = "lblResidentAddress";
            this.lblResidentAddress.Size = new System.Drawing.Size(74, 23);
            this.lblResidentAddress.TabIndex = 1;
            this.lblResidentAddress.Text = "Address:";
            // 
            // lblResidentName
            // 
            this.lblResidentName.AutoSize = true;
            this.lblResidentName.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblResidentName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.lblResidentName.Location = new System.Drawing.Point(27, 37);
            this.lblResidentName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblResidentName.Name = "lblResidentName";
            this.lblResidentName.Size = new System.Drawing.Size(93, 25);
            this.lblResidentName.TabIndex = 0;
            this.lblResidentName.Text = "Resident:";
            // 
            // AyudaForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(933, 862);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.panelTop);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "AyudaForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Release Assistance / Ayuda";
            this.Load += new System.EventHandler(this.AyudaForm_Load);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelMain.ResumeLayout(false);
            this.panelButtons.ResumeLayout(false);
            this.groupBoxAyuda.ResumeLayout(false);
            this.groupBoxAyuda.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAmount)).EndInit();
            this.groupBoxResident.ResumeLayout(false);
            this.groupBoxResident.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.GroupBox groupBoxResident;
        private System.Windows.Forms.Label lblResidentContact;
        private System.Windows.Forms.Label lblResidentAddress;
        private System.Windows.Forms.Label lblResidentName;
        private System.Windows.Forms.GroupBox groupBoxAyuda;
        private System.Windows.Forms.TextBox txtBarcodeID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numAmount;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker dtpDateReleased;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cboPurpose;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtReleasedBy;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cboStatus;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtRemarks;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnAdd;
    }
}