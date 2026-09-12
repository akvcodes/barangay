using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BarangayManagementSystem
{
    public partial class DashboardForm : Form
    {
        public DashboardForm()
        {
            InitializeComponent();
        }

        private void DashboardForm_Load(object sender, EventArgs e)
        {
            LoadDashboardStatistics();
            LoadRecentActivities();
        }

        private void LoadDashboardStatistics()
        {
            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn != null)
                {
                    try
                    {
                        // Total Residents — table: resident
                        SQLiteCommand cmd = new SQLiteCommand("SELECT COUNT(*) FROM resident WHERE IsActive = 1", conn);
                        lblTotalResidents.Text = cmd.ExecuteScalar().ToString();

                        // Total Officials — table: officials
                        cmd = new SQLiteCommand("SELECT COUNT(*) FROM officials WHERE IsActive = 1", conn);
                        lblTotalOfficials.Text = cmd.ExecuteScalar().ToString();

                        // Active Users — table: users
                        cmd = new SQLiteCommand("SELECT COUNT(*) FROM users WHERE IsActive = 1", conn);
                        lblActiveUsers.Text = cmd.ExecuteScalar().ToString();

                        // Pending Blotters — table: blotter
                        cmd = new SQLiteCommand("SELECT COUNT(*) FROM blotter WHERE Status = 'Pending'", conn);
                        lblPendingBlotters.Text = cmd.ExecuteScalar().ToString();

                        // Pending Clearances — table: clearance
                        cmd = new SQLiteCommand("SELECT COUNT(*) FROM clearance WHERE Status = 'Pending'", conn);
                        lblPendingClearances.Text = cmd.ExecuteScalar().ToString();

                        // Total Ayuda Released — table: ayuda
                        cmd = new SQLiteCommand("SELECT COALESCE(SUM(Amount), 0) FROM ayuda", conn);
                        decimal totalAyuda = Convert.ToDecimal(cmd.ExecuteScalar());
                        lblTotalAyuda.Text = "₱" + totalAyuda.ToString("N2");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading dashboard statistics: " + ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void LoadRecentActivities()
        {
            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn != null)
                {
                    try
                    {
                        string query = @"
                            SELECT 'Clearance' AS Type, ClearanceNumber AS RefNumber, DateIssued AS Date,
                                   CONCAT(r.LastName, ', ', r.FirstName) AS ResidentName, Purpose AS Details
                            FROM clearance c
                            INNER JOIN resident r ON c.ResidentID = r.idResident
                            UNION ALL
                            SELECT 'Certificate' AS Type, CertificateNumber AS RefNumber, DateIssued AS Date,
                                   CONCAT(r.LastName, ', ', r.FirstName) AS ResidentName, CertificateType AS Details
                            FROM certificates c
                            INNER JOIN resident r ON c.ResidentID = r.idResident
                            UNION ALL
                            SELECT 'Blotter' AS Type, BlotterNumber AS RefNumber, DateFiled AS Date,
                                   RespondentName AS ResidentName, IncidentType AS Details
                            FROM blotter
                            ORDER BY Date DESC
                            LIMIT 10";

                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, conn);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvRecentActivity.DataSource = dt;

                        if (dgvRecentActivity.Columns.Contains("Type"))
                            dgvRecentActivity.Columns["Type"].HeaderText = "Type";
                        if (dgvRecentActivity.Columns.Contains("RefNumber"))
                            dgvRecentActivity.Columns["RefNumber"].HeaderText = "Reference No.";
                        if (dgvRecentActivity.Columns.Contains("Date"))
                            dgvRecentActivity.Columns["Date"].HeaderText = "Date";
                        if (dgvRecentActivity.Columns.Contains("ResidentName"))
                            dgvRecentActivity.Columns["ResidentName"].HeaderText = "Resident";
                        if (dgvRecentActivity.Columns.Contains("Details"))
                            dgvRecentActivity.Columns["Details"].HeaderText = "Details";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading recent activities: " + ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnClose_MouseEnter(object sender, EventArgs e)
        {
            btnClose.ForeColor = Color.Red;
            btnClose.BackColor = Color.DarkBlue;
        }

        private void btnClose_MouseLeave(object sender, EventArgs e)
        {
            btnClose.ForeColor = Color.White;
            btnClose.BackColor = Color.DarkBlue;
        }

        private void btnClose_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void panelCard3_Paint(object sender, PaintEventArgs e)
        {
        }

        private void panelTop_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}