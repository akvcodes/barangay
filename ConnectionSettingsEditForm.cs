using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySqlConnector;

namespace BarangayManagementSystem.Integration
{
    // Shared editor used by the login screen and the main menu. LAN has
    // separate fields for all three databases; Remote uses the primary app
    // database only.
    public class ConnectionSettingsEditForm : Form
    {
        private ExternalDbConfig _config;
        private TabControl _tabs;

        private CredentialPanel _lanOwnDb, _lanCrs, _lanGgms;
        private CredentialPanel _remoteOwnDb;

        private Button _btnSave, _btnPreset;

        public ConnectionSettingsEditForm()
        {
            _config = ExternalDbConfig.Load();
            BuildUi();
        }

        private void BuildUi()
        {
            Text = "Connection Settings — Edit (Admin)";
            ClientSize = new Size(1200, 440);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var root = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = false,
                Padding = new Padding(16)
            };
            Controls.Add(root);

            root.Controls.Add(new Label
            {
                Text = "CONNECTION SETTINGS",
                AutoSize = true,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 35, 126),
                Margin = new Padding(0, 0, 0, 2)
            });

            _tabs = new TabControl { Width = 1140, Height = 300, Margin = new Padding(0, 12, 0, 0) };
            var tabLan = new TabPage("LAN (Network)");
            var tabRemote = new TabPage("Remote (Online)");
            _tabs.TabPages.Add(tabLan);
            _tabs.TabPages.Add(tabRemote);
            _tabs.SelectedIndex = _config.ActiveProfile == ActiveNetworkProfile.Lan ? 0 : 1;
            root.Controls.Add(_tabs);

            var lanLayout = new FlowLayoutPanel { Location = new Point(0, 0), Width = 1120, Height = 280, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(8) };
            _lanOwnDb = new CredentialPanel("Own Database (BMS mirror)", _config.OwnDb.Lan, 340);
            _lanCrs = new CredentialPanel("CRS — read-only", _config.Crs.Lan, 340);
            _lanGgms = new CredentialPanel("GGMS — read + write", _config.Ggms.Lan, 340);
            lanLayout.Controls.Add(_lanOwnDb);
            lanLayout.Controls.Add(_lanCrs);
            lanLayout.Controls.Add(_lanGgms);
            tabLan.Controls.Add(lanLayout);

            var remoteLayout = new FlowLayoutPanel { Location = new Point(0, 0), Width = 1120, Height = 280, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(8) };
            _remoteOwnDb = new CredentialPanel("Online Connection (Cloud Database)", _config.OwnDb.Remote, 620);
            remoteLayout.Controls.Add(_remoteOwnDb);
            tabRemote.Controls.Add(remoteLayout);

