// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  TaxCollectionControl.cs                                                 ║
// ║  Read-only report of cedula (Community Tax Certificate) records across   ║
// ║  every resident. Shown inside ResidentManagementControl's content area   ║
// ║  for Treasurer (default landing view) and Super Admin (via toolbar       ║
// ║  button). Add as New Item → Class.                                       ║
// ║                                                                            ║
// ║  UPDATE: Added Year / Month / Status filter dropdowns next to the search  ║
// ║  box, a "Clear Filters" shortcut, and a "Print Report" button that opens  ║
// ║  a print preview of whatever rows are currently visible (search + filter ║
// ║  applied).                                                                ║
// ╚══════════════════════════════════════════════════════════════════════════╝

using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;

namespace BarangayManagementSystem.Controls
{
    public class TaxCollectionControl : UserControl
    {
        // ── Colors (matches the rest of Resident Management) ────────────────────
        private static readonly Color NavBlue = Color.FromArgb(26, 35, 126);
        private static readonly Color BgPage = Color.FromArgb(240, 242, 248);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderCol = Color.FromArgb(220, 225, 238);
        private static readonly Color TextGray = Color.FromArgb(108, 114, 140);
        private static readonly Color BtnBlue = Color.FromArgb(66, 133, 244);
        private static readonly Color BtnGreen = Color.FromArgb(15, 157, 88);
        private static readonly Color BtnGray = Color.FromArgb(120, 130, 155);
        private static readonly Color BtnPurple = Color.FromArgb(94, 84, 191);
        private static readonly Color StatBg = Color.FromArgb(238, 237, 254);
        private static readonly Color StatFg = Color.FromArgb(60, 52, 137);

        private const string AllYears = "All Years";
        private const string AllMonths = "All Months";
        private const string AllStatus = "All Status";
        private static readonly string[] MonthNames =
        {
            "January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December"
        };

        private DataGridView _dgv;
        private TextBox _txtSearch;
        private ComboBox _cboYear, _cboMonth, _cboStatus;
        private DataTable _fullTable;
        private DataTable _currentTable;   // currently visible rows (search + filters applied) — used for printing
        private Label _lblTotalCount, _lblTotalYear, _lblTotalMonth;

        // print pagination state
        private int _printRowIndex;
        private readonly Font _printTitleFont = new Font("Segoe UI", 14f, FontStyle.Bold);
        private readonly Font _printMetaFont = new Font("Segoe UI", 8.5f);
        private readonly Font _printColFont = new Font("Segoe UI", 9f, FontStyle.Bold);
        private readonly Font _printRowFont = new Font("Segoe UI", 8.5f);

        public TaxCollectionControl()
        {
            this.DoubleBuffered = true;
            this.BackColor = BgPage;
            this.Dock = DockStyle.Fill;
            BuildUI();
            LoadData();

            AuditLogger.Log("VIEW", "Tax Collection", "Opened Tax Collection · Cedula report");
        }

        // ══════════════════════════════════════════════════════════════════════
        //  BUILD UI
        // ══════════════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            // ── Header ────────────────────────────────────────────────────────
            var header = new Panel { Dock = DockStyle.Top, Height = 148, BackColor = BgPage };
            this.Controls.Add(header);

            var lblTitle = new Label
            {
                Text = "🧾  Tax Collection · Cedula",
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(0, 4),
                BackColor = Color.Transparent,
            };
            header.Controls.Add(lblTitle);

            var lblSub = new Label
            {
                Text = "Community Tax Certificates issued across all residents",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = TextGray,
                AutoSize = true,
                Location = new Point(2, 34),
                BackColor = Color.Transparent,
            };
            header.Controls.Add(lblSub);

            // ── Row 1: search + filters ─────────────────────────────────────
            _txtSearch = new TextBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(0, 62),
                Size = new Size(210, 28),
                BorderStyle = BorderStyle.FixedSingle,
            };
            _txtSearch.TextChanged += (s, e) => ApplyFilter();
            header.Controls.Add(_txtSearch);
            AddPlaceholder(_txtSearch, "Search resident or cedula no...");

