// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  ResidentManagementControl.cs  — Inline resident profile (no popup)      ║
// ║  CHANGE FROM PREVIOUS VERSION:                                           ║
// ║  ResidentProfileDialog (a Form shown via ShowDialog) has been converted  ║
// ║  into ResidentProfileControl (a UserControl). Selecting a resident from  ║
// ║  the picker now swaps this control directly into the main content area   ║
// ║  of ResidentManagementControl instead of opening a separate modal        ║
// ║  window. The "View Resident" button in the toolbar stays visible and     ║
// ║  clickable at all times, so you can jump to another resident without     ║
// ║  ever needing to click "Close" first.                                    ║
// ║  DELETE old file + Designer. Add as New Item → Class.                    ║
// ╚══════════════════════════════════════════════════════════════════════════╝

using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace BarangayManagementSystem.Controls
{
    public class ResidentManagementControl : UserControl
    {
        // ── Colors ────────────────────────────────────────────────────────────
        private static readonly Color NavBlue = Color.FromArgb(26, 35, 126);
        private static readonly Color BgPage = Color.FromArgb(240, 242, 248);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderCol = Color.FromArgb(220, 225, 238);
        private static readonly Color TextGray = Color.FromArgb(108, 114, 140);
        private static readonly Color BtnGreen = Color.FromArgb(15, 157, 88);
        private static readonly Color BtnBlue = Color.FromArgb(66, 133, 244);

        private Panel _toolbar;

        private Panel _contentContainer;
        private Panel _emptyState;
        private ResidentProfileControl _profileControl;
        private TaxCollectionControl _taxCollectionControl;

        // ══════════════════════════════════════════════════════════════════════
        public ResidentManagementControl()
        {
            this.DoubleBuffered = true;
            this.BackColor = BgPage;
            this.Dock = DockStyle.Fill;
            BuildUI();

            if (CurrentUser.IsTreasurer())
                OpenTaxCollection();
        }

        // ══════════════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            var topBar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = NavBlue };

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
            btnBack.Click += (s, e) => this.FindForm()?.Close();
            topBar.Controls.Add(btnBack);

            var lblTopTitle = new Label
            {
                Text = "👥   Resident Management",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            topBar.Controls.Add(lblTopTitle);
            topBar.Layout += (s, e) => lblTopTitle.Location = new Point(
                (topBar.Width - lblTopTitle.Width) / 2, (topBar.Height - lblTopTitle.Height) / 2);

            _toolbar = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = BgCard, Padding = new Padding(16, 8, 16, 8) };
            _toolbar.Paint += (s, e) =>
            {
                using (var pen = new Pen(BorderCol))
                    e.Graphics.DrawLine(pen, 0, _toolbar.Height - 1, _toolbar.Width, _toolbar.Height - 1);
            };
            this.Controls.Add(_toolbar);

            var btnViewResident = MakeBtn("👥  View Resident", BtnBlue, 9.5f);
            btnViewResident.Size = new Size(170, 38);
            btnViewResident.Location = new Point(0, 16);
            btnViewResident.Click += (s, e) => OpenResidentPicker();
            _toolbar.Controls.Add(btnViewResident);

            if (CurrentUser.IsSuperAdmin() || CurrentUser.IsTreasurer())
            {
                var btnTaxCollection = MakeBtn("🧾  Tax Collection · Cedula", BtnGreen, 9.5f);
                btnTaxCollection.Size = new Size(220, 38);
                btnTaxCollection.Location = new Point(182, 16);
                btnTaxCollection.Click += (s, e) => OpenTaxCollection();
                _toolbar.Controls.Add(btnTaxCollection);
            }

            var main = new Panel { Dock = DockStyle.Fill, BackColor = BgPage, Padding = new Padding(16) };
            this.Controls.Add(main);

            _contentContainer = new Panel { Dock = DockStyle.Fill, BackColor = BgPage };
            main.Controls.Add(_contentContainer);

            _emptyState = new Panel { Dock = DockStyle.Fill, BackColor = BgCard };
            _emptyState.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(BorderCol))
                    DrawRR(e.Graphics, pen, new Rectangle(0, 0, _emptyState.Width - 1, _emptyState.Height - 1), 8);
            };
            _contentContainer.Controls.Add(_emptyState);

            var lblIcon = new Label
            {
                Text = "( - )",
                Font = new Font("Segoe UI", 36f, FontStyle.Bold),
                AutoSize = true,
                ForeColor = Color.FromArgb(200, 210, 230),
                BackColor = Color.Transparent,
            };
            var lblTitle = new Label
            {
                Text = "No resident open",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 190, 210),
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            var lblSub = new Label
            {
                Text = "Click \"View Resident\" to search for someone and open their profile",
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(200, 210, 225),
                AutoSize = true,
                BackColor = Color.Transparent,
            };
            _emptyState.Controls.AddRange(new Control[] { lblIcon, lblTitle, lblSub });
            _emptyState.Resize += (s, e) =>
            {
                int cx = _emptyState.Width / 2, cy = _emptyState.Height / 2;
                lblIcon.Location = new Point(cx - lblIcon.Width / 2, cy - 90);
                lblTitle.Location = new Point(cx - lblTitle.Width / 2, cy - 10);
                lblSub.Location = new Point(cx - lblSub.Width / 2, cy + 32);
            };

            this.Controls.Add(topBar);
        }

        private void OpenResidentPicker()
        {
            using (var picker = new ResidentPickerDialog())
            {
                if (picker.ShowDialog(this.FindForm()) == DialogResult.OK && picker.SelectedResidentId > 0)
                {
                    OpenProfile(picker.SelectedResidentId);
                }
            }
        }

        private void OpenProfile(int residentId)
        {
            _contentContainer.SuspendLayout();

            if (_profileControl != null)
            {
                _contentContainer.Controls.Remove(_profileControl);
                _profileControl.Dispose();
                _profileControl = null;
            }
            if (_taxCollectionControl != null)
            {
                _contentContainer.Controls.Remove(_taxCollectionControl);
                _taxCollectionControl.Dispose();
                _taxCollectionControl = null;
            }

            _emptyState.Visible = false;

            _profileControl = new ResidentProfileControl(residentId, ShowEmptyState);
            _profileControl.Dock = DockStyle.Fill;
            _contentContainer.Controls.Add(_profileControl);
            _profileControl.BringToFront();

            _contentContainer.ResumeLayout();
        }

        private void OpenTaxCollection()
        {
            _contentContainer.SuspendLayout();

            if (_profileControl != null)
            {
                _contentContainer.Controls.Remove(_profileControl);
                _profileControl.Dispose();
                _profileControl = null;
            }
            if (_taxCollectionControl != null)
            {
                _contentContainer.Controls.Remove(_taxCollectionControl);
                _taxCollectionControl.Dispose();
                _taxCollectionControl = null;
            }

            _emptyState.Visible = false;

            _taxCollectionControl = new TaxCollectionControl();
            _taxCollectionControl.Dock = DockStyle.Fill;
            _contentContainer.Controls.Add(_taxCollectionControl);
            _taxCollectionControl.BringToFront();

            _contentContainer.ResumeLayout();
        }

        private void ShowEmptyState()
        {
            if (_profileControl != null)
            {
                _contentContainer.Controls.Remove(_profileControl);
                _profileControl.Dispose();
                _profileControl = null;
            }
            if (_taxCollectionControl != null)
            {
                _contentContainer.Controls.Remove(_taxCollectionControl);
                _taxCollectionControl.Dispose();
                _taxCollectionControl = null;
            }
            _emptyState.Visible = true;
            _emptyState.BringToFront();
        }

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

        private static void DrawRR(Graphics g, Pen pen, Rectangle r, int radius)
        {
            using (var path = RoundPath(r, radius))
                g.DrawPath(pen, path);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "ResidentManagementControl";
            this.Load += new System.EventHandler(this.ResidentManagementControl_Load);
            this.ResumeLayout(false);
        }

        private void ResidentManagementControl_Load(object sender, EventArgs e)
        {
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  RESIDENT PICKER DIALOG — search + list popup
    // ══════════════════════════════════════════════════════════════════════════
    public class ResidentPickerDialog : Form
    {
        private static readonly Color NavBlue = Color.FromArgb(26, 35, 126);
        private static readonly Color NavBlueChip = Color.FromArgb(53, 64, 128);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderCol = Color.FromArgb(220, 225, 238);
        private static readonly Color TextDark = Color.FromArgb(28, 30, 44);
        private static readonly Color TextGray = Color.FromArgb(108, 114, 140);
        private static readonly Color ActiveRow = Color.FromArgb(225, 230, 250);
        private static readonly Color BtnGreen = Color.FromArgb(15, 157, 88);
        private static readonly Color BtnBlue = Color.FromArgb(66, 133, 244);
        private static readonly Color BtnGray = Color.FromArgb(100, 110, 128);

        public int SelectedResidentId { get; private set; } = 0;

        private TextBox _txtSearch;
        private Button _chipAll, _chipMale, _chipFemale, _chipSenior;
        private string _currentGenderFilter = "All";
        private Label _lblCount;
        private ListBox _lstResidents;
        private DataTable _dtResidents;
        private readonly Dictionary<int, Image> _listPhotoCache = new Dictionary<int, Image>();

        public ResidentPickerDialog()
        {
            this.Text = "Select a Resident";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(420, 560);
            this.BackColor = Color.White;

            BuildUI();
            this.Load += (s, e) => LoadResidentList();
            this.FormClosed += (s, e) =>
            {
                _lstResidents.DrawItem -= DrawResidentItem;
                _lstResidents.Items.Clear();

                foreach (var img in _listPhotoCache.Values) img?.Dispose();
                _listPhotoCache.Clear();
            };
        }

        private void BuildUI()
        {
            var header = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = NavBlue };
            this.Controls.Add(header);
            header.Controls.Add(new Label
            {
                Text = "Select a resident",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(16, 16),
                BackColor = Color.Transparent,
            });

            var btnAdd = MakeBtn("➕  Add Resident", BtnGreen);
            btnAdd.Location = new Point(16, 64);
            btnAdd.Size = new Size(388, 34);
            btnAdd.Click += (s, e) => OpenAddResident();
            this.Controls.Add(btnAdd);

            _txtSearch = new TextBox
            {
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(16, 108),
                Size = new Size(388, 26),
                BorderStyle = BorderStyle.FixedSingle,
            };
            SetPlaceholder(_txtSearch, "Search residents...");
            _txtSearch.TextChanged += (s, e) =>
            {
                if (_txtSearch.Tag is bool ph && ph) return;
                RefreshFilteredList();
            };
            this.Controls.Add(_txtSearch);

            var chipRow = new Panel { Location = new Point(16, 140), Size = new Size(388, 24), BackColor = Color.Transparent };
            this.Controls.Add(chipRow);
            int cx = 0;
            _chipAll = GenderChip(chipRow, "All", ref cx);
            _chipMale = GenderChip(chipRow, "Male", ref cx);
            _chipFemale = GenderChip(chipRow, "Female", ref cx);
            _chipSenior = GenderChip(chipRow, "Senior", ref cx);
            SetActiveChip(_chipAll);
            _chipAll.Click += (s, e) => { _currentGenderFilter = "All"; SetActiveChip(_chipAll); RefreshFilteredList(); };
            _chipMale.Click += (s, e) => { _currentGenderFilter = "Male"; SetActiveChip(_chipMale); RefreshFilteredList(); };
            _chipFemale.Click += (s, e) => { _currentGenderFilter = "Female"; SetActiveChip(_chipFemale); RefreshFilteredList(); };
            _chipSenior.Click += (s, e) => { _currentGenderFilter = "Senior"; SetActiveChip(_chipSenior); RefreshFilteredList(); };

            _lblCount = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8f),
                ForeColor = TextGray,
                AutoSize = true,
                Location = new Point(16, 170),
                BackColor = Color.Transparent,
            };
            this.Controls.Add(_lblCount);

            _lstResidents = new ListBox
            {
                Location = new Point(16, 192),
                Size = new Size(388, 320),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11f),
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 56,
                BackColor = BgCard,
                ForeColor = TextDark,
                SelectionMode = SelectionMode.One,
            };
            _lstResidents.DrawItem += DrawResidentItem;
            _lstResidents.DoubleClick += (s, e) => AcceptSelection();
            _lstResidents.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) AcceptSelection(); };
            this.Controls.Add(_lstResidents);

            var btnView = MakeBtn("View profile", BtnBlue);
            btnView.Location = new Point(16, 522);
            btnView.Size = new Size(190, 34);
            btnView.Click += (s, e) => AcceptSelection();
            this.Controls.Add(btnView);

            var btnCancel = MakeBtn("Close", BtnGray);
            btnCancel.Location = new Point(214, 522);
            btnCancel.Size = new Size(190, 34);
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);
        }

        private Button GenderChip(Panel row, string text, ref int x)
        {
            int w = text.Length * 8 + 20;
            var b = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 7.5f),
                Location = new Point(x, 0),
                Size = new Size(w, 22),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = NavBlueChip,
                ForeColor = Color.FromArgb(225, 230, 250),
            };
            b.FlatAppearance.BorderSize = 0;
            row.Controls.Add(b);
            x += w + 4;
            return b;
        }

        private void SetActiveChip(Button active)
        {
            foreach (var c in new[] { _chipAll, _chipMale, _chipFemale, _chipSenior })
            {
                if (c == null) continue;
                bool isActive = c == active;
                c.BackColor = isActive ? Color.White : NavBlueChip;
                c.ForeColor = isActive ? NavBlue : Color.FromArgb(225, 230, 250);
            }
        }

        private static void SetPlaceholder(TextBox tb, string placeholder)
        {
            tb.Text = placeholder;
            tb.ForeColor = TextGray;
            tb.Tag = true;
            tb.Enter += (s, e) =>
            {
                if (tb.Tag is bool ph && ph)
                {
                    tb.Text = "";
                    tb.ForeColor = TextDark;
                    tb.Tag = false;
                }
            };
            tb.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.Text = placeholder;
                    tb.ForeColor = TextGray;
                    tb.Tag = true;
                }
            };
        }

        private void AcceptSelection()
        {
            if (_lstResidents.SelectedItem is DataRow row)
            {
                SelectedResidentId = Convert.ToInt32(row["idResident"]);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void OpenAddResident()
        {
            try
            {
                var f = new ResidentsForm();
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    AuditLogger.Log("ADD", "Residents", "Added new resident");
                    LoadResidentList();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void LoadResidentList()
        {
            var conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    // FIXED: SQLite uses || for concatenation, CASE WHEN instead of IIF,
                    // and julianday() arithmetic instead of TIMESTAMPDIFF/CURDATE.
                    string q = @"
                        SELECT idResident,
                               (LastName || ', ' || FirstName ||
                                   CASE WHEN MiddleName IS NOT NULL AND MiddleName != ''
                                        THEN ' ' || MiddleName ELSE '' END) AS FullName,
                               CAST((julianday('now') - julianday(DateOfBirth)) / 365.25 AS INTEGER) AS Age,
                               Gender, Address, Photo
                        FROM resident
                        WHERE IsActive=1
                        ORDER BY LastName, FirstName";

                    var da = new SQLiteDataAdapter(q, conn);
                    _dtResidents = new DataTable();
                    da.Fill(_dtResidents);

                    foreach (var img in _listPhotoCache.Values) img?.Dispose();
                    _listPhotoCache.Clear();
                    foreach (DataRow row in _dtResidents.Rows)
                    {
                        if (row["Photo"] != DBNull.Value)
                        {
                            try
                            {
                                var bytes = (byte[])row["Photo"];
                                using (var ms = new MemoryStream(bytes))
                                using (var src = Image.FromStream(ms))
                                using (var squared = CropToSquare(src))
                                {
                                    var thumb = new Bitmap(32, 32, PixelFormat.Format32bppArgb);
                                    using (var g = Graphics.FromImage(thumb))
                                    {
                                        g.SmoothingMode = SmoothingMode.AntiAlias;
                                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                        using (var clip = new GraphicsPath())
                                        { clip.AddEllipse(0, 0, 32, 32); g.SetClip(clip); }
                                        g.DrawImage(squared, 0, 0, 32, 32);
                                    }
                                    _listPhotoCache[Convert.ToInt32(row["idResident"])] = thumb;
                                }
                            }
                            catch { }
                        }
                    }

                    RefreshFilteredList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading residents:\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void RefreshFilteredList()
        {
            if (_dtResidents == null) return;

            string search = "";
            if (_txtSearch != null && !(_txtSearch.Tag is bool ph && ph))
                search = _txtSearch.Text.Trim().ToLower();

            _lstResidents.Items.Clear();
            foreach (DataRow row in _dtResidents.Rows)
            {
                string name = row["FullName"]?.ToString()?.ToLower() ?? "";
                string address = row["Address"]?.ToString()?.ToLower() ?? "";
                string gender = row["Gender"]?.ToString() ?? "";
                int age = row["Age"] != DBNull.Value ? Convert.ToInt32(row["Age"]) : 0;

                bool matchesSearch = string.IsNullOrWhiteSpace(search)
                    || name.Contains(search) || address.Contains(search);

                bool matchesChip;
                switch (_currentGenderFilter)
                {
                    case "Male": matchesChip = gender.Equals("Male", StringComparison.OrdinalIgnoreCase); break;
                    case "Female": matchesChip = gender.Equals("Female", StringComparison.OrdinalIgnoreCase); break;
                    case "Senior": matchesChip = age >= 60; break;
                    default: matchesChip = true; break;
                }

                if (matchesSearch && matchesChip) _lstResidents.Items.Add(row);
            }

            _lblCount.Text = _lstResidents.Items.Count > 0 ? $"{_lstResidents.Items.Count} residents" : "No matches";
        }

        private void DrawResidentItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _lstResidents.Items.Count) return;
            var row = _lstResidents.Items[e.Index] as DataRow;
            if (row == null) return;

            string name = row["FullName"]?.ToString() ?? "";
            int resId = Convert.ToInt32(row["idResident"]);
            bool selected = (e.State & DrawItemState.Selected) != 0;

            Color bg = selected ? ActiveRow : (e.Index % 2 == 0 ? BgCard : Color.FromArgb(248, 249, 255));
            using (var b = new SolidBrush(bg))
                e.Graphics.FillRectangle(b, e.Bounds);

            if (selected)
                using (var b = new SolidBrush(NavBlue))
                    e.Graphics.FillRectangle(b, e.Bounds.X, e.Bounds.Y, 4, e.Bounds.Height);

            var avatarRect = new Rectangle(e.Bounds.X + 12, e.Bounds.Y + 12, 32, 32);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            if (_listPhotoCache.TryGetValue(resId, out Image thumb) && thumb != null)
            {
                using (var clip = new GraphicsPath())
                { clip.AddEllipse(avatarRect); e.Graphics.SetClip(clip); }
                e.Graphics.DrawImage(thumb, avatarRect);
                e.Graphics.ResetClip();
                using (var pen = new Pen(selected ? NavBlue : Color.FromArgb(180, 190, 210), 1.5f))
                    e.Graphics.DrawEllipse(pen, avatarRect);
            }
            else
            {
                using (var b = new SolidBrush(selected ? NavBlue : Color.FromArgb(197, 202, 233)))
                    e.Graphics.FillEllipse(b, avatarRect);
                string initials = GetInitials(name);
                using (var f = new Font("Segoe UI", 10f, FontStyle.Bold))
                using (var sb = new SolidBrush(selected ? Color.White : NavBlue))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    e.Graphics.DrawString(initials, f, sb, avatarRect, sf);
                }
            }

            using (var f = new Font("Segoe UI", 11f, FontStyle.Bold))
            using (var sb = new SolidBrush(selected ? NavBlue : TextDark))
                e.Graphics.DrawString(name, f, sb,
                    new RectangleF(e.Bounds.X + 56, e.Bounds.Y + 17, e.Bounds.Width - 68, 22));

            using (var pen = new Pen(BorderCol, 0.5f))
                e.Graphics.DrawLine(pen, e.Bounds.X + 12, e.Bounds.Bottom - 1, e.Bounds.Right - 12, e.Bounds.Bottom - 1);
        }

        private static Bitmap CropToSquare(Image src)
        {
            int size = Math.Min(src.Width, src.Height);
            int x = (src.Width - size) / 2;
            int y = (src.Height - size) / 2;
            var square = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(square))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(src, new Rectangle(0, 0, size, size), new Rectangle(x, y, size, size), GraphicsUnit.Pixel);
            }
            return square;
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2) return ("" + parts[0][0] + parts[1][0]).ToUpper();
            return ("" + parts[0][0]).ToUpper();
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
                Math.Max(0, bg.R - 22), Math.Max(0, bg.G - 22), Math.Max(0, bg.B - 22));
            return b;
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  RESIDENT PROFILE CONTROL
    // ══════════════════════════════════════════════════════════════════════════
    public class ResidentProfileControl : UserControl
    {
        private static readonly Color NavBlue = Color.FromArgb(26, 35, 126);
        private static readonly Color NavBlueLight = Color.FromArgb(57, 73, 171);
        private static readonly Color BgPage = Color.FromArgb(240, 242, 248);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderCol = Color.FromArgb(220, 225, 238);
        private static readonly Color TextDark = Color.FromArgb(28, 30, 44);
        private static readonly Color TextGray = Color.FromArgb(108, 114, 140);
        private static readonly Color TextMuted = Color.FromArgb(136, 135, 128);

        private static readonly Color BtnGreen = Color.FromArgb(15, 157, 88);
        private static readonly Color BtnRed = Color.FromArgb(192, 57, 43);
        private static readonly Color BtnBlue = Color.FromArgb(66, 133, 244);

        private static readonly Color ClearanceBg = Color.FromArgb(230, 241, 251);
        private static readonly Color ClearanceFg = Color.FromArgb(12, 68, 124);
        private static readonly Color CertificateBg = Color.FromArgb(234, 243, 222);
        private static readonly Color CertificateFg = Color.FromArgb(39, 80, 10);
        private static readonly Color BlotterBg = Color.FromArgb(250, 238, 218);
        private static readonly Color BlotterFg = Color.FromArgb(99, 56, 6);
        private static readonly Color AyudaBg = Color.FromArgb(225, 245, 238);
        private static readonly Color AyudaFg = Color.FromArgb(8, 80, 65);
        private static readonly Color CedulaBg = Color.FromArgb(238, 237, 254);
        private static readonly Color CedulaFg = Color.FromArgb(60, 52, 137);
        private static readonly Color DeleteBg = Color.FromArgb(252, 235, 235);
        private static readonly Color DeleteFg = Color.FromArgb(121, 31, 31);
        private static readonly Color SuccessBg = Color.FromArgb(234, 243, 222);
        private static readonly Color SuccessFg = Color.FromArgb(39, 80, 10);
        private static readonly Color PendingBg = Color.FromArgb(250, 236, 231);
        private static readonly Color PendingFg = Color.FromArgb(113, 43, 19);

        private readonly int _residentId;
        private readonly Action _onClose;
        private string _currentFilter = "All";
        private int _selectedActivityId = 0;

        private Panel _actionBar;
        private Panel _infoPanel, _activityPanel;

        private Label _lblHeaderTitle;
        private PictureBox _pbAvatarBig;
        private Label _lblBigName, _lblBigAddress, _lblBadgeActive, _lblBadgeAge;
        private Label _infoGender, _infoCivil, _infoOcc, _infoBirth, _infoContact, _infoAddr;
        private Label _infoPlaceOfBirth, _infoCitizenship, _infoHeight, _infoWeight, _infoTIN, _infoICR;
        private string _avatarInitials = "?";

        private Panel _badgesContainer;
        private Panel _emergencyCard;
        private Label _lblEmergencyName, _lblEmergencyPhone;
        private Label _statClearanceNum, _statAyudaNum, _statSinceNum;

        private DataGridView _dgvActivity;
        private Button _pillAll, _pillClearance, _pillCert, _pillBlotter, _pillAyuda;

        private Action _layoutInfoBody;

        public ResidentProfileControl(int residentId, Action onClose)
        {
            _residentId = residentId;
            _onClose = onClose;

            this.Dock = DockStyle.Fill;
            this.BackColor = BgPage;

            BuildUI();
            LoadResidentProfile(_residentId);
            SetPill(_pillAll);
            LoadActivity("All");
        }

        private void BuildUI()
        {
            var header = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = NavBlue };
            _lblHeaderTitle = new Label
            {
                Text = "👤  Resident Profile",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(16, 14),
                BackColor = Color.Transparent,
            };
            header.Controls.Add(_lblHeaderTitle);
            var btnClose = new Button
            {
                Text = "✕  Close",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Size = new Size(90, 34),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(120, 255, 255, 255);
            btnClose.FlatAppearance.BorderSize = 1;
            btnClose.Click += (s, e) => _onClose?.Invoke();
            header.Controls.Add(btnClose);
            header.Resize += (s, e) => btnClose.Location = new Point(header.Width - 106, 9);

            _actionBar = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = BgCard, Padding = new Padding(16, 8, 16, 8) };
            _actionBar.Paint += (s, e) =>
            {
                using (var pen = new Pen(BorderCol))
                    e.Graphics.DrawLine(pen, 0, _actionBar.Height - 1, _actionBar.Width, _actionBar.Height - 1);
            };

            var iconButtons = new List<Button>();
            var bClearance = IconPill("🛡", "Clearance", ClearanceBg, ClearanceFg);
            bClearance.Click += (s, e) => AddClearance();
            _actionBar.Controls.Add(bClearance); iconButtons.Add(bClearance);

            var bCertificate = IconPill("📜", "Certificate", CertificateBg, CertificateFg);
            bCertificate.Click += (s, e) => AddCertificate();
            _actionBar.Controls.Add(bCertificate); iconButtons.Add(bCertificate);

            var bBlotter = IconPill("⚠", "Blotter", BlotterBg, BlotterFg);
            bBlotter.Click += (s, e) => AddBlotter();
            _actionBar.Controls.Add(bBlotter); iconButtons.Add(bBlotter);

            var bAyuda = IconPill("🎁", "Ayuda", AyudaBg, AyudaFg);
            bAyuda.Click += (s, e) => AddAyuda();
            _actionBar.Controls.Add(bAyuda); iconButtons.Add(bAyuda);

            var bCedula = IconPill("🪪", "Cedula", CedulaBg, CedulaFg);
            bCedula.Click += (s, e) => AddCedula();
            _actionBar.Controls.Add(bCedula); iconButtons.Add(bCedula);

            void LayoutActionBar()
            {
                int x = 0;
                foreach (var b in iconButtons) { b.Location = new Point(x, 10); x += b.Width + 6; }
            }
            LayoutActionBar();

            var main = new Panel { Dock = DockStyle.Fill, BackColor = BgPage, Padding = new Padding(16) };

            BuildInfoPanel();
            BuildActivityPanel();
            main.Controls.Add(_infoPanel);
            main.Controls.Add(_activityPanel);

            main.Resize += (s, e) =>
            {
                int h = main.ClientSize.Height - main.Padding.Vertical;
                int w = main.ClientSize.Width - main.Padding.Horizontal;
                if (w < 200) return;
                int gap = 12;
                int infoW = (int)(w * 0.42);
                int actW = w - infoW - gap;
                _infoPanel.Size = new Size(infoW, h);
                _infoPanel.Location = new Point(main.Padding.Left, main.Padding.Top);
                _activityPanel.Size = new Size(actW, h);
                _activityPanel.Location = new Point(main.Padding.Left + infoW + gap, main.Padding.Top);
            };

            this.Controls.Add(main);
            this.Controls.Add(_actionBar);
            this.Controls.Add(header);
        }

        private void BuildInfoPanel()
        {
            _infoPanel = new Panel { BackColor = BgCard, AutoScroll = true };
            _infoPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(BorderCol))
                    DrawRR(e.Graphics, pen, new Rectangle(0, 0, _infoPanel.Width - 1, _infoPanel.Height - 1), 8);
            };

            var btnEdit = IconPillSmall("✎", "Edit", ClearanceBg, ClearanceFg);
            btnEdit.Click += (s, e) => OpenEditProfileDialog();
            var btnQR = IconPillSmall("▦", "QR", CedulaBg, CedulaFg);
            btnQR.Click += (s, e) => PrintQRCode();
            var btnDel = IconPillSmall("🗑", "Delete", DeleteBg, DeleteFg);
            btnDel.Click += (s, e) => DeleteResident();
            _infoPanel.Controls.AddRange(new Control[] { btnEdit, btnQR, btnDel });

            void LayoutActionIcons()
            {
                int x = _infoPanel.ClientSize.Width - 14;
                foreach (var b in new[] { btnDel, btnQR, btnEdit })
                {
                    x -= b.Width;
                    b.Location = new Point(x, 22);
                    x -= 6;
                }
            }
            _infoPanel.Resize += (s, e) => LayoutActionIcons();

            _pbAvatarBig = new PictureBox { Size = new Size(96, 96), SizeMode = PictureBoxSizeMode.StretchImage };
            _pbAvatarBig.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                if (_pbAvatarBig.Image == null)
                {
                    using (var brush = new LinearGradientBrush(
                        new Rectangle(0, 0, _pbAvatarBig.Width, _pbAvatarBig.Height),
                        NavBlueLight, NavBlue, 45f))
                        e.Graphics.FillEllipse(brush, 0, 0, _pbAvatarBig.Width, _pbAvatarBig.Height);
                    using (var f = new Font("Segoe UI", 28f, FontStyle.Bold))
                    using (var sb = new SolidBrush(Color.White))
                    {
                        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        e.Graphics.DrawString(_avatarInitials, f, sb, new RectangleF(0, 0, _pbAvatarBig.Width, _pbAvatarBig.Height), sf);
                    }
                }
                using (var pen = new Pen(Color.FromArgb(225, 230, 250), 3f))
                    e.Graphics.DrawEllipse(pen, 1, 1, _pbAvatarBig.Width - 3, _pbAvatarBig.Height - 3);
            };
            var avatarPath = new GraphicsPath();
            avatarPath.AddEllipse(0, 0, 96, 96);
            _pbAvatarBig.Region = new Region(avatarPath);
            _infoPanel.Controls.Add(_pbAvatarBig);

            _lblBigName = new Label { Text = "", Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = NavBlue, AutoSize = true, BackColor = Color.Transparent };
            _infoPanel.Controls.Add(_lblBigName);

            _lblBigAddress = new Label { Text = "", Font = new Font("Segoe UI", 9.5f), ForeColor = TextGray, AutoSize = true, BackColor = Color.Transparent };
            _infoPanel.Controls.Add(_lblBigAddress);

            _lblBadgeActive = MakeBadge("Active resident", SuccessBg, SuccessFg);
            _lblBadgeAge = MakeBadge("Age —", ClearanceBg, ClearanceFg);
            _infoPanel.Controls.Add(_lblBadgeActive);
            _infoPanel.Controls.Add(_lblBadgeAge);

            var divider = new Panel { Height = 1, BackColor = BorderCol };
            _infoPanel.Controls.Add(divider);

            var keys = new[] { "Gender", "Civil Status", "Occupation", "Birth Date" };
            var keyLabels = new Label[4];
            var valueLabels = new Label[4];
            for (int i = 0; i < 4; i++)
            {
                keyLabels[i] = InfoKey(keys[i]);
                valueLabels[i] = InfoValue();
                _infoPanel.Controls.Add(keyLabels[i]);
                _infoPanel.Controls.Add(valueLabels[i]);
            }
            _infoGender = valueLabels[0];
            _infoCivil = valueLabels[1];
            _infoOcc = valueLabels[2];
            _infoBirth = valueLabels[3];

            var keyContact = InfoKey("Contact Number");
            _infoContact = InfoValue();
            var keyAddr = InfoKey("Full Address");
            _infoAddr = InfoValue();
            _infoPanel.Controls.AddRange(new Control[] { keyContact, _infoContact, keyAddr, _infoAddr });

            var dividerExtra = new Panel { Height = 1, BackColor = BorderCol };
            var lblExtraTitle = SectionTitle("Additional Information");
            var extraKeys = new[] { "Place of Birth", "Citizenship", "Height", "Weight", "TIN", "ICR Number" };
            var extraKeyLabels = new Label[6];
            var extraValueLabels = new Label[6];
            for (int i = 0; i < 6; i++)
            {
                extraKeyLabels[i] = InfoKey(extraKeys[i]);
                extraValueLabels[i] = InfoValue();
                _infoPanel.Controls.Add(extraKeyLabels[i]);
                _infoPanel.Controls.Add(extraValueLabels[i]);
            }
            _infoPanel.Controls.AddRange(new Control[] { dividerExtra, lblExtraTitle });
            _infoPlaceOfBirth = extraValueLabels[0];
            _infoCitizenship = extraValueLabels[1];
            _infoHeight = extraValueLabels[2];
            _infoWeight = extraValueLabels[3];
            _infoTIN = extraValueLabels[4];
            _infoICR = extraValueLabels[5];

            var divider2 = new Panel { Height = 1, BackColor = BorderCol };
            var lblRegTitle = SectionTitle("Registration & Status");
            _badgesContainer = new Panel { BackColor = Color.Transparent, Height = 24 };
            _infoPanel.Controls.AddRange(new Control[] { divider2, lblRegTitle, _badgesContainer });

            var divider3 = new Panel { Height = 1, BackColor = BorderCol };
            var lblEmergencyTitle = SectionTitle("Emergency Contact");
            _emergencyCard = new Panel { Height = 48 };
            _emergencyCard.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var b = new SolidBrush(Color.FromArgb(247, 248, 252)))
                using (var path = RoundPath(new Rectangle(0, 0, _emergencyCard.Width - 1, _emergencyCard.Height - 1), 8))
                    e.Graphics.FillPath(b, path);
            };
            var iconCircle = new Panel { Size = new Size(32, 32), Location = new Point(8, 8) };
            iconCircle.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var b = new SolidBrush(Color.FromArgb(197, 202, 233)))
                    e.Graphics.FillEllipse(b, 0, 0, iconCircle.Width, iconCircle.Height);
                using (var f = new Font("Segoe UI", 9f))
                using (var sb = new SolidBrush(NavBlue))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    e.Graphics.DrawString("☎", f, sb, new RectangleF(0, 0, iconCircle.Width, iconCircle.Height), sf);
                }
            };
            _emergencyCard.Controls.Add(iconCircle);
            _lblEmergencyName = new Label { Text = "No emergency contact on file", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = TextDark, AutoSize = true, Location = new Point(50, 9), BackColor = Color.Transparent };
            _lblEmergencyPhone = new Label { Text = "", Font = new Font("Segoe UI", 8.5f), ForeColor = TextGray, AutoSize = true, Location = new Point(50, 27), BackColor = Color.Transparent };
            _emergencyCard.Controls.Add(_lblEmergencyName);
            _emergencyCard.Controls.Add(_lblEmergencyPhone);
            _infoPanel.Controls.AddRange(new Control[] { divider3, lblEmergencyTitle, _emergencyCard });

            var divider4 = new Panel { Height = 1, BackColor = BorderCol };
            var lblStatsTitle = SectionTitle("Quick Stats");
            var statClearanceBox = MakeStatBox(ClearanceBg, ClearanceFg, "Clearances issued", out _statClearanceNum);
            var statAyudaBox = MakeStatBox(AyudaBg, AyudaFg, "Ayuda received", out _statAyudaNum);
            var statSinceBox = MakeStatBox(BlotterBg, BlotterFg, "Resident since", out _statSinceNum);
            _infoPanel.Controls.AddRange(new Control[] { divider4, lblStatsTitle, statClearanceBox, statAyudaBox, statSinceBox });

            void LayoutInfoBody()
            {
                int w = Math.Max(_infoPanel.ClientSize.Width, 220);
                int cx = w / 2;
                _pbAvatarBig.Location = new Point(cx - 48, 68);
                _lblBigName.Location = new Point(cx - _lblBigName.Width / 2, 170);
                _lblBigAddress.Location = new Point(cx - _lblBigAddress.Width / 2, 196);

                int badgeY = 216;
                int badgesW = _lblBadgeActive.Width + _lblBadgeAge.Width + 6;
                _lblBadgeActive.Location = new Point(cx - badgesW / 2, badgeY);
                _lblBadgeAge.Location = new Point(_lblBadgeActive.Right + 6, badgeY);

                int gridTop = 254;
                divider.Location = new Point(14, gridTop);
                divider.Width = w - 28;

                int margin = 14, gap = 16, colGap = 18;
                int colW = (w - margin * 2 - colGap) / 2;
                int rowH = 40;
                int y = gridTop + 12;
                for (int i = 0; i < 4; i++)
                {
                    int col = i % 2, row = i / 2;
                    int gx = margin + col * (colW + colGap);
                    int gy = y + row * rowH;
                    keyLabels[i].Location = new Point(gx, gy);
                    valueLabels[i].Location = new Point(gx, gy + 15);
                    valueLabels[i].Size = new Size(colW, 20);
                }
                y += 2 * rowH + gap;
                keyContact.Location = new Point(margin, y);
                _infoContact.Location = new Point(margin, y + 15);
                _infoContact.Size = new Size(w - margin * 2, 20);
                y += rowH;
                keyAddr.Location = new Point(margin, y);
                _infoAddr.Location = new Point(margin, y + 15);
                _infoAddr.Size = new Size(w - margin * 2, 36);
                y += rowH + 20;

                dividerExtra.Location = new Point(margin, y);
                dividerExtra.Width = w - margin * 2;
                y += 12;
                lblExtraTitle.Location = new Point(margin, y);
                y += 18;
                for (int i = 0; i < 6; i++)
                {
                    int col = i % 2, row = i / 2;
                    int gx = margin + col * (colW + colGap);
                    int gy = y + row * rowH;
                    extraKeyLabels[i].Location = new Point(gx, gy);
                    extraValueLabels[i].Location = new Point(gx, gy + 15);
                    extraValueLabels[i].Size = new Size(colW, 20);
                }
                y += 3 * rowH + 20;

                divider2.Location = new Point(margin, y);
                divider2.Width = w - margin * 2;
                y += 12;
                lblRegTitle.Location = new Point(margin, y);
                y += 18;
                _badgesContainer.Location = new Point(margin, y);
                _badgesContainer.Width = w - margin * 2;
                y += _badgesContainer.Height + 20;

                divider3.Location = new Point(margin, y);
                divider3.Width = w - margin * 2;
                y += 12;
                lblEmergencyTitle.Location = new Point(margin, y);
                y += 18;
                _emergencyCard.Location = new Point(margin, y);
                _emergencyCard.Width = w - margin * 2;
                y += _emergencyCard.Height + 20;

                divider4.Location = new Point(margin, y);
                divider4.Width = w - margin * 2;
                y += 12;
                lblStatsTitle.Location = new Point(margin, y);
                y += 18;
                int statGap = 8;
                int statW = (w - margin * 2 - statGap * 2) / 3;
                int statH = 46;
                statClearanceBox.Location = new Point(margin, y);
                statClearanceBox.Size = new Size(statW, statH);
                statAyudaBox.Location = new Point(margin + statW + statGap, y);
                statAyudaBox.Size = new Size(statW, statH);
                statSinceBox.Location = new Point(margin + 2 * (statW + statGap), y);
                statSinceBox.Size = new Size(statW, statH);
                y += statH + 20;

                _infoPanel.AutoScrollMinSize = new Size(0, y);
            }
            _infoPanel.Resize += (s, e) => LayoutInfoBody();
            _layoutInfoBody = LayoutInfoBody;
        }

        private void RebuildBadges(List<(string text, Color bg, Color fg)> chips)
        {
            _badgesContainer.Controls.Clear();
            if (chips.Count == 0)
            {
                _badgesContainer.Controls.Add(new Label
                {
                    Text = "No registration info on file",
                    Font = new Font("Segoe UI", 8.5f),
                    ForeColor = TextMuted,
                    AutoSize = true,
                    BackColor = Color.Transparent,
                });
                _badgesContainer.Height = 20;
                return;
            }
            int x = 0;
            foreach (var c in chips)
            {
                var chip = MakeBadge(c.text, c.bg, c.fg);
                chip.Location = new Point(x, 0);
                _badgesContainer.Controls.Add(chip);
                x += chip.Width + 6;
            }
            _badgesContainer.Height = 24;
        }

        private Label SectionTitle(string text) => new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            ForeColor = Color.FromArgb(127, 119, 221),
            AutoSize = true,
            BackColor = Color.Transparent,
        };

        private Panel MakeStatBox(Color bg, Color fg, string caption, out Label numLabel)
        {
            var p = new Panel { BackColor = Color.Transparent };
            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var b = new SolidBrush(bg))
                using (var path = RoundPath(new Rectangle(0, 0, p.Width - 1, p.Height - 1), 8))
                    e.Graphics.FillPath(b, path);
            };
            var num = new Label { Text = "—", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = fg, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent, Dock = DockStyle.Top, Height = 24 };
            var cap = new Label { Text = caption, Font = new Font("Segoe UI", 7f), ForeColor = fg, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent, Dock = DockStyle.Fill };
            p.Controls.Add(cap);
            p.Controls.Add(num);
            numLabel = num;
            return p;
        }

        private Label InfoKey(string text) => new Label { Text = text, Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = TextMuted, AutoSize = true, BackColor = Color.Transparent };

        private Label InfoValue() => new Label { Text = "—", Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = TextDark, AutoSize = false, BackColor = Color.Transparent };

        private Label MakeBadge(string text, Color bg, Color fg)
        {
            var l = new Label { Text = text, Font = new Font("Segoe UI", 8f, FontStyle.Bold), BackColor = bg, ForeColor = fg, AutoSize = true, Padding = new Padding(8, 3, 8, 3) };
            l.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var b = new SolidBrush(bg))
                using (var path = RoundPath(new Rectangle(0, 0, l.Width - 1, l.Height - 1), l.Height / 2))
                    e.Graphics.FillPath(b, path);
                TextRenderer.DrawText(e.Graphics, text, l.Font, l.ClientRectangle, fg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            return l;
        }

        private void BuildActivityPanel()
        {
            _activityPanel = new Panel { BackColor = BgCard };
            _activityPanel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(BorderCol))
                    DrawRR(e.Graphics, pen, new Rectangle(0, 0, _activityPanel.Width - 1, _activityPanel.Height - 1), 8);
            };

            var lblTitle = new Label { Text = "Recent Activity", Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), ForeColor = NavBlue, AutoSize = true, Location = new Point(14, 26), BackColor = Color.Transparent };
            _activityPanel.Controls.Add(lblTitle);

            int px = 14;
            _pillAll = Pill("All", px, 58); px += _pillAll.Width + 6;
            _pillClearance = Pill("Clearance", px, 58); px += _pillClearance.Width + 6;
            _pillCert = Pill("Certificate", px, 58); px += _pillCert.Width + 6;
            _pillBlotter = Pill("Blotter", px, 58); px += _pillBlotter.Width + 6;
            _pillAyuda = Pill("Ayuda", px, 58);
            _activityPanel.Controls.AddRange(new Control[] { _pillAll, _pillClearance, _pillCert, _pillBlotter, _pillAyuda });

            _pillAll.Click += (s, e) => { SetPill(_pillAll); LoadActivity("All"); };
            _pillClearance.Click += (s, e) => { SetPill(_pillClearance); LoadActivity("Clearance"); };
            _pillCert.Click += (s, e) => { SetPill(_pillCert); LoadActivity("Certificate"); };
            _pillBlotter.Click += (s, e) => { SetPill(_pillBlotter); LoadActivity("Blotter"); };
            _pillAyuda.Click += (s, e) => { SetPill(_pillAyuda); LoadActivity("Ayuda"); };

            var divider = new Panel { Location = new Point(0, 94), Height = 1, BackColor = BorderCol };
            _activityPanel.Controls.Add(divider);

            _dgvActivity = MakeGrid();
            _dgvActivity.Location = new Point(8, 102);
            _activityPanel.Controls.Add(_dgvActivity);

            var bU = MakeBtn("↻ Update", BtnBlue, 8.5f); bU.Size = new Size(88, 28);
            var bD = MakeBtn("🗑 Delete", BtnRed, 8.5f); bD.Size = new Size(88, 28);
            bU.Click += (s, e) => UpdateActivity();
            bD.Click += (s, e) => DeleteActivity();
            _activityPanel.Controls.AddRange(new Control[] { bU, bD });

            void LayoutActivity()
            {
                int w = Math.Max(_activityPanel.ClientSize.Width, 260);
                int h = _activityPanel.ClientSize.Height;
                divider.Width = w;
                int btnsH = 40;
                _dgvActivity.Width = w - 16;
                _dgvActivity.Height = Math.Max(120, h - 102 - btnsH);
                bU.Location = new Point(14, _dgvActivity.Bottom + 8);
                bD.Location = new Point(14 + bU.Width + 8, _dgvActivity.Bottom + 8);
            }
            _activityPanel.Resize += (s, e) => LayoutActivity();
        }

        private void LoadResidentProfile(int id)
        {
            var conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand("SELECT * FROM resident WHERE idResident=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    var r = cmd.ExecuteReader();
                    if (!r.Read()) { r.Close(); return; }

                    string fn = r["FirstName"]?.ToString() ?? "";
                    string mn = r["MiddleName"]?.ToString() ?? "";
                    string ln = r["LastName"]?.ToString() ?? "";
                    string fullName = $"{fn} {mn} {ln}".Trim().Replace("  ", " ");

                    DateTime dob = r["DateOfBirth"] != DBNull.Value ? Convert.ToDateTime(r["DateOfBirth"]) : DateTime.Now.AddYears(-18);
                    int age = DateTime.Now.Year - dob.Year;
                    if (DateTime.Now.DayOfYear < dob.DayOfYear) age--;

                    string gender = r["Gender"]?.ToString() ?? "N/A";
                    string contact = DecryptForDisplay(r["ContactNumber"]);
                    string address = DecryptForDisplay(r["Address"]);
                    if (string.IsNullOrWhiteSpace(contact)) contact = "N/A";
                    if (string.IsNullOrWhiteSpace(address)) address = "N/A";
                    string civil = "Single";
                    string occ = "";
                    try { civil = r["CivilStatus"]?.ToString() ?? "Single"; } catch { }
                    try { occ = r["Occupation"]?.ToString() ?? ""; } catch { }

                    _avatarInitials = GetInitials(fn + " " + ln);
                    _lblBigName.Text = fullName;
                    _lblHeaderTitle.Text = "👤  " + fullName;
                    _lblBigAddress.Text = address;
                    _lblBadgeAge.Text = $"Age {age}";
                    _pbAvatarBig.Image = null;

                    _infoGender.Text = gender;
                    _infoCivil.Text = civil;
                    _infoOcc.Text = string.IsNullOrWhiteSpace(occ) ? "—" : occ;
                    _infoBirth.Text = dob.ToString("MMM dd, yyyy");
                    _infoContact.Text = contact;
                    _infoAddr.Text = address;

                    try { _infoPlaceOfBirth.Text = string.IsNullOrWhiteSpace(r["PlaceOfBirth"]?.ToString()) ? "—" : r["PlaceOfBirth"].ToString(); } catch { _infoPlaceOfBirth.Text = "—"; }
                    try { _infoCitizenship.Text = string.IsNullOrWhiteSpace(r["Citizenship"]?.ToString()) ? "Filipino" : r["Citizenship"].ToString(); } catch { _infoCitizenship.Text = "Filipino"; }
                    try { _infoHeight.Text = string.IsNullOrWhiteSpace(r["Height"]?.ToString()) ? "—" : r["Height"].ToString(); } catch { _infoHeight.Text = "—"; }
                    try { _infoWeight.Text = string.IsNullOrWhiteSpace(r["Weight"]?.ToString()) ? "—" : r["Weight"].ToString(); } catch { _infoWeight.Text = "—"; }
                    try
                    {
                        string tin = DecryptForDisplay(r["TIN"]);
                        _infoTIN.Text = string.IsNullOrWhiteSpace(tin) ? "—" : tin;
                    }
                    catch { _infoTIN.Text = "—"; }
                    try { _infoICR.Text = string.IsNullOrWhiteSpace(r["ICRNumber"]?.ToString()) ? "—" : r["ICRNumber"].ToString(); } catch { _infoICR.Text = "—"; }

                    byte[] photoBytes = r["Photo"] != DBNull.Value ? (byte[])r["Photo"] : null;
                    if (photoBytes != null)
                    {
                        using (var ms1 = new MemoryStream(photoBytes))
                        using (var src = Image.FromStream(ms1))
                            _pbAvatarBig.Image = CropToSquare(src);
                    }

                    bool isVoter = false, is4Ps = false;
                    string household = "";
                    try { isVoter = r["IsRegisteredVoter"] != DBNull.Value && Convert.ToBoolean(r["IsRegisteredVoter"]); } catch { }
                    try { is4Ps = r["Is4PsBeneficiary"] != DBNull.Value && Convert.ToBoolean(r["Is4PsBeneficiary"]); } catch { }
                    try { household = r["HouseholdNumber"]?.ToString() ?? ""; } catch { }

                    var chips = new List<(string, Color, Color)>();
                    if (isVoter) chips.Add(("Registered voter", CedulaBg, CedulaFg));
                    if (is4Ps) chips.Add(("4Ps beneficiary", AyudaBg, AyudaFg));
                    if (!string.IsNullOrWhiteSpace(household)) chips.Add(($"Household #{household}", Color.FromArgb(241, 239, 232), Color.FromArgb(87, 84, 75)));
                    RebuildBadges(chips);

                    string emName = "", emRelation = "", emPhone = "";
                    try { emName = r["EmergencyContactName"]?.ToString() ?? ""; } catch { }
                    try { emRelation = r["EmergencyContactRelation"]?.ToString() ?? ""; } catch { }
                    try { emPhone = r["EmergencyContactPhone"]?.ToString() ?? ""; } catch { }
                    if (string.IsNullOrWhiteSpace(emName))
                    {
                        _lblEmergencyName.Text = "No emergency contact on file";
                        _lblEmergencyPhone.Text = "";
                    }
                    else
                    {
                        _lblEmergencyName.Text = string.IsNullOrWhiteSpace(emRelation) ? emName : $"{emName} — {emRelation}";
                        _lblEmergencyPhone.Text = emPhone;
                    }

                    try
                    {
                        _statSinceNum.Text = r["DateRegistered"] != DBNull.Value
                            ? Convert.ToDateTime(r["DateRegistered"]).Year.ToString()
                            : "—";
                    }
                    catch { _statSinceNum.Text = "—"; }

                    _pbAvatarBig.Invalidate();
                    _infoPanel.PerformLayout();
                    _layoutInfoBody?.Invoke();
                    r.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading profile:\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            LoadQuickStats(id);
        }

        private void LoadQuickStats(int id)
        {
            var conn = DatabaseConnection.GetConnection();
            if (conn != null)
            {
                using (conn)
                {
                    try
                    {
                        var cmd = new SQLiteCommand("SELECT COUNT(*) FROM clearance WHERE ResidentID=@id", conn);
                        cmd.Parameters.AddWithValue("@id", id);
                        _statClearanceNum.Text = Convert.ToInt32(cmd.ExecuteScalar()).ToString();
                    }
                    catch { _statClearanceNum.Text = "—"; }
                }
            }

            conn = DatabaseConnection.GetConnection();
            if (conn != null)
            {
                using (conn)
                {
                    try
                    {
                        var cmd = new SQLiteCommand("SELECT COALESCE(SUM(Amount),0) FROM ayuda WHERE ResidentID=@id", conn);
                        cmd.Parameters.AddWithValue("@id", id);
                        decimal total = Convert.ToDecimal(cmd.ExecuteScalar());
                        _statAyudaNum.Text = total >= 1000 ? $"₱{(total / 1000m):0.#}K" : $"₱{total:0}";
                    }
                    catch { _statAyudaNum.Text = "—"; }
                }
            }
        }

        private void LoadActivity(string filter)
        {
            _currentFilter = filter;
            _selectedActivityId = 0;
            var conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    var da = new SQLiteDataAdapter(BuildActivitySQL(filter), conn);
                    da.SelectCommand.Parameters.AddWithValue("@id", _residentId);
                    var dt = new DataTable();
                    da.Fill(dt);
                    _dgvActivity.DataSource = dt;

                    if (_dgvActivity.Columns.Contains("RecordID")) _dgvActivity.Columns["RecordID"].Visible = false;
                    if (_dgvActivity.Columns.Contains("Type")) _dgvActivity.Columns["Type"].Width = 88;
                    if (_dgvActivity.Columns.Contains("RefNumber")) { _dgvActivity.Columns["RefNumber"].HeaderText = "Reference"; _dgvActivity.Columns["RefNumber"].Width = 100; }
                    if (_dgvActivity.Columns.Contains("Date")) { _dgvActivity.Columns["Date"].DefaultCellStyle.Format = "MMM dd, yyyy"; _dgvActivity.Columns["Date"].Width = 100; }
                    if (_dgvActivity.Columns.Contains("Details")) _dgvActivity.Columns["Details"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    if (_dgvActivity.Columns.Contains("Status")) _dgvActivity.Columns["Status"].Width = 90;

                    _dgvActivity.CellFormatting -= ColorActivity;
                    _dgvActivity.CellFormatting += ColorActivity;
                    _dgvActivity.CellClick -= OnActivityClick;
                    _dgvActivity.CellClick += OnActivityClick;
                }
                catch { }
            }
        }

        private void OnActivityClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            try { _selectedActivityId = Convert.ToInt32(_dgvActivity.Rows[e.RowIndex].Cells["RecordID"].Value); }
            catch { }
        }

        private void ColorActivity(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_dgvActivity.Columns.Count <= e.ColumnIndex || e.Value == null) return;
            string colName = _dgvActivity.Columns[e.ColumnIndex].Name;

            if (colName == "Type")
            {
                switch (e.Value.ToString())
                {
                    case "Clearance": e.CellStyle.BackColor = ClearanceBg; e.CellStyle.ForeColor = ClearanceFg; break;
                    case "Certificate": e.CellStyle.BackColor = CertificateBg; e.CellStyle.ForeColor = CertificateFg; break;
                    case "Blotter": e.CellStyle.BackColor = BlotterBg; e.CellStyle.ForeColor = BlotterFg; break;
                    case "Ayuda": e.CellStyle.BackColor = AyudaBg; e.CellStyle.ForeColor = AyudaFg; break;
                }
                e.CellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            }
            else if (colName == "Status")
            {
                string v = e.Value.ToString();
                if (v.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                { e.CellStyle.BackColor = PendingBg; e.CellStyle.ForeColor = PendingFg; }
                else
                { e.CellStyle.BackColor = SuccessBg; e.CellStyle.ForeColor = SuccessFg; }
                e.CellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            }
        }

        private string BuildActivitySQL(string filter)
        {
            const string cl = @"SELECT idClearance AS RecordID,'Clearance' AS Type,ClearanceNumber AS RefNumber,DateIssued AS Date,Purpose AS Details,Status FROM clearance WHERE ResidentID=@id";
            const string ct = @"SELECT idCertificates AS RecordID,'Certificate' AS Type,CertificateNumber AS RefNumber,DateIssued AS Date,CertificateType AS Details,Status FROM certificates WHERE ResidentID=@id";
            const string bl = @"SELECT idblotter AS RecordID,'Blotter' AS Type,BlotterNumber AS RefNumber,DateFiled AS Date,IncidentType AS Details,Status FROM blotter WHERE ComplainantID=@id";
            // FIXED: SQLite uses || for concatenation and CAST(ROUND(...)) instead of FORMAT(...)
            const string ay = @"SELECT idAyuda AS RecordID,'Ayuda' AS Type,BarcodeID AS RefNumber,DateReleased AS Date,('₱' || CAST(ROUND(Amount,0) AS INTEGER)) AS Details,Status FROM ayuda WHERE ResidentID=@id";
            const string order = " ORDER BY Date DESC LIMIT 60";
            switch (filter)
            {
                case "Clearance": return cl + order;
                case "Certificate": return ct + order;
                case "Blotter": return bl + order;
                case "Ayuda": return ay + order;
                default: return $"({cl}) UNION ALL ({ct}) UNION ALL ({bl}) UNION ALL ({ay})" + order;
            }
        }

        private static string DecryptForDisplay(object value)
        {
            if (value == null || value == DBNull.Value) return "";
            string original = value.ToString();
            if (string.IsNullOrEmpty(original)) return original;
            try { return CryptoHelper.Decrypt(original, KeyManager.LoadKey()); }
            catch (CryptoException) { return original; }
        }

        private void OpenEditProfileDialog()
        {
            using (var dlg = new EditResidentDialog(_residentId))
            {
                if (dlg.ShowDialog(this.FindForm()) == DialogResult.OK)
                    LoadResidentProfile(_residentId);
            }
        }

        private void DeleteResident()
        {
            if (MessageBox.Show("Set this resident as inactive?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            var conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand("UPDATE resident SET IsActive=0 WHERE idResident=@id", conn);
                    cmd.Parameters.AddWithValue("@id", _residentId);
                    cmd.ExecuteNonQuery();
                    AuditLogger.Log("DELETE", "Residents", $"Deleted resident ID {_residentId}");
                    MessageBox.Show("Resident deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _onClose?.Invoke();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void PrintQRCode()
        {
            new BarcodePrintForm(_residentId, _lblBigName.Text, _lblBigAddress.Text).ShowDialog(this.FindForm());
        }

        private string ResidentName() => _lblBigName.Text;

        private void AddClearance()
        {
            new ClearanceForm(_residentId, ResidentName()).ShowDialog(this.FindForm());
            AuditLogger.Log("ADD", "Clearance", $"Issued clearance for {ResidentName()}");
            LoadActivity(_currentFilter);
        }
        private void AddCertificate()
        {
            new CertificateForm(_residentId, ResidentName()).ShowDialog(this.FindForm());
            AuditLogger.Log("ADD", "Certificate", $"Issued certificate for {ResidentName()}");
            LoadActivity(_currentFilter);
        }
        private void AddBlotter()
        {
            new BlotterForm(_residentId, ResidentName()).ShowDialog(this.FindForm());
            AuditLogger.Log("ADD", "Blotter", $"Filed blotter for {ResidentName()}");
            LoadActivity(_currentFilter);
        }
        private void AddAyuda()
        {
            new AyudaForm(_residentId).ShowDialog(this.FindForm());
            AuditLogger.Log("ADD", "Ayuda", $"Released ayuda for {ResidentName()}");
            LoadActivity(_currentFilter);
        }
        private void AddCedula()
        {
            new CedulaForm(_residentId, ResidentName()).ShowDialog(this.FindForm());
        }

        private void UpdateActivity()
        {
            if (_selectedActivityId == 0)
            {
                MessageBox.Show("Click a row first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            switch (_currentFilter)
            {
                case "Clearance": new ClearanceForm(_residentId, ResidentName(), _selectedActivityId).ShowDialog(this.FindForm()); break;
                case "Certificate": new CertificateForm(_residentId, ResidentName(), _selectedActivityId).ShowDialog(this.FindForm()); break;
                case "Blotter": new BlotterForm(_residentId, ResidentName(), _selectedActivityId).ShowDialog(this.FindForm()); break;
                default:
                    MessageBox.Show("Select a specific filter to update.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
            }
            LoadActivity(_currentFilter);
        }

        private void DeleteActivity()
        {
            if (_selectedActivityId == 0)
            {
                MessageBox.Show("Click a row first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show("Permanently delete this record?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            string table = "", pk = "";
            switch (_currentFilter)
            {
                case "Clearance": table = "clearance"; pk = "idClearance"; break;
                case "Certificate": table = "certificates"; pk = "idCertificates"; break;
                case "Blotter": table = "blotter"; pk = "idblotter"; break;
                case "Ayuda": table = "ayuda"; pk = "idAyuda"; break;
                default:
                    MessageBox.Show("Select a specific filter to delete.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
            }
            var conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand($"DELETE FROM {table} WHERE {pk}=@id", conn);
                    cmd.Parameters.AddWithValue("@id", _selectedActivityId);
                    cmd.ExecuteNonQuery();
                    AuditLogger.Log("DELETE", _currentFilter, $"Deleted record ID {_selectedActivityId}");
                    MessageBox.Show("Record deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadActivity(_currentFilter);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

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

        private Button IconPill(string icon, string label, Color bg, Color fg)
        {
            int textW = TextRenderer.MeasureText(label, new Font("Segoe UI", 8f, FontStyle.Bold)).Width;
            int w = Math.Max(64, textW + 20);
            var b = new Button
            {
                Text = icon + "\n" + label,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                BackColor = bg,
                ForeColor = fg,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(w, 44),
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(
                Math.Max(0, bg.R - 12), Math.Max(0, bg.G - 12), Math.Max(0, bg.B - 12));
            return b;
        }

        private Button IconPillSmall(string icon, string label, Color bg, Color fg)
        {
            var b = new Button
            {
                Text = icon + "\n" + label,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                BackColor = bg,
                ForeColor = fg,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(56, 36),
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(
                Math.Max(0, bg.R - 12), Math.Max(0, bg.G - 12), Math.Max(0, bg.B - 12));
            return b;
        }

        private Button Pill(string text, int x, int y)
        {
            int w = text.Length * 9 + 24;
            var b = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9f),
                Size = new Size(w, 26),
                Location = new Point(x, y),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = Color.FromArgb(228, 232, 248),
                ForeColor = NavBlue,
                UseVisualStyleBackColor = false,
            };
            b.FlatAppearance.BorderColor = Color.FromArgb(180, 190, 220);
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(210, 218, 242);
            return b;
        }

        private void SetPill(Button pill)
        {
            foreach (var p in new[] { _pillAll, _pillClearance, _pillCert, _pillBlotter, _pillAyuda })
            {
                if (p == null) continue;
                p.BackColor = Color.FromArgb(228, 232, 248);
                p.ForeColor = NavBlue;
                p.FlatAppearance.BorderColor = Color.FromArgb(180, 190, 220);
            }
            pill.BackColor = NavBlue;
            pill.ForeColor = Color.White;
            pill.FlatAppearance.BorderColor = NavBlue;
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
                RowTemplate = { Height = 30 },
                Font = new Font("Segoe UI", 8.5f),
                GridColor = Color.FromArgb(224, 228, 242),
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 32,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = NavBlue;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            g.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            g.DefaultCellStyle.SelectionBackColor = BtnBlue;
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 255);
            return g;
        }

        private static Bitmap CropToSquare(Image src)
        {
            int size = Math.Min(src.Width, src.Height);
            int x = (src.Width - size) / 2;
            int y = (src.Height - size) / 2;
            var square = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(square))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(src, new Rectangle(0, 0, size, size), new Rectangle(x, y, size, size), GraphicsUnit.Pixel);
            }
            return square;
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2) return ("" + parts[0][0] + parts[1][0]).ToUpper();
            return ("" + parts[0][0]).ToUpper();
        }

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

        private static void DrawRR(Graphics g, Pen pen, Rectangle r, int radius)
        {
            using (var path = RoundPath(r, radius))
                g.DrawPath(pen, path);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  EDIT RESIDENT DIALOG
    // ══════════════════════════════════════════════════════════════════════════
    public class EditResidentDialog : Form
    {
        private static readonly Color NavBlue = Color.FromArgb(26, 35, 126);
        private static readonly Color BorderCol = Color.FromArgb(220, 225, 238);
        private static readonly Color TextGray = Color.FromArgb(108, 114, 140);
        private static readonly Color BtnGreen = Color.FromArgb(15, 157, 88);
        private static readonly Color BtnGray = Color.FromArgb(100, 110, 128);
        private static readonly Color BtnBlue = Color.FromArgb(66, 133, 244);
        private static readonly Color BtnRed = Color.FromArgb(192, 57, 43);

        private readonly int _residentId;
        private byte[] _photoBytes;

        private PictureBox _pbPhoto;
        private TextBox _txtFN, _txtMN, _txtLN, _txtContact, _txtOccupation;
        private TextBox _txtAddr;
        private DateTimePicker _dtpBirth;
        private ComboBox _cboGender, _cboCivilStatus;
        private CheckBox _chkVoter, _chk4Ps;
        private TextBox _txtHousehold, _txtEmergencyName, _txtEmergencyRelation, _txtEmergencyPhone;

        private TextBox _txtPlaceOfBirth, _txtCitizenship, _txtHeight, _txtWeight, _txtTIN, _txtICR;

        public EditResidentDialog(int residentId)
        {
            _residentId = residentId;

            this.Text = "Edit Resident Profile";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(460, 840);
            this.BackColor = Color.FromArgb(240, 242, 248);

            BuildDialogUI();
            LoadData();
        }

        private void BuildDialogUI()
        {
            var header = new Panel { Location = new Point(0, 0), Size = new Size(460, 48), BackColor = NavBlue };
            this.Controls.Add(header);
            header.Controls.Add(new Label
            {
                Text = "✎  Edit Profile",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(16, 12),
                BackColor = Color.Transparent,
            });

            var body = new Panel { Location = new Point(0, 48), Size = new Size(460, 792), AutoScroll = true, BackColor = Color.FromArgb(240, 242, 248) };
            this.Controls.Add(body);

            _pbPhoto = new PictureBox
            {
                Location = new Point(20, 20),
                Size = new Size(90, 90),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.FromArgb(208, 214, 232),
            };
            body.Controls.Add(_pbPhoto);

            var btnUp = MakeBtn("Upload", BtnBlue, 8.5f);
            btnUp.Location = new Point(20, 116); btnUp.Size = new Size(90, 26);
            btnUp.Click += (s, e) => UploadPhoto();
            body.Controls.Add(btnUp);

            var btnRm = MakeBtn("Remove", BtnRed, 8.5f);
            btnRm.Location = new Point(20, 146); btnRm.Size = new Size(90, 26);
            btnRm.Click += (s, e) => { _photoBytes = null; _pbPhoto.Image = null; _pbPhoto.BackColor = Color.FromArgb(208, 214, 232); };
            body.Controls.Add(btnRm);

            int fx = 128, fw = 148, gap = 12;
            body.Controls.Add(EL("First Name", fx, 20));
            _txtFN = ET(fx, 38, fw);
            body.Controls.Add(EL("Middle Name", fx + fw + gap, 20));
            _txtMN = ET(fx + fw + gap, 38, fw - 20);
            body.Controls.AddRange(new Control[] { _txtFN, _txtMN });

            body.Controls.Add(EL("Last Name", fx, 70));
            _txtLN = ET(fx, 88, fw);
            body.Controls.Add(EL("Birth Date", fx + fw + gap, 70));
            _dtpBirth = new DateTimePicker { Font = new Font("Segoe UI", 9f), Location = new Point(fx + fw + gap, 88), Width = fw - 20 };
            body.Controls.AddRange(new Control[] { _txtLN, _dtpBirth });

            body.Controls.Add(EL("Gender", fx, 120));
            _cboGender = new ComboBox { Font = new Font("Segoe UI", 9f), Location = new Point(fx, 138), Width = fw, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboGender.Items.AddRange(new object[] { "Male", "Female" });
            body.Controls.Add(EL("Civil Status", fx + fw + gap, 120));
            _cboCivilStatus = new ComboBox { Font = new Font("Segoe UI", 9f), Location = new Point(fx + fw + gap, 138), Width = fw - 20, DropDownStyle = ComboBoxStyle.DropDownList };
            _cboCivilStatus.Items.AddRange(new object[] { "Single", "Married", "Widowed", "Separated" });
            body.Controls.AddRange(new Control[] { _cboGender, _cboCivilStatus });

            int y2 = 190;
            body.Controls.Add(EL("Contact Number", 20, y2));
            _txtContact = ET(20, y2 + 18, 200);
            body.Controls.Add(EL("Occupation", 236, y2));
            _txtOccupation = ET(236, y2 + 18, 200);
            body.Controls.AddRange(new Control[] { _txtContact, _txtOccupation });

            int y3 = 244;
            body.Controls.Add(EL("Address", 20, y3));
            _txtAddr = new TextBox
            {
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(20, y3 + 18),
                Size = new Size(416, 54),
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle,
            };
            body.Controls.Add(_txtAddr);

            int y4 = 330;
            var dividerExtra = new Panel { Location = new Point(0, y4), Size = new Size(460, 1), BackColor = BorderCol };
            body.Controls.Add(dividerExtra);
            y4 += 12;
            body.Controls.Add(SectionLabel("Additional Information", 20, y4));
            y4 += 20;

            body.Controls.Add(EL("Place of Birth", 20, y4));
            _txtPlaceOfBirth = ET(20, y4 + 18, 200);
            body.Controls.Add(EL("Citizenship", 236, y4));
            _txtCitizenship = ET(236, y4 + 18, 200);
            body.Controls.AddRange(new Control[] { _txtPlaceOfBirth, _txtCitizenship });
            y4 += 46;

            body.Controls.Add(EL("Height", 20, y4));
            _txtHeight = ET(20, y4 + 18, 200);
            body.Controls.Add(EL("Weight", 236, y4));
            _txtWeight = ET(236, y4 + 18, 200);
            body.Controls.AddRange(new Control[] { _txtHeight, _txtWeight });
            y4 += 46;

            body.Controls.Add(EL("TIN", 20, y4));
            _txtTIN = ET(20, y4 + 18, 200);
            body.Controls.Add(EL("ICR Number (aliens only)", 236, y4));
            _txtICR = ET(236, y4 + 18, 200);
            body.Controls.AddRange(new Control[] { _txtTIN, _txtICR });
            y4 += 46;

            var dividerReg = new Panel { Location = new Point(0, y4), Size = new Size(460, 1), BackColor = BorderCol };
            body.Controls.Add(dividerReg);
            y4 += 12;
            body.Controls.Add(SectionLabel("Registration & Status", 20, y4));
            y4 += 20;

            _chkVoter = new CheckBox { Text = "Registered voter", Font = new Font("Segoe UI", 9f), Location = new Point(20, y4), AutoSize = true };
            _chk4Ps = new CheckBox { Text = "4Ps beneficiary", Font = new Font("Segoe UI", 9f), Location = new Point(170, y4), AutoSize = true };
            body.Controls.AddRange(new Control[] { _chkVoter, _chk4Ps });
            y4 += 28;

            body.Controls.Add(EL("Household Number", 20, y4));
            _txtHousehold = ET(20, y4 + 18, 200);
            body.Controls.Add(_txtHousehold);
            y4 += 46;

            var dividerEm = new Panel { Location = new Point(0, y4), Size = new Size(460, 1), BackColor = BorderCol };
            body.Controls.Add(dividerEm);
            y4 += 12;
            body.Controls.Add(SectionLabel("Emergency Contact", 20, y4));
            y4 += 20;

            body.Controls.Add(EL("Contact Name", 20, y4));
            _txtEmergencyName = ET(20, y4 + 18, 200);
            body.Controls.Add(EL("Relation", 236, y4));
            _txtEmergencyRelation = ET(236, y4 + 18, 200);
            body.Controls.AddRange(new Control[] { _txtEmergencyName, _txtEmergencyRelation });
            y4 += 46;

            body.Controls.Add(EL("Contact Phone", 20, y4));
            _txtEmergencyPhone = ET(20, y4 + 18, 200);
            body.Controls.Add(_txtEmergencyPhone);
            y4 += 46;

            var divider = new Panel { Location = new Point(0, y4), Size = new Size(460, 1), BackColor = BorderCol };
            body.Controls.Add(divider);
            y4 += 16;

            var btnCancel = MakeBtn("Cancel", BtnGray, 9.5f);
            btnCancel.Size = new Size(100, 34);
            btnCancel.Location = new Point(240, y4);
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            body.Controls.Add(btnCancel);

            var btnSave = MakeBtn("Save Changes", BtnGreen, 9.5f);
            btnSave.Size = new Size(140, 34);
            btnSave.Location = new Point(346, y4);
            btnSave.Click += (s, e) => Save();
            body.Controls.Add(btnSave);

            y4 += 50;
            body.AutoScrollMinSize = new Size(0, y4);
        }

        private static Label SectionLabel(string text, int x, int y) => new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            ForeColor = Color.FromArgb(127, 119, 221),
            AutoSize = true,
            Location = new Point(x, y),
            BackColor = Color.Transparent,
        };

        private void LoadData()
        {
            var conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand("SELECT * FROM resident WHERE idResident=@id", conn);
                    cmd.Parameters.AddWithValue("@id", _residentId);
                    var r = cmd.ExecuteReader();
                    if (!r.Read()) { r.Close(); return; }

                    _txtFN.Text = r["FirstName"]?.ToString() ?? "";
                    _txtMN.Text = r["MiddleName"]?.ToString() ?? "";
                    _txtLN.Text = r["LastName"]?.ToString() ?? "";
                    _dtpBirth.Value = r["DateOfBirth"] != DBNull.Value ? Convert.ToDateTime(r["DateOfBirth"]) : DateTime.Now.AddYears(-18);

                    string gender = r["Gender"]?.ToString() ?? "Male";
                    if (_cboGender.Items.Contains(gender)) _cboGender.SelectedItem = gender; else _cboGender.SelectedIndex = 0;

                    string civil = "Single";
                    try { civil = r["CivilStatus"]?.ToString() ?? "Single"; } catch { }
                    if (_cboCivilStatus.Items.Contains(civil)) _cboCivilStatus.SelectedItem = civil; else _cboCivilStatus.SelectedIndex = 0;

                    _txtContact.Text = r["ContactNumber"]?.ToString() ?? "";
                    _txtAddr.Text = r["Address"]?.ToString() ?? "";
                    try { _txtOccupation.Text = r["Occupation"]?.ToString() ?? ""; } catch { }

                    try { _chkVoter.Checked = r["IsRegisteredVoter"] != DBNull.Value && Convert.ToBoolean(r["IsRegisteredVoter"]); } catch { }
                    try { _chk4Ps.Checked = r["Is4PsBeneficiary"] != DBNull.Value && Convert.ToBoolean(r["Is4PsBeneficiary"]); } catch { }
                    try { _txtHousehold.Text = r["HouseholdNumber"]?.ToString() ?? ""; } catch { }
                    try { _txtEmergencyName.Text = r["EmergencyContactName"]?.ToString() ?? ""; } catch { }
                    try { _txtEmergencyRelation.Text = r["EmergencyContactRelation"]?.ToString() ?? ""; } catch { }
                    try { _txtEmergencyPhone.Text = r["EmergencyContactPhone"]?.ToString() ?? ""; } catch { }

                    try { _txtPlaceOfBirth.Text = r["PlaceOfBirth"]?.ToString() ?? ""; } catch { }
                    try
                    {
                        string cit = r["Citizenship"]?.ToString() ?? "";
                        _txtCitizenship.Text = string.IsNullOrWhiteSpace(cit) ? "Filipino" : cit;
                    }
                    catch { _txtCitizenship.Text = "Filipino"; }
                    try { _txtHeight.Text = r["Height"]?.ToString() ?? ""; } catch { }
                    try { _txtWeight.Text = r["Weight"]?.ToString() ?? ""; } catch { }
                    try { _txtTIN.Text = r["TIN"]?.ToString() ?? ""; } catch { }
                    try { _txtICR.Text = r["ICRNumber"]?.ToString() ?? ""; } catch { }

                    if (r["Photo"] != DBNull.Value)
                    {
                        _photoBytes = (byte[])r["Photo"];
                        using (var ms = new MemoryStream(_photoBytes))
                            _pbPhoto.Image = new Bitmap(Image.FromStream(ms));
                        _pbPhoto.BackColor = Color.Transparent;
                    }
                    r.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading resident:\n" + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UploadPhoto()
        {
            using (var dlg = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    using (var img = Image.FromFile(dlg.FileName))
                    using (var ms = new MemoryStream())
                    {
                        img.Save(ms, ImageFormat.Png);
                        _photoBytes = ms.ToArray();
                    }
                    using (var ms2 = new MemoryStream(_photoBytes))
                        _pbPhoto.Image = new Bitmap(Image.FromStream(ms2));
                    _pbPhoto.BackColor = Color.Transparent;
                }
                catch (Exception ex) { MessageBox.Show("Error:\n" + ex.Message); }
            }
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(_txtFN.Text) || string.IsNullOrWhiteSpace(_txtLN.Text))
            {
                MessageBox.Show("First Name and Last Name are required.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show("Save changes to this profile?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            var conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand(@"
                        UPDATE resident SET
                            FirstName=@fn, LastName=@ln, MiddleName=@mn,
                            DateOfBirth=@dob, Gender=@g, ContactNumber=@c,
                            Address=@a, CivilStatus=@cs, Occupation=@occ, Photo=@p,
                            IsRegisteredVoter=@voter, Is4PsBeneficiary=@ps4,
                            HouseholdNumber=@hh, EmergencyContactName=@emn,
                            EmergencyContactRelation=@emr, EmergencyContactPhone=@emp,
                            PlaceOfBirth=@pob, Citizenship=@cit, Height=@ht, Weight=@wt,
                            TIN=@tin, ICRNumber=@icr
                        WHERE idResident=@id", conn);
                    cmd.Parameters.AddWithValue("@fn", _txtFN.Text.Trim());
                    cmd.Parameters.AddWithValue("@ln", _txtLN.Text.Trim());
                    cmd.Parameters.AddWithValue("@mn", _txtMN.Text.Trim());
                    cmd.Parameters.AddWithValue("@dob", _dtpBirth.Value.Date);
                    cmd.Parameters.AddWithValue("@g", _cboGender.SelectedItem?.ToString() ?? "Male");
                    cmd.Parameters.AddWithValue("@c", _txtContact.Text.Trim());
                    cmd.Parameters.AddWithValue("@a", _txtAddr.Text.Trim());
                    cmd.Parameters.AddWithValue("@cs", _cboCivilStatus.SelectedItem?.ToString() ?? "Single");
                    cmd.Parameters.AddWithValue("@occ", _txtOccupation.Text.Trim());
                    cmd.Parameters.AddWithValue("@p", (object)_photoBytes ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@voter", _chkVoter.Checked);
                    cmd.Parameters.AddWithValue("@ps4", _chk4Ps.Checked);
                    cmd.Parameters.AddWithValue("@hh", string.IsNullOrWhiteSpace(_txtHousehold.Text) ? (object)DBNull.Value : _txtHousehold.Text.Trim());
                    cmd.Parameters.AddWithValue("@emn", string.IsNullOrWhiteSpace(_txtEmergencyName.Text) ? (object)DBNull.Value : _txtEmergencyName.Text.Trim());
                    cmd.Parameters.AddWithValue("@emr", string.IsNullOrWhiteSpace(_txtEmergencyRelation.Text) ? (object)DBNull.Value : _txtEmergencyRelation.Text.Trim());
                    cmd.Parameters.AddWithValue("@emp", string.IsNullOrWhiteSpace(_txtEmergencyPhone.Text) ? (object)DBNull.Value : _txtEmergencyPhone.Text.Trim());
                    cmd.Parameters.AddWithValue("@pob", string.IsNullOrWhiteSpace(_txtPlaceOfBirth.Text) ? (object)DBNull.Value : _txtPlaceOfBirth.Text.Trim());
                    cmd.Parameters.AddWithValue("@cit", string.IsNullOrWhiteSpace(_txtCitizenship.Text) ? "Filipino" : _txtCitizenship.Text.Trim());
                    cmd.Parameters.AddWithValue("@ht", string.IsNullOrWhiteSpace(_txtHeight.Text) ? (object)DBNull.Value : _txtHeight.Text.Trim());
                    cmd.Parameters.AddWithValue("@wt", string.IsNullOrWhiteSpace(_txtWeight.Text) ? (object)DBNull.Value : _txtWeight.Text.Trim());
                    cmd.Parameters.AddWithValue("@tin", string.IsNullOrWhiteSpace(_txtTIN.Text) ? (object)DBNull.Value : _txtTIN.Text.Trim());
                    cmd.Parameters.AddWithValue("@icr", string.IsNullOrWhiteSpace(_txtICR.Text) ? (object)DBNull.Value : _txtICR.Text.Trim());
                    cmd.Parameters.AddWithValue("@id", _residentId);

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        AuditLogger.Log("UPDATE", "Residents", $"Updated resident ID {_residentId}");
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static Label EL(string text, int x, int y) => new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            ForeColor = TextGray,
            AutoSize = true,
            Location = new Point(x, y),
            BackColor = Color.Transparent,
        };

        private static TextBox ET(int x, int y, int w) => new TextBox
        {
            Font = new Font("Segoe UI", 9.5f),
            Location = new Point(x, y),
            Size = new Size(w, 24),
            BorderStyle = BorderStyle.FixedSingle,
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
    }
}