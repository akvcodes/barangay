using System.Data.SQLite;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BarangayManagementSystem
{
    public partial class LoginForm : Form
    {
        // ── Hostinger credentials (kept as fallback info only) ────────────────
        private const string ONLINE_SERVER = "srv1237.hstgr.io";
        private const string ONLINE_DATABASE = "u621755393_BaManSys";
        private const string ONLINE_USER = "u621755393_BaManSys";
        private const string ONLINE_PASS = "BaManSys01";

        private static readonly string CONFIG_FILE = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BarangaySystem", "config.txt");

        // ── Login connection-settings action ─────────────────────────────────
        private Button _btnSettings;

        public LoginForm()
        {
            InitializeComponent();
            txtPassword.PasswordChar = '●';
            txtUsername.Focus();
            BuildSettingsPanel();
            ApplyLoginLayout();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  SETTINGS BUTTON + PANEL
        // ══════════════════════════════════════════════════════════════════════
        private void BuildSettingsPanel()
        {
            // Settings button
            _btnSettings = new Button();
            _btnSettings.Text = "CONNECTION SETTINGS";
            _btnSettings.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _btnSettings.ForeColor = Color.FromArgb(44, 62, 80);
            _btnSettings.BackColor = Color.White;
            _btnSettings.FlatStyle = FlatStyle.Flat;
            _btnSettings.Cursor = Cursors.Hand;
            _btnSettings.Size = new Size(378, 38);
            _btnSettings.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            _btnSettings.TabStop = false;
            _btnSettings.FlatAppearance.BorderSize = 1;
            _btnSettings.FlatAppearance.BorderColor = Color.FromArgb(200, 205, 225);
            _btnSettings.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 235, 248);
            _btnSettings.FlatAppearance.MouseDownBackColor = Color.FromArgb(215, 220, 240);
            _btnSettings.Click += new EventHandler(BtnSettings_Click);
            panelRight.Controls.Add(_btnSettings);

            PositionSettingsBtn();
            _btnSettings.BringToFront();

            panelRight.Resize += new EventHandler(PanelRight_Resize);
        }

        private void PanelRight_Resize(object sender, EventArgs e)
        {
            PositionSettingsBtn();
            ApplyLoginLayout();
        }

        private void PositionSettingsBtn()
        {
            if (_btnSettings == null || panelRight == null) return;
            int width = Math.Min(360, panelRight.ClientSize.Width - 64);
            _btnSettings.Size = new Size(width, 38);
            _btnSettings.Location = new Point(
                (panelRight.ClientSize.Width - width) / 2,
                panelRight.ClientSize.Height - 91);
        }

        private void ApplyLoginLayout()
        {
            if (panelRight == null || panelLeft == null || panelRight.ClientSize.Width < 1) return;

            int panelWidth = Math.Max(360, Math.Min(500, (int)(ClientSize.Width * 0.30f)));
            panelRight.Width = panelWidth;
            int formWidth = Math.Max(260, Math.Min(360, panelRight.ClientSize.Width - 96));
            int formLeft = (panelRight.ClientSize.Width - formWidth) / 2;
            int height = panelRight.ClientSize.Height;
            int top = Math.Max(120, (height - 448) / 2);
            int logoSize = Math.Max(88, Math.Min(130, panelRight.ClientSize.Width / 4));

            int panelCenter = panelRight.ClientSize.Width / 2;
            pictureBoxLogo.Location = new Point(panelCenter - (logoSize / 2), top);
            pictureBoxLogo.Size = new Size(logoSize, (int)(logoSize * 0.86f));

            lblSystemTitle.Location = new Point(0, top + 108);
            lblSystemTitle.Size = new Size(panelRight.ClientSize.Width, 30);
            lblSystemSub.Location = new Point(0, top + 142);
            lblSystemSub.Size = new Size(panelRight.ClientSize.Width, 22);

            label4.Location = new Point(formLeft, top + 162);
            label4.Size = new Size(formWidth, 22);
            txtUsername.Location = new Point(formLeft, top + 186);
            txtUsername.Size = new Size(formWidth, 32);
            label5.Location = new Point(formLeft, top + 216);
            label5.Size = new Size(formWidth, 22);
            txtPassword.Location = new Point(formLeft, top + 240);
            txtPassword.Size = new Size(formWidth, 32);
            chkShowPassword.Location = new Point(formLeft, top + 282);
            btnLogin.Location = new Point(formLeft, top + 318);
            btnLogin.Size = new Size(formWidth, 40);
            lblMessage.Location = new Point(formLeft, top + 366);
            lblMessage.Size = new Size(formWidth, 22);

            int leftPanelWidth = Math.Max(1, panelRight.Left);
            int sealSize = Math.Max(190, Math.Min(310, leftPanelWidth / 4));
            local_government_logo.Location = new Point(
                Math.Max(0, (leftPanelWidth - sealSize) / 2),
                Math.Max(80, (panelLeft.ClientSize.Height - 620) / 2));
            local_government_logo.Size = new Size(sealSize, (int)(sealSize * 0.91f));
            lblLGUName.Location = new Point(0, local_government_logo.Bottom + 20);
            lblLGUName.Size = new Size(leftPanelWidth, 58);
            lblLGUAddress.Location = new Point(0, lblLGUName.Bottom + 4);
            lblLGUAddress.Size = new Size(leftPanelWidth, 36);

            PositionSettingsBtn();
            lblClose.Location = new Point(panelRight.ClientSize.Width - 40, 8);
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            using (var settings = new Integration.ConnectionSettingsEditForm())
                settings.ShowDialog(this);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  FORM LOAD
        // ══════════════════════════════════════════════════════════════════════
        private void LoginForm_Load(object sender, EventArgs e)
        {
            LoadLoginBranding();
            LoadUsernames();
            txtUsername.Select();

            _rbLocal.Visible = false;
            _rbRemote.Visible = false;
            _rbOnline.Visible = false;
            _lblConnType.Visible = false;
            _lblConnStatus.Visible = false;
        }

        private void LoadLoginBranding()
        {
            var systemProfile = BarangayManagementSystem.Controls.SettingsHubControl.LoadSystemProfile();
            lblSystemTitle.Text = string.IsNullOrWhiteSpace(systemProfile.LoginSystemTitle)
                ? "Barangay Management System" : systemProfile.LoginSystemTitle;
            lblSystemSub.Text = string.IsNullOrWhiteSpace(systemProfile.LoginSubtitle)
                ? "Please sign in to your account" : systemProfile.LoginSubtitle;
            lblFooter.Text = string.IsNullOrWhiteSpace(systemProfile.LoginFooterText)
                ? "Barangay Management System" : systemProfile.LoginFooterText;
            lblLGUAddress.Text = systemProfile.LoginAddressSubtitle ?? string.Empty;
            lblLGUName.Text = "Local Government";

            try
            {
                if (!string.IsNullOrWhiteSpace(systemProfile.LoginRightLogoPath) && File.Exists(systemProfile.LoginRightLogoPath))
                {
                    using (var source = Image.FromFile(systemProfile.LoginRightLogoPath))
                    {
                        pictureBoxLogo.Image?.Dispose();
                        pictureBoxLogo.Image = new Bitmap(source);
                        pictureBoxLogo.BackgroundImage = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading login logo: " + ex.Message,
                    "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            string backgroundPath = string.IsNullOrWhiteSpace(systemProfile.LoginLeftBackgroundPath)
                ? systemProfile.BackgroundPath : systemProfile.LoginLeftBackgroundPath;
            try
            {
                if (!string.IsNullOrWhiteSpace(backgroundPath) && File.Exists(backgroundPath))
                {
                    using (var source = Image.FromFile(backgroundPath))
                    {
                        panelLeft.BackgroundImage?.Dispose();
                        panelLeft.BackgroundImage = new Bitmap(source);
                        panelLeft.BackgroundImageLayout = ImageLayout.Stretch;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading login background: " + ex.Message,
                    "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn == null) return;

                try
                {
                    BarangayManagementSystem.Controls.SettingsHubControl.EnsureLoginSettingsColumns(conn);
                    byte[] logoBytes = null;
                    using (var cmd = new SQLiteCommand(
                        "SELECT Municipality, LoginAddressSubtitle, LoginFooterText, Logo FROM company_profile WHERE id = 1 LIMIT 1",
                        conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string municipality = reader["Municipality"]?.ToString();
                            lblLGUName.Text = string.IsNullOrWhiteSpace(municipality)
                                ? "Local Government"
                                : "Local Government of " + municipality.Trim();
                            if (reader["LoginAddressSubtitle"] != DBNull.Value)
                                lblLGUAddress.Text = reader["LoginAddressSubtitle"].ToString();
                            if (reader["LoginFooterText"] != DBNull.Value)
                                lblFooter.Text = reader["LoginFooterText"].ToString();
                            if (reader["Logo"] != DBNull.Value &&
                                (string.IsNullOrWhiteSpace(systemProfile.LoginRightLogoPath) || !File.Exists(systemProfile.LoginRightLogoPath)))
                                logoBytes = (byte[])reader["Logo"];
                        }
                    }

                    if (logoBytes != null)
                    {
                        using (var stream = new MemoryStream(logoBytes))
                        using (var logo = Image.FromStream(stream))
                        {
                            pictureBoxLogo.Image?.Dispose();
                            pictureBoxLogo.Image = new Bitmap(logo);
                            pictureBoxLogo.BackgroundImage = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading login profile: " + ex.Message,
                        "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadUsernames()
        {
            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn == null) return;

                try
                {
                    using (var cmd = new SQLiteCommand(
                        "SELECT Username FROM users WHERE IsActive = 1 ORDER BY Username",
                        conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        txtUsername.BeginUpdate();
                        txtUsername.Items.Clear();
                        while (reader.Read())
                            txtUsername.Items.Add(reader["Username"].ToString());
                        txtUsername.EndUpdate();
                    }
                }
                catch (Exception ex)
                {
                    txtUsername.EndUpdate();
                    MessageBox.Show("Error loading usernames: " + ex.Message,
                        "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  CONNECTION SELECTOR
        // ══════════════════════════════════════════════════════════════════════
        private void rbLocal_CheckedChanged(object sender, EventArgs e)
        {
            if (_rbLocal.Checked) ApplyConnection("Local");
        }

        private void rbRemote_CheckedChanged(object sender, EventArgs e)
        {
            if (_rbRemote.Checked) ApplyConnection("Remote");
        }

        private void rbOnline_CheckedChanged(object sender, EventArgs e)
        {
            if (_rbOnline.Checked) ApplyConnection("Online");
        }

        private void ApplyConnection(string type)
        {
            lblMessage.Text = "";

            // The app now uses a LOCAL SQLite database regardless of the choice.
            // We keep this method for UI compatibility but it no longer changes
            // the actual connection.
            switch (type)
            {
                case "Local":
                    SetStatus("● Local SQLite File", Color.FromArgb(15, 100, 60));
                    break;
                case "Remote":
                    SetStatus("● Local SQLite File", Color.FromArgb(15, 100, 60));
                    break;
                default:
                    SetStatus("● Local SQLite File", Color.FromArgb(15, 100, 60));
                    break;
            }

            SaveConnectionType(type);
        }

        private void SetStatus(string text, Color color)
        {
            _lblConnStatus.Text = text;
            _lblConnStatus.ForeColor = color;
        }

        private void SaveConnectionType(string type)
        {
            try
            {
                string dir = Path.GetDirectoryName(CONFIG_FILE);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var lines = File.Exists(CONFIG_FILE)
                    ? new System.Collections.Generic.List<string>(File.ReadAllLines(CONFIG_FILE))
                    : new System.Collections.Generic.List<string>();
                bool found = false;
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].StartsWith("ConnectionType="))
                    {
                        lines[i] = "ConnectionType=" + type;
                        found = true;
                        break;
                    }
                }
                if (!found) lines.Add("ConnectionType=" + type);
                File.WriteAllLines(CONFIG_FILE, lines);
            }
            catch { }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  LOGIN
        // ══════════════════════════════════════════════════════════════════════
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = "Please enter both username and password.";
                lblMessage.ForeColor = Color.OrangeRed;
                return;
            }

            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn == null) return;
                try
                {
                    EnsureUserSecurityColumns(conn);

                    string query =
                        "SELECT idUsers, Username, Role, Password, Salt " +
                        "FROM users " +
                        "WHERE Username = @username AND IsActive = 1";

                    int userId = 0;
                    string uname = "";
                    string role = "";
                    string storedPassword = "";
                    string salt = null;
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                userId = Convert.ToInt32(reader["idUsers"]);
                                uname = reader["Username"].ToString();
                                role = reader["Role"].ToString();
                                storedPassword = reader["Password"]?.ToString() ?? "";
                                salt = reader["Salt"] == DBNull.Value
                                    ? null
                                    : reader["Salt"].ToString();
                            }
                        }
                    }

                    bool legacyPassword = false;
                    bool passwordValid = false;
                    if (userId > 0)
                    {
                        if (string.IsNullOrEmpty(salt))
                        {
                            bool matchesLegacyMd5 = string.Equals(
                                storedPassword, ComputeMD5(password),
                                StringComparison.OrdinalIgnoreCase);
                            bool matchesLegacyPlaintext = string.Equals(
                                storedPassword, password, StringComparison.Ordinal);
                            passwordValid = matchesLegacyMd5 || matchesLegacyPlaintext;
                            legacyPassword = passwordValid;
                        }
                        else
                        {
                            passwordValid = string.Equals(
                                storedPassword, CryptoHelper.ComputeSHA256(password, salt),
                                StringComparison.Ordinal);
                        }
                    }

                    if (passwordValid)
                    {
                        if (legacyPassword)
                        {
                            salt = CryptoHelper.GenerateSalt();
                            using (var upgradeCmd = new SQLiteCommand(
                                "UPDATE users SET Password=@password, Salt=@salt WHERE idUsers=@id",
                                conn))
                            {
                                upgradeCmd.Parameters.AddWithValue(
                                    "@password", CryptoHelper.ComputeSHA256(password, salt));
                                upgradeCmd.Parameters.AddWithValue("@salt", salt);
                                upgradeCmd.Parameters.AddWithValue("@id", userId);
                                upgradeCmd.ExecuteNonQuery();
                            }
                        }

                        using (var resetCmd = new SQLiteCommand(
                            "UPDATE users SET LoginAttempts = 0 WHERE idUsers = @id", conn))
                        {
                            resetCmd.Parameters.AddWithValue("@id", userId);
                            resetCmd.ExecuteNonQuery();
                        }

                        CurrentUser.Login(userId, uname, role);
                        CurrentUser.LoadProfile();
                        ThemedDialog.Show(
                            "Welcome, " + uname + "!\nRole: " + role,
                            "Login Successful", false);
                        this.Hide();
                        new MainMenuForm().Show();
                    }
                    else
                    {
                        bool accountLocked = false;
                        using (var transaction = conn.BeginTransaction())
                        {
                            int updatedRows;
                            using (var incrementCmd = new SQLiteCommand(
                                "UPDATE users SET LoginAttempts = COALESCE(LoginAttempts, 0) + 1 " +
                                "WHERE Username = @username AND IsActive = 1",
                                conn, transaction))
                            {
                                incrementCmd.Parameters.AddWithValue("@username", username);
                                updatedRows = incrementCmd.ExecuteNonQuery();
                            }

                            object attemptsValue;
                            using (var attemptsCmd = new SQLiteCommand(
                                "SELECT LoginAttempts FROM users WHERE Username = @username",
                                conn, transaction))
                            {
                                attemptsCmd.Parameters.AddWithValue("@username", username);
                                attemptsValue = attemptsCmd.ExecuteScalar();
                            }

                            int attempts = attemptsValue == null || attemptsValue == DBNull.Value
                                ? 0
                                : Convert.ToInt32(attemptsValue);
                            if (updatedRows > 0 && attempts >= 3)
                            {
                                using (var lockCmd = new SQLiteCommand(
                                    "UPDATE users SET IsActive = 0 " +
                                    "WHERE Username = @username AND IsActive = 1",
                                    conn, transaction))
                                {
                                    lockCmd.Parameters.AddWithValue("@username", username);
                                    accountLocked = lockCmd.ExecuteNonQuery() > 0;
                                }
                            }
                            transaction.Commit();
                        }

                        if (accountLocked)
                            MessageBox.Show(
                                "Account locked due to 3 failed login attempts. Please contact the Admin.",
                                "Account Locked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        else
                        {
                            lblMessage.Text = "Invalid username or password.";
                            lblMessage.ForeColor = Color.Red;
                        }
                        txtPassword.Clear();
                        txtPassword.Focus();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error during login: " + ex.Message,
                        "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static void EnsureUserSecurityColumns(SQLiteConnection conn)
        {
            bool hasLoginAttempts = false;
            bool hasSalt = false;
            using (var check = new SQLiteCommand("PRAGMA table_info(users)", conn))
            using (var reader = check.ExecuteReader())
            {
                while (reader.Read())
                {
                    string name = reader["name"].ToString();
                    hasLoginAttempts |= name.Equals("LoginAttempts", StringComparison.OrdinalIgnoreCase);
                    hasSalt |= name.Equals("Salt", StringComparison.OrdinalIgnoreCase);
                }
            }
            if (!hasLoginAttempts)
                using (var alter = new SQLiteCommand(
                    "ALTER TABLE users ADD COLUMN LoginAttempts INTEGER DEFAULT 0", conn))
                    alter.ExecuteNonQuery();
            if (!hasSalt)
                using (var alter = new SQLiteCommand(
                    "ALTER TABLE users ADD COLUMN Salt VARCHAR(100) DEFAULT NULL", conn))
                    alter.ExecuteNonQuery();
        }

        private void btnInstallDB_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                    "This will create the Users table in your database. Continue?",
                    "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                InstallDatabase();
            }
        }

        private void InstallDatabase()
        {
            using (SQLiteConnection conn = DatabaseConnection.GetConnection())
            {
                if (conn == null) return;
                try
                {
                    new SQLiteCommand(
                        "CREATE TABLE IF NOT EXISTS users (" +
                        "idUsers INTEGER PRIMARY KEY AUTOINCREMENT," +
                        "Username TEXT NOT NULL UNIQUE," +
                        "Password TEXT NOT NULL," +
                        "Salt VARCHAR(100) DEFAULT NULL," +
                        "Role TEXT DEFAULT 'User'," +
                        "IsActive INTEGER DEFAULT 1," +
                        "LoginAttempts INTEGER DEFAULT 0," +
                        "CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP);", conn).ExecuteNonQuery();

                    EnsureUserSecurityColumns(conn);
                    string salt1 = CryptoHelper.GenerateSalt();
                    string salt2 = CryptoHelper.GenerateSalt();
                    string salt3 = CryptoHelper.GenerateSalt();
                    string salt4 = CryptoHelper.GenerateSalt();
                    using (var cmd = new SQLiteCommand(
                        "INSERT OR IGNORE INTO users (Username,Password,Salt,Role,IsActive) VALUES" +
                        "(@u1,@p1,@s1,'SuperAdmin',1)," +
                        "(@u2,@p2,@s2,'Admin',1)," +
                        "(@u3,@p3,@s3,'Official',1)," +
                        "(@u4,@p4,@s4,'User',1);", conn))
                    {
                        cmd.Parameters.AddWithValue("@u1", "superadmin");
                        cmd.Parameters.AddWithValue("@s1", salt1);
                        cmd.Parameters.AddWithValue("@p1", CryptoHelper.ComputeSHA256("SuperAdmin123", salt1));
                        cmd.Parameters.AddWithValue("@u2", "admin");
                        cmd.Parameters.AddWithValue("@s2", salt2);
                        cmd.Parameters.AddWithValue("@p2", CryptoHelper.ComputeSHA256("admin123", salt2));
                        cmd.Parameters.AddWithValue("@u3", "official");
                        cmd.Parameters.AddWithValue("@s3", salt3);
                        cmd.Parameters.AddWithValue("@p3", CryptoHelper.ComputeSHA256("official123", salt3));
                        cmd.Parameters.AddWithValue("@u4", "user");
                        cmd.Parameters.AddWithValue("@s4", salt4);
                        cmd.Parameters.AddWithValue("@p4", CryptoHelper.ComputeSHA256("user123", salt4));
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show(
                        "Done!\n\nsuperadmin/SuperAdmin123\nadmin/admin123\nofficial/official123\nuser/user123",
                        "Install Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  MD5 HELPER
        // ══════════════════════════════════════════════════════════════════════
        private static string ComputeMD5(string input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                var sb = new System.Text.StringBuilder();
                foreach (byte b in hash)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        // ── Stubs matching the designer ───────────────────────────────────────
        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = chkShowPassword.Checked ? '\0' : '●';
            txtPassword.SelectionStart = txtPassword.Text.Length;
            txtPassword.SelectionLength = 0;
            txtPassword.Focus();
        }

        private void lblClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void lblClose_MouseEnter(object sender, EventArgs e)
        {
            lblClose.ForeColor = Color.Red;
        }

        private void lblClose_MouseLeave(object sender, EventArgs e)
        {
            lblClose.ForeColor = Color.Gray;
        }

        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void panelLeft_Paint(object sender, PaintEventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void lblLGUAddress_Click(object sender, EventArgs e) { }
        private void lblFooter_Click(object sender, EventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void pictureBox3_Click(object sender, EventArgs e) { }
    }
}