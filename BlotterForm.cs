using System.Data.SQLite;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BarangayManagementSystem
{
    public partial class BlotterForm : Form
    {
        private int _residentId;
        private string _residentName;
        private int _blotterId;

        public BlotterForm(int residentId, string residentName)
        {
            InitializeComponent();
            _residentId = residentId;
            _residentName = residentName;
            _blotterId = 0;
            SetupControls();
        }

        public BlotterForm(int residentId, string residentName, int blotterId)
        {
            InitializeComponent();
            _residentId = residentId;
            _residentName = residentName;
            _blotterId = blotterId;
            SetupControls();
        }

        private void SetupControls()
        {
            cboIncidentType.Items.AddRange(new string[]
                { "Noise Complaint", "Theft", "Assault", "Vandalism", "Dispute", "Trespassing", "Others" });
            cboStatus.Items.AddRange(new string[]
                { "Pending", "Under Investigation", "Resolved", "Dismissed" });
            txtBlotterNumber.ReadOnly = true;
            txtBlotterNumber.BackColor = FormStyler.InputDisabled;
            dtpResolvedDate.Enabled = false;
            txtResolvedBy.Enabled = false;
            txtRemarks.Enabled = false;
        }

        private void BlotterForm_Load(object sender, EventArgs e)
        {
            bool isEdit = _blotterId > 0;

            FormStyler.ApplyTheme(this);
            FormStyler.StyleHeader(
                headerPanel: panelTop,
                titleLabel: lblTitle,
                titleText: isEdit ? "Edit Blotter Record" : "New Blotter Record",
                subtitleText: isEdit ? "Update an existing blotter entry"
                                      : "File a new barangay blotter complaint",
                subtitleLabel: lblTitle,
                closeButton: btnClose
            );
            FormStyler.CenterPanel(panelInputs);
            FormStyler.SetReadOnly(txtResidentName, _residentName);
            dtpDateFiled.Value = DateTime.Now;

            if (isEdit)
            {
                btnAdd.Text = "💾  Save Changes";
                FormStyler.ApplyButtonStyle(btnAdd, FormStyler.SuccessGreen, Color.FromArgb(15, 130, 55));
                LoadBlotterData();
            }
            else
            {
                btnAdd.Text = "✚  File Blotter";
                cboStatus.SelectedIndex = 0;
                GenerateBlotterNumber();
            }

            UpdateResolvedFieldsState();
        }

        private void GenerateBlotterNumber()
            => txtBlotterNumber.Text = "BL-" + DateTime.Now.ToString("yyyyMMddHHmmss");

        private void LoadBlotterData()
        {
            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn == null) return;
                try
                {
                    var cmd = new SQLiteCommand("SELECT * FROM blotter WHERE idblotter=@id", conn);
                    cmd.Parameters.AddWithValue("@id", _blotterId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            txtBlotterNumber.Text = r["BlotterNumber"].ToString();
                            dtpDateFiled.Value = Convert.ToDateTime(r["DateFiled"]);
                            txtRespondent.Text = r["RespondentName"].ToString();
                            txtDescription.Text = r["Description"]?.ToString() ?? "";
                            string it = r["IncidentType"].ToString();
                            if (cboIncidentType.Items.Contains(it)) cboIncidentType.SelectedItem = it;
                            string s = r["Status"].ToString();
                            if (cboStatus.Items.Contains(s)) cboStatus.SelectedItem = s;
                            if (r["ResolvedDate"] != DBNull.Value)
                                dtpResolvedDate.Value = Convert.ToDateTime(r["ResolvedDate"]);
                            txtResolvedBy.Text = r["ResolvedBy"]?.ToString() ?? "";
                            txtRemarks.Text = r["Remarks"]?.ToString() ?? "";
                        }
                    }
                    UpdateResolvedFieldsState();
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }

        private void UpdateResolvedFieldsState()
        {
            bool resolved = cboStatus.Text == "Resolved";
            dtpResolvedDate.Enabled = resolved;
            txtResolvedBy.Enabled = resolved;
            txtRemarks.Enabled = resolved;
            dtpResolvedDate.BackColor = resolved ? FormStyler.InputBg : FormStyler.InputDisabled;
            txtResolvedBy.BackColor = resolved ? FormStyler.InputBg : FormStyler.InputDisabled;
            txtRemarks.BackColor = resolved ? FormStyler.InputBg : FormStyler.InputDisabled;
            if (!resolved) { txtResolvedBy.Clear(); txtRemarks.Clear(); }
        }

        private void cboStatus_SelectedIndexChanged(object sender, EventArgs e)
            => UpdateResolvedFieldsState();

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtRespondent.Text))
            { MessageBox.Show("Respondent name is required.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtRespondent.Focus(); return false; }
            if (cboIncidentType.SelectedIndex == -1)
            { MessageBox.Show("Please select an Incident Type.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); cboIncidentType.Focus(); return false; }
            if (cboStatus.SelectedIndex == -1)
            { MessageBox.Show("Please select a Status.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); cboStatus.Focus(); return false; }
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            { MessageBox.Show("Description is required.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtDescription.Focus(); return false; }
            return true;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;
            if (_blotterId > 0) UpdateBlotter(); else AddBlotter();
        }

        private void AddBlotter()
        {
            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn == null) return;
                try
                {
                    bool resolved = cboStatus.Text == "Resolved";
                    var cmd = new SQLiteCommand(@"INSERT INTO blotter
                        (BlotterNumber,DateFiled,ComplainantID,RespondentName,IncidentType,
                         Description,Status,ResolvedDate,ResolvedBy,Remarks)
                        VALUES (@num,@date,@cid,@name,@type,@desc,@status,@rdate,@rby,@rem)", conn);
                    cmd.Parameters.AddWithValue("@num", txtBlotterNumber.Text.Trim());
                    cmd.Parameters.AddWithValue("@date", dtpDateFiled.Value.Date);
                    cmd.Parameters.AddWithValue("@cid", _residentId);
                    cmd.Parameters.AddWithValue("@name", txtRespondent.Text.Trim());
                    cmd.Parameters.AddWithValue("@type", cboIncidentType.Text);
                    cmd.Parameters.AddWithValue("@desc", txtDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@status", cboStatus.Text);
                    cmd.Parameters.AddWithValue("@rdate", resolved ? (object)dtpResolvedDate.Value.Date : DBNull.Value);
                    cmd.Parameters.AddWithValue("@rby", resolved ? txtResolvedBy.Text.Trim() : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@rem", resolved ? txtRemarks.Text.Trim() : (object)DBNull.Value);
                    if (cmd.ExecuteNonQuery() > 0)
                    { MessageBox.Show("✅  Blotter record added!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); this.DialogResult = DialogResult.OK; this.Close(); }
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void UpdateBlotter()
        {
            if (MessageBox.Show("Save changes?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;
            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn == null) return;
                try
                {
                    bool resolved = cboStatus.Text == "Resolved";
                    var cmd = new SQLiteCommand(@"UPDATE blotter
                        SET DateFiled=@date,RespondentName=@name,IncidentType=@type,
                            Description=@desc,Status=@status,ResolvedDate=@rdate,ResolvedBy=@rby,Remarks=@rem
                        WHERE idblotter=@id", conn);
                    cmd.Parameters.AddWithValue("@date", dtpDateFiled.Value.Date);
                    cmd.Parameters.AddWithValue("@name", txtRespondent.Text.Trim());
                    cmd.Parameters.AddWithValue("@type", cboIncidentType.Text);
                    cmd.Parameters.AddWithValue("@desc", txtDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@status", cboStatus.Text);
                    cmd.Parameters.AddWithValue("@rdate", resolved ? (object)dtpResolvedDate.Value.Date : DBNull.Value);
                    cmd.Parameters.AddWithValue("@rby", resolved ? txtResolvedBy.Text.Trim() : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@rem", resolved ? txtRemarks.Text.Trim() : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", _blotterId);
                    if (cmd.ExecuteNonQuery() > 0)
                    { MessageBox.Show("✅  Blotter record updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); this.DialogResult = DialogResult.OK; this.Close(); }
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (_blotterId == 0) GenerateBlotterNumber();
            txtRespondent.Clear(); txtDescription.Clear();
            cboIncidentType.SelectedIndex = -1; cboStatus.SelectedIndex = 0;
            dtpDateFiled.Value = DateTime.Now;
            UpdateResolvedFieldsState();
        }

        private void btnClose_Click(object sender, EventArgs e) => this.Close();

        private void panelTop_Paint(object sender, System.Windows.Forms.PaintEventArgs e) { }
        private void panelInputs_Paint(object sender, System.Windows.Forms.PaintEventArgs e) { }
        private void panel1_Paint(object sender, System.Windows.Forms.PaintEventArgs e) { }
        private void lblTitle_Click(object sender, EventArgs e) { }
        private void label8_Click(object sender, EventArgs e) { }
        private void label11_Click(object sender, EventArgs e) { }
        private void txtRemarks_TextChanged(object sender, EventArgs e) { }
    }
}