using System.Data.SQLite;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace BarangayManagementSystem
{
    public class ResidentsForm : Form
    {
        private static readonly Color NavBlue = Color.FromArgb(26, 35, 126);
        private static readonly Color BgPage = Color.FromArgb(240, 242, 248);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderCol = Color.FromArgb(210, 218, 235);
        private static readonly Color LabelCol = Color.FromArgb(55, 65, 100);
        private static readonly Color SectionCol = Color.FromArgb(127, 119, 221);
        private static readonly Color BtnGreen = Color.FromArgb(15, 157, 88);
        private static readonly Color BtnGray = Color.FromArgb(100, 110, 128);
        private static readonly Color BtnBlue = Color.FromArgb(66, 133, 244);
        private static readonly Color BtnRed = Color.FromArgb(192, 57, 43);

        private int _residentId = 0;
        private byte[] _photo = null;
        private bool _isEditMode = false;

        private TextBox _txtFirstName, _txtMiddleName, _txtLastName;
        private TextBox _txtContact, _txtAddress, _txtOccupation;
        private DateTimePicker _dtpBirth;
        private ComboBox _cboGender, _cboCivilStatus;
        private PictureBox _pbPhoto;

        // ── Registration & Status / Emergency Contact ────────────────────────
        private CheckBox _chkVoter, _chk4Ps;
        private TextBox _txtHousehold, _txtEmergencyName, _txtEmergencyRelation, _txtEmergencyPhone;

        // ── Additional Information (Height/Weight/PlaceOfBirth/Citizenship/TIN/ICR) ──
        private TextBox _txtPlaceOfBirth, _txtCitizenship, _txtHeight, _txtWeight, _txtTIN, _txtICR;

        // Fixed layout constants — all based on 600px card width
        // Left col: x=24, width=250
        // Right col: x=306, width=250
        // Gap between cols: 32px
        private const int PAD = 24;   // card left padding
        private const int FW = 250;  // field width
        private const int COL2 = 306;  // right column x
        private const int ROW = 64;   // row height

        public ResidentsForm(int residentId = 0)
        {
            _residentId = residentId;
            _isEditMode = residentId > 0;

            this.Text = _isEditMode ? "Edit Resident" : "Add New Resident";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.ControlBox = false;
            this.BackColor = BgPage;
            this.Font = new Font("Segoe UI", 9.5f);

            BuildUI();
            this.Load += (s, e) => { if (_isEditMode) LoadResident(); };
        }

        private void BuildUI()
        {
            // ── Header ────────────────────────────────────────────────────────
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = NavBlue,
            };
            this.Controls.Add(header);

            header.Controls.Add(new Label
            {
                Text = _isEditMode ? "Edit Resident" : "Add New Resident",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 12),
                BackColor = Color.Transparent,
            });
            header.Controls.Add(new Label
            {
                Text = _isEditMode
                    ? "Update resident information"
                    : "Fill in the details to register a new resident",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(170, 195, 240),
                AutoSize = true,
                Location = new Point(22, 44),
                BackColor = Color.Transparent,
            });
            var btnX = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Size = new Size(40, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            btnX.FlatAppearance.BorderSize = 0;
            btnX.Click += (s, e) => this.Close();
            header.Controls.Add(btnX);
            // Position after header is sized
            header.Resize += (s, e) => btnX.Location = new Point(header.Width - 48, 15);

            // ── Scroll panel ──────────────────────────────────────────────────
            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgPage,
                AutoScroll = true,
                Padding = new Padding(20, 20, 20, 20),
            };
            this.Controls.Add(scroll);
            scroll.BringToFront();

            // ── Card — fixed width 560px, centered ────────────────────────────
            var card = new Panel
            {
                BackColor = BgCard,
                Location = new Point(0, 0),
                Size = new Size(560, 100),  // height set at end
            };
            card.Paint += (s, e) => PaintCard(e.Graphics, card);
            scroll.Controls.Add(card);

            // Resize card — max 700px wide, centered
            scroll.Resize += (s, e) =>
            {
                int w = scroll.ClientSize.Width - scroll.Padding.Horizontal;
                if (w < 100) return;
                int cardW = w > 720 ? 720 : w;
                card.Width = cardW;
                card.Left = (scroll.ClientSize.Width - cardW) / 2;
            };

            // ── Card title ────────────────────────────────────────────────────
            card.Controls.Add(new Label
            {
                Text = "👤  Resident Information",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(PAD, 14),
                BackColor = Color.Transparent,
            });
            var div = new Panel { Location = new Point(0, 44), Height = 1, BackColor = BorderCol };
            card.Controls.Add(div);
            card.Resize += (s, e) => div.Width = card.Width;

            // ── Fields — fixed 2-column layout ───────────────────────────────
            int y = 56;

            // Row 1: First Name | Last Name
            L(card, "First Name:", PAD, y); L(card, "Last Name:", COL2, y);
            _txtFirstName = F(card, PAD, y + 20, FW);
            _txtLastName = F(card, COL2, y + 20, FW);
            y += ROW;

            // Row 2: Middle Name | Birth Date
            L(card, "Middle Name:", PAD, y); L(card, "Birth Date:", COL2, y);
            _txtMiddleName = F(card, PAD, y + 20, FW);
            _dtpBirth = new DateTimePicker
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(COL2, y + 20),
                Size = new Size(FW, 28),
                Value = DateTime.Now.AddYears(-18),
            };
            card.Controls.Add(_dtpBirth);
            y += ROW;

            // Row 3: Gender | Civil Status
            L(card, "Gender:", PAD, y); L(card, "Civil Status:", COL2, y);
            _cboGender = new ComboBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(PAD, y + 20),
                Size = new Size(FW, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            _cboGender.Items.AddRange(new object[] { "Male", "Female" });
            card.Controls.Add(_cboGender);

            _cboCivilStatus = new ComboBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(COL2, y + 20),
                Size = new Size(FW, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            _cboCivilStatus.Items.AddRange(new object[] {
                "Single", "Married", "Widowed", "Separated" });
            _cboCivilStatus.SelectedIndex = 0;
            card.Controls.Add(_cboCivilStatus);
            y += ROW;

            // Row 4: Contact | Occupation
            L(card, "Contact Number:", PAD, y); L(card, "Occupation:", COL2, y);
            _txtContact = F(card, PAD, y + 20, FW);
            _txtOccupation = F(card, COL2, y + 20, FW);
            y += ROW;

            // Row 5: Address (full width — spans both columns)
            L(card, "Address:", PAD, y);
            _txtAddress = F(card, PAD, y + 20, FW + (COL2 - PAD - FW) + FW);
            y += ROW;

            // ── Additional Information ───────────────────────────────────────
            var dividerAdd = new Panel { Location = new Point(0, y), Height = 1, BackColor = BorderCol };
            card.Controls.Add(dividerAdd);
            card.Resize += (s, e) => dividerAdd.Width = card.Width;
            y += 14;

            card.Controls.Add(new Label
            {
                Text = "Additional Information",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = SectionCol,
                AutoSize = true,
                Location = new Point(PAD, y),
                BackColor = Color.Transparent,
            });
            y += 24;

            L(card, "Place of Birth:", PAD, y); L(card, "Citizenship:", COL2, y);
            _txtPlaceOfBirth = F(card, PAD, y + 20, FW);
            _txtCitizenship = F(card, COL2, y + 20, FW);
            _txtCitizenship.Text = "Filipino";
            y += ROW;

            L(card, "Height (e.g. 165 cm):", PAD, y); L(card, "Weight (e.g. 60 kg):", COL2, y);
            _txtHeight = F(card, PAD, y + 20, FW);
            _txtWeight = F(card, COL2, y + 20, FW);
            y += ROW;

            L(card, "TIN:", PAD, y); L(card, "ICR Number (aliens only):", COL2, y);
            _txtTIN = F(card, PAD, y + 20, FW);
            _txtICR = F(card, COL2, y + 20, FW);
            y += ROW;

            // ── Registration & Status ────────────────────────────────────────
            var dividerReg = new Panel { Location = new Point(0, y), Height = 1, BackColor = BorderCol };
            card.Controls.Add(dividerReg);
            card.Resize += (s, e) => dividerReg.Width = card.Width;
            y += 14;

            card.Controls.Add(new Label
            {
                Text = "Registration & Status",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = SectionCol,
                AutoSize = true,
                Location = new Point(PAD, y),
                BackColor = Color.Transparent,
            });
            y += 24;

            _chkVoter = new CheckBox
            {
                Text = "Registered voter",
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(PAD, y),
                AutoSize = true,
            };
            _chk4Ps = new CheckBox
            {
                Text = "4Ps beneficiary",
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(COL2, y),
                AutoSize = true,
            };
            card.Controls.Add(_chkVoter);
            card.Controls.Add(_chk4Ps);
            y += 32;

            L(card, "Household Number:", PAD, y);
            _txtHousehold = F(card, PAD, y + 20, FW);
            y += ROW;

            // ── Emergency Contact ────────────────────────────────────────────
            var dividerEm = new Panel { Location = new Point(0, y - 8), Height = 1, BackColor = BorderCol };
            card.Controls.Add(dividerEm);
            card.Resize += (s, e) => dividerEm.Width = card.Width;
            y += 6;

            card.Controls.Add(new Label
            {
                Text = "Emergency Contact",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = SectionCol,
                AutoSize = true,
                Location = new Point(PAD, y),
                BackColor = Color.Transparent,
            });
            y += 24;

            L(card, "Contact Name:", PAD, y); L(card, "Relation:", COL2, y);
            _txtEmergencyName = F(card, PAD, y + 20, FW);
            _txtEmergencyRelation = F(card, COL2, y + 20, FW);
            y += ROW;

            L(card, "Contact Phone:", PAD, y);
            _txtEmergencyPhone = F(card, PAD, y + 20, FW);
            y += ROW;

            // Row: Photo
            L(card, "Photo (Optional):", PAD, y);
            y += 22;

            _pbPhoto = new PictureBox
            {
                Location = new Point(PAD, y),
                Size = new Size(110, 110),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.FromArgb(220, 225, 244),
                BorderStyle = BorderStyle.FixedSingle,
            };
            card.Controls.Add(_pbPhoto);

            var btnUp = Btn("Upload Photo", BtnBlue);
            btnUp.Location = new Point(PAD + 124, y);
            btnUp.Size = new Size(140, 36);
            btnUp.Click += (s, e) => UploadPhoto();
            card.Controls.Add(btnUp);

            var btnRm = Btn("Remove Photo", BtnRed);
            btnRm.Location = new Point(PAD + 124, y + 44);
            btnRm.Size = new Size(140, 36);
            btnRm.Click += (s, e) => RemovePhoto();
            card.Controls.Add(btnRm);
            y += 126;

            // ── Buttons ───────────────────────────────────────────────────────
            var btnSave = Btn(
                _isEditMode ? "💾  Save Changes" : "✚  Add Resident",
                BtnGreen);
            btnSave.Location = new Point(PAD, y);
            btnSave.Size = new Size(200, 42);
            btnSave.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            btnSave.Click += (s, e) => Save();
            card.Controls.Add(btnSave);

            var btnClear = Btn("Clear", BtnGray);
            btnClear.Location = new Point(PAD + 210, y);
            btnClear.Size = new Size(110, 42);
            btnClear.Click += (s, e) => ClearForm();
            card.Controls.Add(btnClear);

            card.Height = y + 60;
        }

        // ── Draw card border + shadow ─────────────────────────────────────────
        private void PaintCard(Graphics g, Panel card)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
            using (var bg = new SolidBrush(BgCard))
            using (var path = RoundPath(rect, 10))
                g.FillPath(bg, path);
            using (var pen = new Pen(BorderCol, 1f))
            using (var path = RoundPath(rect, 10))
                g.DrawPath(pen, path);
        }

        private GraphicsPath RoundPath(Rectangle r, int radius)
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

        // ── Load for edit mode ────────────────────────────────────────────────
        private void LoadResident()
        {
            var conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand(
                        "SELECT * FROM resident WHERE idResident=@id", conn);
                    cmd.Parameters.AddWithValue("@id", _residentId);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return;
                        _txtFirstName.Text = r["FirstName"]?.ToString() ?? "";
                        _txtMiddleName.Text = r["MiddleName"]?.ToString() ?? "";
                        _txtLastName.Text = r["LastName"]?.ToString() ?? "";
                        _txtContact.Text = r["ContactNumber"]?.ToString() ?? "";
                        _txtAddress.Text = r["Address"]?.ToString() ?? "";
                        try { _txtOccupation.Text = r["Occupation"]?.ToString() ?? ""; } catch { }

                        if (r["DateOfBirth"] != DBNull.Value)
                            _dtpBirth.Value = Convert.ToDateTime(r["DateOfBirth"]);

                        string g = r["Gender"]?.ToString() ?? "";
                        if (_cboGender.Items.Contains(g)) _cboGender.SelectedItem = g;

                        try
                        {
                            string cs = r["CivilStatus"]?.ToString() ?? "Single";
                            if (_cboCivilStatus.Items.Contains(cs))
                                _cboCivilStatus.SelectedItem = cs;
                        }
                        catch { _cboCivilStatus.SelectedIndex = 0; }

                        // Registration & Status / Emergency Contact — wrapped in try/catch
                        // so this form still works even before the columns exist in the DB.
                        try { _chkVoter.Checked = r["IsRegisteredVoter"] != DBNull.Value && Convert.ToBoolean(r["IsRegisteredVoter"]); } catch { }
                        try { _chk4Ps.Checked = r["Is4PsBeneficiary"] != DBNull.Value && Convert.ToBoolean(r["Is4PsBeneficiary"]); } catch { }
                        try { _txtHousehold.Text = r["HouseholdNumber"]?.ToString() ?? ""; } catch { }
                        try { _txtEmergencyName.Text = r["EmergencyContactName"]?.ToString() ?? ""; } catch { }
                        try { _txtEmergencyRelation.Text = r["EmergencyContactRelation"]?.ToString() ?? ""; } catch { }
                        try { _txtEmergencyPhone.Text = r["EmergencyContactPhone"]?.ToString() ?? ""; } catch { }

                        // Additional Information — same defensive try/catch pattern.
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
                            _photo = (byte[])r["Photo"];
                            using (var ms = new MemoryStream(_photo))
                                _pbPhoto.Image = Image.FromStream(ms);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ── Save ─────────────────────────────────────────────────────────────
        private void Save()
        {
            if (!Validate2()) return;
            if (_isEditMode) Update2(); else Add2();
        }

        private bool Validate2()
        {
            if (string.IsNullOrWhiteSpace(_txtFirstName.Text))
            { MessageBox.Show("First Name is required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); _txtFirstName.Focus(); return false; }
            if (string.IsNullOrWhiteSpace(_txtLastName.Text))
            { MessageBox.Show("Last Name is required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); _txtLastName.Focus(); return false; }
            if (_cboGender.SelectedIndex == -1)
            { MessageBox.Show("Please select a Gender!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); _cboGender.Focus(); return false; }
            if (_dtpBirth.Value.Date >= DateTime.Now.Date)
            { MessageBox.Show("Date of Birth must be in the past!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            if (string.IsNullOrWhiteSpace(_txtAddress.Text))
            { MessageBox.Show("Address is required!", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); _txtAddress.Focus(); return false; }
            return true;
        }

        private void Add2()
        {
            var conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand(@"
                        INSERT INTO resident
                            (FirstName,MiddleName,LastName,DateOfBirth,
                             Gender,CivilStatus,ContactNumber,Address,
                             Occupation,Photo,IsActive,
                             IsRegisteredVoter,Is4PsBeneficiary,HouseholdNumber,
                             EmergencyContactName,EmergencyContactRelation,EmergencyContactPhone,
                             PlaceOfBirth,Citizenship,Height,Weight,TIN,ICRNumber,
                             DateRegistered)
                        VALUES
                            (@fn,@mn,@ln,@dob,@g,@cs,@c,@a,@occ,@ph,1,
                             @voter,@ps4,@hh,@emn,@emr,@emp,
                             @pob,@cit,@ht,@wt,@tin,@icr,
                             datetime('now'))", conn);
                    Params(cmd);
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show("Resident added successfully!",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Update2()
        {
            if (MessageBox.Show("Save changes?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            var conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand(@"
                        UPDATE resident SET
                            FirstName=@fn,MiddleName=@mn,LastName=@ln,
                            DateOfBirth=@dob,Gender=@g,CivilStatus=@cs,
                            ContactNumber=@c,Address=@a,Occupation=@occ,Photo=@ph,
                            IsRegisteredVoter=@voter,Is4PsBeneficiary=@ps4,HouseholdNumber=@hh,
                            EmergencyContactName=@emn,EmergencyContactRelation=@emr,EmergencyContactPhone=@emp,
                            PlaceOfBirth=@pob,Citizenship=@cit,Height=@ht,Weight=@wt,TIN=@tin,ICRNumber=@icr
                        WHERE idResident=@id", conn);
                    Params(cmd);
                    cmd.Parameters.AddWithValue("@id", _residentId);
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show("Resident updated!",
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Params(SQLiteCommand cmd)
        {
            cmd.Parameters.AddWithValue("@fn", _txtFirstName.Text.Trim());
            cmd.Parameters.AddWithValue("@mn", _txtMiddleName.Text.Trim());
            cmd.Parameters.AddWithValue("@ln", _txtLastName.Text.Trim());
            cmd.Parameters.AddWithValue("@dob", _dtpBirth.Value.Date);
            cmd.Parameters.AddWithValue("@g", _cboGender.SelectedItem?.ToString() ?? "Male");
            cmd.Parameters.AddWithValue("@cs", _cboCivilStatus.SelectedItem?.ToString() ?? "Single");
            cmd.Parameters.AddWithValue("@c", _txtContact.Text.Trim());
            cmd.Parameters.AddWithValue("@a", _txtAddress.Text.Trim());
            cmd.Parameters.AddWithValue("@occ", _txtOccupation.Text.Trim());
            cmd.Parameters.AddWithValue("@ph", _photo != null ? (object)_photo : DBNull.Value);

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
        }

        // ── Photo ─────────────────────────────────────────────────────────────
        private void UploadPhoto()
        {
            using (var dlg = new OpenFileDialog
            { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    using (var img = Image.FromFile(dlg.FileName))
                    {
                        _pbPhoto.Image = new Bitmap(img);
                        _pbPhoto.SizeMode = PictureBoxSizeMode.StretchImage;
                        using (var ms = new MemoryStream())
                        {
                            img.Save(ms, ImageFormat.Png);
                            _photo = ms.ToArray();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ResidentsForm
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "ResidentsForm";
            this.Load += new System.EventHandler(this.ResidentsForm_Load);
            this.ResumeLayout(false);

        }

        private void ResidentsForm_Load(object sender, EventArgs e)
        {

        }

        private void RemovePhoto()
        {
            _photo = null;
            _pbPhoto.Image = null;
            _pbPhoto.BackColor = Color.FromArgb(220, 225, 244);
        }

        private void ClearForm()
        {
            _txtFirstName.Clear(); _txtMiddleName.Clear(); _txtLastName.Clear();
            _txtContact.Clear(); _txtAddress.Clear(); _txtOccupation.Clear();
            _dtpBirth.Value = DateTime.Now.AddYears(-18);
            _cboGender.SelectedIndex = -1;
            _cboCivilStatus.SelectedIndex = 0;
            _chkVoter.Checked = false;
            _chk4Ps.Checked = false;
            _txtHousehold.Clear();
            _txtEmergencyName.Clear();
            _txtEmergencyRelation.Clear();
            _txtEmergencyPhone.Clear();
            _txtPlaceOfBirth.Clear();
            _txtCitizenship.Text = "Filipino";
            _txtHeight.Clear();
            _txtWeight.Clear();
            _txtTIN.Clear();
            _txtICR.Clear();
            RemovePhoto();
        }

        // ── UI helpers ────────────────────────────────────────────────────────
        private static void L(Panel p, string text, int x, int y)
        {
            p.Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = LabelCol,
                AutoSize = true,
                Location = new Point(x, y),
                BackColor = Color.Transparent,
            });
        }

        private static TextBox F(Panel p, int x, int y, int w)
        {
            var t = new TextBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(x, y),
                Size = new Size(w, 28),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
            };
            p.Controls.Add(t);
            return t;
        }

        private static Button Btn(string text, Color bg)
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
                Math.Max(0, bg.R - 22), Math.Max(0, bg.G - 22),
                Math.Max(0, bg.B - 22));
            return b;
        }
    }
}