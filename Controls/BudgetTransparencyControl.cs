// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  BudgetTransparencyControl.cs  (UPDATED)                                 ║
// ║                                                                          ║
// ║  1) Recent Transactions no longer has a per-row "Beneficiaries" button.  ║
// ║     Clicking an income/fund row opens a fund-info popup with a          ║
// ║     contextual action button ("View Beneficiaries" or "Distribute").    ║
// ║     A "Funds & Beneficiaries" button next to "Add Income" opens a       ║
// ║     popup list of every fund showing its beneficiary count.             ║
// ║                                                                          ║
// ║  2) FIXED (title/banner order): on the Beneficiaries and Distribution   ║
// ║     screens, the title bar and fund banner are children of one fixed-   ║
// ║     height wrapper panel with explicit Location/Size, so the title bar  ║
// ║     is always the outermost, topmost strip and the fund banner always   ║
// ║     sits directly beneath it.                                          ║
// ║                                                                          ║
// ║  3) FIXED (content clipped under header): the body/scroll area below    ║
// ║     the header on Beneficiaries and Distribution was declared with      ║
// ║     Dock = DockStyle.Fill as a sibling of the header wrapper. In this    ║
// ║     environment that combination was rendering the Fill panel as if it  ║
// ║     started at y=0 (underneath/behind the header) instead of right      ║
// ║     below it, so anything inside the first ~100px of that panel (the    ║
// ║     card headers, the search box) was hidden behind the opaque header   ║
// ║     bars. Fix: the body/scroll panel no longer uses Dock=Fill at all —   ║
// ║     its bounds are now computed explicitly from the view's size and the ║
// ║     header's fixed height on every resize, so it always starts exactly  ║
// ║     below the header with nothing hidden behind it.                     ║
// ║                                                                          ║
// ║  4) RESTYLED buttons: the previous glossy gradient/sheen look didn't    ║
// ║     match the rest of the system's flatter, card-based aesthetic. All   ║
// ║     buttons are now a calmer flat-with-subtle-depth style — a solid     ║
// ║     fill, a thin defining border, a soft (not glowing) drop shadow,     ║
// ║     rounded corners, and a gentle hover/press tint. Same idea as        ║
// ║     before (not perfectly flat) but toned down to fit the app's vibe.   ║
// ║                                                                          ║
// ║  5) NEW: QR / Resident-ID scan on the Distribution screen now offers    ║
// ║     an "Add as Beneficiary" action directly from the matched-resident   ║
// ║     card when the scanned resident is legitimate (found in the         ║
// ║     resident table) but is NOT yet a beneficiary of the selected fund.  ║
// ║     Clicking it opens the same Add Beneficiary dialog used elsewhere,   ║
// ║     then immediately re-runs the scan so the card updates in place.     ║
// ║                                                                          ║
// ║  6) NEW: on the Distribution screen's Beneficiaries List (All /         ║
// ║     Pending / Released), every row now has a "⋮" actions column.        ║
// ║     Clicking anywhere else on the row opens the same resident profile   ║
// ║     popup used by the QR scanner. The "⋮" menu offers View Profile,     ║
// ║     Edit Allocated Amount (capped at the fund's remaining balance,      ║
// ║     disabled once released), and Remove Beneficiary (disabled once      ║
// ║     released).                                                          ║
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
    // FIX (v3): ScrollControlIntoView is NOT virtual in .NET Framework's
    // ScrollableControl (confirmed by CS0506), so it can't be overridden.
    // The reliable workaround: WinForms scrolls a panel to bring a newly
    // focused control into view as part of handling focus, and fires
    // GotFocus on the child right after. We hook GotFocus on every child
    // (recursively, including controls added later) and use BeginInvoke to
    // queue a correction that runs immediately after that internal scroll
    // finishes — snapping the panel back to the last position the USER
    // actually set (tracked via the real Scroll event). This is what stops
    // the summary cards / QR scan card header from getting silently
    // scrolled out of view and looking "cut off".
    public class NoAutoScrollPanel : Panel
    {
        private Point _lastGoodScroll = Point.Empty;

        public NoAutoScrollPanel()
        {
            this.Scroll += (s, e) => _lastGoodScroll = this.AutoScrollPosition;
            this.ControlAdded += (s, e) => HookRecursive(e.Control);
        }

        private void HookRecursive(Control c)
        {
            c.GotFocus += ChildGotFocus;
            c.ControlAdded += (s, e) => HookRecursive(e.Control);
            foreach (Control child in c.Controls)
                HookRecursive(child);
        }

        private void ChildGotFocus(object sender, EventArgs e)
        {
            var target = _lastGoodScroll;
            if (!this.IsHandleCreated) return;
            this.BeginInvoke(new Action(() =>
            {
                if (this.AutoScrollPosition != target)
                    this.AutoScrollPosition = new Point(-target.X, -target.Y);
            }));
        }

        // Explicit, programmatic scroll reset (e.g. when switching views).
        public void ResetScroll(Point p)
        {
            _lastGoodScroll = new Point(Math.Abs(p.X), Math.Abs(p.Y));
            this.AutoScrollPosition = p;
        }
    }

    // A custom-drawn button used everywhere instead of a plain flat Button:
    // a solid fill, a thin defining border, a soft drop shadow, rounded
    // corners, and a gentle hover/press tint. Deliberately NOT a glossy
    // gradient button — just enough lift to not look flat, without clashing
    // with the app's clean, card-based look. Set BaseColor to the desired
    // accent color (same colors used before — BtnGreen, NavBlue, BtnTeal,
    // etc.) and it behaves like a normal Button otherwise (Text, Click,
    // Location, Size, Font, ForeColor all still work as usual).
    public class PrettyButton : Button
    {
        public Color BaseColor { get; set; } = Color.FromArgb(26, 35, 126);
        public int CornerRadius { get; set; } = 8;

        private bool _hover;
        private bool _pressed;

        public PrettyButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                      ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                      ControlStyles.SupportsTransparentBackColor, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            Cursor = Cursors.Hand;

            MouseEnter += (s, e) => { _hover = true; Invalidate(); };
            MouseLeave += (s, e) => { _hover = false; _pressed = false; Invalidate(); };
            MouseDown += (s, e) => { _pressed = true; Invalidate(); };
            MouseUp += (s, e) => { _pressed = false; Invalidate(); };
            EnabledChanged += (s, e) => Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // Paint the parent's background color first so the rounded
            // corners blend seamlessly instead of showing square artifacts.
            pevent.Graphics.Clear(Parent != null ? Parent.BackColor : BackColor);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            int radius = Math.Min(CornerRadius, Math.Min(rect.Width, rect.Height) / 2);

            Color baseCol = Enabled ? BaseColor : Color.FromArgb(190, 195, 210);
            Color fill = ShiftColor(baseCol, _pressed ? -22 : (_hover ? 12 : 0));

            // Soft, subtle drop shadow — just enough lift to read as a
            // button sitting slightly above the page, not a glow effect.
            if (Enabled)
            {
                for (int i = 2; i >= 1; i--)
                {
                    var shadowRect = new Rectangle(rect.X, rect.Y + i + 1, rect.Width, rect.Height);
                    using (var path = RoundedRectPath(shadowRect, radius))
                    using (var brush = new SolidBrush(Color.FromArgb(5 + i * 3, 20, 30, 60)))
                        g.FillPath(brush, path);
                }
            }

            using (var path = RoundedRectPath(rect, radius))
            {
                using (var brush = new SolidBrush(fill))
                    g.FillPath(brush, path);
                using (var pen = new Pen(ShiftColor(baseCol, -35), 1f))
                    g.DrawPath(pen, path);
            }

            Color textColor = Enabled ? ForeColor : Color.FromArgb(235, 237, 244);
            TextRenderer.DrawText(g, Text, Font, rect, textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        private static Color ShiftColor(Color c, int amt)
        {
            int r = Math.Max(0, Math.Min(255, c.R + amt));
            int gg = Math.Max(0, Math.Min(255, c.G + amt));
            int b = Math.Max(0, Math.Min(255, c.B + amt));
            return Color.FromArgb(c.A, r, gg, b);
        }

        private static GraphicsPath RoundedRectPath(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            if (d <= 0) { path.AddRectangle(r); return path; }
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public class BudgetTransparencyControl : UserControl
    {
        // ── Palette ───────────────────────────────────────────────────────────
        private static readonly Color NavBlue = Color.FromArgb(26, 35, 126);
        private static readonly Color NavBlueLight = Color.FromArgb(48, 63, 159);
        private static readonly Color BgPage = Color.FromArgb(241, 244, 251);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderCol = Color.FromArgb(220, 225, 238);
        private static readonly Color TextGray = Color.FromArgb(108, 114, 140);
        private static readonly Color BtnGreen = Color.FromArgb(15, 157, 88);
        private static readonly Color BtnBlue = Color.FromArgb(66, 133, 244);
        private static readonly Color BtnRed = Color.FromArgb(192, 57, 43);
        private static readonly Color BtnGray = Color.FromArgb(120, 128, 145);
        private static readonly Color BtnTeal = Color.FromArgb(0, 130, 120);
        private static readonly Color BtnAmber = Color.FromArgb(180, 100, 0);
        private static readonly Color LightBlue = Color.FromArgb(238, 242, 255);
        private static readonly Color GreenAccent = Color.FromArgb(46, 204, 113);
        private static readonly Color RedAccent = Color.FromArgb(231, 76, 60);
        private static readonly Color BlueAccent = Color.FromArgb(52, 152, 219);
        private static readonly Color PendingAmber = Color.FromArgb(180, 90, 0);
        private static readonly Color ReleasedGreen = Color.FromArgb(15, 120, 60);
        private static readonly Color ScannerBg = Color.FromArgb(240, 248, 255);
        private static readonly Color PillInactive = Color.FromArgb(228, 232, 248);
        private static readonly Color PaleGreenRow = Color.FromArgb(234, 247, 239);
        private static readonly Color PaleAmberRow = Color.FromArgb(255, 246, 232);
        private static readonly Color KebabPillBg = Color.FromArgb(225, 229, 245);

        // ── View panels ───────────────────────────────────────────────────────
        private Panel _mainView;
        private Panel _benefView;
        private Panel _distView;

        // ── Summary labels ────────────────────────────────────────────────────
        private Label _lblIncome, _lblExpense, _lblBalance;
        // Reference to the "Funds & Beneficiaries" button so its badge count
        // can be refreshed whenever the summary cards reload.
        private Button _btnFundsList;

        // ── Shared state ──────────────────────────────────────────────────────
        private int _selectedFundId = 0;
        private string _selectedFundName = "";
        private decimal _selectedFundAmt = 0;
        private string _benefStatusFilter = "All";

        // ══════════════════════════════════════════════════════════════════════
        public BudgetTransparencyControl()
        {
            this.DoubleBuffered = true;
            this.BackColor = BgPage;
            this.Dock = DockStyle.Fill;
            BuildAll();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ShowMain();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  BUILD
        // ══════════════════════════════════════════════════════════════════════
        private void BuildAll()
        {
            _distView = BuildDistributionView();
            _benefView = BuildBeneficiariesView();
            _mainView = BuildMainView();

            this.Controls.Add(_distView);
            this.Controls.Add(_benefView);
            this.Controls.Add(_mainView);

            _distView.Visible = false;
            _benefView.Visible = false;
            _mainView.Visible = true;
        }

        // ── Navigation ────────────────────────────────────────────────────────
        // Screens: Main  →  (popup) Fund info  →  either Beneficiaries (add/view
        // residents for that one fund) or Distribution (QR release for that one
        // fund). Beneficiaries and Distribution are siblings — neither is nested
        // inside the other — and both return to Main. The fund is always chosen
        // first via a popup (either from a Recent Transactions row, or from the
        // "Funds & Beneficiaries" list popup), so there is no separate full-page
        // funds hub anymore.
        private void ShowMain()
        {
            _benefView.Visible = false;
            _distView.Visible = false;
            _mainView.Visible = true;
            LoadSummaryCards();
            // Force scroll to top whenever we return to the main view.
            foreach (Control c in _mainView.Controls)
            {
                if (c is NoAutoScrollPanel nsp)
                {
                    nsp.ResetScroll(new Point(0, 0));
                    break;
                }
            }
        }

        private void ShowBeneficiaries(int preselectFundId, string preselectFundName, decimal preselectFundAmt)
        {
            _mainView.Visible = false;
            _distView.Visible = false;
            _selectedFundId = preselectFundId;
            _selectedFundName = preselectFundName;
            _selectedFundAmt = preselectFundAmt;
            _benefView.Visible = true;
            (_benefView.Tag as Action)?.Invoke();
        }

        private void ShowDistributionForFund(int fundId, string fundName, decimal fundAmt)
        {
            _selectedFundId = fundId;
            _selectedFundName = fundName;
            _selectedFundAmt = fundAmt;
            ShowDistribution();
        }

        private void ShowDistribution()
        {
            _benefView.Visible = false;
            _mainView.Visible = false;
            _distView.Visible = true;
            (_distView.Tag as Action)?.Invoke();
        }

        // Re-loads the summary cards and the Recent Transactions grid on the
        // Main view without switching screens — used after actions performed
        // from a popup (e.g. adding a fund) so Main is up to date once the
        // person returns to it.
        private void RefreshMainView()
        {
            LoadSummaryCards();
            foreach (Control c in _mainView.Controls)
            {
                if (c is NoAutoScrollPanel nsp && nsp.Controls.Count > 0 && nsp.Controls[0] is Panel content)
                {
                    FindAndLoadTxGrid(content);
                    break;
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  MAIN VIEW
        // ══════════════════════════════════════════════════════════════════════
        private Panel BuildMainView()
        {
            var view = new Panel { Dock = DockStyle.Fill, BackColor = BgPage };

            // ── Top bar ──────────────────────────────────────────────────────
            var header = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = NavBlue };
            header.Paint += (s2, e2) =>
            {
                using (var lg = new LinearGradientBrush(header.ClientRectangle,
                    Color.FromArgb(26, 35, 126), Color.FromArgb(40, 55, 160), LinearGradientMode.Horizontal))
                    e2.Graphics.FillRectangle(lg, header.ClientRectangle);
            };

            var btnBack = NavBtn("◀  Back to Main Menu");
            btnBack.Location = new Point(12, 18);
            btnBack.Click += (s, e) =>
            {
                Form parentForm = this.FindForm();
                parentForm?.Close();
            };
            header.Controls.Add(btnBack);

            var lblTitle = new Label
            {
                Text = "💰  Budget Transparency",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            header.Controls.Add(lblTitle);

            var lblSubTitle = new Label
            {
                Text = "Barangay funds, transactions and Ayuda distribution",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(170, 195, 240),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            header.Controls.Add(lblSubTitle);

            header.Layout += (s, e) =>
            {
                int centerX = (header.Width - lblTitle.Width) / 2;
                lblTitle.Location = new Point(centerX, 14);
                lblSubTitle.Location = new Point(centerX, 46);
            };
            view.Controls.Add(header);

            // ── Scroll container ──────────────────────────────────────────────
            // FIX: NoAutoScrollPanel instead of Panel — stops WinForms from
            // auto-scrolling down whenever a control receives focus, which
            // was the cause of the summary cards getting clipped under the
            // header.
            var scroll = new NoAutoScrollPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = BgPage };
            view.Controls.Add(scroll);

            // ── Inner content panel ──────────────────────────────────────────
            // FIX: pushed down a bit further (was 75) so the summary cards
            // sit with a bit more breathing room under the header instead
            // of feeling jammed right against it.
            var content = new Panel
            {
                BackColor = BgPage,
                Location = new Point(20, 100),
                Size = new Size(860, 800),
                AutoSize = false
            };
            scroll.Controls.Add(content);

            // Small section eyebrow above the cards — adds a bit of visual
            // structure instead of the cards floating with nothing above them.
            const int cardsTop = 28;
            var stripOverview = new Panel { BackColor = BlueAccent, Location = new Point(0, 2), Size = new Size(4, 16) };
            var lblOverview = new Label
            {
                Text = "FINANCIAL OVERVIEW",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 128, 155),
                AutoSize = true,
                Location = new Point(12, 0),
                BackColor = Color.Transparent
            };
            content.Controls.Add(stripOverview);
            content.Controls.Add(lblOverview);

            // ── Summary cards ──────────────────────────────────────────────────
            const int gap = 16;
            const int ch = 110;
            const int cw = 240;

            var cardIncome = SummaryCard(new Point(0, cardsTop), new Size(cw, ch), "Total Income", "All recorded income", GreenAccent, "up");
            var cardExpense = SummaryCard(new Point(cw + gap, cardsTop), new Size(cw, ch), "Total Expenses", "All recorded expenses", RedAccent, "down");
            var cardBalance = SummaryCard(new Point((cw + gap) * 2, cardsTop), new Size(cw, ch), "Available Balance", "Income minus expenses", BlueAccent, "wallet");

            _lblIncome = (Label)cardIncome.Controls[1];
            _lblExpense = (Label)cardExpense.Controls[1];
            _lblBalance = (Label)cardBalance.Controls[1];

            content.Controls.Add(cardIncome);
            content.Controls.Add(cardExpense);
            content.Controls.Add(cardBalance);

            // ── Action buttons ────────────────────────────────────────────────
            // NOTE: beneficiaries are no longer managed via a button inside the
            // Recent Transactions grid — clicking anywhere on an income row now
            // opens the fund-info popup instead (see TxGrid_RowClick). The new
            // "Funds & Beneficiaries" button opens a popup listing every fund
            // with its beneficiary count, as an alternate entry point.
            int btnY = cardsTop + ch + 16;
            var btnAddTx = ActionBtn("+ Add Transaction", BtnGreen, new Point(0, btnY));
            var btnAddInc = ActionBtn("+ Add Income", NavBlue, new Point(192, btnY));
            _btnFundsList = ActionBtn("👥  Funds & Beneficiaries", BtnTeal, new Point(384, btnY));
            _btnFundsList.Size = new Size(232, 42);
            _btnFundsList.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            btnAddTx.Click += (s, e) => ShowAddTransactionDialog(() => { LoadSummaryCards(); FindAndLoadTxGrid(content); });
            btnAddInc.Click += (s, e) => { var f = new AddIncomeForm(); if (f.ShowDialog() == DialogResult.OK) { LoadSummaryCards(); FindAndLoadTxGrid(content); } };
            _btnFundsList.Click += (s, e) => ShowFundsListPopup();
            content.Controls.Add(btnAddTx);
            content.Controls.Add(btnAddInc);
            content.Controls.Add(_btnFundsList);

            // ── Transactions card ─────────────────────────────────────────────
            int gridY = btnY + 56;
            var txCard = new Panel { BackColor = BgCard, Location = new Point(0, gridY), Size = new Size(860, 460) };
            txCard.Paint += (s, e) => PaintCard(e.Graphics, txCard);
            content.Controls.Add(txCard);

            // Colored header bar for tx card
            var txHdrBar = new Panel { BackColor = NavBlue, Location = new Point(0, 0), Height = 48 };
            txHdrBar.Paint += (s, e) =>
            {
                using (var lg = new LinearGradientBrush(txHdrBar.ClientRectangle,
                    NavBlue, NavBlueLight, LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(lg, txHdrBar.ClientRectangle);
            };
            txHdrBar.Controls.Add(new Label
            {
                Text = "📊  Recent Transactions",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(16, 12)
            });
            txCard.Controls.Add(txHdrBar);
            txHdrBar.Resize += (s, e) => txHdrBar.Width = txCard.Width;
            var divLine = new Panel { BackColor = BorderCol, Location = new Point(0, 48), Height = 1 };
            txCard.Controls.Add(divLine);

            var dgvTx = MakeGrid();
            dgvTx.Location = new Point(0, 50);
            txCard.Controls.Add(dgvTx);

            txCard.Resize += (s, e) =>
            {
                txHdrBar.Width = txCard.Width;
                divLine.Width = txCard.Width;
                dgvTx.Width = txCard.Width;
                dgvTx.Height = txCard.Height - 50;
            };

            // ── Responsive resize ─────────────────────────────────────────────
            scroll.Resize += (s, e) =>
            {
                int w = scroll.ClientSize.Width - 40;
                if (w < 300) return;
                content.Width = w;

                int crdW = Math.Min(320, (w - gap * 2) / 3);
                cardIncome.SetBounds(0, cardsTop, crdW, ch);
                cardExpense.SetBounds(crdW + gap, cardsTop, crdW, ch);
                cardBalance.SetBounds((crdW + gap) * 2, cardsTop, crdW, ch);

                int txH = Math.Max(350, scroll.ClientSize.Height - gridY - 60);
                txCard.SetBounds(0, gridY, w, txH);
                content.Height = gridY + txH + 30;
            };

            view.VisibleChanged += (s, e) =>
            {
                if (!view.Visible) return;
                LoadSummaryCards();
                FindAndLoadTxGrid(content);
                // FIXED: Reset scroll position to top
                scroll.ResetScroll(new Point(0, 0));
            };

            return view;
        }

        private void FindAndLoadTxGrid(Panel content)
        {
            foreach (Control c in content.Controls)
            {
                if (!(c is Panel card)) continue;
                foreach (Control cc in card.Controls)
                {
                    if (cc is DataGridView dgv) { LoadTransactions(dgv); return; }
                }
            }
        }

        private PrettyButton ActionBtn(string text, Color bg, Point loc)
        {
            return new PrettyButton
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                BaseColor = bg,
                ForeColor = Color.White,
                Location = loc,
                Size = new Size(178, 42),
                CornerRadius = 8
            };
        }

        // ══════════════════════════════════════════════════════════════════════
        //  FUND INFO POPUP  — the single entry point into a fund's details.
        //  Reached either by clicking an income row in Recent Transactions
        //  (distributeMode = false → offers "View Beneficiaries") or by
        //  picking a fund from the Funds & Beneficiaries list popup
        //  (distributeMode = true → offers "Distribute", once it has at
        //  least one beneficiary).
        // ══════════════════════════════════════════════════════════════════════
        private void ShowFundInfoDialog(int fundId, string fundName, DateTime date, decimal amount, string desc, bool distributeMode)
        {
            int benefCount = GetBeneficiaryCount(fundId);

            var dlg = MakeDialog(fundName, date.ToString("MMM dd, yyyy"), new Size(400, 300));
            var body = (Panel)dlg.Tag;
            const int w = 350;

            var lblAmount = new Label
            {
                Text = "₱" + amount.ToString("N2"),
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = ReleasedGreen,
                AutoSize = true,
                Location = new Point(0, 0),
                BackColor = Color.Transparent
            };
            body.Controls.Add(lblAmount);

            var lblDesc = new Label
            {
                Text = string.IsNullOrWhiteSpace(desc) ? "No description provided." : desc,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = TextGray,
                AutoSize = false,
                Size = new Size(w, 40),
                Location = new Point(0, 38),
                BackColor = Color.Transparent
            };
            body.Controls.Add(lblDesc);

            var divider = new Panel { BackColor = BorderCol, Location = new Point(0, 84), Size = new Size(w, 1) };
            body.Controls.Add(divider);

            body.Controls.Add(new Label
            {
                Text = "Beneficiaries",
                Font = new Font("Segoe UI", 9f),
                ForeColor = TextGray,
                AutoSize = true,
                Location = new Point(0, 94),
                BackColor = Color.Transparent
            });
            body.Controls.Add(new Label
            {
                Text = benefCount.ToString(),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(w - 24, 94),
                BackColor = Color.Transparent
            });

            int btnY = 126;
            if (!distributeMode)
            {
                var btnAction = MakeBtn("👥  View Beneficiaries", BtnTeal, 10f);
                btnAction.Location = new Point(0, btnY);
                btnAction.Size = new Size(w, 38);
                btnAction.Click += (s, e) =>
                {
                    dlg.Close();
                    ShowBeneficiaries(fundId, fundName, amount);
                };
                body.Controls.Add(btnAction);
                btnY += 46;
            }
            else if (benefCount > 0)
            {
                var btnAction = MakeBtn("📤  Distribute", BtnTeal, 10f);
                btnAction.Location = new Point(0, btnY);
                btnAction.Size = new Size(w, 38);
                btnAction.Click += (s, e) =>
                {
                    dlg.Close();
                    ShowDistributionForFund(fundId, fundName, amount);
                };
                body.Controls.Add(btnAction);
                btnY += 46;
            }
            else
            {
                body.Controls.Add(new Label
                {
                    Text = "Add beneficiaries to this fund before distributing.",
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                    ForeColor = TextGray,
                    AutoSize = true,
                    Location = new Point(0, btnY + 10),
                    BackColor = Color.Transparent
                });

                var btnGoAdd = MakeBtn("👥  Add Beneficiaries", BtnTeal, 10f);
                btnGoAdd.Location = new Point(0, btnY + 34);
                btnGoAdd.Size = new Size(w, 38);
                btnGoAdd.Click += (s, e) =>
                {
                    dlg.Close();
                    ShowBeneficiaries(fundId, fundName, amount);
                };
                body.Controls.Add(btnGoAdd);
                btnY += 80;
            }

            var btnClose = MakeBtn("Close", BtnGray, 9.5f);
            btnClose.Location = new Point(0, btnY);
            btnClose.Size = new Size(w, 34);
            btnClose.Click += (s, e) => dlg.Close();
            body.Controls.Add(btnClose);

            dlg.ShowDialog();
        }

        private int GetBeneficiaryCount(int fundId)
        {
            int count = 0;
            var conn = DatabaseConnection.GetConnection();
            if (conn == null) return 0;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand("SELECT COUNT(*) FROM fund_beneficiaries WHERE FundID=@fid", conn);
                    cmd.Parameters.AddWithValue("@fid", fundId);
                    count = Convert.ToInt32(cmd.ExecuteScalar());
                }
                catch { }
            }
            return count;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  FUNDS & BENEFICIARIES LIST POPUP  — every fund (income transaction)
        //  with a badge showing how many residents are on it. Clicking a fund
        //  opens the same info popup as above, in "distribute" mode.
        // ══════════════════════════════════════════════════════════════════════
        private void ShowFundsListPopup()
        {
            var dlg = MakeDialog("Funds & Beneficiaries", "Select a fund to manage or distribute", new Size(440, 480));
            var body = (Panel)dlg.Tag;
            const int rowW = 390;

            var conn = DatabaseConnection.GetConnection();
            if (conn == null) { dlg.Close(); return; }

            var funds = new List<(int Id, string Name, DateTime Date, decimal Amount, string Desc, int Benef)>();
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand(@"
                        SELECT idTransaction, TransactionDate, Category, Amount, Description,
                               IFNULL((SELECT COUNT(*) FROM fund_beneficiaries fb WHERE fb.FundID=idTransaction),0) AS Benef
                        FROM budget_transactions
                        WHERE TransactionType='Income'
                        ORDER BY TransactionDate DESC", conn);
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            funds.Add((
                                Convert.ToInt32(r["idTransaction"]),
                                r["Category"]?.ToString() ?? "",
                                Convert.ToDateTime(r["TransactionDate"]),
                                Convert.ToDecimal(r["Amount"]),
                                r["Description"]?.ToString() ?? "",
                                Convert.ToInt32(r["Benef"])
                            ));
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading funds:\n" + ex.Message);
                    dlg.Close();
                    return;
                }
            }

            if (funds.Count == 0)
            {
                body.Controls.Add(new Label
                {
                    Text = "No funds recorded yet.",
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Italic),
                    ForeColor = TextGray,
                    AutoSize = true,
                    Location = new Point(0, 20),
                    BackColor = Color.Transparent
                });
            }

            int rowY = 0;
            const int rowH = 64;
            foreach (var f in funds)
            {
                var row = new Panel
                {
                    Location = new Point(0, rowY),
                    Size = new Size(rowW, rowH - 8),
                    BackColor = BgCard,
                    Cursor = Cursors.Hand
                };
                row.Paint += (s, e) => PaintCard(e.Graphics, row);

                var lblName = new Label
                {
                    Text = f.Name,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = NavBlue,
                    AutoSize = true,
                    Location = new Point(12, 8),
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand
                };
                var lblAmt = new Label
                {
                    Text = "₱" + f.Amount.ToString("N2"),
                    Font = new Font("Segoe UI", 8.5f),
                    ForeColor = TextGray,
                    AutoSize = true,
                    Location = new Point(12, 28),
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand
                };
                var badge = new Label
                {
                    Text = $"👥 {f.Benef} resident{(f.Benef == 1 ? "" : "s")}",
                    Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                    ForeColor = f.Benef > 0 ? ReleasedGreen : TextGray,
                    BackColor = f.Benef > 0 ? Color.FromArgb(230, 245, 238) : Color.FromArgb(238, 240, 246),
                    AutoSize = true,
                    Padding = new Padding(8, 4, 8, 4),
                    Location = new Point(rowW - 140, 16),
                    Cursor = Cursors.Hand
                };

                row.Controls.Add(lblName);
                row.Controls.Add(lblAmt);
                row.Controls.Add(badge);

                var fCopy = f;
                EventHandler rowClick = (s, e) =>
                {
                    dlg.Close();
                    ShowFundInfoDialog(fCopy.Id, fCopy.Name, fCopy.Date, fCopy.Amount, fCopy.Desc, true);
                };
                row.Click += rowClick;
                lblName.Click += rowClick;
                lblAmt.Click += rowClick;
                badge.Click += rowClick;

                body.Controls.Add(row);
                rowY += rowH;
            }

            dlg.ShowDialog();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  BENEFICIARIES VIEW
        // ══════════════════════════════════════════════════════════════════════
        private Panel BuildBeneficiariesView()
        {
            var view = new Panel { Dock = DockStyle.Fill, BackColor = BgPage, Visible = false };

            // ── Header wrapper ───────────────────────────────────────────────
            // topBar ("Back to Overview" / "Beneficiaries") and subHeader
            // (fund banner) are explicit children of ONE fixed-height wrapper
            // panel instead of two independent Dock=Top panels, so topBar is
            // always the outermost strip and subHeader always sits directly
            // beneath it.
            const int headerH = 100;
            var headerWrap = new Panel { Dock = DockStyle.Top, Height = headerH, BackColor = NavBlue };
            view.Controls.Add(headerWrap);

            var topBar = new Panel { Location = new Point(0, 0), Size = new Size(headerWrap.Width, 56), BackColor = NavBlue };
            headerWrap.Controls.Add(topBar);
            headerWrap.Resize += (s, e) => topBar.Width = headerWrap.Width;

            var btnBack = NavBtn("◀  Back to Overview");
            btnBack.Click += (s, e) => ShowMain();
            topBar.Controls.Add(btnBack);

            var lblTitle = new Label
            {
                Text = "👥  Beneficiaries",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            topBar.Controls.Add(lblTitle);
            topBar.Layout += (s, e) =>
                lblTitle.Location = new Point((topBar.Width - lblTitle.Width) / 2,
                                              (topBar.Height - lblTitle.Height) / 2);

            // Fund banner sub-header — the fund is always pre-selected before
            // this screen is shown, so it's always visible right under the
            // title bar.
            var subHeader = new Panel { Location = new Point(0, 56), Size = new Size(headerWrap.Width, 44), BackColor = NavBlueLight };
            headerWrap.Controls.Add(subHeader);
            headerWrap.Resize += (s, e) => subHeader.Width = headerWrap.Width;

            var lblFundBanner = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(20, 8)
            };
            var lblRemainingBalance = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(190, 230, 210),
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(20, 25)
            };
            subHeader.Controls.Add(lblFundBanner);
            subHeader.Controls.Add(lblRemainingBalance);

            // ── Body ─────────────────────────────────────────────────────────
            // FIX: previously this was Dock = DockStyle.Fill as a sibling of
            // headerWrap. In this environment that made the Fill panel render
            // as though it started at y=0 — i.e. UNDER/behind headerWrap —
            // instead of below it, so the first ~100px of its content (the
            // card headers, the search box) ended up hidden behind the opaque
            // header bars. Giving body no Dock at all and explicitly setting
            // its bounds from view.ClientSize + headerH on every resize
            // removes any ambiguity: body always starts exactly below the
            // header, full stop.
            var body = new Panel { BackColor = BgPage };
            view.Controls.Add(body);

            void LayoutBody()
            {
                body.SetBounds(0, headerH, view.ClientSize.Width, Math.Max(0, view.ClientSize.Height - headerH));
            }
            view.Resize += (s, e) => LayoutBody();
            LayoutBody();

            // LEFT card: search + add residents
            var leftCard = new Panel { BackColor = BgCard, Location = new Point(20, 20) };
            leftCard.Paint += (s, e) => PaintCard(e.Graphics, leftCard);
            body.Controls.Add(leftCard);

            var leftHdrBar = new Panel { BackColor = BtnTeal, Location = new Point(0, 0), Height = 46 };
            leftHdrBar.Paint += (s, e) => {
                using (var lg = new LinearGradientBrush(leftHdrBar.ClientRectangle, BtnTeal, Color.FromArgb(0, 160, 150), LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(lg, leftHdrBar.ClientRectangle);
            };
            leftHdrBar.Controls.Add(new Label { Text = "👤  Add Residents to Fund", Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, BackColor = Color.Transparent, Location = new Point(14, 12) });
            leftCard.Controls.Add(leftHdrBar);
            leftCard.Resize += (s, e) => leftHdrBar.Width = leftCard.Width;
            var divL = new Panel { BackColor = BorderCol, Location = new Point(0, 46), Height = 1 };
            leftCard.Controls.Add(divL);
            leftCard.Resize += (s2, e2) => divL.Width = leftCard.Width;

            int searchY = 58;
            var lblSearch = new Label
            {
                Text = "Search:",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = TextGray,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(14, searchY + 4)
            };
            var txtSearch = new TextBox
            {
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(76, searchY),
                Size = new Size(200, 26),
                BorderStyle = BorderStyle.FixedSingle
            };
            var btnSearch = SmallBtn("Search", BtnBlue);
            btnSearch.Location = new Point(282, searchY);
            btnSearch.Size = new Size(78, 26);

            leftCard.Controls.Add(lblSearch);
            leftCard.Controls.Add(txtSearch);
            leftCard.Controls.Add(btnSearch);

            var lblHint = new Label
            {
                Text = "Click a resident row to add as beneficiary",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = TextGray,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(14, searchY + 34)
            };
            leftCard.Controls.Add(lblHint);

            var dgvRes = MakeGrid();
            dgvRes.Location = new Point(14, searchY + 56);
            dgvRes.Cursor = Cursors.Hand;
            leftCard.Controls.Add(dgvRes);

            leftCard.Resize += (s, e) =>
            {
                int lw = leftCard.Width;
                txtSearch.Width = Math.Max(80, lw - 76 - 96 - 28);
                btnSearch.Location = new Point(txtSearch.Right + 6, searchY);
                dgvRes.Width = lw - 28;
                dgvRes.Height = Math.Max(80, leftCard.Height - searchY - 66);
            };

            // RIGHT card: residents already added to this fund
            var rightCard = new Panel { BackColor = BgCard, Location = new Point(0, 20) };
            rightCard.Paint += (s, e) => PaintCard(e.Graphics, rightCard);
            body.Controls.Add(rightCard);

            var rightHdrBar = new Panel { BackColor = NavBlue, Location = new Point(0, 0), Height = 46 };
            rightHdrBar.Paint += (s, e) => {
                using (var lg = new LinearGradientBrush(rightHdrBar.ClientRectangle, NavBlue, NavBlueLight, LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(lg, rightHdrBar.ClientRectangle);
            };
            rightHdrBar.Controls.Add(new Label { Text = "📋  Current Beneficiaries", Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, BackColor = Color.Transparent, Location = new Point(14, 12) });
            rightCard.Controls.Add(rightHdrBar);
            rightCard.Resize += (s, e) => rightHdrBar.Width = rightCard.Width;
            var divR = new Panel { BackColor = BorderCol, Location = new Point(0, 46), Height = 1 };
            rightCard.Controls.Add(divR);
            rightCard.Resize += (s2, e2) => divR.Width = rightCard.Width;

            var lblEmptyBenef = new Label
            {
                Text = "No residents added yet.\nAdd some from the list on the left.",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Italic),
                ForeColor = TextGray,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Visible = false
            };
            rightCard.Controls.Add(lblEmptyBenef);

            var dgvCurrentBenef = MakeGrid();
            dgvCurrentBenef.Location = new Point(0, 48);
            rightCard.Controls.Add(dgvCurrentBenef);

            rightCard.Resize += (s, e) =>
            {
                dgvCurrentBenef.Width = rightCard.Width;
                dgvCurrentBenef.Height = rightCard.Height - 48;
                lblEmptyBenef.SetBounds(0, 48, rightCard.Width, dgvCurrentBenef.Height);
            };

            // Body layout
            body.Resize += (s, e) =>
            {
                int w = body.ClientSize.Width - 40;
                int h = body.ClientSize.Height - 36;
                int leftW = Math.Min(500, (int)((w - 16) * 0.5));
                leftCard.SetBounds(20, 20, leftW, h);
                rightCard.SetBounds(20 + leftW + 16, 20, w - leftW - 16, h);
            };

            // ── Data loaders ───────────────────────────────────────────────
            void UpdateRemainingLabel()
            {
                if (_selectedFundId == 0) return;
                var conn2 = DatabaseConnection.GetConnection(); if (conn2 == null) return;
                using (conn2)
                {
                    try
                    {
                        var cmdTotal = new SQLiteCommand(
                            "SELECT IFNULL(SUM(AllocatedAmount),0) FROM fund_beneficiaries WHERE FundID=@fid",
                            conn2);
                        cmdTotal.Parameters.AddWithValue("@fid", _selectedFundId);
                        decimal allocated = Convert.ToDecimal(cmdTotal.ExecuteScalar());
                        decimal remaining = _selectedFundAmt - allocated;
                        lblRemainingBalance.Text =
                            $"Allocated ₱{allocated:N2}   |   Remaining ₱{remaining:N2}";
                    }
                    catch { }
                }
            }

            void LoadResidents(string search = "")
            {
                var conn = DatabaseConnection.GetConnection(); if (conn == null) return;
                using (conn)
                {
                    try
                    {
                        string where = string.IsNullOrWhiteSpace(search)
                            ? "WHERE r.IsActive=1"
                            : "WHERE r.IsActive=1 AND (r.FirstName LIKE @s OR r.LastName LIKE @s OR r.MiddleName LIKE @s)";
                        string q = $@"
                            SELECT r.idResident AS ResidentID,
                                   CONCAT(r.LastName,', ',r.FirstName,
                                       IIF(r.MiddleName IS NOT NULL AND r.MiddleName!='',
                                          CONCAT(' ',r.MiddleName),'')) AS FullName,
                                   r.Address,
                                   TIMESTAMPDIFF(YEAR,r.DateOfBirth,CURDATE()) AS Age,
                                   IIF(EXISTS(SELECT 1 FROM fund_beneficiaries fb
                                             WHERE fb.FundID=@fid AND fb.ResidentID=r.idResident),
                                      '✔ Added','') AS InFund
                            FROM resident r {where}
                            ORDER BY r.LastName,r.FirstName LIMIT 120";
                        var da = new SQLiteDataAdapter(q, conn);
                        da.SelectCommand.Parameters.AddWithValue("@fid", _selectedFundId);
                        if (!string.IsNullOrWhiteSpace(search))
                            da.SelectCommand.Parameters.AddWithValue("@s", "%" + search + "%");
                        var dt = new DataTable(); da.Fill(dt);
                        dgvRes.DataSource = dt;
                        ColVis(dgvRes, "ResidentID", false);
                        ColHdr(dgvRes, "FullName", "Name", 180);
                        ColW(dgvRes, "Age", 52);
                        ColFill(dgvRes, "Address");
                        ColW(dgvRes, "InFund", 76);
                        dgvRes.CellFormatting -= ColorInFund;
                        dgvRes.CellFormatting += ColorInFund;
                    }
                    catch (Exception ex) { MessageBox.Show("Error loading residents:\n" + ex.Message); }
                }
            }

            void LoadCurrentBeneficiaries()
            {
                if (_selectedFundId == 0) { dgvCurrentBenef.DataSource = null; return; }
                var conn = DatabaseConnection.GetConnection(); if (conn == null) return;
                using (conn)
                {
                    try
                    {
                        const string q = @"
                            SELECT fb.idBeneficiary AS BenefID,
                                   CONCAT(r.LastName,', ',r.FirstName) AS Resident,
                                   fb.AllocatedAmount AS Allocated,
                                   CASE WHEN fb.IsReleased=1 THEN 'Released' ELSE 'Pending' END AS Status
                            FROM fund_beneficiaries fb
                            INNER JOIN resident r ON fb.ResidentID=r.idResident
                            WHERE fb.FundID=@fid
                            ORDER BY fb.IsReleased ASC, r.LastName";
                        var da = new SQLiteDataAdapter(q, conn);
                        da.SelectCommand.Parameters.AddWithValue("@fid", _selectedFundId);
                        var dt = new DataTable(); da.Fill(dt);
                        dgvCurrentBenef.DataSource = dt;
                        ColVis(dgvCurrentBenef, "BenefID", false);
                        ColHdr(dgvCurrentBenef, "Resident", "Resident", 170);
                        if (dgvCurrentBenef.Columns.Contains("Allocated"))
                        {
                            dgvCurrentBenef.Columns["Allocated"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                            dgvCurrentBenef.Columns["Allocated"].DefaultCellStyle.Format = "₱#,##0.00";
                            dgvCurrentBenef.Columns["Allocated"].Width = 110;
                            dgvCurrentBenef.Columns["Allocated"].DefaultCellStyle.Alignment =
                                DataGridViewContentAlignment.MiddleRight;
                        }
                        ColFill(dgvCurrentBenef, "Status");
                        dgvCurrentBenef.CellFormatting -= ColorBenefStatus;
                        dgvCurrentBenef.CellFormatting += ColorBenefStatus;

                        bool isEmpty = dgvCurrentBenef.Rows.Count == 0;
                        dgvCurrentBenef.Visible = !isEmpty;
                        lblEmptyBenef.Visible = isEmpty;
                    }
                    catch (Exception ex) { MessageBox.Show("Error loading current beneficiaries:\n" + ex.Message); }
                }
            }

            var ctxRemove = new ContextMenuStrip();
            ctxRemove.Items.Add("Remove Beneficiary", null, (s, e) =>
            {
                if (dgvCurrentBenef.SelectedRows.Count == 0) return;
                var row = dgvCurrentBenef.SelectedRows[0];
                if (row.Cells["Status"].Value?.ToString() == "Released")
                {
                    MessageBox.Show("Cannot remove a released beneficiary.", "Not Allowed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                int bid = Convert.ToInt32(row.Cells["BenefID"].Value);
                string nm = row.Cells["Resident"].Value?.ToString() ?? "";
                if (MessageBox.Show($"Remove {nm} from this fund?", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                var conn = DatabaseConnection.GetConnection(); if (conn == null) return;
                using (conn)
                {
                    try
                    {
                        var cmd = new SQLiteCommand(
                            "DELETE FROM fund_beneficiaries WHERE idBeneficiary=@id", conn);
                        cmd.Parameters.AddWithValue("@id", bid);
                        cmd.ExecuteNonQuery();
                        LoadCurrentBeneficiaries();
                        LoadResidents(txtSearch.Text.Trim());
                        UpdateRemainingLabel();
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
            });
            dgvCurrentBenef.ContextMenuStrip = ctxRemove;

            // Events
            dgvRes.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var row = dgvRes.Rows[e.RowIndex];
                ShowAddBeneficiaryDialog(
                    Convert.ToInt32(row.Cells["ResidentID"].Value),
                    row.Cells["FullName"].Value?.ToString() ?? "",
                    () =>
                    {
                        LoadResidents(txtSearch.Text.Trim());
                        LoadCurrentBeneficiaries();
                        UpdateRemainingLabel();
                        LoadSummaryCards();
                    });
            };

            btnSearch.Click += (s, e) => LoadResidents(txtSearch.Text.Trim());
            txtSearch.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                { e.Handled = true; LoadResidents(txtSearch.Text.Trim()); }
            };

            void RefreshBeneficiariesScreen()
            {
                lblFundBanner.Text = $"Fund:  {_selectedFundName}   |   Total ₱{_selectedFundAmt:N2}";
                LoadResidents();
                LoadCurrentBeneficiaries();
                UpdateRemainingLabel();
            }

            view.Tag = (Action)RefreshBeneficiariesScreen;
            view.VisibleChanged += (s, e) =>
            {
                if (!view.Visible) return;
                RefreshBeneficiariesScreen();
            };

            return view;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  DISTRIBUTION VIEW  (FIXED - proper QR scanner layout, no clipping)
        // ══════════════════════════════════════════════════════════════════════
        private Panel BuildDistributionView()
        {
            var view = new Panel { Dock = DockStyle.Fill, BackColor = BgPage, Visible = false };

            // ── Header wrapper ───────────────────────────────────────────────
            // Same fix as the Beneficiaries screen: topBar and subHeader are
            // explicit children of one fixed-height wrapper (rather than two
            // independent Dock=Top panels), so the title bar is guaranteed to
            // be the outermost strip and the fund banner always sits directly
            // beneath it.
            const int headerH = 100;
            var headerWrap = new Panel { Dock = DockStyle.Top, Height = headerH, BackColor = NavBlue };
            view.Controls.Add(headerWrap);

            var topBar = new Panel { Location = new Point(0, 0), Size = new Size(headerWrap.Width, 56), BackColor = NavBlue };
            headerWrap.Controls.Add(topBar);
            headerWrap.Resize += (s, e) => topBar.Width = headerWrap.Width;

            var btnBack = NavBtn("◀  Back to Overview");
            btnBack.Click += (s, e) => ShowMain();
            topBar.Controls.Add(btnBack);

            // Title, prefixed with a crisp vector "logo" badge instead of an
            // emoji glyph (emoji rendered as broken/blank tofu boxes on
            // machines without a full color-emoji font installed).
            var titleGroup = new Panel { BackColor = Color.Transparent, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            var titleIcon = IconBadge("box", Color.FromArgb(55, 255, 255, 255), Color.White, 28);
            var lblTitle = new Label
            {
                Text = "Distribution",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent
            };
            titleGroup.Controls.Add(titleIcon);
            titleGroup.Controls.Add(lblTitle);
            topBar.Controls.Add(titleGroup);
            topBar.Layout += (s, e) =>
            {
                titleIcon.Location = new Point(0, 0);
                lblTitle.Location = new Point(titleIcon.Width + 10, (titleIcon.Height - lblTitle.Height) / 2);
                titleGroup.Size = new Size(titleIcon.Width + 10 + lblTitle.Width, titleIcon.Height);
                titleGroup.Location = new Point((topBar.Width - titleGroup.Width) / 2, (topBar.Height - titleGroup.Height) / 2);
            };

            // Sub-header: fund name + stats
            var subHeader = new Panel { Location = new Point(0, 56), Size = new Size(headerWrap.Width, 44), BackColor = NavBlueLight };
            headerWrap.Controls.Add(subHeader);
            headerWrap.Resize += (s, e) => subHeader.Width = headerWrap.Width;

            var lblFundName = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(20, 12)
            };
            var lblPendingBadge = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 220, 100),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            var pendingIcon = new Panel { Size = new Size(15, 15), BackColor = Color.Transparent };
            string pendingIconKind = "hourglass";
            Color pendingIconColor = Color.FromArgb(255, 220, 100);
            pendingIcon.Paint += (s, e) =>
                DrawIcon(e.Graphics, new Rectangle(0, 1, 14, 13), pendingIconColor, pendingIconKind);
            void RepositionPendingBadge()
            {
                lblPendingBadge.Location = new Point(subHeader.Width - lblPendingBadge.Width - 20, 14);
                pendingIcon.Location = new Point(lblPendingBadge.Left - 20, 15);
            }
            subHeader.Controls.Add(lblFundName);
            subHeader.Controls.Add(pendingIcon);
            subHeader.Controls.Add(lblPendingBadge);
            subHeader.Resize += (s, e) => RepositionPendingBadge();

            // ── Main scrollable body ──────────────────────────────────────────
            // FIX: like the Beneficiaries screen above, this panel no longer
            // uses Dock = DockStyle.Fill as a sibling of headerWrap (that
            // combination was rendering it as if it started at y=0, hidden
            // behind the header, clipping the QR scan card's top content).
            // Its bounds are now computed explicitly from view.ClientSize and
            // the fixed header height on every resize.
            var scroll = new NoAutoScrollPanel { AutoScroll = true, BackColor = BgPage };
            view.Controls.Add(scroll);

            void LayoutScroll()
            {
                scroll.SetBounds(0, headerH, view.ClientSize.Width, Math.Max(0, view.ClientSize.Height - headerH));
            }
            view.Resize += (s, e) => LayoutScroll();
            LayoutScroll();

            var content = new Panel { BackColor = BgPage, Location = new Point(20, 16), Size = new Size(900, 900) };
            scroll.Controls.Add(content);

            scroll.Resize += (s, e) =>
            {
                int w = scroll.ClientSize.Width - 40;
                if (w < 300) return;
                content.Width = w;
            };

            // ── MIDDLE SECTION: QR Scan card ─────────────────────────────────
            // Positioned (and centered internally) as the focal/"main" panel
            // of the Distribution screen, sitting between the Beneficiaries
            // List card above it and the release-confirmation panel that
            // appears directly beneath the scanner once a resident is
            // matched.
            const string ScanPlaceholder = "Scan QR here, or type ID and press Enter...";
            var scanCard = new Panel
            {
                BackColor = BgCard,
                Location = new Point(0, 420),
                Size = new Size(800, 414),
                BorderStyle = BorderStyle.None
            };
            scanCard.Paint += (s, e) =>
            {
                PaintCard(e.Graphics, scanCard);
                using (var pen = new Pen(BtnTeal, 3))
                {
                    e.Graphics.DrawLine(pen, 0, 0, scanCard.Width, 0);
                }
            };
            content.Controls.Add(scanCard);

            // Colored scan card header
            var scanHdrBar = new Panel { BackColor = BtnTeal, Location = new Point(0, 0), Height = 50 };
            scanHdrBar.Paint += (s, e) => {
                using (var lg = new LinearGradientBrush(scanHdrBar.ClientRectangle, BtnTeal, Color.FromArgb(0, 160, 150), LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(lg, scanHdrBar.ClientRectangle);
            };
            var scanIcon = IconBadge("camera", Color.FromArgb(55, 255, 255, 255), Color.White, 26);
            scanIcon.Location = new Point(14, 12);
            scanHdrBar.Controls.Add(scanIcon);
            scanHdrBar.Controls.Add(new Label { Text = "Scan Resident QR Code", Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, BackColor = Color.Transparent, Location = new Point(50, 15) });
            scanCard.Controls.Add(scanHdrBar);
            scanCard.Resize += (s, e2) => scanHdrBar.Width = scanCard.Width;

            var divScan = new Panel { BackColor = BorderCol, Location = new Point(0, 50), Height = 1 };
            scanCard.Controls.Add(divScan);
            scanCard.Resize += (s2, e2) => { divScan.Width = scanCard.Width; };

            // Soft tinted frame behind the scanner box so it has some visual
            // weight instead of floating bare on the white card background.
            var scannerFrame = new Panel
            {
                Location = new Point(14, 66),
                Size = new Size(180, 180),
                BackColor = Color.Transparent
            };
            scannerFrame.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var fr = new Rectangle(0, 0, scannerFrame.Width - 1, scannerFrame.Height - 1);
                using (var brush = new SolidBrush(ScannerBg))
                using (var pen = new Pen(Color.FromArgb(205, 225, 240), 1f))
                using (var path = RoundedPath(fr, 14))
                {
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                }
            };
            scanCard.Controls.Add(scannerFrame);

            // QR Scanner visual — parented directly to scannerFrame (not a
            // sibling) so it is guaranteed to paint on top of the tinted
            // background regardless of control add order.
            var qrScannerBox = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(160, 160),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            qrScannerBox.Paint += (s, e) =>
            {
                DrawQrScannerGlyph(e.Graphics, qrScannerBox.ClientRectangle, NavBlue, BtnTeal);
                using (var pen = new Pen(BtnTeal, 2))
                {
                    int corner = 14;
                    int offset = 6;
                    e.Graphics.DrawLine(pen, offset, offset, offset + corner, offset);
                    e.Graphics.DrawLine(pen, offset, offset, offset, offset + corner);
                    e.Graphics.DrawLine(pen, qrScannerBox.Width - offset - corner, offset, qrScannerBox.Width - offset, offset);
                    e.Graphics.DrawLine(pen, qrScannerBox.Width - offset, offset, qrScannerBox.Width - offset, offset + corner);
                    e.Graphics.DrawLine(pen, offset, qrScannerBox.Height - offset - corner, offset, qrScannerBox.Height - offset);
                    e.Graphics.DrawLine(pen, offset, qrScannerBox.Height - offset, offset + corner, qrScannerBox.Height - offset);
                    e.Graphics.DrawLine(pen, qrScannerBox.Width - offset - corner, qrScannerBox.Height - offset, qrScannerBox.Width - offset, qrScannerBox.Height - offset);
                    e.Graphics.DrawLine(pen, qrScannerBox.Width - offset, qrScannerBox.Height - offset - corner, qrScannerBox.Width - offset, qrScannerBox.Height - offset);
                }
            };
            scannerFrame.Controls.Add(qrScannerBox);

            // Instructions
            var lblInstructions = new Label
            {
                Text = "Position the QR code in front of the scanner,\nor type the resident ID manually below.",
                Font = new Font("Segoe UI", 9f),
                ForeColor = TextGray,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Location = new Point(0, 258)
            };
            scanCard.Controls.Add(lblInstructions);

            // Scan input
            var lblScanLabel = new Label
            {
                Text = "RESIDENT ID / QR CODE",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = TextGray,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(0, 300)
            };
            scanCard.Controls.Add(lblScanLabel);

            var txtScan = new TextBox
            {
                Font = new Font("Consolas", 11f),
                Location = new Point(0, 318),
                Size = new Size(300, 30),
                BorderStyle = BorderStyle.FixedSingle,
                Text = "",
                ForeColor = Color.Black,
                BackColor = Color.White
            };

            // Real placeholder: a separate label drawn over the empty textbox,
            // instead of stuffing the hint text into the Text property itself.
            // This way the box is always genuinely empty when cleared — no
            // more "ghost text" that looks like real content.
            var lblScanPlaceholder = new Label
            {
                Text = ScanPlaceholder,
                Font = new Font("Consolas", 9.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(165, 170, 185),
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(txtScan.Left + 6, txtScan.Top),
                Size = new Size(txtScan.Width - 12, txtScan.Height),
                Cursor = Cursors.IBeam
            };
            lblScanPlaceholder.Click += (s, e) => txtScan.Focus();

            // FIXED: Proper placeholder visibility logic
            void UpdateScanPlaceholder()
            {
                // Show placeholder ONLY when text is empty AND textbox does NOT have focus
                lblScanPlaceholder.Visible = string.IsNullOrEmpty(txtScan.Text) && !txtScan.Focused;
            }

            txtScan.TextChanged += (s, e) => UpdateScanPlaceholder();
            txtScan.GotFocus += (s, e) => UpdateScanPlaceholder();
            txtScan.LostFocus += (s, e) => UpdateScanPlaceholder();

            // FIXED: Also hook into the textbox's Enter/Leave events for better reliability
            txtScan.Enter += (s, e) => UpdateScanPlaceholder();
            txtScan.Leave += (s, e) => UpdateScanPlaceholder();

            var btnClearScan = SmallBtn("✕ Clear", BtnGray);
            btnClearScan.Size = new Size(72, 30);
            btnClearScan.Location = new Point(0, 318);

            var lblScanStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = TextGray,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(0, 358)
            };
            scanCard.Controls.Add(txtScan);
            scanCard.Controls.Add(lblScanPlaceholder);
            lblScanPlaceholder.BringToFront();
            // Initial call to set the correct visibility
            UpdateScanPlaceholder();
            scanCard.Controls.Add(btnClearScan);
            scanCard.Controls.Add(lblScanStatus);

            // Scan button
            var btnScan = MakeBtn("Scan", BtnBlue, 10f);
            btnScan.Size = new Size(90, 30);
            btnScan.Location = new Point(0, 318);
            btnScan.Click += (s, e) =>
            {
                txtScan.Focus();
                txtScan.Text = txtScan.Text.Trim();
                SendKeys.Send("{ENTER}");
            };
            scanCard.Controls.Add(btnScan);

            // Verified resident info card
            var verCard = new Panel
            {
                BackColor = Color.FromArgb(248, 255, 252),
                Location = new Point(24, 394),
                Size = new Size(760, 60),
                Visible = false
            };
            verCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.FromArgb(150, 200, 170)))
                    RoundRect(e.Graphics, pen,
                        new Rectangle(0, 0, verCard.Width - 1, verCard.Height - 1), 8);
                using (var b = new SolidBrush(GreenAccent))
                    e.Graphics.FillRectangle(b, 0, 0, 6, verCard.Height);
            };
            scanCard.Controls.Add(verCard);

            var lblVBadge = new Label
            {
                Text = "✔ IDENTITY VERIFIED",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = ReleasedGreen,
                AutoSize = true,
                Location = new Point(12, 4),
                BackColor = Color.Transparent
            };
            var lblVName = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(12, 18),
                BackColor = Color.Transparent
            };
            var lblVDetails = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8f),
                ForeColor = TextGray,
                AutoSize = true,
                Location = new Point(12, 38),
                BackColor = Color.Transparent
            };

            var lblVFund = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(380, 4),
                BackColor = Color.Transparent
            };
            var lblVStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = PendingAmber,
                AutoSize = true,
                Location = new Point(380, 18),
                BackColor = Color.Transparent
            };

            var btnRelQr = MakeBtn("✓  Confirm Release", BtnTeal, 9f);
            btnRelQr.Size = new Size(140, 28);
            btnRelQr.Location = new Point(600, 18);
            btnRelQr.Visible = false;

            // NEW: shown instead of btnRelQr when the scanned/matched
            // resident is a legitimate resident but is NOT yet a
            // beneficiary of the currently selected fund. Opens the same
            // "Add Beneficiary" dialog used on the Beneficiaries screen.
            var btnAddBenefQr = MakeBtn("➕  Add as Beneficiary", BtnGreen, 9f);
            btnAddBenefQr.Size = new Size(170, 28);
            btnAddBenefQr.Location = new Point(600, 18);
            btnAddBenefQr.Visible = false;

            verCard.Controls.Add(lblVBadge);
            verCard.Controls.Add(lblVName);
            verCard.Controls.Add(lblVDetails);
            verCard.Controls.Add(lblVFund);
            verCard.Controls.Add(lblVStatus);
            verCard.Controls.Add(btnRelQr);
            verCard.Controls.Add(btnAddBenefQr);

            verCard.Resize += (s, e) =>
            {
                btnRelQr.Location = new Point(verCard.Width - btnRelQr.Width - 12, 18);
                btnAddBenefQr.Location = new Point(verCard.Width - btnAddBenefQr.Width - 12, 18);
            };

            // Centers the QR scanner (now the card's "hero" element) and the
            // scan input row horizontally, regardless of card width, so the
            // QR code always reads as the main, focal item of the screen.
            void CenterScanControls()
            {
                int cw = scanCard.Width;

                scannerFrame.Location = new Point(Math.Max(20, (cw - scannerFrame.Width) / 2), 66);

                lblInstructions.Location = new Point(
                    Math.Max(0, (cw - lblInstructions.Width) / 2), scannerFrame.Bottom + 12);

                lblScanLabel.Location = new Point(
                    Math.Max(0, (cw - lblScanLabel.Width) / 2), lblInstructions.Bottom + 10);

                int rowWidth = txtScan.Width + 8 + btnScan.Width + 8 + btnClearScan.Width;
                int rowX = Math.Max(20, (cw - rowWidth) / 2);
                int rowY = lblScanLabel.Bottom + 8;
                txtScan.Location = new Point(rowX, rowY);
                lblScanPlaceholder.Location = new Point(txtScan.Left + 6, txtScan.Top);
                btnScan.Location = new Point(txtScan.Right + 8, rowY);
                btnClearScan.Location = new Point(btnScan.Right + 8, rowY);

                lblScanStatus.Location = new Point(rowX, rowY + txtScan.Height + 10);

                verCard.Location = new Point(24, lblScanStatus.Bottom + 14);
            }

            scanCard.Resize += (s, e) =>
            {
                verCard.Width = scanCard.Width - 48;
                CenterScanControls();
                scanCard.Height = verCard.Visible ? 474 : 414;
            };
            CenterScanControls();

            // ── TOP SECTION: Beneficiary list card (All / Pending / Released) ──
            var listCard = new Panel { BackColor = BgCard, Location = new Point(0, 0), Size = new Size(800, 400) };
            listCard.Paint += (s, e) => PaintCard(e.Graphics, listCard);
            content.Controls.Add(listCard);

            var listHdrBar = new Panel { BackColor = NavBlue, Location = new Point(0, 0), Height = 46 };
            listHdrBar.Paint += (s, e) => {
                using (var lg = new LinearGradientBrush(listHdrBar.ClientRectangle, NavBlue, NavBlueLight, LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(lg, listHdrBar.ClientRectangle);
            };
            var listIcon = IconBadge("clipboard", Color.FromArgb(55, 255, 255, 255), Color.White, 22);
            listIcon.Location = new Point(14, 12);
            listHdrBar.Controls.Add(listIcon);
            listHdrBar.Controls.Add(new Label { Text = "Beneficiaries List", Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, BackColor = Color.Transparent, Location = new Point(44, 13) });

            // Print Report — lets the user filter (date range / status) and
            // choose a template before printing a paper record of the fund's
            // beneficiaries and release status.
            var btnPrintReport = SmallBtn("🖨  Print Report", BtnBlue);
            btnPrintReport.Size = new Size(150, 30);
            btnPrintReport.Location = new Point(listCard.Width - btnPrintReport.Width - 12, 8);
            btnPrintReport.Click += (s, e) => ShowPrintReportDialog();
            listHdrBar.Controls.Add(btnPrintReport);

            listCard.Controls.Add(listHdrBar);
            listCard.Resize += (s, e) =>
            {
                listHdrBar.Width = listCard.Width;
                btnPrintReport.Location = new Point(listHdrBar.Width - btnPrintReport.Width - 12, 8);
            };
            var divList = new Panel { BackColor = BorderCol, Location = new Point(0, 46), Height = 1 };
            listCard.Controls.Add(divList);
            listCard.Resize += (s2, e2) => divList.Width = listCard.Width;

            // Three separate lists, shown together at the same time instead
            // of one grid behind a pill filter: the full Beneficiaries
            // roster, a Pending column (awaiting release, with the Release
            // action), and a Released column (already paid out, showing
            // the date it was released).
            const int listsTop = 58;
            const int colGap = 10;

            var colBenefHdr = new Label { Text = "👥  Beneficiaries", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = NavBlue, AutoSize = true, BackColor = Color.Transparent, Location = new Point(4, listsTop) };
            var colPendingHdr = new Label { Text = "⏳  Pending", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = PendingAmber, AutoSize = true, BackColor = Color.Transparent, Location = new Point(4, listsTop) };
            var colReleasedHdr = new Label { Text = "✔  Released", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = ReleasedGreen, AutoSize = true, BackColor = Color.Transparent, Location = new Point(4, listsTop) };
            listCard.Controls.Add(colBenefHdr);
            listCard.Controls.Add(colPendingHdr);
            listCard.Controls.Add(colReleasedHdr);

            var dgvAll = MakeGrid();
            var dgvPending = MakeGrid();
            var dgvReleased = MakeGrid();
            listCard.Controls.Add(dgvAll);
            listCard.Controls.Add(dgvPending);
            listCard.Controls.Add(dgvReleased);

            // Small "nothing here yet" placeholders, one per column, shown
            // instead of a blank grid when that particular list has no rows.
            Label MakeColEmptyLabel(string text)
            {
                var lbl = new Label
                {
                    Text = text,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                    ForeColor = TextGray,
                    TextAlign = ContentAlignment.TopCenter,
                    BackColor = Color.Transparent,
                    Visible = false
                };
                listCard.Controls.Add(lbl);
                lbl.BringToFront();
                return lbl;
            }
            var lblAllEmpty = MakeColEmptyLabel("No beneficiaries yet.\nAdd residents from the\nBeneficiaries screen first.");
            var lblPendingEmpty = MakeColEmptyLabel("Nothing pending.");
            var lblReleasedEmpty = MakeColEmptyLabel("Nothing released yet.");

            const int gridTop = listsTop + 22;
            void LayoutLists()
            {
                int w = listCard.Width;
                int h = Math.Max(0, listCard.Height - gridTop - 8);
                int colW = Math.Max(140, (w - 8 - colGap * 2) / 3);

                int x1 = 4;
                int x2 = x1 + colW + colGap;
                int x3 = x2 + colW + colGap;

                colBenefHdr.Location = new Point(x1 + 4, listsTop);
                colPendingHdr.Location = new Point(x2 + 4, listsTop);
                colReleasedHdr.Location = new Point(x3 + 4, listsTop);

                dgvAll.SetBounds(x1, gridTop, colW, h);
                dgvPending.SetBounds(x2, gridTop, colW, h);
                dgvReleased.SetBounds(x3, gridTop, colW, h);

                lblAllEmpty.SetBounds(x1, gridTop + 10, colW, h - 10);
                lblPendingEmpty.SetBounds(x2, gridTop + 10, colW, h - 10);
                lblReleasedEmpty.SetBounds(x3, gridTop + 10, colW, h - 10);
            }
            listCard.Resize += (s, e) => LayoutLists();
            LayoutLists();

            // Responsive resize — Beneficiaries List sits on top, the QR
            // scanner (the "main" panel) sits directly beneath it in the
            // middle of the screen. Card is a bit taller now so all three
            // lists have room to breathe.
            scroll.Resize += (s, e) =>
            {
                int w = scroll.ClientSize.Width - 40;
                if (w < 300) return;
                content.Width = w;
                listCard.SetBounds(0, 0, w, Math.Max(300, Math.Min(460, scroll.ClientSize.Height - scanCard.Height - 68)));
                scanCard.Width = w;
                scanCard.Location = new Point(0, listCard.Bottom + 20);
                content.Height = scanCard.Bottom + 20;
            };

            // ── Data loader ───────────────────────────────────────────────────
            // One query pulls every beneficiary of the fund; the results are
            // then split client-side into the three lists (All / Pending /
            // Released) so all three can be shown together at once.
            void LoadBenef()
            {
                if (_selectedFundId == 0)
                {
                    dgvAll.DataSource = null; dgvPending.DataSource = null; dgvReleased.DataSource = null;
                    return;
                }
                var conn = DatabaseConnection.GetConnection(); if (conn == null) return;
                using (conn)
                {
                    try
                    {
                        string q = @"
                            SELECT fb.idBeneficiary AS BenefID,
                                   fb.ResidentID,
                                   CONCAT(r.LastName,', ',r.FirstName) AS Resident,
                                   bt.Category AS Fund,
                                   r.Address,
                                   fb.AllocatedAmount AS Allocated,
                                   CASE WHEN fb.IsReleased=1 THEN 'Released' ELSE 'Pending' END AS Status,
                                   fb.DateReleased AS Released,
                                   fb.ReleasedBy
                            FROM fund_beneficiaries fb
                            INNER JOIN resident r ON fb.ResidentID=r.idResident
                            INNER JOIN budget_transactions bt ON bt.idTransaction=fb.FundID
                            WHERE fb.FundID=@fid
                            ORDER BY fb.IsReleased ASC,r.LastName";
                        var da = new SQLiteDataAdapter(q, conn);
                        da.SelectCommand.Parameters.AddWithValue("@fid", _selectedFundId);
                        var dtAll = new DataTable(); da.Fill(dtAll);

                        DataTable dtPending = dtAll.Clone();
                        DataTable dtReleased = dtAll.Clone();
                        foreach (DataRow r in dtAll.Rows)
                        {
                            if (r["Status"].ToString() == "Released") dtReleased.ImportRow(r);
                            else dtPending.ImportRow(r);
                        }

                        dgvAll.DataSource = dtAll;
                        dgvPending.DataSource = dtPending;
                        dgvReleased.DataSource = dtReleased;

                        // Beneficiaries (full roster) — who's on this fund and
                        // their current status.
                        ColVis(dgvAll, "BenefID", false);
                        ColVis(dgvAll, "ResidentID", false);
                        ColHdr(dgvAll, "Resident", "Resident", 140);
                        ColFill(dgvAll, "Resident");
                        ColVis(dgvAll, "Fund", false);
                        ColVis(dgvAll, "Address", false);
                        ColVis(dgvAll, "Allocated", false);
                        ColW(dgvAll, "Status", 80);
                        ColVis(dgvAll, "Released", false);
                        ColVis(dgvAll, "ReleasedBy", false);
                        dgvAll.CellFormatting -= ColorBenefStatus;
                        dgvAll.CellFormatting += ColorBenefStatus;

                        if (dgvAll.Columns.Contains("Actions")) dgvAll.Columns.Remove("Actions");
                        dgvAll.Columns.Add(MakeActionsColumn());

                        // Pending — awaiting release, with a one-click Release
                        // action.
                        ColVis(dgvPending, "BenefID", false);
                        ColVis(dgvPending, "ResidentID", false);
                        ColHdr(dgvPending, "Resident", "Resident", 130);
                        ColFill(dgvPending, "Resident");
                        ColVis(dgvPending, "Fund", false);
                        ColVis(dgvPending, "Address", false);
                        ColFmt(dgvPending, "Allocated", 108, "₱#,##0.00");
                        ColVis(dgvPending, "Status", false);
                        ColVis(dgvPending, "Released", false);
                        ColVis(dgvPending, "ReleasedBy", false);

                        if (dgvPending.Columns.Contains("Release")) dgvPending.Columns.Remove("Release");
                        var btnCol = new DataGridViewButtonColumn
                        {
                            Name = "Release",
                            HeaderText = "",
                            UseColumnTextForButtonValue = true,
                            Text = "Release",
                            Width = 90,
                            FlatStyle = FlatStyle.Flat,
                            DefaultCellStyle = {
                                BackColor = BtnTeal,
                                ForeColor = Color.White,
                                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
                            }
                        };
                        dgvPending.Columns.Add(btnCol);

                        if (dgvPending.Columns.Contains("Actions")) dgvPending.Columns.Remove("Actions");
                        dgvPending.Columns.Add(MakeActionsColumn());

                        // Released — already paid out, with the date (and who
                        // released it).
                        ColVis(dgvReleased, "BenefID", false);
                        ColVis(dgvReleased, "ResidentID", false);
                        ColHdr(dgvReleased, "Resident", "Resident", 120);
                        ColVis(dgvReleased, "Fund", false);
                        ColVis(dgvReleased, "Address", false);
                        ColFmt(dgvReleased, "Allocated", 100, "₱#,##0.00");
                        ColVis(dgvReleased, "Status", false);
                        ColHdr(dgvReleased, "Released", "Date Released", 110);
                        ColFmt(dgvReleased, "Released", 110, "MMM dd, yyyy");
                        ColHdr(dgvReleased, "ReleasedBy", "Released By", 100);
                        ColFill(dgvReleased, "ReleasedBy");

                        if (dgvReleased.Columns.Contains("Actions")) dgvReleased.Columns.Remove("Actions");
                        dgvReleased.Columns.Add(MakeActionsColumn());

                        // ── Row color-coding ──────────────────────────────────
                        // "Beneficiaries" mixes both statuses in one list, so
                        // each row gets a pale tint matching its status (green
                        // for released, amber for pending) for at-a-glance
                        // identification. Pending / Released are already
                        // single-status lists (their column headers carry the
                        // same amber/green accent), so a uniform tint there
                        // just reinforces which list you're looking at.
                        //
                        // SelectionBackColor is set to a slightly darker shade
                        // of the SAME tint (not the grid's default blue) —
                        // WinForms selects the whole row on any click before
                        // CellClick fires, and with the default blue that
                        // repainted every cell (status tint, action pills,
                        // everything) solid blue, making the row look like one
                        // merged block. Darkening the row's own tint instead
                        // gives a click a light "pressed" feel without losing
                        // the status color-coding.
                        foreach (DataGridViewRow gr in dgvAll.Rows)
                        {
                            bool rowReleased = gr.Cells["Status"].Value?.ToString() == "Released";
                            Color tint = rowReleased ? PaleGreenRow : PaleAmberRow;
                            gr.DefaultCellStyle.BackColor = tint;
                            gr.DefaultCellStyle.SelectionBackColor = Darken(tint, 14);
                            gr.DefaultCellStyle.SelectionForeColor = Color.FromArgb(40, 46, 70);
                        }
                        foreach (DataGridViewRow gr in dgvPending.Rows)
                        {
                            gr.DefaultCellStyle.BackColor = PaleAmberRow;
                            gr.DefaultCellStyle.SelectionBackColor = Darken(PaleAmberRow, 14);
                            gr.DefaultCellStyle.SelectionForeColor = Color.FromArgb(40, 46, 70);
                        }
                        foreach (DataGridViewRow gr in dgvReleased.Rows)
                        {
                            gr.DefaultCellStyle.BackColor = PaleGreenRow;
                            gr.DefaultCellStyle.SelectionBackColor = Darken(PaleGreenRow, 14);
                            gr.DefaultCellStyle.SelectionForeColor = Color.FromArgb(40, 46, 70);
                        }

                        // Also clear any pre-existing selection so nothing
                        // shows the (now much subtler) selected state before
                        // the person actually clicks a row.
                        dgvAll.ClearSelection();
                        dgvPending.ClearSelection();
                        dgvReleased.ClearSelection();

                        int totalC = dtAll.Rows.Count;
                        int pendingC = dtPending.Rows.Count;
                        int releasedC = dtReleased.Rows.Count;

                        int pc = pendingC;
                        lblPendingBadge.Text = pc > 0
                            ? $"{pc} pending release{(pc == 1 ? "" : "s")}"
                            : "All released";
                        lblPendingBadge.ForeColor = pc > 0 ? Color.FromArgb(255, 220, 100) : GreenAccent;
                        pendingIconKind = pc > 0 ? "hourglass" : "check";
                        pendingIconColor = lblPendingBadge.ForeColor;
                        pendingIcon.Invalidate();
                        RepositionPendingBadge();

                        dgvAll.Visible = totalC > 0;
                        lblAllEmpty.Visible = totalC == 0;
                        dgvPending.Visible = pendingC > 0;
                        lblPendingEmpty.Visible = pendingC == 0;
                        dgvReleased.Visible = releasedC > 0;
                        lblReleasedEmpty.Visible = releasedC == 0;
                    }
                    catch (Exception ex) { MessageBox.Show("Error loading beneficiaries:\n" + ex.Message); }
                }
            }

            // ── Row actions menu (⋮) — shared by all three beneficiary lists ──
            // Shown when the "Actions" column is clicked. Clicking anywhere
            // else on a row opens the resident's profile popup instead (see
            // the CellClick handlers for dgvAll / dgvPending / dgvReleased
            // below). "Edit Allocated Amount" and "Remove Beneficiary" are
            // left out of the menu entirely once the beneficiary has already
            // been released, since a paid-out allocation shouldn't change.
            void ShowRowActionsMenu(DataGridView grid, int rowIndex, Point screenLoc)
            {
                var row = grid.Rows[rowIndex];
                int bid = Convert.ToInt32(row.Cells["BenefID"].Value);
                int residId = Convert.ToInt32(row.Cells["ResidentID"].Value);
                string name = row.Cells["Resident"].Value?.ToString() ?? "";
                string status = grid.Columns.Contains("Status") ? row.Cells["Status"].Value?.ToString() : null;
                bool isReleased = status == "Released";
                decimal alloc = 0;
                try { alloc = Convert.ToDecimal(row.Cells["Allocated"].Value); } catch { }

                var menu = new ContextMenuStrip();
                menu.Items.Add("👤  View Profile", null, (s, e) =>
                {
                    ShowResidentProfilePopup(residId, () => { LoadBenef(); LoadSummaryCards(); });
                });

                // Edit Allocated Amount stays available even after release — it's
                // treated as a correction to the record, not a reversal of the
                // payment (see ShowEditAllocationDialog's warning banner for
                // released rows). Remove stays locked once released, since the
                // money has already gone out and the row needs to stay as the
                // paper trail for it.
                menu.Items.Add("✏️  Edit Allocated Amount", null, (s, e) =>
                {
                    ShowEditAllocationDialog(bid, name, alloc, isReleased, () => { LoadBenef(); LoadSummaryCards(); });
                });

                if (!isReleased)
                {
                    menu.Items.Add(new ToolStripSeparator());
                    var removeItem = menu.Items.Add("🗑  Remove Beneficiary", null, (s, e) =>
                    {
                        if (MessageBox.Show($"Remove {name} from this fund?", "Confirm",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                        var conn = DatabaseConnection.GetConnection(); if (conn == null) return;
                        using (conn)
                        {
                            try
                            {
                                var cmd = new SQLiteCommand("DELETE FROM fund_beneficiaries WHERE idBeneficiary=@id", conn);
                                cmd.Parameters.AddWithValue("@id", bid);
                                cmd.ExecuteNonQuery();
                                AuditLogger.Log("REMOVE_BENEFICIARY", "Ayuda", $"Removed {name} from fund {_selectedFundName}");
                                LoadBenef();
                                LoadSummaryCards();
                            }
                            catch (Exception ex) { MessageBox.Show(ex.Message); }
                        }
                    });
                    ((ToolStripMenuItem)removeItem).ForeColor = BtnRed;
                }

                menu.Show(screenLoc);
            }

            // ── Compact custom-painted cells for "Actions" (⋮) and "Release" ──
            // DataGridViewButtonColumn cells render as a solid block filling
            // the whole cell — at this row height that read as an oversized,
            // edge-to-edge button, and once a row was selected the button's
            // own color got washed out by the row-selection blue, making the
            // whole row look like one merged block. This paints a small,
            // fixed-size pill centered in the cell instead — a 20px rounded
            // square for the kebab, a snug rounded pill for "Release" — so
            // both stay compact and keep their own color regardless of row
            // selection state.
            void PaintCompactActionCells(object sender, DataGridViewCellPaintingEventArgs e)
            {
                var grid = sender as DataGridView;
                if (grid == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;
                string colName = grid.Columns[e.ColumnIndex].Name;

                if (colName == "Actions")
                {
                    e.PaintBackground(e.ClipBounds, true);
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    const int d = 20;
                    var rect = new Rectangle(
                        e.CellBounds.X + (e.CellBounds.Width - d) / 2,
                        e.CellBounds.Y + (e.CellBounds.Height - d) / 2, d, d);
                    using (var path = RoundedPath(rect, 5))
                    using (var brush = new SolidBrush(KebabPillBg))
                        g.FillPath(brush, path);
                    using (var f = new Font("Segoe UI", 9f, FontStyle.Bold))
                    using (var sb = new SolidBrush(NavBlue))
                    {
                        var sz = g.MeasureString("⋮", f);
                        g.DrawString("⋮", f, sb, rect.X + (rect.Width - sz.Width) / 2, rect.Y + (rect.Height - sz.Height) / 2 - 2);
                    }
                    e.Handled = true;
                }
                else if (colName == "Release")
                {
                    e.PaintBackground(e.ClipBounds, true);
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    const string text = "Release";
                    using (var f = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                    {
                        var sz = g.MeasureString(text, f);
                        int padX = 12, padY = 5;
                        int bw = (int)sz.Width + padX * 2;
                        int bh = (int)sz.Height + padY;
                        var rect = new Rectangle(
                            e.CellBounds.X + (e.CellBounds.Width - bw) / 2,
                            e.CellBounds.Y + (e.CellBounds.Height - bh) / 2, bw, bh);
                        using (var path = RoundedPath(rect, bh / 2))
                        using (var brush = new SolidBrush(BtnTeal))
                            g.FillPath(brush, path);
                        using (var sb = new SolidBrush(Color.White))
                            g.DrawString(text, f, sb, rect.X + padX, rect.Y + (bh - sz.Height) / 2f - 1);
                    }
                    e.Handled = true;
                }
            }
            dgvAll.CellPainting += PaintCompactActionCells;
            dgvPending.CellPainting += PaintCompactActionCells;
            dgvReleased.CellPainting += PaintCompactActionCells;

            dgvAll.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                if (dgvAll.Columns[e.ColumnIndex].Name == "Actions")
                {
                    var cellRect = dgvAll.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                    var screenPt = dgvAll.PointToScreen(new Point(cellRect.Left, cellRect.Bottom));
                    ShowRowActionsMenu(dgvAll, e.RowIndex, screenPt);
                    return;
                }
                int residId = Convert.ToInt32(dgvAll.Rows[e.RowIndex].Cells["ResidentID"].Value);
                ShowResidentProfilePopup(residId, () => { LoadBenef(); LoadSummaryCards(); });
            };

            dgvReleased.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                if (dgvReleased.Columns[e.ColumnIndex].Name == "Actions")
                {
                    var cellRect = dgvReleased.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                    var screenPt = dgvReleased.PointToScreen(new Point(cellRect.Left, cellRect.Bottom));
                    ShowRowActionsMenu(dgvReleased, e.RowIndex, screenPt);
                    return;
                }
                int residId = Convert.ToInt32(dgvReleased.Rows[e.RowIndex].Cells["ResidentID"].Value);
                ShowResidentProfilePopup(residId, () => { LoadBenef(); LoadSummaryCards(); });
            };

            dgvPending.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
                string colName = dgvPending.Columns[e.ColumnIndex].Name;

                if (colName == "Actions")
                {
                    var cellRect = dgvPending.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
                    var screenPt = dgvPending.PointToScreen(new Point(cellRect.Left, cellRect.Bottom));
                    ShowRowActionsMenu(dgvPending, e.RowIndex, screenPt);
                    return;
                }

                if (colName == "Release")
                {
                    var row = dgvPending.Rows[e.RowIndex];
                    int bid = Convert.ToInt32(row.Cells["BenefID"].Value);
                    string name = row.Cells["Resident"].Value?.ToString() ?? "";
                    decimal alloc = 0;
                    try { alloc = Convert.ToDecimal(row.Cells["Allocated"].Value); } catch { }
                    if (MessageBox.Show(
                        $"Release ₱{alloc:N2} to {name}?\n\nFund: {_selectedFundName}\nReleased by: {CurrentUser.Username}",
                        "Confirm Release", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                    ReleaseBeneficiary(bid, alloc, name, () =>
                    {
                        LoadBenef(); LoadSummaryCards();
                        verCard.Visible = false; txtScan.Text = ""; lblScanStatus.Text = "";
                        UpdateScanPlaceholder();
                    });
                    return;
                }

                int residId2 = Convert.ToInt32(dgvPending.Rows[e.RowIndex].Cells["ResidentID"].Value);
                ShowResidentProfilePopup(residId2, () => { LoadBenef(); LoadSummaryCards(); });
            };

            void RemoveBeneficiary(DataGridView grid)
            {
                if (grid.SelectedRows.Count == 0) return;
                var row = grid.SelectedRows[0];
                int bid = Convert.ToInt32(row.Cells["BenefID"].Value);
                string nm = row.Cells["Resident"].Value?.ToString() ?? "";
                if (MessageBox.Show($"Remove {nm} from this fund?", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                var conn = DatabaseConnection.GetConnection(); if (conn == null) return;
                using (conn)
                {
                    try
                    {
                        var cmd = new SQLiteCommand(
                            "DELETE FROM fund_beneficiaries WHERE idBeneficiary=@id", conn);
                        cmd.Parameters.AddWithValue("@id", bid);
                        cmd.ExecuteNonQuery();
                        LoadBenef();
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                }
            }

            var ctxAll = new ContextMenuStrip();
            ctxAll.Items.Add("Remove Beneficiary", null, (s, e) =>
            {
                if (dgvAll.SelectedRows.Count == 0) return;
                if (dgvAll.SelectedRows[0].Cells["Status"].Value?.ToString() == "Released")
                {
                    MessageBox.Show("Cannot remove a released beneficiary.", "Not Allowed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                RemoveBeneficiary(dgvAll);
            });
            dgvAll.ContextMenuStrip = ctxAll;

            var ctxPending = new ContextMenuStrip();
            ctxPending.Items.Add("Remove Beneficiary", null, (s, e) => RemoveBeneficiary(dgvPending));
            dgvPending.ContextMenuStrip = ctxPending;

            // ── QR scan logic ─────────────────────────────────────────────────
            EventHandler qrReleaseHandler = null;
            EventHandler qrAddBenefHandler = null;

            // FIX: This is the function behind the "Clear" button. Previously
            // it called txtScan.Focus() directly at the end. When invoked from
            // btnClearScan.Click, WinForms hands focus back to the Button
            // itself right after the click finishes processing — so that
            // Focus() call got silently overridden and txtScan never actually
            // regained focus. Because the placeholder-visibility rule is
            // "empty AND not focused", the box stayed unfocused and the
            // placeholder text incorrectly reappeared after pressing Clear
            // (the opposite of the intended behavior). Deferring the focus
            // call with BeginInvoke makes it run after the button's own
            // focus handling completes, so it reliably sticks this time.
            void ClearScanPanel()
            {
                txtScan.Text = "";
                lblScanStatus.Text = "";
                verCard.Visible = false;
                scanCard.Height = 414;
                UpdateScanPlaceholder();

                if (txtScan.IsHandleCreated)
                {
                    txtScan.BeginInvoke(new Action(() =>
                    {
                        txtScan.Focus();
                        UpdateScanPlaceholder();
                    }));
                }
            }

            txtScan.KeyPress += (s, e) =>
            {
                if (e.KeyChar != (char)Keys.Enter) return;
                e.Handled = true;

                string scanValue = txtScan.Text;
                int? resId = ParseResidentIdFromQr(scanValue);
                if (resId == null)
                {
                    lblScanStatus.Text = "✖  Unrecognized QR / ID format";
                    lblScanStatus.ForeColor = BtnRed;
                    verCard.Visible = false;
                    scanCard.Height = 414;
                    UpdateScanPlaceholder();
                    return;
                }
                if (_selectedFundId == 0)
                {
                    lblScanStatus.Text = "⚠  No fund selected — go back to Overview first";
                    lblScanStatus.ForeColor = BtnAmber;
                    verCard.Visible = false;
                    scanCard.Height = 414;
                    UpdateScanPlaceholder();
                    return;
                }

                var conn = DatabaseConnection.GetConnection(); if (conn == null) return;
                using (conn)
                {
                    try
                    {
                        var cmd = new SQLiteCommand(@"
                            SELECT CONCAT(LastName,', ',FirstName,
                                IIF(MiddleName IS NOT NULL AND MiddleName!='',
                                   CONCAT(' ',MiddleName),'')) AS FullName,
                                   Address, TIMESTAMPDIFF(YEAR,DateOfBirth,CURDATE()) AS Age,
                                   Gender, IsActive
                            FROM resident WHERE idResident=@id", conn);
                        cmd.Parameters.AddWithValue("@id", resId.Value);
                        var r = cmd.ExecuteReader();
                        if (!r.Read())
                        {
                            r.Close();
                            lblScanStatus.Text = $"✖  No resident found with ID #{resId.Value}";
                            lblScanStatus.ForeColor = BtnRed;
                            verCard.Visible = false;
                            scanCard.Height = 414;
                            UpdateScanPlaceholder();
                            return;
                        }
                        string fullName = r["FullName"]?.ToString() ?? "";
                        string address = r["Address"]?.ToString() ?? "";
                        string age = r["Age"]?.ToString() ?? "—";
                        string gender = r["Gender"]?.ToString() ?? "—";
                        bool active = r["IsActive"] != DBNull.Value &&
                                          Convert.ToInt32(r["IsActive"]) == 1;
                        r.Close();

                        lblVName.Text = fullName;
                        lblVDetails.Text = $"ID #{resId.Value:D6}  ·  Age {age}  ·  {gender}  ·  {address}";
                        lblVBadge.Text = active ? "✔ IDENTITY VERIFIED" : "⚠ MARKED INACTIVE";
                        lblVBadge.ForeColor = active ? ReleasedGreen : BtnRed;

                        var cmd2 = new SQLiteCommand(@"
                            SELECT idBeneficiary, AllocatedAmount, IsReleased
                            FROM fund_beneficiaries
                            WHERE FundID=@fid AND ResidentID=@rid", conn);
                        cmd2.Parameters.AddWithValue("@fid", _selectedFundId);
                        cmd2.Parameters.AddWithValue("@rid", resId.Value);
                        var r2 = cmd2.ExecuteReader();

                        btnRelQr.Visible = false;
                        if (qrReleaseHandler != null)
                        {
                            btnRelQr.Click -= qrReleaseHandler;
                            qrReleaseHandler = null;
                        }
                        btnAddBenefQr.Visible = false;
                        if (qrAddBenefHandler != null)
                        {
                            btnAddBenefQr.Click -= qrAddBenefHandler;
                            qrAddBenefHandler = null;
                        }

                        if (r2.Read())
                        {
                            decimal alloc = Convert.ToDecimal(r2["AllocatedAmount"]);
                            bool released = r2["IsReleased"] != DBNull.Value && Convert.ToInt32(r2["IsReleased"]) == 1;
                            int matchedBid = Convert.ToInt32(r2["idBeneficiary"]);
                            r2.Close();

                            lblVFund.Text = $"Fund: {_selectedFundName}  ·  Allocated: ₱{alloc:N2}";

                            if (released)
                            {
                                lblVStatus.Text = "✔  Ayuda already released";
                                lblVStatus.ForeColor = ReleasedGreen;
                                btnRelQr.Visible = false;
                            }
                            else
                            {
                                lblVStatus.Text = $"⏳  PENDING — ₱{alloc:N2} ready";
                                lblVStatus.ForeColor = PendingAmber;
                                btnRelQr.Visible = true;

                                string nameCopy = fullName;
                                decimal allocCopy = alloc;
                                int bidCopy = matchedBid;

                                qrReleaseHandler = (bs, be) =>
                                {
                                    if (MessageBox.Show(
                                        $"Release ₱{allocCopy:N2} to {nameCopy}?\n\nFund: {_selectedFundName}\nReleased by: {CurrentUser.Username}",
                                        "Confirm Release",
                                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                                    ReleaseBeneficiary(bidCopy, allocCopy, nameCopy, () =>
                                    {
                                        LoadBenef();
                                        LoadSummaryCards();
                                        ClearScanPanel();
                                    });
                                };
                                btnRelQr.Click += qrReleaseHandler;
                            }

                            foreach (DataGridViewRow gr in dgvAll.Rows)
                            {
                                if (gr.Cells["BenefID"]?.Value != null &&
                                    Convert.ToInt32(gr.Cells["BenefID"].Value) == matchedBid)
                                {
                                    dgvAll.ClearSelection();
                                    gr.Selected = true;
                                    dgvAll.FirstDisplayedScrollingRowIndex = gr.Index;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            r2.Close();
                            lblVFund.Text = $"Fund: {_selectedFundName}";
                            lblVStatus.Text = "Not yet a beneficiary of this fund";
                            lblVStatus.ForeColor = TextGray;
                            btnRelQr.Visible = false;

                            // Resident is legitimate (found in the resident
                            // table) but has no fund_beneficiaries row for
                            // this fund yet — offer to add them right here
                            // instead of sending the user back to the
                            // Beneficiaries screen.
                            int residIdCopy = resId.Value;
                            string nameCopy2 = fullName;

                            qrAddBenefHandler = (bs, be) =>
                            {
                                ShowAddBeneficiaryDialog(residIdCopy, nameCopy2, () =>
                                {
                                    LoadBenef();
                                    LoadSummaryCards();
                                });
                                // Re-run the same scan lookup (mirrors the
                                // "Scan" button's own focus + SendKeys
                                // pattern) so the verified-resident card
                                // immediately reflects the new beneficiary
                                // status without the user re-scanning.
                                txtScan.Focus();
                                SendKeys.Send("{ENTER}");
                            };
                            btnAddBenefQr.Click += qrAddBenefHandler;
                            btnAddBenefQr.Visible = true;
                        }

                        verCard.Visible = true;
                        scanCard.Height = 474;

                        lblScanStatus.Text = $"Matched: {fullName} (#{resId.Value:D6})";
                        lblScanStatus.ForeColor = ReleasedGreen;
                        UpdateScanPlaceholder();

                        // NEW: pop up the full resident profile (photo +
                        // details + fund legitimacy + contextual action)
                        // right on top of the screen instead of making the
                        // user hunt for the info in the inline strip below.
                        int residIdForPopup = resId.Value;
                        ShowResidentProfilePopup(residIdForPopup, () =>
                        {
                            LoadBenef();
                            LoadSummaryCards();
                            if (txtScan.IsHandleCreated)
                            {
                                txtScan.BeginInvoke(new Action(() =>
                                {
                                    txtScan.Focus();
                                    SendKeys.Send("{ENTER}");
                                }));
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        lblScanStatus.Text = "✖  Error: " + ex.Message;
                        lblScanStatus.ForeColor = BtnRed;
                        verCard.Visible = false;
                        scanCard.Height = 414;
                        UpdateScanPlaceholder();
                    }
                }
            };

            btnClearScan.Click += (s, e) => ClearScanPanel();

            view.Tag = (Action)(() =>
            {
                lblFundName.Text = $"Fund: {_selectedFundName}   |   ₱{_selectedFundAmt:N2}";
                LoadBenef();
                ClearScanPanel();
                scroll.ResetScroll(new Point(0, 0));
            });

            view.VisibleChanged += (s, e) =>
            {
                if (!view.Visible) return;
                lblFundName.Text = $"Fund: {_selectedFundName}   |   ₱{_selectedFundAmt:N2}";
                LoadBenef();
                scroll.ResetScroll(new Point(0, 0));
            };

            return view;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PRINTABLE FUND REPORT (filter by date/status, choose a template,
        //  preview or print) — reachable via the "🖨 Print Report" button on
        //  the Distribution screen's Beneficiaries List card.
        // ══════════════════════════════════════════════════════════════════════
        private class ReportRow
        {
            public string Resident;
            public string Address;
            public decimal Allocated;
            public string Status;
            public DateTime? Released;
            public string ReleasedBy;
        }

        private List<ReportRow> GetReportRows(string statusFilter, DateTime? from, DateTime? to)
        {
            var rows = new List<ReportRow>();
            if (_selectedFundId == 0) return rows;

            var conn = DatabaseConnection.GetConnection();
            if (conn == null) return rows;
            using (conn)
            {
                string sc = statusFilter == "Pending" ? " AND fb.IsReleased=0"
                          : statusFilter == "Released" ? " AND fb.IsReleased=1"
                          : "";
                string dateFilter = "";
                if (from.HasValue) dateFilter += " AND fb.DateReleased >= @dfrom";
                if (to.HasValue) dateFilter += " AND fb.DateReleased <= @dto";

                string q = $@"
                    SELECT CONCAT(r.LastName,', ',r.FirstName) AS Resident,
                           r.Address,
                           fb.AllocatedAmount AS Allocated,
                           CASE WHEN fb.IsReleased=1 THEN 'Released' ELSE 'Pending' END AS Status,
                           fb.DateReleased AS Released,
                           fb.ReleasedBy
                    FROM fund_beneficiaries fb
                    INNER JOIN resident r ON fb.ResidentID=r.idResident
                    WHERE fb.FundID=@fid {sc} {dateFilter}
                    ORDER BY fb.IsReleased ASC, r.LastName";

                using (var cmd = new SQLiteCommand(q, conn))
                {
                    cmd.Parameters.AddWithValue("@fid", _selectedFundId);
                    if (from.HasValue) cmd.Parameters.AddWithValue("@dfrom", from.Value.Date);
                    if (to.HasValue) cmd.Parameters.AddWithValue("@dto", to.Value.Date.AddDays(1).AddSeconds(-1));

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            rows.Add(new ReportRow
                            {
                                Resident = r["Resident"].ToString(),
                                Address = r["Address"] == DBNull.Value ? "" : r["Address"].ToString(),
                                Allocated = Convert.ToDecimal(r["Allocated"]),
                                Status = r["Status"].ToString(),
                                Released = r["Released"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["Released"]),
                                ReleasedBy = r["ReleasedBy"] == DBNull.Value ? "" : r["ReleasedBy"].ToString()
                            });
                        }
                    }
                }
            }
            return rows;
        }

        // "Simple List" / "Detailed Report" templates — a running table,
        // paginated across as many pages as needed. Returns the index of the
        // next unprinted row so the caller can resume on the next page.
        private int PrintTablePage(PrintPageEventArgs e, List<ReportRow> rows, int startIndex, bool detailed)
        {
            var g = e.Graphics;
            int left = e.MarginBounds.Left;
            int width = e.MarginBounds.Width;
            var fontTitle = new Font("Segoe UI", 14f, FontStyle.Bold);
            var fontSub = new Font("Segoe UI", 9f);
            var fontHdr = new Font("Segoe UI", 9f, FontStyle.Bold);
            var fontRow = new Font("Segoe UI", 9f);
            int y = e.MarginBounds.Top;

            if (startIndex == 0)
            {
                g.DrawString("Barangay Ayuda Fund Distribution Report", fontTitle, Brushes.Black, left, y);
                y += 26;
                g.DrawString($"Fund: {_selectedFundName}   |   Fund Total: ₱{_selectedFundAmt:N2}", fontSub, Brushes.Black, left, y);
                y += 16;
                g.DrawString($"Generated: {DateTime.Now:MMM dd, yyyy hh:mm tt}   |   By: {CurrentUser.Username}", fontSub, Brushes.Black, left, y);
                y += 16;
                if (detailed)
                {
                    int totalRel = rows.Count(x => x.Status == "Released");
                    int totalPen = rows.Count(x => x.Status == "Pending");
                    decimal sumRel = rows.Where(x => x.Status == "Released").Sum(x => x.Allocated);
                    decimal sumPen = rows.Where(x => x.Status == "Pending").Sum(x => x.Allocated);
                    g.DrawString(
                        $"Beneficiaries: {rows.Count}   Released: {totalRel} (₱{sumRel:N2})   Pending: {totalPen} (₱{sumPen:N2})",
                        fontSub, Brushes.Black, left, y);
                    y += 18;
                }
                y += 10;
            }

            int[] colX = detailed
                ? new[] { left, left + 170, left + 300, left + 380, left + 470 }
                : new[] { left, left + 260, left + 380 };

            g.FillRectangle(new SolidBrush(Color.FromArgb(26, 35, 126)), left, y, width, 22);
            if (detailed)
            {
                g.DrawString("Resident", fontHdr, Brushes.White, colX[0] + 4, y + 4);
                g.DrawString("Address", fontHdr, Brushes.White, colX[1] + 4, y + 4);
                g.DrawString("Amount", fontHdr, Brushes.White, colX[2] + 4, y + 4);
                g.DrawString("Status", fontHdr, Brushes.White, colX[3] + 4, y + 4);
                g.DrawString("Released / By", fontHdr, Brushes.White, colX[4] + 4, y + 4);
            }
            else
            {
                g.DrawString("Resident", fontHdr, Brushes.White, colX[0] + 4, y + 4);
                g.DrawString("Status", fontHdr, Brushes.White, colX[1] + 4, y + 4);
                g.DrawString("Amount", fontHdr, Brushes.White, colX[2] + 4, y + 4);
            }
            y += 26;

            int i = startIndex;
            while (i < rows.Count)
            {
                if (y + 22 > e.MarginBounds.Bottom)
                {
                    e.HasMorePages = true;
                    return i;
                }
                var row = rows[i];
                var statusColor = row.Status == "Released" ? Color.FromArgb(15, 120, 60) : Color.FromArgb(180, 90, 0);
                using (var statusBrush = new SolidBrush(statusColor))
                {
                    if (detailed)
                    {
                        g.DrawString(row.Resident, fontRow, Brushes.Black, colX[0] + 4, y);
                        g.DrawString(row.Address, fontRow, Brushes.Black, colX[1] + 4, y);
                        g.DrawString($"₱{row.Allocated:N2}", fontRow, Brushes.Black, colX[2] + 4, y);
                        g.DrawString(row.Status, fontRow, statusBrush, colX[3] + 4, y);
                        string relInfo = row.Released.HasValue
                            ? $"{row.Released.Value:MMM dd, yyyy} — {row.ReleasedBy}"
                            : "—";
                        g.DrawString(relInfo, fontRow, Brushes.Black, colX[4] + 4, y);
                    }
                    else
                    {
                        g.DrawString(row.Resident, fontRow, Brushes.Black, colX[0] + 4, y);
                        g.DrawString(row.Status, fontRow, statusBrush, colX[1] + 4, y);
                        g.DrawString($"₱{row.Allocated:N2}", fontRow, Brushes.Black, colX[2] + 4, y);
                    }
                }
                g.DrawLine(Pens.LightGray, left, y + 20, left + width, y + 20);
                y += 22;
                i++;
            }

            e.HasMorePages = false;
            return i;
        }

        // "Individual Acknowledgment Receipts" template — one signable slip
        // per beneficiary, one per page.
        private int PrintReceiptPage(PrintPageEventArgs e, List<ReportRow> rows, int startIndex)
        {
            if (startIndex >= rows.Count)
            {
                e.HasMorePages = false;
                return startIndex;
            }

            var g = e.Graphics;
            var row = rows[startIndex];
            int left = e.MarginBounds.Left;
            int top = e.MarginBounds.Top;
            int width = e.MarginBounds.Width;
            var fontTitle = new Font("Segoe UI", 15f, FontStyle.Bold);
            var fontSubtitle = new Font("Segoe UI", 10f, FontStyle.Italic);
            var fontLbl = new Font("Segoe UI", 10f, FontStyle.Bold);
            var fontVal = new Font("Segoe UI", 10f);
            var fontSig = new Font("Segoe UI", 8f);

            using (var pen = new Pen(Color.Black, 1.5f))
                g.DrawRectangle(pen, left, top, width, 260);

            g.DrawString("BARANGAY AYUDA DISTRIBUTION", fontTitle, Brushes.Black, left + 20, top + 16);
            g.DrawString("Acknowledgment Receipt", fontSubtitle, Brushes.Gray, left + 20, top + 44);
            g.DrawLine(Pens.Black, left + 20, top + 68, left + width - 20, top + 68);

            int ly = top + 84;
            void Line(string label, string val)
            {
                g.DrawString(label, fontLbl, Brushes.Black, left + 20, ly);
                g.DrawString(val, fontVal, Brushes.Black, left + 160, ly);
                ly += 22;
            }
            Line("Fund:", _selectedFundName);
            Line("Resident:", row.Resident);
            Line("Address:", string.IsNullOrEmpty(row.Address) ? "—" : row.Address);
            Line("Amount:", $"₱{row.Allocated:N2}");
            Line("Status:", row.Status);
            Line("Date Released:", row.Released.HasValue ? row.Released.Value.ToString("MMM dd, yyyy") : "Not yet released");
            Line("Released By:", string.IsNullOrEmpty(row.ReleasedBy) ? "—" : row.ReleasedBy);

            int sigY = top + 224;
            g.DrawLine(Pens.Black, left + 20, sigY, left + 240, sigY);
            g.DrawString("Signature over Printed Name", fontSig, Brushes.Gray, left + 20, sigY + 4);

            e.HasMorePages = startIndex + 1 < rows.Count;
            return startIndex + 1;
        }

        // Builds the PrintDocument for the chosen filters/template and either
        // shows a Print Preview window or sends it straight to a printer.
        private void RunFundReportPrint(string statusFilter, DateTime? from, DateTime? to, string template, bool preview)
        {
            var rows = GetReportRows(statusFilter, from, to);
            if (rows.Count == 0)
            {
                MessageBox.Show("No beneficiaries match the selected filters.", "Nothing to Print",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int cursor = 0;
            var doc = new PrintDocument { DocumentName = $"{_selectedFundName} - {template}" };
            doc.BeginPrint += (s, e) => cursor = 0;
            doc.PrintPage += (s, e) =>
            {
                cursor = template == "Individual Acknowledgment Receipts"
                    ? PrintReceiptPage(e, rows, cursor)
                    : PrintTablePage(e, rows, cursor, template == "Detailed Report");
            };

            if (preview)
            {
                using (var pv = new PrintPreviewDialog
                {
                    Document = doc,
                    Width = 950,
                    Height = 750,
                    StartPosition = FormStartPosition.CenterParent
                })
                {
                    pv.ShowDialog();
                }
            }
            else
            {
                using (var pd = new PrintDialog { Document = doc })
                {
                    if (pd.ShowDialog() == DialogResult.OK)
                        doc.Print();
                }
            }
        }

        // Filter + template picker shown before printing, so the user can
        // choose what date range / status to include and which layout
        // ("Simple List", "Detailed Report", or "Individual Acknowledgment
        // Receipts") to print or preview.
        private void ShowPrintReportDialog()
        {
            if (_selectedFundId == 0)
            {
                MessageBox.Show("Select a fund first.", "No Fund Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dlg = MakeDialog("Print Fund Report", $"Fund: {_selectedFundName}", new Size(420, 460));
            var inner = (Panel)dlg.Tag;

            inner.Controls.Add(FL("Beneficiary Status", 0, 0));
            string statusChoice = "All";
            var pAll = Pill("All", 0, 22);
            var pPending = Pill("Pending", 0, 22);
            var pReleased = Pill("Released", 0, 22);
            inner.Controls.Add(pAll);
            inner.Controls.Add(pPending);
            inner.Controls.Add(pReleased);
            int px = 0;
            pAll.Location = new Point(px, 22); px += pAll.Width + 6;
            pPending.Location = new Point(px, 22); px += pPending.Width + 6;
            pReleased.Location = new Point(px, 22);

            void SetStatusPill(PrettyButton active)
            {
                foreach (var p in new[] { pAll, pPending, pReleased })
                {
                    p.BaseColor = p == active ? BtnTeal : PillInactive;
                    p.ForeColor = p == active ? Color.White : NavBlue;
                    p.Invalidate();
                }
            }
            SetStatusPill(pAll);
            pAll.Click += (s, e) => { statusChoice = "All"; SetStatusPill(pAll); };
            pPending.Click += (s, e) => { statusChoice = "Pending"; SetStatusPill(pPending); };
            pReleased.Click += (s, e) => { statusChoice = "Released"; SetStatusPill(pReleased); };

            inner.Controls.Add(FL("Date Released From", 0, 62));
            var dtFrom = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Checked = false,
                Location = new Point(0, 84),
                Width = 180
            };
            inner.Controls.Add(dtFrom);

            inner.Controls.Add(FL("Date Released To", 200, 62));
            var dtTo = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Checked = false,
                Location = new Point(200, 84),
                Width = 180
            };
            inner.Controls.Add(dtTo);

            inner.Controls.Add(FL("Print Template", 0, 124));
            var cboTemplate = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(0, 146),
                Width = 380,
                Font = new Font("Segoe UI", 9.5f)
            };
            cboTemplate.Items.AddRange(new object[]
            {
                "Simple List",
                "Detailed Report",
                "Individual Acknowledgment Receipts"
            });
            cboTemplate.SelectedIndex = 0;
            inner.Controls.Add(cboTemplate);

            var lblHint = new Label
            {
                Text = "Leave a date unchecked to include all records regardless of release date.\nDate filters only affect beneficiaries that already have a release date.",
                Font = new Font("Segoe UI", 8f, FontStyle.Italic),
                ForeColor = TextGray,
                AutoSize = true,
                Location = new Point(0, 182)
            };
            inner.Controls.Add(lblHint);

            var btnPreview = MakeBtn("👁  Preview", BtnBlue, 9.5f);
            btnPreview.Size = new Size(120, 34);
            btnPreview.Location = new Point(0, 234);
            inner.Controls.Add(btnPreview);

            var btnPrintNow = MakeBtn("🖨  Print", BtnGreen, 9.5f);
            btnPrintNow.Size = new Size(120, 34);
            btnPrintNow.Location = new Point(130, 234);
            inner.Controls.Add(btnPrintNow);

            var btnCancel = SmallBtn("Cancel", BtnGray);
            btnCancel.Size = new Size(100, 34);
            btnCancel.Location = new Point(260, 234);
            btnCancel.Click += (s, e) => dlg.Close();
            inner.Controls.Add(btnCancel);

            btnPreview.Click += (s, e) =>
            {
                RunFundReportPrint(statusChoice,
                    dtFrom.Checked ? dtFrom.Value : (DateTime?)null,
                    dtTo.Checked ? dtTo.Value : (DateTime?)null,
                    cboTemplate.SelectedItem.ToString(), true);
            };
            btnPrintNow.Click += (s, e) =>
            {
                RunFundReportPrint(statusChoice,
                    dtFrom.Checked ? dtFrom.Value : (DateTime?)null,
                    dtTo.Checked ? dtTo.Value : (DateTime?)null,
                    cboTemplate.SelectedItem.ToString(), false);
            };

            dlg.ShowDialog();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  TRANSACTIONS LOADER
        // ══════════════════════════════════════════════════════════════════════
        private void LoadTransactions(DataGridView dgv)
        {
            var conn = DatabaseConnection.GetConnection(); if (conn == null) return;
            using (conn)
            {
                try
                {
                    const string q = @"
                        SELECT idTransaction AS FundID,
                               TransactionDate AS Date,
                               Category,
                               Description,
                               TransactionType AS Type,
                               Amount,
                               Source
                        FROM budget_transactions
                        ORDER BY TransactionDate DESC, idTransaction DESC
                        LIMIT 80";
                    var da = new SQLiteDataAdapter(q, conn);
                    var dt = new DataTable(); da.Fill(dt);
                    dgv.DataSource = dt;

                    ColVis(dgv, "FundID", false);
                    ColFmt(dgv, "Date", 115, "MMM dd, yyyy");
                    ColW(dgv, "Category", 160);
                    ColFill(dgv, "Description");
                    ColW(dgv, "Type", 90);
                    if (dgv.Columns.Contains("Amount"))
                    {
                        dgv.Columns["Amount"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                        dgv.Columns["Amount"].DefaultCellStyle.Format = "₱#,##0.00";
                        dgv.Columns["Amount"].Width = 130;
                        dgv.Columns["Amount"].DefaultCellStyle.Alignment =
                            DataGridViewContentAlignment.MiddleRight;
                    }
                    ColW(dgv, "Source", 140);

                    dgv.CellFormatting -= TxTypeColorFormat;
                    dgv.CellFormatting += TxTypeColorFormat;

                    // No more per-row "Beneficiaries" button column — clicking
                    // anywhere on an income row opens the fund-info popup
                    // instead (see TxGrid_RowClick).
                    dgv.CellClick -= TxGrid_RowClick;
                    dgv.CellClick += TxGrid_RowClick;
                }
                catch (Exception ex) { MessageBox.Show("Error loading transactions:\n" + ex.Message); }
            }
        }

        // Colors the Type column (Income = green, Expense = red).
        private void TxTypeColorFormat(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var dgv = sender as DataGridView;
            if (dgv == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgv.Columns[e.ColumnIndex].Name != "Type" || e.Value == null) return;
            e.CellStyle.ForeColor = e.Value.ToString() == "Income" ? ReleasedGreen : BtnRed;
            e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        }

        // Clicking anywhere on an income/fund row opens the fund-info popup,
        // which then offers a "View Beneficiaries" action. Expense rows
        // aren't funds and don't respond to clicks.
        private void TxGrid_RowClick(object sender, DataGridViewCellEventArgs e)
        {
            var dgv = sender as DataGridView;
            if (dgv == null || e.RowIndex < 0) return;
            var row = dgv.Rows[e.RowIndex];
            if (row.Cells["Type"].Value?.ToString() != "Income") return;

            int fundId = Convert.ToInt32(row.Cells["FundID"].Value);
            string fundName = row.Cells["Category"].Value?.ToString() ?? "";
            DateTime date = row.Cells["Date"].Value != null ? Convert.ToDateTime(row.Cells["Date"].Value) : DateTime.Now;
            decimal amount = 0;
            try { amount = Convert.ToDecimal(row.Cells["Amount"].Value); } catch { }
            string desc = row.Cells["Description"].Value?.ToString() ?? "";

            ShowFundInfoDialog(fundId, fundName, date, amount, desc, false);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  SUMMARY CARDS
        // ══════════════════════════════════════════════════════════════════════
        private void LoadSummaryCards()
        {
            var conn = DatabaseConnection.GetConnection(); if (conn == null) return;
            using (conn)
            {
                try
                {
                    decimal income = Convert.ToDecimal(new SQLiteCommand(
                        "SELECT IFNULL(SUM(Amount),0) FROM budget_transactions WHERE TransactionType='Income'",
                        conn).ExecuteScalar());
                    decimal expense = Convert.ToDecimal(new SQLiteCommand(
                        "SELECT IFNULL(SUM(Amount),0) FROM budget_transactions WHERE TransactionType='Expense'",
                        conn).ExecuteScalar());
                    decimal balance = income - expense;

                    if (_lblIncome != null) _lblIncome.Text = "₱" + income.ToString("N2");
                    if (_lblExpense != null) _lblExpense.Text = "₱" + expense.ToString("N2");
                    if (_lblBalance != null)
                    {
                        _lblBalance.Text = "₱" + balance.ToString("N2");
                        _lblBalance.ForeColor = balance >= 0 ? ReleasedGreen : BtnRed;
                    }

                    if (_btnFundsList != null)
                    {
                        int fundsWithBenef = Convert.ToInt32(new SQLiteCommand(
                            "SELECT COUNT(DISTINCT FundID) FROM fund_beneficiaries", conn).ExecuteScalar());
                        _btnFundsList.Text = fundsWithBenef > 0
                            ? $"👥  Funds & Beneficiaries ({fundsWithBenef})"
                            : "👥  Funds & Beneficiaries";
                    }
                }
                catch { }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  ADD FUND DIALOG  (kept for potential reuse; a fund/income row can
        //  also be created via "+ Add Income")
        // ══════════════════════════════════════════════════════════════════════
        private void ShowAddFundDialog(Action onSuccess)
        {
            var dlg = MakeDialog("Add New Fund", "Record an incoming fund / budget source", new Size(480, 420));
            var body = (Panel)dlg.Tag;
            const int fw = 400; int y = 0;

            body.Controls.Add(FL("Date:", 0, y));
            var dtpDate = new DateTimePicker
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(0, y + 20),
                Size = new Size(fw, 28),
                Value = DateTime.Now
            };
            body.Controls.Add(dtpDate); y += 56;

            body.Controls.Add(FL("Category / Fund Name:", 0, y));
            var cbo = new ComboBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(0, y + 20),
                Size = new Size(fw, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbo.Items.AddRange(new object[] {
                "IRA (Internal Revenue Allotment)", "Local Tax Collection",
                "Business Permits", "Fines and Penalties",
                "Grants and Donations", "Financial Assistance",
                "Medical Assistance", "Disaster Relief Fund", "Others"
            });
            body.Controls.Add(cbo); y += 56;

            body.Controls.Add(FL("Total Amount (₱):", 0, y));
            var numAmt = new NumericUpDown
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(0, y + 20),
                Size = new Size(fw, 28),
                DecimalPlaces = 2,
                Maximum = 99999999,
                ThousandsSeparator = true
            };
            body.Controls.Add(numAmt); y += 56;

            body.Controls.Add(FL("Description / Remarks:", 0, y));
            var txtDesc = new TextBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(0, y + 20),
                Size = new Size(fw, 28),
                BorderStyle = BorderStyle.FixedSingle
            };
            body.Controls.Add(txtDesc); y += 56;

            var btnSave = MakeBtn("💾  Save Fund", BtnGreen, 10f);
            btnSave.Location = new Point(0, y); btnSave.Size = new Size(180, 38);
            body.Controls.Add(btnSave);
            var btnCx = MakeBtn("Cancel", BtnGray, 10f);
            btnCx.Location = new Point(190, y); btnCx.Size = new Size(100, 38);
            btnCx.Click += (s, e) => dlg.Close();
            body.Controls.Add(btnCx);

            btnSave.Click += (s, e) =>
            {
                if (cbo.SelectedIndex == -1)
                { MessageBox.Show("Select a category.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (numAmt.Value <= 0)
                { MessageBox.Show("Enter an amount.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                var conn = DatabaseConnection.GetConnection(); if (conn == null) return;
                using (conn)
                {
                    try
                    {
                        var cmd = new SQLiteCommand(
                            "INSERT INTO budget_transactions (TransactionDate,Category,Description,TransactionType,Amount,Source,ReferenceID,ReferenceType) " +
                            "VALUES(@d,@cat,@desc,'Income',@amt,@src,0,'Fund')", conn);
                        cmd.Parameters.AddWithValue("@d", dtpDate.Value.Date);
                        cmd.Parameters.AddWithValue("@cat", cbo.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@desc", txtDesc.Text.Trim());
                        cmd.Parameters.AddWithValue("@amt", numAmt.Value);
                        cmd.Parameters.AddWithValue("@src", CurrentUser.Username);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Fund added!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dlg.DialogResult = DialogResult.OK;
                        dlg.Close();
                        onSuccess?.Invoke();
                    }
                    catch (Exception ex) { MessageBox.Show("Error:\n" + ex.Message); }
                }
            };
            dlg.ShowDialog();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  ADD BENEFICIARY DIALOG
        // ══════════════════════════════════════════════════════════════════════
        private void ShowAddBeneficiaryDialog(int residentId, string residentName, Action onSuccess)
        {
            var conn0 = DatabaseConnection.GetConnection(); if (conn0 == null) return;
            decimal remainingBalance = 0;
            using (conn0)
            {
                var chk = new SQLiteCommand(
                    "SELECT COUNT(*) FROM fund_beneficiaries WHERE FundID=@f AND ResidentID=@r", conn0);
                chk.Parameters.AddWithValue("@f", _selectedFundId);
                chk.Parameters.AddWithValue("@r", residentId);
                if (Convert.ToInt32(chk.ExecuteScalar()) > 0)
                {
                    MessageBox.Show($"{residentName} is already a beneficiary of this fund.",
                        "Already Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var cmdBal = new SQLiteCommand(
                    "SELECT IFNULL(SUM(AllocatedAmount),0) FROM fund_beneficiaries WHERE FundID=@f", conn0);
                cmdBal.Parameters.AddWithValue("@f", _selectedFundId);
                decimal allocated = Convert.ToDecimal(cmdBal.ExecuteScalar());
                remainingBalance = _selectedFundAmt - allocated;
            }

            if (remainingBalance <= 0)
            {
                MessageBox.Show(
                    $"The fund has no remaining balance to allocate.\nFund Total: ₱{_selectedFundAmt:N2}",
                    "Insufficient Balance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dlg = MakeDialog("Add Beneficiary", $"Fund: {_selectedFundName}", new Size(440, 320));
            var body = (Panel)dlg.Tag;

            body.Controls.Add(new Label
            {
                Text = "Resident:",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = TextGray,
                AutoSize = true,
                Location = new Point(0, 0),
                BackColor = Color.Transparent
            });
            body.Controls.Add(new Label
            {
                Text = residentName,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(0, 18),
                BackColor = Color.Transparent
            });

            body.Controls.Add(FL("Allocated Amount (₱):", 0, 52));
            var numAlloc = new NumericUpDown
            {
                Font = new Font("Segoe UI", 11f),
                Location = new Point(0, 72),
                Size = new Size(400, 32),
                DecimalPlaces = 2,
                Maximum = remainingBalance,
                ThousandsSeparator = true,
                Value = Math.Round(remainingBalance, 2)
            };
            body.Controls.Add(numAlloc);

            body.Controls.Add(new Label
            {
                Text = $"Fund total: ₱{_selectedFundAmt:N2}   |   Remaining: ₱{remainingBalance:N2}",
                Font = new Font("Segoe UI", 9f),
                ForeColor = remainingBalance > 0 ? ReleasedGreen : BtnRed,
                AutoSize = true,
                Location = new Point(0, 112),
                BackColor = Color.Transparent
            });

            var btnAdd = MakeBtn("✔  Add as Beneficiary", BtnGreen, 10f);
            btnAdd.Location = new Point(0, 140); btnAdd.Size = new Size(210, 36);
            body.Controls.Add(btnAdd);
            var btnCx = MakeBtn("Cancel", BtnGray, 10f);
            btnCx.Location = new Point(220, 140); btnCx.Size = new Size(90, 36);
            btnCx.Click += (s, e) => dlg.Close();
            body.Controls.Add(btnCx);

            btnAdd.Click += (s, e) =>
            {
                if (numAlloc.Value <= 0)
                { MessageBox.Show("Enter an allocation amount.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (numAlloc.Value > remainingBalance)
                { MessageBox.Show($"Amount exceeds remaining balance of ₱{remainingBalance:N2}.", "Exceeds Balance", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                var conn = DatabaseConnection.GetConnection(); if (conn == null) return;
                using (conn)
                {
                    try
                    {
                        var cmd = new SQLiteCommand(
                            "INSERT INTO fund_beneficiaries (FundID,ResidentID,AllocatedAmount,IsReleased,AddedBy,DateAdded) " +
                            "VALUES(@fid,@rid,@amt,0,@by,datetime('now'))", conn);
                        cmd.Parameters.AddWithValue("@fid", _selectedFundId);
                        cmd.Parameters.AddWithValue("@rid", residentId);
                        cmd.Parameters.AddWithValue("@amt", numAlloc.Value);
                        cmd.Parameters.AddWithValue("@by", CurrentUser.Username);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show($"{residentName} added as beneficiary!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dlg.DialogResult = DialogResult.OK;
                        dlg.Close();
                        onSuccess?.Invoke();
                    }
                    catch (Exception ex) { MessageBox.Show("Error:\n" + ex.Message); }
                }
            };
            dlg.ShowDialog();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  EDIT ALLOCATED AMOUNT DIALOG  — reached from the "⋮" actions menu
        //  on any of the three Distribution beneficiary lists. The max value
        //  is capped at the fund's remaining balance PLUS this beneficiary's
        //  own current allocation (so they can raise their own amount back
        //  up to whatever headroom the fund has, without double-counting
        //  their existing share as "already spent"). Not offered once the
        //  beneficiary has already been released.
        // ══════════════════════════════════════════════════════════════════════
        private void ShowEditAllocationDialog(int benefId, string residentName, decimal currentAlloc, bool isReleased, Action onSuccess)
        {
            var conn0 = DatabaseConnection.GetConnection(); if (conn0 == null) return;
            decimal maxAllowed;
            using (conn0)
            {
                var cmdBal = new SQLiteCommand(
                    "SELECT IFNULL(SUM(AllocatedAmount),0) FROM fund_beneficiaries WHERE FundID=@f AND idBeneficiary<>@id",
                    conn0);
                cmdBal.Parameters.AddWithValue("@f", _selectedFundId);
                cmdBal.Parameters.AddWithValue("@id", benefId);
                decimal allocatedByOthers = Convert.ToDecimal(cmdBal.ExecuteScalar());
                maxAllowed = _selectedFundAmt - allocatedByOthers;
            }

            if (maxAllowed <= 0)
            {
                MessageBox.Show("No remaining balance available for this beneficiary.",
                    "Insufficient Balance", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Extra vertical room for the "already released" warning banner —
            // editing a released row is allowed (it's a record correction,
            // e.g. fixing a clerical typo), but it does NOT touch the ayuda
            // payout or the expense transaction already logged for it, so the
            // person needs that spelled out before they save.
            int warnOffset = isReleased ? 44 : 0;
            var dlg = MakeDialog("Edit Allocated Amount", $"Fund: {_selectedFundName}", new Size(440, 260 + warnOffset));
            var body = (Panel)dlg.Tag;

            body.Controls.Add(new Label
            {
                Text = "Resident:",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = TextGray,
                AutoSize = true,
                Location = new Point(0, 0),
                BackColor = Color.Transparent
            });
            body.Controls.Add(new Label
            {
                Text = residentName,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(0, 18),
                BackColor = Color.Transparent
            });

            if (isReleased)
            {
                var warnPanel = new Panel
                {
                    Location = new Point(0, 44),
                    Size = new Size(400, 36),
                    BackColor = Color.FromArgb(255, 244, 227)
                };
                warnPanel.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var path = RoundedPath(new Rectangle(0, 0, warnPanel.Width - 1, warnPanel.Height - 1), 6))
                    using (var brush = new SolidBrush(Color.FromArgb(255, 244, 227)))
                    using (var pen = new Pen(Color.FromArgb(240, 200, 140)))
                    {
                        e.Graphics.FillPath(brush, path);
                        e.Graphics.DrawPath(pen, path);
                    }
                };
                warnPanel.Controls.Add(new Label
                {
                    Text = "⚠  Already released — this corrects the record only.\nIt will NOT reverse or reissue the ₱ already paid out.",
                    Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                    ForeColor = BtnAmber,
                    AutoSize = false,
                    Size = new Size(384, 32),
                    Location = new Point(8, 3),
                    BackColor = Color.Transparent
                });
                body.Controls.Add(warnPanel);
            }

            body.Controls.Add(FL("New Allocated Amount (₱):", 0, 52 + warnOffset));
            var numAlloc = new NumericUpDown
            {
                Font = new Font("Segoe UI", 11f),
                Location = new Point(0, 72 + warnOffset),
                Size = new Size(400, 32),
                DecimalPlaces = 2,
                Maximum = maxAllowed,
                ThousandsSeparator = true,
                Value = Math.Min(currentAlloc, maxAllowed)
            };
            body.Controls.Add(numAlloc);

            body.Controls.Add(new Label
            {
                Text = $"Fund total: ₱{_selectedFundAmt:N2}   |   Max for this beneficiary: ₱{maxAllowed:N2}",
                Font = new Font("Segoe UI", 9f),
                ForeColor = TextGray,
                AutoSize = true,
                Location = new Point(0, 112 + warnOffset),
                BackColor = Color.Transparent
            });

            var btnSave = MakeBtn("💾  Save Amount", BtnGreen, 10f);
            btnSave.Location = new Point(0, 148 + warnOffset); btnSave.Size = new Size(180, 36);
            body.Controls.Add(btnSave);
            var btnCx = MakeBtn("Cancel", BtnGray, 10f);
            btnCx.Location = new Point(190, 148 + warnOffset); btnCx.Size = new Size(100, 36);
            btnCx.Click += (s, e) => dlg.Close();
            body.Controls.Add(btnCx);

            btnSave.Click += (s, e) =>
            {
                if (numAlloc.Value <= 0)
                { MessageBox.Show("Enter an allocation amount.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

                if (isReleased && MessageBox.Show(
                    $"This beneficiary was already released. Save ₱{numAlloc.Value:N2} as a record correction?\n\nThe original ₱{currentAlloc:N2} payout already made will NOT change.",
                    "Confirm Correction", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

                var conn = DatabaseConnection.GetConnection(); if (conn == null) return;
                using (conn)
                {
                    try
                    {
                        var cmd = new SQLiteCommand(
                            "UPDATE fund_beneficiaries SET AllocatedAmount=@amt WHERE idBeneficiary=@id", conn);
                        cmd.Parameters.AddWithValue("@amt", numAlloc.Value);
                        cmd.Parameters.AddWithValue("@id", benefId);
                        cmd.ExecuteNonQuery();
                        AuditLogger.Log("EDIT_ALLOCATION", "Ayuda",
                            isReleased
                                ? $"Corrected allocation for {residentName} on fund {_selectedFundName} to ₱{numAlloc.Value:N2} (post-release correction — payout unchanged)"
                                : $"Changed allocation for {residentName} on fund {_selectedFundName} to ₱{numAlloc.Value:N2}");
                        dlg.DialogResult = DialogResult.OK;
                        dlg.Close();
                        onSuccess?.Invoke();
                    }
                    catch (Exception ex) { MessageBox.Show("Error:\n" + ex.Message); }
                }
            };
            dlg.ShowDialog();
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  HOW TO USE THIS FILE
        //
        //  This is a drop-in REPLACEMENT for the existing method:
        //
        //      private void ShowResidentProfilePopup(int residentId, Action onChanged)
        //
        //  inside BudgetTransparencyControl.cs. Delete the old method body (the one
        //  currently in your file) and paste this one in its place. It uses the
        //  same helpers already in your class (RoundedPath, MakeBtn, PrettyButton,
        //  NavBlue, GreenAccent, etc.) so nothing else needs to change.
        //
        //  NEW FIELDS (Contact No., Voter Status, 4Ps Beneficiary, Household Head,
        //  Emergency Contact) are read in a SCHEMA-TOLERANT way, same idea as your
        //  existing photo lookup: it tries a list of likely column names and falls
        //  back to "—" if none of them exist on your `resident` table. Nothing will
        //  break if the columns aren't there yet — those fields will just show "—".
        //
        //  Once you tell me your actual column names, I can hard-wire the exact
        //  ones instead of guessing, and add proper Yes/No formatting for the
        //  Voter Status / 4Ps columns if they're stored as 0/1 or as text.
        // ══════════════════════════════════════════════════════════════════════════

        private void ShowResidentProfilePopup(int residentId, Action onChanged)
        {
            string fullName = "", address = "", age = "—", gender = "—";
            string contactNo = "—", voterStatus = "—", fourPsStatus = "—",
                   householdHead = "—", emergencyContact = "—";
            bool active = false;
            bool isRegisteredVoter = false;
            bool is4PsBeneficiary = false;
            Image photo = null;

            var conn = DatabaseConnection.GetConnection(); if (conn == null) return;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand("SELECT * FROM resident WHERE idResident=@id", conn);
                    cmd.Parameters.AddWithValue("@id", residentId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                        {
                            MessageBox.Show($"No resident found with ID #{residentId}.", "Not Found",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        string GetStr(string col)
                        {
                            try
                            {
                                int i = r.GetOrdinal(col);
                                return r.IsDBNull(i) ? "" : Convert.ToString(r.GetValue(i));
                            }
                            catch { return ""; }
                        }

                        // Tries each candidate column name in order, returns the
                        // first non-empty value found. Lets this screen work even
                        // if your resident table uses a different naming scheme.
                        string GetAny(params string[] cols)
                        {
                            foreach (var c in cols)
                            {
                                var v = GetStr(c);
                                if (!string.IsNullOrWhiteSpace(v)) return v;
                            }
                            return "";
                        }

                        bool IsTrueLike(string v) =>
                            v == "1" || v.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                            v.Equals("yes", StringComparison.OrdinalIgnoreCase);
                        bool IsFalseLike(string v) =>
                            v == "0" || v.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                            v.Equals("no", StringComparison.OrdinalIgnoreCase);

                        string last = GetStr("LastName");
                        string first = GetStr("FirstName");
                        string middle = GetStr("MiddleName");
                        fullName = $"{last}, {first}" + (string.IsNullOrEmpty(middle) ? "" : $" {middle}");
                        address = GetStr("Address");
                        string g0 = GetStr("Gender");
                        gender = string.IsNullOrEmpty(g0) ? "—" : g0;

                        string dobStr = GetStr("DateOfBirth");
                        if (DateTime.TryParse(dobStr, out var dob))
                            age = ((int)((DateTime.Now - dob).TotalDays / 365.25)).ToString();

                        string activeStr = GetStr("IsActive");
                        active = activeStr == "1" || activeStr.Equals("true", StringComparison.OrdinalIgnoreCase);

                        // ── New fields (schema-tolerant) ──────────────────────────
                        contactNo = GetAny("ContactNo", "ContactNumber", "PhoneNumber",
                            "MobileNumber", "Contact", "CellphoneNumber");
                        if (string.IsNullOrWhiteSpace(contactNo)) contactNo = "—";

                        string voterRaw = GetAny("VoterStatus", "IsRegisteredVoter", "IsVoter", "VoterRegistration");
                        if (IsTrueLike(voterRaw)) { voterStatus = "✔ Registered Voter"; isRegisteredVoter = true; }
                        else if (IsFalseLike(voterRaw)) { voterStatus = "Not Registered"; }
                        else if (!string.IsNullOrWhiteSpace(voterRaw)) { voterStatus = voterRaw; }
                        else { voterStatus = "—"; }

                        string fourPsRaw = GetAny("Is4PsBeneficiary", "FourPsBeneficiary", "Is4Ps", "FourPs");
                        string householdNo = GetAny("HouseholdID", "HouseholdNo", "HouseholdNumber");
                        if (IsTrueLike(fourPsRaw))
                        {
                            is4PsBeneficiary = true;
                            fourPsStatus = string.IsNullOrWhiteSpace(householdNo)
                                ? "Yes"
                                : $"Yes — Household #{householdNo}";
                        }
                        else if (IsFalseLike(fourPsRaw)) { fourPsStatus = "No"; }
                        else if (!string.IsNullOrWhiteSpace(fourPsRaw)) { fourPsStatus = fourPsRaw; }
                        else { fourPsStatus = "—"; }

                        householdHead = GetAny("HouseholdHead", "HeadOfHousehold", "FamilyHead");
                        if (string.IsNullOrWhiteSpace(householdHead)) householdHead = "—";

                        emergencyContact = GetAny("EmergencyContact", "EmergencyContactName",
                            "EmergencyContactNumber", "EmergencyContactNo");
                        if (string.IsNullOrWhiteSpace(emergencyContact)) emergencyContact = "—";

                        // Schema-tolerant photo lookup (unchanged from your original).
                        for (int i = 0; i < r.FieldCount; i++)
                        {
                            string lc = r.GetName(i).ToLowerInvariant();
                            if (!(lc.Contains("photo") || lc.Contains("picture") || lc.Contains("image"))) continue;
                            if (r.IsDBNull(i)) continue;
                            try
                            {
                                var val = r.GetValue(i);
                                if (val is byte[] bytes && bytes.Length > 0)
                                {
                                    using (var ms = new System.IO.MemoryStream(bytes))
                                        photo = Image.FromStream(ms);
                                }
                                else
                                {
                                    string path = Convert.ToString(val);
                                    if (!string.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path))
                                        photo = Image.FromFile(path);
                                }
                            }
                            catch { /* unreadable image data — fall back to initials avatar */ }
                            if (photo != null) break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading resident:\n" + ex.Message);
                    return;
                }
            }

            // ── Fund legitimacy check (unchanged) ───────────────────────────────
            bool isBeneficiary = false, released = false;
            decimal alloc = 0; int matchedBid = 0;

            if (_selectedFundId != 0)
            {
                var conn2 = DatabaseConnection.GetConnection();
                if (conn2 != null)
                {
                    using (conn2)
                    {
                        var cmd2 = new SQLiteCommand(
                            "SELECT idBeneficiary, AllocatedAmount, IsReleased FROM fund_beneficiaries WHERE FundID=@fid AND ResidentID=@rid", conn2);
                        cmd2.Parameters.AddWithValue("@fid", _selectedFundId);
                        cmd2.Parameters.AddWithValue("@rid", residentId);
                        using (var r2 = cmd2.ExecuteReader())
                        {
                            if (r2.Read())
                            {
                                isBeneficiary = true;
                                alloc = Convert.ToDecimal(r2["AllocatedAmount"]);
                                released = r2["IsReleased"] != DBNull.Value && Convert.ToInt32(r2["IsReleased"]) == 1;
                                matchedBid = Convert.ToInt32(r2["idBeneficiary"]);
                            }
                        }
                    }
                }
            }

            // ══════════════════════════════════════════════════════════════════
            //  BUILD THE POPUP — "Resident Match Found" card layout
            // ══════════════════════════════════════════════════════════════════
            Color HeaderGreen = Color.FromArgb(39, 174, 96);
            Color PillGreenBg = Color.FromArgb(230, 246, 237);
            Color FieldLabelGray = Color.FromArgb(140, 146, 168);

            int dialogW = 460;
            var dlg = new Form
            {
                Text = "Resident Match Found",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ControlBox = false,
                ClientSize = new Size(dialogW, 40),   // grown at the end
                BackColor = Color.FromArgb(244, 246, 251),
                Font = new Font("Segoe UI", 9.5f)
            };

            // ── Header (green) ───────────────────────────────────────────────
            var hdr = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = HeaderGreen };
            hdr.Controls.Add(new Label
            {
                Text = "✔  Resident Match Found",
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(18, 14)
            });
            var lblClose = new Label
            {
                Text = "✕",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            lblClose.Location = new Point(dialogW - 34, 14);
            lblClose.Click += (s, e) => dlg.Close();
            hdr.Controls.Add(lblClose);
            dlg.Controls.Add(hdr);

            var body = new Panel { Location = new Point(0, 52), Size = new Size(dialogW, 10), BackColor = dlg.BackColor };
            dlg.Controls.Add(body);

            int y = 20;
            const int margin = 24;
            int cardW = dialogW - margin * 2;

            // ── Photo / avatar + name card ──────────────────────────────────
            var photoCard = new Panel { Location = new Point(margin, y), Size = new Size(cardW, 130), BackColor = Color.White };
            photoCard.Paint += (s, e) => PaintCard(e.Graphics, photoCard);
            body.Controls.Add(photoCard);

            const int photoSize = 76;
            var picBox = new PictureBox
            {
                Size = new Size(photoSize, photoSize),
                Location = new Point(20, (photoCard.Height - photoSize) / 2),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };
            using (var gp = new GraphicsPath())
            {
                gp.AddEllipse(0, 0, photoSize, photoSize);
                picBox.Region = new Region(gp);
            }
            picBox.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(GreenAccent, 3f))
                    e.Graphics.DrawEllipse(pen, 1, 1, photoSize - 3, photoSize - 3);
            };

            if (photo != null)
            {
                picBox.Image = photo;
            }
            else
            {
                string initials = string.Concat(
                    fullName.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                            .Where(w => w.Length > 0)
                            .Select(w => w[0]))
                    .ToUpperInvariant();
                if (initials.Length > 2) initials = initials.Substring(0, 2);
                if (initials.Length == 0) initials = "?";

                var bmp = new Bitmap(photoSize, photoSize);
                using (var g2 = Graphics.FromImage(bmp))
                {
                    g2.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var b = new SolidBrush(NavBlueLight))
                        g2.FillEllipse(b, 0, 0, photoSize, photoSize);
                    using (var f = new Font("Segoe UI", 22f, FontStyle.Bold))
                    using (var sb = new SolidBrush(Color.White))
                    {
                        var sz = g2.MeasureString(initials, f);
                        g2.DrawString(initials, f, sb, (photoSize - sz.Width) / 2, (photoSize - sz.Height) / 2);
                    }
                }
                picBox.Image = bmp;
            }
            photoCard.Controls.Add(picBox);

            int textX = picBox.Right + 16;
            photoCard.Controls.Add(new Label
            {
                Text = fullName,
                Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(textX, 28)
            });
            photoCard.Controls.Add(new Label
            {
                Text = $"Resident ID #{residentId:D6}  ·  Age {age}  ·  {gender}",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = TextGray,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(textX, 52)
            });

            var pillLbl = new Label
            {
                Text = active ? "✔ IDENTITY VERIFIED" : "⚠ MARKED INACTIVE",
                Font = new Font("Segoe UI", 7.8f, FontStyle.Bold),
                ForeColor = active ? ReleasedGreen : BtnRed,
                BackColor = active ? PillGreenBg : Color.FromArgb(252, 234, 232),
                AutoSize = true,
                Padding = new Padding(8, 3, 8, 3),
                Location = new Point(textX, 76)
            };
            photoCard.Controls.Add(pillLbl);

            y += photoCard.Height + 14;

            // ── Info grid card (Address/Contact, Voter/4Ps, Household/Emergency) ─
            var gridCard = new Panel { Location = new Point(margin, y), Size = new Size(cardW, 148), BackColor = Color.White };
            gridCard.Paint += (s, e) => PaintCard(e.Graphics, gridCard);
            body.Controls.Add(gridCard);

            int colLx = 18, colRx = cardW / 2 + 6;
            int colW = cardW / 2 - 30;

            void Field(int col, int row, string label, string value, Color valColor)
            {
                int fx = col == 0 ? colLx : colRx;
                int fy = 14 + row * 44;
                gridCard.Controls.Add(new Label
                {
                    Text = label.ToUpper(),
                    Font = new Font("Segoe UI", 7.3f, FontStyle.Bold),
                    ForeColor = FieldLabelGray,
                    AutoSize = false,
                    Size = new Size(colW, 14),
                    Location = new Point(fx, fy),
                    BackColor = Color.Transparent
                });
                gridCard.Controls.Add(new Label
                {
                    Text = value,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = valColor,
                    AutoSize = false,
                    Size = new Size(colW, 20),
                    Location = new Point(fx, fy + 16),
                    BackColor = Color.Transparent
                });
            }

            Field(0, 0, "Address", string.IsNullOrWhiteSpace(address) ? "—" : address, NavBlue);
            Field(1, 0, "Contact No.", contactNo, NavBlue);
            Field(0, 1, "Voter Status", voterStatus, isRegisteredVoter ? ReleasedGreen : NavBlue);
            Field(1, 1, "4Ps Beneficiary", fourPsStatus, is4PsBeneficiary ? BtnAmber : NavBlue);
            Field(0, 2, "Household Head", householdHead, NavBlue);
            Field(1, 2, "Emergency Contact", emergencyContact, NavBlue);

            y += gridCard.Height + 14;

            // ── Fund box ─────────────────────────────────────────────────────
            var fundCard = new Panel { Location = new Point(margin, y), Size = new Size(cardW, 76), BackColor = LightBlue };
            fundCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = RoundedPath(new Rectangle(0, 0, fundCard.Width - 1, fundCard.Height - 1), 8))
                using (var brush = new SolidBrush(LightBlue))
                using (var pen = new Pen(Color.FromArgb(210, 220, 245)))
                {
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                }
            };
            body.Controls.Add(fundCard);

            fundCard.Controls.Add(new Label
            {
                Text = "FUND",
                Font = new Font("Segoe UI", 7.3f, FontStyle.Bold),
                ForeColor = FieldLabelGray,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(14, 8)
            });
            fundCard.Controls.Add(new Label
            {
                Text = _selectedFundId == 0
                    ? "No fund selected"
                    : $"{_selectedFundName}   ·   ₱{alloc:N2}",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(14, 24)
            });

            string statusText; Color statusColor;
            if (_selectedFundId == 0) { statusText = "Select a fund from Overview to check legitimacy."; statusColor = TextGray; }
            else if (!isBeneficiary) { statusText = "Not yet a beneficiary of this fund."; statusColor = TextGray; }
            else if (released) { statusText = "✔  Ayuda already released"; statusColor = ReleasedGreen; }
            else { statusText = $"⏳  PENDING — ₱{alloc:N2} ready to release"; statusColor = PendingAmber; }

            fundCard.Controls.Add(new Label
            {
                Text = statusText,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = statusColor,
                AutoSize = false,
                Size = new Size(cardW - 28, 18),
                BackColor = Color.Transparent,
                Location = new Point(14, 48)
            });

            y += fundCard.Height + 18;

            // ── Row of secondary actions (Edit Amount / Remove) — only when
            // this resident is an active (not-yet-released) beneficiary of
            // the currently selected fund. Mirrors the "⋮" menu options on
            // the Distribution beneficiary lists so both entry points stay
            // in sync.
            if (_selectedFundId != 0 && isBeneficiary)
            {
                // Edit Amount is available even once released (treated as a
                // record correction — see ShowEditAllocationDialog's warning).
                // Remove stays hidden once released so the row keeps standing
                // as the paper trail for the payout.
                bool showRemove = !released;
                int halfW = showRemove ? (cardW - 10) / 2 : cardW;

                var btnEditAmt = MakeBtn("✏️  Edit Amount", BtnBlue, 9f);
                btnEditAmt.Size = new Size(halfW, 36);
                btnEditAmt.Location = new Point(margin, y);
                int bidCopyE = matchedBid;
                decimal allocCopyE = alloc;
                string nameCopyE = fullName;
                bool releasedCopyE = released;
                btnEditAmt.Click += (s, e) =>
                {
                    dlg.Close();
                    ShowEditAllocationDialog(bidCopyE, nameCopyE, allocCopyE, releasedCopyE, () =>
                    {
                        onChanged?.Invoke();
                        ShowResidentProfilePopup(residentId, onChanged);
                    });
                };
                body.Controls.Add(btnEditAmt);

                if (showRemove)
                {
                    var btnRemove = MakeBtn("🗑  Remove", BtnRed, 9f);
                    btnRemove.Size = new Size(halfW, 36);
                    btnRemove.Location = new Point(margin + halfW + 10, y);
                    int bidCopyR = matchedBid;
                    string nameCopyR = fullName;
                    btnRemove.Click += (s, e) =>
                    {
                        if (MessageBox.Show($"Remove {nameCopyR} from this fund?", "Confirm",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                        var conn3 = DatabaseConnection.GetConnection(); if (conn3 == null) return;
                        using (conn3)
                        {
                            try
                            {
                                var cmd3 = new SQLiteCommand("DELETE FROM fund_beneficiaries WHERE idBeneficiary=@id", conn3);
                                cmd3.Parameters.AddWithValue("@id", bidCopyR);
                                cmd3.ExecuteNonQuery();
                                AuditLogger.Log("REMOVE_BENEFICIARY", "Ayuda", $"Removed {nameCopyR} from fund {_selectedFundName}");
                                dlg.Close();
                                onChanged?.Invoke();
                            }
                            catch (Exception ex) { MessageBox.Show(ex.Message); }
                        }
                    };
                    body.Controls.Add(btnRemove);
                }
                y += 46;
            }

            // ── Contextual full-width action button ─────────────────────────
            PrettyButton btnAction = null;
            if (_selectedFundId != 0 && isBeneficiary && !released)
            {
                btnAction = MakeBtn("✓  Confirm Release", BtnTeal, 10.5f);
            }
            else if (_selectedFundId != 0 && !isBeneficiary)
            {
                btnAction = MakeBtn("+  Add as Beneficiary", BtnGreen, 10.5f);
            }

            if (btnAction != null)
            {
                btnAction.Size = new Size(cardW, 42);
                btnAction.Location = new Point(margin, y);
                body.Controls.Add(btnAction);
                y += btnAction.Height + 10;

                if (!isBeneficiary)
                {
                    btnAction.Click += (s, e) =>
                    {
                        dlg.Close();
                        ShowAddBeneficiaryDialog(residentId, fullName, () =>
                        {
                            onChanged?.Invoke();
                            ShowResidentProfilePopup(residentId, onChanged); // refresh in place
                        });
                    };
                }
                else
                {
                    int bidCopy = matchedBid;
                    decimal allocCopy = alloc;
                    string nameCopy = fullName;
                    btnAction.Click += (s, e) =>
                    {
                        if (MessageBox.Show(
                            $"Release ₱{allocCopy:N2} to {nameCopy}?\n\nFund: {_selectedFundName}\nReleased by: {CurrentUser.Username}",
                            "Confirm Release", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                        dlg.Close();
                        ReleaseBeneficiary(bidCopy, allocCopy, nameCopy, () => onChanged?.Invoke());
                    };
                }
            }

            var btnClose = MakeBtn("Close", BtnGray, 9.5f);
            btnClose.Size = new Size(cardW, 38);
            btnClose.Location = new Point(margin, y);
            btnClose.Click += (s, e) => dlg.Close();
            body.Controls.Add(btnClose);
            y += btnClose.Height + 20;

            body.Height = y;
            dlg.ClientSize = new Size(dialogW, 52 + y);

            dlg.ShowDialog();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  ADD TRANSACTION DIALOG
        // ══════════════════════════════════════════════════════════════════════
        private void ShowAddTransactionDialog(Action onSuccess)
        {
            var dlg = MakeDialog("Add Budget Transaction", "Record an income or expense transaction", new Size(500, 480));
            var body = (Panel)dlg.Tag;
            const int fw = 440; int y = 0;

            body.Controls.Add(FL("Date:", 0, y));
            var dtpDate = new DateTimePicker
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(0, y + 20),
                Size = new Size(fw, 28),
                Value = DateTime.Now
            };
            body.Controls.Add(dtpDate); y += 58;

            body.Controls.Add(FL("Type:", 0, y));
            var cboType = new ComboBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(0, y + 20),
                Size = new Size(fw, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboType.Items.AddRange(new object[] { "Income", "Expense" });
            cboType.SelectedIndex = 0;
            body.Controls.Add(cboType); y += 58;

            body.Controls.Add(FL("Category:", 0, y));
            var cboCat = new ComboBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(0, y + 20),
                Size = new Size(fw, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboCat.Items.AddRange(new object[] {
                "IRA (Internal Revenue Allotment)", "Local Tax Collection",
                "Business Permits", "Fines and Penalties",
                "Grants and Donations", "Financial Assistance",
                "Medical Assistance", "Infrastructure",
                "Salaries", "Office Supplies", "Others"
            });
            body.Controls.Add(cboCat); y += 58;

            body.Controls.Add(FL("Amount (₱):", 0, y));
            var numAmt = new NumericUpDown
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(0, y + 20),
                Size = new Size(fw, 28),
                DecimalPlaces = 2,
                Maximum = 99999999,
                ThousandsSeparator = true
            };
            body.Controls.Add(numAmt); y += 58;

            body.Controls.Add(FL("Description:", 0, y));
            var txtDesc = new TextBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(0, y + 20),
                Size = new Size(fw, 28),
                BorderStyle = BorderStyle.FixedSingle
            };
            body.Controls.Add(txtDesc); y += 58;

            var btnSave = MakeBtn("💾  Save", BtnGreen, 10.5f);
            btnSave.Location = new Point(0, y); btnSave.Size = new Size(160, 40);
            body.Controls.Add(btnSave);
            var btnCx = MakeBtn("Cancel", BtnGray, 10.5f);
            btnCx.Location = new Point(170, y); btnCx.Size = new Size(110, 40);
            btnCx.Click += (s, e) => dlg.Close();
            body.Controls.Add(btnCx);

            btnSave.Click += (s, e) =>
            {
                if (cboCat.SelectedIndex == -1)
                { MessageBox.Show("Select a category.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (numAmt.Value <= 0)
                { MessageBox.Show("Enter an amount.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                var conn = DatabaseConnection.GetConnection(); if (conn == null) return;
                using (conn)
                {
                    try
                    {
                        var cmd = new SQLiteCommand(
                            "INSERT INTO budget_transactions (TransactionDate,Category,Description,TransactionType,Amount,Source,ReferenceID,ReferenceType) " +
                            "VALUES(@d,@cat,@desc,@type,@amt,@src,0,'Manual')", conn);
                        cmd.Parameters.AddWithValue("@d", dtpDate.Value.Date);
                        cmd.Parameters.AddWithValue("@cat", cboCat.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@desc", txtDesc.Text.Trim());
                        cmd.Parameters.AddWithValue("@type", cboType.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@amt", numAmt.Value);
                        cmd.Parameters.AddWithValue("@src", CurrentUser.Username);
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Transaction saved!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dlg.DialogResult = DialogResult.OK;
                        dlg.Close();
                        onSuccess?.Invoke();
                    }
                    catch (Exception ex) { MessageBox.Show("Error:\n" + ex.Message); }
                }
            };
            dlg.ShowDialog();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  RELEASE BENEFICIARY
        // ══════════════════════════════════════════════════════════════════════
        private void ReleaseBeneficiary(int benefId, decimal amount,
                                        string residentName, Action onSuccess)
        {
            var conn = DatabaseConnection.GetConnection(); if (conn == null) return;
            using (conn)
            {
                var txn = conn.BeginTransaction();
                try
                {
                    var cmdGet = new SQLiteCommand(
                        "SELECT ResidentID FROM fund_beneficiaries WHERE idBeneficiary=@id",
                        conn, txn);
                    cmdGet.Parameters.AddWithValue("@id", benefId);
                    int residentId = Convert.ToInt32(cmdGet.ExecuteScalar());

                    var cmdAyuda = new SQLiteCommand(
                        "INSERT INTO ayuda (ResidentID,BarcodeID,Amount,DateReleased,Purpose,ReleasedBy,Status,Remarks) " +
                        "VALUES(@rid,@barcode,@amt,@date,@purpose,@by,'Released',@remarks)", conn, txn);
                    cmdAyuda.Parameters.AddWithValue("@rid", residentId);
                    cmdAyuda.Parameters.AddWithValue("@barcode", residentId.ToString());
                    cmdAyuda.Parameters.AddWithValue("@amt", amount);
                    cmdAyuda.Parameters.AddWithValue("@date", DateTime.Now.Date);
                    cmdAyuda.Parameters.AddWithValue("@purpose", _selectedFundName);
                    cmdAyuda.Parameters.AddWithValue("@by", CurrentUser.Username);
                    cmdAyuda.Parameters.AddWithValue("@remarks", $"Released from fund: {_selectedFundName}");
                    cmdAyuda.ExecuteNonQuery();

                    var cmdMark = new SQLiteCommand(
                        "UPDATE fund_beneficiaries SET IsReleased=1, DateReleased=@date, ReleasedBy=@by " +
                        "WHERE idBeneficiary=@id", conn, txn);
                    cmdMark.Parameters.AddWithValue("@date", DateTime.Now.Date);
                    cmdMark.Parameters.AddWithValue("@by", CurrentUser.Username);
                    cmdMark.Parameters.AddWithValue("@id", benefId);
                    cmdMark.ExecuteNonQuery();

                    var cmdExp = new SQLiteCommand(
                        "INSERT INTO budget_transactions (TransactionDate,Category,Description,TransactionType,Amount,Source,ReferenceID,ReferenceType) " +
                        "VALUES(@d,'Financial Assistance',@desc,'Expense',@amt,@by,@bid,'Ayuda')", conn, txn);
                    cmdExp.Parameters.AddWithValue("@d", DateTime.Now.Date);
                    cmdExp.Parameters.AddWithValue("@desc", $"Ayuda release to {residentName} — {_selectedFundName}");
                    cmdExp.Parameters.AddWithValue("@amt", amount);
                    cmdExp.Parameters.AddWithValue("@by", CurrentUser.Username);
                    cmdExp.Parameters.AddWithValue("@bid", benefId);
                    cmdExp.ExecuteNonQuery();

                    txn.Commit();
                    AuditLogger.Log("RELEASE", "Ayuda",
                        $"Released ₱{amount:N2} to {residentName} from {_selectedFundName}");
                    MessageBox.Show(
                        $"✅  ₱{amount:N2} released to {residentName}!",
                        "Release Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    onSuccess?.Invoke();
                }
                catch (Exception ex)
                {
                    txn.Rollback();
                    MessageBox.Show("Release failed:\n" + ex.Message);
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  HELPERS
        // ══════════════════════════════════════════════════════════════════════
        private int? ParseResidentIdFromQr(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            raw = raw.Trim();
            const string marker = "ResidentID:";
            int idx = raw.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                int start = idx + marker.Length;
                int end = raw.IndexOf('|', start);
                string part = end >= 0 ? raw.Substring(start, end - start) : raw.Substring(start);
                if (int.TryParse(part.Trim(), out int parsed)) return parsed;
            }
            if (int.TryParse(raw, out int plain)) return plain;
            return null;
        }

        private void ColorBenefStatus(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var dgv = sender as DataGridView;
            if (dgv == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgv.Columns[e.ColumnIndex].Name != "Status" || e.Value == null) return;
            e.CellStyle.ForeColor = e.Value.ToString() == "Released" ? ReleasedGreen : PendingAmber;
            e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        }

        private void ColorInFund(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var dgv = sender as DataGridView;
            if (dgv == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgv.Columns[e.ColumnIndex].Name != "InFund" || e.Value == null) return;
            if (e.Value.ToString() == "✔ Added")
            {
                e.CellStyle.ForeColor = ReleasedGreen;
                e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            }
        }

        // Column helpers
        private static void ColVis(DataGridView g, string col, bool v)
        { if (g.Columns.Contains(col)) g.Columns[col].Visible = v; }
        private static void ColW(DataGridView g, string col, int w)
        { if (g.Columns.Contains(col)) { g.Columns[col].AutoSizeMode = DataGridViewAutoSizeColumnMode.None; g.Columns[col].Width = w; } }
        private static void ColFill(DataGridView g, string col)
        { if (g.Columns.Contains(col)) g.Columns[col].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; }
        private static void ColHdr(DataGridView g, string col, string hdr, int w)
        { if (!g.Columns.Contains(col)) return; g.Columns[col].HeaderText = hdr; ColW(g, col, w); }
        private static void ColFmt(DataGridView g, string col, int w, string fmt)
        { if (!g.Columns.Contains(col)) return; ColW(g, col, w); g.Columns[col].DefaultCellStyle.Format = fmt; }

        // Darkens a color by a fixed amount per channel — used to derive a
        // row's SelectionBackColor from its own status tint (see LoadBenef)
        // instead of falling back to the grid's default solid-blue selection.
        private static Color Darken(Color c, int amt)
        {
            int r = Math.Max(0, c.R - amt);
            int g = Math.Max(0, c.G - amt);
            int b = Math.Max(0, c.B - amt);
            return Color.FromArgb(c.A, r, g, b);
        }

        // Builds the "⋮" actions column shared by all three Distribution
        // beneficiary lists (All / Pending / Released). Kept narrow and
        // right-most so it never competes with the Fill'd Resident column
        // for space. Rendering itself is fully custom-painted (see
        // PaintCompactActionCells in BuildDistributionView) as a small
        // fixed-size pill rather than a full-cell button, so this column
        // just needs to exist and stay narrow — no button styling here.
        private DataGridViewTextBoxColumn MakeActionsColumn()
        {
            return new DataGridViewTextBoxColumn
            {
                Name = "Actions",
                HeaderText = "",
                ReadOnly = true,
                Width = 34,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
        }

        // UI factory helpers
        private Button NavBtn(string text)
        {
            var b = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Size = new Size(200, 36),
                Location = new Point(12, 10)
            };
            b.FlatAppearance.BorderColor = Color.FromArgb(120, 255, 255, 255);
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 255, 255, 255);
            return b;
        }

        private Panel SummaryCard(Point loc, Size size, string title, string caption, Color accent, string iconKind)
        {
            var p = new Panel { BackColor = BgPage, Location = loc, Size = size };
            p.Paint += (s, e) =>
            {
                var g2 = e.Graphics;
                g2.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, p.Width - 1, p.Height - 1);

                // Soft drop shadow (layered translucent passes to fake a blur)
                for (int i = 4; i >= 1; i--)
                {
                    var shadowRect = new Rectangle(r.X + i, r.Y + i + 1, r.Width, r.Height);
                    using (var shadowPath = RoundedPath(shadowRect, 12))
                    using (var shadowBrush = new SolidBrush(Color.FromArgb(5 + i, 20, 30, 60)))
                        g2.FillPath(shadowBrush, shadowPath);
                }

                // Card fill
                using (var path = RoundedPath(r, 12))
                using (var bg2 = new SolidBrush(Color.White))
                    g2.FillPath(bg2, path);

                // Left accent bar (clipped to the card's rounded shape)
                using (var path = RoundedPath(r, 12))
                {
                    var oldClip = g2.Clip;
                    g2.SetClip(path);
                    using (var accentBrush = new LinearGradientBrush(
                        new Rectangle(r.X, r.Y, 5, r.Height),
                        accent, Color.FromArgb(200, accent), LinearGradientMode.Vertical))
                        g2.FillRectangle(accentBrush, r.X, r.Y, 5, r.Height);
                    g2.Clip = oldClip;
                }

                // Card border
                using (var pen2 = new Pen(BorderCol, 1f))
                using (var path3 = RoundedPath(r, 12))
                    g2.DrawPath(pen2, path3);

                // Icon badge (soft tinted square, rounded) with a drawn vector glyph
                int cr = 38;
                int cx = p.Width - cr - 16;
                int cy = 16;
                var badgeRect = new Rectangle(cx, cy, cr, cr);
                using (var badgePath = RoundedPath(badgeRect, 10))
                using (var badgeBrush = new SolidBrush(Color.FromArgb(24, accent.R, accent.G, accent.B)))
                    g2.FillPath(badgeBrush, badgePath);
                var iconBounds = Rectangle.Inflate(badgeRect, -9, -9);
                DrawSummaryIcon(g2, iconBounds, accent, iconKind);
            };

            // Title label (Controls[0])
            p.Controls.Add(new Label
            {
                Text = title.ToUpper(),
                Font = new Font("Segoe UI", 7.8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(130, 136, 160),
                AutoSize = true,
                Location = new Point(20, 16),
                BackColor = Color.Transparent
            });

            // Amount label (Controls[1])
            var lblAmount = new Label
            {
                Text = "₱0.00",
                Font = new Font("Segoe UI", 19f, FontStyle.Bold),
                ForeColor = accent,
                AutoSize = true,
                Location = new Point(18, 38),
                BackColor = Color.Transparent
            };
            p.Controls.Add(lblAmount);

            // Caption label (Controls[2])
            p.Controls.Add(new Label
            {
                Text = caption,
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(150, 155, 175),
                AutoSize = true,
                Location = new Point(20, size.Height - 26),
                BackColor = Color.Transparent
            });

            return p;
        }

        // Draws a small, crisp vector glyph for a summary card — used instead of
        // emoji so the icon always renders correctly regardless of font support
        // (the previous emoji approach silently produced blank circles because
        // Substring(0,1) split surrogate-pair emoji characters in half).
        private static void DrawSummaryIcon(Graphics g, Rectangle bounds, Color color, string kind)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(color, 2f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            using (var brush = new SolidBrush(color))
            {
                switch (kind)
                {
                    case "up":
                    case "down":
                        {
                            bool up = kind == "up";
                            int barW = Math.Max(3, bounds.Width / 6);
                            int gap = Math.Max(2, barW / 2);
                            int totalW = barW * 3 + gap * 2;
                            int startX = bounds.X + (bounds.Width - totalW) / 2;
                            int[] h = up
                                ? new[] { bounds.Height * 2 / 5, bounds.Height * 3 / 5, bounds.Height }
                                : new[] { bounds.Height, bounds.Height * 3 / 5, bounds.Height * 2 / 5 };
                            for (int i = 0; i < 3; i++)
                            {
                                int bh = Math.Max(2, h[i]);
                                int x = startX + i * (barW + gap);
                                int y = bounds.Bottom - bh;
                                using (var barPath = RoundedPath(new Rectangle(x, y, barW, bh), 1))
                                    g.FillPath(brush, barPath);
                            }
                            Point tip = up
                                ? new Point(bounds.Right - 1, bounds.Y + 1)
                                : new Point(bounds.Right - 1, bounds.Bottom - 1);
                            int dir = up ? 1 : -1;
                            Point p1 = new Point(tip.X - 8, tip.Y);
                            Point p2 = new Point(tip.X, tip.Y + 8 * dir);
                            g.DrawLine(pen, p1.X, p1.Y, tip.X, tip.Y);
                            g.DrawLine(pen, tip.X, tip.Y, p2.X, p2.Y);
                            break;
                        }
                    case "wallet":
                    default:
                        {
                            var walletRect = new Rectangle(bounds.X + 1, bounds.Y + 4, bounds.Width - 2, bounds.Height - 8);
                            using (var wp = RoundedPath(walletRect, 4))
                                g.DrawPath(pen, wp);
                            int flapW = Math.Max(6, bounds.Width / 3);
                            var flapRect = new Rectangle(walletRect.Right - flapW - 1, walletRect.Y + walletRect.Height / 2 - 4, flapW, 8);
                            using (var fp = RoundedPath(flapRect, 2))
                                g.FillPath(brush, fp);
                            break;
                        }
                }
            }
        }

        // Small, crisp vector "logo" glyphs used for section/header badges
        // throughout the Distribution screen — drawn instead of emoji so
        // they always render cleanly regardless of the machine's installed
        // emoji font (this mirrors the fix already applied to DrawSummaryIcon).
        private static void DrawIcon(Graphics g, Rectangle bounds, Color color, string kind)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(color, 1.8f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            using (var brush = new SolidBrush(color))
            {
                switch (kind)
                {
                    case "box": // package / distribution
                        {
                            var body = new Rectangle(bounds.X, bounds.Y + bounds.Height / 3,
                                bounds.Width, bounds.Height - bounds.Height / 3);
                            g.DrawRectangle(pen, body);
                            g.DrawLine(pen, body.X, body.Y + body.Height / 2, body.Right, body.Y + body.Height / 2);
                            g.DrawLine(pen, body.X + body.Width / 2, body.Y, body.X + body.Width / 2, body.Bottom);
                            int midX = bounds.X + bounds.Width / 2;
                            g.DrawLine(pen, bounds.X, body.Y, midX, bounds.Y);
                            g.DrawLine(pen, bounds.Right, body.Y, midX, bounds.Y);
                            break;
                        }
                    case "camera": // QR scan
                        {
                            var bodyRect = new Rectangle(bounds.X, bounds.Y + bounds.Height / 5,
                                bounds.Width, bounds.Height - bounds.Height / 5);
                            using (var path = RoundedPath(bodyRect, 3))
                                g.DrawPath(pen, path);
                            int bumpW = Math.Max(4, bodyRect.Width / 3);
                            int bumpX = bodyRect.X + bodyRect.Width / 2 - bumpW / 2;
                            g.DrawLine(pen, bumpX, bodyRect.Y, bumpX, bounds.Y);
                            g.DrawLine(pen, bumpX + bumpW, bodyRect.Y, bumpX + bumpW, bounds.Y);
                            g.DrawLine(pen, bumpX, bounds.Y, bumpX + bumpW, bounds.Y);
                            int lensD = Math.Min(bodyRect.Width, bodyRect.Height) / 2;
                            var lensRect = new Rectangle(
                                bodyRect.X + bodyRect.Width / 2 - lensD / 2,
                                bodyRect.Y + bodyRect.Height / 2 - lensD / 2, lensD, lensD);
                            g.DrawEllipse(pen, lensRect);
                            break;
                        }
                    case "clipboard": // beneficiary list
                        {
                            var board = new Rectangle(bounds.X + 1, bounds.Y + 3, bounds.Width - 2, bounds.Height - 4);
                            using (var path = RoundedPath(board, 2))
                                g.DrawPath(pen, path);
                            int clipW = Math.Max(6, bounds.Width / 2);
                            var clip = new Rectangle(bounds.X + (bounds.Width - clipW) / 2, bounds.Y, clipW, 4);
                            using (var clipPath = RoundedPath(clip, 2))
                                g.FillPath(brush, clipPath);
                            int ly = board.Y + 7;
                            for (int i = 0; i < 3; i++)
                            {
                                g.DrawLine(pen, board.X + 4, ly, board.Right - 4, ly);
                                ly += Math.Max(4, board.Height / 5);
                            }
                            break;
                        }
                    case "hourglass": // pending
                        {
                            var p1 = new Point(bounds.X, bounds.Y);
                            var p2 = new Point(bounds.Right, bounds.Y);
                            var p3 = new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
                            var p4 = new Point(bounds.X, bounds.Bottom);
                            var p5 = new Point(bounds.Right, bounds.Bottom);
                            g.DrawLine(pen, p1, p2);
                            g.DrawLine(pen, p1, p3);
                            g.DrawLine(pen, p2, p3);
                            g.DrawLine(pen, p3, p4);
                            g.DrawLine(pen, p3, p5);
                            g.DrawLine(pen, p4, p5);
                            break;
                        }
                    case "check": // all released / confirmed
                        {
                            g.DrawLine(pen, bounds.X, bounds.Y + bounds.Height / 2, bounds.X + bounds.Width / 3, bounds.Bottom);
                            g.DrawLine(pen, bounds.X + bounds.Width / 3, bounds.Bottom, bounds.Right, bounds.Y);
                            break;
                        }
                }
            }
        }

        // A small rounded, tinted badge with a vector icon centered inside —
        // the "logo" mark used to the left of each section header.
        private Panel IconBadge(string kind, Color badgeBg, Color iconColor, int size)
        {
            var p = new Panel { Size = new Size(size, size), BackColor = Color.Transparent };
            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, size - 1, size - 1);
                using (var path = RoundedPath(r, Math.Max(4, size / 4)))
                using (var bgBrush = new SolidBrush(badgeBg))
                    e.Graphics.FillPath(bgBrush, path);
                var iconBounds = Rectangle.Inflate(r, -size / 4, -size / 4);
                DrawIcon(e.Graphics, iconBounds, iconColor, kind);
            };
            return p;
        }

        private Panel SectionLabelPanel(string text, int x, int y, Color accent)
        {
            var strip = new Panel { BackColor = accent, Location = new Point(x, y), Size = new Size(4, 22) };
            var lbl = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(x + 10, y - 1),
                BackColor = Color.Transparent
            };
            return strip; // caller adds both strip + lbl; we return strip for positioning
        }
        private static Label SectionLabel(string text, int x, int y) =>
            new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(x, y),
                BackColor = Color.Transparent
            };

        private PrettyButton SmallBtn(string text, Color bg)
        {
            return new PrettyButton
            {
                Text = text,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BaseColor = bg,
                ForeColor = Color.White,
                CornerRadius = 6
            };
        }

        private PrettyButton MakeBtn(string text, Color bg, float size)
        {
            return new PrettyButton
            {
                Text = text,
                Font = new Font("Segoe UI", size, FontStyle.Bold),
                BaseColor = bg,
                ForeColor = Color.White,
                CornerRadius = 6
            };
        }

        private PrettyButton Pill(string text, int x, int y)
        {
            int w = text.Length * 9 + 24;
            int h = 26;
            return new PrettyButton
            {
                Text = text,
                Font = new Font("Segoe UI", 9f),
                Size = new Size(w, h),
                Location = new Point(x, y),
                BaseColor = PillInactive,
                ForeColor = NavBlue,
                CornerRadius = h / 2
            };
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
                RowTemplate = { Height = 38 },
                Font = new Font("Segoe UI", 9.5f),
                GridColor = Color.FromArgb(224, 228, 242),
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 42,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = NavBlue;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            g.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            g.DefaultCellStyle.SelectionBackColor = BtnBlue;
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.AlternatingRowsDefaultCellStyle.BackColor = LightBlue;
            return g;
        }

        private Form MakeDialog(string title, string subtitle, Size size)
        {
            var dlg = new Form
            {
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ControlBox = true,
                ClientSize = size,
                BackColor = BgPage,
                Font = new Font("Segoe UI", 9.5f)
            };
            var hdr = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = NavBlue };
            hdr.Controls.Add(new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 10),
                BackColor = Color.Transparent
            });
            hdr.Controls.Add(new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(170, 195, 240),
                AutoSize = true,
                Location = new Point(22, 42),
                BackColor = Color.Transparent
            });
            dlg.Controls.Add(hdr);

            var outer = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = BgPage };
            dlg.Controls.Add(outer);
            outer.BringToFront();

            var inner = new Panel
            {
                BackColor = BgPage,
                Location = new Point(24, 18),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            outer.Controls.Add(inner);
            dlg.Tag = inner;
            return dlg;
        }

        private static Label FL(string text, int x, int y) =>
            new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 70, 100),
                AutoSize = true,
                Location = new Point(x, y + 2),
                BackColor = Color.Transparent
            };

        private void PaintCard(Graphics g, Panel p)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
            int d = 10;

            // Soft drop shadow — layered translucent passes to fake a blur,
            // giving every card a bit of lift off the page background.
            for (int i = 3; i >= 1; i--)
            {
                var shadowRect = new Rectangle(r.X + i, r.Y + i + 1, r.Width, r.Height);
                using (var shadowPath = new GraphicsPath())
                {
                    int sd = d * 2;
                    shadowPath.AddArc(shadowRect.X, shadowRect.Y, sd, sd, 180, 90);
                    shadowPath.AddArc(shadowRect.Right - sd, shadowRect.Y, sd, sd, 270, 90);
                    shadowPath.AddArc(shadowRect.Right - sd, shadowRect.Bottom - sd, sd, sd, 0, 90);
                    shadowPath.AddArc(shadowRect.X, shadowRect.Bottom - sd, sd, sd, 90, 90);
                    shadowPath.CloseFigure();
                    using (var shadowBrush = new SolidBrush(Color.FromArgb(4 + i, 20, 30, 60)))
                        g.FillPath(shadowBrush, shadowPath);
                }
            }

            using (var bg = new SolidBrush(BgCard))
            using (var pen = new Pen(BorderCol, 1f))
            {
                using (var path = new GraphicsPath())
                {
                    path.AddArc(r.X, r.Y, d * 2, d * 2, 180, 90);
                    path.AddArc(r.Right - d * 2, r.Y, d * 2, d * 2, 270, 90);
                    path.AddArc(r.Right - d * 2, r.Bottom - d * 2, d * 2, d * 2, 0, 90);
                    path.AddArc(r.X, r.Bottom - d * 2, d * 2, d * 2, 90, 90);
                    path.CloseFigure();
                    g.FillPath(bg, path);
                    g.DrawPath(pen, path);
                }
            }
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

        private static void DrawQrScannerGlyph(Graphics g, Rectangle bounds, Color frameColor, Color accentColor)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var bgBrush = new SolidBrush(Color.White))
            using (var bgPen = new Pen(BorderCol, 1f))
            {
                RoundRect(g, bgPen, new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1), 8);
                g.FillPath(bgBrush, RoundedPath(new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1), 8));
            }

            int pad = 14;
            var inner = Rectangle.Inflate(bounds, -pad, -pad);
            int cell = Math.Max(3, inner.Width / 7);
            bool[,] pattern = {
                {true, true, true, false,true, true, true},
                {true, false,true, false,true, false,true},
                {true, true, true, false,true, true, true},
                {false,false,false,true, false,false,false},
                {true, true, true, false,true, false,true},
                {true, false,true, false,false,true, true},
                {true, true, true, false,true, true, false},
            };
            using (var moduleBrush = new SolidBrush(frameColor))
            {
                for (int row = 0; row < 7; row++)
                    for (int col = 0; col < 7; col++)
                        if (pattern[row, col])
                            g.FillRectangle(moduleBrush,
                                inner.X + col * cell, inner.Y + row * cell, cell - 1, cell - 1);
            }
        }

        private static GraphicsPath RoundedPath(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "BudgetTransparencyControl";
            this.ResumeLayout(false);
        }
    }
}