            // ── Buttons ──────────────────────────────────────────────────
            var btnRow = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, Margin = new Padding(0, 10, 0, 0) };

            _btnPreset = new Button { Text = "Load Preset (this machine only)", AutoSize = true };
            _btnPreset.Click += BtnPreset_Click;

            var btnTestCurrentTab = new Button { Text = "Test Connections on This Tab", AutoSize = true };
            btnTestCurrentTab.Click += async (s, e) => await TestCurrentTabAsync();

            _btnSave = new Button { Text = "Save", AutoSize = true };
            _btnSave.Click += BtnSave_Click;

            btnRow.Controls.Add(_btnPreset);
            btnRow.Controls.Add(btnTestCurrentTab);
            btnRow.Controls.Add(_btnSave);
            root.Controls.Add(btnRow);
        }

        // Presets never come from a hardcoded source value — only from a
        // gitignored, dev-machine-local file the developer creates once
        // from integration_presets.example.json.
        private void BtnPreset_Click(object sender, EventArgs e)
        {
            var preset = ExternalDbConfig.LoadPresetsOrNull();
            if (preset == null)
            {
                MessageBox.Show(
                    "No local preset file found.\n\n" +
                    "Copy integration_presets.example.json to\n" +
                    "%AppData%\\BarangaySystem\\integration_presets.local.json\n" +
                    "and fill in real values on this machine only — never commit that file.",
                    "No Preset Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _lanOwnDb.LoadFrom(preset.OwnDb.Lan);
            _lanCrs.LoadFrom(preset.Crs.Lan);
            _lanGgms.LoadFrom(preset.Ggms.Lan);
            _remoteOwnDb.LoadFrom(preset.OwnDb.Remote);
        }

        private async Task TestCurrentTabAsync()
        {
            bool lan = _tabs.SelectedIndex == 0;
            var results = new System.Collections.Generic.List<string>();
            if (lan)
            {
                results.Add(await TestOneAsync(_lanOwnDb.ToCredential(), "Own Database"));
                results.Add(await TestOneAsync(_lanCrs.ToCredential(), "CRS"));
                results.Add(await TestOneAsync(_lanGgms.ToCredential(), "GGMS"));
            }
            else
            {
                results.Add(await TestOneAsync(_remoteOwnDb.ToCredential(), "Own Database"));
            }

            MessageBox.Show(string.Join("\n", results), "Test Results",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task<string> TestOneAsync(DbCredential cred, string label)
        {
            if (!cred.IsFilledIn()) return $"{label}: incomplete — fill in Host, Database, Username.";
            try
            {
                using var conn = new MySqlConnection(cred.ToConnectionString());
                await conn.OpenAsync();
                return $"{label}: connection successful.";
            }
            catch (Exception ex)
            {
                return $"{label}: failed — {ex.Message}";
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            _config.OwnDb.Lan = _lanOwnDb.ToCredential();
            _config.Crs.Lan = _lanCrs.ToCredential();
            _config.Ggms.Lan = _lanGgms.ToCredential();
            _config.OwnDb.Remote = _remoteOwnDb.ToCredential();

            _config.ActiveProfile = _tabs.SelectedIndex == 0
                ? ActiveNetworkProfile.Lan
                : ActiveNetworkProfile.Remote;

            _config.Save();
            MessageBox.Show("Connection settings saved.", "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    // One reusable block of five editable fields.
    internal class CredentialPanel : Panel
    {
        private readonly TextBox _txtHost, _txtPort, _txtDatabase, _txtUsername, _txtPassword;
        private readonly Button _btnPassword;
        private readonly TableLayoutPanel _fields;
        private readonly Panel _divider;
        private readonly Label _titleLabel;
        private static readonly Color NavBlue = Color.FromArgb(26, 35, 126);
        private static readonly Color BorderCol = Color.FromArgb(220, 225, 238);
        private static readonly Color TextGray = Color.FromArgb(108, 114, 140);

        public CredentialPanel(string title, DbCredential initial, int width = 500)
        {
            Width = width;
            Height = 250;
            Padding = new Padding(16, 12, 16, 12);
            BackColor = Color.White;
            ForeColor = NavBlue;
            Margin = new Padding(0, 0, 0, 12);
            Paint += PaintCard;

            _titleLabel = new Label { Text = title, Location = new Point(16, 12), Size = new Size(Math.Max(145, Width - 130), 24), AutoEllipsis = true, ForeColor = NavBlue, Font = new Font("Segoe UI", 9f, FontStyle.Bold), BackColor = Color.Transparent };
            Controls.Add(_titleLabel);
            string badgeText = title.IndexOf("CRS", StringComparison.OrdinalIgnoreCase) >= 0 ? "READ ONLY" : title.IndexOf("GGMS", StringComparison.OrdinalIgnoreCase) >= 0 ? "READ + WRITE" : "PRIMARY";
            Color badgeColor = badgeText == "READ ONLY" ? Color.FromArgb(180, 100, 0) : badgeText == "READ + WRITE" ? Color.FromArgb(15, 157, 88) : Color.FromArgb(100, 110, 128);
            var badge = new Label { Text = badgeText, AutoSize = true, Location = new Point(Width - 105, 13), Padding = new Padding(8, 3, 8, 3), BackColor = badgeColor, ForeColor = Color.White, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold) };
            badge.Resize += (s, e) => RoundControl(badge, 10);
            RoundControl(badge, 10);
            Controls.Add(badge);
            _divider = new Panel { Location = new Point(16, 40), Height = 1, Width = Width - 32, BackColor = BorderCol };
            Controls.Add(_divider);

            _fields = new TableLayoutPanel { Location = new Point(16, 48), Size = new Size(Width - 32, Height - 58), ColumnCount = 3, RowCount = 6, BackColor = Color.White, Padding = new Padding(2) };
            var t = _fields;
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 116));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34));
            for (int row = 0; row < 5; row++)
                t.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            t.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            t.Controls.Add(FieldLabel("Hostname/IP:"), 0, 0);
            t.Controls.Add(_txtHost = Field(initial.Host), 1, 0);

            t.Controls.Add(FieldLabel("Port:"), 0, 1);
            t.Controls.Add(_txtPort = Field(initial.Port), 1, 1);

            t.Controls.Add(FieldLabel("Database/Schema:"), 0, 2);
            t.Controls.Add(_txtDatabase = Field(initial.Database), 1, 2);

            t.Controls.Add(FieldLabel("Username:"), 0, 3);
            t.Controls.Add(_txtUsername = Field(initial.Username), 1, 3);

            t.Controls.Add(FieldLabel("Password:"), 0, 4);
            var passwordGroup = new Panel { Dock = DockStyle.Fill, Height = 28, Margin = new Padding(0, 2, 0, 2), BackColor = Color.White };
            _txtPassword = Field(initial.Password);
            _txtPassword.Location = new Point(0, 0);
            _txtPassword.Width = 250;
            passwordGroup.Controls.Add(_txtPassword);
            _txtPassword.PasswordChar = '●';
            _btnPassword = new Button { Width = 30, Height = 26, FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = Color.FromArgb(66, 133, 244), Cursor = Cursors.Hand, TabStop = false };
            _btnPassword.Location = new Point(_txtPassword.Right + 5, 0);
            _btnPassword.FlatAppearance.BorderColor = Color.FromArgb(220, 225, 238);
            _btnPassword.FlatAppearance.BorderSize = 1;
            RoundControl(_btnPassword, 8);
            var passwordTip = new ToolTip();
            passwordTip.SetToolTip(_btnPassword, "Show or hide password");
            _btnPassword.Paint += (s, e) => PaintPasswordIcon(e.Graphics, _txtPassword.PasswordChar == '\0');
            _btnPassword.Click += (s, e) => { _txtPassword.PasswordChar = _txtPassword.PasswordChar == '\0' ? '●' : '\0'; _btnPassword.Invalidate(); };
            passwordGroup.Controls.Add(_btnPassword);
            t.Controls.Add(passwordGroup, 1, 4);

            Controls.Add(t);
            Resize += (s, e) =>
            {
                _divider.Width = Math.Max(120, Width - 32);
                _fields.Width = Math.Max(120, Width - 32);
                _fields.Height = Math.Max(80, Height - 58);
            };
        }

        private static Label FieldLabel(string text) => new Label { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = TextGray, Font = new Font("Segoe UI", 8f) };

        public void SetTitle(string title)
        {
            _titleLabel.Text = title;
        }

        private void PaintPasswordIcon(Graphics graphics, bool isVisible)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(Color.FromArgb(66, 133, 244), 1.6f))
            {
                var eye = new Rectangle(7, 8, 15, 9);
                graphics.DrawArc(pen, eye, 180, 180);
                graphics.DrawArc(pen, eye, 0, 180);
                graphics.FillEllipse(Brushes.Transparent, eye);
                graphics.FillEllipse(pen.Brush, new Rectangle(13, 11, 4, 4));
                if (!isVisible)
                    graphics.DrawLine(pen, 5, 6, 24, 20);
            }
        }

        private void PaintCard(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var shadow = new SolidBrush(Color.FromArgb(22, 20, 35, 75))) e.Graphics.FillPath(shadow, RoundedPath(new Rectangle(2, 3, Width - 3, Height - 2), 12));
            using (var fill = new SolidBrush(Color.White)) e.Graphics.FillPath(fill, RoundedPath(new Rectangle(0, 0, Width - 3, Height - 4), 12));
            using (var border = new Pen(BorderCol)) e.Graphics.DrawPath(border, RoundedPath(new Rectangle(0, 0, Width - 3, Height - 4), 12));
            using (var accent = new SolidBrush(NavBlue)) e.Graphics.FillRectangle(accent, 0, 14, 4, 30);
        }

        private static GraphicsPath RoundedPath(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath(); int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90); path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90); path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90); path.CloseFigure(); return path;
        }

        private static void RoundControl(Control control, int radius)
        {
            if (control.Width < 1 || control.Height < 1) return;
            control.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, control.Width, control.Height, radius * 2, radius * 2));
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int left, int top, int right, int bottom, int width, int height);

        private static TextBox Field(string value)
        {
            var field = new TextBox { Width = 300, Height = 28, Text = value ?? "", BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White, ForeColor = Color.FromArgb(28, 30, 44), Margin = new Padding(0, 2, 0, 2) };
            field.Enter += (s, e) => field.BackColor = Color.FromArgb(246, 248, 255);
            field.Leave += (s, e) => field.BackColor = Color.White;
            return field;
        }

        public DbCredential ToCredential() => new DbCredential
        {
            Host = _txtHost.Text.Trim(),
            Port = string.IsNullOrWhiteSpace(_txtPort.Text) ? "3306" : _txtPort.Text.Trim(),
            Database = _txtDatabase.Text.Trim(),
            Username = _txtUsername.Text.Trim(),
            Password = _txtPassword.Text
        };

        public void LoadFrom(DbCredential cred)
        {
            _txtHost.Text = cred.Host;
            _txtPort.Text = cred.Port;
            _txtDatabase.Text = cred.Database;
            _txtUsername.Text = cred.Username;
            _txtPassword.Text = cred.Password;
        }
    }
}