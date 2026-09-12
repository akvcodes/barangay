namespace BarangayManagementSystem
{
    partial class DashboardForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.panelCard1 = new System.Windows.Forms.Panel();
            this.lblTotalResidents = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panelCard2 = new System.Windows.Forms.Panel();
            this.lblTotalOfficials = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.panelCard3 = new System.Windows.Forms.Panel();
            this.lblActiveUsers = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.panelCard4 = new System.Windows.Forms.Panel();
            this.lblPendingBlotters = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.panelCard5 = new System.Windows.Forms.Panel();
            this.lblPendingClearances = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.panelCard6 = new System.Windows.Forms.Panel();
            this.lblTotalAyuda = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.panelRecentActivity = new System.Windows.Forms.Panel();
            this.dgvRecentActivity = new System.Windows.Forms.DataGridView();
            this.label7 = new System.Windows.Forms.Label();
            this.panelTop.SuspendLayout();
            this.panelCard1.SuspendLayout();
            this.panelCard2.SuspendLayout();
            this.panelCard3.SuspendLayout();
            this.panelCard4.SuspendLayout();
            this.panelCard5.SuspendLayout();
            this.panelCard6.SuspendLayout();
            this.panelRecentActivity.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecentActivity)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.DarkBlue;
            this.panelTop.Controls.Add(this.btnClose);
            this.panelTop.Controls.Add(this.lblTitle);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1081, 70);
            this.panelTop.TabIndex = 1;
            this.panelTop.Paint += new System.Windows.Forms.PaintEventHandler(this.panelTop_Paint);
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.Transparent;
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(1017, 12);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(40, 40);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click_1);
            this.btnClose.MouseEnter += new System.EventHandler(this.btnClose_MouseEnter);
            this.btnClose.MouseLeave += new System.EventHandler(this.btnClose_MouseLeave);
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(12, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(206, 41);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "DASHBOARD";
            // 
            // panelCard1
            // 
            this.panelCard1.BackColor = System.Drawing.Color.MidnightBlue;
            this.panelCard1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelCard1.Controls.Add(this.lblTotalResidents);
            this.panelCard1.Controls.Add(this.label1);
            this.panelCard1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panelCard1.ForeColor = System.Drawing.Color.White;
            this.panelCard1.Location = new System.Drawing.Point(18, 96);
            this.panelCard1.Name = "panelCard1";
            this.panelCard1.Size = new System.Drawing.Size(330, 120);
            this.panelCard1.TabIndex = 2;
            // 
            // lblTotalResidents
            // 
            this.lblTotalResidents.AutoSize = true;
            this.lblTotalResidents.BackColor = System.Drawing.Color.Transparent;
            this.lblTotalResidents.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalResidents.Location = new System.Drawing.Point(15, 47);
            this.lblTotalResidents.Name = "lblTotalResidents";
            this.lblTotalResidents.Size = new System.Drawing.Size(70, 81);
            this.lblTotalResidents.TabIndex = 1;
            this.lblTotalResidents.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(15, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(156, 28);
            this.label1.TabIndex = 0;
            this.label1.Text = "Total Residents";
            // 
            // panelCard2
            // 
            this.panelCard2.BackColor = System.Drawing.Color.DarkGreen;
            this.panelCard2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelCard2.Controls.Add(this.lblTotalOfficials);
            this.panelCard2.Controls.Add(this.label2);
            this.panelCard2.Location = new System.Drawing.Point(371, 97);
            this.panelCard2.Name = "panelCard2";
            this.panelCard2.Size = new System.Drawing.Size(330, 120);
            this.panelCard2.TabIndex = 3;
            // 
            // lblTotalOfficials
            // 
            this.lblTotalOfficials.AutoSize = true;
            this.lblTotalOfficials.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalOfficials.ForeColor = System.Drawing.Color.White;
            this.lblTotalOfficials.Location = new System.Drawing.Point(15, 48);
            this.lblTotalOfficials.Name = "lblTotalOfficials";
            this.lblTotalOfficials.Size = new System.Drawing.Size(70, 81);
            this.lblTotalOfficials.TabIndex = 1;
            this.lblTotalOfficials.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(15, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(144, 28);
            this.label2.TabIndex = 0;
            this.label2.Text = "Total Officials";
            // 
            // panelCard3
            // 
            this.panelCard3.BackColor = System.Drawing.Color.DarkMagenta;
            this.panelCard3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelCard3.Controls.Add(this.lblActiveUsers);
            this.panelCard3.Controls.Add(this.label3);
            this.panelCard3.Location = new System.Drawing.Point(727, 98);
            this.panelCard3.Name = "panelCard3";
            this.panelCard3.Size = new System.Drawing.Size(330, 120);
            this.panelCard3.TabIndex = 4;
            this.panelCard3.Paint += new System.Windows.Forms.PaintEventHandler(this.panelCard3_Paint);
            // 
            // lblActiveUsers
            // 
            this.lblActiveUsers.AutoSize = true;
            this.lblActiveUsers.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActiveUsers.ForeColor = System.Drawing.Color.White;
            this.lblActiveUsers.Location = new System.Drawing.Point(15, 48);
            this.lblActiveUsers.Name = "lblActiveUsers";
            this.lblActiveUsers.Size = new System.Drawing.Size(70, 81);
            this.lblActiveUsers.TabIndex = 1;
            this.lblActiveUsers.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(15, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(129, 28);
            this.label3.TabIndex = 0;
            this.label3.Text = "Active Users";
            // 
            // panelCard4
            // 
            this.panelCard4.BackColor = System.Drawing.Color.Brown;
            this.panelCard4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelCard4.Controls.Add(this.lblPendingBlotters);
            this.panelCard4.Controls.Add(this.label4);
            this.panelCard4.Location = new System.Drawing.Point(18, 240);
            this.panelCard4.Name = "panelCard4";
            this.panelCard4.Size = new System.Drawing.Size(330, 120);
            this.panelCard4.TabIndex = 5;
            // 
            // lblPendingBlotters
            // 
            this.lblPendingBlotters.AutoSize = true;
            this.lblPendingBlotters.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPendingBlotters.ForeColor = System.Drawing.Color.White;
            this.lblPendingBlotters.Location = new System.Drawing.Point(15, 48);
            this.lblPendingBlotters.Name = "lblPendingBlotters";
            this.lblPendingBlotters.Size = new System.Drawing.Size(70, 81);
            this.lblPendingBlotters.TabIndex = 1;
            this.lblPendingBlotters.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.ImageAlign = System.Drawing.ContentAlignment.BottomRight;
            this.label4.Location = new System.Drawing.Point(15, 15);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(169, 28);
            this.label4.TabIndex = 0;
            this.label4.Text = "Pending Blotters";
            // 
            // panelCard5
            // 
            this.panelCard5.BackColor = System.Drawing.Color.Goldenrod;
            this.panelCard5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelCard5.Controls.Add(this.lblPendingClearances);
            this.panelCard5.Controls.Add(this.label5);
            this.panelCard5.Location = new System.Drawing.Point(372, 240);
            this.panelCard5.Name = "panelCard5";
            this.panelCard5.Size = new System.Drawing.Size(330, 120);
            this.panelCard5.TabIndex = 6;
            // 
            // lblPendingClearances
            // 
            this.lblPendingClearances.AutoSize = true;
            this.lblPendingClearances.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPendingClearances.ForeColor = System.Drawing.Color.White;
            this.lblPendingClearances.Location = new System.Drawing.Point(15, 48);
            this.lblPendingClearances.Name = "lblPendingClearances";
            this.lblPendingClearances.Size = new System.Drawing.Size(70, 81);
            this.lblPendingClearances.TabIndex = 1;
            this.lblPendingClearances.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(15, 15);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(195, 28);
            this.label5.TabIndex = 0;
            this.label5.Text = "Pending Clearances";
            // 
            // panelCard6
            // 
            this.panelCard6.BackColor = System.Drawing.Color.MediumTurquoise;
            this.panelCard6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelCard6.Controls.Add(this.lblTotalAyuda);
            this.panelCard6.Controls.Add(this.label6);
            this.panelCard6.Location = new System.Drawing.Point(728, 240);
            this.panelCard6.Name = "panelCard6";
            this.panelCard6.Size = new System.Drawing.Size(330, 120);
            this.panelCard6.TabIndex = 7;
            // 
            // lblTotalAyuda
            // 
            this.lblTotalAyuda.AutoSize = true;
            this.lblTotalAyuda.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalAyuda.ForeColor = System.Drawing.Color.White;
            this.lblTotalAyuda.Location = new System.Drawing.Point(15, 48);
            this.lblTotalAyuda.Name = "lblTotalAyuda";
            this.lblTotalAyuda.Size = new System.Drawing.Size(194, 81);
            this.lblTotalAyuda.TabIndex = 1;
            this.lblTotalAyuda.Text = "₱0.00";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(15, 15);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(162, 28);
            this.label6.TabIndex = 0;
            this.label6.Text = "Ayuda Released";
            // 
            // panelRecentActivity
            // 
            this.panelRecentActivity.BackColor = System.Drawing.Color.White;
            this.panelRecentActivity.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelRecentActivity.Controls.Add(this.dgvRecentActivity);
            this.panelRecentActivity.Controls.Add(this.label7);
            this.panelRecentActivity.Location = new System.Drawing.Point(19, 380);
            this.panelRecentActivity.Name = "panelRecentActivity";
            this.panelRecentActivity.Size = new System.Drawing.Size(1040, 280);
            this.panelRecentActivity.TabIndex = 8;
            // 
            // dgvRecentActivity
            // 
            this.dgvRecentActivity.AllowUserToAddRows = false;
            this.dgvRecentActivity.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvRecentActivity.BackgroundColor = System.Drawing.Color.White;
            this.dgvRecentActivity.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(73)))), ((int)(((byte)(94)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvRecentActivity.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvRecentActivity.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRecentActivity.Location = new System.Drawing.Point(19, 50);
            this.dgvRecentActivity.Name = "dgvRecentActivity";
            this.dgvRecentActivity.ReadOnly = true;
            this.dgvRecentActivity.RowHeadersWidth = 51;
            this.dgvRecentActivity.RowTemplate.Height = 24;
            this.dgvRecentActivity.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRecentActivity.Size = new System.Drawing.Size(1000, 220);
            this.dgvRecentActivity.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.label7.Location = new System.Drawing.Point(20, 15);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(192, 31);
            this.label7.TabIndex = 0;
            this.label7.Text = "Recent Activities";
            // 
            // DashboardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(1081, 674);
            this.Controls.Add(this.panelRecentActivity);
            this.Controls.Add(this.panelCard6);
            this.Controls.Add(this.panelCard5);
            this.Controls.Add(this.panelCard4);
            this.Controls.Add(this.panelCard3);
            this.Controls.Add(this.panelCard2);
            this.Controls.Add(this.panelCard1);
            this.Controls.Add(this.panelTop);
            this.Location = new System.Drawing.Point(30, 100);
            this.Name = "DashboardForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Dashboard";
            this.Load += new System.EventHandler(this.DashboardForm_Load);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelCard1.ResumeLayout(false);
            this.panelCard1.PerformLayout();
            this.panelCard2.ResumeLayout(false);
            this.panelCard2.PerformLayout();
            this.panelCard3.ResumeLayout(false);
            this.panelCard3.PerformLayout();
            this.panelCard4.ResumeLayout(false);
            this.panelCard4.PerformLayout();
            this.panelCard5.ResumeLayout(false);
            this.panelCard5.PerformLayout();
            this.panelCard6.ResumeLayout(false);
            this.panelCard6.PerformLayout();
            this.panelRecentActivity.ResumeLayout(false);
            this.panelRecentActivity.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRecentActivity)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel panelCard1;
        private System.Windows.Forms.Label lblTotalResidents;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelCard2;
        private System.Windows.Forms.Label lblTotalOfficials;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panelCard3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblActiveUsers;
        private System.Windows.Forms.Panel panelCard4;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblPendingBlotters;
        private System.Windows.Forms.Panel panelCard5;
        private System.Windows.Forms.Label lblPendingClearances;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Panel panelCard6;
        private System.Windows.Forms.Label lblTotalAyuda;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel panelRecentActivity;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.DataGridView dgvRecentActivity;
    }
}