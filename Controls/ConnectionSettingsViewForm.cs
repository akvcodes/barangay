using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySqlConnector;

namespace BarangayManagementSystem.Integration
{
    // Login-safe database selector. Credentials remain editable in the admin editor.
    public class ConnectionSettingsViewForm : Form
    {
        private static readonly Color Canvas = Color.FromArgb(246, 244, 239);
        private static readonly Color Ink = Color.FromArgb(45, 49, 58);
        private static readonly Color Muted = Color.FromArgb(111, 113, 119);
        private static readonly Color Border = Color.FromArgb(218, 216, 211);
        private static readonly Color Navy = Color.FromArgb(24, 36, 57);
        private static readonly Color Gold = Color.FromArgb(209, 177, 111);
        private static readonly Color Blue = Color.FromArgb(40, 92, 143);

        private readonly ExternalDbConfig _config;
        private string _mode;
        private Button _localButton, _lanButton, _remoteButton;
        private Panel _content;
        private Label _status;

        public ConnectionSettingsViewForm()
        {
            _config = ExternalDbConfig.Load();
            _mode = _config.ActiveProfile == ActiveNetworkProfile.Lan ? "LAN" : "REMOTE";
            BuildUi();
            RenderMode();
        }

        private void BuildUi()
        {
            Text = "App Database Settings";
            ClientSize = new Size(760, 650);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            BackColor = Canvas;

            var root = new Panel { Dock = DockStyle.Fill };
            Controls.Add(root);
            root.Controls.Add(new Label { Text = "App Database Settings", AutoSize = true, Font = new Font("Segoe UI", 19f, FontStyle.Bold), ForeColor = Ink, Location = new Point(28, 24) });
            root.Controls.Add(new Label { Text = "Choose which app database is active for login, startup, dashboard data, and snapshots. Network (LAN) can be configured here.", AutoSize = false, Width = 690, Height = 34, Font = new Font("Segoe UI", 9f), ForeColor = Muted, Location = new Point(30, 60) });
            root.Controls.Add(new Label { Text = "APP DATABASE MODE", AutoSize = true, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = Muted, Location = new Point(30, 104) });

            _localButton = ModeButton("[ ]  LOCAL", new Point(30, 124));
            _lanButton = ModeButton("[+]  NETWORK LAN", new Point(258, 124));
            _remoteButton = ModeButton("[~]  REMOTE", new Point(486, 124));
            _localButton.Click += (s, e) => SelectMode("LOCAL");
            _lanButton.Click += (s, e) => SelectMode("LAN");
            _remoteButton.Click += (s, e) => SelectMode("REMOTE");
            root.Controls.Add(_localButton); root.Controls.Add(_lanButton); root.Controls.Add(_remoteButton);

            _content = new Panel { Location = new Point(30, 180), Size = new Size(690, 335), BackColor = Color.White };
            _content.Paint += (s, e) => { using (var pen = new Pen(Border)) e.Graphics.DrawRectangle(pen, 0, 0, _content.Width - 1, _content.Height - 1); };
            root.Controls.Add(_content);

            _status = new Label { AutoSize = false, Location = new Point(30, 530), Size = new Size(690, 42), Padding = new Padding(14, 12, 10, 8), Font = new Font("Segoe UI", 8.5f), ForeColor = Muted, BackColor = Color.White };
            root.Controls.Add(_status);

            var cancel = ActionButton("CANCEL", false, new Point(390, 585), 86);
            cancel.FlatAppearance.BorderSize = 0;
            cancel.Click += (s, e) => Close();
            root.Controls.Add(cancel);
            var test = ActionButton("TEST CONNECTION", false, new Point(486, 585), 136);
            test.Click += async (s, e) => await TestCurrentAsync();
            root.Controls.Add(test);
            var save = ActionButton("SAVE AND CONTINUE", true, new Point(630, 585), 120);
            save.Click += (s, e) => SaveSelection();
            root.Controls.Add(save);
        }

        private Button ModeButton(string text, Point location)
        {
            var button = new Button { Text = text, Location = location, Size = new Size(210, 54), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f, FontStyle.Bold), ForeColor = Ink, BackColor = Color.White };
            button.FlatAppearance.BorderColor = Border;
            button.FlatAppearance.BorderSize = 1;
            return button;
        }

