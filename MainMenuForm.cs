using BarangayManagementSystem.Controls;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace BarangayManagementSystem
{
    public partial class MainMenuForm : Form
    {
        private bool _dragging;
        private Point _dragStart;
        private Label _connectionModeLabel;
        private Panel _connectionStatusDot;
        private Label _clockPulse;
        private Panel _clockBadge;
        private Label _brandMunicipality;
        private FlowLayoutPanel _bottomStatusFlow;
        private Label _profileBadge;
        private Button _btnRefresh;

        private static readonly string CONFIG_FILE = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BarangaySystem", "config.txt");

        public MainMenuForm()
        {
            InitializeComponent();
        }

        private void MainMenuForm_Load(object sender, EventArgs e)
        {
            ConfigureOverhauledLayout();
            CurrentUser.LoadProfile();
            LoadLiveBranding();
            lblSuperAdmin.Text = $"{CurrentUser.Username.ToLower()} · {CurrentUser.Role}";
            BuildProfileBadge();
            LayoutTopBar();

            // ── Fix PictureBox white borders ──────────────────────────────────
            RemovePictureBorders();

            // ── Wire up all tiles ─────────────────────────────────────────────
            // Treasurer only gets Resident Management + Logout; every other
            // tile is locked (grayed, popup on click) instead of hidden, so
            // the grid still looks intact. DB Status is a special case — its
            // color is owned by RefreshDbStatus(), so it keeps its live
            // green/red/amber status color but its click is still blocked.
            bool isTreasurer = CurrentUser.IsTreasurer();

            if (isTreasurer)
                LockTile(tileDashboard, picDashboard, lblDashboard, lblDashboardSub);
            else
                WireTile(tileDashboard, picDashboard, lblDashboard, lblDashboardSub,
                    Color.FromArgb(30, 58, 138),
                    () => OpenControl(new DashboardControl(), "Analytics"));

            WireTile(tileResidents, picResidents, lblResidents, lblResidentsSub,
                Color.FromArgb(15, 118, 110),
                () => OpenControl(new ResidentManagementControl(), "Residents"));

            if (isTreasurer)
                LockTile(tileClearance, picClearance, lblClearance, lblClearanceSub);
            else
                WireTile(tileClearance, picClearance, lblClearance, lblClearanceSub,
                    Color.FromArgb(26, 35, 126),
                    () => OpenControl(new SettingsHubControl(), "Settings"));

            if (isTreasurer)
                LockTile(tileOfficials, picOfficials, lblOfficials, lblOfficialsSub);
            else
                WireTile(tileOfficials, picOfficials, lblOfficials, lblOfficialsSub,
                    Color.FromArgb(37, 99, 235),
                    () => OpenControl(new OfficialsControl(), "Officials"));

            if (isTreasurer)
                LockTile(tileBackup, picBackup, lblBackup, lblBackupSub);
            else
                WireTile(tileBackup, picBackup, lblBackup, lblBackupSub,
                    Color.FromArgb(30, 41, 59),
                    () =>
                    {
                        AuditLogger.Log("VIEW", "System", "Opened Database Backup");
                        OpenControl(new BackupForm(), "Database Backup");
                    });

            if (isTreasurer)
                LockTile(tileAyuda, picAyuda, lblAyuda, lblAyudaSub);
            else
                WireTile(tileAyuda, picAyuda, lblAyuda, lblAyudaSub,
                    Color.FromArgb(146, 64, 14),
                    () =>
                    {
                        AuditLogger.Log("VIEW", "Ayuda", "Opened Quick Ayuda");
                        new QuickAyudaReleaseForm().ShowDialog();
                    });

            if (isTreasurer)
                LockTile(tileBudget, picBudget, lblBudget, lblBudgetSub);
            else
                WireTile(tileBudget, picBudget, lblBudget, lblBudgetSub,
                    Color.FromArgb(161, 98, 7),
                    () => OpenControl(new BudgetTransparencyControl(), "Budget Transparency"));

            WireTile(tileDbStatus, null, lblDbStatus, null,
                Color.FromArgb(20, 83, 45),
                isTreasurer
                    ? (Action)(() => MessageBox.Show(
                        "This section is restricted to Super Admin access only.",
                        "Access Restricted", MessageBoxButtons.OK, MessageBoxIcon.Information))
                    : (() =>
                    {
                        AuditLogger.Log("VIEW", "System", "Opened Connection Setup");
                        new Integration.ConnectionSettingsEditForm().ShowDialog(this);
                        RefreshDbStatus();
                    }));

            if (CurrentUser.IsSuperAdmin() || CurrentUser.IsAdmin())
            {
                WireTile(tileUserMgmt, picUserMgmt, lblUserMgmt, lblUserMgmtSub,
                    Color.FromArgb(109, 40, 217),
                    () => OpenControl(new UsersControl(), "User Management"));
            }
            else
            {
                tileUserMgmt.Visible = false;
            }

            if (CurrentUser.IsSuperAdmin())
            {
                WireTile(tileUserLogs, picUserLogs, lblUserLogs, lblUserLogsSub,
                    Color.FromArgb(30, 41, 59),
                    () => OpenControl(new UserTransactionLogsControl(), "User Transaction Logs"));
            }
            else
            {
                tileUserLogs.Visible = false;
            }

            lblLogout.Text = "EXIT";
            lblLogoutSub.Text = "Exit program";
            WireTile(tileLogout, picLogout, lblLogout, lblLogoutSub,
                Color.FromArgb(153, 27, 27), DoLogout);

            // ── Rounded corners ───────────────────────────────────────────────
            ApplyRound(panelDbDot, 10);
            ApplyRound(tileDashboard, 12);
            ApplyRound(tileResidents, 12);
            ApplyRound(tileClearance, 12);
            ApplyRound(tileBrand, 12);
            ApplyRound(tileDate, 12);
            ApplyRound(tileDbStatus, 12);
            ApplyRound(tileOfficials, 12);
            ApplyRound(tileBackup, 12);
            ApplyRound(tileAyuda, 12);
            ApplyRound(tileBudget, 12);
            ApplyRound(tileUserMgmt, 12);
            ApplyRound(tileUserLogs, 12);
            ApplyRound(tileLogout, 12);
            ApplyRound(picBrandLogo, 8);
            ApplyRound(picNotifBell, 18);
            picNotifBell.BackColor = Color.FromArgb(42, 255, 255, 255);
            tableGrid.Paint += TableGrid_Paint;

            RefreshDbStatus();

            lblWelcomeUser.Text = $"Welcome, {CurrentUser.Username}  ·  {CurrentUser.Role}";

            timerClock_Tick(null, null);
            timerClock.Start();
        }

        private void ConfigureOverhauledLayout()
        {
            tableGrid.SuspendLayout();
            tableGrid.Controls.Clear();
            tableGrid.ColumnCount = 3;
            tableGrid.RowCount = 4;
            tableGrid.ColumnStyles.Clear();
            tableGrid.RowStyles.Clear();
            tableGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 31f));
            tableGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 31f));
            tableGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38f));
            for (int i = 0; i < 4; i++) tableGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));

            tableGrid.SetColumnSpan(tileBrand, 1);
            tableGrid.SetColumnSpan(tileDate, 1);

            AddGridTile(tileDashboard, 0, 0);
            AddGridTile(tileResidents, 1, 0);
            AddGridTile(tileOfficials, 0, 1);
            AddGridTile(tileClearance, 1, 1);
            AddGridTile(tileAyuda, 0, 2);
            AddGridTile(tileBudget, 1, 2);
            AddGridTile(tileDate, 0, 3);
            AddGridTile(tileLogout, 1, 3);

            tileBrand.Visible = true;
            tileBrand.BackColor = Color.White;
            tileBrand.Margin = new Padding(8);
            tableGrid.Controls.Add(tileBrand, 2, 0);
            tableGrid.SetRowSpan(tileBrand, 4);
            lblBrandName.Text = "Barangay Management System";
            lblBrandSub.Text = "eTechTugon Software Solution";
            lblBrandName.ForeColor = Color.FromArgb(26, 35, 126);
            lblBrandSub.ForeColor = Color.FromArgb(108, 114, 140);
            lblSuperAdmin.ForeColor = Color.FromArgb(108, 114, 140);
            lblClearance.Text = "SETTINGS";
            lblClearanceSub.Text = "Profile, database, users";
            FitTile(tileDashboard, picDashboard, lblDashboard, lblDashboardSub);
            FitTile(tileResidents, picResidents, lblResidents, lblResidentsSub);
            FitTile(tileOfficials, picOfficials, lblOfficials, lblOfficialsSub);
            FitTile(tileClearance, picClearance, lblClearance, lblClearanceSub);
            FitTile(tileAyuda, picAyuda, lblAyuda, lblAyudaSub);
            FitTile(tileBudget, picBudget, lblBudget, lblBudgetSub);
            FitTile(tileLogout, picLogout, lblLogout, lblLogoutSub);
            tileBrand.Resize += (s, e) => LayoutBrandPanel();
            LayoutBrandPanel();
            _connectionStatusDot = new Panel { Size = new Size(9, 9), BackColor = Color.Gray, Cursor = Cursors.Hand };
            _connectionModeLabel = new Label { AutoSize = true, ForeColor = Color.FromArgb(108, 114, 140), Font = new Font("Segoe UI", 9f), Cursor = Cursors.Hand };
            _clockPulse = new Label { Text = "●", AutoSize = true, ForeColor = Color.FromArgb(74, 222, 128), BackColor = Color.Transparent, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            _clockBadge = new Panel { Size = new Size(46, 46), BackColor = Color.FromArgb(54, 76, 164) };
            _clockBadge.Paint += ClockBadge_Paint;
            _brandMunicipality = new Label { AutoSize = false, ForeColor = Color.FromArgb(26, 35, 126), BackColor = Color.Transparent, Font = new Font("Segoe UI", 10f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
            tileBrand.Controls.Add(_brandMunicipality);
            tileBrand.Paint += BrandPanel_Paint;
            tileDate.Paint += DateTile_Paint;
            _bottomStatusFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(14, 5, 0, 0),
                BackColor = Color.Transparent,
                Margin = Padding.Empty
            };
            lblBottomPage.Margin = new Padding(0, 0, 20, 0);
            _connectionStatusDot.Margin = new Padding(0, 5, 7, 0);
            _connectionModeLabel.Margin = Padding.Empty;
            _bottomStatusFlow.Controls.Add(lblBottomPage);
            _bottomStatusFlow.Controls.Add(_connectionStatusDot);
            _bottomStatusFlow.Controls.Add(_connectionModeLabel);
            panelBottomBar.Controls.Add(_bottomStatusFlow);
            _btnRefresh = new Button
            {
                Text = "REFRESH",
                Size = new Size(86, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(26, 35, 126),
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(190, 198, 225);
            _btnRefresh.Click += (s, e) => RefreshMainMenu();
            panelBottomBar.Controls.Add(_btnRefresh);
            tileDate.Controls.Add(_clockPulse);
            tileDate.Controls.Add(_clockBadge);
            tileDate.Resize += (s, e) => LayoutDateTile();
            LayoutDateTile();
            EventHandler openSettings = (s, e) => OpenControl(new SettingsHubControl(), "Settings");
            panelBottomBar.Click += openSettings;
            _connectionStatusDot.Click += openSettings;
            _connectionModeLabel.Click += openSettings;
            _bottomStatusFlow.Click += openSettings;
            panelTopBar.Resize += (s, e) => LayoutTopBar();
            panelBottomBar.Resize += (s, e) => LayoutBottomRefresh();
            LayoutBottomRefresh();
            tileDbStatus.Visible = false;
            tileBackup.Visible = false;
            tileUserMgmt.Visible = false;
            tileUserLogs.Visible = false;
            tableGrid.ResumeLayout(true);
        }

        private void AddGridTile(Control tile, int column, int row)
        {
            tile.Visible = true;
            tile.Margin = new Padding(10);
            tableGrid.Controls.Add(tile, column, row);
        }

        private void FitTile(Panel tile, PictureBox icon, Label title, Label subtitle)
        {
            tile.Resize += (s, e) =>
            {
                int width = tile.ClientSize.Width;
                icon.Size = new Size(68, 68);
                icon.Location = new Point((width - icon.Width) / 2, 18);
                title.Location = new Point(8, 94);
                title.Size = new Size(width - 10, 30);
                subtitle.Location = new Point(8, 126);
                subtitle.Size = new Size(width - 10, 24);
                ApplyRound(tile, 14);
            };
        }

        private void LayoutBrandPanel()
        {
            int width = tileBrand.ClientSize.Width;
            picBrandLogo.Size = new Size(76, 76);
            picBrandLogo.Location = new Point((width - picBrandLogo.Width) / 2, 34);
            lblBrandName.Location = new Point(18, 122);
            lblBrandName.Size = new Size(width - 36, 58);
            lblBrandName.TextAlign = ContentAlignment.MiddleCenter;
            lblBrandName.Font = new Font("Segoe UI", 15f, FontStyle.Bold);
            lblBrandSub.Location = new Point(18, 184);
            lblBrandSub.Size = new Size(width - 36, 28);
            lblBrandSub.TextAlign = ContentAlignment.MiddleCenter;
            lblSuperAdmin.Location = new Point(18, 222);
            lblSuperAdmin.Size = new Size(width - 36, 26);
            lblSuperAdmin.TextAlign = ContentAlignment.MiddleCenter;
            if (_brandMunicipality != null)
            {
                _brandMunicipality.Location = new Point(18, 250);
                _brandMunicipality.Size = new Size(width - 36, 28);
            }
        }

        private void LayoutDateTile()
        {
            if (_clockBadge == null) return;
            _clockBadge.Location = new Point(tileDate.ClientSize.Width - 66, 18);
            lblDateDay.Location = new Point(22, 18);
            lblDateDay.Size = new Size(tileDate.ClientSize.Width - 102, 30);
            lblDateDay.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            lblDateRest.Location = new Point(22, 64);
            lblDateRest.Size = new Size(tileDate.ClientSize.Width - 44, 42);
            lblDateRest.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
            lblDateTime.Location = new Point(22, 112);
            lblDateTime.Size = new Size(tileDate.ClientSize.Width - 44, 30);
            lblDateTime.Font = new Font("Segoe UI", 11f);
            _clockPulse.Location = new Point(tileDate.ClientSize.Width - 34, 122);
        }

        private void BuildProfileBadge()
        {
            string username = CurrentUser.Username ?? "User";
            string initials = username.Trim().Length == 0
                ? "U"
                : username.Trim().Substring(0, Math.Min(2, username.Trim().Length)).ToUpperInvariant();

            _profileBadge = new Label
            {
                Text = CurrentUser.ProfilePhoto == null ? initials : string.Empty,
                Size = new Size(40, 40),
                BackColor = Color.FromArgb(224, 231, 255),
                ForeColor = Color.FromArgb(26, 35, 126),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackgroundImageLayout = ImageLayout.Zoom,
                Cursor = Cursors.Hand,
                TabStop = false
            };
            if (CurrentUser.ProfilePhoto != null)
            {
                using (var stream = new MemoryStream(CurrentUser.ProfilePhoto))
                using (var image = Image.FromStream(stream))
                    _profileBadge.BackgroundImage = new Bitmap(image);
            }
            _profileBadge.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 40, 40, 40, 40));
            var profileMenu = new ContextMenuStrip();
            profileMenu.Items.Add("Edit Profile", null, (s, e) =>
            {
                var settings = new SettingsHubControl();
                settings.ShowMyAccount();
                OpenControl(settings, "My account");
                RefreshMainMenu();
            });
            _profileBadge.Click += (s, e) => profileMenu.Show(_profileBadge, new Point(0, _profileBadge.Height));
            panelTopBar.Controls.Add(_profileBadge);
        }

        private void UpdateProfileBadge()
        {
            if (_profileBadge == null) return;
            _profileBadge.BackgroundImage?.Dispose();
            _profileBadge.BackgroundImage = null;
            string username = CurrentUser.Username ?? "User";
            _profileBadge.Text = CurrentUser.ProfilePhoto == null
                ? (username.Trim().Length == 0 ? "U" : username.Trim().Substring(0, Math.Min(2, username.Trim().Length)).ToUpperInvariant())
                : string.Empty;
            if (CurrentUser.ProfilePhoto != null)
            {
                using (var stream = new MemoryStream(CurrentUser.ProfilePhoto))
                using (var image = Image.FromStream(stream))
                    _profileBadge.BackgroundImage = new Bitmap(image);
            }
            _profileBadge.Invalidate();
        }

        private void LayoutBottomRefresh()
        {
            if (_btnRefresh == null) return;
            int rightEdge = lblBottomRight != null && lblBottomRight.Left > 0
                ? lblBottomRight.Left - 10
                : panelBottomBar.ClientSize.Width - 10;
            _btnRefresh.Size = new Size(86, 24);
            _btnRefresh.Location = new Point(Math.Max(0, rightEdge - _btnRefresh.Width), 3);
        }

        private void RefreshMainMenu()
        {
            CurrentUser.LoadProfile();
            UpdateProfileBadge();
            LoadLiveBranding();
            RefreshDbStatus();
            RefreshNotifBadge();
            timerClock_Tick(null, null);
        }

        private void LayoutTopBar()
        {
            int left = picLogoTop.Right + 12;
            lblSystemTitle.AutoSize = true;
            lblSystemTitle.Location = new Point(left, 15);
            lblSystemTitle.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            lblSystemTitle.BringToFront();

            lblSystemSub.AutoSize = true;
            lblSystemSub.Location = new Point(lblSystemTitle.Right + 14, 22);
            lblSystemSub.BringToFront();

            if (_profileBadge != null)
            {
                _profileBadge.Location = new Point(panelTopBar.ClientSize.Width - 62, 18);
                _profileBadge.BringToFront();
                picNotifBell.Location = new Point(panelTopBar.ClientSize.Width - 112, 17);
                int rightEdge = picNotifBell.Left - 12;
                int logoWidth = 150;
                int logoLeft = Math.Max(lblSystemSub.Right + 20, rightEdge - 360);
                pictureBox1.Location = new Point(logoLeft, 4);
                pictureBox1.Size = new Size(logoWidth, 64);
                pictureBox1.BringToFront();
                lblWelcomeUser.Location = new Point(logoLeft + logoWidth + 12, 24);
                lblWelcomeUser.Width = Math.Max(120, rightEdge - lblWelcomeUser.Left);
                lblWelcomeUser.BringToFront();
                _profileBadge.BringToFront();
            }
        }

        private void BrandPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var accent = new SolidBrush(Color.FromArgb(26, 35, 126)))
                e.Graphics.FillRectangle(accent, 0, 0, tileBrand.Width, 6);
            int sealSize = Math.Min(picBrandLogo.Width, picBrandLogo.Height);
            int glowSize = (int)(sealSize * 1.42f);
            int glowX = (tileBrand.Width - glowSize) / 2;
            int glowY = 34 + (sealSize - glowSize) / 2;
            using (var glow = new SolidBrush(Color.FromArgb(12, 26, 35, 126)))
                e.Graphics.FillEllipse(glow, glowX, glowY, glowSize, glowSize);
            using (var ring = new Pen(Color.FromArgb(8, 26, 35, 126), 1f))
            {
                e.Graphics.DrawEllipse(ring, glowX, glowY, glowSize, glowSize);
            }
        }

        private void DateTile_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var gradient = new LinearGradientBrush(tileDate.ClientRectangle,
                Color.FromArgb(26, 35, 126), Color.FromArgb(20, 28, 92), 35f))
                e.Graphics.FillRectangle(gradient, tileDate.ClientRectangle);

            int size = Math.Max(80, tileDate.Height * 2 / 3);
            int x = tileDate.Width - size / 2;
            int y = tileDate.Height - size / 2;
            using (var spoke = new Pen(Color.FromArgb(10, 220, 225, 238), 2f))
            {
                e.Graphics.DrawEllipse(spoke, x, y, size, size);
                e.Graphics.DrawLine(spoke, x + size / 2, y + size / 2, x + size / 2, y + 8);
                e.Graphics.DrawLine(spoke, x + size / 2, y + size / 2, x + size - 8, y + size / 2);
            }
        }

        private void ClockBadge_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new SolidBrush(Color.FromArgb(72, 107, 202)))
                e.Graphics.FillEllipse(brush, 1, 1, _clockBadge.Width - 3, _clockBadge.Height - 3);
            using (var pen = new Pen(Color.FromArgb(190, 224, 231, 255), 2f))
            {
                int cx = _clockBadge.Width / 2;
                int cy = _clockBadge.Height / 2;
                e.Graphics.DrawEllipse(pen, 12, 12, 22, 22);
                e.Graphics.DrawLine(pen, cx, cy, cx, 17);
                e.Graphics.DrawLine(pen, cx, cy, 29, cy);
            }
        }

        private void TableGrid_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            foreach (Control tile in new Control[] { tileDashboard, tileResidents, tileOfficials, tileClearance, tileAyuda, tileBudget, tileDate, tileLogout })
            {
                if (!tile.Visible || tile.Width < 1 || tile.Height < 1) continue;
                var bounds = new Rectangle(tile.Left + 2, tile.Top + 3, tile.Width, tile.Height);
                using (var path = RoundedPath(bounds, 14))
                using (var brush = new SolidBrush(Color.FromArgb(28, 20, 35, 80)))
                    e.Graphics.FillPath(brush, path);
            }
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

        private void LoadLiveBranding()
        {
            var profile = SettingsHubControl.LoadSystemProfile();
            if (!string.IsNullOrWhiteSpace(profile.SystemTitle))
            {
                lblSystemTitle.Text = profile.SystemTitle.ToUpperInvariant();
                lblBrandName.Text = profile.SystemTitle;
            }
            if (!string.IsNullOrWhiteSpace(profile.Tagline)) lblBrandSub.Text = profile.Tagline;
            if (!string.IsNullOrWhiteSpace(profile.Municipality)) _brandMunicipality.Text = profile.Municipality;
            SetImageIfAvailable(picBrandLogo, profile.LogoPath);
            SetImageIfAvailable(picLogoTop, profile.TopLogoPath);
            SetImageIfAvailable(pictureBox1, profile.VendorLogoPath);
            picLogoTop.BorderStyle = BorderStyle.FixedSingle;
            pictureBox1.BorderStyle = BorderStyle.FixedSingle;
        }

        private static void SetImageIfAvailable(PictureBox target, string path)
        {
            if (target == null || string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;
            try
            {
                using (var source = Image.FromFile(path)) target.Image = new Bitmap(source);
                target.SizeMode = PictureBoxSizeMode.Zoom;
            }
            catch { }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  REMOVE PICTUREBOX WHITE BORDERS
        // ═════════════════════════════════════════════════════════════════════
        private void RemovePictureBorders()
        {
            PictureBox[] pics = {
                picDashboard, picResidents, picClearance, picOfficials,
                picBackup, picAyuda, picBudget, picUserMgmt, picUserLogs, picLogout
            };
            foreach (var pic in pics)
            {
                if (pic == null) continue;
                pic.BorderStyle = BorderStyle.None;
                pic.BackColor = Color.Transparent;
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  OPEN CONTROL — no Windows chrome, covers entire screen including taskbar
        // ═════════════════════════════════════════════════════════════════════
        private void OpenControl(UserControl ctrl, string title)
        {
            AuditLogger.Log("VIEW", title, $"Opened {title}");

            var f = new Form
            {
                Text = string.Empty,
                BackColor = Color.FromArgb(240, 242, 248),
                ForeColor = Color.FromArgb(30, 30, 60),
                Font = new Font("Segoe UI", 10f),

                // Removes the title bar, minimize/maximize/close buttons (X),
                // and the rectangle border around the window entirely
                FormBorderStyle = FormBorderStyle.None,
                ControlBox = false,

                // Cover the exact screen that MainMenuForm occupies so nothing bleeds through
                StartPosition = FormStartPosition.Manual,
                Bounds = Screen.FromControl(this).Bounds,  // covers FULL monitor including taskbar
                WindowState = FormWindowState.Normal,           // Normal + Bounds = true fullscreen

                ShowInTaskbar = false,
            };

            ctrl.Dock = DockStyle.Fill;
            f.Controls.Add(ctrl);
            f.ShowDialog(this);
        }

        // ═════════════════════════════════════════════════════════════════════
        //  REST OF METHODS
        // ═════════════════════════════════════════════════════════════════════

        private void MainMenuForm_Shown(object sender, EventArgs e)
        {
            using (var w = new WelcomeNotificationForm())
                w.ShowDialog(this);

            RefreshNotifBadge();

            AuditLogger.Log("LOGIN", "Login",
                $"{CurrentUser.Username} logged in as {CurrentUser.Role}");
        }

        public void RefreshDbStatus()
        {
            string mode = ReadConnectionTypeFromConfig();
            bool connected = TestDbConnection();
            Color dotColor;
            string statusText;

            if (!connected)
            {
                dotColor = Color.FromArgb(239, 68, 68);
                statusText = "DATABASE\r\nDISCONNECTED";
                tileDbStatus.BackColor = Color.FromArgb(80, 29, 29);
            }
            else
            {
                switch (mode)
                {
                    case "Local":
                        dotColor = Color.FromArgb(74, 222, 128);
                        statusText = "DATABASE\r\nLOCAL  ·  Connected";
                        tileDbStatus.BackColor = Color.FromArgb(20, 83, 45);
                        break;
                    case "Remote":
                        dotColor = Color.FromArgb(251, 191, 36);
                        statusText = "DATABASE\r\nREMOTE  ·  Connected";
                        tileDbStatus.BackColor = Color.FromArgb(78, 60, 10);
                        break;
                    default:
                        dotColor = Color.FromArgb(74, 222, 128);
                        statusText = "DATABASE\r\nONLINE  ·  Connected";
                        tileDbStatus.BackColor = Color.FromArgb(20, 83, 45);
                        break;
                }
            }

            if (panelDbDot != null) { panelDbDot.BackColor = dotColor; ApplyRound(panelDbDot, 10); }
            if (lblDbStatus != null) lblDbStatus.Text = statusText;
            tileDbStatus.Cursor = Cursors.Hand;
            if (_connectionModeLabel != null)
            {
                string displayMode = mode.Equals("Local", StringComparison.OrdinalIgnoreCase) ? "Offline" : mode.Equals("Remote", StringComparison.OrdinalIgnoreCase) ? "LAN" : "Online";
                _connectionModeLabel.Text = displayMode + (connected ? " - Connected" : " - Unreachable");
                _connectionModeLabel.ForeColor = connected ? Color.FromArgb(20, 130, 70) : Color.FromArgb(160, 55, 55);
                _connectionStatusDot.BackColor = connected ? Color.FromArgb(15, 157, 88) : Color.FromArgb(150, 150, 150);
                _bottomStatusFlow?.PerformLayout();
            }
        }

        private string ReadConnectionTypeFromConfig()
        {
            if (File.Exists(CONFIG_FILE))
            {
                try
                {
                    foreach (string line in File.ReadAllLines(CONFIG_FILE))
                    {
                        int eq = line.IndexOf('=');
                        if (eq < 0) continue;
                        string key = line.Substring(0, eq).Trim();
                        string val = line.Substring(eq + 1).Trim();
                        if (key == "ConnectionType" && val.Equals("Local", StringComparison.OrdinalIgnoreCase)) return "Local";
                    }
                }
                catch { }
            }
            try
            {
                return Integration.ExternalDbConfig.Load().ActiveProfile == Integration.ActiveNetworkProfile.Lan ? "Lan" : "Online";
            }
            catch { }
            return "Online";
        }

        private bool TestDbConnection()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                    return conn != null;
            }
            catch { return false; }
        }

        private void timerClock_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            if (lblDateDay != null) lblDateDay.Text = now.ToString("dddd,");
            if (lblDateRest != null) lblDateRest.Text = now.ToString("d MMMM yyyy");
            if (lblDateTime != null) lblDateTime.Text = now.ToString("h:mm:ss tt");
            if (_clockPulse != null) _clockPulse.ForeColor = now.Second % 2 == 0 ? Color.FromArgb(74, 222, 128) : Color.FromArgb(180, 220, 190);
            RefreshDbStatus();

            lblBottomRight.Text = $"{now:dd/MM/yyyy}   {now:h:mm tt}   ·  {CurrentUser.Username}";
            if (lblBottomRight.IsHandleCreated)
                lblBottomRight.Left = panelBottomBar.Width - lblBottomRight.Width - 14;
            LayoutBottomRefresh();
        }

        private static void WireTile(Panel tile, PictureBox pic,
            Label name, Label sub, Color baseColor, Action action)
        {
            tile.BackColor = baseColor;
            Color hover = ControlPaint.Light(baseColor, 0.18f);

            EventHandler enter = (s, e2) => tile.BackColor = hover;
            EventHandler leave = (s, e2) => tile.BackColor = baseColor;
            MouseEventHandler down = (s, e2) => { if (e2.Button == MouseButtons.Left) tile.BackColor = ControlPaint.Dark(baseColor, 0.12f); };
            MouseEventHandler up = (s, e2) =>
            {
                if (tile.IsDisposed || tile.Disposing || !tile.IsHandleCreated) return;
                tile.BackColor = tile.ClientRectangle.Contains(tile.PointToClient(Control.MousePosition)) ? hover : baseColor;
            };
            EventHandler click = (s, e2) => action?.Invoke();

            var ctrls = new System.Collections.Generic.List<Control> { tile };
            if (pic != null) ctrls.Add(pic);
            if (name != null) ctrls.Add(name);
            if (sub != null) ctrls.Add(sub);

            foreach (Control c in ctrls)
            {
                c.MouseEnter += enter;
                c.MouseLeave += leave;
                c.MouseDown += down;
                c.MouseUp += up;
                c.Click += click;
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  LOCK TILE — re-wires a tile to look disabled and show a restriction
        //  notice on click, instead of running its normal action.
        // ═════════════════════════════════════════════════════════════════════
        private static void LockTile(Panel tile, PictureBox pic, Label name, Label sub)
        {
            Color lockedColor = Color.FromArgb(203, 206, 214);
            Color lockedTextColor = Color.FromArgb(120, 123, 133);

            WireTile(tile, pic, name, sub, lockedColor, () =>
                MessageBox.Show(
                    "This section is restricted to Super Admin access only.",
                    "Access Restricted", MessageBoxButtons.OK, MessageBoxIcon.Information));

            if (name != null) name.ForeColor = lockedTextColor;
            if (sub != null) sub.ForeColor = lockedTextColor;
            if (pic != null) pic.Enabled = true;
        }

        private void DoLogout()
        {
            if (ThemedDialog.Confirm("Are you sure you want to exit?", "Exit Application"))
            {
                AuditLogger.Log("LOGOUT", "Login", $"{CurrentUser.Username} logged out");
                timerClock.Stop();
                CurrentUser.Logout();
                this.Close();
                new LoginForm().Show();
            }
        }

        public void RefreshNotifBadge()
        {
            int count = 0;
            lblNotifBadge.Text = count.ToString();
            lblNotifBadge.Visible = count > 0;
        }

        private void picNotifBell_Click(object sender, EventArgs e)
        {
            using (var f = new WelcomeNotificationForm())
                f.ShowDialog(this);
        }

        private void TopBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && WindowState != FormWindowState.Maximized)
            { _dragging = true; _dragStart = e.Location; }
        }

        private void TopBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
                Location = new Point(Left + e.X - _dragStart.X, Top + e.Y - _dragStart.Y);
        }

        private void TopBar_MouseUp(object sender, MouseEventArgs e) => _dragging = false;

        private void panelTopBar_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawLine(new Pen(Color.FromArgb(40, 0, 0, 80)),
                0, panelTopBar.Height - 1, panelTopBar.Width, panelTopBar.Height - 1);
        }

        private void panelDiamond_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int cW = 44, cH = panelDiamond.Height;
            using (var pen = new Pen(Color.FromArgb(25, 30, 80, 200), 1f))
            using (var fill = new SolidBrush(Color.FromArgb(10, 30, 80, 200)))
            {
                for (int cx = -cW; cx < panelDiamond.Width + cW; cx += cW)
                {
                    g.DrawPolygon(pen, new[] {
                        new Point(cx + cW/2, 2),    new Point(cx + cW-2, cH/2),
                        new Point(cx + cW/2, cH-2), new Point(cx + 2, cH/2) });
                    g.FillPolygon(fill, new[] {
                        new Point(cx + cW/2, 8),    new Point(cx + cW-8, cH/2),
                        new Point(cx + cW/2, cH-8), new Point(cx + 8, cH/2) });
                }
            }
        }

        private void panelBottomBar_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawLine(new Pen(Color.FromArgb(40, 30, 80, 200)),
                0, 0, panelBottomBar.Width, 0);
        }

        private static void ApplyRound(Control c, int r)
        {
            if (c == null || c.Width < 1 || c.Height < 1) return;
            c.Region = Region.FromHrgn(
                CreateRoundRectRgn(0, 0, c.Width, c.Height, r * 2, r * 2));
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int l, int t, int r, int b, int w, int h);

        private void lblBrandSub_Click(object sender, EventArgs e) { }
        private void lblWelcomeUser_Click(object sender, EventArgs e) { }
    }
}