// ╔══════════════════════════════════════════════════════════════════════════╗
// ║  CedulaForm.cs                                                           ║
// ║  Single file — no Designer needed.                                       ║
// ║  Add as New Item → Class → CedulaForm.cs                                 ║
// ╚══════════════════════════════════════════════════════════════════════════╝

using System.Data.SQLite;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;

namespace BarangayManagementSystem
{
    public class CedulaForm : Form
    {
        // ── Colors ────────────────────────────────────────────────────────────
        private static readonly Color NavBlue = Color.FromArgb(26, 35, 126);
        private static readonly Color BgPage = Color.FromArgb(240, 242, 248);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderCol = Color.FromArgb(220, 225, 238);
        private static readonly Color TextGray = Color.FromArgb(108, 114, 140);
        private static readonly Color BtnBlue = Color.FromArgb(66, 133, 244);
        private static readonly Color BtnGreen = Color.FromArgb(15, 157, 88);
        private static readonly Color BtnGray = Color.FromArgb(100, 110, 128);
        private static readonly Color BtnNavy = Color.FromArgb(26, 35, 126);

        // ── Resident state (pulled straight from resident table — read-only here) ──
        private int _residentId = 0;
        private string _fullName = "";
        private string _address = "";
        private string _age = "";
        private string _gender = "";
        private string _occupationFromResident = "";
        private string _civilStatusFromResident = "";
        private string _placeOfBirth = "";
        private string _citizenship = "Filipino";
        private string _height = "";
        private string _weight = "";
        private string _tin = "";
        private string _icrNumber = "";
        private DateTime _dob = DateTime.MinValue;

        // ── Search controls ───────────────────────────────────────────────────
        private TextBox _txtSearch;
        private ListBox _lstResults;
        private Panel _searchResultPanel;

        // ── Resident info labels (read-only display of pulled fields) ─────────
        private Label _lblName, _lblLine2, _lblLine3, _lblLine4, _lblAddress;

        // ── Cedula identifiers ────────────────────────────────────────────────
        private TextBox _txtCedulaNo;
        private TextBox _txtSerialNo;
        private DateTimePicker _dtpDate;
        private ComboBox _cboCivil;
        private TextBox _txtOccupation;

        // ── Income breakdown (drives the tax computation) ─────────────────────
        private TextBox _txtGrossReceipts;
        private TextBox _txtSalariesIncome;
        private TextBox _txtRealPropertyIncome;
        private CheckBox _chkExempted;
        private TextBox _txtInterest;
        private TextBox _txtIssuedBy;

        // ── Computed totals (read-only, recalculated live) ────────────────────
        private Label _lblTaxable, _lblBasicTax, _lblAdditionalTax, _lblCommunityDue, _lblTotalPaid, _lblAmountWords;

        private decimal _taxableAmount = 0m;
        private decimal _basicTax = 5.00m;
        private decimal _additionalTax = 0m;
        private decimal _communityTaxDue = 5.00m;
        private decimal _interestAmount = 0m;
        private decimal _totalAmountPaid = 5.00m;
        private string _amountInWords = "";

        // ── Preview panel ─────────────────────────────────────────────────────
        private Panel _previewPanel;

