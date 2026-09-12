using System.Data.SQLite;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BarangayManagementSystem
{
    public partial class ClearanceForm : Form
    {
        private int _residentId;
        private string _residentName;
        private int _clearanceId;

        public ClearanceForm(int residentId, string residentName)
        {
            InitializeComponent();
            _residentId = residentId;
            _residentName = residentName;
            _clearanceId = 0;
        }

        public ClearanceForm(int residentId, string residentName, int clearanceId)
        {
            InitializeComponent();
            _residentId = residentId;
            _residentName = residentName;
            _clearanceId = clearanceId;
        }

        private void ClearanceForm_Load(object sender, EventArgs e)
        {
            bool isEdit = _clearanceId > 0;
            // ── Theme + header ───────────────────────────────────────────────
            FormStyler.ApplyTheme(this);
            FormStyler.StyleHeader(
                headerPanel: panelTop,
                titleLabel: lblTitle,
                titleText: isEdit ? "Edit Clearance" : "New Clearance",
                subtitleText: isEdit ? "Update an existing clearance record"
                                     : "Issue a new clearance for this resident",
                subtitleLabel: lblTitle, // ← null if you don't have this label
                closeButton: btnClose
            );
            FormStyler.CenterPanel(panelInputs);
            FormStyler.SetReadOnly(txtResidentName, _residentName);

            dtpDateIssued.Value = DateTime.Now;
            dtpValidUntil.Value = DateTime.Now.AddMonths(6);
            txtIssuedBy.Text = CurrentUser.Username;
            cboStatus.SelectedIndex = 0;

            if (isEdit)
            {
                btnAdd.Text = "💾 Save Changes";
                FormStyler.ApplyButtonStyle(btnAdd, FormStyler.SuccessGreen, Color.FromArgb(15, 130, 55));
                txtClearanceNumber.ReadOnly = true;
                txtClearanceNumber.BackColor = FormStyler.InputDisabled;
                txtClearanceNumber.ForeColor = FormStyler.TextMuted;
                LoadClearanceData();
            }
            else
            {
                btnAdd.Text = "✚ Add Clearance";
                GenerateClearanceNumber();
            }
        }

        private void GenerateClearanceNumber()
        {
            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn == null) return;
                try
                {
                    int count = Convert.ToInt32(
                        new SQLiteCommand("SELECT COUNT(*) FROM clearance", conn).ExecuteScalar());
                    // ↑ Changed: Clearance → clearance

                    txtClearanceNumber.Text = $"CLR-{DateTime.Now.Year}-{(count + 1):D3}";
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }

        private void LoadClearanceData()
        {
            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn == null) return;
                try
                {
                    var cmd = new SQLiteCommand("SELECT * FROM clearance WHERE idClearance=@id", conn);
                    // ↑ Changed: Clearance → clearance

                    cmd.Parameters.AddWithValue("@id", _clearanceId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            txtClearanceNumber.Text = r["ClearanceNumber"].ToString();
                            dtpDateIssued.Value = Convert.ToDateTime(r["DateIssued"]);
                            string p = r["Purpose"].ToString();
                            if (cboPurpose.Items.Contains(p)) cboPurpose.SelectedItem = p;
                            txtIssuedBy.Text = r["IssuedBy"].ToString();
                            dtpValidUntil.Value = Convert.ToDateTime(r["ValidUntil"]);
                            string s = r["Status"].ToString();
                            if (cboStatus.Items.Contains(s)) cboStatus.SelectedItem = s;
                            txtRemarks.Text = r["Remarks"]?.ToString() ?? "";
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }

        private bool ValidateInput()
        {
            if (cboPurpose.SelectedIndex == -1)
            { MessageBox.Show("Please select a Purpose.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); cboPurpose.Focus(); return false; }
            if (string.IsNullOrWhiteSpace(txtIssuedBy.Text))
            { MessageBox.Show("Issued By is required.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtIssuedBy.Focus(); return false; }
            if (cboStatus.SelectedIndex == -1)
            { MessageBox.Show("Please select a Status.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); cboStatus.Focus(); return false; }
            if (dtpValidUntil.Value.Date <= dtpDateIssued.Value.Date)
            { MessageBox.Show("Valid Until must be after Date Issued.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            return true;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;
            if (_clearanceId > 0) UpdateClearance(); else AddClearance();
        }

        private void AddClearance()
        {
            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn == null) return;
                try
                {
                    var cmd = new SQLiteCommand(@"INSERT INTO clearance
                        (ClearanceNumber,ResidentID,DateIssued,Purpose,IssuedBy,ValidUntil,Status,Remarks)
                        VALUES (@num,@rid,@di,@pur,@ib,@vu,@st,@rem)", conn);
                    // ↑ Changed: Clearance → clearance

                    cmd.Parameters.AddWithValue("@num", txtClearanceNumber.Text.Trim());
                    cmd.Parameters.AddWithValue("@rid", _residentId);
                    cmd.Parameters.AddWithValue("@di", dtpDateIssued.Value.Date);
                    cmd.Parameters.AddWithValue("@pur", cboPurpose.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@ib", txtIssuedBy.Text.Trim());
                    cmd.Parameters.AddWithValue("@vu", dtpValidUntil.Value.Date);
                    cmd.Parameters.AddWithValue("@st", cboStatus.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@rem", txtRemarks.Text.Trim());

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show("✅ Clearance added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateClearance()
        {
            if (MessageBox.Show("Save changes?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn == null) return;
                try
                {
                    var cmd = new SQLiteCommand(@"UPDATE clearance
                        SET DateIssued=@di,Purpose=@pur,IssuedBy=@ib,ValidUntil=@vu,Status=@st,Remarks=@rem
                        WHERE idClearance=@id", conn);
                    // ↑ Changed: Clearance → clearance

                    cmd.Parameters.AddWithValue("@di", dtpDateIssued.Value.Date);
                    cmd.Parameters.AddWithValue("@pur", cboPurpose.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@ib", txtIssuedBy.Text.Trim());
                    cmd.Parameters.AddWithValue("@vu", dtpValidUntil.Value.Date);
                    cmd.Parameters.AddWithValue("@st", cboStatus.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@rem", txtRemarks.Text.Trim());
                    cmd.Parameters.AddWithValue("@id", _clearanceId);

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show("✅ Clearance updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (_clearanceId == 0) GenerateClearanceNumber();
            cboPurpose.SelectedIndex = -1;
            dtpDateIssued.Value = DateTime.Now;
            txtIssuedBy.Text = CurrentUser.Username;
            dtpValidUntil.Value = DateTime.Now.AddMonths(6);
            cboStatus.SelectedIndex = 0;
            txtRemarks.Clear();
        }

        private void btnClose_Click(object sender, EventArgs e) => this.Close();
    }
}