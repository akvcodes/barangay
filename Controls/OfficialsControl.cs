// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  OfficialsControl.cs                                                     ║
// ║  Add to Controls folder. No Designer.cs needed.                          ║
// ║  Wire in MainMenuForm.cs: btnOfficials_Click => NavigateTo(...)          ║
// ╚══════════════════════════════════════════════════════════════════════════╝

using System.Data.SQLite;
using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BarangayManagementSystem.Controls
{
    public class OfficialsControl : UserControl
    {
        // ── State ─────────────────────────────────────────────────────────────
        private int _selectedId = 0;

        // ── Colors ────────────────────────────────────────────────────────────
        private static readonly Color NavBlue = Color.FromArgb(26, 35, 126);
        private static readonly Color BgPage = Color.FromArgb(240, 242, 248);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderCol = Color.FromArgb(220, 225, 238);
        private static readonly Color TextDark = Color.FromArgb(28, 30, 44);
        private static readonly Color TextGray = Color.FromArgb(108, 114, 140);
        private static readonly Color BtnBlue = Color.FromArgb(66, 133, 244);
        private static readonly Color BtnGreen = Color.FromArgb(15, 157, 88);
        private static readonly Color BtnRed = Color.FromArgb(192, 57, 43);
        private static readonly Color BtnGray = Color.FromArgb(100, 110, 128);
        private static readonly Color LightBlue = Color.FromArgb(238, 242, 255);

        // ── LIST VIEW ─────────────────────────────────────────────────────────
        private Panel _listPanel;
        private TextBox _txtSearch;
        private DataGridView _dgv;

        // ── FORM VIEW ─────────────────────────────────────────────────────────
        private Panel _formPanel;
        private Label _lblFormTitle;
        private TextBox _txtFirstName;
        private TextBox _txtLastName;
        private ComboBox _cboPosition;
        private TextBox _txtContact;
        private TextBox _txtEmail;
        private Button _btnSave;

        // ══════════════════════════════════════════════════════════════════════
        public OfficialsControl()
        {
            this.DoubleBuffered = true;
            this.BackColor = BgPage;
            this.Dock = DockStyle.Fill;
            BuildListView();
            BuildFormView();
            ShowList();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  VIEW SWITCHING
        // ══════════════════════════════════════════════════════════════════════
        private void ShowList()
        {
            _formPanel.Visible = false;
            _listPanel.Visible = true;
            _listPanel.BringToFront();
            LoadOfficials();
        }

        private void ShowForm(bool isEdit = false, int id = 0)
        {
            _selectedId = id;
            _listPanel.Visible = false;
            _formPanel.Visible = true;
            _formPanel.BringToFront();
            if (isEdit)
            {
                _lblFormTitle.Text = "✏  Edit Official";
                _btnSave.Text = "💾  Save Changes";
                LoadForEdit(id);
            }
            else
            {
                _lblFormTitle.Text = "➕  Add New Official";
                _btnSave.Text = "💾  Save Official";
                ClearForm();
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  BUILD LIST VIEW
        // ══════════════════════════════════════════════════════════════════════
        private void BuildListView()
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
                Text = "👥   Barangay Officials",
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

            _listPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgPage,
                Padding = new Padding(24, 20, 24, 16),
            };
            this.Controls.Add(_listPanel);

            // Heading
            _listPanel.Controls.Add(new Label
            {
                Text = "Officials",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(0, 0),
                BackColor = Color.Transparent,
            });
            _listPanel.Controls.Add(new Label
            {
                Text = "Manage barangay officials and their positions",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = TextGray,
                AutoSize = true,
                Location = new Point(2, 36),
                BackColor = Color.Transparent,
            });

            // Search card
            var searchCard = Card(new Point(0, 66), new Size(200, 62));
            _listPanel.Controls.Add(searchCard);

            searchCard.Controls.Add(new Label
            {
                Text = "Search:",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = TextDark,
                AutoSize = true,
                Location = new Point(16, 20),
                BackColor = Color.Transparent,
            });

            _txtSearch = new TextBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(90, 16),
                Size = new Size(300, 30),
                BorderStyle = BorderStyle.FixedSingle,
            };
            searchCard.Controls.Add(_txtSearch);

            var btnSearch = Btn("🔍  Search", BtnBlue, 9.5f);
            btnSearch.Location = new Point(400, 15);
            btnSearch.Size = new Size(116, 32);
            btnSearch.Click += (s, e) => LoadOfficials(_txtSearch.Text.Trim());
            searchCard.Controls.Add(btnSearch);

            var btnRefresh = Btn("⟳  Refresh", BtnGray, 9.5f);
            btnRefresh.Location = new Point(718, 15);
            btnRefresh.Size = new Size(112, 32);
            btnRefresh.Click += (s, e) => LoadOfficials(_txtSearch.Text.Trim());
            searchCard.Controls.Add(btnRefresh);

            var btnAdd = Btn("+ Add New Official", BtnGreen, 9.5f);
            btnAdd.Location = new Point(838, 15);
            btnAdd.Size = new Size(190, 32);
            btnAdd.Click += (s, e) => ShowForm(false);
            searchCard.Controls.Add(btnAdd);

            // Grid
            _dgv = MakeGrid();
            _dgv.Location = new Point(0, 140);
            _listPanel.Controls.Add(_dgv);

            _listPanel.Resize += (s, e) =>
            {
                int w = _listPanel.ClientSize.Width - _listPanel.Padding.Horizontal;
                int h = _listPanel.ClientSize.Height - _listPanel.Padding.Vertical;
                if (w < 100) return;
                searchCard.Width = w;
                _dgv.Width = w;
                _dgv.Height = h - 148;
            };
        }

        // ══════════════════════════════════════════════════════════════════════
        //  BUILD FORM VIEW
        // ══════════════════════════════════════════════════════════════════════
        private void BuildFormView()
        {
            _formPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgPage,
                AutoScroll = true,
                Padding = new Padding(24, 20, 24, 24),
                Visible = false,
            };
            this.Controls.Add(_formPanel);

            // Back button
            var btnBack = Btn("← Back to Officials", BtnGray, 9.5f);
            btnBack.Location = new Point(0, 0);
            btnBack.Size = new Size(190, 34);
            btnBack.Click += (s, e) => ShowList();
            _formPanel.Controls.Add(btnBack);

            // Card
            var card = Card(new Point(0, 50), new Size(200, 420));
            _formPanel.Controls.Add(card);

            _lblFormTitle = new Label
            {
                Text = "➕  Add New Official",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(24, 16),
                BackColor = Color.Transparent,
            };
            card.Controls.Add(_lblFormTitle);

            var div = new Panel { Location = new Point(0, 48), Height = 1, BackColor = BorderCol };
            card.Controls.Add(div);
            card.Resize += (s, e) => div.Width = card.Width;

            // ── Fields: label above field, two columns ────────────────────────
            const int lx = 24, rx = 400, fw = 300, rowH = 72;
            int y = 64;

            // Row 1: First Name | Last Name
            card.Controls.Add(FL("First Name:", lx, y));
            _txtFirstName = FT(lx, y + 22, fw);
            card.Controls.Add(_txtFirstName);

            card.Controls.Add(FL("Last Name:", rx, y));
            _txtLastName = FT(rx, y + 22, fw);
            card.Controls.Add(_txtLastName);
            y += rowH;

            // Row 2: Position | Contact Number
            card.Controls.Add(FL("Position:", lx, y));
            _cboPosition = new ComboBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(lx, y + 22),
                Size = new Size(fw, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            _cboPosition.Items.AddRange(new object[] {
                "Barangay Captain", "SK Chairman", "Councilor",
                "Secretary", "Treasurer", "Kagawad"
            });
            card.Controls.Add(_cboPosition);

            card.Controls.Add(FL("Contact Number:", rx, y));
            _txtContact = FT(rx, y + 22, fw);
            card.Controls.Add(_txtContact);
            y += rowH;

            // Row 3: Email (full width)
            card.Controls.Add(FL("Email:", lx, y));
            _txtEmail = FT(lx, y + 22, fw + (rx - lx - fw) + fw);
            card.Controls.Add(_txtEmail);
            y += rowH;

            // Buttons
            _btnSave = Btn("💾  Save Official", BtnGreen, 10f);
            _btnSave.Location = new Point(lx, y);
            _btnSave.Size = new Size(200, 40);
            _btnSave.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            _btnSave.Click += (s, e) => Save();
            card.Controls.Add(_btnSave);

            var btnCancel = Btn("Cancel", BtnGray, 10f);
            btnCancel.Location = new Point(lx + 210, y);
            btnCancel.Size = new Size(110, 40);
            btnCancel.Click += (s, e) => ShowList();
            card.Controls.Add(btnCancel);

            _formPanel.Resize += (s, e) =>
            {
                int w = _formPanel.ClientSize.Width - _formPanel.Padding.Horizontal;
                if (w < 100) return;
                card.Width = w;
                div.Width = w;
            };
        }

        // ══════════════════════════════════════════════════════════════════════
        //  DATA
        // ══════════════════════════════════════════════════════════════════════
        private void LoadOfficials(string search = "")
        {
            SQLiteConnection conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    string where = string.IsNullOrWhiteSpace(search)
                        ? "WHERE IsActive = 1"
                        : @"WHERE IsActive = 1 AND (
                              FirstName LIKE @s OR LastName LIKE @s OR
                              Position  LIKE @s OR ContactNumber LIKE @s OR
                              Email     LIKE @s)";

                    var da = new SQLiteDataAdapter($@"
                        SELECT idOfficials,
                               CONCAT(FirstName,' ',LastName) AS Name,
                               Position,
                               ContactNumber AS Contact,
                               Email
                        FROM officials {where}
                        ORDER BY
                            CASE Position
                                WHEN 'Barangay Captain' THEN 1
                                WHEN 'SK Chairman'      THEN 2
                                WHEN 'Secretary'        THEN 3
                                WHEN 'Treasurer'        THEN 4
                                ELSE 5
                            END, LastName", conn);

                    if (!string.IsNullOrWhiteSpace(search))
                        da.SelectCommand.Parameters.AddWithValue("@s", "%" + search + "%");

                    var dt = new DataTable();
                    da.Fill(dt);
                    _dgv.DataSource = dt;

                    if (_dgv.Columns.Contains("idOfficials")) _dgv.Columns["idOfficials"].Visible = false;
                    if (_dgv.Columns.Contains("Name")) { _dgv.Columns["Name"].Width = 220; }
                    if (_dgv.Columns.Contains("Position")) { _dgv.Columns["Position"].Width = 180; }
                    if (_dgv.Columns.Contains("Contact")) { _dgv.Columns["Contact"].Width = 150; }
                    if (_dgv.Columns.Contains("Email")) { _dgv.Columns["Email"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; }

                    _dgv.CellFormatting -= ColorPos;
                    _dgv.CellFormatting += ColorPos;

                    if (dt.Rows.Count == 0 && !string.IsNullOrWhiteSpace(search))
                        MessageBox.Show("No officials found.", "Search",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading officials:\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ColorPos(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_dgv.Columns.Count <= e.ColumnIndex) return;
            if (_dgv.Columns[e.ColumnIndex].Name != "Position" || e.Value == null) return;
            switch (e.Value.ToString())
            {
                case "Barangay Captain":
                    e.CellStyle.ForeColor = Color.FromArgb(26, 35, 126);
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold); break;
                case "SK Chairman":
                    e.CellStyle.ForeColor = Color.FromArgb(15, 120, 60);
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold); break;
                case "Treasurer":
                    e.CellStyle.ForeColor = Color.FromArgb(160, 90, 0);
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold); break;
                case "Secretary":
                    e.CellStyle.ForeColor = Color.FromArgb(100, 30, 160);
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold); break;
            }
        }

        private void LoadForEdit(int id)
        {
            SQLiteConnection conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand(
                        "SELECT * FROM officials WHERE idOfficials=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    var r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        _txtFirstName.Text = r["FirstName"].ToString();
                        _txtLastName.Text = r["LastName"].ToString();
                        string pos = r["Position"].ToString();
                        if (_cboPosition.Items.Contains(pos))
                            _cboPosition.SelectedItem = pos;
                        _txtContact.Text = r["ContactNumber"]?.ToString() ?? "";
                        _txtEmail.Text = r["Email"]?.ToString() ?? "";
                    }
                    r.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading official:\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(_txtFirstName.Text))
            { MessageBox.Show("First Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); _txtFirstName.Focus(); return; }
            if (string.IsNullOrWhiteSpace(_txtLastName.Text))
            { MessageBox.Show("Last Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); _txtLastName.Focus(); return; }
            if (_cboPosition.SelectedIndex == -1)
            { MessageBox.Show("Please select a Position.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); _cboPosition.Focus(); return; }

            SQLiteConnection conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    SQLiteCommand cmd;
                    if (_selectedId == 0)
                    {
                        cmd = new SQLiteCommand(@"
                            INSERT INTO officials
                                (FirstName,LastName,Position,ContactNumber,Email,IsActive)
                            VALUES (@fn,@ln,@pos,@c,@e,1)", conn);
                    }
                    else
                    {
                        if (MessageBox.Show("Save changes?", "Confirm",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                            != DialogResult.Yes) return;

                        cmd = new SQLiteCommand(@"
                            UPDATE officials SET
                                FirstName=@fn, LastName=@ln, Position=@pos,
                                ContactNumber=@c, Email=@e
                            WHERE idOfficials=@id", conn);
                        cmd.Parameters.AddWithValue("@id", _selectedId);
                    }
                    cmd.Parameters.AddWithValue("@fn", _txtFirstName.Text.Trim());
                    cmd.Parameters.AddWithValue("@ln", _txtLastName.Text.Trim());
                    cmd.Parameters.AddWithValue("@pos", _cboPosition.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@c", _txtContact.Text.Trim());
                    cmd.Parameters.AddWithValue("@e", _txtEmail.Text.Trim());

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show(
                            _selectedId == 0 ? "Official added!" : "Official updated!",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ShowList();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving:\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Delete(int id, string name)
        {
            if (MessageBox.Show(
                $"Permanently delete '{name}'?\n\nThis cannot be undone!",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                != DialogResult.Yes) return;

            SQLiteConnection conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand(
                        "DELETE FROM officials WHERE idOfficials=@id", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Official deleted.", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadOfficials();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearForm()
        {
            _txtFirstName.Text = "";
            _txtLastName.Text = "";
            _cboPosition.SelectedIndex = -1;
            _txtContact.Text = "";
            _txtEmail.Text = "";
            _selectedId = 0;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  GRID
        // ══════════════════════════════════════════════════════════════════════
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
                ColumnHeadersHeight = 40,
                ColumnHeadersHeightSizeMode =
                    DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = NavBlue;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9.5f, FontStyle.Bold);
            g.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            g.DefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            g.DefaultCellStyle.SelectionBackColor = BtnBlue;
            g.DefaultCellStyle.SelectionForeColor = Color.White;
            g.AlternatingRowsDefaultCellStyle.BackColor = LightBlue;

            // Action button columns
            var colEdit = new DataGridViewButtonColumn
            {
                Name = "colEdit",
                HeaderText = "Actions",
                Text = "✏ Edit",
                UseColumnTextForButtonValue = true,
                Width = 110,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                FlatStyle = FlatStyle.Flat,
            };
            var colDelete = new DataGridViewButtonColumn
            {
                Name = "colDelete",
                HeaderText = "",
                Text = "🗑 Delete",
                UseColumnTextForButtonValue = true,
                Width = 110,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                FlatStyle = FlatStyle.Flat,
            };

            g.DataBindingComplete += (s, e) =>
            {
                if (!g.Columns.Contains("colEdit")) g.Columns.Add(colEdit);
                if (!g.Columns.Contains("colDelete")) g.Columns.Add(colDelete);
            };

            g.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                int id = Convert.ToInt32(g.Rows[e.RowIndex].Cells["idOfficials"].Value);
                string name = g.Rows[e.RowIndex].Cells["Name"].Value?.ToString() ?? "";
                if (e.ColumnIndex == g.Columns["colEdit"]?.Index)
                    ShowForm(true, id);
                else if (e.ColumnIndex == g.Columns["colDelete"]?.Index)
                    Delete(id, name);
            };

            // Paint rounded buttons
            g.CellPainting += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                bool isEdit = g.Columns["colEdit"] != null && e.ColumnIndex == g.Columns["colEdit"].Index;
                bool isDelete = g.Columns["colDelete"] != null && e.ColumnIndex == g.Columns["colDelete"].Index;
                if (!isEdit && !isDelete) return;

                // Fill background
                using (var bg = new SolidBrush(e.RowIndex % 2 == 0 ? BgCard : LightBlue))
                    e.Graphics.FillRectangle(bg, e.CellBounds);

                Color btnColor = isEdit ? BtnBlue : BtnRed;
                string btnText = isEdit ? "Edit" : "Delete";
                int pad = 6;

                var rect = new Rectangle(
                    e.CellBounds.X + pad,
                    e.CellBounds.Y + 5,
                    e.CellBounds.Width - pad * 2,
                    e.CellBounds.Height - 10);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = new GraphicsPath())
                {
                    int r = 6, d = r * 2;
                    path.AddArc(rect.X, rect.Y, d, d, 180, 90);
                    path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
                    path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
                    path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
                    path.CloseFigure();
                    using (var fill = new SolidBrush(btnColor))
                        e.Graphics.FillPath(fill, path);
                }
                using (var sf = new StringFormat
                { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString(btnText,
                        new Font("Segoe UI", 9f, FontStyle.Bold),
                        Brushes.White, rect, sf);
                e.Handled = true;
            };

            return g;
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
                    RoundRect(e.Graphics, pen,
                        new Rectangle(0, 0, p.Width - 1, p.Height - 1), 10);
            };
            return p;
        }

        private Button Btn(string text, Color bg, float size)
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
                Math.Max(0, bg.R - 22),
                Math.Max(0, bg.G - 22),
                Math.Max(0, bg.B - 22));
            return b;
        }

        private static Label FL(string text, int x, int y) =>
            new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 70, 100),
                AutoSize = true,
                Location = new Point(x, y),
                BackColor = Color.Transparent,
            };

        private static TextBox FT(int x, int y, int w) =>
            new TextBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(x, y),
                Size = new Size(w, 30),
                BorderStyle = BorderStyle.FixedSingle,
            };

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
            // 
            // OfficialsControl
            // 
            this.Name = "OfficialsControl";
            this.Load += new System.EventHandler(this.OfficialsControl_Load);
            this.ResumeLayout(false);

        }

        private void OfficialsControl_Load(object sender, EventArgs e)
        {

        }
    }
}