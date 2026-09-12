using System.Data.SQLite;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BarangayManagementSystem
{
    public partial class CertificateForm : Form
    {
        private int _residentId;
        private string _residentName;
        private int _certificateId;

        public CertificateForm(int residentId, string residentName)
        {
            InitializeComponent();
            _residentId = residentId;
            _residentName = residentName;
            _certificateId = 0;
        }

        public CertificateForm(int residentId, string residentName, int certificateId)
        {
            InitializeComponent();
            _residentId = residentId;
            _residentName = residentName;
            _certificateId = certificateId;
        }

        private void CertificateForm_Load(object sender, EventArgs e)
        {
            bool isEdit = _certificateId > 0;

            FormStyler.ApplyTheme(this);
            FormStyler.StyleHeader(
                headerPanel: panelTop,
                titleLabel: lblTitle,
                titleText: isEdit ? "Edit Certificate" : "New Certificate",
                subtitleText: isEdit ? "Update an existing certificate record"
                                      : "Issue a new certificate for this resident",
                subtitleLabel: lblTitle,
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
                btnAdd.Text = "💾  Save Changes";
                FormStyler.ApplyButtonStyle(btnAdd, FormStyler.SuccessGreen, Color.FromArgb(15, 130, 55));
                txtCertificateNumber.ReadOnly = true;
                txtCertificateNumber.BackColor = FormStyler.InputDisabled;
                txtCertificateNumber.ForeColor = FormStyler.TextMuted;
                LoadCertificateData();
            }
            else
            {
                btnAdd.Text = "✚  Add Certificate";
                GenerateCertificateNumber();
            }
        }

        private void GenerateCertificateNumber()
        {
            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn == null) return;
                try
                {
                    int count = Convert.ToInt32(new SQLiteCommand("SELECT COUNT(*) FROM certificates", conn).ExecuteScalar());
                    txtCertificateNumber.Text = $"CERT-{DateTime.Now.Year}-{(count + 1):D3}";
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }

        private void LoadCertificateData()
        {
            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn == null) return;
                try
                {
                    var cmd = new SQLiteCommand("SELECT * FROM certificates WHERE idcertificates=@id", conn);
                    cmd.Parameters.AddWithValue("@id", _certificateId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            txtCertificateNumber.Text = r["CertificateNumber"].ToString();
                            dtpDateIssued.Value = Convert.ToDateTime(r["DateIssued"]);
                            string ct = r["CertificateType"].ToString();
                            if (cboCertificateType.Items.Contains(ct)) cboCertificateType.SelectedItem = ct;
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
            if (cboCertificateType.SelectedIndex == -1)
            { MessageBox.Show("Please select a Certificate Type.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); cboCertificateType.Focus(); return false; }
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
            if (_certificateId > 0) UpdateCertificate(); else AddCertificate();
        }

        private void AddCertificate()
        {
            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn == null) return;
                try
                {
                    var cmd = new SQLiteCommand(@"INSERT INTO certificates
                        (CertificateNumber,ResidentID,CertificateType,DateIssued,IssuedBy,ValidUntil,Status,Remarks)
                        VALUES (@num,@rid,@ct,@di,@ib,@vu,@st,@rem)", conn);
                    cmd.Parameters.AddWithValue("@num", txtCertificateNumber.Text.Trim());
                    cmd.Parameters.AddWithValue("@rid", _residentId);
                    cmd.Parameters.AddWithValue("@ct", cboCertificateType.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@di", dtpDateIssued.Value.Date);
                    cmd.Parameters.AddWithValue("@ib", txtIssuedBy.Text.Trim());
                    cmd.Parameters.AddWithValue("@vu", dtpValidUntil.Value.Date);
                    cmd.Parameters.AddWithValue("@st", cboStatus.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@rem", txtRemarks.Text.Trim());
                    if (cmd.ExecuteNonQuery() > 0)
                    { MessageBox.Show("✅  Certificate added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); this.DialogResult = DialogResult.OK; this.Close(); }
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void UpdateCertificate()
        {
            if (MessageBox.Show("Save changes?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;
            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn == null) return;
                try
                {
                    var cmd = new SQLiteCommand(@"UPDATE certificates
                        SET CertificateType=@ct,DateIssued=@di,IssuedBy=@ib,ValidUntil=@vu,Status=@st,Remarks=@rem
                        WHERE idcertificates=@id", conn);
                    cmd.Parameters.AddWithValue("@ct", cboCertificateType.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@di", dtpDateIssued.Value.Date);
                    cmd.Parameters.AddWithValue("@ib", txtIssuedBy.Text.Trim());
                    cmd.Parameters.AddWithValue("@vu", dtpValidUntil.Value.Date);
                    cmd.Parameters.AddWithValue("@st", cboStatus.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@rem", txtRemarks.Text.Trim());
                    cmd.Parameters.AddWithValue("@id", _certificateId);
                    if (cmd.ExecuteNonQuery() > 0)
                    { MessageBox.Show("✅  Certificate updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); this.DialogResult = DialogResult.OK; this.Close(); }
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (_certificateId == 0) GenerateCertificateNumber();
            cboCertificateType.SelectedIndex = -1;
            dtpDateIssued.Value = DateTime.Now;
            txtIssuedBy.Text = CurrentUser.Username;
            dtpValidUntil.Value = DateTime.Now.AddMonths(6);
            cboStatus.SelectedIndex = 0;
            txtRemarks.Clear();
        }

        private void btnClose_Click(object sender, EventArgs e) => this.Close();
        private void label2_Click(object sender, EventArgs e) { }
    }
}