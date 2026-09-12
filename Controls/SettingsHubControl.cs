using BarangayManagementSystem.Integration;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace BarangayManagementSystem.Controls
{
    public sealed class SettingsHubControl : UserControl
    {
        private static readonly Color NavBlue = Color.FromArgb(26, 35, 126);
        private static readonly Color BgPage = Color.FromArgb(240, 242, 248);
        private static readonly Color BorderCol = Color.FromArgb(220, 225, 238);
        private static readonly Color TextDark = Color.FromArgb(28, 30, 44);
        private static readonly Color TextGray = Color.FromArgb(108, 114, 140);
        private static readonly Color BtnGreen = Color.FromArgb(15, 157, 88);
        private static readonly Color BtnBlue = Color.FromArgb(66, 133, 244);
        private readonly TabControl _tabs = new TabControl();
        private readonly SystemProfileEditor _profile = new SystemProfileEditor();
        private readonly DatabaseEditor _database = new DatabaseEditor();
        private readonly UsersControl _users = new UsersControl();
        private readonly UserTransactionLogsControl _logs = new UserTransactionLogsControl();
        private readonly BackupForm _backup = new BackupForm();
        private readonly SegmentedSettingsTabs _segmentTabs = new SegmentedSettingsTabs(new[]
        {
            "Database connection", "System profile", "My account",
            "User management", "Backup", "User logs"
        });

        public static SystemProfileData LoadSystemProfile()
        {
            return ProfileStore.Load();
        }

        public SettingsHubControl()
        {
            Dock = DockStyle.Fill;
            BackColor = BgPage;
            BuildUi();
        }

        private void BuildUi()
        {
            var top = new Panel { Dock = DockStyle.Top, Height = 96, BackColor = BgPage, Padding = new Padding(20, 12, 20, 8) };
            var title = new Label { Text = "Settings", AutoSize = true, ForeColor = TextDark, Font = new Font("Segoe UI", 22f, FontStyle.Bold), Location = new Point(20, 10) };
            var subtitle = new Label { Text = "Manage system profile, account security, database connections, backups, and audit history.", AutoSize = true, ForeColor = TextGray, Font = new Font("Segoe UI", 9.5f), Location = new Point(22, 48) };
            var back = new Button { Text = "Back to Main Menu", Anchor = AnchorStyles.Top | AnchorStyles.Right, Size = new Size(170, 34), FlatStyle = FlatStyle.Flat, ForeColor = NavBlue, BackColor = Color.White, Font = new Font("Segoe UI", 9f, FontStyle.Bold), Cursor = Cursors.Hand };
            back.Location = new Point(top.Width - back.Width - 20, 24);
            back.FlatAppearance.BorderColor = BorderCol;
            back.FlatAppearance.MouseOverBackColor = Color.FromArgb(244, 246, 253);
            back.Click += (s, e) => FindForm()?.Close();
            top.Controls.Add(title);
            top.Controls.Add(subtitle);
            top.Controls.Add(back);
            top.Resize += (s, e) => back.Left = top.ClientSize.Width - back.Width - 20;
            Controls.Add(top);

            _tabs.Dock = DockStyle.Fill;
            _tabs.Padding = new Point(0, 0);
            _tabs.BackColor = BgPage;
            _tabs.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _tabs.Appearance = TabAppearance.FlatButtons;
            _tabs.ItemSize = new Size(1, 1);
            _tabs.SizeMode = TabSizeMode.Fixed;
            _tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            _tabs.DrawItem += (s, e) => { };
            _tabs.TabPages.Add(MakeTab("Database connection", _database));
            _tabs.TabPages.Add(MakeTab("System profile", _profile));
            _tabs.TabPages.Add(MakeTab("My account", new AccountEditor()));
            _tabs.TabPages.Add(MakeEmbeddedTab("User management", _users));
            _tabs.TabPages.Add(MakeEmbeddedTab("Backup", _backup));
            _tabs.TabPages.Add(MakeEmbeddedTab("User logs", _logs));
            Controls.Add(_tabs);
            _segmentTabs.Dock = DockStyle.Top;
            _segmentTabs.Height = 62;
            _segmentTabs.Margin = new Padding(18, 8, 18, 8);
            _segmentTabs.SelectedIndexChanged += (s, e) => _tabs.SelectedIndex = _segmentTabs.SelectedIndex;
            _tabs.SelectedIndexChanged += (s, e) => _segmentTabs.SelectedIndex = _tabs.SelectedIndex;
            Controls.Add(_segmentTabs);
            _segmentTabs.BringToFront();
            _tabs.BringToFront();
            _segmentTabs.BringToFront();
        }

        private static TabPage MakeTab(string title, Control content)
        {
            var page = new TabPage(title) { BackColor = BgPage, Padding = new Padding(18) };
            content.Dock = DockStyle.Fill;
            page.Controls.Add(content);
            return page;
        }

        private static TabPage MakeEmbeddedTab(string title, Control content)
        {
            var page = new TabPage(title) { BackColor = BgPage };
            content.Dock = DockStyle.Fill;
            page.Controls.Add(content);
            page.Enter += (s, e) =>
            {
                var users = content as UsersControl;
                if (users != null) users.RefreshData();
                var logs = content as UserTransactionLogsControl;
                if (logs != null) logs.RefreshData();
            };
            return page;
        }

        private static Button ActionButton(string text, Color color)
        {
            bool secondary = color == Color.White || color == Color.FromArgb(100, 110, 128);
            var button = new Button { Text = text, AutoSize = true, Height = 36, BackColor = secondary ? Color.White : color, ForeColor = secondary ? NavBlue : Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f, FontStyle.Bold), Padding = new Padding(14, 0, 14, 0), Cursor = Cursors.Hand };
            button.FlatAppearance.BorderColor = secondary ? BorderCol : color;
            button.FlatAppearance.BorderSize = secondary ? 1 : 0;
            button.FlatAppearance.MouseOverBackColor = secondary ? Color.FromArgb(245, 247, 253) : ControlPaint.Light(color, 0.12f);
            button.FlatAppearance.MouseDownBackColor = secondary ? Color.FromArgb(232, 236, 248) : ControlPaint.Dark(color, 0.1f);
            button.Resize += (s, e) => RoundControl(button, 10);
            RoundControl(button, 10);
            return button;
        }

        private static void RoundControl(Control control, int radius)
        {
            if (control.Width < 1 || control.Height < 1) return;
            control.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, control.Width, control.Height, radius * 2, radius * 2));
        }

        private static GraphicsPath RoundedPath(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static void StyleCard(Control card, bool accent = true)
        {
            card.BackColor = Color.White;
            card.Padding = new Padding(16);
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var shadow = new SolidBrush(Color.FromArgb(20, 20, 35, 75))) e.Graphics.FillPath(shadow, RoundedPath(new Rectangle(2, 3, card.Width - 3, card.Height - 2), 12));
                using (var fill = new SolidBrush(Color.White)) e.Graphics.FillPath(fill, RoundedPath(new Rectangle(0, 0, card.Width - 3, card.Height - 4), 12));
                using (var border = new Pen(BorderCol)) e.Graphics.DrawPath(border, RoundedPath(new Rectangle(0, 0, card.Width - 3, card.Height - 4), 12));
                if (accent) using (var marker = new SolidBrush(NavBlue)) e.Graphics.FillRectangle(marker, 0, 14, 4, Math.Min(42, card.Height - 28));
            };
            RoundControl(card, 12);
        }

        private static void StyleField(TextBox field)
        {
            field.BorderStyle = BorderStyle.FixedSingle;
            field.BackColor = Color.White;
            field.Enter += (s, e) => field.BackColor = Color.FromArgb(246, 248, 255);
            field.Leave += (s, e) => field.BackColor = Color.White;
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int left, int top, int right, int bottom, int width, int height);

        private sealed class SystemProfileEditor : UserControl
        {
            private readonly TextBox _title = Field();
            private readonly TextBox _municipality = Field();
            private readonly TextBox _tagline = Field();
            private readonly PictureBox _logo = Preview();
            private readonly PictureBox _background = Preview();
            private readonly PictureBox _topLogo = Preview();
            private readonly PictureBox _vendorLogo = Preview();
            private byte[] _logoBytes;
            private byte[] _topLogoBytes;
            private byte[] _vendorLogoBytes;
            private string _backgroundPath;

            public SystemProfileEditor()
            {
                BackColor = BgPage;
                var scroll = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(2), BackColor = BgPage };
                scroll.HorizontalScroll.Enabled = false;
                scroll.HorizontalScroll.Visible = false;
                scroll.Controls.Add(Header("Identity", "Keep the official system identity consistent across login and the main menu."));
                var identity = Card(700, 178);
                StyleCard(identity);
                AddField(identity, "System title", _title, 18, 18, 650);
                AddField(identity, "Municipality / barangay", _municipality, 18, 70, 650);
                AddField(identity, "Login screen tagline", _tagline, 18, 122, 650);
                scroll.Controls.Add(identity);
                scroll.Controls.Add(Header("Login branding", "Use the official seal and a clear left-panel image for the sign-in experience."));
                var branding = new TableLayoutPanel { Width = 700, Height = 350, ColumnCount = 2, RowCount = 2, BackColor = BgPage, Margin = new Padding(0, 0, 0, 16), Padding = new Padding(0) };
                branding.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); branding.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                branding.RowStyles.Add(new RowStyle(SizeType.Percent, 50)); branding.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
                branding.Controls.Add(UploadCard("Login seal / logo", _logo, 0), 0, 0);
                branding.Controls.Add(UploadCard("Left panel background", _background, 1), 1, 0);
                branding.Controls.Add(UploadCard("Top bar logo (left)", _topLogo, 2), 0, 1);
                branding.Controls.Add(UploadCard("Vendor logo (right)", _vendorLogo, 3), 1, 1);
                var save = ActionButton("SAVE SYSTEM PROFILE", BtnGreen); save.Location = new Point(2, 0); save.Click += Save; scroll.Controls.Add(branding); scroll.Controls.Add(save);
                Controls.Add(scroll);
                Resize += (s, e) =>
                {
                    int width = Math.Max(700, ClientSize.Width - 8);
                    identity.Width = width;
                    _title.Width = Math.Max(400, width - 36);
                    _municipality.Width = Math.Max(400, width - 36);
                    _tagline.Width = Math.Max(400, width - 36);
                    branding.Width = width;
                    branding.ColumnStyles[0].Width = 50;
                    branding.ColumnStyles[1].Width = 50;
                    foreach (Control item in branding.Controls)
                        item.Width = Math.Max(300, (width / 2) - 12);
                    save.Left = 2;
                };
                Load += (s, e) => LoadProfile();
            }

            private static TextBox Field() { var field = new TextBox { Height = 30, ForeColor = TextDark }; StyleField(field); return field; }
            private static PictureBox Preview() => new PictureBox { Size = new Size(104, 104), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.FromArgb(232, 237, 248), BorderStyle = BorderStyle.FixedSingle };
            private static Panel Card(int width, int height) => new Panel { Width = width, Height = height, BackColor = Color.White, Margin = new Padding(0, 0, 0, 16) };
            private static Label Header(string title, string subtitle) => new Label { Text = title + Environment.NewLine + subtitle, AutoSize = false, Width = 700, Height = 48, ForeColor = NavBlue, Font = new Font("Segoe UI", 10f, FontStyle.Bold), Margin = new Padding(0, 4, 0, 4) };
            private static void AddField(Control parent, string label, TextBox field, int x, int y, int width) { parent.Controls.Add(new Label { Text = label, Location = new Point(x, y), AutoSize = true, ForeColor = TextGray }); field.Location = new Point(x, y + 20); field.Width = width; parent.Controls.Add(field); }

            private Panel UploadCard(string title, PictureBox preview, int assetType)
            {
                var card = new Panel { Width = 338, Height = 164, BackColor = Color.White, Margin = new Padding(6) };
                StyleCard(card, false);
                AddPreview(card, title, preview, 14, 14, assetType);
                return card;
            }

            private void AddPreview(Panel card, string label, PictureBox preview, int x, int y, int assetType)
            {
                card.Controls.Add(preview); preview.Location = new Point(x, y);
                if (assetType != 1) RoundControl(preview, 52);
                card.Controls.Add(new Label { Text = assetType == 1 ? "RECTANGULAR PREVIEW" : "CIRCULAR PREVIEW", Location = new Point(x, y + 108), AutoSize = true, ForeColor = TextGray, Font = new Font("Segoe UI", 7.5f, FontStyle.Italic) });
                card.Controls.Add(new Label { Text = label, Location = new Point(x + 118, y + 4), AutoSize = true, ForeColor = NavBlue, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) });
                bool isBackground = assetType == 1;
                var upload = ActionButton(isBackground ? "UPLOAD BACKGROUND" : "UPLOAD LOGO", BtnBlue); upload.AutoSize = false; upload.Size = new Size(164, 34); upload.Font = new Font("Segoe UI", 8f, FontStyle.Bold); upload.Location = new Point(x + 118, y + 34); upload.Click += (s, e) => Upload(assetType); card.Controls.Add(upload);
                var remove = ActionButton(isBackground ? "RESET BACKGROUND" : "REMOVE LOGO", Color.FromArgb(100, 110, 128)); remove.AutoSize = false; remove.Size = new Size(164, 34); remove.Font = new Font("Segoe UI", 8f, FontStyle.Bold); remove.Location = new Point(x + 118, y + 76); remove.Click += (s, e) => RemoveAsset(assetType); card.Controls.Add(remove);
            }

            private void RemoveAsset(int assetType)
            {
                if (assetType == 0) { _logo.Image = null; _logoBytes = null; }
                else if (assetType == 1) { _background.Image = null; _backgroundPath = null; }
                else if (assetType == 2) { _topLogo.Image = null; _topLogoBytes = null; }
                else { _vendorLogo.Image = null; _vendorLogoBytes = null; }
            }

            private void LoadProfile()
            {
                var data = ProfileStore.Load(); _title.Text = data.SystemTitle; _municipality.Text = data.Municipality; _tagline.Text = data.Tagline; _backgroundPath = data.BackgroundPath;
                if (!string.IsNullOrWhiteSpace(data.LogoPath) && File.Exists(data.LogoPath)) _logo.Image = Image.FromFile(data.LogoPath);
                if (!string.IsNullOrWhiteSpace(data.BackgroundPath) && File.Exists(data.BackgroundPath)) _background.Image = Image.FromFile(data.BackgroundPath);
                if (!string.IsNullOrWhiteSpace(data.TopLogoPath) && File.Exists(data.TopLogoPath)) _topLogo.Image = Image.FromFile(data.TopLogoPath);
                if (!string.IsNullOrWhiteSpace(data.VendorLogoPath) && File.Exists(data.VendorLogoPath)) _vendorLogo.Image = Image.FromFile(data.VendorLogoPath);
            }

            private void Upload(int assetType)
            {
                using (var dialog = new OpenFileDialog { Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp" })
                {
                    if (dialog.ShowDialog() != DialogResult.OK) return;
                    using (var image = Image.FromFile(dialog.FileName))
                    {
                        using (var ms = new MemoryStream())
                        {
                            image.Save(ms, ImageFormat.Png);
                            if (assetType == 0) { _logo.Image = new Bitmap(image); _logoBytes = ms.ToArray(); }
                            else if (assetType == 1) { _background.Image = new Bitmap(image); _backgroundPath = dialog.FileName; }
                            else if (assetType == 2) { _topLogo.Image = new Bitmap(image); _topLogoBytes = ms.ToArray(); }
                            else { _vendorLogo.Image = new Bitmap(image); _vendorLogoBytes = ms.ToArray(); }
                        }
                    }
                }
            }

            private void Save(object sender, EventArgs e)
            {
                if (string.IsNullOrWhiteSpace(_title.Text) || string.IsNullOrWhiteSpace(_municipality.Text)) { MessageBox.Show("System title and municipality are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                var data = ProfileStore.Load(); data.SystemTitle = _title.Text.Trim(); data.Municipality = _municipality.Text.Trim(); data.Tagline = _tagline.Text.Trim();
                if (_logoBytes != null) { Directory.CreateDirectory(ProfileStore.AssetDirectory); data.LogoPath = Path.Combine(ProfileStore.AssetDirectory, "system-logo.png"); File.WriteAllBytes(data.LogoPath, _logoBytes); }
                if (_topLogoBytes != null) { Directory.CreateDirectory(ProfileStore.AssetDirectory); data.TopLogoPath = Path.Combine(ProfileStore.AssetDirectory, "top-logo.png"); File.WriteAllBytes(data.TopLogoPath, _topLogoBytes); }
                if (_vendorLogoBytes != null) { Directory.CreateDirectory(ProfileStore.AssetDirectory); data.VendorLogoPath = Path.Combine(ProfileStore.AssetDirectory, "vendor-logo.png"); File.WriteAllBytes(data.VendorLogoPath, _vendorLogoBytes); }
                data.BackgroundPath = _backgroundPath; ProfileStore.Save(data); MessageBox.Show("System profile saved.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private sealed class AccountEditor : UserControl
        {
            private readonly TextBox _newPassword = PasswordField();
            private readonly TextBox _confirmPassword = PasswordField();
            public AccountEditor()
            {
                BackColor = BgPage; var card = Card(700, 300); StyleCard(card); var avatar = new Label { Text = CurrentUser.Username.Length == 0 ? "?" : CurrentUser.Username.Substring(0, 1).ToUpperInvariant(), TextAlign = ContentAlignment.MiddleCenter, Location = new Point(20, 22), Size = new Size(64, 64), BackColor = NavBlue, ForeColor = Color.White, Font = new Font("Segoe UI", 24f, FontStyle.Bold) }; RoundControl(avatar, 32); card.Controls.Add(avatar);
                card.Controls.Add(new Label { Text = CurrentUser.Username, Location = new Point(100, 26), AutoSize = true, ForeColor = NavBlue, Font = new Font("Segoe UI", 13f, FontStyle.Bold) }); card.Controls.Add(new Label { Text = CurrentUser.Role, Location = new Point(100, 56), AutoSize = true, ForeColor = TextGray });
                AddField(card, "New password", _newPassword, 20, 112, 270); AddField(card, "Confirm new password", _confirmPassword, 20, 174, 270);
                AddPasswordToggle(card, _newPassword, 296, 132);
                AddPasswordToggle(card, _confirmPassword, 296, 194);
                var strength = new Panel { Location = new Point(20, 166), Size = new Size(270, 4), BackColor = Color.FromArgb(226, 229, 238) };
                var strengthFill = new Panel { Location = new Point(0, 0), Size = new Size(0, 4), BackColor = Color.FromArgb(192, 57, 43) };
                strength.Controls.Add(strengthFill); _newPassword.TextChanged += (s, e) => UpdateStrength(strengthFill, _newPassword.Text);
                card.Controls.Add(strength);
                var save = ActionButton("CHANGE PASSWORD", NavBlue); save.Location = new Point(20, 236); save.Click += ChangePassword; card.Controls.Add(save); Controls.Add(card);
                Resize += (s, e) => card.Width = Math.Max(700, ClientSize.Width - 8);
            }
            private static TextBox PasswordField() { var field = new TextBox { Height = 30, PasswordChar = '●' }; StyleField(field); return field; }
            private static void AddPasswordToggle(Control parent, TextBox field, int x, int y)
            {
                var toggle = new Button { Text = "SHOW", Location = new Point(x, y), Size = new Size(48, 28), FlatStyle = FlatStyle.Flat, BackColor = Color.White, ForeColor = BtnBlue, Cursor = Cursors.Hand, TabStop = false, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold) };
                toggle.FlatAppearance.BorderColor = BorderCol;
                toggle.FlatAppearance.BorderSize = 1;
                RoundControl(toggle, 8);
                new ToolTip().SetToolTip(toggle, "Show or hide password");
                toggle.Click += (s, e) => { bool show = field.PasswordChar != '\0'; field.PasswordChar = show ? '\0' : '●'; toggle.Text = show ? "HIDE" : "SHOW"; };
                parent.Controls.Add(toggle);
            }
            private static Panel Card(int width, int height) => new Panel { Width = width, Height = height, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Location = new Point(2, 2) };
            private static void AddField(Control parent, string label, TextBox field, int x, int y, int width) { parent.Controls.Add(new Label { Text = label, Location = new Point(x, y), AutoSize = true, ForeColor = TextGray }); field.Location = new Point(x, y + 20); field.Width = width; parent.Controls.Add(field); }
            private void ChangePassword(object sender, EventArgs e)
            {
                if (_newPassword.Text.Length < 6 || _newPassword.Text != _confirmPassword.Text) { MessageBox.Show("Use at least 6 matching characters.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (MessageBox.Show("Change your password now?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                using (var conn = DatabaseConnection.GetConnection()) using (var cmd = new System.Data.SQLite.SQLiteCommand("UPDATE users SET Password=MD5(@pw) WHERE Username=@user", conn)) { cmd.Parameters.AddWithValue("@pw", _newPassword.Text); cmd.Parameters.AddWithValue("@user", CurrentUser.Username); cmd.ExecuteNonQuery(); }
                _newPassword.Clear(); _confirmPassword.Clear(); MessageBox.Show("Password changed.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            private static void UpdateStrength(Control fill, string password)
            {
                int score = 0;
                if (password.Length >= 6) score++;
                if (password.Length >= 10) score++;
                if (System.Text.RegularExpressions.Regex.IsMatch(password, "[A-Z0-9]")) score++;
                fill.Width = Math.Min(300, score * 100);
                fill.BackColor = score <= 1 ? Color.FromArgb(192, 57, 43) : score == 2 ? Color.FromArgb(180, 100, 0) : BtnGreen;
            }
        }

        private sealed class DatabaseEditor : UserControl
        {
            private readonly ExternalDbConfig _config = ExternalDbConfig.Load();
            private readonly CredentialPanel _own;
            private readonly CredentialPanel _crs;
            private readonly CredentialPanel _ggms;
            private readonly Label _status = new Label { AutoSize = true, ForeColor = TextGray };
            private Button _lanPreset;
            private Button _onlinePreset;
            public DatabaseEditor()
            {
                BackColor = BgPage; var root = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(4, 8, 4, 16) };
                var presets = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Padding = new Padding(4), BackColor = Color.White }; _lanPreset = ActionButton("NETWORK (LAN)", Color.White); _onlinePreset = ActionButton("REMOTE (ONLINE)", Color.White); _lanPreset.Margin = new Padding(0); _onlinePreset.Margin = new Padding(-1, 0, 0, 0); _lanPreset.Click += (s, e) => ApplyPreset(ActiveNetworkProfile.Lan); _onlinePreset.Click += (s, e) => ApplyPreset(ActiveNetworkProfile.Remote); presets.Controls.Add(_lanPreset); presets.Controls.Add(_onlinePreset); root.Controls.Add(presets); root.Controls.Add(new Label { Text = "Offline mode uses the built-in local database automatically - no configuration needed.", AutoSize = true, ForeColor = TextGray, Margin = new Padding(4, 8, 0, 14) });
                _own = MakeCard("Own database", _config.ActiveOwnDb); _crs = MakeCard("CRS - read-only", _config.ActiveCrs); _ggms = MakeCard("GGMS - read + write", _config.ActiveGgms);
                var cards = new TableLayoutPanel { Width = 1200, Height = 310, ColumnCount = 3, RowCount = 1, BackColor = BgPage, Margin = new Padding(0, 0, 0, 12), Padding = new Padding(0) };
                cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f)); cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f)); cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));
                cards.Controls.Add(_own, 0, 0); cards.Controls.Add(_crs, 1, 0); cards.Controls.Add(_ggms, 2, 0);
                root.Controls.Add(cards);
                ApplyPreset(_config.ActiveProfile);
                var actions = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 4, 0, 0) }; var test = ActionButton("TEST CONNECTIONS", BtnBlue); var save = ActionButton("SAVE CONNECTION SETTINGS", BtnGreen); test.Click += Test; save.Click += Save; actions.Controls.Add(test); actions.Controls.Add(save); actions.Controls.Add(_status); root.Controls.Add(actions); Controls.Add(root);
                Resize += (s, e) =>
                {
                    int width = Math.Max(700, ClientSize.Width - 12);
                    cards.Width = width;
                    _own.Width = Math.Max(300, width / 3 - 8);
                    _crs.Width = Math.Max(300, width / 3 - 8);
                    _ggms.Width = Math.Max(300, width / 3 - 8);
                    cards.Height = 310;
                };
            }
            private CredentialPanel MakeCard(string title, DbCredential credential) { var card = new CredentialPanel(title, credential) { Width = 390, Height = 292, BackColor = Color.White, ForeColor = NavBlue, Margin = new Padding(6) }; return card; }
            private void ApplyPreset(ActiveNetworkProfile profile)
            {
                _config.ActiveProfile = profile;
                bool lan = profile == ActiveNetworkProfile.Lan;
                SetPresetState(_lanPreset, lan);
                SetPresetState(_onlinePreset, !lan);
                _status.Text = lan ? "LAN selected; local fallback is active." : "Online selected; review each database card before saving.";
            }

            private static void SetPresetState(Button button, bool active)
            {
                button.BackColor = active ? NavBlue : Color.White;
                button.ForeColor = active ? Color.White : NavBlue;
                button.FlatAppearance.BorderColor = active ? NavBlue : BorderCol;
            }
            private void Save(object sender, EventArgs e) { _config.OwnDb.Remote = _own.ToCredential(); _config.Crs.Remote = _crs.ToCredential(); _config.Ggms.Remote = _ggms.ToCredential(); _config.Save(); _status.Text = "Connection settings saved."; }
            private void Test(object sender, EventArgs e) { using (var conn = DatabaseConnection.GetConnection()) _status.Text = conn == null ? "Own database: disconnected." : "Own database: connected. CRS/GGMS are tested by their integration services."; }
        }

        private sealed class SegmentedSettingsTabs : UserControl
        {
            private readonly string[] _labels;
            private int _selectedIndex;
            private int _hoveredIndex = -1;

            public event EventHandler SelectedIndexChanged;

            public int SelectedIndex
            {
                get { return _selectedIndex; }
                set
                {
                    int next = Math.Max(0, Math.Min(_labels.Length - 1, value));
                    if (_selectedIndex == next) return;
                    _selectedIndex = next;
                    Invalidate();
                    SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            public SegmentedSettingsTabs(string[] labels)
            {
                _labels = labels ?? new string[0];
                DoubleBuffered = true;
                BackColor = BgPage;
                Cursor = Cursors.Hand;
                Padding = new Padding(18, 8, 18, 8);
                MouseMove += OnMouseMove;
                MouseLeave += (s, e) => { _hoveredIndex = -1; Invalidate(); };
                MouseClick += OnMouseClick;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var shell = new Rectangle(Padding.Left, Padding.Top, Math.Max(1, ClientSize.Width - Padding.Horizontal), Math.Max(1, ClientSize.Height - Padding.Vertical));
                using (var shellPath = RoundedPath(shell, shell.Height / 2))
                using (var shellBrush = new SolidBrush(Color.White)) e.Graphics.FillPath(shellBrush, shellPath);
                using (var shellPath = RoundedPath(shell, shell.Height / 2))
                using (var shellPen = new Pen(BorderCol)) e.Graphics.DrawPath(shellPen, shellPath);

                int segmentWidth = Math.Max(1, shell.Width / Math.Max(1, _labels.Length));
                for (int i = 0; i < _labels.Length; i++)
                {
                    var bounds = new Rectangle(shell.X + i * segmentWidth + 3, shell.Y + 3, segmentWidth - 6, shell.Height - 6);
                    bool active = i == _selectedIndex;
                    bool hovered = i == _hoveredIndex && !active;
                    if (active)
                    {
                        using (var shadow = new SolidBrush(Color.FromArgb(24, 20, 35, 75))) e.Graphics.FillPath(shadow, RoundedPath(new Rectangle(bounds.X + 1, bounds.Y + 2, bounds.Width, bounds.Height), 18));
                    }
                    using (var path = RoundedPath(bounds, 18))
                    using (var brush = new SolidBrush(active ? NavBlue : hovered ? Color.FromArgb(244, 246, 253) : Color.Transparent)) e.Graphics.FillPath(brush, path);
                    TextRenderer.DrawText(e.Graphics, _labels[i], new Font("Segoe UI", 8.5f, active ? FontStyle.Bold : FontStyle.Regular), bounds, active ? Color.White : TextGray, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                }
            }

            private void OnMouseMove(object sender, MouseEventArgs e)
            {
                int index = HitIndex(e.Location);
                if (index != _hoveredIndex) { _hoveredIndex = index; Invalidate(); }
            }

            private void OnMouseClick(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left) SelectedIndex = HitIndex(e.Location);
            }

            private int HitIndex(Point point)
            {
                var shell = new Rectangle(Padding.Left, Padding.Top, Math.Max(1, ClientSize.Width - Padding.Horizontal), Math.Max(1, ClientSize.Height - Padding.Vertical));
                if (!shell.Contains(point)) return -1;
                return Math.Min(_labels.Length - 1, Math.Max(0, (point.X - shell.X) / Math.Max(1, shell.Width / Math.Max(1, _labels.Length))));
            }
        }

        private static class ProfileStore
        {
            public static readonly string AssetDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BarangaySystem", "branding");
            private static readonly string FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BarangaySystem", "system_profile.json");
            public static SystemProfileData Load() { try { return File.Exists(FilePath) ? JsonSerializer.Deserialize<SystemProfileData>(File.ReadAllText(FilePath)) ?? new SystemProfileData() : new SystemProfileData(); } catch { return new SystemProfileData(); } }
            public static void Save(SystemProfileData data) { Directory.CreateDirectory(Path.GetDirectoryName(FilePath)); File.WriteAllText(FilePath, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true })); }
        }
        public sealed class SystemProfileData { public string SystemTitle { get; set; } = "Barangay Management System"; public string Municipality { get; set; } = ""; public string Tagline { get; set; } = "eTechTugon Software Solution"; public string LogoPath { get; set; } = ""; public string BackgroundPath { get; set; } = ""; public string TopLogoPath { get; set; } = ""; public string VendorLogoPath { get; set; } = ""; }
    }
}