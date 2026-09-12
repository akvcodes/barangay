// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  UserTransactionLogsControl.cs                                           ║
// ║  Add as New Item → Class. No Designer needed.                            ║
// ║  Wire in MainMenuForm: SuperAdmin sidebar only.                          ║
// ╚══════════════════════════════════════════════════════════════════════════╝

using System.Data.SQLite;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BarangayManagementSystem.Controls
{
    public class UserTransactionLogsControl : UserControl
    {
        private static readonly Color NavBlue = Color.FromArgb(26, 35, 126);
        private static readonly Color BgPage = Color.FromArgb(240, 242, 248);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderCol = Color.FromArgb(220, 225, 238);
        private static readonly Color TextGray = Color.FromArgb(108, 114, 140);
        private static readonly Color BtnBlue = Color.FromArgb(66, 133, 244);
        private static readonly Color BtnGray = Color.FromArgb(100, 110, 128);
        private static readonly Color BtnRed = Color.FromArgb(192, 57, 43);
        private static readonly Color LightBlue = Color.FromArgb(238, 242, 255);

        private DataGridView _dgv;
        private TextBox _txtSearch;
        private ComboBox _cboModule, _cboAction;
        private DateTimePicker _dtpFrom, _dtpTo;
        private Label _lblCount;
        private Panel _topBar;

        public UserTransactionLogsControl()
        {
            this.DoubleBuffered = true;
            this.BackColor = BgPage;
            this.Dock = DockStyle.Fill;
            BuildUI();
        }

        public void RefreshData()
        {
            LoadLogs();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadLogs();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  BUILD UI
        // ══════════════════════════════════════════════════════════════════════
        private void BuildUI()
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
                Text = "📋   User Transaction Logs",
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

            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = BgPage,
                Padding = new Padding(24, 20, 24, 16),
            };
            this.Controls.Add(scroll);

            // Heading
            scroll.Controls.Add(new Label
            {
                Text = "User Transaction Logs",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(0, 0),
                BackColor = Color.Transparent,
            });
            scroll.Controls.Add(new Label
            {
                Text = "All user activity — logins, logouts and transactions",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = TextGray,
                AutoSize = true,
                Location = new Point(2, 36),
                BackColor = Color.Transparent,
            });

            // ── Filter card ───────────────────────────────────────────────────
            var filterCard = Card(new Point(0, 66), new Size(200, 110));
            scroll.Controls.Add(filterCard);

            int fx = 16, fy = 14, gap = 140;

            filterCard.Controls.Add(FL("Search:", fx, fy));
            _txtSearch = FT(fx, fy + 20, 200);
            filterCard.Controls.Add(_txtSearch);
            fx += gap + 60;

            filterCard.Controls.Add(FL("Module:", fx, fy));
            _cboModule = new ComboBox
            {
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(fx, fy + 20),
                Size = new Size(130, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            _cboModule.Items.AddRange(new object[] {
                "All", "Login", "Residents", "Clearance", "Certificate",
                "Blotter", "Ayuda", "Cedula", "Officials", "Budget", "Users", "System"
            });
            _cboModule.SelectedIndex = 0;
            filterCard.Controls.Add(_cboModule);
            fx += 148;

            filterCard.Controls.Add(FL("Action:", fx, fy));
            _cboAction = new ComboBox
            {
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(fx, fy + 20),
                Size = new Size(130, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            _cboAction.Items.AddRange(new object[] {
                "All", "LOGIN", "LOGOUT", "ADD", "UPDATE", "DELETE", "VIEW", "PRINT", "EXPORT"
            });
            _cboAction.SelectedIndex = 0;
            filterCard.Controls.Add(_cboAction);
            fx += 148;

            filterCard.Controls.Add(FL("From:", fx, fy));
            _dtpFrom = new DateTimePicker
            {
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(fx, fy + 20),
                Size = new Size(130, 28),
                Value = DateTime.Now.AddDays(-30),
            };
            filterCard.Controls.Add(_dtpFrom);
            fx += 148;

            filterCard.Controls.Add(FL("To:", fx, fy));
            _dtpTo = new DateTimePicker
            {
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(fx, fy + 20),
                Size = new Size(130, 28),
                Value = DateTime.Now,
            };
            filterCard.Controls.Add(_dtpTo);
            fx += 148;

            var btnFilter = Btn("🔍  Filter", BtnBlue);
            btnFilter.Location = new Point(fx, fy + 18);
            btnFilter.Size = new Size(100, 30);
            btnFilter.Click += (s, e) => LoadLogs();
            filterCard.Controls.Add(btnFilter);

            var btnClear = Btn("Clear", BtnGray);
            btnClear.Location = new Point(fx + 108, fy + 18);
            btnClear.Size = new Size(80, 30);
            btnClear.Click += (s, e) =>
            {
                _txtSearch.Clear();
                _cboModule.SelectedIndex = 0;
                _cboAction.SelectedIndex = 0;
                _dtpFrom.Value = DateTime.Now.AddDays(-30);
                _dtpTo.Value = DateTime.Now;
                LoadLogs();
            };
            filterCard.Controls.Add(btnClear);

            // Second row — count + export
            _lblCount = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = TextGray,
                AutoSize = true,
                Location = new Point(16, 72),
                BackColor = Color.Transparent,
            };
            filterCard.Controls.Add(_lblCount);

            var btnExport = Btn("📋  Export CSV", Color.FromArgb(26, 35, 126));
            btnExport.Location = new Point(fx + 196, fy + 18);
            btnExport.Size = new Size(120, 30);
            btnExport.Click += (s, e) => ExportCSV();
            filterCard.Controls.Add(btnExport);

            // ── Grid ──────────────────────────────────────────────────────────
            _dgv = MakeGrid();
            _dgv.Location = new Point(0, 188);
            scroll.Controls.Add(_dgv);

            scroll.Resize += (s, e) =>
            {
                int w = scroll.ClientSize.Width - scroll.Padding.Horizontal;
                int h = scroll.ClientSize.Height - scroll.Padding.Vertical;
                if (w < 100) return;
                filterCard.Width = w;
                _dgv.Width = w;
                _dgv.Height = h - 196;
            };
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (_topBar != null) _topBar.Visible = !(Parent is TabPage);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  LOAD LOGS
        // ══════════════════════════════════════════════════════════════════════
        private void LoadLogs()
        {
            var conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    string where = "WHERE LoggedAt BETWEEN @from AND @to";
                    if (!string.IsNullOrWhiteSpace(_txtSearch.Text))
                        where += " AND (Username LIKE @s OR Description LIKE @s OR Role LIKE @s)";
                    if (_cboModule.SelectedIndex > 0)
                        where += " AND Module = @module";
                    if (_cboAction.SelectedIndex > 0)
                        where += " AND Action = @action";

                    var da = new SQLiteDataAdapter($@"
                        SELECT
                            idLog        AS ID,
                            LoggedAt     AS DateTime,
                            Username,
                            Role,
                            Action,
                            Module,
                            Description,
                            IPAddress    AS IP
                        FROM auditlog
                        {where}
                        ORDER BY LoggedAt DESC
                        LIMIT 500", conn);

                    da.SelectCommand.Parameters.AddWithValue("@from",
                        _dtpFrom.Value.Date);
                    da.SelectCommand.Parameters.AddWithValue("@to",
                        _dtpTo.Value.Date.AddDays(1).AddSeconds(-1));

                    if (!string.IsNullOrWhiteSpace(_txtSearch.Text))
                        da.SelectCommand.Parameters.AddWithValue("@s", "%" + _txtSearch.Text.Trim() + "%");
                    if (_cboModule.SelectedIndex > 0)
                        da.SelectCommand.Parameters.AddWithValue("@module", _cboModule.SelectedItem.ToString());
                    if (_cboAction.SelectedIndex > 0)
                        da.SelectCommand.Parameters.AddWithValue("@action", _cboAction.SelectedItem.ToString());

                    var dt = new DataTable();
                    da.Fill(dt);
                    _dgv.DataSource = dt;

                    // Column formatting
                    _dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                    if (_dgv.Columns.Contains("ID")) { _dgv.Columns["ID"].Width = 60; }
                    if (_dgv.Columns.Contains("DateTime")) { _dgv.Columns["DateTime"].Width = 150; _dgv.Columns["DateTime"].DefaultCellStyle.Format = "MMM dd, yyyy  hh:mm tt"; }
                    if (_dgv.Columns.Contains("Username")) { _dgv.Columns["Username"].Width = 120; }
                    if (_dgv.Columns.Contains("Role")) { _dgv.Columns["Role"].Width = 100; }
                    if (_dgv.Columns.Contains("Action")) { _dgv.Columns["Action"].Width = 90; }
                    if (_dgv.Columns.Contains("Module")) { _dgv.Columns["Module"].Width = 100; }
                    if (_dgv.Columns.Contains("IP")) { _dgv.Columns["IP"].Width = 110; }
                    if (_dgv.Columns.Contains("Description"))
                        _dgv.Columns["Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                    // Color code by action
                    _dgv.CellFormatting -= ColorRows;
                    _dgv.CellFormatting += ColorRows;

                    _lblCount.Text = $"Showing {dt.Rows.Count} record{(dt.Rows.Count != 1 ? "s" : "")} (max 500)";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading logs:\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ColorRows(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_dgv.Columns.Count <= e.ColumnIndex || e.RowIndex < 0) return;
            if (_dgv.Columns[e.ColumnIndex].Name != "Action" || e.Value == null) return;
            switch (e.Value.ToString())
            {
                case "LOGIN":
                    e.CellStyle.ForeColor = Color.FromArgb(15, 120, 60);
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold); break;
                case "LOGOUT":
                    e.CellStyle.ForeColor = Color.FromArgb(108, 114, 140);
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold); break;
                case "DELETE":
                    e.CellStyle.ForeColor = Color.FromArgb(192, 57, 43);
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold); break;
                case "ADD":
                    e.CellStyle.ForeColor = Color.FromArgb(26, 35, 126);
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold); break;
                case "UPDATE":
                    e.CellStyle.ForeColor = Color.FromArgb(160, 80, 0);
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold); break;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  EXPORT CSV
        // ══════════════════════════════════════════════════════════════════════
        private void ExportCSV()
        {
            if (_dgv.Rows.Count == 0)
            { MessageBox.Show("No records to export.", "Empty", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            using (var dlg = new SaveFileDialog
            {
                Filter = "CSV Files|*.csv",
                FileName = $"AuditLog_{DateTime.Now:yyyyMMdd_HHmm}.csv",
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    var sb = new System.Text.StringBuilder();
                    // Header
                    foreach (DataGridViewColumn col in _dgv.Columns)
                        sb.Append(col.HeaderText + ",");
                    sb.AppendLine();
                    // Rows
                    foreach (DataGridViewRow row in _dgv.Rows)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                            sb.Append($"\"{cell.Value?.ToString()?.Replace("\"", "'")}\",");
                        sb.AppendLine();
                    }
                    System.IO.File.WriteAllText(dlg.FileName, sb.ToString());
                    MessageBox.Show("Exported successfully!\n" + dlg.FileName,
                        "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Export error:\n" + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  UI HELPERS
        // ══════════════════════════════════════════════════════════════════════
        private Panel Card(Point loc, Size size)
        {
            var p = new Panel { BackColor = BgCard, Location = loc, Size = size };
            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(BorderCol))
                    DrawRR(e.Graphics, pen, new Rectangle(0, 0, p.Width - 1, p.Height - 1), 10);
            };
            return p;
        }

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
                RowTemplate = { Height = 36 },
                Font = new Font("Segoe UI", 9.5f),
                GridColor = Color.FromArgb(224, 228, 242),
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 40,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = NavBlue;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            g.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            g.DefaultCellStyle.SelectionBackColor = BtnBlue;
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.AlternatingRowsDefaultCellStyle.BackColor = LightBlue;
            return g;
        }

        private Button Btn(string text, Color bg)
        {
            var b = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
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
                ForeColor = Color.FromArgb(55, 65, 100),
                AutoSize = true,
                Location = new Point(x, y),
                BackColor = Color.Transparent,
            };

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // UserTransactionLogsControl
            // 
            this.Name = "UserTransactionLogsControl";
            this.Load += new System.EventHandler(this.UserTransactionLogsControl_Load);
            this.ResumeLayout(false);

        }

        private void UserTransactionLogsControl_Load(object sender, EventArgs e)
        {

        }

        private static TextBox FT(int x, int y, int w) =>
            new TextBox
            {
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(x, y),
                Size = new Size(w, 28),
                BorderStyle = BorderStyle.FixedSingle,
            };

        private static void DrawRR(Graphics g, Pen pen, Rectangle r, int radius)
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
    }
}