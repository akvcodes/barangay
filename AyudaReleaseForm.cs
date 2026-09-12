using System.Data.SQLite;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BarangayManagementSystem
{
    public partial class QuickAyudaReleaseForm : Form
    {
        private int _selectedResidentId = 0;
        private string _residentName = "";

        public QuickAyudaReleaseForm()
        {
            InitializeComponent();
        }

        private void QuickAyudaReleaseForm_Load(object sender, EventArgs e)
        {
            txtReleasedBy.Text = CurrentUser.Username;
            numAmount.DecimalPlaces = 2;
            numAmount.Maximum = 999999;

            cboPurpose.Items.AddRange(new string[]
            {
                "Financial Assistance",
                "Medical Assistance",
                "Food Assistance",
                "Educational Assistance",
                "Burial Assistance",
                "Disaster Relief",
                "Emergency Assistance"
            });

            txtBarcodeScanner.Focus();
            lblInstructions.Text = "SCAN RESIDENT BARCODE OR ID CARD";
            lblInstructions.ForeColor = Color.FromArgb(26, 35, 126);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  BARCODE SCANNER INPUT
        //  Physical barcode scanners type the value then press Enter
        // ══════════════════════════════════════════════════════════════════════
        private void txtBarcodeScanner_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                string barcode = txtBarcodeScanner.Text.Trim();
                if (!string.IsNullOrEmpty(barcode))
                    ProcessBarcode(barcode);
                txtBarcodeScanner.Clear();
            }
        }

        private void ProcessBarcode(string barcode)
        {
            // Try parsing as resident ID number
            if (int.TryParse(barcode, out int residentId))
            {
                LoadResidentById(residentId);
            }
            else
            {
                MessageBox.Show(
                    $"Invalid barcode: {barcode}\n\nPlease scan a valid resident barcode.",
                    "Invalid Barcode", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBarcodeScanner.Clear();
                txtBarcodeScanner.Focus();
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  LOAD RESIDENT
        // ══════════════════════════════════════════════════════════════════════
        private void LoadResidentById(int residentId)
        {
            SQLiteConnection conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    string query = @"
                        SELECT idResident,
                               CONCAT(FirstName, ' ',
                                   CASE WHEN MiddleName IS NOT NULL AND MiddleName != ''
                                        THEN CONCAT(MiddleName, ' ') ELSE '' END,
                                   LastName) AS FullName,
                               Address,
                               ContactNumber
                        FROM resident
                        WHERE idResident = @id AND IsActive = 1";

                    SQLiteCommand cmd = new SQLiteCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", residentId);

                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            _selectedResidentId = Convert.ToInt32(reader["idResident"]);
                            _residentName = reader["FullName"].ToString().Trim();

                            lblResidentName.Text = _residentName;
                            lblResidentName.ForeColor = Color.FromArgb(15, 100, 60);
                            lblResidentAddress.Text = reader["Address"]?.ToString() ?? "";
                            lblResidentContact.Text = reader["ContactNumber"]?.ToString() ?? "";

                            btnRelease.Enabled = true;
                            numAmount.Focus();

                            System.Media.SystemSounds.Beep.Play();

                            lblInstructions.Text = "✓ RESIDENT FOUND!  Enter amount and click Release.";
                            lblInstructions.ForeColor = Color.FromArgb(15, 100, 60);
                        }
                        else
                        {
                            MessageBox.Show(
                                $"Resident ID {residentId} not found or inactive.",
                                "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            ClearResidentInfo();
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
        private void btnRelease_Click(object sender, EventArgs e)
        {
            if (_selectedResidentId == 0)
            {
                MessageBox.Show("Please scan a resident barcode first!",
                    "No Resident", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBarcodeScanner.Focus();
                return;
            }

            if (numAmount.Value <= 0)
            {
                MessageBox.Show("Please enter the amount!",
                    "Amount Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numAmount.Focus();
                return;
            }

            if (cboPurpose.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a purpose!",
                    "Purpose Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboPurpose.Focus();
                return;
            }

            // Confirm before releasing
            DialogResult confirm = MessageBox.Show(
                $"Release ₱{numAmount.Value:N2} to:\n\n" +
                $"Resident : {_residentName}\n" +
                $"Purpose  : {cboPurpose.Text}\n" +
                $"Released By: {txtReleasedBy.Text}\n\n" +
                "Confirm release?",
                "Confirm Ayuda Release",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.No) return;

            SQLiteConnection conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    string query = @"
                        INSERT INTO ayuda
                            (ResidentID, BarcodeID, Amount, DateReleased,
                             Purpose, ReleasedBy, Status, Remarks)
                        VALUES
                            (@resId, @barcode, @amount, @date,
                             @purpose, @relBy, 'Released', 'Quick release via scanner')";

                    SQLiteCommand cmd = new SQLiteCommand(query, conn);
                    cmd.Parameters.AddWithValue("@resId", _selectedResidentId);
                    cmd.Parameters.AddWithValue("@barcode", _selectedResidentId.ToString());
                    cmd.Parameters.AddWithValue("@amount", numAmount.Value);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now.Date);
                    cmd.Parameters.AddWithValue("@purpose", cboPurpose.Text);
                    cmd.Parameters.AddWithValue("@relBy", txtReleasedBy.Text.Trim());

                    cmd.ExecuteNonQuery();

                    MessageBox.Show(
                        $"✅  ₱{numAmount.Value:N2} successfully released to {_residentName}!",
                        "Release Successful",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error releasing ayuda: " + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  CLEAR
        // ══════════════════════════════════════════════════════════════════════
        private void ClearForm()
        {
            ClearResidentInfo();
            numAmount.Value = 0;
            cboPurpose.SelectedIndex = -1;
            txtBarcodeScanner.Clear();
            txtBarcodeScanner.Focus();
            lblInstructions.Text = "SCAN NEXT RESIDENT BARCODE";
            lblInstructions.ForeColor = Color.FromArgb(26, 35, 126);
        }

        private void ClearResidentInfo()
        {
            _selectedResidentId = 0;
            _residentName = "";
            lblResidentName.Text = "No resident scanned yet";
            lblResidentName.ForeColor = Color.FromArgb(108, 114, 140);
            lblResidentAddress.Text = "";
            lblResidentContact.Text = "";
            btnRelease.Enabled = false;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ── Empty stubs ───────────────────────────────────────────────────────
        private void QuickAyudaReleaseForm_Load_1(object sender, EventArgs e) { }
    }
}