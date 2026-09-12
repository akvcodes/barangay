namespace BarangayManagementSystem
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.panelLeft = new System.Windows.Forms.Panel();
            this.panelRight = new System.Windows.Forms.Panel();
            this.lblClose = new System.Windows.Forms.Label();
            this.lblSystemTitle = new System.Windows.Forms.Label();
            this.lblSystemSub = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.chkShowPassword = new System.Windows.Forms.CheckBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.lblMessage = new System.Windows.Forms.Label();
            this.lblFooter = new System.Windows.Forms.Label();
            this._lblConnType = new System.Windows.Forms.Label();
            this._rbLocal = new System.Windows.Forms.RadioButton();
            this._rbRemote = new System.Windows.Forms.RadioButton();
            this._rbOnline = new System.Windows.Forms.RadioButton();
            this._lblConnStatus = new System.Windows.Forms.Label();
            this.lblLGUAddress = new System.Windows.Forms.Label();
            this.lblLGUName = new System.Windows.Forms.Label();
            this.pictureBoxLogo = new System.Windows.Forms.PictureBox();
            this.local_government_logo = new System.Windows.Forms.PictureBox();
            this.pictureBoxSeal = new System.Windows.Forms.PictureBox();
            this.panelLeft.SuspendLayout();
            this.panelRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.local_government_logo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSeal)).BeginInit();
            this.SuspendLayout();
            // 
            // panelLeft
            // 
            this.panelLeft.AutoSize = true;
            this.panelLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(22)))), ((int)(((byte)(128)))));
            this.panelLeft.Controls.Add(this.local_government_logo);
            this.panelLeft.Controls.Add(this.pictureBoxSeal);
            this.panelLeft.Controls.Add(this.lblLGUName);
            this.panelLeft.Controls.Add(this.lblLGUAddress);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(1674, 1055);
            this.panelLeft.TabIndex = 0;
            this.panelLeft.Paint += new System.Windows.Forms.PaintEventHandler(this.panelLeft_Paint);
            // 
            // panelRight
            // 
            this.panelRight.AutoSize = true;
            this.panelRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(243)))), ((int)(((byte)(252)))));
            this.panelRight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelRight.Controls.Add(this.lblClose);
            this.panelRight.Controls.Add(this.pictureBoxLogo);
            this.panelRight.Controls.Add(this.lblSystemTitle);
            this.panelRight.Controls.Add(this.lblSystemSub);
            this.panelRight.Controls.Add(this.label4);
            this.panelRight.Controls.Add(this.txtUsername);
            this.panelRight.Controls.Add(this.label5);
            this.panelRight.Controls.Add(this.txtPassword);
            this.panelRight.Controls.Add(this.chkShowPassword);
            this.panelRight.Controls.Add(this.btnLogin);
            this.panelRight.Controls.Add(this.lblMessage);
            this.panelRight.Controls.Add(this.lblFooter);
            this.panelRight.Controls.Add(this._lblConnType);
            this.panelRight.Controls.Add(this._rbLocal);
            this.panelRight.Controls.Add(this._rbRemote);
            this.panelRight.Controls.Add(this._rbOnline);
            this.panelRight.Controls.Add(this._lblConnStatus);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelRight.Location = new System.Drawing.Point(1159, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(515, 1055);
            this.panelRight.TabIndex = 1;
            this.panelRight.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // lblClose
            // 
            this.lblClose.AutoSize = true;
            this.lblClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblClose.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblClose.ForeColor = System.Drawing.Color.Gray;
            this.lblClose.Location = new System.Drawing.Point(483, 8);
            this.lblClose.Name = "lblClose";
            this.lblClose.Size = new System.Drawing.Size(27, 30);
            this.lblClose.TabIndex = 0;
            this.lblClose.Text = "X";
            this.lblClose.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.lblClose.Click += new System.EventHandler(this.lblClose_Click);
            this.lblClose.MouseEnter += new System.EventHandler(this.lblClose_MouseEnter);
            this.lblClose.MouseLeave += new System.EventHandler(this.lblClose_MouseLeave);
            // 
            // lblSystemTitle
            // 
            this.lblSystemTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblSystemTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(22)))), ((int)(((byte)(128)))));
            this.lblSystemTitle.Location = new System.Drawing.Point(46, 334);
            this.lblSystemTitle.Name = "lblSystemTitle";
            this.lblSystemTitle.Size = new System.Drawing.Size(442, 30);
            this.lblSystemTitle.TabIndex = 2;
            this.lblSystemTitle.Text = "Barangay Management System";
            this.lblSystemTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSystemSub
            // 
            this.lblSystemSub.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSystemSub.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(150)))), ((int)(((byte)(180)))));
            this.lblSystemSub.Location = new System.Drawing.Point(46, 368);
            this.lblSystemSub.Name = "lblSystemSub";
            this.lblSystemSub.Size = new System.Drawing.Size(442, 22);
            this.lblSystemSub.TabIndex = 3;
            this.lblSystemSub.Text = "Please sign in to your account";
            this.lblSystemSub.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(70)))), ((int)(((byte)(110)))));
            this.label4.Location = new System.Drawing.Point(78, 408);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(378, 22);
            this.label4.TabIndex = 4;
            this.label4.Text = "Username";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // txtUsername
            // 
            this.txtUsername.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.txtUsername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUsername.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtUsername.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(25)))), ((int)(((byte)(70)))));
            this.txtUsername.Location = new System.Drawing.Point(78, 432);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(378, 32);
            this.txtUsername.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(70)))), ((int)(((byte)(110)))));
            this.label5.Location = new System.Drawing.Point(78, 478);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(378, 22);
            this.label5.TabIndex = 6;
            this.label5.Text = "Password";
            // 
            // txtPassword
            // 
            this.txtPassword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPassword.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(25)))), ((int)(((byte)(70)))));
            this.txtPassword.Location = new System.Drawing.Point(78, 502);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '●';
            this.txtPassword.Size = new System.Drawing.Size(378, 32);
            this.txtPassword.TabIndex = 7;
            // 
            // chkShowPassword
            // 
            this.chkShowPassword.AutoSize = true;
            this.chkShowPassword.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkShowPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(120)))), ((int)(((byte)(150)))));
            this.chkShowPassword.Location = new System.Drawing.Point(78, 544);
            this.chkShowPassword.Name = "chkShowPassword";
            this.chkShowPassword.Size = new System.Drawing.Size(132, 24);
            this.chkShowPassword.TabIndex = 8;
            this.chkShowPassword.Text = "Show Password";
            this.chkShowPassword.Click += new System.EventHandler(this.chkShowPassword_CheckedChanged);
            // 
            // btnLogin
            // 
            this.btnLogin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(22)))), ((int)(((byte)(128)))));
            this.btnLogin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(8)))), ((int)(((byte)(14)))), ((int)(((byte)(100)))));
            this.btnLogin.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(38)))), ((int)(((byte)(160)))));
            this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogin.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnLogin.ForeColor = System.Drawing.Color.White;
            this.btnLogin.Location = new System.Drawing.Point(78, 580);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(378, 44);
            this.btnLogin.TabIndex = 9;
            this.btnLogin.Text = "LOGIN";
            this.btnLogin.UseVisualStyleBackColor = false;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // lblMessage
            // 
            this.lblMessage.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblMessage.ForeColor = System.Drawing.Color.Red;
            this.lblMessage.Location = new System.Drawing.Point(78, 632);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(378, 22);
            this.lblMessage.TabIndex = 10;
            this.lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFooter
            // 
            this.lblFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblFooter.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblFooter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(178)))), ((int)(((byte)(210)))));
            this.lblFooter.Location = new System.Drawing.Point(0, 1008);
            this.lblFooter.Name = "lblFooter";
            this.lblFooter.Size = new System.Drawing.Size(513, 45);
            this.lblFooter.TabIndex = 11;
            this.lblFooter.Text = "Barangay Management System  •  Matanao, Davao del Sur";
            this.lblFooter.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblFooter.Click += new System.EventHandler(this.lblFooter_Click);
            // 
            // _lblConnType
            // 
            this._lblConnType.Location = new System.Drawing.Point(0, 0);
            this._lblConnType.Name = "_lblConnType";
            this._lblConnType.Size = new System.Drawing.Size(100, 23);
            this._lblConnType.TabIndex = 20;
            this._lblConnType.Visible = false;
            // 
            // _rbLocal
            // 
            this._rbLocal.Location = new System.Drawing.Point(0, 0);
            this._rbLocal.Name = "_rbLocal";
            this._rbLocal.Size = new System.Drawing.Size(104, 24);
            this._rbLocal.TabIndex = 21;
            this._rbLocal.Visible = false;
            this._rbLocal.CheckedChanged += new System.EventHandler(this.rbLocal_CheckedChanged);
            // 
            // _rbRemote
            // 
            this._rbRemote.Location = new System.Drawing.Point(0, 0);
            this._rbRemote.Name = "_rbRemote";
            this._rbRemote.Size = new System.Drawing.Size(104, 24);
            this._rbRemote.TabIndex = 22;
            this._rbRemote.Visible = false;
            this._rbRemote.CheckedChanged += new System.EventHandler(this.rbRemote_CheckedChanged);
            // 
            // _rbOnline
            // 
            this._rbOnline.Location = new System.Drawing.Point(0, 0);
            this._rbOnline.Name = "_rbOnline";
            this._rbOnline.Size = new System.Drawing.Size(104, 24);
            this._rbOnline.TabIndex = 23;
            this._rbOnline.Visible = false;
            this._rbOnline.CheckedChanged += new System.EventHandler(this.rbOnline_CheckedChanged);
            // 
            // _lblConnStatus
            // 
            this._lblConnStatus.Location = new System.Drawing.Point(0, 0);
            this._lblConnStatus.Name = "_lblConnStatus";
            this._lblConnStatus.Size = new System.Drawing.Size(100, 23);
            this._lblConnStatus.TabIndex = 24;
            this._lblConnStatus.Visible = false;
            // 
            // lblLGUAddress
            // 
            this.lblLGUAddress.Cursor = System.Windows.Forms.Cursors.No;
            this.lblLGUAddress.Font = new System.Drawing.Font("Segoe UI", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLGUAddress.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(200)))), ((int)(((byte)(255)))));
            this.lblLGUAddress.Location = new System.Drawing.Point(318, 583);
            this.lblLGUAddress.Name = "lblLGUAddress";
            this.lblLGUAddress.Size = new System.Drawing.Size(760, 39);
            this.lblLGUAddress.TabIndex = 2;
            this.lblLGUAddress.Text = "Poblacion, Matanao, Davao del Sur";
            this.lblLGUAddress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblLGUAddress.Click += new System.EventHandler(this.lblLGUAddress_Click);
            // 
            // lblLGUName
            // 
            this.lblLGUName.Font = new System.Drawing.Font("Segoe UI", 25.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLGUName.ForeColor = System.Drawing.Color.White;
            this.lblLGUName.Location = new System.Drawing.Point(3, 506);
            this.lblLGUName.Name = "lblLGUName";
            this.lblLGUName.Size = new System.Drawing.Size(1345, 77);
            this.lblLGUName.TabIndex = 1;
            this.lblLGUName.Text = "Local Government of Matanao";
            this.lblLGUName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBoxLogo
            // 
            this.pictureBoxLogo.BackgroundImage = global::BarangayManagementSystem.Properties.Resources.Screenshot_2026_04_09_152025_removebg_preview;
            this.pictureBoxLogo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureBoxLogo.Location = new System.Drawing.Point(185, 189);
            this.pictureBoxLogo.Name = "pictureBoxLogo";
            this.pictureBoxLogo.Size = new System.Drawing.Size(166, 142);
            this.pictureBoxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxLogo.TabIndex = 1;
            this.pictureBoxLogo.TabStop = false;
            this.pictureBoxLogo.Click += new System.EventHandler(this.pictureBox3_Click);
            // 
            // local_government_logo
            // 
            this.local_government_logo.BackgroundImage = global::BarangayManagementSystem.Properties.Resources.LOCAL_GOVERNMENT_OF_MATANAO_LOGO;
            this.local_government_logo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.local_government_logo.Location = new System.Drawing.Point(521, 190);
            this.local_government_logo.Name = "local_government_logo";
            this.local_government_logo.Size = new System.Drawing.Size(355, 320);
            this.local_government_logo.TabIndex = 3;
            this.local_government_logo.TabStop = false;
            // 
            // pictureBoxSeal
            // 
            this.pictureBoxSeal.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pictureBoxSeal.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBoxSeal.Location = new System.Drawing.Point(1237, 707);
            this.pictureBoxSeal.Name = "pictureBoxSeal";
            this.pictureBoxSeal.Size = new System.Drawing.Size(160, 160);
            this.pictureBoxSeal.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxSeal.TabIndex = 0;
            this.pictureBoxSeal.TabStop = false;
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1674, 1055);
            this.ControlBox = false;
            this.Controls.Add(this.panelRight);
            this.Controls.Add(this.panelLeft);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Barangay Management System - Login";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.LoginForm_Load);
            this.panelLeft.ResumeLayout(false);
            this.panelRight.ResumeLayout(false);
            this.panelRight.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.local_government_logo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSeal)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        // ── Field declarations ────────────────────────────────────────────────
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.PictureBox pictureBoxSeal;

        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.Label lblClose;
        private System.Windows.Forms.PictureBox pictureBoxLogo;
        private System.Windows.Forms.Label lblSystemTitle;
        private System.Windows.Forms.Label lblSystemSub;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.CheckBox chkShowPassword;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.Label lblFooter;

        // hidden legacy
        private System.Windows.Forms.Label _lblConnType;
        private System.Windows.Forms.RadioButton _rbLocal;
        private System.Windows.Forms.RadioButton _rbRemote;
        private System.Windows.Forms.RadioButton _rbOnline;
        private System.Windows.Forms.Label _lblConnStatus;
        private System.Windows.Forms.PictureBox local_government_logo;
        private System.Windows.Forms.Label lblLGUName;
        private System.Windows.Forms.Label lblLGUAddress;
    }
}