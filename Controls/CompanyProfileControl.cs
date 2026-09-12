// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  CompanyProfileControl.cs                                                ║
// ║  Add as New Item → Class → place in Controls folder                      ║
// ║  Wire in MainMenuForm sidebar — visible to SuperAdmin                    ║
// ╚══════════════════════════════════════════════════════════════════════════╝

using System.Data.SQLite;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace BarangayManagementSystem.Controls
{
    public class CompanyProfileControl : UserControl
    {
        private static readonly Color NavBlue = Color.FromArgb(26, 35, 126);
        private static readonly Color BgPage = Color.FromArgb(240, 242, 248);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderCol = Color.FromArgb(220, 225, 238);
        private static readonly Color TextDark = Color.FromArgb(28, 30, 44);
        private static readonly Color TextGray = Color.FromArgb(108, 114, 140);
        private static readonly Color BtnGreen = Color.FromArgb(15, 157, 88);
        private static readonly Color BtnBlue = Color.FromArgb(66, 133, 244);
        private static readonly Color BtnRed = Color.FromArgb(192, 57, 43);
        private static readonly Color BtnGray = Color.FromArgb(100, 110, 128);

        // ── Barangay fields ───────────────────────────────────────────────────
        private PictureBox _pbLogo;
        private TextBox _txtBarangayName, _txtMunicipality, _txtProvince;
        private TextBox _txtRegion, _txtAddress, _txtContact, _txtEmail;
        private TextBox _txtLGUCode, _txtSerial;
        private byte[] _logoBytes;

        // ── User profile fields ───────────────────────────────────────────────
        private Label _lblCurrentUser, _lblCurrentRole;
        private TextBox _txtNewPassword, _txtConfirmPassword;

        public CompanyProfileControl()
        {
            this.DoubleBuffered = true;
            this.BackColor = BgPage;
            this.Dock = DockStyle.Fill;
            BuildUI();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadProfile();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  BUILD UI
        // ══════════════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            // ── Custom top bar with back button ───────────────────────────────
            var topBar = new Panel
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
            topBar.Controls.Add(btnBack);

            var lblTopTitle = new Label
            {
                Text = "🏛   Company Profile",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            topBar.Controls.Add(lblTopTitle);
            topBar.Layout += (s, e) =>
            {
                lblTopTitle.Location = new Point(
                    (topBar.Width - lblTopTitle.Width) / 2,
                    (topBar.Height - lblTopTitle.Height) / 2);
            };

            this.Controls.Add(topBar);

            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = BgPage,
                Padding = new Padding(24, 20, 24, 20),
            };
            this.Controls.Add(scroll);

            // Heading
            scroll.Controls.Add(new Label
            {
                Text = "Company Profile",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(0, 0),
                BackColor = Color.Transparent,
            });
            scroll.Controls.Add(new Label
            {
                Text = "Manage barangay information and your account settings",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = TextGray,
                AutoSize = true,
                Location = new Point(2, 36),
                BackColor = Color.Transparent,
            });

            int y = 70;

            // ── Barangay Info Card ────────────────────────────────────────────
            var barangayCard = MakeCard(new Point(0, y), new Size(600, 100));
            scroll.Controls.Add(barangayCard);

            barangayCard.Controls.Add(CardTitle("🏛  Barangay Information", 14, 12));
            var div1 = new Panel { Location = new Point(0, 38), Height = 1, BackColor = BorderCol };
            barangayCard.Controls.Add(div1);
            barangayCard.Resize += (s, e) => div1.Width = barangayCard.Width;

            // Logo section
            _pbLogo = new PictureBox
            {
                Location = new Point(16, 50),
                Size = new Size(100, 100),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.FromArgb(208, 214, 232),
                BorderStyle = BorderStyle.FixedSingle,
            };
            _pbLogo.Paint += (s, e) =>
            {
                if (_pbLogo.Image == null)
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var f = new Font("Segoe UI", 28f, FontStyle.Bold))
                    using (var sb = new SolidBrush(Color.FromArgb(150, 160, 200)))
                    {
                        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        e.Graphics.DrawString("🏛", f, sb, new RectangleF(0, 0, _pbLogo.Width, _pbLogo.Height), sf);
                    }
                }
            };
            barangayCard.Controls.Add(_pbLogo);

            var btnUpLogo = MakeBtn("Upload Logo", BtnBlue, 9f);
            btnUpLogo.Location = new Point(16, 158); btnUpLogo.Size = new Size(100, 28);
            btnUpLogo.Click += (s, e) => UploadLogo();
            barangayCard.Controls.Add(btnUpLogo);

            var btnRmLogo = MakeBtn("Remove", BtnRed, 9f);
            btnRmLogo.Location = new Point(16, 190); btnRmLogo.Size = new Size(100, 28);
            btnRmLogo.Click += (s, e) => RemoveLogo();
            barangayCard.Controls.Add(btnRmLogo);

            // Info fields — 2 columns
            const int fx = 134, fw = 220, gap = 54, col2 = 370;
            int fy = 50;

            barangayCard.Controls.Add(FL("Barangay Name:", fx, fy));
            _txtBarangayName = FT(fx, fy + 18, fw); barangayCard.Controls.Add(_txtBarangayName);

            barangayCard.Controls.Add(FL("Municipality/City:", col2, fy));
            _txtMunicipality = FT(col2, fy + 18, fw); barangayCard.Controls.Add(_txtMunicipality);
            fy += gap;

            barangayCard.Controls.Add(FL("Province:", fx, fy));
            _txtProvince = FT(fx, fy + 18, fw); barangayCard.Controls.Add(_txtProvince);

            barangayCard.Controls.Add(FL("Region:", col2, fy));
            _txtRegion = FT(col2, fy + 18, fw); barangayCard.Controls.Add(_txtRegion);
            fy += gap;

            barangayCard.Controls.Add(FL("Complete Address:", fx, fy));
            _txtAddress = FT(fx, fy + 18, fw * 2 + (col2 - fx - fw));
            barangayCard.Controls.Add(_txtAddress);
            fy += gap;

            barangayCard.Controls.Add(FL("Contact Number:", fx, fy));
            _txtContact = FT(fx, fy + 18, fw); barangayCard.Controls.Add(_txtContact);

            barangayCard.Controls.Add(FL("Email Address:", col2, fy));
            _txtEmail = FT(col2, fy + 18, fw); barangayCard.Controls.Add(_txtEmail);
            fy += gap;

            barangayCard.Controls.Add(FL("LGU Code:", fx, fy));
            _txtLGUCode = FT(fx, fy + 18, fw); barangayCard.Controls.Add(_txtLGUCode);

            barangayCard.Controls.Add(FL("Serial Number:", col2, fy));
            _txtSerial = FT(col2, fy + 18, fw); barangayCard.Controls.Add(_txtSerial);
            fy += gap;

            var btnSaveBarangay = MakeBtn("💾  Save Barangay Info", BtnGreen, 10f);
            btnSaveBarangay.Location = new Point(fx, fy);
            btnSaveBarangay.Size = new Size(200, 36);
            btnSaveBarangay.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnSaveBarangay.Click += (s, e) => SaveBarangayProfile();
            barangayCard.Controls.Add(btnSaveBarangay);

            barangayCard.Height = fy + 52;
            y += barangayCard.Height + 16;

            // ── User Profile Card ─────────────────────────────────────────────
            var userCard = MakeCard(new Point(0, y), new Size(600, 100));
            scroll.Controls.Add(userCard);

            userCard.Controls.Add(CardTitle("👤  User Profile", 14, 12));
            var div2 = new Panel { Location = new Point(0, 38), Height = 1, BackColor = BorderCol };
            userCard.Controls.Add(div2);
            userCard.Resize += (s, e) => div2.Width = userCard.Width;

            // User info display
            var userAvatar = new Panel
            {
                Location = new Point(16, 50),
                Size = new Size(72, 72),
                BackColor = NavBlue,
            };
            userAvatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var b = new SolidBrush(NavBlue))
                using (var path = RoundPath(new Rectangle(0, 0, 71, 71), 36))
                    e.Graphics.FillPath(b, path);
                using (var f = new Font("Segoe UI", 24f, FontStyle.Bold))
                using (var sb = new SolidBrush(Color.White))
                {
                    string initial = CurrentUser.IsLoggedIn && CurrentUser.Username.Length > 0
                        ? CurrentUser.Username[0].ToString().ToUpper() : "?";
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    e.Graphics.DrawString(initial, f, sb, new RectangleF(0, 0, 71, 71), sf);
                }
            };
            userCard.Controls.Add(userAvatar);

            _lblCurrentUser = new Label
            {
                Text = CurrentUser.IsLoggedIn ? CurrentUser.Username : "—",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(104, 54),
                BackColor = Color.Transparent,
            };
            userCard.Controls.Add(_lblCurrentUser);

            _lblCurrentRole = new Label
            {
                Text = CurrentUser.IsLoggedIn ? CurrentUser.Role : "—",
                Font = new Font("Segoe UI", 10f),
                ForeColor = TextGray,
                AutoSize = true,
                Location = new Point(106, 84),
                BackColor = Color.Transparent,
            };
            userCard.Controls.Add(_lblCurrentRole);

            // Change password section
            int uy = 140;
            userCard.Controls.Add(new Label
            {
                Text = "Change Password",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(16, uy),
                BackColor = Color.Transparent,
            });
            uy += 24;

            userCard.Controls.Add(FL("New Password:", 16, uy));
            _txtNewPassword = FT(16, uy + 18, 280);
            _txtNewPassword.PasswordChar = '●';
            userCard.Controls.Add(_txtNewPassword);
            uy += 54;

            userCard.Controls.Add(FL("Confirm New Password:", 16, uy));
            _txtConfirmPassword = FT(16, uy + 18, 280);
            _txtConfirmPassword.PasswordChar = '●';
            userCard.Controls.Add(_txtConfirmPassword);
            uy += 54;

            var chkShow = new CheckBox
            {
                Text = "Show passwords",
                Font = new Font("Segoe UI", 9f),
                ForeColor = TextGray,
                AutoSize = true,
                Location = new Point(16, uy),
                BackColor = Color.Transparent,
            };
            chkShow.CheckedChanged += (s, e) =>
            {
                _txtNewPassword.PasswordChar = chkShow.Checked ? '\0' : '●';
                _txtConfirmPassword.PasswordChar = chkShow.Checked ? '\0' : '●';
            };
            userCard.Controls.Add(chkShow);
            uy += 32;

            var btnChangePw = MakeBtn("🔒  Change Password", NavBlue, 10f);
            btnChangePw.Location = new Point(16, uy);
            btnChangePw.Size = new Size(200, 36);
            btnChangePw.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnChangePw.Click += (s, e) => ChangePassword();
            userCard.Controls.Add(btnChangePw);
            uy += 52;

            userCard.Height = uy + 10;
            y += userCard.Height + 16;

            // Resize cards to fill width
            scroll.Resize += (s, e) =>
            {
                int w = scroll.ClientSize.Width - scroll.Padding.Horizontal;
                if (w < 100) return;
                barangayCard.Width = w;
                userCard.Width = w;
            };
        }

        // ══════════════════════════════════════════════════════════════════════
        //  LOAD PROFILE
        // ══════════════════════════════════════════════════════════════════════
        private void LoadProfile()
        {
            var conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand("SELECT * FROM company_profile WHERE id=1", conn);
                    var r = cmd.ExecuteReader();
                    if (!r.Read()) return;

                    _txtBarangayName.Text = r["BarangayName"]?.ToString() ?? "";
                    _txtMunicipality.Text = r["Municipality"]?.ToString() ?? "";
                    _txtProvince.Text = r["Province"]?.ToString() ?? "";
                    _txtRegion.Text = r["Region"]?.ToString() ?? "";
                    _txtAddress.Text = r["Address"]?.ToString() ?? "";
                    _txtContact.Text = r["ContactNumber"]?.ToString() ?? "";
                    _txtEmail.Text = r["EmailAddress"]?.ToString() ?? "";
                    _txtLGUCode.Text = r["LGUCode"]?.ToString() ?? "";
                    _txtSerial.Text = r["SerialNumber"]?.ToString() ?? "";

                    if (r["Logo"] != DBNull.Value)
                    {
                        _logoBytes = (byte[])r["Logo"];
                        using (var ms = new MemoryStream(_logoBytes))
                            _pbLogo.Image = Image.FromStream(ms);
                    }
                    r.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading profile:\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  SAVE BARANGAY PROFILE
        // ══════════════════════════════════════════════════════════════════════
        private void SaveBarangayProfile()
        {
            if (string.IsNullOrWhiteSpace(_txtBarangayName.Text))
            {
                MessageBox.Show("Barangay name is required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand(@"
                        INSERT INTO company_profile
                            (id, BarangayName, Municipality, Province, Region,
                             Address, ContactNumber, EmailAddress, LGUCode, SerialNumber, Logo)
                        VALUES
                            (1, @bn, @mu, @pr, @re, @ad, @cn, @em, @lgu, @sn, @logo)
                        ON DUPLICATE KEY UPDATE
                            BarangayName=@bn, Municipality=@mu, Province=@pr,
                            Region=@re, Address=@ad, ContactNumber=@cn,
                            EmailAddress=@em, LGUCode=@lgu, SerialNumber=@sn, Logo=@logo", conn);

                    cmd.Parameters.AddWithValue("@bn", _txtBarangayName.Text.Trim());
                    cmd.Parameters.AddWithValue("@mu", _txtMunicipality.Text.Trim());
                    cmd.Parameters.AddWithValue("@pr", _txtProvince.Text.Trim());
                    cmd.Parameters.AddWithValue("@re", _txtRegion.Text.Trim());
                    cmd.Parameters.AddWithValue("@ad", _txtAddress.Text.Trim());
                    cmd.Parameters.AddWithValue("@cn", _txtContact.Text.Trim());
                    cmd.Parameters.AddWithValue("@em", _txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@lgu", _txtLGUCode.Text.Trim());
                    cmd.Parameters.AddWithValue("@sn", _txtSerial.Text.Trim());
                    cmd.Parameters.AddWithValue("@logo", _logoBytes != null ? (object)_logoBytes : DBNull.Value);

                    cmd.ExecuteNonQuery();
                    AuditLogger.Log("UPDATE", "System", "Updated company profile");
                    MessageBox.Show("Barangay profile saved successfully!",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving:\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  LOGO
        // ══════════════════════════════════════════════════════════════════════
        private void UploadLogo()
        {
            using (var dlg = new OpenFileDialog
            { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    using (var img = Image.FromFile(dlg.FileName))
                    {
                        _pbLogo.Image = new Bitmap(img);
                        _pbLogo.SizeMode = PictureBoxSizeMode.StretchImage;
                        using (var ms = new MemoryStream())
                        { img.Save(ms, ImageFormat.Png); _logoBytes = ms.ToArray(); }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // CompanyProfileControl
            // 
            this.Name = "CompanyProfileControl";
            this.Load += new System.EventHandler(this.CompanyProfileControl_Load);
            this.ResumeLayout(false);

        }

        private void CompanyProfileControl_Load(object sender, EventArgs e)
        {

        }

        private void RemoveLogo()
        {
            _pbLogo.Image = null;
            _logoBytes = null;
            _pbLogo.BackColor = Color.FromArgb(208, 214, 232);
            _pbLogo.Refresh();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  CHANGE PASSWORD
        // ══════════════════════════════════════════════════════════════════════
        private void ChangePassword()
        {
            if (string.IsNullOrWhiteSpace(_txtNewPassword.Text))
            { MessageBox.Show("Enter a new password.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (_txtNewPassword.Text.Length < 6)
            { MessageBox.Show("Password must be at least 6 characters.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (_txtNewPassword.Text != _txtConfirmPassword.Text)
            { MessageBox.Show("Passwords do not match.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (MessageBox.Show("Change your password?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            var conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand(@"
                        UPDATE users SET Password=MD5(@pw)
                        WHERE idUsers=@id", conn);
                    cmd.Parameters.AddWithValue("@pw", _txtNewPassword.Text);
                    cmd.Parameters.AddWithValue("@id", CurrentUser.UserId);
                    cmd.ExecuteNonQuery();

                    AuditLogger.Log("UPDATE", "Users", $"{CurrentUser.Username} changed their password");
                    MessageBox.Show("Password changed successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _txtNewPassword.Clear();
                    _txtConfirmPassword.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:\n" + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  UI HELPERS
        // ══════════════════════════════════════════════════════════════════════
        private Panel MakeCard(Point loc, Size size)
        {
            var p = new Panel { BackColor = BgCard, Location = loc, Size = size };
            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var bg = new SolidBrush(BgCard))
                using (var pen = new Pen(BorderCol, 1f))
                using (var path = RoundPath(new Rectangle(0, 0, p.Width - 1, p.Height - 1), 10))
                { e.Graphics.FillPath(bg, path); e.Graphics.DrawPath(pen, path); }
            };
            return p;
        }

        private Label CardTitle(string text, int x, int y) =>
            new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(x, y),
                BackColor = Color.Transparent,
            };

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
                Math.Max(0, bg.R - 22), Math.Max(0, bg.G - 22), Math.Max(0, bg.B - 22));
            return b;
        }

        private static Label FL(string text, int x, int y) =>
            new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = TextGray,
                AutoSize = true,
                Location = new Point(x, y),
                BackColor = Color.Transparent,
            };

        private static TextBox FT(int x, int y, int w) =>
            new TextBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(x, y),
                Size = new Size(w, 28),
                BorderStyle = BorderStyle.FixedSingle,
            };

        private static GraphicsPath RoundPath(Rectangle r, int radius)
        {
            int d = radius * 2;
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}