            _cboYear = MakeFilterCombo(220, 63, 110);
            _cboYear.SelectedIndexChanged += (s, e) => ApplyFilter();
            header.Controls.Add(_cboYear);

            _cboMonth = MakeFilterCombo(340, 63, 130);
            _cboMonth.SelectedIndexChanged += (s, e) => ApplyFilter();
            header.Controls.Add(_cboMonth);

            _cboStatus = MakeFilterCombo(480, 63, 130);
            _cboStatus.SelectedIndexChanged += (s, e) => ApplyFilter();
            header.Controls.Add(_cboStatus);

            var btnClear = MakeBtn("✕  Clear Filters", BtnGray);
            btnClear.Size = new Size(130, 28);
            btnClear.Location = new Point(620, 63);
            btnClear.Click += (s, e) => ClearFilters();
            header.Controls.Add(btnClear);

            // ── Row 2: actions ───────────────────────────────────────────────
            var btnIssue = MakeBtn("🪪  Issue New Cedula", BtnGreen);
            btnIssue.Size = new Size(180, 34);
            btnIssue.Location = new Point(0, 102);
            btnIssue.Click += (s, e) => IssueNewCedula();
            header.Controls.Add(btnIssue);

            var btnPrint = MakeBtn("🖨  Print Report", BtnPurple);
            btnPrint.Size = new Size(150, 34);
            btnPrint.Location = new Point(192, 102);
            btnPrint.Click += (s, e) => PrintReport();
            header.Controls.Add(btnPrint);

            var btnRefresh = MakeBtn("⟳  Refresh", BtnBlue);
            btnRefresh.Size = new Size(110, 34);
            btnRefresh.Location = new Point(354, 102);
            btnRefresh.Click += (s, e) => LoadData();
            header.Controls.Add(btnRefresh);

