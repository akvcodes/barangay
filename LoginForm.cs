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

        // ── Settings panel controls ───────────────────────────────────────────
        private Button _btnSettings;
        private Panel _settingsPanel;
        private RadioButton _rbLocal2, _rbRemote2, _rbOnline2;
        private Label _lblStatus2;

        public LoginForm()
        {
            InitializeComponent();
            txtPassword.PasswordChar = '●';
            txtUsername.Focus();
            BuildSettingsPanel();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  SETTINGS BUTTON + PANEL
        // ══════════════════════════════════════════════════════════════════════
        private void BuildSettingsPanel()
        {
            // Settings button
            _btnSettings = new Button();
            _btnSettings.Text = "Settings";
            _btnSettings.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            _btnSettings.ForeColor = Color.FromArgb(44, 62, 80);
            _btnSettings.BackColor = Color.FromArgb(240, 242, 250);
            _btnSettings.FlatStyle = FlatStyle.Flat;
            _btnSettings.Cursor = Cursors.Hand;
            _btnSettings.Size = new Size(74, 26);
            _btnSettings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _btnSettings.TabStop = false;
            _btnSettings.FlatAppearance.BorderSize = 1;
            _btnSettings.FlatAppearance.BorderColor = Color.FromArgb(200, 205, 225);
            _btnSettings.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 224, 245);
            _btnSettings.FlatAppearance.MouseDownBackColor = Color.FromArgb(200, 208, 240);
            _btnSettings.Click += new EventHandler(BtnSettings_Click);
            panelRight.Controls.Add(_btnSettings);
            PositionSettingsBtn();
            _btnSettings.BringToFront();

            // Settings panel
            _settingsPanel = new Panel();
            _settingsPanel.Size = new Size(220, 200);
            _settingsPanel.BackColor = Color.FromArgb(250, 251, 255);
            _settingsPanel.BorderStyle = BorderStyle.FixedSingle;
            _settingsPanel.Visible = false;
            _settingsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // Section: Connection Type
            Label lblConnSection = new Label();
            lblConnSection.Text = "CONNECTION TYPE";
            lblConnSection.Font = new Font("Segoe UI", 7f, FontStyle.Bold);
            lblConnSection.ForeColor = Color.FromArgb(26, 35, 126);
            lblConnSection.BackColor = Color.FromArgb(235, 238, 255);
            lblConnSection.Location = new Point(0, 0);
            lblConnSection.Size = new Size(220, 22);
            lblConnSection.TextAlign = ContentAlignment.MiddleLeft;
            lblConnSection.Padding = new Padding(10, 0, 0, 0);

            _rbLocal2 = MakeRadio("Local", new Point(14, 28));
            _rbRemote2 = MakeRadio("Remote", new Point(14, 52));
            _rbOnline2 = MakeRadio("Online", new Point(14, 76));

            _rbLocal2.CheckedChanged += new EventHandler(RbConn_Changed);
            _rbRemote2.CheckedChanged += new EventHandler(RbConn_Changed);
            _rbOnline2.CheckedChanged += new EventHandler(RbConn_Changed);

            _lblStatus2 = new Label();
            _lblStatus2.Text = "";
            _lblStatus2.Font = new Font("Segoe UI", 7.5f, FontStyle.Italic);
            _lblStatus2.ForeColor = Color.FromArgb(100, 110, 140);
            _lblStatus2.Location = new Point(14, 102);
            _lblStatus2.Size = new Size(192, 18);
            _lblStatus2.BackColor = Color.Transparent;

            // Divider
            Panel divider = new Panel();
            divider.BackColor = Color.FromArgb(210, 215, 235);
            divider.Location = new Point(0, 126);
            divider.Size = new Size(220, 1);

            // Section: Application
            Label lblAppSection = new Label();
            lblAppSection.Text = "APPLICATION";
            lblAppSection.Font = new Font("Segoe UI", 7f, FontStyle.Bold);
            lblAppSection.ForeColor = Color.FromArgb(26, 35, 126);
            lblAppSection.BackColor = Color.FromArgb(235, 238, 255);
            lblAppSection.Location = new Point(0, 128);
            lblAppSection.Size = new Size(220, 22);
            lblAppSection.TextAlign = ContentAlignment.MiddleLeft;
            lblAppSection.Padding = new Padding(10, 0, 0, 0);

            Button btnExitApp = new Button();
            btnExitApp.Text = "Exit Application";
            btnExitApp.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
            btnExitApp.ForeColor = Color.FromArgb(180, 35, 35);
            btnExitApp.BackColor = Color.Transparent;
            btnExitApp.FlatStyle = FlatStyle.Flat;
            btnExitApp.Cursor = Cursors.Hand;
            btnExitApp.Location = new Point(10, 156);
            btnExitApp.Size = new Size(198, 30);
            btnExitApp.TextAlign = ContentAlignment.MiddleLeft;
            btnExitApp.FlatAppearance.BorderSize = 0;
            btnExitApp.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 235, 235);
            btnExitApp.Click += new EventHandler(BtnExitApp_Click);

            _settingsPanel.Controls.Add(lblConnSection);
            _settingsPanel.Controls.Add(_rbLocal2);
            _settingsPanel.Controls.Add(_rbRemote2);
            _settingsPanel.Controls.Add(_rbOnline2);
            _settingsPanel.Controls.Add(_lblStatus2);
            _settingsPanel.Controls.Add(divider);
            _settingsPanel.Controls.Add(lblAppSection);
            _settingsPanel.Controls.Add(btnExitApp);

            panelRight.Controls.Add(_settingsPanel);
            _settingsPanel.BringToFront();

            this.MouseDown += new MouseEventHandler(Form_MouseDown);
            panelLeft.MouseDown += new MouseEventHandler(PanelLeft_MouseDown);
            panelRight.Resize += new EventHandler(PanelRight_Resize);
        }

        // ── Named event handlers ──────────────────────────────────────────────
        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            HideSettings();
        }

        private void PanelLeft_MouseDown(object sender, MouseEventArgs e)
        {
            HideSettings();
        }

        private void PanelRight_Resize(object sender, EventArgs e)
        {
            PositionSettingsBtn();
            PositionSettingsPanel();
        }

        private RadioButton MakeRadio(string text, Point loc)
        {
            RadioButton rb = new RadioButton();
            rb.Text = text;
            rb.Font = new Font("Segoe UI", 9.5f);
            rb.ForeColor = Color.FromArgb(44, 55, 90);
            rb.Location = loc;
            rb.Size = new Size(192, 22);
            rb.BackColor = Color.Transparent;
            rb.Cursor = Cursors.Hand;
            return rb;
        }

        private void RbConn_Changed(object sender, EventArgs e)
        {
            if (_rbLocal2.Checked)
            {
                ApplyConnection("Local");
                SyncMainRadios();
            }
            else if (_rbRemote2.Checked)
            {
                ApplyConnection("Remote");
                SyncMainRadios();
            }
            else if (_rbOnline2.Checked)
            {
                ApplyConnection("Online");
                SyncMainRadios();
            }
        }

        private void BtnExitApp_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to exit?", "Exit Application",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void PositionSettingsBtn()
        {
            if (_btnSettings == null || panelRight == null) return;
            _btnSettings.Location = new Point(panelRight.Width - 82, 8);
        }

        private void PositionSettingsPanel()
        {
            if (_settingsPanel == null || panelRight == null) return;
            _settingsPanel.Location = new Point(
                panelRight.Width - _settingsPanel.Width - 4, 38);
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            if (_settingsPanel.Visible)
            {
                HideSettings();
                return;
            }
            PositionSettingsPanel();
            _settingsPanel.BringToFront();
            _settingsPanel.Visible = true;
        }

        private void HideSettings()
        {
            if (_settingsPanel != null)
                _settingsPanel.Visible = false;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  FORM LOAD
        // ══════════════════════════════════════════════════════════════════════
        private void LoginForm_Load(object sender, EventArgs e)
        {
            txtUsername.Select();
            LoadSavedConnection();

            _rbLocal.Visible = false;
            _rbRemote.Visible = false;
            _rbOnline.Visible = false;
            _lblConnType.Visible = false;
            _lblConnStatus.Visible = false;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  CONNECTION SELECTOR
        // ══════════════════════════════════════════════════════════════════════
        private void LoadSavedConnection()
        {
            string connType = "Online";
            if (File.Exists(CONFIG_FILE))
            {
                try
                {
                    foreach (string line in File.ReadAllLines(CONFIG_FILE))
                    {
                        int eq = line.IndexOf('=');
                        if (eq < 0) continue;
                        if (line.Substring(0, eq).Trim() == "ConnectionType")
                            connType = line.Substring(eq + 1).Trim();
                    }
                }
                catch { }
            }
            switch (connType)
            {
                case "Local": _rbLocal2.Checked = true; break;
                case "Remote": _rbRemote2.Checked = true; break;
                default: _rbOnline2.Checked = true; break;
            }
        }

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
            if (_lblStatus2 != null)
            {
                _lblStatus2.Text = text;
                _lblStatus2.ForeColor = color;
            }
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

        private void SyncMainRadios()
        {
            _rbLocal.Checked = _rbLocal2.Checked;
            _rbRemote.Checked = _rbRemote2.Checked;
            _rbOnline.Checked = _rbOnline2.Checked;
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
                    string query =
                        "SELECT idUsers, Username, Role " +
                        "FROM users " +
                        "WHERE Username = @username " +
                        "  AND Password = @password " +
                        "  AND IsActive = 1";

                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", ComputeMD5(password));

                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = Convert.ToInt32(reader["idUsers"]);
                                string uname = reader["Username"].ToString();
                                string role = reader["Role"].ToString();

                                CurrentUser.Login(userId, uname, role);

                                MessageBox.Show(
                                    "Welcome, " + uname + "!\nRole: " + role,
                                    "Login Successful",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);

                                this.Hide();
                                new MainMenuForm().Show();
                            }
                            else
                            {
                                lblMessage.Text = "Invalid username or password.";
                                lblMessage.ForeColor = Color.Red;
                                txtPassword.Clear();
                                txtPassword.Focus();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error during login: " + ex.Message,
                        "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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
                        "Role TEXT DEFAULT 'User'," +
                        "IsActive INTEGER DEFAULT 1," +
                        "CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP);", conn).ExecuteNonQuery();

                    using (var cmd = new SQLiteCommand(
                        "INSERT OR IGNORE INTO users (Username,Password,Role,IsActive) VALUES" +
                        "(@u1,@p1,'SuperAdmin',1)," +
                        "(@u2,@p2,'Admin',1)," +
                        "(@u3,@p3,'Official',1)," +
                        "(@u4,@p4,'User',1);", conn))
                    {
                        cmd.Parameters.AddWithValue("@u1", "superadmin");
                        cmd.Parameters.AddWithValue("@p1", ComputeMD5("SuperAdmin123"));
                        cmd.Parameters.AddWithValue("@u2", "admin");
                        cmd.Parameters.AddWithValue("@p2", ComputeMD5("admin123"));
                        cmd.Parameters.AddWithValue("@u3", "official");
                        cmd.Parameters.AddWithValue("@p3", ComputeMD5("official123"));
                        cmd.Parameters.AddWithValue("@u4", "user");
                        cmd.Parameters.AddWithValue("@p4", ComputeMD5("user123"));
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