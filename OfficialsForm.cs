using System.Data.SQLite;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace BarangayManagementSystem
{
    public partial class OfficialsForm : Form
    {
        private int selectedOfficialId = 0;

        public OfficialsForm()
        {
            InitializeComponent();
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
        }

        private void btnClose_Click(object sender, EventArgs e) => this.Close();

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

        private void OfficialsForm_Load(object sender, EventArgs e)
        {
            LoadOfficials();
            ApplyHeaderStyle();
        }

        private void LoadOfficials()
        {
            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn != null)
                {
                    try
                    {
                        string query = @"SELECT idOfficials, FirstName, LastName, Position, 
                                         ContactNumber, Email FROM officials 
                                         WHERE IsActive = 1 
                                         ORDER BY Position, LastName, FirstName";

                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, conn);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvOfficials.DataSource = dt;

                        dgvOfficials.Columns["idOfficials"].Visible = false;
                        dgvOfficials.Columns["idOfficials"].HeaderText = "ID";
                        dgvOfficials.Columns["FirstName"].HeaderText = "First Name";
                        dgvOfficials.Columns["LastName"].HeaderText = "Last Name";
                        dgvOfficials.Columns["Position"].HeaderText = "Position";
                        dgvOfficials.Columns["ContactNumber"].HeaderText = "Contact";
                        dgvOfficials.Columns["Email"].HeaderText = "Email";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading officials: " + ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ApplyHeaderStyle()
        {
            dgvOfficials.EnableHeadersVisualStyles = false;
            dgvOfficials.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvOfficials.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            dgvOfficials.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        }

        private void ClearFields()
        {
            txtFirstName.Clear();
            txtLastName.Clear();
            cboPosition.Text = "";
            txtContactNumber.Clear();
            txtEmail.Clear();
            selectedOfficialId = 0;
            btnAdd.Enabled = true;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
        }

        private void btnClear_Click(object sender, EventArgs e) => ClearFields();

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            { MessageBox.Show("First Name is required!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtFirstName.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            { MessageBox.Show("Last Name is required!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtLastName.Focus(); return; }
            if (string.IsNullOrWhiteSpace(cboPosition.Text))
            { MessageBox.Show("Please select a Position!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); cboPosition.Focus(); return; }

            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn != null)
                {
                    try
                    {
                        string query = @"INSERT INTO officials 
                                        (FirstName, LastName, Position, ContactNumber, Email, IsActive)
                                        VALUES 
                                        (@firstName, @lastName, @position, @contact, @email, 1)";

                        SQLiteCommand cmd = new SQLiteCommand(query, conn);
                        cmd.Parameters.AddWithValue("@firstName", txtFirstName.Text.Trim());
                        cmd.Parameters.AddWithValue("@lastName", txtLastName.Text.Trim());
                        cmd.Parameters.AddWithValue("@position", cboPosition.Text.Trim());
                        cmd.Parameters.AddWithValue("@contact", txtContactNumber.Text.Trim());
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());

                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            MessageBox.Show("Official added successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadOfficials();
                            ClearFields();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error adding official: " + ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedOfficialId == 0)
            { MessageBox.Show("No official selected for update!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            { MessageBox.Show("First Name is required!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtFirstName.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            { MessageBox.Show("Last Name is required!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtLastName.Focus(); return; }
            if (string.IsNullOrWhiteSpace(cboPosition.Text))
            { MessageBox.Show("Please select a Position!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); cboPosition.Focus(); return; }

            if (MessageBox.Show("Are you sure you want to update this official's information?",
                "Confirm Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn != null)
                {
                    try
                    {
                        string query = @"UPDATE officials
                                         SET FirstName = @firstName,
                                             LastName = @lastName,
                                             Position = @position,
                                             ContactNumber = @contact,
                                             Email = @email
                                         WHERE idOfficials = @id";

                        SQLiteCommand cmd = new SQLiteCommand(query, conn);
                        cmd.Parameters.AddWithValue("@firstName", txtFirstName.Text.Trim());
                        cmd.Parameters.AddWithValue("@lastName", txtLastName.Text.Trim());
                        cmd.Parameters.AddWithValue("@position", cboPosition.Text.Trim());
                        cmd.Parameters.AddWithValue("@contact", txtContactNumber.Text.Trim());
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@id", selectedOfficialId);

                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            MessageBox.Show("Official updated successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadOfficials();
                            ClearFields();
                        }
                        else
                        {
                            MessageBox.Show("Update failed. No rows affected.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error updating official: " + ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void dgvOfficials_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvOfficials.Rows[e.RowIndex];
                selectedOfficialId = Convert.ToInt32(row.Cells["idOfficials"].Value);

                txtFirstName.Text = row.Cells["FirstName"].Value.ToString();
                txtLastName.Text = row.Cells["LastName"].Value.ToString();
                cboPosition.Text = row.Cells["Position"].Value.ToString();
                txtContactNumber.Text = row.Cells["ContactNumber"].Value?.ToString() ?? "";
                txtEmail.Text = row.Cells["Email"].Value?.ToString() ?? "";

                btnAdd.Enabled = false;
                btnUpdate.Enabled = true;
                btnDelete.Enabled = true;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(searchText)) { LoadOfficials(); return; }

            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn != null)
                {
                    try
                    {
                        string query = @"SELECT idOfficials, FirstName, LastName, Position, 
                                         ContactNumber, Email
                                         FROM officials
                                         WHERE IsActive = 1
                                         AND (FirstName LIKE @search
                                              OR LastName LIKE @search
                                              OR Position LIKE @search
                                              OR ContactNumber LIKE @search
                                              OR Email LIKE @search)
                                         ORDER BY Position, LastName, FirstName";

                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, conn);
                        adapter.SelectCommand.Parameters.AddWithValue("@search", "%" + searchText + "%");
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvOfficials.DataSource = dt;

                        dgvOfficials.Columns["idOfficials"].Visible = false;
                        dgvOfficials.Columns["FirstName"].HeaderText = "First Name";
                        dgvOfficials.Columns["LastName"].HeaderText = "Last Name";
                        dgvOfficials.Columns["Position"].HeaderText = "Position";
                        dgvOfficials.Columns["ContactNumber"].HeaderText = "Contact";
                        dgvOfficials.Columns["Email"].HeaderText = "Email";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error searching officials: " + ex.Message, "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedOfficialId == 0)
            { MessageBox.Show("Please select an official to delete!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (MessageBox.Show("Are you sure you want to PERMANENTLY delete this official?\n\nThis action CANNOT be undone!",
                "Confirm Permanent Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (SQLiteConnection conn = DatabaseConnection.GetConnection())
                {
                    if (conn != null)
                    {
                        try
                        {
                            string query = "DELETE FROM officials WHERE idOfficials = @id";

                            SQLiteCommand cmd = new SQLiteCommand(query, conn);
                            cmd.Parameters.AddWithValue("@id", selectedOfficialId);

                            if (cmd.ExecuteNonQuery() > 0)
                            {
                                MessageBox.Show("Official permanently deleted!", "Success",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadOfficials();
                                ClearFields();
                            }
                            else
                            {
                                MessageBox.Show("Delete failed. Official may not exist.", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error deleting official: " + ex.Message, "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void dgvOfficials_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void panelTop_Paint(object sender, PaintEventArgs e) { }
        private void panelInputs_Paint(object sender, PaintEventArgs e) { }
        private void txtContactNumber_TextChanged(object sender, EventArgs e) { }
        private void cboPosition_SelectedIndexChanged(object sender, EventArgs e) { }
        private void txtLastName_TextChanged(object sender, EventArgs e) { }
        private void txtFirstName_TextChanged(object sender, EventArgs e) { }
        private void txtSearch_TextChanged(object sender, EventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
    }
}