        private Button ActionButton(string text, bool primary, Point location, int width)
        {
            var button = new Button { Text = text, Location = location, Size = new Size(width, 36), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8f, FontStyle.Bold), BackColor = primary ? Blue : Color.White, ForeColor = primary ? Color.White : Ink };
            button.FlatAppearance.BorderColor = primary ? Blue : Border;
            button.FlatAppearance.BorderSize = 1;
            return button;
        }

        private void SelectMode(string mode)
        {
            _mode = mode;
            RenderMode();
        }

        private void RenderMode()
        {
            StyleMode(_localButton, _mode == "LOCAL");
            StyleMode(_lanButton, _mode == "LAN");
            StyleMode(_remoteButton, _mode == "REMOTE");
            _content.Controls.Clear();
            string heading = _mode == "LAN" ? "Network (LAN)" : (_mode == "LOCAL" ? "Local" : "Remote");
            AddLabel(heading, new Point(22, 20), 12f, true, Ink, 0);
            AddLabel("The selected app database is what EF Core, startup initialization, and snapshot tools will use.", new Point(22, 48), 8f, false, Muted, 460, 42);
            AddLabel("[check]  ACTIVE PRESET\n" + heading, new Point(532, 30), 8f, true, Color.White, 0, 0, new Size(136, 48), Navy);

            if (_mode == "LAN") BuildLanList();
            else AddLabel("Fixed preset\n\n" + heading + " is selection-only before sign-in. Switch to Network (LAN) if you need to configure connection details here.", new Point(22, 116), 8.5f, false, Muted, 646, 104, new Size(646, 104), Color.FromArgb(250, 249, 247));

            _status.Text = "[plug]  Connection status\n" + (_mode == "LAN" ? "Network (LAN) app database selected. Test the connection, then save and continue." : heading + " app database selected. Switch to Network (LAN) to edit connection details.");
        }

        private void AddLabel(string text, Point location, float size, bool bold, Color color, int width, int height = 0, Size? forcedSize = null, Color? backColor = null)
        {
            var label = new Label { Text = text, Location = location, AutoSize = forcedSize == null && height == 0, Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular), ForeColor = color, BackColor = backColor ?? Color.Transparent };
            if (forcedSize.HasValue) label.Size = forcedSize.Value;
            else if (width > 0) { label.Width = width; label.Height = height; }
            _content.Controls.Add(label);
        }

        private void BuildLanList()
        {
            AddLabel("AVAILABLE NETWORK LAN DATABASES", new Point(22, 104), 7.5f, true, Muted, 0);
            AddDatabaseRow("Network (LAN)", _config.OwnDb.Lan, 132, true, "Primary application database");
            AddDatabaseRow("Network LAN (2)", _config.Crs.Lan, 192, false, "CRS - read-only");
            AddDatabaseRow("Network LAN (3)", _config.Ggms.Lan, 252, false, "GGMS - read + write");
        }

        private void AddDatabaseRow(string name, DbCredential credential, int y, bool active, string role)
        {
            var row = new Panel { Location = new Point(22, y), Size = new Size(646, 52), BackColor = active ? Color.FromArgb(246, 249, 255) : Color.White, BorderStyle = BorderStyle.FixedSingle };
            row.Controls.Add(new Label { Text = name, AutoSize = true, Location = new Point(14, 7), Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), ForeColor = Ink });
            row.Controls.Add(new Label { Text = (string.IsNullOrWhiteSpace(credential.Host) ? "Not configured" : credential.Host + ":" + credential.Port + " / " + credential.Database) + "\n" + role, AutoSize = true, Location = new Point(14, 25), Font = new Font("Segoe UI", 7.5f), ForeColor = Muted });
            var test = new Button { Text = "TEST", Size = new Size(48, 27), Location = new Point(470, 12), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), BackColor = Color.White, ForeColor = Ink };
            test.FlatAppearance.BorderColor = Border;
            test.Click += async (s, e) => await TestCredentialAsync(credential, name);
            row.Controls.Add(test);
            row.Controls.Add(new Label { Text = active ? "Active" : "", AutoSize = true, Location = new Point(528, 18), Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(35, 125, 87) });
            _content.Controls.Add(row);
        }

        private void StyleMode(Button button, bool active)
        {
            button.BackColor = active ? Color.FromArgb(232, 220, 194) : Color.White;
            button.FlatAppearance.BorderColor = active ? Gold : Border;
        }

        private void SaveSelection()
        {
            _config.ActiveProfile = _mode == "REMOTE" ? ActiveNetworkProfile.Remote : ActiveNetworkProfile.Lan;
            _config.Save();
            DialogResult = DialogResult.OK;
            Close();
        }

        private async Task TestCurrentAsync()
        {
            if (_mode == "LAN") await TestCredentialAsync(_config.ActiveOwnDb, "Network (LAN)");
            else MessageBox.Show("This preset is selection-only before sign-in. Switch to Network (LAN) to test server details.", "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task TestCredentialAsync(DbCredential credential, string name)
        {
            try
            {
                if (!credential.IsFilledIn()) throw new InvalidOperationException("Not configured");
                using (var connection = new MySqlConnection(credential.ToConnectionString())) await connection.OpenAsync();
                MessageBox.Show(name + ": connection successful.", "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(name + ": connection failed.\n" + ex.Message, "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
