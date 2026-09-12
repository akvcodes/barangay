// ╔══════════════════════════════════════════════════════════════════════════╗
// ║ BackupForm.cs                                                        ║
// ║ DELETE BackupForm.cs + BackupForm.Designer.cs                        ║
// ║ Add this as a NEW Class file named BackupForm.cs — no Designer needed║
// ╚══════════════════════════════════════════════════════════════════════════╝

using System.Data.SQLite;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace BarangayManagementSystem
{
    public class BackupForm : UserControl
    {
        // ── Colors ────────────────────────────────────────────────────────────
        private static readonly Color NavBlue = Color.FromArgb(26, 35, 126);
        private static readonly Color BgPage = Color.FromArgb(240, 242, 248);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderCol = Color.FromArgb(220, 225, 238);
        private static readonly Color TextGray = Color.FromArgb(108, 114, 140);
        private static readonly Color BtnGreen = Color.FromArgb(15, 157, 88);
        private static readonly Color BtnBlue = Color.FromArgb(66, 133, 244);
        private static readonly Color BtnAmber = Color.FromArgb(180, 100, 0);
        private static readonly Color BtnRed = Color.FromArgb(192, 57, 43);
        private static readonly Color BtnGray = Color.FromArgb(100, 110, 128);
        private static readonly Color LightBlue = Color.FromArgb(238, 242, 255);

        // ── State ─────────────────────────────────────────────────────────────
        private string _backupFolder;
        private string _configFile;
        private DateTime _lastFullBackup = DateTime.MinValue;

        // ── Controls ──────────────────────────────────────────────────────────
        private ListBox _lstHistory;
        private Label _lblStatus;
        private TextBox _txtBackupPath;
        private Button _btnBrowseFolder;

        // ══════════════════════════════════════════════════════════════════════
        public BackupForm()
        {
            // ── Persistent backup location (saved in AppData) ─────────────────
            string appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BarangayManagementSystem");

            if (!Directory.Exists(appDataFolder))
                Directory.CreateDirectory(appDataFolder);

            _configFile = Path.Combine(appDataFolder, "backup_location.txt");

            if (File.Exists(_configFile))
            {
                string saved = File.ReadAllText(_configFile).Trim();
                if (!string.IsNullOrEmpty(saved) && Directory.Exists(saved))
                    _backupFolder = saved;
                else
                    _backupFolder = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "BarangayBackups");
            }
            else
            {
                _backupFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "BarangayBackups");
            }

            this.BackColor = BgPage;
            this.Font = new Font("Segoe UI", 9.5f);
            this.ClientSize = new Size(820, 610);   // Increased height for new controls

            BuildUI();
            LoadHistory();
        }

        // ══════════════════════════════════════════════════════════════════════
        // BUILD UI
        // ══════════════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            // ── Page heading ──────────────────────────────────────────────────
            this.Controls.Add(new Label
            {
                Text = "Database Backup",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(24, 16),
                BackColor = Color.Transparent,
            });

            this.Controls.Add(new Label
            {
                Text = "Create and manage database backups — SuperAdmin only",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = TextGray,
                AutoSize = true,
                Location = new Point(26, 52),
                BackColor = Color.Transparent,
            });

            // ── NEW: Storage Location Selector ────────────────────────────────
            this.Controls.Add(new Label
            {
                Text = "Storage Location:",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = TextGray,
                AutoSize = true,
                Location = new Point(24, 82),
                BackColor = Color.Transparent,
            });

            _txtBackupPath = new TextBox
            {
                Text = _backupFolder,
                Font = new Font("Segoe UI", 9f),
                ForeColor = NavBlue,
                BackColor = BgCard,
                BorderStyle = BorderStyle.Fixed3D,
                ReadOnly = true,
                Location = new Point(170, 79),
                Size = new Size(565, 28),
                TabStop = false,
            };
            this.Controls.Add(_txtBackupPath);

            _btnBrowseFolder = MakeBtn("Change Folder", BtnBlue);
            _btnBrowseFolder.Location = new Point(745, 78);
            _btnBrowseFolder.Size = new Size(72, 30);
            _btnBrowseFolder.Click += (s, e) => ChangeBackupFolder();
            this.Controls.Add(_btnBrowseFolder);

            // ── Backup type cards ─────────────────────────────────────────────
            int cx = 24, cy = 124, cw = 240, ch = 120, gap = 16;   // shifted down

            // Full Backup card
            var card1 = MakeCard(new Point(cx, cy), new Size(cw, ch));
            this.Controls.Add(card1);
            card1.Controls.Add(new Label
            {
                Text = "Full Backup",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(16, 14),
                BackColor = Color.Transparent,
            });
            card1.Controls.Add(new Label
            {
                Text = "Backs up the entire database.\nRecommended weekly.",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = TextGray,
                Size = new Size(cw - 32, 40),
                Location = new Point(16, 40),
                BackColor = Color.Transparent,
            });
            var btnFull = MakeBtn("Run Full Backup", BtnGreen);
            btnFull.Location = new Point(16, 78);
            btnFull.Size = new Size(cw - 32, 32);
            btnFull.Click += (s, e) => DoBackup("FULL");
            card1.Controls.Add(btnFull);
            cx += cw + gap;

            // Incremental card
            var card2 = MakeCard(new Point(cx, cy), new Size(cw, ch));
            this.Controls.Add(card2);
            card2.Controls.Add(new Label
            {
                Text = "Incremental Backup",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(16, 14),
                BackColor = Color.Transparent,
            });
            card2.Controls.Add(new Label
            {
                Text = "Backs up records from the\nlast 24 hours only.",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = TextGray,
                Size = new Size(cw - 32, 40),
                Location = new Point(16, 40),
                BackColor = Color.Transparent,
            });
            var btnInc = MakeBtn("Run Incremental", BtnBlue);
            btnInc.Location = new Point(16, 78);
            btnInc.Size = new Size(cw - 32, 32);
            btnInc.Click += (s, e) => DoBackup("INCREMENTAL");
            card2.Controls.Add(btnInc);
            cx += cw + gap;

            // Differential card
            var card3 = MakeCard(new Point(cx, cy), new Size(cw, ch));
            this.Controls.Add(card3);
            card3.Controls.Add(new Label
            {
                Text = "Differential Backup",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(16, 14),
                BackColor = Color.Transparent,
            });
            card3.Controls.Add(new Label
            {
                Text = "Backs up changes since\nlast Full Backup.",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = TextGray,
                Size = new Size(cw - 32, 40),
                Location = new Point(16, 40),
                BackColor = Color.Transparent,
            });
            var btnDiff = MakeBtn("Run Differential", BtnAmber);
            btnDiff.Location = new Point(16, 78);
            btnDiff.Size = new Size(cw - 32, 32);
            btnDiff.Click += (s, e) => DoBackup("DIFFERENTIAL");
            card3.Controls.Add(btnDiff);

            // ── History card ──────────────────────────────────────────────────
            var histCard = MakeCard(new Point(24, 260), new Size(772, 290));   // shifted down
            this.Controls.Add(histCard);
            histCard.Controls.Add(new Label
            {
                Text = "Backup History",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(16, 14),
                BackColor = Color.Transparent,
            });

            var div = new Panel
            {
                Location = new Point(0, 44),
                Size = new Size(772, 1),
                BackColor = BorderCol,
            };
            histCard.Controls.Add(div);

            _lstHistory = new ListBox
            {
                Location = new Point(8, 52),
                Size = new Size(756, 200),
                Font = new Font("Consolas", 9.5f),
                BorderStyle = BorderStyle.None,
                BackColor = BgCard,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 28,
                SelectionMode = SelectionMode.One,
            };
            _lstHistory.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;
                e.DrawBackground();
                bool selected = (e.State & DrawItemState.Selected) != 0;
                using (var bg = new SolidBrush(selected ? BtnBlue :
                    (e.Index % 2 == 0 ? BgCard : LightBlue)))
                    e.Graphics.FillRectangle(bg, e.Bounds);
                string text = _lstHistory.Items[e.Index].ToString();
                using (var brush = new SolidBrush(selected ? Color.White : Color.FromArgb(40, 40, 60)))
                    e.Graphics.DrawString(text,
                        new Font("Consolas", 9f),
                        brush,
                        new Point(e.Bounds.X + 8, e.Bounds.Y + 6));
            };
            histCard.Controls.Add(_lstHistory);

            // ── Bottom action buttons ─────────────────────────────────────────
            var btnRefresh = MakeBtn("Refresh", BtnGray);
            btnRefresh.Location = new Point(24, 566);
            btnRefresh.Size = new Size(110, 36);
            btnRefresh.Click += (s, e) => LoadHistory();
            this.Controls.Add(btnRefresh);

            var btnOpenFolder = MakeBtn("Open Folder", BtnBlue);
            btnOpenFolder.Location = new Point(144, 566);
            btnOpenFolder.Size = new Size(130, 36);
            btnOpenFolder.Click += (s, e) => OpenFolder();
            this.Controls.Add(btnOpenFolder);

            var btnDelete = MakeBtn("Delete Selected", BtnRed);
            btnDelete.Location = new Point(284, 566);
            btnDelete.Size = new Size(160, 36);
            btnDelete.Click += (s, e) => DeleteSelected();
            this.Controls.Add(btnDelete);

            // Status label
            _lblStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9f),
                ForeColor = BtnGreen,
                AutoSize = true,
                Location = new Point(460, 574),
                BackColor = Color.Transparent,
            };
            this.Controls.Add(_lblStatus);
        }

        // ══════════════════════════════════════════════════════════════════════
        // NEW: Change backup storage location (with persistence)
        // ══════════════════════════════════════════════════════════════════════
        private void ChangeBackupFolder()
        {
            using (var dialog = new FolderBrowserDialog
            {
                Description = "Select Backup Storage Location",
                SelectedPath = _backupFolder,
                ShowNewFolderButton = true
            })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _backupFolder = dialog.SelectedPath;
                    _txtBackupPath.Text = _backupFolder;

                    // Persist choice
                    File.WriteAllText(_configFile, _backupFolder);

                    if (!Directory.Exists(_backupFolder))
                        Directory.CreateDirectory(_backupFolder);

                    LoadHistory();
                    _lblStatus.Text = $"✅ Storage location updated";
                    _lblStatus.ForeColor = BtnGreen;
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // BACKUP LOGIC (unchanged except folder creation is now flexible)
        // ══════════════════════════════════════════════════════════════════════
        private void DoBackup(string type)
        {
            if (type == "INCREMENTAL" && _lastFullBackup == DateTime.MinValue)
            { MessageBox.Show("Please run a Full Backup first!", "Full Backup Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (type == "DIFFERENTIAL" && _lastFullBackup == DateTime.MinValue)
            { MessageBox.Show("Please run a Full Backup first!", "Full Backup Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (MessageBox.Show(
                $"Create a {type} backup now?",
                "Confirm Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                == DialogResult.No) return;

            if (!Directory.Exists(_backupFolder))
                Directory.CreateDirectory(_backupFolder);

            string file = Path.Combine(_backupFolder,
                $"{type}_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql");

            try
            {
                this.Cursor = Cursors.WaitCursor;
                _lblStatus.Text = $"Running {type} backup...";
                Application.DoEvents();

                using (var conn = DatabaseConnection.GetConnection())
                {
                    if (conn == null) return;
                    using (var writer = new StreamWriter(file))
                    {
                        writer.WriteLine($"-- {type} BACKUP");
                        writer.WriteLine($"-- Created : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        writer.WriteLine($"-- Database: {conn.Database}");
                        writer.WriteLine($"-- By : {CurrentUser.Username}");
                        writer.WriteLine();
                        var tables = conn.GetSchema("Tables");
                        foreach (System.Data.DataRow row in tables.Rows)
                        {
                            string tbl = row["TABLE_NAME"].ToString();
                            if (tbl == "auditlog") continue;
                            writer.WriteLine($"-- ── Table: {tbl} ──────────────────────────");
                            if (type == "FULL")
                                BackupFull(conn, writer, tbl);
                            else if (type == "INCREMENTAL")
                                BackupIncremental(conn, writer, tbl);
                            else
                                BackupDifferential(conn, writer, tbl);
                            writer.WriteLine();
                        }
                        writer.WriteLine($"-- Completed: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    }
                }

                if (type == "FULL") _lastFullBackup = DateTime.Now;
                long kb = new FileInfo(file).Length / 1024;
                _lblStatus.Text = $"✅ {type} backup saved ({kb} KB)";
                _lblStatus.ForeColor = BtnGreen;
                LoadHistory();
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "❌ Backup failed";
                _lblStatus.ForeColor = BtnRed;
                MessageBox.Show("Backup error:\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { this.Cursor = Cursors.Default; }
        }

        private void BackupFull(SQLiteConnection conn, StreamWriter w, string tbl)
        {
            try
            {
                var cmd = new SQLiteCommand($"SELECT * FROM `{tbl}`", conn);
                using (var r = cmd.ExecuteReader())
                {
                    int n = 0;
                    while (r.Read()) { w.WriteLine(InsertSql(tbl, r)); n++; }
                    w.WriteLine($"-- {n} rows");
                }
            }
            catch (Exception ex) { w.WriteLine($"-- Error: {ex.Message}"); }
        }

        private void BackupIncremental(SQLiteConnection conn, StreamWriter w, string tbl)
        {
            try
            {
                string col = HasColumn(conn, tbl, "CreatedAt") ? "CreatedAt" :
                             HasColumn(conn, tbl, "created_at") ? "created_at" : null;
                if (col == null) { w.WriteLine("-- No timestamp column, skipped"); return; }
                var cmd = new SQLiteCommand(
                    $"SELECT * FROM `{tbl}` WHERE `{col}` >= DATE_SUB(datetime('now'), INTERVAL 1 DAY)", conn);
                using (var r = cmd.ExecuteReader())
                {
                    int n = 0;
                    while (r.Read()) { w.WriteLine(InsertSql(tbl, r)); n++; }
                    w.WriteLine($"-- {n} rows (last 24h)");
                }
            }
            catch (Exception ex) { w.WriteLine($"-- Error: {ex.Message}"); }
        }

        private void BackupDifferential(SQLiteConnection conn, StreamWriter w, string tbl)
        {
            try
            {
                string col = HasColumn(conn, tbl, "UpdatedAt") ? "UpdatedAt" :
                             HasColumn(conn, tbl, "updated_at") ? "updated_at" :
                             HasColumn(conn, tbl, "CreatedAt") ? "CreatedAt" :
                             HasColumn(conn, tbl, "created_at") ? "created_at" : null;
                if (col == null) { w.WriteLine("-- No timestamp column, skipped"); return; }
                var cmd = new SQLiteCommand(
                    $"SELECT * FROM `{tbl}` WHERE `{col}` >= '{_lastFullBackup:yyyy-MM-dd HH:mm:ss}'", conn);
                using (var r = cmd.ExecuteReader())
                {
                    int n = 0;
                    while (r.Read()) { w.WriteLine(InsertSql(tbl, r)); n++; }
                    w.WriteLine($"-- {n} rows since last full backup");
                }
            }
            catch (Exception ex) { w.WriteLine($"-- Error: {ex.Message}"); }
        }

        private string InsertSql(string tbl, SQLiteDataReader r)
        {
            var cols = new System.Text.StringBuilder();
            var vals = new System.Text.StringBuilder();
            for (int i = 0; i < r.FieldCount; i++)
            {
                if (i > 0) { cols.Append(", "); vals.Append(", "); }
                cols.Append($"`{r.GetName(i)}`");
                if (r.IsDBNull(i)) vals.Append("NULL");
                else if (r.GetFieldType(i) == typeof(byte[])) vals.Append("NULL");
                else if (r.GetFieldType(i) == typeof(string) ||
                         r.GetFieldType(i) == typeof(DateTime))
                    vals.Append($"'{r.GetValue(i).ToString().Replace("'", "''")}'");
                else vals.Append(r.GetValue(i));
            }
            return $"INSERT INTO `{tbl}` ({cols}) VALUES ({vals});";
        }

        private bool HasColumn(SQLiteConnection conn, string tbl, string col)
        {
            try
            {
                var cmd = new SQLiteCommand(
                    $"SHOW COLUMNS FROM `{tbl}` LIKE '{col}'", conn);
                return cmd.ExecuteScalar() != null;
            }
            catch { return false; }
        }

        // ══════════════════════════════════════════════════════════════════════
        // HISTORY / ACTIONS (unchanged)
        // ══════════════════════════════════════════════════════════════════════
        private void LoadHistory()
        {
            _lstHistory.Items.Clear();
            if (!Directory.Exists(_backupFolder))
            { _lstHistory.Items.Add(" No backups yet."); return; }

            var files = Directory.GetFiles(_backupFolder, "*.sql");
            if (files.Length == 0)
            { _lstHistory.Items.Add(" No backup files found."); return; }

            Array.Sort(files, (a, b) =>
                File.GetCreationTime(b).CompareTo(File.GetCreationTime(a)));

            foreach (string f in files)
            {
                var fi = new FileInfo(f);
                string kb = (fi.Length / 1024).ToString("N0") + " KB";
                string dt = fi.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
                _lstHistory.Items.Add($" {fi.Name,-50} {kb,8} {dt}");
            }
        }

        private void DeleteSelected()
        {
            if (_lstHistory.SelectedItem == null)
            { MessageBox.Show("Please select a backup to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            string selected = _lstHistory.SelectedItem.ToString().Trim();
            string fileName = selected.Split(new[] { ' ' }, 2)[0];
            string fullPath = Path.Combine(_backupFolder, fileName);

            if (!File.Exists(fullPath))
            { MessageBox.Show("File not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

            if (MessageBox.Show($"Delete {fileName}?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                File.Delete(fullPath);
                _lblStatus.Text = "🗑 Backup deleted.";
                _lblStatus.ForeColor = BtnGray;
                LoadHistory();
            }
        }

        private void OpenFolder()
        {
            if (!Directory.Exists(_backupFolder))
                Directory.CreateDirectory(_backupFolder);
            System.Diagnostics.Process.Start("explorer.exe", _backupFolder);
        }

        // ══════════════════════════════════════════════════════════════════════
        // UI HELPERS (unchanged)
        // ══════════════════════════════════════════════════════════════════════
        private Panel MakeCard(Point loc, Size size)
        {
            var p = new Panel { BackColor = BgCard, Location = loc, Size = size };
            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(BorderCol))
                    RoundRect(e.Graphics, pen,
                        new Rectangle(0, 0, p.Width - 1, p.Height - 1), 10);
            };
            return p;
        }

        private Button MakeBtn(string text, Color bg)
        {
            var b = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
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

        private static void RoundRect(Graphics g, Pen pen, Rectangle r, int radius)
        {
            int d = radius * 2;
            using (var path = new GraphicsPath())
            {
                path.AddArc(r.X, r.Y, d, d, 180, 90);
                path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                g.DrawPath(pen, path);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "BackupForm";
            this.Load += new System.EventHandler(this.BackupForm_Load);
            this.ResumeLayout(false);
        }

        private void BackupForm_Load(object sender, EventArgs e) { }
    }
}