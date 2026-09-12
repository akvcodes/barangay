// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  UsersControl.cs                                                         ║
// ║                                                                          ║
// ║  SETUP:                                                                  ║
// ║  1. Add this file to your project (anywhere, or Controls folder)         ║
// ║  2. No Designer.cs needed                                                ║
// ║  3. In MainMenuForm.cs, change btnUsers_Click to load this control       ║
// ╚══════════════════════════════════════════════════════════════════════════╝

using System.Data.SQLite;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace BarangayManagementSystem.Controls
{
    public class UsersControl : UserControl
    {
        // ── State ─────────────────────────────────────────────────────────────
        private int _selectedUserId = 0;
        private bool _isEditMode = false;

        // ── Colors ────────────────────────────────────────────────────────────
        private static readonly Color NavBlue = Color.FromArgb(26, 35, 126);
        private static readonly Color BgPage = Color.FromArgb(240, 242, 248);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderCol = Color.FromArgb(220, 225, 238);
        private static readonly Color TextDark = Color.FromArgb(28, 30, 44);
        private static readonly Color TextGray = Color.FromArgb(108, 114, 140);
        private static readonly Color BtnBlue = Color.FromArgb(66, 133, 244);
        private static readonly Color BtnGreen = Color.FromArgb(15, 157, 88);
        private static readonly Color BtnRed = Color.FromArgb(192, 57, 43);
        private static readonly Color BtnGray = Color.FromArgb(100, 110, 128);
        private static readonly Color LightBlue = Color.FromArgb(238, 242, 255);

        // ── LIST VIEW controls ─────────────────────────────────────────────────
        private Panel _listPanel;
        private TextBox _txtSearch;
        private DataGridView _dgvUsers;

        // ── ADD / EDIT VIEW controls ───────────────────────────────────────────
        private Panel _formPanel;
        private Label _lblFormTitle;
        private TextBox _txtUsername;
        private TextBox _txtPassword;
        private TextBox _txtConfirmPassword;
        private ComboBox _cboRole;
        private ComboBox _cboStatus;
        private Label _lblRoleInfo;
        private Button _btnSave;
        private Panel _topBar;

        // ══════════════════════════════════════════════════════════════════════
        public UsersControl()
        {
            this.DoubleBuffered = true;
            this.BackColor = BgPage;
            this.Dock = DockStyle.Fill;

            BuildListView();
            BuildFormView();
            ShowList();
        }

        public void RefreshData()
        {
            if (_listPanel != null && _listPanel.Visible) LoadUsers();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  VIEW SWITCHING
        // ══════════════════════════════════════════════════════════════════════
        private void ShowList()
        {
            _formPanel.Visible = false;
            _listPanel.Visible = true;
            _listPanel.BringToFront();
            LoadUsers();
        }

        private void ShowForm(bool editMode, int userId = 0)
        {
            _isEditMode = editMode;
            _selectedUserId = userId;

            _listPanel.Visible = false;
            _formPanel.Visible = true;
            _formPanel.BringToFront();

            if (editMode)
            {
                _lblFormTitle.Text = "✏  Edit User";
                _btnSave.Text = "💾  Save Changes";
                LoadUserForEdit(userId);
            }
            else
            {
                _lblFormTitle.Text = "➕  Add New User";
                _btnSave.Text = "💾  Save User";
                ClearForm();
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  BUILD LIST VIEW
        // ══════════════════════════════════════════════════════════════════════
        private void BuildListView()
        {
            // ── Custom top bar with back button ───────────────────────────────
            _topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = NavBlue,
            };

            var btnBack = new Button
            {
                Text = "◀   Back to Main Menu",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Size = new Size(190, 36),
                Location = new Point(12, 8),
            };
            btnBack.FlatAppearance.BorderColor = Color.FromArgb(120, 255, 255, 255);
            btnBack.FlatAppearance.BorderSize = 1;
            btnBack.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 255, 255, 255);
            btnBack.Click += (s, e) =>
            {
                Form parentForm = this.FindForm();
                parentForm?.Close();
            };
            _topBar.Controls.Add(btnBack);

            var lblTopTitle = new Label
            {
                Text = "👤   User Management",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent,
            };
                _topBar.Controls.Add(lblTopTitle);
                _topBar.Layout += (s, e) =>
            {
                lblTopTitle.Location = new Point(
                    (_topBar.Width - lblTopTitle.Width) / 2,
                    (_topBar.Height - lblTopTitle.Height) / 2);
            };

                this.Controls.Add(_topBar);

            _listPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgPage,
                Padding = new Padding(24, 20, 24, 16),
            };
            this.Controls.Add(_listPanel);

            // Page heading
            var lblTitle = new Label
            {
                Text = "User Management",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(0, 0),
                BackColor = Color.Transparent,
            };
            _listPanel.Controls.Add(lblTitle);

            var lblSub = new Label
            {
                Text = "Manage system accounts and role access",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = TextGray,
                AutoSize = true,
                Location = new Point(2, 36),
                BackColor = Color.Transparent,
            };
            _listPanel.Controls.Add(lblSub);

            // Search card
            var searchCard = MakeCard(new Point(0, 66), new Size(200, 62));
            _listPanel.Controls.Add(searchCard);

            var lblSearch = new Label
            {
                Text = "Search:",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(16, 20),
                BackColor = Color.Transparent,
            };
            searchCard.Controls.Add(lblSearch);

            _txtSearch = new TextBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(90, 16),
                Size = new Size(300, 30),
                BorderStyle = BorderStyle.FixedSingle,
            };
            searchCard.Controls.Add(_txtSearch);

            var btnSearch = MakeBtn("🔍  Search", BtnBlue, 9.5f);
            btnSearch.Location = new Point(400, 15);
            btnSearch.Size = new Size(116, 32);
            btnSearch.Click += (s, e) => SearchUsers();
            searchCard.Controls.Add(btnSearch);

            var btnAdd = MakeBtn("+ Add New User", BtnGreen, 9.5f);
            btnAdd.Location = new Point(524, 15);
            btnAdd.Size = new Size(170, 32);
            btnAdd.Click += (s, e) => ShowForm(false);
            searchCard.Controls.Add(btnAdd);

            // Users grid
            _dgvUsers = MakeGrid();
            _dgvUsers.Location = new Point(0, 140);
            _listPanel.Controls.Add(_dgvUsers);

            // Resize handler
            _listPanel.Resize += (s, e) =>
            {
                int w = _listPanel.ClientSize.Width - _listPanel.Padding.Horizontal;
                int h = _listPanel.ClientSize.Height - _listPanel.Padding.Vertical;
                if (w < 100) return;
                searchCard.Width = w;
                _dgvUsers.Width = w;
                _dgvUsers.Height = h - 148;
            };
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (_topBar != null) _topBar.Visible = !(Parent is TabPage);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  BUILD FORM VIEW  (Add / Edit)
        // ══════════════════════════════════════════════════════════════════════
        private void BuildFormView()
        {
            _formPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgPage,
                AutoScroll = true,
                Padding = new Padding(24, 20, 24, 24),
                Visible = false,
            };
            this.Controls.Add(_formPanel);

            // Back button
            var btnBack = MakeBtn("← Back to Users", BtnGray, 9.5f);
            btnBack.Location = new Point(0, 0);
            btnBack.Size = new Size(160, 34);
            btnBack.Click += (s, e) => ShowList();
            _formPanel.Controls.Add(btnBack);

            // Form card — tall enough for all fields
            var card = MakeCard(new Point(0, 50), new Size(200, 500));
            _formPanel.Controls.Add(card);

            // Title
            _lblFormTitle = new Label
            {
                Text = "➕  Add New User",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(24, 16),
                BackColor = Color.Transparent,
            };
            card.Controls.Add(_lblFormTitle);

            // Divider
            var div = new Panel
            {
                Location = new Point(0, 48),
                Height = 1,
                BackColor = BorderCol,
            };
            card.Controls.Add(div);
            card.Resize += (s, e) => div.Width = card.Width;

            // ── All fields use label-above-field pattern ──────────────────────
            const int lx = 24, rx = 400;
            const int lw = 340, rw = 220;
            const int rowH = 72;
            int y = 64;

            // ── Row 1: Username + Role ────────────────────────────────────────
            card.Controls.Add(FormLabel("Username", lx, y));
            _txtUsername = new TextBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(lx, y + 20),
                Size = new Size(lw, 30),
                BorderStyle = BorderStyle.FixedSingle,
            };
            card.Controls.Add(_txtUsername);

            card.Controls.Add(FormLabel("Role", rx, y));
            _cboRole = new ComboBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(rx, y + 20),
                Size = new Size(rw, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            _cboRole.Items.AddRange(new object[] { "SuperAdmin", "Admin", "Official", "Treasurer", "User" });
            card.Controls.Add(_cboRole);
            _cboRole.SelectedIndexChanged += (s, e) => UpdateRoleInfo();
            y += rowH;

            // ── Row 2: Password + Status ──────────────────────────────────────
            card.Controls.Add(FormLabel("Password", lx, y));
            _txtPassword = new TextBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(lx, y + 20),
                Size = new Size(lw, 30),
                BorderStyle = BorderStyle.FixedSingle,
                PasswordChar = '●',
            };
            card.Controls.Add(_txtPassword);

            card.Controls.Add(FormLabel("Status", rx, y));
            _cboStatus = new ComboBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(rx, y + 20),
                Size = new Size(rw, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            _cboStatus.Items.AddRange(new object[] { "Active", "Inactive" });
            _cboStatus.SelectedIndex = 0;
            card.Controls.Add(_cboStatus);
            y += rowH;

            // ── Row 3: Confirm Password ───────────────────────────────────────
            card.Controls.Add(FormLabel("Confirm Password", lx, y));
            _txtConfirmPassword = new TextBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(lx, y + 20),
                Size = new Size(lw, 30),
                BorderStyle = BorderStyle.FixedSingle,
                PasswordChar = '●',
            };
            card.Controls.Add(_txtConfirmPassword);
            y += rowH;

            // ── Password hint ─────────────────────────────────────────────────
            var lblHint = new Label
            {
                Text = "💡 Leave password blank to keep the current password",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = TextGray,
                AutoSize = true,
                Location = new Point(lx, y),
                BackColor = Color.Transparent,
            };
            card.Controls.Add(lblHint);
            y += 28;

            // ── Role info box ─────────────────────────────────────────────────
            _lblRoleInfo = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = NavBlue,
                AutoSize = false,
                Size = new Size(lw + 56 + rw, 44),
                Location = new Point(lx, y),
                BackColor = LightBlue,
                Padding = new Padding(10, 8, 8, 8),
                Visible = false,
            };
            card.Controls.Add(_lblRoleInfo);
            y += 54;

            // ── Buttons ───────────────────────────────────────────────────────
            _btnSave = MakeBtn("💾  Save User", BtnGreen, 10f);
            _btnSave.Location = new Point(lx, y);
            _btnSave.Size = new Size(180, 40);
            _btnSave.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            _btnSave.Click += (s, e) => SaveUser();
            card.Controls.Add(_btnSave);

            var btnCancel = MakeBtn("Cancel", BtnGray, 10f);
            btnCancel.Location = new Point(lx + 190, y);
            btnCancel.Size = new Size(110, 40);
            btnCancel.Click += (s, e) => ShowList();
            card.Controls.Add(btnCancel);

            // Resize
            _formPanel.Resize += (s, e) =>
            {
                int w = _formPanel.ClientSize.Width - _formPanel.Padding.Horizontal;
                if (w < 100) return;
                card.Width = w;
                div.Width = w;
                _lblRoleInfo.Width = Math.Min(lw + 56 + rw, w - 48);
            };
        }

        // Clean label above field
        private static Label FormLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text + ":",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 70, 100),
                AutoSize = true,
                Location = new Point(x, y),
                BackColor = Color.Transparent,
            };
        }
        // ══════════════════════════════════════════════════════════════════════
        //  ROLE INFO BOX
        // ══════════════════════════════════════════════════════════════════════
        private void UpdateRoleInfo()
        {
            if (_cboRole.SelectedItem == null) { _lblRoleInfo.Visible = false; return; }
            string role = _cboRole.SelectedItem.ToString();
            switch (role)
            {
                case "SuperAdmin":
                    _lblRoleInfo.Text = "👑 SuperAdmin — Full access including Connection Setup and Database Backup.";
                    break;
                case "Admin":
                    _lblRoleInfo.Text = "🔧 Admin — Full access except Connection Setup and Database Backup.";
                    break;
                case "Official":
                    _lblRoleInfo.Text = "🏛 Official — Dashboard, Residents, Officials, and Budget Transparency only.";
                    break;
                case "Treasurer":
                    _lblRoleInfo.Text = "💰 Treasurer — Resident Management only, for collecting Cedula tax payments.";
                    break;
                case "User":
                    _lblRoleInfo.Text = "👤 User — Dashboard, Residents, and Budget Transparency only.";
                    break;
                default:
                    _lblRoleInfo.Visible = false; return;
            }
            _lblRoleInfo.Visible = true;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  DATA — LOAD
        // ══════════════════════════════════════════════════════════════════════
        private void LoadUsers(string search = "")
        {
            SQLiteConnection conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    string where = string.IsNullOrWhiteSpace(search)
                        ? ""
                        : "WHERE Username LIKE @s OR Role LIKE @s";

                    // Convert IsActive to text in SQL to avoid checkbox rendering in DataGridView
                    string q = $@"SELECT idUsers, Username, Role,
                                  IIF(IsActive=1,'Active','Inactive') AS Status,
                                  CreatedAt
                                  FROM users {where}
                                  ORDER BY
                                    CASE Role
                                        WHEN 'SuperAdmin' THEN 1
                                        WHEN 'Admin'      THEN 2
                                        WHEN 'Official'   THEN 3
                                        WHEN 'Treasurer'  THEN 4
                                        ELSE 5
                                    END, Username";

                    var da = new SQLiteDataAdapter(q, conn);
                    if (!string.IsNullOrWhiteSpace(search))
                        da.SelectCommand.Parameters.AddWithValue("@s", "%" + search + "%");

                    var dt = new DataTable();
                    da.Fill(dt);
                    _dgvUsers.DataSource = dt;

                    if (_dgvUsers.Columns.Contains("idUsers"))
                        _dgvUsers.Columns["idUsers"].Visible = false;
                    if (_dgvUsers.Columns.Contains("Username"))
                    { _dgvUsers.Columns["Username"].HeaderText = "Username"; _dgvUsers.Columns["Username"].Width = 200; }
                    if (_dgvUsers.Columns.Contains("Role"))
                    { _dgvUsers.Columns["Role"].HeaderText = "Role"; _dgvUsers.Columns["Role"].Width = 140; }
                    if (_dgvUsers.Columns.Contains("Status"))
                    { _dgvUsers.Columns["Status"].HeaderText = "Status"; _dgvUsers.Columns["Status"].Width = 100; }
                    if (_dgvUsers.Columns.Contains("CreatedAt"))
                    { _dgvUsers.Columns["CreatedAt"].HeaderText = "Created"; _dgvUsers.Columns["CreatedAt"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; }

                    // Color-code Role column
                    _dgvUsers.CellFormatting -= ColorRoleCell;
                    _dgvUsers.CellFormatting += ColorRoleCell;

                    // Color-code Status column
                    _dgvUsers.CellFormatting -= ColorStatusCell;
                    _dgvUsers.CellFormatting += ColorStatusCell;

                    if (dt.Rows.Count == 0 && !string.IsNullOrWhiteSpace(search))
                        MessageBox.Show("No users found.", "Search",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading users:\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ColorRoleCell(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_dgvUsers.Columns.Count <= e.ColumnIndex) return;
            if (_dgvUsers.Columns[e.ColumnIndex].Name != "Role" || e.Value == null) return;
            switch (e.Value.ToString())
            {
                case "SuperAdmin":
                    e.CellStyle.ForeColor = Color.FromArgb(180, 30, 30);
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold); break;
                case "Admin":
                    e.CellStyle.ForeColor = NavBlue;
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold); break;
                case "Official":
                    e.CellStyle.ForeColor = Color.FromArgb(15, 120, 60);
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold); break;
                case "Treasurer":
                    e.CellStyle.ForeColor = Color.FromArgb(146, 64, 14);
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold); break;
                case "User":
                    e.CellStyle.ForeColor = Color.FromArgb(160, 90, 0);
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold); break;
            }
        }

        private void ColorStatusCell(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_dgvUsers.Columns.Count <= e.ColumnIndex) return;
            if (_dgvUsers.Columns[e.ColumnIndex].Name != "Status" || e.Value == null) return;
            bool active = e.Value.ToString() == "Active";
            e.CellStyle.ForeColor = active
                ? Color.FromArgb(15, 120, 60)
                : Color.FromArgb(180, 30, 30);
            e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        }

        private void LoadUserForEdit(int id)
        {
            SQLiteConnection conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand(
                        "SELECT Username, Role, IsActive FROM users WHERE idUsers=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    var r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        _txtUsername.Text = r["Username"].ToString();
                        _txtUsername.ReadOnly = true;  // don't allow username change during edit
                        _txtUsername.BackColor = Color.FromArgb(240, 240, 245);
                        string role = r["Role"].ToString();
                        if (_cboRole.Items.Contains(role))
                            _cboRole.SelectedItem = role;
                        _cboStatus.SelectedItem = Convert.ToInt32(r["IsActive"]) == 1
                            ? "Active" : "Inactive";
                    }
                    r.Close();
                    _txtPassword.Clear();
                    _txtConfirmPassword.Clear();
                    UpdateRoleInfo();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading user:\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  SAVE USER  (Add or Update)
        // ══════════════════════════════════════════════════════════════════════
        private void SaveUser()
        {
            // Validate
            if (string.IsNullOrWhiteSpace(_txtUsername.Text) ||
                _txtUsername.Text.Trim().Length < 4)
            {
                MessageBox.Show("Username must be at least 4 characters.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtUsername.Focus(); return;
            }
            if (_cboRole.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a Role.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _cboRole.Focus(); return;
            }

            // Password validation
            bool changingPassword = !string.IsNullOrWhiteSpace(_txtPassword.Text);
            if (!_isEditMode || changingPassword)
            {
                if (!_isEditMode && string.IsNullOrWhiteSpace(_txtPassword.Text))
                {
                    MessageBox.Show("Password is required for new users.",
                        "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtPassword.Focus(); return;
                }
                if (changingPassword && _txtPassword.Text.Length < 6)
                {
                    MessageBox.Show("Password must be at least 6 characters.",
                        "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtPassword.Focus(); return;
                }
                if (changingPassword && _txtPassword.Text != _txtConfirmPassword.Text)
                {
                    MessageBox.Show("Passwords do not match.",
                        "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtConfirmPassword.Focus(); return;
                }
            }

            if (_isEditMode) UpdateUser(changingPassword);
            else AddUser();
        }

        private void AddUser()
        {
            // Check username uniqueness
            if (UsernameExists(_txtUsername.Text.Trim()))
            {
                MessageBox.Show("Username already exists. Please choose a different one.",
                    "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtUsername.Focus(); return;
            }

            SQLiteConnection conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand(@"
                        INSERT INTO users (Username, Password, Role, IsActive)
                        VALUES (@u, MD5(@p), @r, @a)", conn);
                    cmd.Parameters.AddWithValue("@u", _txtUsername.Text.Trim());
                    cmd.Parameters.AddWithValue("@p", _txtPassword.Text);
                    cmd.Parameters.AddWithValue("@r", _cboRole.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@a",
                        _cboStatus.SelectedItem.ToString() == "Active" ? 1 : 0);

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show("User added successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ShowList();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error adding user:\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateUser(bool changingPassword)
        {
            if (MessageBox.Show("Save changes to this user?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            SQLiteConnection conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    string q = changingPassword
                        ? "UPDATE users SET Password=MD5(@p), Role=@r, IsActive=@a WHERE idUsers=@id"
                        : "UPDATE users SET Role=@r, IsActive=@a WHERE idUsers=@id";

                    var cmd = new SQLiteCommand(q, conn);
                    if (changingPassword)
                        cmd.Parameters.AddWithValue("@p", _txtPassword.Text);
                    cmd.Parameters.AddWithValue("@r", _cboRole.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@a",
                        _cboStatus.SelectedItem.ToString() == "Active" ? 1 : 0);
                    cmd.Parameters.AddWithValue("@id", _selectedUserId);

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show("User updated successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ShowList();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating user:\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DeleteUser(int id, string username)
        {
            // Protect SuperAdmin from deletion
            if (username.Equals("superadmin", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("The SuperAdmin account cannot be deleted.",
                    "Protected", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
            }
            // Protect currently logged-in user
            if (id == CurrentUser.UserId)
            {
                MessageBox.Show("You cannot delete your own account while logged in.",
                    "Protected", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
            }

            if (MessageBox.Show(
                $"Permanently delete user '{username}'?\n\nThis cannot be undone!",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                != DialogResult.Yes) return;

            SQLiteConnection conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand(
                        "DELETE FROM users WHERE idUsers=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("User deleted.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SearchUsers() => LoadUsers(_txtSearch.Text.Trim());

        private void ClearForm()
        {
            _txtUsername.Text = "";
            _txtUsername.ReadOnly = false;
            _txtUsername.BackColor = Color.White;
            _txtPassword.Text = "";
            _txtConfirmPassword.Text = "";
            _cboRole.SelectedIndex = -1;
            _cboStatus.SelectedIndex = 0;
            _lblRoleInfo.Visible = false;
        }

        private bool UsernameExists(string username, int excludeId = 0)
        {
            SQLiteConnection conn = DatabaseConnection.GetConnection();
            if (conn == null) return false;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand(
                        "SELECT COUNT(*) FROM users WHERE Username=@u AND idUsers!=@id",
                        conn);
                    cmd.Parameters.AddWithValue("@u", username);
                    cmd.Parameters.AddWithValue("@id", excludeId);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
                catch { return false; }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  GRID — add Edit + Delete buttons as extra columns
        // ══════════════════════════════════════════════════════════════════════
        private DataGridView MakeGrid()
        {
            var g = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = BgCard,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowTemplate = { Height = 38 },
                Font = new Font("Segoe UI", 9.5f),
                GridColor = Color.FromArgb(224, 228, 242),
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 40,
                ColumnHeadersHeightSizeMode =
                    DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = NavBlue;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9.5f, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            g.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            g.DefaultCellStyle.SelectionBackColor = BtnBlue;
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.AlternatingRowsDefaultCellStyle.BackColor = LightBlue;

            // Add Edit + Delete button columns — wider and properly painted
            var colEdit = new DataGridViewButtonColumn
            {
                Name = "colEdit",
                HeaderText = "Actions",
                Text = "✏ Edit",
                UseColumnTextForButtonValue = true,
                Width = 110,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                FlatStyle = FlatStyle.Flat,
            };
            var colDelete = new DataGridViewButtonColumn
            {
                Name = "colDelete",
                HeaderText = "",
                Text = "🗑 Delete",
                UseColumnTextForButtonValue = true,
                Width = 110,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                FlatStyle = FlatStyle.Flat,
            };

            g.DataBindingComplete += (s, e) =>
            {
                if (!g.Columns.Contains("colEdit")) g.Columns.Add(colEdit);
                if (!g.Columns.Contains("colDelete")) g.Columns.Add(colDelete);
            };

            g.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                int id = Convert.ToInt32(g.Rows[e.RowIndex].Cells["idUsers"].Value);
                string uname = g.Rows[e.RowIndex].Cells["Username"].Value?.ToString() ?? "";
                if (e.ColumnIndex == g.Columns["colEdit"]?.Index)
                    ShowForm(true, id);
                else if (e.ColumnIndex == g.Columns["colDelete"]?.Index)
                    DeleteUser(id, uname);
            };

            // Paint clean rounded buttons in each action cell
            g.CellPainting += (s, e) =>
            {
                if (e.RowIndex < 0) return;

                bool isEdit = g.Columns["colEdit"] != null && e.ColumnIndex == g.Columns["colEdit"].Index;
                bool isDelete = g.Columns["colDelete"] != null && e.ColumnIndex == g.Columns["colDelete"].Index;
                if (!isEdit && !isDelete) return;

                // Fill cell background
                e.Graphics.FillRectangle(
                    new SolidBrush(e.RowIndex % 2 == 0 ? BgCard : LightBlue),
                    e.CellBounds);

                // Draw rounded button inside cell
                Color btnColor = isEdit ? BtnBlue : BtnRed;
                string btnText = isEdit ? "Edit" : "Delete";

                int pad = 6;
                var btnRect = new Rectangle(
                    e.CellBounds.X + pad,
                    e.CellBounds.Y + 5,
                    e.CellBounds.Width - pad * 2,
                    e.CellBounds.Height - 10);

                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    int r = 6, d = r * 2;
                    path.AddArc(btnRect.X, btnRect.Y, d, d, 180, 90);
                    path.AddArc(btnRect.Right - d, btnRect.Y, d, d, 270, 90);
                    path.AddArc(btnRect.Right - d, btnRect.Bottom - d, d, d, 0, 90);
                    path.AddArc(btnRect.X, btnRect.Bottom - d, d, d, 90, 90);
                    path.CloseFigure();
                    e.Graphics.FillPath(new SolidBrush(btnColor), path);
                }

                using (var sf = new StringFormat
                { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                using (var brush = new SolidBrush(Color.White))
                    e.Graphics.DrawString(btnText,
                        new Font("Segoe UI", 9f, FontStyle.Bold),
                        brush, btnRect, sf);

                e.Handled = true;
            };

            return g;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  UI HELPERS
        // ══════════════════════════════════════════════════════════════════════
        private Panel MakeCard(Point location, Size size)
        {
            var p = new Panel { BackColor = BgCard, Location = location, Size = size };
            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(BorderCol))
                    DrawRR(e.Graphics, pen,
                        new Rectangle(0, 0, p.Width - 1, p.Height - 1), 10);
            };
            return p;
        }

        private Button MakeBtn(string text, Color bg, float size)
        {
            var b = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", size, FontStyle.Bold),
                BackColor = bg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(
                Math.Max(0, bg.R - 22),
                Math.Max(0, bg.G - 22),
                Math.Max(0, bg.B - 22));
            return b;
        }

        private static Label FieldLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(68, 78, 102),
                AutoSize = true,
                Location = new Point(x, y + 6),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleRight,
            };
        }

        private static TextBox FieldBox(int x, int y, int w)
        {
            return new TextBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(x, y),
                Size = new Size(w, 30),
                BorderStyle = BorderStyle.FixedSingle,
            };
        }

        private static void DrawRR(Graphics g, Pen pen, Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            g.DrawPath(pen, path);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // UsersControl
            // 
            this.Name = "UsersControl";
            this.Load += new System.EventHandler(this.UsersControl_Load);
            this.ResumeLayout(false);

        }

        private void UsersControl_Load(object sender, EventArgs e)
        {

        }
    }
}