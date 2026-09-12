using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BarangayManagementSystem
{
    public static class FormStyler
    {
        // ── Colour Palette ───────────────────────────────────────────────────
        public static readonly Color PrimaryDark = Color.FromArgb(10, 36, 99);
        public static readonly Color PrimaryMid = Color.FromArgb(21, 67, 160);
        public static readonly Color PrimaryLight = Color.FromArgb(41, 98, 196);
        public static readonly Color AccentGold = Color.FromArgb(212, 175, 55);
        public static readonly Color FormBackground = Color.FromArgb(245, 247, 252);
        public static readonly Color PanelWhite = Color.White;
        public static readonly Color TextDark = Color.FromArgb(15, 23, 42);
        public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);
        public static readonly Color BorderColor = Color.FromArgb(203, 213, 225);
        public static readonly Color InputBg = Color.FromArgb(248, 250, 255);
        public static readonly Color InputDisabled = Color.FromArgb(235, 238, 245);
        public static readonly Color SuccessGreen = Color.FromArgb(22, 163, 74);
        public static readonly Color DangerRed = Color.FromArgb(220, 38, 38);
        public static readonly Color SlateGrey = Color.FromArgb(71, 85, 105);

        // ── Fonts ────────────────────────────────────────────────────────────
        public static readonly Font FontTitle = new Font("Segoe UI", 20f, FontStyle.Bold);
        public static readonly Font FontSubtitle = new Font("Segoe UI", 9f, FontStyle.Regular);
        public static readonly Font FontLabel = new Font("Segoe UI", 9f, FontStyle.Bold);
        public static readonly Font FontInput = new Font("Segoe UI", 9.5f, FontStyle.Regular);
        public static readonly Font FontButton = new Font("Segoe UI", 9.5f, FontStyle.Bold);

        // ====================================================================
        // STEP 1 — sets form background + styles every control
        // ====================================================================
        public static void ApplyTheme(Form form)
        {
            form.BackColor = FormBackground;
            StyleAllControls(form);
        }

        // ====================================================================
        // STEP 2 — style your existing panelTop from Designer
        //
        // Parameters:
        //   headerPanel   = your panel control (e.g. panelTop)
        //   titleLabel    = label inside the panel showing the form title
        //   titleText     = text to show as the title
        //   subtitleText  = smaller text below title (pass null to hide)
        //   subtitleLabel = the subtitle label control (pass null if none)
        //   closeButton   = your X button inside the panel (pass null if none)
        // ====================================================================
        public static void StyleHeader(
            Panel headerPanel,
            Label titleLabel,
            string titleText,
            string subtitleText = null,
            Label subtitleLabel = null,
            Button closeButton = null)
        {
            // ── Panel gradient ───────────────────────────────────────────────
            headerPanel.BackColor = PrimaryDark;
            headerPanel.Height = Math.Max(headerPanel.Height, 80);
            headerPanel.Dock = DockStyle.Top;

            headerPanel.Paint -= HeaderPanel_Paint;
            headerPanel.Paint += HeaderPanel_Paint;

            // ── Title label ──────────────────────────────────────────────────
            if (titleLabel != null)
            {
                titleLabel.Text = titleText;
                titleLabel.Font = FontTitle;
                titleLabel.ForeColor = Color.White;
                titleLabel.BackColor = Color.Transparent;
                titleLabel.AutoSize = true;

                // Position: left-aligned with padding, vertically centered
                titleLabel.Location = new Point(20, subtitleText != null ? 14 : 22);
            }

            // ── Subtitle label ───────────────────────────────────────────────
            if (subtitleLabel != null)
            {
                subtitleLabel.Text = subtitleText ?? "";
                subtitleLabel.Font = new Font("Segoe UI", 12f, FontStyle.Regular);
                subtitleLabel.ForeColor = Color.FromArgb(180, 210, 255);
                subtitleLabel.BackColor = Color.Transparent;
                subtitleLabel.AutoSize = true;
                subtitleLabel.Location = new Point(22, 46);
                subtitleLabel.Visible = subtitleText != null;
            }

            // ── Close (X) button ─────────────────────────────────────────────
            if (closeButton != null)
            {
                // Match the dark navy of the header exactly
                closeButton.FlatStyle = FlatStyle.Flat;
                closeButton.FlatAppearance.BorderSize = 0;
                closeButton.FlatAppearance.MouseOverBackColor = DangerRed;
                closeButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 20, 20);
                closeButton.BackColor = PrimaryMid;   // same as header — blends in
                closeButton.ForeColor = Color.White;
                closeButton.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
                closeButton.Text = "✕";
                closeButton.Cursor = Cursors.Hand;
                closeButton.Size = new Size(46, 46);

                // Dock to the right edge, vertically centered in header
                closeButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                int headerH = headerPanel.Height;
                closeButton.Location = new Point(
                    headerPanel.Width - closeButton.Width,   // flush right
                    (headerH - closeButton.Height) / 2       // vertically centered
                );

                // Keep it flush-right when form is resized
                headerPanel.Resize += (s, e) =>
                {
                    closeButton.Location = new Point(
                        headerPanel.Width - closeButton.Width,
                        (headerPanel.Height - closeButton.Height) / 2);
                };
            }
        }

        private static void HeaderPanel_Paint(object sender, PaintEventArgs e)
        {
            Panel p = (Panel)sender;
            using (LinearGradientBrush brush = new LinearGradientBrush(
                p.ClientRectangle, Color.DarkBlue, PrimaryMid,
                LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(brush, p.ClientRectangle);
            }
            // Gold accent line at bottom of header
            using (SolidBrush gold = new SolidBrush(AccentGold))
                e.Graphics.FillRectangle(gold, 0, p.Height - 3, p.Width, 3);
        }

        // ====================================================================
        // CENTER a panel horizontally inside its parent form
        // Call after form Load so the form size is known.
        // ====================================================================
        public static void CenterPanel(Panel panel, int topOffset = 20)
        {
            if (panel.Parent == null) return;
            panel.Left = (panel.Parent.ClientSize.Width - panel.Width) / 2;
            panel.Top = panel.Top + topOffset;
            panel.Anchor = AnchorStyles.Top;
        }

        // ====================================================================
        // STYLE ALL CONTROLS RECURSIVELY
        // ====================================================================
        public static void StyleAllControls(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                switch (ctrl)
                {
                    case TextBox tb:
                        tb.Font = FontInput;
                        tb.ForeColor = TextDark;
                        tb.BorderStyle = BorderStyle.FixedSingle;
                        tb.BackColor = tb.ReadOnly ? InputDisabled : InputBg;
                        if (tb.ReadOnly) tb.ForeColor = TextMuted;
                        break;

                    case ComboBox cb:
                        cb.Font = FontInput;
                        cb.BackColor = InputBg;
                        cb.ForeColor = TextDark;
                        cb.FlatStyle = FlatStyle.Flat;
                        break;

                    case DateTimePicker dtp:
                        dtp.Font = FontInput;
                        dtp.CalendarTitleBackColor = PrimaryMid;
                        dtp.CalendarTitleForeColor = Color.White;
                        dtp.CalendarMonthBackground = InputBg;
                        dtp.CalendarForeColor = TextDark;
                        break;

                    case Button btn:
                        // Skip buttons inside the header (handled by StyleHeader)
                        if (IsInsideHeader(btn)) break;
                        StyleButton(btn);
                        break;

                    case Label lbl:
                        // Skip labels already styled by StyleHeader
                        if (lbl.ForeColor == Color.White ||
                            lbl.ForeColor == AccentGold ||
                            lbl.ForeColor == Color.FromArgb(180, 210, 255))
                            break;
                        lbl.Font = FontLabel;
                        lbl.ForeColor = TextDark;
                        lbl.BackColor = Color.Transparent;
                        break;

                    case GroupBox gb:
                        gb.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                        gb.ForeColor = PrimaryMid;
                        gb.BackColor = Color.Transparent;
                        break;
                }

                if (ctrl.Controls.Count > 0)
                    StyleAllControls(ctrl);
            }
        }

        private static bool IsInsideHeader(Control ctrl)
        {
            Control parent = ctrl.Parent;
            while (parent != null)
            {
                if (parent is Panel p && p.BackColor == PrimaryDark)
                    return true;
                parent = parent.Parent;
            }
            return false;
        }

        // ====================================================================
        // BUTTON STYLER
        // ====================================================================
        public static void StyleButton(Button btn)
        {
            string name = btn.Name?.ToLower() ?? "";

            if (name.Contains("add") || name.Contains("save") || name.Contains("submit"))
                Apply(btn, PrimaryMid, PrimaryLight);
            else if (name.Contains("delete") || name.Contains("remove"))
                Apply(btn, DangerRed, Color.FromArgb(185, 28, 28));
            else if (name.Contains("cancel") || name.Contains("close") || name.Contains("back"))
                Apply(btn, SlateGrey, Color.FromArgb(51, 65, 85));
            else if (name.Contains("clear") || name.Contains("reset"))
                Apply(btn, SlateGrey, Color.FromArgb(51, 65, 85));
            else if (name.Contains("upload") || name.Contains("photo") || name.Contains("browse"))
                Apply(btn, Color.FromArgb(8, 145, 178), Color.FromArgb(6, 113, 140));
            else
                Apply(btn, PrimaryMid, PrimaryLight);
        }

        public static void ApplyButtonStyle(Button btn, Color bg, Color hover)
            => Apply(btn, bg, hover);

        private static void Apply(Button btn, Color bg, Color hover)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = hover;
            btn.FlatAppearance.MouseDownBackColor = PrimaryDark;
            btn.BackColor = bg;
            btn.ForeColor = Color.White;
            btn.Font = FontButton;
            btn.Cursor = Cursors.Hand;
            btn.Height = Math.Max(btn.Height, 34);
        }

        // ====================================================================
        // READONLY TEXTBOX HELPER
        // ====================================================================
        public static void SetReadOnly(TextBox tb, string value)
        {
            tb.Text = value;
            tb.ReadOnly = true;
            tb.BackColor = InputDisabled;
            tb.ForeColor = TextMuted;
        }
    }
}