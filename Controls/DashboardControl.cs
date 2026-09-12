using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BarangayManagementSystem.Controls
{
    public class DashboardControl : UserControl
    {
        // ── Barangay info labels ───────────────────────────────────────────────
        private Label lblBarangayName;
        private Label lblBarangayAddress;

        // ── DB-bound stat labels ──────────────────────────────────────────────
        private Label lblTotalResidents;
        private Label lblTotalOfficials;
        private Label lblActiveUsers;
        private Label lblPendingBlotters;
        private Label lblPendingClearances;
        private Label lblTotalAyuda;

        // ── Trend labels ──────────────────────────────────────────────────────
        private Label lblTrendResidents;
        private Label lblTrendBlotters;
        private Label lblTrendClearances;
        private Label lblTrendAyuda;

        // ── Progress bar panels ───────────────────────────────────────────────
        private Panel pbClearance, pbCertificate, pbBlotter, pbAyuda;
        private Panel pbSenior, pbPwd, pbFourPs;
        private Label lblPctClearance, lblPctCertificate, lblPctBlotter, lblPctAyuda;
        private Label lblPctSenior, lblPctPwd, lblPctFourPs;

        // ── Monthly chart data ────────────────────────────────────────────────
        private Panel _chartPanel;
        private int[] _clrData = { 0, 0, 0, 0, 0, 0 };
        private int[] _certData = { 0, 0, 0, 0, 0, 0 };
        private int[] _bltData = { 0, 0, 0, 0, 0, 0 };

        // ── Grid ─────────────────────────────────────────────────────────────
        private DataGridView dgvRecentActivity;

        // ── Layout refs ───────────────────────────────────────────────────────
        private Panel _topBar;
        private Panel _scrollPanel;
        private Panel _infoCard;
        private Panel[] _cardPanels;
        private Label[] _cardValues;
        private Panel _actCard;
        private Panel _chartCard;
        private Panel _statusCard;

        // ── Colors ────────────────────────────────────────────────────────────
        private readonly Color BgPage = Color.FromArgb(240, 242, 248);
        private readonly Color BgCard = Color.White;
        private readonly Color NavBlue = Color.FromArgb(26, 35, 126);
        private readonly Color TextDark = Color.FromArgb(25, 25, 40);
        private readonly Color TextGray = Color.FromArgb(110, 115, 135);
        private readonly Color TrendUp = Color.FromArgb(15, 110, 86);
        private readonly Color TrendDown = Color.FromArgb(153, 60, 29);

        // ── Card definitions ──────────────────────────────────────────────────
        private readonly (string Title, Color Accent, string Icon)[] _cards =
            new (string, Color, string)[]
        {
            ("Total Residents",    Color.FromArgb(0,   102, 204), "👥"),
            ("Total Officials",    Color.FromArgb(0,   153, 102), "🏛"),
            ("Active Users",       Color.FromArgb(111,  66, 193), "👤"),
            ("Pending Blotters",   Color.FromArgb(220,  53,  69), "📋"),
            ("Pending Clearances", Color.FromArgb(230, 130,   0), "📄"),
            ("Ayuda Released",     Color.FromArgb(0,   123, 200), "💰"),
        };

        public DashboardControl()
        {
            this.DoubleBuffered = true;
            this.BackColor = BgPage;
            BuildUI();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadBarangayInfo();
            LoadDashboardStatistics();
            LoadMonthlyData();
            LoadRecentActivities();
        }

        // ═════════════════════════════════════════════════════════════════════
        //  BUILD UI
        // ═════════════════════════════════════════════════════════════════════
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
                Text = "📊   Analytics Dashboard",
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

            // ── Scroll panel fills everything below the top bar ───────────────
            // NOTE: equal padding on left/right (20,20) — no right-side gap
            _scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = BgPage,
                Padding = new Padding(20, 16, 20, 24),
            };
            this.Controls.Add(_scrollPanel);

            // ── Page heading ──────────────────────────────────────────────────
            var lblTitle = new Label
            {
                Text = "Analytics",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            _scrollPanel.Controls.Add(lblTitle);

            var lblSub = new Label
            {
                Text = "Overview  —  " + DateTime.Now.ToString("MMMM dd, yyyy"),
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = TextGray,
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            _scrollPanel.Controls.Add(lblSub);

            // ── Barangay Info Card ────────────────────────────────────────────
            _infoCard = new Panel { BackColor = BgCard };
            _infoCard.Paint += (s, e) => PaintInfoCard(e.Graphics, _infoCard);

            lblBarangayName = new Label
            {
                Text = "Loading...",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = false,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            _infoCard.Controls.Add(lblBarangayName);

            lblBarangayAddress = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9f),
                ForeColor = TextGray,
                AutoSize = false,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.TopLeft,
            };
            _infoCard.Controls.Add(lblBarangayAddress);
            _scrollPanel.Controls.Add(_infoCard);

            // ── 6 stat cards ──────────────────────────────────────────────────
            _cardPanels = new Panel[6];
            _cardValues = new Label[6];
            Label[] trendLabels = new Label[6];

            for (int i = 0; i < 6; i++)
            {
                int idx = i;
                var card = new Panel { BackColor = BgCard };
                card.Paint += (s, e) => PaintStatCard(e.Graphics, card, idx);
                _cardPanels[i] = card;

                var val = new Label
                {
                    Text = "—",
                    Font = new Font("Segoe UI", 26f, FontStyle.Bold),
                    ForeColor = _cards[i].Accent,
                    AutoSize = false,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft,
                };
                card.Controls.Add(val);
                _cardValues[i] = val;

                var trend = new Label
                {
                    Text = "",
                    Font = new Font("Segoe UI", 8f),
                    ForeColor = TextGray,
                    AutoSize = false,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft,
                };
                card.Controls.Add(trend);
                trendLabels[i] = trend;

                _scrollPanel.Controls.Add(card);
            }

            lblTotalResidents = _cardValues[0];
            lblTotalOfficials = _cardValues[1];
            lblActiveUsers = _cardValues[2];
            lblPendingBlotters = _cardValues[3];
            lblPendingClearances = _cardValues[4];
            lblTotalAyuda = _cardValues[5];

            lblTrendResidents = trendLabels[0];
            lblTrendBlotters = trendLabels[3];
            lblTrendClearances = trendLabels[4];
            lblTrendAyuda = trendLabels[5];

            // ── Row 2: Monthly Chart + Status Breakdown ───────────────────────
            _chartCard = new Panel { BackColor = BgCard };
            _chartCard.Paint += (s, e) => PaintCardHeader(e.Graphics, _chartCard, "Monthly Document Issuances");
            _scrollPanel.Controls.Add(_chartCard);

            var legendPanel = new Panel { BackColor = Color.Transparent, Tag = "legend" };
            AddLegendDot(legendPanel, Color.FromArgb(24, 95, 165), "Clearances", 0);
            AddLegendDot(legendPanel, Color.FromArgb(59, 109, 17), "Certificates", 82);
            AddLegendDot(legendPanel, Color.FromArgb(230, 130, 0), "Blotters", 168);
            _chartCard.Controls.Add(legendPanel);

            _chartPanel = new Panel { BackColor = Color.Transparent };
            _chartPanel.Paint += OnChartPaint;
            _chartCard.Controls.Add(_chartPanel);

            _statusCard = new Panel { BackColor = BgCard };
            _statusCard.Paint += (s, e) => PaintCardHeader(e.Graphics, _statusCard, "Document Status Breakdown");
            _scrollPanel.Controls.Add(_statusCard);
            BuildProgressSection(_statusCard);

            // ── Row 3: Recent Activities — full width, no Quick Actions ────────
            _actCard = new Panel { BackColor = BgCard };
            _actCard.Paint += (s, e) => PaintCardHeader(e.Graphics, _actCard, "Recent Activities");
            _scrollPanel.Controls.Add(_actCard);

            dgvRecentActivity = BuildGrid();
            _actCard.Controls.Add(dgvRecentActivity);

            // ── Resize ────────────────────────────────────────────────────────
            _scrollPanel.Resize += (s, e) => LayoutAll();
            this.Resize += (s, e) => LayoutAll();
        }

        private void AddLegendDot(Panel parent, Color color, string text, int x)
        {
            var dot = new Panel
            {
                BackColor = color,
                Size = new Size(10, 10),
                Location = new Point(x, 3),
            };
            parent.Controls.Add(dot);

            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = TextGray,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(x + 14, 0),
            };
            parent.Controls.Add(lbl);
        }

        private void BuildProgressSection(Panel statusCard)
        {
            pbClearance = MakeBar(Color.FromArgb(24, 95, 165));
            pbCertificate = MakeBar(Color.FromArgb(59, 109, 17));
            pbBlotter = MakeBar(Color.FromArgb(220, 53, 69));
            pbAyuda = MakeBar(Color.FromArgb(0, 123, 200));
            pbSenior = MakeBar(Color.FromArgb(111, 66, 193));
            pbPwd = MakeBar(Color.FromArgb(0, 153, 102));
            pbFourPs = MakeBar(Color.FromArgb(230, 130, 0));

            lblPctClearance = MakePctLabel();
            lblPctCertificate = MakePctLabel();
            lblPctBlotter = MakePctLabel();
            lblPctAyuda = MakePctLabel();
            lblPctSenior = MakePctLabel();
            lblPctPwd = MakePctLabel();
            lblPctFourPs = MakePctLabel();

            SetPct(pbClearance, lblPctClearance, 87);
            SetPct(pbCertificate, lblPctCertificate, 72);
            SetPct(pbBlotter, lblPctBlotter, 55);
            SetPct(pbAyuda, lblPctAyuda, 91);
            SetPct(pbSenior, lblPctSenior, 18);
            SetPct(pbPwd, lblPctPwd, 8);
            SetPct(pbFourPs, lblPctFourPs, 24);

            statusCard.Controls.AddRange(new Control[]
            {
                pbClearance, pbCertificate, pbBlotter, pbAyuda,
                pbSenior, pbPwd, pbFourPs,
                lblPctClearance, lblPctCertificate, lblPctBlotter, lblPctAyuda,
                lblPctSenior, lblPctPwd, lblPctFourPs,
            });

            string[] rowNames =
            {
                "Clearances Issued", "Certificates Issued", "Blotters Resolved",
                "Ayuda Distributed", "Senior Citizens", "PWD Registered", "4Ps Beneficiaries"
            };
            foreach (var name in rowNames)
            {
                var lbl = new Label
                {
                    Text = name,
                    Font = new Font("Segoe UI", 8.5f),
                    ForeColor = TextGray,
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Tag = "progLabel_" + name,
                };
                statusCard.Controls.Add(lbl);
            }
        }

        private Panel MakeBar(Color color)
        {
            var bg = new Panel { BackColor = Color.FromArgb(238, 240, 246), Tag = 0 };
            var fill = new Panel { BackColor = color, Dock = DockStyle.Left, Width = 0 };
            bg.Controls.Add(fill);
            return bg;
        }

        private Label MakePctLabel() => new Label
        {
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            ForeColor = TextDark,
            AutoSize = true,
            BackColor = Color.Transparent,
            Text = "0%",
        };

        private void SetPct(Panel bar, Label pctLbl, int pct)
        {
            bar.Tag = pct;
            pctLbl.Text = pct + "%";
        }

        private DataGridView BuildGrid()
        {
            var dgv = new DataGridView
            {
                Location = new Point(0, 44),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = BgCard,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                GridColor = Color.FromArgb(225, 225, 232),
                RowTemplate = { Height = 34 },
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                // Anchor fill — fills the card horizontally too
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor = NavBlue;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            dgv.ColumnHeadersHeight = 38;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            dgv.DefaultCellStyle.ForeColor = TextDark;
            dgv.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(225, 230, 255);
            dgv.DefaultCellStyle.SelectionForeColor = TextDark;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 248, 253);

            dgv.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex < 0 || e.RowIndex < 0) return;
                if (dgv.Columns[e.ColumnIndex].Name != "Type" || e.Value == null) return;
                switch (e.Value.ToString())
                {
                    case "Clearance":
                        e.CellStyle.ForeColor = Color.FromArgb(0, 102, 204);
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold); break;
                    case "Certificate":
                        e.CellStyle.ForeColor = Color.FromArgb(0, 153, 102);
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold); break;
                    case "Blotter":
                        e.CellStyle.ForeColor = Color.FromArgb(220, 53, 69);
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold); break;
                    case "Ayuda":
                        e.CellStyle.ForeColor = Color.FromArgb(230, 130, 0);
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold); break;
                }
            };
            return dgv;
        }

        // ═════════════════════════════════════════════════════════════════════
        //  LAYOUT — all widths now use (avail) with NO subtracted remainder
        // ═════════════════════════════════════════════════════════════════════
        private void LayoutAll()
        {
            // avail = full inner width of scroll panel (padding already excluded)
            int pad = _scrollPanel.Padding.Left;   // symmetric left=right=20
            int avail = _scrollPanel.ClientSize.Width - pad * 2;
            if (avail < 200) return;

            const int gap = 12;
            const int cardH = 100;
            int y = 0;

            // ── Page title + subtitle ─────────────────────────────────────────
            foreach (Control c in _scrollPanel.Controls)
            {
                if (c is Label l && l.Font.Size == 20f)
                    l.Location = new Point(0, y);
                if (c is Label l2 && l2.Font.Size == 9.5f && l2.Text.StartsWith("Overview"))
                    l2.Location = new Point(2, y + 36);
            }
            y += 68;

            // ── Row 1: Barangay Info card (fixed 200px) + 6 stat cards ────────
            const int infoW = 200;
            // 3 stat cards per row, fill remaining width exactly
            int statTotal = avail - infoW - gap;          // space for 3 cards + 2 gaps
            int cW = (statTotal - 2 * gap) / 3;
            int twoRowH = 2 * cardH + gap;

            _infoCard.SetBounds(0, y, infoW, twoRowH);
            lblBarangayName.SetBounds(14, 50, infoW - 28, 28);
            lblBarangayAddress.SetBounds(14, 84, infoW - 28, twoRowH - 96);

            for (int i = 0; i < 6; i++)
            {
                int col = i % 3, row = i / 3;
                int cx = infoW + gap + col * (cW + gap);
                int cy = y + row * (cardH + gap);
                _cardPanels[i].SetBounds(cx, cy, cW, cardH);
                _cardValues[i].SetBounds(18, 28, cW - 76, 42);

                foreach (Control c in _cardPanels[i].Controls)
                    if (c is Label tl && tl != _cardValues[i])
                        tl.SetBounds(18, 72, cW - 76, 18);
            }
            y += twoRowH + gap;

            // ── Row 2: Monthly Chart (60%) + Status Breakdown (40%) ────────────
            int chartW = (int)(avail * 0.60);
            int statusW = avail - chartW - gap;   // fills exactly to avail
            int row2H = 260;

            _chartCard.SetBounds(0, y, chartW, row2H);
            foreach (Control c in _chartCard.Controls)
                if (c is Panel lp && lp.Tag?.ToString() == "legend")
                    lp.SetBounds(16, 48, chartW - 32, 18);

            _chartPanel.SetBounds(0, 70, chartW, row2H - 70);
            _chartPanel.Invalidate();

            _statusCard.SetBounds(chartW + gap, y, statusW, row2H);
            LayoutProgressBars(_statusCard, statusW, row2H);

            y += row2H + gap;

            // ── Row 3: Recent Activities — full width, fixed height 320 ────────
            // Use a fixed generous height so it never gets clipped during scroll
            const int row3H = 320;
            _actCard.SetBounds(0, y, avail, row3H);
            dgvRecentActivity.SetBounds(0, 44, avail, row3H - 44);

            // Tell scroll panel the total content height so scrollbar works right
            _scrollPanel.AutoScrollMinSize = new Size(avail, y + row3H + _scrollPanel.Padding.Bottom);
        }

        private void LayoutProgressBars(Panel card, int w, int h)
        {
            int innerW = w - 32;
            int startY = 50;
            const int rowH = 27;

            Panel[] bars = { pbClearance, pbCertificate, pbBlotter, pbAyuda, pbSenior, pbPwd, pbFourPs };
            Label[] pcts = { lblPctClearance, lblPctCertificate, lblPctBlotter, lblPctAyuda, lblPctSenior, lblPctPwd, lblPctFourPs };
            string[] names =
            {
                "Clearances Issued", "Certificates Issued", "Blotters Resolved",
                "Ayuda Distributed", "Senior Citizens", "PWD Registered", "4Ps Beneficiaries"
            };

            for (int i = 0; i < bars.Length; i++)
            {
                if (i == 4) startY += 10;

                foreach (Control c in card.Controls)
                    if (c is Label lbl && lbl.Tag is string t && t == "progLabel_" + names[i])
                    { lbl.SetBounds(16, startY, innerW - 36, 14); break; }

                pcts[i].SetBounds(w - 44, startY, 36, 14);
                bars[i].SetBounds(16, startY + 15, innerW, 6);

                int pct = (int)(bars[i].Tag ?? 0);
                int fillW = Math.Max(0, (int)(innerW * pct / 100.0));
                if (bars[i].Controls.Count > 0)
                    bars[i].Controls[0].Width = fillW;

                startY += rowH;
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  CHART PAINT
        // ═════════════════════════════════════════════════════════════════════
        private void OnChartPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var panel = (Panel)sender;
            int w = panel.Width, h = panel.Height;
            if (w < 10 || h < 10) return;

            string[] months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };
            int[][] datasets = { _clrData, _certData, _bltData };
            Color[] colors =
            {
                Color.FromArgb(24,  95, 165),
                Color.FromArgb(59, 109,  17),
                Color.FromArgb(230, 130,   0),
            };

            int maxVal = 1;
            foreach (var ds in datasets)
                foreach (var v in ds)
                    if (v > maxVal) maxVal = v;
            maxVal = (int)Math.Ceiling(maxVal / 5.0) * 5;

            const int padL = 36, padR = 16, padT = 8, padB = 28;
            int chartW = w - padL - padR;
            int chartH = h - padT - padB;

            using (var gridPen = new Pen(Color.FromArgb(230, 232, 240)))
                for (int i = 0; i <= 4; i++)
                {
                    int gy = padT + (int)(chartH * i / 4.0);
                    g.DrawLine(gridPen, padL, gy, padL + chartW, gy);
                    int val = maxVal - (maxVal * i / 4);
                    using (var f = new Font("Segoe UI", 7.5f))
                    using (var sb = new SolidBrush(TextGray))
                        g.DrawString(val.ToString(), f, sb, new PointF(2, gy - 6));
                }

            float groupW = chartW / (float)months.Length;
            float barW = groupW / (datasets.Length + 1.5f);

            for (int gi = 0; gi < months.Length; gi++)
            {
                for (int di = 0; di < datasets.Length; di++)
                {
                    int val = datasets[di][gi];
                    float barH = maxVal > 0 ? (val / (float)maxVal) * chartH : 0;
                    float bx = padL + gi * groupW + (di + 0.4f) * barW;
                    float by = padT + chartH - barH;

                    using (var sb = new SolidBrush(colors[di]))
                    {
                        if (barH > 4)
                        {
                            var bp = new GraphicsPath();
                            bp.AddArc(bx, by, barW - 1, 4, 180, 180);
                            bp.AddLine(bx + barW - 1, by + 2, bx + barW - 1, by + barH);
                            bp.AddLine(bx, by + barH, bx, by + 2);
                            bp.CloseFigure();
                            g.FillPath(sb, bp);
                        }
                        else if (barH > 0)
                            g.FillRectangle(sb, bx, by, barW - 1, barH);
                    }
                }

                float lx = padL + gi * groupW + groupW / 2f;
                using (var f = new Font("Segoe UI", 8f))
                using (var sb = new SolidBrush(TextGray))
                {
                    SizeF sz = g.MeasureString(months[gi], f);
                    g.DrawString(months[gi], f, sb, new PointF(lx - sz.Width / 2f, padT + chartH + 6));
                }
            }
        }

        private void DashboardControl_Load(object sender, EventArgs e)
        {

        }

        // ═════════════════════════════════════════════════════════════════════
        //  DATA LOADERS
        // ═════════════════════════════════════════════════════════════════════
        private void LoadBarangayInfo()
        {
            SQLiteConnection conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    string query = @"SELECT BarangayName, Municipality, Province, Region, Address
                                     FROM company_profile LIMIT 1";
                    using (var reader = new SQLiteCommand(query, conn).ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string name = reader["BarangayName"]?.ToString() ?? "";
                            var parts = new List<string>();
                            foreach (string col in new[] { "Address", "Municipality", "Province", "Region" })
                            {
                                string v = reader[col]?.ToString() ?? "";
                                if (!string.IsNullOrWhiteSpace(v)) parts.Add(v);
                            }
                            lblBarangayName.Text = string.IsNullOrWhiteSpace(name) ? "No name set" : name;
                            lblBarangayAddress.Text = string.Join(", ", parts);
                        }
                        else
                        {
                            lblBarangayName.Text = "No profile set";
                            lblBarangayAddress.Text = "Go to Company Profile to set up.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblBarangayName.Text = "Error";
                    lblBarangayAddress.Text = ex.Message;
                }
            }
        }

        private void LoadDashboardStatistics()
        {
            SQLiteConnection conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    object r;

                    r = new SQLiteCommand("SELECT COUNT(*) FROM resident WHERE IsActive=1", conn).ExecuteScalar();
                    lblTotalResidents.Text = r.ToString();
                    lblTrendResidents.Text = "Active residents";
                    lblTrendResidents.ForeColor = TrendUp;

                    r = new SQLiteCommand("SELECT COUNT(*) FROM officials WHERE IsActive=1", conn).ExecuteScalar();
                    lblTotalOfficials.Text = r.ToString();

                    r = new SQLiteCommand("SELECT COUNT(*) FROM users WHERE IsActive=1", conn).ExecuteScalar();
                    lblActiveUsers.Text = r.ToString();

                    r = new SQLiteCommand("SELECT COUNT(*) FROM blotter WHERE Status='Pending'", conn).ExecuteScalar();
                    int pb = Convert.ToInt32(r);
                    lblPendingBlotters.Text = pb.ToString();
                    lblTrendBlotters.Text = pb > 0 ? pb + " need attention" : "All resolved";
                    lblTrendBlotters.ForeColor = pb > 0 ? TrendDown : TrendUp;

                    r = new SQLiteCommand("SELECT COUNT(*) FROM clearance WHERE Status='Pending'", conn).ExecuteScalar();
                    int pc = Convert.ToInt32(r);
                    lblPendingClearances.Text = pc.ToString();
                    lblTrendClearances.Text = pc > 0 ? pc + " pending" : "None pending";
                    lblTrendClearances.ForeColor = pc > 0 ? TrendDown : TrendUp;

                    r = new SQLiteCommand("SELECT COALESCE(SUM(Amount),0) FROM ayuda", conn).ExecuteScalar();
                    decimal total = Convert.ToDecimal(r);
                    lblTotalAyuda.Text = "₱" + total.ToString("N0");
                    lblTrendAyuda.Text = "Total released";
                    lblTrendAyuda.ForeColor = TrendUp;

                    int totClr = Convert.ToInt32(new SQLiteCommand("SELECT COUNT(*) FROM clearance", conn).ExecuteScalar());
                    int issuedClr = Convert.ToInt32(new SQLiteCommand(
                        "SELECT COUNT(*) FROM clearance WHERE Status='Issued' OR Status='Released'", conn).ExecuteScalar());
                    if (totClr > 0) SetPct(pbClearance, lblPctClearance, (int)(issuedClr * 100.0 / totClr));

                    int totBlt = Convert.ToInt32(new SQLiteCommand("SELECT COUNT(*) FROM blotter", conn).ExecuteScalar());
                    int resBlt = Convert.ToInt32(new SQLiteCommand(
                        "SELECT COUNT(*) FROM blotter WHERE Status='Resolved'", conn).ExecuteScalar());
                    if (totBlt > 0) SetPct(pbBlotter, lblPctBlotter, (int)(resBlt * 100.0 / totBlt));

                    LayoutProgressBars(_statusCard, _statusCard.Width, _statusCard.Height);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading analytics statistics: " + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadMonthlyData()
        {
            SQLiteConnection conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    int curMonth = DateTime.Now.Month;
                    int curYear = DateTime.Now.Year;

                    for (int i = 0; i < 6; i++)
                    {
                        int mo = curMonth - 5 + i;
                        int yr = curYear;
                        if (mo <= 0) { mo += 12; yr--; }

                        _clrData[i] = Convert.ToInt32(new SQLiteCommand(
                            $"SELECT COUNT(*) FROM clearance WHERE MONTH(DateIssued)={mo} AND YEAR(DateIssued)={yr}", conn).ExecuteScalar());
                        _certData[i] = Convert.ToInt32(new SQLiteCommand(
                            $"SELECT COUNT(*) FROM certificates WHERE MONTH(DateIssued)={mo} AND YEAR(DateIssued)={yr}", conn).ExecuteScalar());
                        _bltData[i] = Convert.ToInt32(new SQLiteCommand(
                            $"SELECT COUNT(*) FROM blotter WHERE MONTH(DateFiled)={mo} AND YEAR(DateFiled)={yr}", conn).ExecuteScalar());
                    }

                    _chartPanel?.Invalidate();
                }
                catch { /* chart shows zeros on error */ }
            }
        }

        private void LoadRecentActivities()
        {
            SQLiteConnection conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    string query = @"
                        SELECT 'Clearance' AS Type, ClearanceNumber AS RefNumber,
                               DateIssued AS Date,
                               CONCAT(r.LastName, ', ', r.FirstName) AS ResidentName,
                               Purpose AS Details
                        FROM clearance c
                        INNER JOIN resident r ON c.ResidentID = r.idResident
                        WHERE c.Status != 'Cancelled'
                        UNION ALL
                        SELECT 'Certificate', CertificateNumber, DateIssued,
                               CONCAT(r.LastName, ', ', r.FirstName), CertificateType
                        FROM certificates c
                        INNER JOIN resident r ON c.ResidentID = r.idResident
                        WHERE c.Status != 'Cancelled'
                        UNION ALL
                        SELECT 'Blotter', BlotterNumber, DateFiled,
                               RespondentName, IncidentType
                        FROM blotter WHERE Status != 'Resolved'
                        UNION ALL
                        SELECT 'Ayuda', BarcodeID, DateReleased,
                               CONCAT(r.LastName, ', ', r.FirstName),
                               CONCAT('₱', FORMAT(Amount, 0))
                        FROM ayuda a
                        INNER JOIN resident r ON a.ResidentID = r.idResident
                        ORDER BY Date DESC LIMIT 15";

                    var da = new SQLiteDataAdapter(query, conn);
                    var dt = new DataTable();
                    da.Fill(dt);
                    dgvRecentActivity.DataSource = dt;

                    if (dgvRecentActivity.Columns.Count > 0)
                    {
                        dgvRecentActivity.Columns["Type"].HeaderText = "Type";
                        dgvRecentActivity.Columns["RefNumber"].HeaderText = "Reference No.";
                        dgvRecentActivity.Columns["Date"].HeaderText = "Date";
                        dgvRecentActivity.Columns["ResidentName"].HeaderText = "Resident / Respondent";
                        dgvRecentActivity.Columns["Details"].HeaderText = "Details";
                        dgvRecentActivity.Columns["Date"].DefaultCellStyle.Format = "MMM dd, yyyy";
                        dgvRecentActivity.Columns["Type"].Width = 110;
                        dgvRecentActivity.Columns["RefNumber"].Width = 150;
                        dgvRecentActivity.Columns["Date"].Width = 120;
                        dgvRecentActivity.Columns["ResidentName"].Width = 180;
                        dgvRecentActivity.Columns["Details"].AutoSizeMode =
                            DataGridViewAutoSizeColumnMode.Fill;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading recent activities: " + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void RefreshDashboard()
        {
            LoadBarangayInfo();
            LoadDashboardStatistics();
            LoadMonthlyData();
            LoadRecentActivities();
        }

        // ═════════════════════════════════════════════════════════════════════
        //  PAINT HELPERS
        // ═════════════════════════════════════════════════════════════════════
        private void PaintInfoCard(Graphics g, Panel card)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var bg = RoundRect(0, 0, card.Width, card.Height, 10))
            using (var sb = new SolidBrush(BgCard)) g.FillPath(sb, bg);
            using (var bg = RoundRect(0, 0, card.Width - 1, card.Height - 1, 10))
            using (var pen = new Pen(Color.FromArgb(200, 210, 230))) g.DrawPath(pen, bg);

            using (var header = new GraphicsPath())
            {
                header.AddArc(0, 0, 20, 20, 180, 90);
                header.AddArc(card.Width - 20, 0, 20, 20, 270, 90);
                header.AddLine(card.Width, 10, card.Width, 44);
                header.AddLine(0, 44, 0, 10);
                header.CloseFigure();
                using (var sb = new SolidBrush(NavBlue)) g.FillPath(sb, header);
            }
            using (var f = new Font("Segoe UI Emoji", 10f, FontStyle.Bold))
                g.DrawString("🏛  Barangay Info", f, Brushes.White, new PointF(12, 12));
        }

        private void PaintStatCard(Graphics g, Panel card, int idx)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var bg = RoundRect(0, 0, card.Width, card.Height, 10))
            using (var sb = new SolidBrush(BgCard)) g.FillPath(sb, bg);
            using (var bg = RoundRect(0, 0, card.Width - 1, card.Height - 1, 10))
            using (var pen = new Pen(Color.FromArgb(200, 210, 230))) g.DrawPath(pen, bg);

            Color accent = _cards[idx].Accent;
            using (var strip = new GraphicsPath())
            {
                strip.AddArc(0, 0, 16, 16, 180, 90);
                strip.AddLine(8, 0, 8, 0);
                strip.AddLine(8, 0, 8, card.Height);
                strip.AddArc(0, card.Height - 16, 16, 16, 90, 90);
                strip.CloseFigure();
                using (var sb = new SolidBrush(accent)) g.FillPath(sb, strip);
            }

            using (var f = new Font("Segoe UI", 9f))
            using (var sb = new SolidBrush(TextGray))
                g.DrawString(_cards[idx].Title, f, sb, new PointF(18, 10));

            int iconZoneW = 60;
            Color darkAccent = ControlPaint.Dark(accent, 0.12f);
            using (var zoneBrush = new SolidBrush(darkAccent))
            {
                var zone = new GraphicsPath();
                int x = card.Width - iconZoneW;
                zone.AddLine(x, 0, card.Width - 10, 0);
                zone.AddArc(card.Width - 20, 0, 20, 20, 270, 90);
                zone.AddArc(card.Width - 20, card.Height - 20, 20, 20, 0, 90);
                zone.AddLine(card.Width - 10, card.Height, x, card.Height);
                zone.CloseFigure();
                g.FillPath(zoneBrush, zone);
            }
            using (var iconFont = new Font("Segoe UI Emoji", 18f))
            {
                string icon = _cards[idx].Icon;
                SizeF sz = g.MeasureString(icon, iconFont);
                float ix = card.Width - iconZoneW + (iconZoneW - sz.Width) / 2f;
                float iy = (card.Height - sz.Height) / 2f;
                g.DrawString(icon, iconFont, Brushes.White, new PointF(ix, iy));
            }
        }

        private void PaintCardHeader(Graphics g, Panel card, string title)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var bg = RoundRect(0, 0, card.Width, card.Height, 10))
            using (var sb = new SolidBrush(BgCard)) g.FillPath(sb, bg);
            using (var bg = RoundRect(0, 0, card.Width - 1, card.Height - 1, 10))
            using (var pen = new Pen(Color.FromArgb(200, 210, 230))) g.DrawPath(pen, bg);

            using (var header = new GraphicsPath())
            {
                header.AddArc(0, 0, 20, 20, 180, 90);
                header.AddArc(card.Width - 20, 0, 20, 20, 270, 90);
                header.AddLine(card.Width, 10, card.Width, 44);
                header.AddLine(0, 44, 0, 10);
                header.CloseFigure();
                using (var sb = new SolidBrush(NavBlue)) g.FillPath(sb, header);
            }
            using (var f = new Font("Segoe UI", 11f, FontStyle.Bold))
                g.DrawString(title, f, Brushes.White, new PointF(16, 12));
        }

        private static GraphicsPath RoundRect(int x, int y, int w, int h, int r)
        {
            int d = r * 2;
            var p = new GraphicsPath();
            p.AddArc(x, y, d, d, 180, 90);
            p.AddArc(x + w - d, y, d, d, 270, 90);
            p.AddArc(x + w - d, y + h - d, d, d, 0, 90);
            p.AddArc(x, y + h - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DashboardControl
            // 
            this.Name = "DashboardControl";
            this.Load += new System.EventHandler(this.DashboardControl_Load);
            this.ResumeLayout(false);

        }
    }
}