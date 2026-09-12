using System.Data.SQLite;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BarangayManagementSystem
{
    public partial class IncidentReportsForm : Form
    {
        private int _residentId;
        private string _residentName;
        private int _incidentId;

        public IncidentReportsForm(int residentId, string residentName)
        {
            InitializeComponent();
            _residentId = residentId;
            _residentName = residentName;
            _incidentId = 0;
        }

        public IncidentReportsForm(int residentId, string residentName, int incidentId)
        {
            InitializeComponent();
            _residentId = residentId;
            _residentName = residentName;
            _incidentId = incidentId;
        }

        private void IncidentReportsForm_Load(object sender, EventArgs e)
        {
            bool isEdit = _incidentId > 0;

            FormStyler.ApplyTheme(this);
            FormStyler.StyleHeader(
                headerPanel: panelTop,
                titleLabel: lblTitle,
                titleText: isEdit ? "Edit Incident Report" : "New Incident Report",
                subtitleText: isEdit ? "Update an existing incident report"
                                      : "File a new incident report",
                subtitleLabel: lblTitle,
                closeButton: btnClose
            );
            FormStyler.CenterPanel(panelInputs);
            FormStyler.SetReadOnly(txtResidentName, _residentName);

            dtpDateReported.Value = DateTime.Now;
            dtpResolvedDate.Value = DateTime.Now;
            cboStatus.SelectedIndex = 0;

            if (isEdit)
            {
                btnAdd.Text = "💾  Save Changes";
                FormStyler.ApplyButtonStyle(btnAdd, FormStyler.SuccessGreen, Color.FromArgb(15, 130, 55));
                txtReportNumber.ReadOnly = true;
                txtReportNumber.BackColor = FormStyler.InputDisabled;
                txtReportNumber.ForeColor = FormStyler.TextMuted;
                LoadIncidentData();
            }
            else
            {
                btnAdd.Text = "✚  File Report";
                GenerateReportNumber();
            }
        }

        private void GenerateReportNumber()
        {
            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn == null) return;
                try
                {
                    int count = Convert.ToInt32(new SQLiteCommand("SELECT COUNT(*) FROM incidentreports", conn).ExecuteScalar());
                    txtReportNumber.Text = $"INC-{DateTime.Now.Year}-{(count + 1):D3}";
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }

        private void LoadIncidentData()
        {
            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn == null) return;
                try
                {
                    var cmd = new SQLiteCommand("SELECT * FROM incidentreports WHERE idincidentreports=@id", conn);
                    cmd.Parameters.AddWithValue("@id", _incidentId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            txtReportNumber.Text = r["ReportNumber"].ToString();
                            dtpDateReported.Value = Convert.ToDateTime(r["DateReported"]);
                            string it = r["IncidentType"].ToString();
                            if (cboIncidentType.Items.Contains(it)) cboIncidentType.SelectedItem = it;
                            txtDescription.Text = r["Description"]?.ToString() ?? "";
                            txtLocation.Text = r["Location"]?.ToString() ?? "";
                            string sv = r["Severity"].ToString();
                            if (cboSeverity.Items.Contains(sv)) cboSeverity.SelectedItem = sv;
                            string s = r["Status"].ToString();
                            if (cboStatus.Items.Contains(s)) cboStatus.SelectedItem = s;
                            txtAssignedTo.Text = r["AssignedTo"]?.ToString() ?? "";
                            if (r["ResolvedDate"] != DBNull.Value)
                                dtpResolvedDate.Value = Convert.ToDateTime(r["ResolvedDate"]);
                            txtRemarks.Text = r["Remarks"]?.ToString() ?? "";
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }

        private bool ValidateInput()
        {
            if (cboIncidentType.SelectedIndex == -1)
            { MessageBox.Show("Please select an Incident Type.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); cboIncidentType.Focus(); return false; }
            if (string.IsNullOrWhiteSpace(txtLocation.Text))
            { MessageBox.Show("Location is required.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtLocation.Focus(); return false; }
            if (cboSeverity.SelectedIndex == -1)
            { MessageBox.Show("Please select a Severity level.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); cboSeverity.Focus(); return false; }
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            { MessageBox.Show("Description is required.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtDescription.Focus(); return false; }
            if (cboStatus.SelectedIndex == -1)
            { MessageBox.Show("Please select a Status.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); cboStatus.Focus(); return false; }
            return true;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;
            if (_incidentId > 0) UpdateIncident(); else AddIncident();
        }

        private void AddIncident()
        {
            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn == null) return;
                try
                {
                    string st = cboStatus.SelectedItem.ToString();
                    var cmd = new SQLiteCommand(@"INSERT INTO incidentreports
                        (ReportNumber,DateReported,ReportedByID,IncidentType,Description,
                         Location,Severity,Status,AssignedTo,ResolvedDate,Remarks)
                        VALUES (@num,@dr,@rid,@it,@desc,@loc,@sv,@st,@at,@rdate,@rem)", conn);
                    cmd.Parameters.AddWithValue("@num", txtReportNumber.Text.Trim());
                    cmd.Parameters.AddWithValue("@dr", dtpDateReported.Value.Date);
                    cmd.Parameters.AddWithValue("@rid", _residentId);
                    cmd.Parameters.AddWithValue("@it", cboIncidentType.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@desc", txtDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@loc", txtLocation.Text.Trim());
                    cmd.Parameters.AddWithValue("@sv", cboSeverity.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@st", st);
                    cmd.Parameters.AddWithValue("@at", txtAssignedTo.Text.Trim());
                    cmd.Parameters.AddWithValue("@rdate", (st == "Resolved" || st == "Closed") ? (object)dtpResolvedDate.Value.Date : DBNull.Value);
                    cmd.Parameters.AddWithValue("@rem", txtRemarks.Text.Trim());
                    if (cmd.ExecuteNonQuery() > 0)
                    { MessageBox.Show("✅  Incident report added!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); this.DialogResult = DialogResult.OK; this.Close(); }
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void UpdateIncident()
        {
            if (MessageBox.Show("Save changes?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;
            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn == null) return;
                try
                {
                    string st = cboStatus.SelectedItem.ToString();
                    var cmd = new SQLiteCommand(@"UPDATE incidentreports
                        SET DateReported=@dr,IncidentType=@it,Description=@desc,Location=@loc,
                            Severity=@sv,Status=@st,AssignedTo=@at,ResolvedDate=@rdate,Remarks=@rem
                        WHERE idincidentreports=@id", conn);
                    cmd.Parameters.AddWithValue("@dr", dtpDateReported.Value.Date);
                    cmd.Parameters.AddWithValue("@it", cboIncidentType.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@desc", txtDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@loc", txtLocation.Text.Trim());
                    cmd.Parameters.AddWithValue("@sv", cboSeverity.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@st", st);
                    cmd.Parameters.AddWithValue("@at", txtAssignedTo.Text.Trim());
                    cmd.Parameters.AddWithValue("@rdate", (st == "Resolved" || st == "Closed") ? (object)dtpResolvedDate.Value.Date : DBNull.Value);
                    cmd.Parameters.AddWithValue("@rem", txtRemarks.Text.Trim());
                    cmd.Parameters.AddWithValue("@id", _incidentId);
                    if (cmd.ExecuteNonQuery() > 0)
                    { MessageBox.Show("✅  Incident report updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); this.DialogResult = DialogResult.OK; this.Close(); }
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (_incidentId == 0) GenerateReportNumber();
            dtpDateReported.Value = DateTime.Now; cboIncidentType.SelectedIndex = -1;
            txtLocation.Clear(); cboSeverity.SelectedIndex = -1; txtDescription.Clear();
            cboStatus.SelectedIndex = 0; txtAssignedTo.Clear();
            dtpResolvedDate.Value = DateTime.Now; txtRemarks.Clear();
        }

        private void btnClose_Click(object sender, EventArgs e) => this.Close();

        private void label2_Click(object sender, EventArgs e) { }
        private void label11_Click(object sender, EventArgs e) { }
        private void button1_Click(object sender, EventArgs e) { }
        private void panelTop_Paint(object sender, System.Windows.Forms.PaintEventArgs e) { }
        private void panel1_Paint(object sender, System.Windows.Forms.PaintEventArgs e) { }
        private void txtReportNumber_TextChanged(object sender, EventArgs e) { }
        private void dtpDateReported_ValueChanged(object sender, EventArgs e) { }
        private void cboIncidentType_SelectedIndexChanged(object sender, EventArgs e) { }
        private void cboSeverity_SelectedIndexChanged(object sender, EventArgs e) { }
        private void txtDescription_TextChanged(object sender, EventArgs e) { }
        private void cboStatus_SelectedIndexChanged(object sender, EventArgs e) { }
        private void txtAssignedTo_TextChanged(object sender, EventArgs e) { }
        private void dtpResolvedDate_ValueChanged(object sender, EventArgs e) { }
        private void txtRemarks_TextChanged(object sender, EventArgs e) { }
        private void textBox2_TextChanged(object sender, EventArgs e) { }
    }
}