        // ══════════════════════════════════════════════════════════════════════
        public CedulaForm(int residentId = 0, string residentName = "")
        {
            this.Text = "Generate Cedula";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = BgPage;
            this.Font = new Font("Segoe UI", 9.5f);
            this.MinimumSize = new Size(1050, 700);

            BuildUI();

            if (residentId > 0)
            {
                LoadResidentById(residentId);
            }

            GenerateCedulaNumber();
            RecalculateTax();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  BUILD UI
        // ══════════════════════════════════════════════════════════════════════
        private void BuildUI()
        {
            // ── Page title ────────────────────────────────────────────────────
            this.Controls.Add(new Label
            {
                Text = "Generate Cedula",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(24, 16),
                BackColor = Color.Transparent,
            });
            this.Controls.Add(new Label
            {
                Text = "Search a resident, fill in income details, then print the Community Tax Certificate",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = TextGray,
                AutoSize = true,
                Location = new Point(26, 52),
                BackColor = Color.Transparent,
            });

            // ── LEFT PANEL ────────────────────────────────────────────────────
            var leftPanel = new Panel
            {
                Location = new Point(24, 76),
                Size = new Size(430, 700),
                BackColor = Color.Transparent,
                AutoScroll = true,
            };
            this.Controls.Add(leftPanel);

            // Search card
            var searchCard = MakeCard(new Point(0, 0), new Size(410, 80));
            leftPanel.Controls.Add(searchCard);

            searchCard.Controls.Add(new Label
            {
                Text = "Search Resident:",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(16, 14),
                BackColor = Color.Transparent,
            });

            _txtSearch = new TextBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(16, 36),
                Size = new Size(290, 28),
                BorderStyle = BorderStyle.FixedSingle,
            };
            _txtSearch.TextChanged += (s, e) => SearchResidents();
            searchCard.Controls.Add(_txtSearch);

            var btnSearch = MakeBtn("Search", BtnBlue);
            btnSearch.Location = new Point(314, 35);
            btnSearch.Size = new Size(80, 30);
            btnSearch.Click += (s, e) => SearchResidents();
            searchCard.Controls.Add(btnSearch);

            // Search results dropdown
            _searchResultPanel = new Panel
            {
                Location = new Point(24, 156),
                Size = new Size(398, 160),
                BackColor = BgCard,
                BorderStyle = BorderStyle.FixedSingle,
                Visible = false,
            };
            this.Controls.Add(_searchResultPanel);
            this.Controls.SetChildIndex(_searchResultPanel, 0);

            _lstResults = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5f),
                BorderStyle = BorderStyle.None,
            };
            _lstResults.DoubleClick += (s, e) => SelectFromList();
            _lstResults.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) SelectFromList(); };
            _searchResultPanel.Controls.Add(_lstResults);

            // Resident info card — expanded to show all pulled fields, read-only
            var infoCard = MakeCard(new Point(0, 90), new Size(410, 168));
            leftPanel.Controls.Add(infoCard);

            infoCard.Controls.Add(new Label
            {
                Text = "👤  Resident Information",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(16, 12),
                BackColor = Color.Transparent,
            });

            _lblName = InfoLabel("— Select a resident —", 16, 38, 380, NavBlue, 11f, FontStyle.Bold);
            infoCard.Controls.Add(_lblName);
            _lblLine2 = InfoLabel("", 16, 62, 380, TextGray, 8.5f, FontStyle.Regular);
            infoCard.Controls.Add(_lblLine2);
            _lblLine3 = InfoLabel("", 16, 80, 380, TextGray, 8.5f, FontStyle.Regular);
            infoCard.Controls.Add(_lblLine3);
            _lblLine4 = InfoLabel("", 16, 98, 380, TextGray, 8.5f, FontStyle.Regular);
            infoCard.Controls.Add(_lblLine4);
            _lblAddress = InfoLabel("", 16, 122, 380, TextGray, 8.5f, FontStyle.Regular);
            _lblAddress.Height = 38;
            infoCard.Controls.Add(_lblAddress);

            // Cedula details card — identifiers, income breakdown, computed totals
            var detailsCard = MakeCard(new Point(0, 268), new Size(410, 560));
            leftPanel.Controls.Add(detailsCard);

            detailsCard.Controls.Add(new Label
            {
                Text = "📋  Cedula Details",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(16, 12),
                BackColor = Color.Transparent,
            });

            int fy = 40; int fw = 185; int lx = 16; int fx = 16;

            detailsCard.Controls.Add(FL("Cedula Number:", lx, fy));
            _txtCedulaNo = FT(fx, fy + 20, fw);
            _txtCedulaNo.ReadOnly = true;
            _txtCedulaNo.TextChanged += (s, e) => _previewPanel?.Invalidate();
            detailsCard.Controls.Add(_txtCedulaNo);

            detailsCard.Controls.Add(FL("Serial Number:", lx + fw + 16, fy));
            _txtSerialNo = FT(fx + fw + 16, fy + 20, fw);
            _txtSerialNo.ReadOnly = true;
            _txtSerialNo.TextChanged += (s, e) => _previewPanel?.Invalidate();
            detailsCard.Controls.Add(_txtSerialNo);
            fy += 62;

            detailsCard.Controls.Add(FL("Date Issued:", lx, fy));
            _dtpDate = new DateTimePicker
            {
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(fx, fy + 20),
                Size = new Size(fw, 28),
                Value = DateTime.Now,
            };
            _dtpDate.ValueChanged += (s, e) => _previewPanel?.Invalidate();
            detailsCard.Controls.Add(_dtpDate);

            detailsCard.Controls.Add(FL("Civil Status:", lx + fw + 16, fy));
            _cboCivil = new ComboBox
            {
                Font = new Font("Segoe UI", 9.5f),
                Location = new Point(fx + fw + 16, fy + 20),
                Size = new Size(fw, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            _cboCivil.Items.AddRange(new object[] { "Single", "Married", "Widowed", "Separated" });
            _cboCivil.SelectedIndex = 0;
            _cboCivil.SelectedIndexChanged += (s, e) => _previewPanel?.Invalidate();
            detailsCard.Controls.Add(_cboCivil);
            fy += 62;

            detailsCard.Controls.Add(FL("Occupation:", lx, fy));
            _txtOccupation = FT(fx, fy + 20, fw * 2 + 16);
            _txtOccupation.TextChanged += (s, e) => _previewPanel?.Invalidate();
            detailsCard.Controls.Add(_txtOccupation);
            fy += 54;

            _chkExempted = new CheckBox
            {
                Text = "Exempted / Voluntary (basic tax ₱1.00 instead of ₱5.00)",
                Font = new Font("Segoe UI", 8.5f),
                Location = new Point(lx, fy),
                AutoSize = true,
            };
            _chkExempted.CheckedChanged += (s, e) => RecalculateTax();
            detailsCard.Controls.Add(_chkExempted);
            fy += 28;

            // ── Income breakdown — basis for additional tax ──────────────────
            detailsCard.Controls.Add(new Label
            {
                Text = "Basis for additional tax",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(127, 119, 221),
                AutoSize = true,
                Location = new Point(lx, fy),
                BackColor = Color.Transparent,
            });
            fy += 20;

            detailsCard.Controls.Add(FL("Gross Receipts (Business ₱):", lx, fy));
            _txtGrossReceipts = FT(fx, fy + 20, fw);
            _txtGrossReceipts.Text = "0";
            _txtGrossReceipts.TextChanged += (s, e) => RecalculateTax();
            detailsCard.Controls.Add(_txtGrossReceipts);

            detailsCard.Controls.Add(FL("Salaries / Profession Income (₱):", lx + fw + 16, fy));
            _txtSalariesIncome = FT(fx + fw + 16, fy + 20, fw);
            _txtSalariesIncome.Text = "0";
            _txtSalariesIncome.TextChanged += (s, e) => RecalculateTax();
            detailsCard.Controls.Add(_txtSalariesIncome);
            fy += 54;

            detailsCard.Controls.Add(FL("Income from Real Property (₱):", lx, fy));
            _txtRealPropertyIncome = FT(fx, fy + 20, fw);
            _txtRealPropertyIncome.Text = "0";
            _txtRealPropertyIncome.TextChanged += (s, e) => RecalculateTax();
            detailsCard.Controls.Add(_txtRealPropertyIncome);

            detailsCard.Controls.Add(FL("Interest (late payment, ₱):", lx + fw + 16, fy));
            _txtInterest = FT(fx + fw + 16, fy + 20, fw);
            _txtInterest.Text = "0";
            _txtInterest.TextChanged += (s, e) => RecalculateTax();
            detailsCard.Controls.Add(_txtInterest);
            fy += 60;

            // ── Computed totals box ───────────────────────────────────────────
            var totalsBox = new Panel
            {
                Location = new Point(lx, fy),
                Size = new Size(fw * 2 + 16, 150),
                BackColor = Color.FromArgb(247, 248, 252),
            };
            totalsBox.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(BorderCol))
                    DrawRR(e.Graphics, pen, new Rectangle(0, 0, totalsBox.Width - 1, totalsBox.Height - 1), 8);
            };
            detailsCard.Controls.Add(totalsBox);

            _lblTaxable = TotalsRow(totalsBox, "Taxable amount:", 8);
            _lblBasicTax = TotalsRow(totalsBox, "Basic tax:", 30);
            _lblAdditionalTax = TotalsRow(totalsBox, "Additional tax:", 52);
            _lblCommunityDue = TotalsRow(totalsBox, "Community tax due:", 74, true);
            _lblTotalPaid = TotalsRow(totalsBox, "Total amount paid:", 96, true);
            _lblAmountWords = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8f, FontStyle.Italic),
                ForeColor = TextGray,
                AutoSize = false,
                Size = new Size(totalsBox.Width - 16, 30),
                Location = new Point(8, 118),
                BackColor = Color.Transparent,
            };
            totalsBox.Controls.Add(_lblAmountWords);
            fy += 162;

            detailsCard.Controls.Add(FL("Issued By:", lx, fy));
            _txtIssuedBy = FT(fx, fy + 20, fw * 2 + 16);
            _txtIssuedBy.Text = CurrentUser.Username;
            _txtIssuedBy.TextChanged += (s, e) => _previewPanel?.Invalidate();
            detailsCard.Controls.Add(_txtIssuedBy);
            fy += 62;

            // Print + Save buttons
            var btnPrint = MakeBtn("🖨️  Print Cedula", BtnNavy);
            btnPrint.Location = new Point(lx, fy + 10);
            btnPrint.Size = new Size(190, 40);
            btnPrint.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnPrint.Click += (s, e) => PrintCedula();
            detailsCard.Controls.Add(btnPrint);

            var btnSave = MakeBtn("💾  Save Only", BtnGreen);
            btnSave.Location = new Point(lx + 200, fy + 10);
            btnSave.Size = new Size(190, 40);
            btnSave.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnSave.Click += (s, e) => SaveCedula(false);
            detailsCard.Controls.Add(btnSave);
            fy += 60;

            detailsCard.Height = fy + 16;

            // ── RIGHT PANEL — Preview ─────────────────────────────────────────
            _previewPanel = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
            };
            _previewPanel.Paint += (s, e) => DrawCedulaPreview(e.Graphics, _previewPanel);
            this.Controls.Add(_previewPanel);

            // Resize handler
            this.Resize += (s, e) => LayoutControls(leftPanel);
            LayoutControls(leftPanel);
        }

        private Label TotalsRow(Panel box, string label, int y, bool bold = false)
        {
            box.Controls.Add(new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 8.5f, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = bold ? NavBlue : Color.FromArgb(60, 70, 100),
                AutoSize = true,
                Location = new Point(8, y),
                BackColor = Color.Transparent,
            });
            var val = new Label
            {
                Text = "₱0.00",
                Font = new Font("Segoe UI", 8.5f, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = bold ? NavBlue : Color.Black,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleRight,
                Size = new Size(box.Width - 160, 18),
                Location = new Point(150, y),
                BackColor = Color.Transparent,
            };
            box.Controls.Add(val);
            return val;
        }

        private void LayoutControls(Panel leftPanel)
        {
            int lw = 460;
            int margin = 24;
            int topY = 76;
            leftPanel.SetBounds(margin, topY, lw,
                this.ClientSize.Height - topY - margin);

            int previewX = margin + lw + 16;
            int previewW = this.ClientSize.Width - previewX - margin;
            int previewH = this.ClientSize.Height - topY - margin;
            _previewPanel.SetBounds(previewX, topY, previewW, previewH);

            // Search results panel — positioned under search box
            _searchResultPanel.SetBounds(
                margin + 16,
                topY + 90 + 36 + 10,
                lw - 32, 160);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  TAX COMPUTATION — live, driven by the income breakdown fields
        // ══════════════════════════════════════════════════════════════════════
        private void RecalculateTax()
        {
            decimal gross = ParseDec(_txtGrossReceipts?.Text);
            decimal salaries = ParseDec(_txtSalariesIncome?.Text);
            decimal realProperty = ParseDec(_txtRealPropertyIncome?.Text);
            decimal interest = ParseDec(_txtInterest?.Text);

            decimal taxable = gross + salaries + realProperty;
            decimal basicTax = (_chkExempted != null && _chkExempted.Checked) ? 1.00m : 5.00m;
            decimal additionalTax = Math.Floor(taxable / 1000m) * 1.00m;
            if (additionalTax > 5000m) additionalTax = 5000m;
            decimal communityTaxDue = basicTax + additionalTax;
            decimal totalPaid = communityTaxDue + interest;

            _taxableAmount = taxable;
            _basicTax = basicTax;
            _additionalTax = additionalTax;
            _communityTaxDue = communityTaxDue;
            _interestAmount = interest;
            _totalAmountPaid = totalPaid;
            _amountInWords = NumberToWordsPesos(totalPaid);

            if (_lblTaxable != null)
            {
                _lblTaxable.Text = "₱" + taxable.ToString("N2");
                _lblBasicTax.Text = "₱" + basicTax.ToString("N2");
                _lblAdditionalTax.Text = "₱" + additionalTax.ToString("N2");
                _lblCommunityDue.Text = "₱" + communityTaxDue.ToString("N2");
                _lblTotalPaid.Text = "₱" + totalPaid.ToString("N2");
                _lblAmountWords.Text = _amountInWords;
            }

            _previewPanel?.Invalidate();
        }

        private static decimal ParseDec(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return 0m;
            return decimal.TryParse(raw, out decimal v) ? v : 0m;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  NUMBER TO WORDS (pesos)
        // ══════════════════════════════════════════════════════════════════════
        private static readonly string[] Ones = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
            "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
        private static readonly string[] Tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

        private string NumberToWordsPesos(decimal amount)
        {
            long pesos = (long)Math.Floor(amount);
            int centavos = (int)Math.Round((amount - pesos) * 100m);
            string pesosWords = pesos == 0 ? "Zero" : NumberToWords(pesos);
            string result = $"{pesosWords} Peso{(pesos == 1 ? "" : "s")}";
            if (centavos > 0)
                result += $" and {NumberToWords(centavos)} Centavo{(centavos == 1 ? "" : "s")}";
            return result + " Only";
        }

        private string NumberToWords(long n)
        {
            if (n == 0) return "Zero";
            if (n < 0) return "Negative " + NumberToWords(-n);

            string words = "";
            if (n / 1000000 > 0) { words += NumberToWords(n / 1000000) + " Million "; n %= 1000000; }
            if (n / 1000 > 0) { words += NumberToWords(n / 1000) + " Thousand "; n %= 1000; }
            if (n / 100 > 0) { words += NumberToWords(n / 100) + " Hundred "; n %= 100; }
            if (n > 0)
            {
                if (n < 20) words += Ones[n];
                else
                {
                    words += Tens[n / 10];
                    if (n % 10 > 0) words += "-" + Ones[n % 10];
                }
            }
            return words.Trim();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  SEARCH
        // ══════════════════════════════════════════════════════════════════════
        private void SearchResidents()
        {
            string q = _txtSearch.Text.Trim();
            if (q.Length < 2) { _searchResultPanel.Visible = false; return; }

            SQLiteConnection conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand(@"
                        SELECT idResident,
                               CONCAT(FirstName,' ',
                                   IFNULL(MiddleName,''),' ',
                                   LastName) AS FullName,
                               Address
                        FROM resident
                        WHERE IsActive=1 AND (
                            FirstName LIKE @s OR LastName LIKE @s OR
                            CONCAT(FirstName,' ',LastName) LIKE @s)
                        LIMIT 10", conn);
                    cmd.Parameters.AddWithValue("@s", "%" + q + "%");
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            string name = r["FullName"].ToString()
                                .Replace("  ", " ").Trim();
                            string addr = r["Address"]?.ToString() ?? "";
                            int id = Convert.ToInt32(r["idResident"]);
                            _lstResults.Items.Add(
                                new ResidentItem(id, name, addr));
                        }
                    }
                    _searchResultPanel.Visible = _lstResults.Items.Count > 0;
                    _searchResultPanel.BringToFront();
                }
                catch { _searchResultPanel.Visible = false; }
            }
        }

        private void SelectFromList()
        {
            if (_lstResults.SelectedItem == null) return;
            var item = (ResidentItem)_lstResults.SelectedItem;
            _searchResultPanel.Visible = false;
            _txtSearch.Clear();
            LoadResidentById(item.Id);
        }

        private void LoadResidentById(int id)
        {
            SQLiteConnection conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand(@"
                        SELECT idResident,
                               CONCAT(FirstName,' ',
                                   IFNULL(MiddleName,''),' ',
                                   LastName) AS FullName,
                               DateOfBirth, Gender, Address,
                               IFNULL(CivilStatus,'Single') AS CivilStatus,
                               IFNULL(Occupation,'') AS Occupation,
                               PlaceOfBirth, Citizenship, Height, Weight, TIN, ICRNumber
                        FROM resident
                        WHERE idResident=@id AND IsActive=1", conn);
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return;
                        _residentId = id;
                        _fullName = r["FullName"].ToString()
                            .Replace("  ", " ").Trim();
                        _address = r["Address"]?.ToString() ?? "";
                        _gender = r["Gender"]?.ToString() ?? "";
                        _occupationFromResident = r["Occupation"]?.ToString() ?? "";
                        _civilStatusFromResident = r["CivilStatus"]?.ToString() ?? "Single";

                        // Pulled read-only fields — defensive in case the schema
                        // migration hasn't run yet in some environments.
                        try { _placeOfBirth = r["PlaceOfBirth"]?.ToString() ?? ""; } catch { _placeOfBirth = ""; }
                        try
                        {
                            string cit = r["Citizenship"]?.ToString() ?? "";
                            _citizenship = string.IsNullOrWhiteSpace(cit) ? "Filipino" : cit;
                        }
                        catch { _citizenship = "Filipino"; }
                        try { _height = r["Height"]?.ToString() ?? ""; } catch { _height = ""; }
                        try { _weight = r["Weight"]?.ToString() ?? ""; } catch { _weight = ""; }
                        try { _tin = r["TIN"]?.ToString() ?? ""; } catch { _tin = ""; }
                        try { _icrNumber = r["ICRNumber"]?.ToString() ?? ""; } catch { _icrNumber = ""; }

                        if (r["DateOfBirth"] != DBNull.Value)
                        {
                            _dob = Convert.ToDateTime(r["DateOfBirth"]);
                            int age = DateTime.Now.Year - _dob.Year;
                            if (DateTime.Now.DayOfYear < _dob.DayOfYear) age--;
                            _age = $"{age} years old";
                        }
                    }

                    // Update read-only labels
                    _lblName.Text = _fullName;
                    _lblLine2.Text = $"Age: {_age}   |   Sex: {_gender}   |   Civil Status: {_civilStatusFromResident}";
                    _lblLine3.Text = $"Height: {(string.IsNullOrWhiteSpace(_height) ? "—" : _height)}   |   Weight: {(string.IsNullOrWhiteSpace(_weight) ? "—" : _weight)}   |   TIN: {(string.IsNullOrWhiteSpace(_tin) ? "—" : _tin)}";
                    _lblLine4.Text = $"Place of birth: {(string.IsNullOrWhiteSpace(_placeOfBirth) ? "—" : _placeOfBirth)}   |   Citizenship: {_citizenship}";
                    _lblAddress.Text = "Address: " + _address;

                    // Pre-fill cedula-specific fields from resident data (still editable here)
                    _txtOccupation.Text = _occupationFromResident;
                    if (!string.IsNullOrWhiteSpace(_civilStatusFromResident) &&
                        _cboCivil.Items.Contains(_civilStatusFromResident))
                        _cboCivil.SelectedItem = _civilStatusFromResident;
                    else
                        _cboCivil.SelectedIndex = 0;

                    RecalculateTax();
                    _previewPanel.Invalidate();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading resident: " + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void GenerateCedulaNumber()
        {
            SQLiteConnection conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand(
                        "SELECT COUNT(*) FROM cedula WHERE YEAR(DateIssued)=YEAR(datetime('now'))", conn);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    _txtCedulaNo.Text = $"CTC-{DateTime.Now.Year}-{(count + 1):D4}";
                    _txtSerialNo.Text = $"SN-{DateTime.Now.Year}-{(count + 1):D4}";
                }
                catch
                {
                    _txtCedulaNo.Text = $"CTC-{DateTime.Now.Year}-0001";
                    _txtSerialNo.Text = $"SN-{DateTime.Now.Year}-0001";
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  SAVE
        // ══════════════════════════════════════════════════════════════════════
        private bool SaveCedula(bool silent)
        {
            if (_residentId == 0)
            {
                MessageBox.Show("Please search and select a resident first.",
                    "No Resident", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            RecalculateTax();

            SQLiteConnection conn = DatabaseConnection.GetConnection();
            if (conn == null) return false;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand(@"
                        INSERT INTO cedula
                            (ResidentID, CedulaNumber, SerialNumber, DateIssued, CivilStatus,
                             Occupation, GrossIncome, AmountPaid, IssuedBy, Remarks,
                             BasicTax, AdditionalTax, TaxableAmount, Interest, AmountInWords)
                        VALUES
                            (@rid, @num, @serial, @date, @civil,
                             @occ, @taxable, @totalpaid, @by, 'Issued',
                             @basic, @additional, @taxable2, @interest, @words)
                        ON DUPLICATE KEY UPDATE
                            DateIssued=@date, CivilStatus=@civil,
                            Occupation=@occ, GrossIncome=@taxable, AmountPaid=@totalpaid,
                            IssuedBy=@by, SerialNumber=@serial,
                            BasicTax=@basic, AdditionalTax=@additional,
                            TaxableAmount=@taxable2, Interest=@interest, AmountInWords=@words", conn);

                    cmd.Parameters.AddWithValue("@rid", _residentId);
                    cmd.Parameters.AddWithValue("@num", _txtCedulaNo.Text.Trim());
                    cmd.Parameters.AddWithValue("@serial", _txtSerialNo.Text.Trim());
                    cmd.Parameters.AddWithValue("@date", _dtpDate.Value.Date);
                    cmd.Parameters.AddWithValue("@civil", _cboCivil.SelectedItem?.ToString() ?? "Single");
                    cmd.Parameters.AddWithValue("@occ", _txtOccupation.Text.Trim());
                    cmd.Parameters.AddWithValue("@taxable", _taxableAmount);
                    cmd.Parameters.AddWithValue("@taxable2", _taxableAmount);
                    cmd.Parameters.AddWithValue("@totalpaid", _totalAmountPaid);
                    cmd.Parameters.AddWithValue("@by", _txtIssuedBy.Text.Trim());
                    cmd.Parameters.AddWithValue("@basic", _basicTax);
                    cmd.Parameters.AddWithValue("@additional", _additionalTax);
                    cmd.Parameters.AddWithValue("@interest", _interestAmount);
                    cmd.Parameters.AddWithValue("@words", _amountInWords);
                    cmd.ExecuteNonQuery();

                    if (!silent)
                        MessageBox.Show("Cedula saved successfully!",
                            "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving cedula:\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PRINT
        // ══════════════════════════════════════════════════════════════════════
        private void PrintCedula()
        {
            if (_residentId == 0)
            {
                MessageBox.Show("Please select a resident first.",
                    "No Resident", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Save first
            if (!SaveCedula(true)) return;

            var pd = new PrintDocument();
            pd.DefaultPageSettings.PaperSize =
                new PaperSize("A4", 827, 1169);
            pd.DefaultPageSettings.Margins =
                new Margins(60, 60, 60, 60);

            pd.PrintPage += (s, e) => DrawCedulaPrint(e.Graphics,
                e.MarginBounds.Width, e.MarginBounds.Height,
                e.MarginBounds.X, e.MarginBounds.Y);

            var preview = new PrintPreviewDialog
            {
                Document = pd,
                Text = "Cedula Print Preview",
                WindowState = FormWindowState.Maximized,
            };
            preview.ShowDialog();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  DRAW CEDULA  (shared by preview panel + print) — mirrors BIR Form 0016
        // ══════════════════════════════════════════════════════════════════════
        private void DrawCedulaPreview(Graphics g, Panel panel)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(Color.FromArgb(220, 220, 225));

            // A4-like proportions
            int docW = 580, docH = 860;
            float scaleX = (panel.Width - 48f) / docW;
            float scaleY = (panel.Height - 48f) / docH;
            float scale = Math.Min(scaleX, scaleY);

            float ox = (panel.Width - docW * scale) / 2f;
            float oy = (panel.Height - docH * scale) / 2f;

            // White paper shadow
            g.FillRectangle(new SolidBrush(Color.FromArgb(180, 0, 0, 0)),
                ox + 4, oy + 4, docW * scale, docH * scale);
            g.FillRectangle(Brushes.White, ox, oy, docW * scale, docH * scale);

            g.TranslateTransform(ox, oy);
            g.ScaleTransform(scale, scale);
            DrawCedulaDocument(g, 8, 8, docW - 16, docH - 16);
        }

        private void DrawCedulaPrint(Graphics g, int w, int h, int ox, int oy)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.TranslateTransform(ox, oy);
            DrawCedulaDocument(g, 0, 0, w, h);
        }

        private void DrawCedulaDocument(Graphics g, float x, float y, float w, float h)
        {
            // ── Document border ───────────────────────────────────────────────
            using (var pen = new Pen(Color.Black, 2))
                g.DrawRectangle(pen, x, y, w - 1, h - 1);
            using (var pen = new Pen(Color.Black, 0.5f))
                g.DrawRectangle(pen, x + 5, y + 5, w - 11, h - 11);

            float cx = x + w / 2f;
            float cur = y + 22;
            float lx = x + 20;
            float vx = x + 190;
            float vw = w - 210;

            // ── Header ────────────────────────────────────────────────────────
            DrawCentered(g, "Republic of the Philippines", cx, cur, 9f, FontStyle.Regular); cur += 14;
            DrawCentered(g, "Province of Davao del Sur  |  Municipality of Matanao", cx, cur, 9f, FontStyle.Regular); cur += 14;
            DrawCentered(g, "COMMUNITY TAX CERTIFICATE", cx, cur, 15f, FontStyle.Bold); cur += 18;
            DrawCentered(g, "(Cedula — BIR Form 0016)", cx, cur, 8.5f, FontStyle.Italic); cur += 15;
            DrawCentered(g, $"Issued at Matanao, Davao del Sur this {_dtpDate.Value:MMMM dd, yyyy}", cx, cur, 8f, FontStyle.Regular); cur += 14;

            using (var pen = new Pen(Color.Black, 2f))
                g.DrawLine(pen, x + 14, cur, x + w - 14, cur); cur += 4;
            using (var pen = new Pen(Color.Black, 0.5f))
                g.DrawLine(pen, x + 14, cur, x + w - 14, cur); cur += 10;

            // Cedula number + serial number, side by side
            using (var f = new Font("Segoe UI", 8f))
            using (var fb = new Font("Segoe UI", 8f, FontStyle.Bold))
            using (var b = new SolidBrush(Color.Black))
            {
                g.DrawString("Cedula No.  " + (_txtCedulaNo?.Text ?? ""), fb, b, new PointF(lx, cur));
                string serialLine = "Serial No.  " + (_txtSerialNo?.Text ?? "");
                SizeF sz = g.MeasureString(serialLine, fb);
                g.DrawString(serialLine, fb, b, new PointF(x + w - 14 - sz.Width, cur));
            }
            cur += 20;

            // ── Taxpayer information grid ─────────────────────────────────────
            DrawSection(g, "TAXPAYER INFORMATION", lx, cur); cur += 18;
            cur = DrawRow(g, "Name:", _fullName, lx, vx, vw, cur); cur += 4;
            cur = DrawRowDouble(g, "Sex:", _gender, "Civil Status:",
                _cboCivil?.SelectedItem?.ToString() ?? "", lx, vx, w, cur); cur += 4;
            cur = DrawRowDouble(g, "Height:", string.IsNullOrWhiteSpace(_height) ? "—" : _height,
                "Weight:", string.IsNullOrWhiteSpace(_weight) ? "—" : _weight, lx, vx, w, cur); cur += 4;
            cur = DrawRowDouble(g, "Place of Birth:", string.IsNullOrWhiteSpace(_placeOfBirth) ? "—" : _placeOfBirth,
                "Citizenship:", _citizenship, lx, vx, w, cur); cur += 4;
            cur = DrawRowDouble(g, "TIN:", string.IsNullOrWhiteSpace(_tin) ? "—" : _tin,
                "Age / DOB:", $"{_age}  ({(_dob == DateTime.MinValue ? "—" : _dob.ToString("MMM dd, yyyy"))})", lx, vx, w, cur); cur += 4;
            if (!string.IsNullOrWhiteSpace(_icrNumber))
            {
                cur = DrawRow(g, "ICR Number:", _icrNumber, lx, vx, vw, cur); cur += 4;
            }
            cur = DrawRow(g, "Occupation:", _txtOccupation?.Text ?? "", lx, vx, vw, cur); cur += 4;
            cur = DrawRow(g, "Address:", _address, lx, vx, vw, cur);
            cur += 16;

            // ── Tax breakdown table ────────────────────────────────────────────
            DrawSection(g, "BASIC TAX / ADDITIONAL TAX BREAKDOWN", lx, cur); cur += 18;
            cur = DrawTaxTableRow(g, "Gross receipts (business):", ParseDec(_txtGrossReceipts?.Text), lx, vx, vw, cur);
            cur = DrawTaxTableRow(g, "Salaries / profession income:", ParseDec(_txtSalariesIncome?.Text), lx, vx, vw, cur);
            cur = DrawTaxTableRow(g, "Income from real property:", ParseDec(_txtRealPropertyIncome?.Text), lx, vx, vw, cur);
            using (var pen = new Pen(Color.Black, 0.5f))
                g.DrawLine(pen, vx, cur, vx + vw, cur);
            cur += 4;
            cur = DrawTaxTableRow(g, "Taxable amount:", _taxableAmount, lx, vx, vw, cur, true);
            cur += 12;

            // ── Boxed totals section ───────────────────────────────────────────
            float boxH = 96;
            using (var pen = new Pen(Color.Black, 1f))
                g.DrawRectangle(pen, lx, cur, w - 40, boxH);
            float by = cur + 8;
            by = DrawTaxTableRow(g, "Basic tax:", _basicTax, lx + 6, vx, vw - 6, by);
            by = DrawTaxTableRow(g, "Additional tax:", _additionalTax, lx + 6, vx, vw - 6, by);
            using (var pen = new Pen(Color.Black, 0.5f))
                g.DrawLine(pen, vx, by, vx + vw - 6, by);
            by += 4;
            by = DrawTaxTableRow(g, "Community tax due:", _communityTaxDue, lx + 6, vx, vw - 6, by, true);
            by = DrawTaxTableRow(g, "Interest:", _interestAmount, lx + 6, vx, vw - 6, by);
            using (var pen = new Pen(Color.Black, 0.5f))
                g.DrawLine(pen, vx, by, vx + vw - 6, by);
            by += 4;
            by = DrawTaxTableRow(g, "Total amount paid:", _totalAmountPaid, lx + 6, vx, vw - 6, by, true);
            cur += boxH + 10;

            using (var f = new Font("Segoe UI", 8.5f, FontStyle.Italic))
            using (var b = new SolidBrush(Color.Black))
                g.DrawString(_amountInWords, f, b, new PointF(lx, cur));
            cur += 26;

            // ── Signatures + thumbprint ───────────────────────────────────────
            float sigW = 160f;
            float leftSig = x + 30;
            float rightSig = x + w - 30 - 90;

            using (var pen = new Pen(Color.Black, 0.5f))
                g.DrawLine(pen, leftSig, cur + 40, leftSig + sigW, cur + 40);
            DrawCentered(g, "Signature of Taxpayer", leftSig + sigW / 2f, cur + 46, 8f, FontStyle.Regular);

            using (var pen = new Pen(Color.Black, 1f))
                g.DrawRectangle(pen, rightSig, cur, 90, 90);
            DrawCentered(g, "Right", rightSig + 45, cur + 96, 7.5f, FontStyle.Regular);
            DrawCentered(g, "Thumbprint", rightSig + 45, cur + 108, 7.5f, FontStyle.Regular);
            cur += 130;

            string issuedBy = _txtIssuedBy?.Text ?? "";
            using (var pen = new Pen(Color.Black, 0.5f))
                g.DrawLine(pen, leftSig, cur, leftSig + sigW, cur);
            DrawCentered(g, issuedBy, leftSig + sigW / 2f, cur + 4, 8.5f, FontStyle.Bold);
            DrawCentered(g, "Issuing Officer", leftSig + sigW / 2f, cur + 18, 8f, FontStyle.Regular);
            cur += 40;

            // ── Footer ────────────────────────────────────────────────────────
            using (var pen = new Pen(Color.Black, 0.5f))
                g.DrawLine(pen, x + 14, cur, x + w - 14, cur);
            cur += 8;
            DrawCentered(g,
                "This Community Tax Certificate is issued pursuant to Sections 156-164 of the Local Government Code of 1991 (R.A. 7160)",
                cx, cur, 7f, FontStyle.Regular);
        }

        // ── Drawing helpers ───────────────────────────────────────────────────
        private void DrawCentered(Graphics g, string text, float cx, float y,
            float size, FontStyle style)
        {
            using (var f = new Font("Segoe UI", size, style))
            using (var b = new SolidBrush(Color.Black))
            {
                SizeF sz = g.MeasureString(text, f);
                g.DrawString(text, f, b, new PointF(cx - sz.Width / 2f, y));
            }
        }

        private void DrawSection(Graphics g, string text, float x, float y)
        {
            using (var f = new Font("Segoe UI", 8.5f, FontStyle.Bold | FontStyle.Underline))
            using (var b = new SolidBrush(Color.Black))
                g.DrawString(text, f, b, new PointF(x, y));
        }

        private float DrawRow(Graphics g, string label, string value,
            float lx, float vx, float vw, float y)
        {
            using (var lf = new Font("Segoe UI", 8.5f))
            using (var vf = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (var b = new SolidBrush(Color.Black))
            using (var pen = new Pen(Color.Black, 0.5f))
            {
                g.DrawString(label, lf, b, new PointF(lx, y));
                g.DrawString(value, vf, b, new PointF(vx, y));
                g.DrawLine(pen, vx, y + 13, vx + vw, y + 13);
            }
            return y + 17;
        }

        private float DrawRowDouble(Graphics g, string l1, string v1,
            string l2, string v2, float lx, float vx, float w, float y)
        {
            float mid = lx + (w / 2f);
            float vw1 = mid - vx - 20;
            float vx2 = mid + 70;
            float vw2 = lx + w - vx2 - 14;

            using (var lf = new Font("Segoe UI", 8.5f))
            using (var vf = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (var b = new SolidBrush(Color.Black))
            using (var pen = new Pen(Color.Black, 0.5f))
            {
                g.DrawString(l1, lf, b, new PointF(lx, y));
                g.DrawString(v1, vf, b, new PointF(vx, y));
                g.DrawLine(pen, vx, y + 13, vx + vw1, y + 13);

                g.DrawString(l2, lf, b, new PointF(mid, y));
                g.DrawString(v2, vf, b, new PointF(vx2, y));
                g.DrawLine(pen, vx2, y + 13, vx2 + vw2, y + 13);
            }
            return y + 17;
        }

        private float DrawTaxTableRow(Graphics g, string label, decimal amount,
            float lx, float vx, float vw, float y, bool bold = false)
        {
            using (var lf = new Font("Segoe UI", 8.5f, bold ? FontStyle.Bold : FontStyle.Regular))
            using (var b = new SolidBrush(Color.Black))
            {
                g.DrawString(label, lf, b, new PointF(lx, y));
                string amtStr = "₱" + amount.ToString("N2");
                SizeF sz = g.MeasureString(amtStr, lf);
                g.DrawString(amtStr, lf, b, new PointF(vx + vw - sz.Width, y));
            }
            return y + 18;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  UI HELPERS
        // ══════════════════════════════════════════════════════════════════════
        private Panel MakeCard(Point loc, Size size)
        {
            var p = new Panel { BackColor = BgCard, Location = loc, Size = size };
            p.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(BorderCol))
                    DrawRR(e.Graphics, pen,
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
            return b;
        }

        private static Label InfoLabel(string text, int x, int y, int w,
            Color color, float size, FontStyle style) =>
            new Label
            {
                Text = text,
                Font = new Font("Segoe UI", size, style),
                ForeColor = color,
                Location = new Point(x, y),
                Size = new Size(w, 20),
                BackColor = Color.Transparent,
            };

        private static Label FL(string text, int x, int y) =>
            new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 70, 100),
                AutoSize = true,
                Location = new Point(x, y),
                BackColor = Color.Transparent,
            };

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // CedulaForm
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "CedulaForm";
            this.Load += new System.EventHandler(this.CedulaForm_Load);
            this.ResumeLayout(false);

        }

        private void CedulaForm_Load(object sender, EventArgs e)
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

        // ── Helper class for ListBox items ────────────────────────────────────
        private class ResidentItem
        {
            public int Id { get; }
            public string Name { get; }
            public string Addr { get; }
            public ResidentItem(int id, string name, string addr)
            { Id = id; Name = name; Addr = addr; }
            public override string ToString() =>
                $"{Name}  —  {Addr}";
        }
    }
}