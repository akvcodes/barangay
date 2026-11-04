// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  BarcodePrintForm.cs  — QR Code ID Card with Template Picker             ║
// ║  3 layouts: ID-Style · Minimal · Official                                ║
// ║  Uses QRCoder NuGet package                                              ║
// ╚══════════════════════════════════════════════════════════════════════════╝

using QRCoder;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace BarangayManagementSystem
{
    public class BarcodePrintForm : Form
    {
        // ── Colors ────────────────────────────────────────────────────────────
        private static readonly Color NavBlue = Color.FromArgb(26, 35, 126);
        private static readonly Color BgPage = Color.FromArgb(240, 242, 248);
        private static readonly Color BtnGreen = Color.FromArgb(15, 157, 88);
        private static readonly Color BorderCol = Color.FromArgb(220, 225, 238);

        // ── Resident data ─────────────────────────────────────────────────────
        private readonly int _residentId;
        private readonly string _residentName;
        private readonly string _address;

        // ── QR bitmap ─────────────────────────────────────────────────────────
        private Bitmap _qrBitmap;

        // ── UI ────────────────────────────────────────────────────────────────
        private Panel _previewPanel;
        private int _selectedTemplate = 0; // 0=ID-Style  1=Minimal  2=Official
        private Button[] _templateBtns;

        // Card dimensions — consistent across all templates
        private const int CardW = 400;
        private const int CardH = 260;

        // ══════════════════════════════════════════════════════════════════════
        public BarcodePrintForm(int residentId, string residentName, string address = "")
        {
            _residentId = residentId;
            _residentName = residentName;
            _address = DecryptForDisplay(address);

            this.Text = "Print QR Code ID Card";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = BgPage;
            this.ClientSize = new Size(600, 620);
            this.Font = new Font("Segoe UI", 9.5f);

            GenerateQR();
            BuildUI();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  GENERATE QR
        // ══════════════════════════════════════════════════════════════════════
        private void GenerateQR()
        {
            string qrData = $"ResidentID:{_residentId}|Name:{_residentName}|Address:{_address}";
            using (var gen = new QRCodeGenerator())
            {
                var data = gen.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
                using (var code = new QRCode(data))
                    _qrBitmap = code.GetGraphic(10, Color.Black, Color.White, true);
            }
        }

        private static string DecryptForDisplay(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            try
            {
                return CryptoHelper.Decrypt(value, KeyManager.LoadKey());
            }
            catch (CryptoException)
            {
                return value;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  BUILD UI
        // ══════════════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            // ── Top bar ───────────────────────────────────────────────────────
            var header = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = NavBlue };
            header.Controls.Add(new Label
            {
                Text = "🪪  Print QR Code ID Card",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 12),
                BackColor = Color.Transparent,
            });
            header.Controls.Add(new Label
            {
                Text = "Choose a card layout, preview it, then print",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(170, 195, 240),
                AutoSize = true,
                Location = new Point(22, 40),
                BackColor = Color.Transparent,
            });
            this.Controls.Add(header);

            // ── Content ───────────────────────────────────────────────────────
            var content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgPage,
                Padding = new Padding(24, 16, 24, 16),
            };
            this.Controls.Add(content);
            content.BringToFront();

            // Resident info line
            content.Controls.Add(new Label
            {
                Text = $"Resident:  {_residentName}   (ID #{_residentId:D6})",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(0, 0),
                BackColor = Color.Transparent,
            });

            // ── Template picker label ─────────────────────────────────────────
            content.Controls.Add(new Label
            {
                Text = "Card Layout:",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 70, 100),
                AutoSize = true,
                Location = new Point(0, 28),
                BackColor = Color.Transparent,
            });

            // ── Template buttons ──────────────────────────────────────────────
            string[] labels = { "🪪  ID-Style", "◻  Minimal", "📋  Official" };
            _templateBtns = new Button[3];
            int tx = 100;
            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                var btn = new Button
                {
                    Text = labels[i],
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Size = new Size(130, 32),
                    Location = new Point(tx, 24),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    UseVisualStyleBackColor = false,
                    BackColor = i == 0 ? NavBlue : Color.FromArgb(220, 225, 238),
                    ForeColor = i == 0 ? Color.White : NavBlue,
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += (s, e) =>
                {
                    _selectedTemplate = idx;
                    HighlightTemplate(idx);
                    _previewPanel.Invalidate();
                };
                _templateBtns[i] = btn;
                content.Controls.Add(btn);
                tx += 138;
            }

            // ── Template description ──────────────────────────────────────────
            var lblDesc = new Label
            {
                Text = GetTemplateDesc(0),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(108, 114, 140),
                AutoSize = true,
                Location = new Point(0, 62),
                BackColor = Color.Transparent,
            };
            content.Controls.Add(lblDesc);

            // Update desc on template change
            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                _templateBtns[i].Click += (s, e) => lblDesc.Text = GetTemplateDesc(idx);
            }

            // ── Card preview ──────────────────────────────────────────────────
            int previewX = (this.ClientSize.Width - CardW) / 2 - content.Padding.Left;
            _previewPanel = new Panel
            {
                Location = new Point(previewX, 86),
                Size = new Size(CardW, CardH),
                BackColor = Color.White,
            };
            _previewPanel.Paint += (s, e) => DrawTemplate(e.Graphics, _selectedTemplate, _previewPanel.Width, _previewPanel.Height);
            content.Controls.Add(_previewPanel);

            // Drop shadow effect for preview
            _previewPanel.Paint += (s, e) =>
            {
                // outer shadow ring — painted before card content would overdraw, but we paint after so it frames
                using (var pen = new Pen(Color.FromArgb(60, 0, 0, 0), 2f))
                    e.Graphics.DrawRectangle(pen, 1, 1, _previewPanel.Width - 3, _previewPanel.Height - 3);
            };

            // ── Action buttons ────────────────────────────────────────────────
            int btnY = 86 + CardH + 20;

            var btnPrint = MakeBtn("🖨️  Print", NavBlue);
            btnPrint.Location = new Point(previewX, btnY);
            btnPrint.Size = new Size(140, 42);
            btnPrint.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnPrint.Click += (s, e) => DoPrint();
            content.Controls.Add(btnPrint);

            var btnPreview = MakeBtn("👁  Preview", BtnGreen);
            btnPreview.Location = new Point(previewX + 148, btnY);
            btnPreview.Size = new Size(140, 42);
            btnPreview.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnPreview.Click += (s, e) => DoPreview();
            content.Controls.Add(btnPreview);

            var btnClose = MakeBtn("Close", Color.FromArgb(100, 110, 128));
            btnClose.Location = new Point(previewX + 296, btnY);
            btnClose.Size = new Size(100, 42);
            btnClose.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnClose.Click += (s, e) => this.Close();
            content.Controls.Add(btnClose);

            content.Controls.Add(new Label
            {
                Text = "💡 Tip: Print on sticker paper or cut and laminate for durability.",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(108, 114, 140),
                AutoSize = true,
                Location = new Point(previewX, btnY + 50),
                BackColor = Color.Transparent,
            });
        }

        private string GetTemplateDesc(int idx)
        {
            switch (idx)
            {
                case 0: return "Bold navy header · resident name · QR code · ID number strip — classic ID card look";
                case 1: return "Clean white card · large QR centred · minimal text · name and ID only";
                case 2: return "Official document style · seal placeholder · formal layout with bordered sections";
                default: return "";
            }
        }

        private void HighlightTemplate(int idx)
        {
            for (int i = 0; i < _templateBtns.Length; i++)
            {
                _templateBtns[i].BackColor = i == idx ? NavBlue : Color.FromArgb(220, 225, 238);
                _templateBtns[i].ForeColor = i == idx ? Color.White : NavBlue;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  TEMPLATE DISPATCHER
        // ══════════════════════════════════════════════════════════════════════
        private void DrawTemplate(Graphics g, int template, int w, int h)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            switch (template)
            {
                case 1: DrawMinimal(g, w, h); break;
                case 2: DrawOfficial(g, w, h); break;
                default: DrawIDStyle(g, w, h); break;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  TEMPLATE 0 — ID-STYLE
        //  Two-column: left navy sidebar + QR on right, name/details below header
        // ══════════════════════════════════════════════════════════════════════
        private void DrawIDStyle(Graphics g, int w, int h)
        {
            // Background
            g.FillRectangle(Brushes.White, 0, 0, w, h);

            // ── Header strip ──────────────────────────────────────────────────
            int hdrH = 52;
            using (var b = new LinearGradientBrush(
                new Rectangle(0, 0, w, hdrH),
                NavBlue, Color.FromArgb(50, 60, 160), LinearGradientMode.Horizontal))
                g.FillRectangle(b, 0, 0, w, hdrH);

            // Barangay name
            using (var f = new Font("Segoe UI", 12f, FontStyle.Bold))
            {
                var sz = g.MeasureString("BARANGAY MARBER", f);
                g.DrawString("BARANGAY MARBER", f, Brushes.White,
                    new PointF((w - sz.Width) / 2f, 8));
            }
            using (var f = new Font("Segoe UI", 7.5f))
            {
                var sz = g.MeasureString("MATANAO, DAVAO DEL SUR  •  AYUDA ID CARD", f);
                g.DrawString("MATANAO, DAVAO DEL SUR  •  AYUDA ID CARD", f,
                    new SolidBrush(Color.FromArgb(180, 210, 255)),
                    new PointF((w - sz.Width) / 2f, 32));
            }

            // ── Left sidebar ──────────────────────────────────────────────────
            int sideW = 130;
            g.FillRectangle(new SolidBrush(Color.FromArgb(240, 242, 252)), 0, hdrH, sideW, h - hdrH);

            // QR code in sidebar — centred
            if (_qrBitmap != null)
            {
                int qrSize = 104;
                int qrX = (sideW - qrSize) / 2;
                int qrY = hdrH + 12;
                // White bg behind QR
                g.FillRectangle(Brushes.White, qrX - 4, qrY - 4, qrSize + 8, qrSize + 8);
                using (var pen = new Pen(Color.FromArgb(200, 210, 230), 1f))
                    g.DrawRectangle(pen, qrX - 4, qrY - 4, qrSize + 8, qrSize + 8);
                g.DrawImage(_qrBitmap, new Rectangle(qrX, qrY, qrSize, qrSize));
            }

            // Scan label
            using (var f = new Font("Segoe UI", 6.5f))
            {
                var sz = g.MeasureString("SCAN TO VERIFY", f);
                g.DrawString("SCAN TO VERIFY", f,
                    new SolidBrush(Color.FromArgb(120, 130, 160)),
                    new PointF((sideW - sz.Width) / 2f, hdrH + 122));
            }

            // Navy accent line between sidebar and main
            g.FillRectangle(new SolidBrush(NavBlue), sideW, hdrH, 3, h - hdrH);

            // ── Right main area ───────────────────────────────────────────────
            int rx = sideW + 14;
            int ry = hdrH + 16;

            // "RESIDENT" badge
            using (var b = new SolidBrush(Color.FromArgb(26, 35, 126)))
            {
                var badgeRect = new Rectangle(rx, ry, 68, 16);
                using (var path = RoundPath(badgeRect, 4))
                    g.FillPath(b, path);
            }
            using (var f = new Font("Segoe UI", 6.5f, FontStyle.Bold))
                g.DrawString("RESIDENT", f, Brushes.White, new PointF(rx + 6, ry + 2));
            ry += 24;

            // Name
            string name = _residentName.Length > 28 ? _residentName.Substring(0, 25) + "..." : _residentName;
            using (var f = new Font("Segoe UI", 13f, FontStyle.Bold))
            {
                var szName = g.MeasureString(name.ToUpper(), f);
                // Wrap if too wide
                int maxW = w - rx - 12;
                if (szName.Width > maxW)
                {
                    g.DrawString(name.ToUpper(), f,
                        new SolidBrush(Color.FromArgb(20, 30, 80)),
                        new RectangleF(rx, ry, maxW, 52));
                }
                else
                    g.DrawString(name.ToUpper(), f, new SolidBrush(Color.FromArgb(20, 30, 80)), new PointF(rx, ry));
            }
            ry += 46;

            // Address
            if (!string.IsNullOrWhiteSpace(_address))
            {
                using (var f = new Font("Segoe UI", 7.5f))
                {
                    string addr = _address.Length > 50 ? _address.Substring(0, 47) + "..." : _address;
                    g.DrawString(addr, f, new SolidBrush(Color.FromArgb(90, 100, 120)),
                        new RectangleF(rx, ry, w - rx - 12, 30));
                }
                ry += 26;
            }

            // Divider
            g.DrawLine(new Pen(Color.FromArgb(210, 215, 235), 0.8f), rx, ry, w - 10, ry);
            ry += 10;

            // ID number section
            using (var f = new Font("Segoe UI", 7f))
                g.DrawString("RESIDENT ID NO.", f, new SolidBrush(Color.FromArgb(130, 140, 160)), new PointF(rx, ry));
            ry += 14;
            using (var f = new Font("Courier New", 14f, FontStyle.Bold))
                g.DrawString(_residentId.ToString("D6"), f, new SolidBrush(NavBlue), new PointF(rx, ry));
            ry += 26;

            // Valid / Issued line
            using (var f = new Font("Segoe UI", 7f))
                g.DrawString($"Issued: {DateTime.Now:MMMM dd, yyyy}", f,
                    new SolidBrush(Color.FromArgb(140, 150, 170)), new PointF(rx, ry));

            // ── Bottom strip ──────────────────────────────────────────────────
            int bsY = h - 22;
            g.FillRectangle(new SolidBrush(NavBlue), 0, bsY, w, 22);
            using (var f = new Font("Segoe UI", 6.5f))
                g.DrawString(
                    "This card is property of Barangay Marber — present during Ayuda distribution.",
                    f, new SolidBrush(Color.FromArgb(170, 195, 240)),
                    new PointF(8, bsY + 5));

            // Card border
            using (var pen = new Pen(Color.FromArgb(180, 190, 210), 1.2f))
                g.DrawRectangle(pen, 0, 0, w - 1, h - 1);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  TEMPLATE 1 — MINIMAL
        //  Clean white card, large QR centred, name + ID below, no heavy chrome
        // ══════════════════════════════════════════════════════════════════════
        private void DrawMinimal(Graphics g, int w, int h)
        {
            // Background
            g.FillRectangle(Brushes.White, 0, 0, w, h);

            // Top thin accent bar
            g.FillRectangle(new SolidBrush(NavBlue), 0, 0, w, 6);

            // Barangay name — top, small
            using (var f = new Font("Segoe UI", 8f, FontStyle.Bold))
            {
                string txt = "BARANGAY MARBER  ·  MATANAO, DAVAO DEL SUR";
                var sz = g.MeasureString(txt, f);
                g.DrawString(txt, f, new SolidBrush(Color.FromArgb(80, 90, 120)),
                    new PointF((w - sz.Width) / 2f, 14));
            }

            // Thin rule
            g.DrawLine(new Pen(Color.FromArgb(220, 225, 238), 0.8f), 30, 30, w - 30, 30);

            // Large QR code — centred
            if (_qrBitmap != null)
            {
                int qrSize = 130;
                int qrX = (w - qrSize) / 2;
                int qrY = 36;
                g.DrawImage(_qrBitmap, new Rectangle(qrX, qrY, qrSize, qrSize));
            }

            // Name — bold, centred
            using (var f = new Font("Segoe UI", 12f, FontStyle.Bold))
            {
                string name = _residentName.ToUpper();
                var sz = g.MeasureString(name, f);
                float nx = (w - sz.Width) / 2f;
                if (sz.Width > w - 20)
                {
                    var fmt = new StringFormat { Alignment = StringAlignment.Center };
                    g.DrawString(name, f, new SolidBrush(Color.FromArgb(20, 30, 80)),
                        new RectangleF(10, 174, w - 20, 40), fmt);
                }
                else
                    g.DrawString(name, f, new SolidBrush(Color.FromArgb(20, 30, 80)), new PointF(nx, 174));
            }

            // Address — small, centred, truncated
            if (!string.IsNullOrWhiteSpace(_address))
            {
                using (var f = new Font("Segoe UI", 7.5f))
                {
                    string addr = _address.Length > 60 ? _address.Substring(0, 57) + "..." : _address;
                    var fmt = new StringFormat { Alignment = StringAlignment.Center };
                    g.DrawString(addr, f, new SolidBrush(Color.FromArgb(110, 120, 140)),
                        new RectangleF(10, 198, w - 20, 22), fmt);
                }
            }

            // Thin rule
            g.DrawLine(new Pen(Color.FromArgb(220, 225, 238), 0.8f), 30, 222, w - 30, 222);

            // ID number row
            using (var fLabel = new Font("Segoe UI", 7f))
            using (var fNum = new Font("Courier New", 11f, FontStyle.Bold))
            {
                string idStr = _residentId.ToString("D6");
                var szLabel = g.MeasureString("ID NO.", fLabel);
                var szNum = g.MeasureString(idStr, fNum);
                float totalW = szLabel.Width + 8 + szNum.Width;
                float startX = (w - totalW) / 2f;

                g.DrawString("ID NO.", fLabel, new SolidBrush(Color.FromArgb(130, 140, 160)),
                    new PointF(startX, 230));
                g.DrawString(idStr, fNum, new SolidBrush(NavBlue),
                    new PointF(startX + szLabel.Width + 8, 226));
            }

            // Bottom accent bar
            g.FillRectangle(new SolidBrush(NavBlue), 0, h - 6, w, 6);

            // Card border
            using (var pen = new Pen(Color.FromArgb(200, 210, 230), 1f))
                g.DrawRectangle(pen, 0, 0, w - 1, h - 1);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  TEMPLATE 2 — OFFICIAL
        //  Formal document style: seal circle, bordered sections, full details
        // ══════════════════════════════════════════════════════════════════════
        private void DrawOfficial(Graphics g, int w, int h)
        {
            // Cream-white background
            g.FillRectangle(new SolidBrush(Color.FromArgb(253, 252, 248)), 0, 0, w, h);

            // Double border
            using (var pen = new Pen(Color.FromArgb(26, 35, 126), 2.5f))
                g.DrawRectangle(pen, 2, 2, w - 5, h - 5);
            using (var pen = new Pen(Color.FromArgb(180, 190, 220), 1f))
                g.DrawRectangle(pen, 6, 6, w - 13, h - 13);

            // ── Top header ────────────────────────────────────────────────────
            // Republic / Republic text
            using (var f = new Font("Segoe UI", 6.5f))
            {
                string txt = "Republic of the Philippines  ·  Province of Davao del Sur";
                var sz = g.MeasureString(txt, f);
                g.DrawString(txt, f, new SolidBrush(Color.FromArgb(100, 110, 130)),
                    new PointF((w - sz.Width) / 2f, 12));
            }

            // ── Seal circle (left of heading) ─────────────────────────────────
            int sealX = 16, sealY = 20, sealR = 36;
            using (var b = new SolidBrush(Color.FromArgb(240, 242, 252)))
                g.FillEllipse(b, sealX, sealY, sealR, sealR);
            using (var pen = new Pen(NavBlue, 1.5f))
                g.DrawEllipse(pen, sealX, sealY, sealR, sealR);
            using (var pen = new Pen(NavBlue, 0.5f))
                g.DrawEllipse(pen, sealX + 3, sealY + 3, sealR - 6, sealR - 6);
            // Star inside seal
            using (var f = new Font("Segoe UI", 14f))
            {
                var sz = g.MeasureString("★", f);
                g.DrawString("★", f, new SolidBrush(NavBlue),
                    new PointF(sealX + (sealR - sz.Width) / 2f, sealY + (sealR - sz.Height) / 2f + 1));
            }

            // ── Heading block ─────────────────────────────────────────────────
            int hcx = w / 2;
            using (var f = new Font("Segoe UI", 11f, FontStyle.Bold))
            {
                var sz = g.MeasureString("BARANGAY MARBER", f);
                g.DrawString("BARANGAY MARBER", f, new SolidBrush(NavBlue),
                    new PointF(hcx - sz.Width / 2f, 22));
            }
            using (var f = new Font("Segoe UI", 7.5f))
            {
                string sub = "Municipality of Matanao, Davao del Sur";
                var sz = g.MeasureString(sub, f);
                g.DrawString(sub, f, new SolidBrush(Color.FromArgb(80, 90, 120)),
                    new PointF(hcx - sz.Width / 2f, 38));
            }

            // Title bar
            int titleY = 58;
            using (var b = new SolidBrush(NavBlue))
                g.FillRectangle(b, 10, titleY, w - 20, 22);
            using (var f = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            {
                string title = "OFFICIAL RESIDENT IDENTIFICATION CARD";
                var sz = g.MeasureString(title, f);
                g.DrawString(title, f, Brushes.White,
                    new PointF((w - sz.Width) / 2f, titleY + 3));
            }

            // ── Two-column body ───────────────────────────────────────────────
            int bodyY = titleY + 28;
            int leftW = 140;
            int rightX = leftW + 14;
            int rightW = w - rightX - 14;

            // Left column: QR code
            if (_qrBitmap != null)
            {
                int qrSize = 110;
                int qrX = (leftW - qrSize) / 2 + 10;
                int qrY = bodyY + 4;
                // QR box
                g.FillRectangle(Brushes.White, qrX - 4, qrY - 4, qrSize + 8, qrSize + 8);
                using (var pen = new Pen(Color.FromArgb(180, 190, 210), 1f))
                    g.DrawRectangle(pen, qrX - 4, qrY - 4, qrSize + 8, qrSize + 8);
                g.DrawImage(_qrBitmap, new Rectangle(qrX, qrY, qrSize, qrSize));
            }

            // Scan label under QR
            using (var f = new Font("Segoe UI", 6f))
            {
                var sz = g.MeasureString("SCAN TO VERIFY", f);
                g.DrawString("SCAN TO VERIFY", f,
                    new SolidBrush(Color.FromArgb(110, 120, 150)),
                    new PointF((leftW - sz.Width) / 2f + 10, bodyY + 120));
            }

            // Vertical divider
            g.DrawLine(new Pen(Color.FromArgb(200, 210, 228), 1f),
                leftW + 6, bodyY, leftW + 6, h - 14);

            // Right column — fields
            int fy = bodyY + 4;
            DrawField(g, rightX, fy, rightW, "Name", _residentName.ToUpper(), true);
            fy += 32;
            DrawField(g, rightX, fy, rightW, "Address", _address ?? "", false);
            fy += 30;
            DrawField(g, rightX, fy, rightW, "ID Number", _residentId.ToString("D6"), true);
            fy += 28;
            DrawField(g, rightX, fy, rightW, "Date Issued", DateTime.Now.ToString("MMMM dd, yyyy"), false);
            fy += 28;
            DrawField(g, rightX, fy, rightW, "Issued By", "Barangay Secretary", false);

            // ── Bottom strip ──────────────────────────────────────────────────
            int bsY = h - 22;
            using (var b = new SolidBrush(NavBlue))
                g.FillRectangle(b, 10, bsY, w - 20, 16);
            using (var f = new Font("Segoe UI", 6f))
            {
                string txt = "This card is property of Barangay Marber · Present during Ayuda distribution · Not transferable";
                var sz = g.MeasureString(txt, f);
                g.DrawString(txt, f, new SolidBrush(Color.FromArgb(180, 200, 240)),
                    new PointF((w - sz.Width) / 2f, bsY + 2));
            }
        }

        // Helper: draw a labelled field for the Official template
        private void DrawField(Graphics g, int x, int y, int w, string label, string value, bool bold)
        {
            using (var fLabel = new Font("Segoe UI", 6.5f))
                g.DrawString(label + ":", fLabel,
                    new SolidBrush(Color.FromArgb(120, 130, 155)), new PointF(x, y));
            using (var fVal = new Font("Segoe UI", bold ? 9f : 8f, bold ? FontStyle.Bold : FontStyle.Regular))
            {
                string val = value.Length > 36 ? value.Substring(0, 33) + "..." : value;
                g.DrawString(val, fVal, new SolidBrush(Color.FromArgb(20, 30, 70)),
                    new RectangleF(x, y + 10, w, 18));
            }
            g.DrawLine(new Pen(Color.FromArgb(200, 210, 228), 0.6f), x, y + 26, x + w - 4, y + 26);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PRINT
        // ══════════════════════════════════════════════════════════════════════
        private PrintDocument BuildPrintDoc()
        {
            var pd = new PrintDocument();
            pd.DefaultPageSettings.PaperSize = new PaperSize("Card", 350, 220);
            pd.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);
            pd.PrintPage += (s, e) =>
            {
                int pw = e.MarginBounds.Width;
                int ph = e.MarginBounds.Height;
                e.Graphics.TranslateTransform(e.MarginBounds.X, e.MarginBounds.Y);
                DrawTemplate(e.Graphics, _selectedTemplate, pw, ph);
            };
            return pd;
        }

        private void DoPrint()
        {
            using (var pd = BuildPrintDoc())
            using (var dlg = new PrintDialog { Document = pd })
                if (dlg.ShowDialog() == DialogResult.OK) pd.Print();
        }

        private void DoPreview()
        {
            using (var pd = BuildPrintDoc())
            {
                var prev = new PrintPreviewDialog
                {
                    Document = pd,
                    Text = "QR Code ID Card Preview",
                    WindowState = FormWindowState.Maximized,
                };
                prev.ShowDialog();
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  HELPERS
        // ══════════════════════════════════════════════════════════════════════
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

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            _qrBitmap?.Dispose();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // BarcodePrintForm
            // 
            this.ClientSize = new System.Drawing.Size(600, 620);
            this.Name = "BarcodePrintForm";
            this.Load += new System.EventHandler(this.BarcodePrintForm_Load);
            this.ResumeLayout(false);

        }

        private void BarcodePrintForm_Load(object sender, EventArgs e)
        {

        }
    }
}