using System.Data.SQLite;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BarangayManagementSystem
{
    public partial class AyudaForm : Form
    {
        private int residentId;
        private string residentName;

        public AyudaForm(int selectedResidentId)
        {
            InitializeComponent();

            this.residentId = selectedResidentId;

            cboPurpose.Items.AddRange(new string[]
            {
                "Financial Assistance",
                "Medical Assistance",
                "Food Assistance",
                "Educational Assistance",
                "Burial Assistance",
                "Disaster Relief",
                "Senior Citizen Assistance",
                "PWD Assistance",
                "Emergency Assistance",
                "Others"
            });

            cboStatus.Items.AddRange(new string[]
            {
                "Released",
                "Pending",
                "Claimed"
            });
        }

        // ══════════════════════════════════════════════════════════════════════
        //  FORM LOAD
        // ══════════════════════════════════════════════════════════════════════
        private void AyudaForm_Load(object sender, EventArgs e)
        {
            LoadResidentInfo();
            dtpDateReleased.Value = DateTime.Now;
            numAmount.Maximum = 999999;
            numAmount.DecimalPlaces = 2;
            cboStatus.SelectedIndex = 0;
            txtReleasedBy.Text = CurrentUser.Username;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  LOAD RESIDENT INFO
        // ══════════════════════════════════════════════════════════════════════
        private void LoadResidentInfo()
        {
            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn == null) return;
                try
                {
                    string query = @"
                        SELECT CONCAT(FirstName, ' ',
                                   CASE WHEN MiddleName IS NOT NULL AND MiddleName != ''
                                        THEN CONCAT(MiddleName, ' ') ELSE '' END,
                                   LastName) AS FullName,
                               Address, ContactNumber
                        FROM resident
                        WHERE idResident = @id";

                    SQLiteCommand cmd = new SQLiteCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", residentId);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            residentName = reader["FullName"].ToString().Trim();
                            string address = reader["Address"]?.ToString() ?? "";
                            string contact = reader["ContactNumber"]?.ToString() ?? "";

                            lblResidentName.Text = $"Resident: {residentName}";
                            lblResidentAddress.Text = $"Address: {address}";
                            lblResidentContact.Text = $"Contact: {contact}";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading resident: " + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  RELEASE AYUDA
        // ══════════════════════════════════════════════════════════════════════
        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Validation
            if (numAmount.Value <= 0)
            {
                MessageBox.Show("Please enter amount!", "Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numAmount.Focus();
                return;
            }

            if (cboPurpose.SelectedIndex == -1 &&
                string.IsNullOrWhiteSpace(cboPurpose.Text))
            {
                MessageBox.Show("Please enter or select a purpose!", "Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboPurpose.Focus();
                return;
            }

            if (cboStatus.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a status!", "Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboStatus.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtReleasedBy.Text))
            {
                MessageBox.Show("Please enter 'Released By'!", "Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtReleasedBy.Focus();
                return;
            }

            // Confirm
            if (MessageBox.Show(
                $"Release assistance of ₱{numAmount.Value:N2} to {residentName}?\n\n" +
                $"Purpose  : {cboPurpose.Text}\n" +
                $"Status   : {cboStatus.Text}\n" +
                $"Released By: {txtReleasedBy.Text}",
                "Confirm Release",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.No) return;

            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn == null) return;
                try
                {
                    string query = @"
                        INSERT INTO ayuda
                            (ResidentID, BarcodeID, Amount, DateReleased,
                             Purpose, ReleasedBy, Status, Remarks)
                        VALUES
                            (@resId, @barcode, @amount, @dateReleased,
                             @purpose, @releasedBy, @status, @remarks)";

                    SQLiteCommand cmd = new SQLiteCommand(query, conn);
                    cmd.Parameters.AddWithValue("@resId", residentId);
                    cmd.Parameters.AddWithValue("@barcode", txtBarcodeID.Text.Trim());
                    cmd.Parameters.AddWithValue("@amount", numAmount.Value);
                    cmd.Parameters.AddWithValue("@dateReleased", dtpDateReleased.Value.Date);
                    cmd.Parameters.AddWithValue("@purpose", cboPurpose.Text.Trim());
                    cmd.Parameters.AddWithValue("@releasedBy", txtReleasedBy.Text.Trim());
                    cmd.Parameters.AddWithValue("@status", cboStatus.Text);
                    cmd.Parameters.AddWithValue("@remarks", txtRemarks.Text.Trim());

                    cmd.ExecuteNonQuery();

                    MessageBox.Show(
                        $"✅  ₱{numAmount.Value:N2} assistance released to {residentName}!",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error releasing assistance: " + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  CLEAR
        // ══════════════════════════════════════════════════════════════════════
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtBarcodeID.Clear();
            numAmount.Value = 0;
            cboPurpose.SelectedIndex = -1;
            txtRemarks.Clear();
            dtpDateReleased.Value = DateTime.Now;
            cboStatus.SelectedIndex = 0;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  CLOSE
        // ══════════════════════════════════════════════════════════════════════
        private void btnClose_Click(object sender, EventArgs e) => this.Close();

        // ── Empty designer stubs ──────────────────────────────────────────────
        private void panelTop_Paint(object sender, PaintEventArgs e) { }
        private void panelMain_Paint(object sender, PaintEventArgs e) { }
    }
}