            // ── Summary stat cards ───────────────────────────────────────────
            var statsRow = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = BgPage, Padding = new Padding(0, 0, 0, 12) };
            this.Controls.Add(statsRow);

            var statAll = MakeStatCard("Total Cedulas Issued", out _lblTotalCount);
            statAll.Location = new Point(0, 0);
            statsRow.Controls.Add(statAll);

            var statYear = MakeStatCard("Collected This Year", out _lblTotalYear);
            statYear.Location = new Point(276, 0);
            statsRow.Controls.Add(statYear);

            var statMonth = MakeStatCard("Collected This Month", out _lblTotalMonth);
            statMonth.Location = new Point(552, 0);
            statsRow.Controls.Add(statMonth);

            // ── Grid ──────────────────────────────────────────────────────────
            var gridCard = new Panel { Dock = DockStyle.Fill, BackColor = BgCard };
            gridCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(BorderCol))
                    DrawRR(e.Graphics, pen, new Rectangle(0, 0, gridCard.Width - 1, gridCard.Height - 1), 8);
            };
            this.Controls.Add(gridCard);

            _dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = BgCard,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9.5f),
                ColumnHeadersHeight = 34,
                ColumnHeadersDefaultCellStyle = { BackColor = Color.FromArgb(245, 246, 250), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = NavBlue },
                Margin = new Padding(16),
            };
            gridCard.Padding = new Padding(16);
            gridCard.Controls.Add(_dgv);

            // Order: header (top), statsRow (top), gridCard (fill) — must be
            // added so gridCard fills what's left after the docked tops.
            this.Controls.SetChildIndex(header, 2);
            this.Controls.SetChildIndex(statsRow, 1);
            this.Controls.SetChildIndex(gridCard, 0);
        }

        private ComboBox MakeFilterCombo(int x, int y, int w)
        {
            return new ComboBox
            {
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(x, y),
                Size = new Size(w, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
            };
        }

        // Lightweight placeholder text for the search box (no watermark support built into WinForms TextBox).
        private void AddPlaceholder(TextBox box, string placeholder)
        {
            box.Text = placeholder;
            box.ForeColor = TextGray;
            box.GotFocus += (s, e) =>
            {
                if (box.Text == placeholder)
                {
                    box.Text = "";
                    box.ForeColor = Color.Black;
                }
            };
            box.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(box.Text))
                {
                    box.Text = placeholder;
                    box.ForeColor = TextGray;
                }
            };
        }

        private Panel MakeStatCard(string caption, out Label numLabel)
        {
            var p = new Panel { Size = new Size(260, 64), BackColor = StatBg };
            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(BorderCol))
                    DrawRR(e.Graphics, pen, new Rectangle(0, 0, p.Width - 1, p.Height - 1), 8);
            };

            var num = new Label
            {
                Text = "—",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = StatFg,
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 38,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
            };
            var cap = new Label
            {
                Text = caption,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = StatFg,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter,
                BackColor = Color.Transparent,
            };
            p.Controls.Add(cap);
            p.Controls.Add(num);
            numLabel = num;
            return p;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  DATA
        // ══════════════════════════════════════════════════════════════════════
        private void LoadData()
        {
            var conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    const string sql = @"
                        SELECT c.ResidentID,
                               CONCAT(r.LastName, ', ', r.FirstName,
                                   IIF(r.MiddleName IS NOT NULL AND r.MiddleName != '',
                                      CONCAT(' ', r.MiddleName), '')) AS Resident,
                               c.CedulaNumber   AS `Cedula No.`,
                               c.SerialNumber   AS `Serial No.`,
                               c.DateIssued     AS `Date Issued`,
                               c.AmountPaid     AS `Amount Paid`,
                               c.IssuedBy       AS `Issued By`,
                               c.Remarks        AS Status
                        FROM cedula c
                        JOIN resident r ON r.idResident = c.ResidentID
                        ORDER BY c.DateIssued DESC";

                    var da = new SQLiteDataAdapter(sql, conn);
                    _fullTable = new DataTable();
                    da.Fill(_fullTable);

                    PopulateFilterOptions();
                    ApplyFilter();
                    UpdateStats();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading cedula records:\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Rebuilds the Year / Month / Status dropdown contents from the loaded
        // data while trying to preserve whatever the user currently has
        // selected (e.g. after hitting Refresh).
        private void PopulateFilterOptions()
        {
            if (_fullTable == null) return;

            string prevYear = _cboYear.SelectedItem?.ToString();
            string prevMonth = _cboMonth.SelectedItem?.ToString();
            string prevStatus = _cboStatus.SelectedItem?.ToString();

            var years = _fullTable.AsEnumerable()
                .Where(r => r["Date Issued"] != DBNull.Value)
                .Select(r => Convert.ToDateTime(r["Date Issued"]).Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            _cboYear.Items.Clear();
            _cboYear.Items.Add(AllYears);
            foreach (var y in years) _cboYear.Items.Add(y.ToString());
            _cboYear.SelectedItem = (prevYear != null && _cboYear.Items.Contains(prevYear)) ? prevYear : AllYears;

            _cboMonth.Items.Clear();
            _cboMonth.Items.Add(AllMonths);
            foreach (var m in MonthNames) _cboMonth.Items.Add(m);
            _cboMonth.SelectedItem = (prevMonth != null && _cboMonth.Items.Contains(prevMonth)) ? prevMonth : AllMonths;

            var statuses = _fullTable.AsEnumerable()
                .Select(r => r["Status"]?.ToString())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            _cboStatus.Items.Clear();
            _cboStatus.Items.Add(AllStatus);
            foreach (var st in statuses) _cboStatus.Items.Add(st);
            _cboStatus.SelectedItem = (prevStatus != null && _cboStatus.Items.Contains(prevStatus)) ? prevStatus : AllStatus;
        }

        private void ClearFilters()
        {
            _txtSearch.Text = "";
            _txtSearch.ForeColor = TextGray; // placeholder resets on LostFocus, but reset color immediately too
            if (_cboYear.Items.Count > 0) _cboYear.SelectedItem = AllYears;
            if (_cboMonth.Items.Count > 0) _cboMonth.SelectedItem = AllMonths;
            if (_cboStatus.Items.Count > 0) _cboStatus.SelectedItem = AllStatus;
            ApplyFilter();
        }

        // Combines the free-text search box with the Year / Month / Status
        // dropdowns. Filtering is done in code (rather than DataView.RowFilter)
        // so we can match on the Year/Month parts of a DateTime column, which
        // RowFilter expressions can't do directly.
        private void ApplyFilter()
        {
            if (_fullTable == null) return;

            string term = (_txtSearch.Text ?? "").Trim();
            bool isPlaceholder = term == "Search resident or cedula no...";
            term = isPlaceholder ? "" : term.ToLowerInvariant();

            string yearSel = _cboYear.SelectedItem?.ToString() ?? AllYears;
            string monthSel = _cboMonth.SelectedItem?.ToString() ?? AllMonths;
            string statusSel = _cboStatus.SelectedItem?.ToString() ?? AllStatus;

            DataTable filtered = _fullTable.Clone();
            foreach (DataRow row in _fullTable.Rows)
            {
                if (!string.IsNullOrEmpty(term))
                {
                    string resident = row["Resident"]?.ToString().ToLowerInvariant() ?? "";
                    string cedula = row["Cedula No."]?.ToString().ToLowerInvariant() ?? "";
                    if (!resident.Contains(term) && !cedula.Contains(term)) continue;
                }

                DateTime? dateIssued = row["Date Issued"] != DBNull.Value
                    ? Convert.ToDateTime(row["Date Issued"])
                    : (DateTime?)null;

                if (yearSel != AllYears)
                {
                    if (dateIssued == null || dateIssued.Value.Year.ToString() != yearSel) continue;
                }

                if (monthSel != AllMonths)
                {
                    if (dateIssued == null || dateIssued.Value.ToString("MMMM") != monthSel) continue;
                }

                if (statusSel != AllStatus)
                {
                    if ((row["Status"]?.ToString() ?? "") != statusSel) continue;
                }

                filtered.ImportRow(row);
            }

            _currentTable = filtered;
            _dgv.DataSource = _currentTable.DefaultView;

            if (_dgv.Columns.Contains("Amount Paid"))
                _dgv.Columns["Amount Paid"].DefaultCellStyle.Format = "₱#,##0.00";
            if (_dgv.Columns.Contains("Date Issued"))
                _dgv.Columns["Date Issued"].DefaultCellStyle.Format = "MMM dd, yyyy";
        }

        private void UpdateStats()
        {
            if (_fullTable == null) return;

            int totalCount = _fullTable.Rows.Count;
            decimal totalYear = 0m, totalMonth = 0m;
            var now = DateTime.Now;

            foreach (DataRow row in _fullTable.Rows)
            {
                if (row["Date Issued"] == DBNull.Value || row["Amount Paid"] == DBNull.Value) continue;
                DateTime dt = Convert.ToDateTime(row["Date Issued"]);
                decimal amt = Convert.ToDecimal(row["Amount Paid"]);

                if (dt.Year == now.Year) totalYear += amt;
                if (dt.Year == now.Year && dt.Month == now.Month) totalMonth += amt;
            }

            _lblTotalCount.Text = totalCount.ToString("N0");
            _lblTotalYear.Text = "₱" + totalYear.ToString("N2");
            _lblTotalMonth.Text = "₱" + totalMonth.ToString("N2");
        }

        private void IssueNewCedula()
        {
            using (var picker = new ResidentPickerDialog())
            {
                if (picker.ShowDialog(this.FindForm()) == DialogResult.OK && picker.SelectedResidentId > 0)
                {
                    new CedulaForm(picker.SelectedResidentId, "").ShowDialog(this.FindForm());
                    LoadData();
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PRINT
        // ══════════════════════════════════════════════════════════════════════
        private void PrintReport()
        {
            if (_currentTable == null || _currentTable.Rows.Count == 0)
            {
                MessageBox.Show("There are no records to print for the current search/filter.",
                    "Nothing to Print", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var pd = new PrintDocument();
            pd.DefaultPageSettings.Landscape = true;
            _printRowIndex = 0;
            pd.PrintPage += PrintDocument_PrintPage;

            using (var preview = new PrintPreviewDialog())
            {
                preview.Document = pd;
                preview.Width = 1000;
                preview.Height = 720;
                preview.StartPosition = FormStartPosition.CenterParent;
                preview.UseAntiAlias = true;

                AuditLogger.Log("PRINT", "Tax Collection",
                    $"Printed Tax Collection · Cedula report ({_currentTable.Rows.Count} record(s), filter: {BuildFilterSummaryText()})");

                preview.ShowDialog(this.FindForm());
            }
        }

        private string BuildFilterSummaryText()
        {
            var parts = new List<string>();

            string yearSel = _cboYear.SelectedItem?.ToString() ?? AllYears;
            string monthSel = _cboMonth.SelectedItem?.ToString() ?? AllMonths;
            string statusSel = _cboStatus.SelectedItem?.ToString() ?? AllStatus;
            string term = (_txtSearch.Text ?? "").Trim();

            if (yearSel != AllYears) parts.Add(yearSel);
            if (monthSel != AllMonths) parts.Add(monthSel);
            if (statusSel != AllStatus) parts.Add(statusSel);
            if (!string.IsNullOrWhiteSpace(term) && term != "Search resident or cedula no...")
                parts.Add("Search: \"" + term + "\"");

            return parts.Count == 0 ? "All records" : string.Join("  ·  ", parts);
        }

        private static readonly string[] PrintHeaders =
            { "ResidentID", "Resident", "Cedula No.", "Serial No.", "Date Issued", "Amount Paid", "Issued By", "Status" };
        private static readonly float[] PrintColWidths =
            { 0.07f, 0.24f, 0.13f, 0.13f, 0.13f, 0.12f, 0.11f, 0.07f }; // fractions of usable width

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            var g = e.Graphics;
            float x = e.MarginBounds.Left;
            float y = e.MarginBounds.Top;
            float width = e.MarginBounds.Width;

            if (_printRowIndex == 0)
            {
                g.DrawString("Barangay Tax Collection · Cedula Report", _printTitleFont, Brushes.Black, x, y);
                y += 26;
                g.DrawString("Generated: " + DateTime.Now.ToString("MMM dd, yyyy hh:mm tt"), _printMetaFont, Brushes.Gray, x, y);
                y += 14;
                g.DrawString("Filter: " + BuildFilterSummaryText(), _printMetaFont, Brushes.Gray, x, y);
                y += 14;

                decimal totalAmount = _currentTable.AsEnumerable()
                    .Where(r => r["Amount Paid"] != DBNull.Value)
                    .Sum(r => Convert.ToDecimal(r["Amount Paid"]));
                g.DrawString($"Records: {_currentTable.Rows.Count}     Total Amount: ₱{totalAmount:N2}",
                    new Font("Segoe UI", 9f, FontStyle.Bold), Brushes.Black, x, y);
                y += 22;
            }

            // column headers
            float cx = x;
            for (int c = 0; c < PrintHeaders.Length; c++)
            {
                g.DrawString(PrintHeaders[c], _printColFont, Brushes.Black, cx, y);
                cx += width * PrintColWidths[c];
            }
            y += 18;
            g.DrawLine(Pens.Black, x, y, x + width, y);
            y += 4;

            DataRowCollection rows = _currentTable.Rows;
            while (_printRowIndex < rows.Count)
            {
                if (y > e.MarginBounds.Bottom - 20)
                {
                    e.HasMorePages = true;
                    return;
                }

                DataRow row = rows[_printRowIndex];
                string[] vals =
                {
                    row["ResidentID"]?.ToString() ?? "",
                    row["Resident"]?.ToString() ?? "",
                    row["Cedula No."]?.ToString() ?? "",
                    row["Serial No."]?.ToString() ?? "",
                    row["Date Issued"] != DBNull.Value ? Convert.ToDateTime(row["Date Issued"]).ToString("MMM dd, yyyy") : "",
                    row["Amount Paid"] != DBNull.Value ? "₱" + Convert.ToDecimal(row["Amount Paid"]).ToString("N2") : "",
                    row["Issued By"]?.ToString() ?? "",
                    row["Status"]?.ToString() ?? "",
                };

                cx = x;
                for (int c = 0; c < vals.Length; c++)
                {
                    g.DrawString(vals[c], _printRowFont, Brushes.Black, cx, y);
                    cx += width * PrintColWidths[c];
                }

                y += 16;
                _printRowIndex++;
            }

            e.HasMorePages = false;
        }

        // ── UI helpers ────────────────────────────────────────────────────────
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
            return b;
        }

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
