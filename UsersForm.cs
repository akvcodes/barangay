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
    public partial class UsersForm : Form
    {
        private int selectedUserId = 0;
        private bool isEditMode = false; // Track if we're editing an existing user

        public UsersForm()
        {
            InitializeComponent();
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
        }

        private void UsersForm_Load(object sender, EventArgs e)
        {
            LoadUsers();
            cboStatus.SelectedIndex = 0; // Default to "Active"
            ApplyHeaderStyle();
        }

        private void LoadUsers()
        {
            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn != null)
                {
                    try
                    {
                        // Don't show passwords in the grid
                        string query = "SELECT idUsers, Username, Role, IsActive, CreatedAt FROM Users ORDER BY Username";
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, conn);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        dgvUsers.DataSource = dt;

                        // Rename column headers for display
                        dgvUsers.Columns["idUsers"].HeaderText = "ID";
                        dgvUsers.Columns["Username"].HeaderText = "Username";
                        dgvUsers.Columns["Role"].HeaderText = "Role";
                        dgvUsers.Columns["IsActive"].HeaderText = "Active";
                        dgvUsers.Columns["CreatedAt"].HeaderText = "Created Date";

                        // Hide ID column
                        dgvUsers.Columns["idUsers"].Visible = false;

                        // Format the IsActive column to show Active/Inactive
                        dgvUsers.Columns["IsActive"].DefaultCellStyle.Format = "Active;Inactive";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading users: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void ApplyHeaderStyle()
        {
            dgvUsers.EnableHeadersVisualStyles = false;
            dgvUsers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvUsers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            dgvUsers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        }

        private void ClearFields()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            txtConfirmPassword.Clear();
            cboRole.SelectedIndex = -1;
            cboStatus.SelectedIndex = 0; // Default to Active
            selectedUserId = 0;
            isEditMode = false;
            btnAdd.Enabled = true;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;

            // Reset password fields to required
            txtPassword.Enabled = true;
            txtConfirmPassword.Enabled = true;
        }

        private bool ValidateInput()
        {
            // Check Username
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Username is required!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return false;
            }

            // Check if username is at least 4 characters
            if (txtUsername.Text.Trim().Length < 4)
            {
                MessageBox.Show("Username must be at least 4 characters long!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return false;
            }

            // Check Password (only required when adding new user or if password fields are filled)
            if (!isEditMode) // Adding new user
            {
                if (string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("Password is required!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return false;
                }

                if (txtPassword.Text.Length < 6)
                {
                    MessageBox.Show("Password must be at least 6 characters long!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return false;
                }

                // Check if passwords match
                if (txtPassword.Text != txtConfirmPassword.Text)
                {
                    MessageBox.Show("Passwords do not match!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtConfirmPassword.Focus();
                    return false;
                }
            }
            else // Editing existing user
            {
                // Only validate password if user is trying to change it
                if (!string.IsNullOrWhiteSpace(txtPassword.Text) || !string.IsNullOrWhiteSpace(txtConfirmPassword.Text))
                {
                    if (txtPassword.Text.Length < 6)
                    {
                        MessageBox.Show("Password must be at least 6 characters long!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtPassword.Focus();
                        return false;
                    }

                    if (txtPassword.Text != txtConfirmPassword.Text)
                    {
                        MessageBox.Show("Passwords do not match!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtConfirmPassword.Focus();
                        return false;
                    }
                }
            }

            // Check Role
            if (cboRole.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a Role!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboRole.Focus();
                return false;
            }

            // Check Status
            if (cboStatus.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a Status!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboStatus.Focus();
                return false;
            }

            return true;
        }

        private bool UsernameExists(string username, int excludeUserId = 0)
        {
            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn != null)
                {
                    try
                    {
                        string query = "SELECT COUNT(*) FROM Users WHERE Username = @username AND idUsers != @excludeId";
                        SQLiteCommand cmd = new SQLiteCommand(query, conn);
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@excludeId", excludeUserId);

                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error checking username: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            return false;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Validate input
            if (!ValidateInput())
            {
                return;
            }

            // Check if username already exists
            if (UsernameExists(txtUsername.Text.Trim()))
            {
                MessageBox.Show("Username already exists! Please choose a different username.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn != null)
                {
                    try
                    {
                        string query = @"INSERT INTO Users (Username, Password, Role, IsActive) 
                                VALUES (@username, @password, @role, @isActive)";

                        SQLiteCommand cmd = new SQLiteCommand(query, conn);
                        cmd.Parameters.AddWithValue("@username", txtUsername.Text.Trim());
                        cmd.Parameters.AddWithValue("@password", txtPassword.Text); // In production, you should hash this!
                        cmd.Parameters.AddWithValue("@role", cboRole.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@isActive", cboStatus.SelectedItem.ToString() == "Active" ? 1 : 0);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("User added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadUsers();
                            ClearFields();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error adding user: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedUserId == 0)
            {
                MessageBox.Show("Please select a user from the list to update!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate input
            if (!ValidateInput())
            {
                return;
            }

            // Check if username already exists (excluding current user)
            if (UsernameExists(txtUsername.Text.Trim(), selectedUserId))
            {
                MessageBox.Show("Username already exists! Please choose a different username.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            DialogResult confirmResult = MessageBox.Show(
                "Are you sure you want to update this user's information?",
                "Confirm Update",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmResult == DialogResult.No)
            {
                return;
            }

            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn != null)
                {
                    try
                    {
                        string query;
                        SQLiteCommand cmd;

                        // Check if password is being changed
                        if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                        {
                            // Update with new password
                            query = @"UPDATE Users 
                                    SET Username = @username, 
                                        Password = @password, 
                                        Role = @role, 
                                        IsActive = @isActive 
                                    WHERE idUsers = @id";
                            cmd = new SQLiteCommand(query, conn);
                            cmd.Parameters.AddWithValue("@password", txtPassword.Text);
                        }
                        else
                        {
                            // Update without changing password
                            query = @"UPDATE Users 
                                    SET Username = @username, 
                                        Role = @role, 
                                        IsActive = @isActive 
                                    WHERE idUsers = @id";
                            cmd = new SQLiteCommand(query, conn);
                        }

                        cmd.Parameters.AddWithValue("@username", txtUsername.Text.Trim());
                        cmd.Parameters.AddWithValue("@role", cboRole.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@isActive", cboStatus.SelectedItem.ToString() == "Active" ? 1 : 0);
                        cmd.Parameters.AddWithValue("@id", selectedUserId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("User updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadUsers();
                            ClearFields();
                        }
                        else
                        {
                            MessageBox.Show("Update failed. User may have been deleted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error updating user: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedUserId == 0)
            {
                MessageBox.Show("Please select a user from the list to delete!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Don't allow deleting yourself or the admin account
            if (selectedUserId == 1) // Assuming ID 1 is the main admin
            {
                MessageBox.Show("Cannot delete the main administrator account!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Are you sure you want to PERMANENTLY delete this user?\n\nThis action CANNOT be undone!",
                "Confirm Permanent Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                using (SQLiteConnection conn = DatabaseConnection.GetConnection())
                {
                    if (conn != null)
                    {
                        try
                        {
                            // PERMANENT DELETE
                            string query = "DELETE FROM Users WHERE idUsers = @id";

                            SQLiteCommand cmd = new SQLiteCommand(query, conn);
                            cmd.Parameters.AddWithValue("@id", selectedUserId);

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("User permanently deleted from database!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadUsers();
                                ClearFields();
                            }
                            else
                            {
                                MessageBox.Show("Delete failed. User may not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error deleting user: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void dgvUsers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                try
                {
                    DataGridViewRow row = dgvUsers.Rows[e.RowIndex];

                    // Fill the form fields with the selected row data
                    selectedUserId = Convert.ToInt32(row.Cells["idUsers"].Value);
                    txtUsername.Text = row.Cells["Username"].Value?.ToString() ?? "";

                    // Load role from database
                    using (SQLiteConnection conn = DatabaseConnection.GetConnection())
                    {
                        if (conn != null)
                        {
                            string query = "SELECT Role, IsActive FROM Users WHERE idUsers = @id";
                            SQLiteCommand cmd = new SQLiteCommand(query, conn);
                            cmd.Parameters.AddWithValue("@id", selectedUserId);
                            SQLiteDataReader reader = cmd.ExecuteReader();

                            if (reader.Read())
                            {
                                cboRole.SelectedItem = reader["Role"].ToString();
                                int isActive = Convert.ToInt32(reader["IsActive"]);
                                cboStatus.SelectedItem = isActive == 1 ? "Active" : "Inactive";
                            }
                            reader.Close();
                        }
                    }

                    // Clear password fields when editing (optional: leave blank to keep same password)
                    txtPassword.Clear();
                    txtConfirmPassword.Clear();

                    // Set edit mode
                    isEditMode = true;

                    // Enable Update and Delete buttons
                    btnAdd.Enabled = false;
                    btnUpdate.Enabled = true;
                    btnDelete.Enabled = true;

                    // Show message about password
                    MessageBox.Show("Leave password fields blank to keep the current password, or enter a new password to change it.",
                        "Edit User", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading user data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchText = txtSearch.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                LoadUsers();
                return;
            }

            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn != null)
                {
                    try
                    {
                        string query = @"SELECT idUsers, Username, Role, IsActive, CreatedAt 
                                FROM Users 
                                WHERE Username LIKE @search 
                                    OR Role LIKE @search
                                ORDER BY Username";

                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, conn);
                        adapter.SelectCommand.Parameters.AddWithValue("@search", "%" + searchText + "%");

                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        dgvUsers.DataSource = dt;

                        // Rename column headers
                        dgvUsers.Columns["idUsers"].Visible = false;
                        dgvUsers.Columns["Username"].HeaderText = "Username";
                        dgvUsers.Columns["Role"].HeaderText = "Role";
                        dgvUsers.Columns["IsActive"].HeaderText = "Active";
                        dgvUsers.Columns["CreatedAt"].HeaderText = "Created Date";

                        if (dt.Rows.Count == 0)
                        {
                            MessageBox.Show("No users found matching your search.", "Search Results", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error searching users: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void panelTop_Paint(object sender, PaintEventArgs e)
        {

        }
    }
    }
