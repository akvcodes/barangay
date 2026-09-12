using System.Data.SQLite;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BarangayManagementSystem
{
    public class AddIncomeForm : Form
    {
        private static readonly Color NavBlue = Color.FromArgb(26, 35, 126);
        private static readonly Color BgPage = Color.FromArgb(240, 242, 248);
        private static readonly Color BgCard = Color.White;
        private static readonly Color BorderCol = Color.FromArgb(210, 218, 235);
        private static readonly Color LabelCol = Color.FromArgb(55, 65, 100);
        private static readonly Color BtnGreen = Color.FromArgb(15, 157, 88);
        private static readonly Color BtnGray = Color.FromArgb(100, 110, 128);

        private ComboBox _cboSource;
        private TextBox _txtDescription;
        private NumericUpDown _numAmount;
        private DateTimePicker _dtpDate;
        private TextBox _txtRemarks;

        public AddIncomeForm()
        {
            this.Text = "Add Income";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = false;
            this.BackColor = BgPage;
            this.Font = new Font("Segoe UI", 9.5f);
            this.ClientSize = new Size(520, 520);
            BuildUI();
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
                Text = "Add Income / Fund Source",
                Font = new Font("Segoe UI", 15f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 12),
                BackColor = Color.Transparent,
            });
            header.Controls.Add(new Label
            {
                Text = "Record a new income or fund allocation",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(170, 195, 240),
                AutoSize = true,
                Location = new Point(22, 44),
                BackColor = Color.Transparent,
            });



            // ── Scroll ────────────────────────────────────────────────────────
            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = BgPage,
                Padding = new Padding(20, 20, 20, 20),
            };
            this.Controls.Add(scroll);
            scroll.BringToFront();

            // ── Card ──────────────────────────────────────────────────────────
            var card = new Panel { BackColor = BgCard, Location = new Point(0, 0), Size = new Size(460, 380) };
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                using (var bg = new SolidBrush(BgCard))
                using (var pen = new Pen(BorderCol, 1f))
                using (var path = RoundPath(r, 10))
                { e.Graphics.FillPath(bg, path); e.Graphics.DrawPath(pen, path); }
            };
            scroll.Controls.Add(card);
            scroll.Resize += (s, e) =>
            {
                int w = scroll.ClientSize.Width - scroll.Padding.Horizontal;
                if (w > 100) card.Width = w;
            };

            card.Controls.Add(new Label
            {
                Text = "💰  Income Details",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = NavBlue,
                AutoSize = true,
                Location = new Point(20, 14),
                BackColor = Color.Transparent,
            });
            var div = new Panel { Location = new Point(0, 44), Height = 1, BackColor = BorderCol };
            card.Controls.Add(div);
            card.Resize += (s, e) => div.Width = card.Width;

            const int lx = 20, fw = 420;
            int y = 58;

            // Source of Funds
            L(card, "Source of Funds:", lx, y);
            _cboSource = new ComboBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(lx, y + 20),
                Size = new Size(fw, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
            };
            _cboSource.Items.AddRange(new string[] {
                "LGU Budget Allocation",
                "National Government Fund",
                "Internal Revenue Allotment (IRA)",
                "Special Purpose Fund",
                "Donation / Grant",
                "Local Tax Collection",
                "Other Income",
            });
            _cboSource.SelectedIndex = 0;
            card.Controls.Add(_cboSource);
            y += 58;

            // Description
            L(card, "Description:", lx, y);
            _txtDescription = new TextBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(lx, y + 20),
                Size = new Size(fw, 28),
                BorderStyle = BorderStyle.FixedSingle,
            };
            card.Controls.Add(_txtDescription);
            y += 58;

            // Amount
            L(card, "Amount (₱):", lx, y);
            _numAmount = new NumericUpDown
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(lx, y + 20),
                Size = new Size(fw, 28),
                DecimalPlaces = 2,
                Maximum = 99999999,
                ThousandsSeparator = true,
            };
            card.Controls.Add(_numAmount);
            y += 58;

            // Date
            L(card, "Date Received:", lx, y);
            _dtpDate = new DateTimePicker
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(lx, y + 20),
                Size = new Size(fw, 28),
                Value = DateTime.Now,
            };
            card.Controls.Add(_dtpDate);
            y += 58;

            // Remarks
            L(card, "Remarks (Optional):", lx, y);
            _txtRemarks = new TextBox
            {
                Font = new Font("Segoe UI", 10f),
                Location = new Point(lx, y + 20),
                Size = new Size(fw, 28),
                BorderStyle = BorderStyle.FixedSingle,
            };
            card.Controls.Add(_txtRemarks);
            y += 58;

            // Buttons
            var btnSave = Btn("💾  Save Income", BtnGreen);
            btnSave.Location = new Point(lx, y);
            btnSave.Size = new Size(190, 42);
            btnSave.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            btnSave.Click += (s, e) => Save();
            card.Controls.Add(btnSave);

            var btnCancel = Btn("Cancel", BtnGray);
            btnCancel.Location = new Point(lx + 200, y);
            btnCancel.Size = new Size(110, 42);
            btnCancel.Click += (s, e) => this.Close();
            card.Controls.Add(btnCancel);

            card.Height = y + 60;
        }

        private void Save()
        {
            if (_numAmount.Value <= 0)
            {
                MessageBox.Show("Please enter an amount!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _numAmount.Focus(); return;
            }
            if (string.IsNullOrWhiteSpace(_txtDescription.Text))
            {
                MessageBox.Show("Please enter a description!", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtDescription.Focus(); return;
            }

            var conn = DatabaseConnection.GetConnection();
            if (conn == null) return;
            using (conn)
            {
                try
                {
                    var cmd = new SQLiteCommand(@"
                        INSERT INTO budget_transactions
                            (TransactionDate, Description, TransactionType,
                             Category, Amount, Source, ReferenceID, ReferenceType)
                        VALUES
                            (@date, @desc, 'Income',
                             @cat, @amt, @src, 0, 'Income')", conn);
                    cmd.Parameters.AddWithValue("@date", _dtpDate.Value.Date);
                    cmd.Parameters.AddWithValue("@desc", _txtDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@cat", _cboSource.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@amt", _numAmount.Value);
                    cmd.Parameters.AddWithValue("@src", _cboSource.SelectedItem.ToString());
                    cmd.ExecuteNonQuery();

                    MessageBox.Show(
                        $"✅  ₱{_numAmount.Value:N2} income recorded successfully!",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

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

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // AddIncomeForm
            // 
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.Name = "AddIncomeForm";
            this.Load += new System.EventHandler(this.AddIncomeForm_Load);
            this.ResumeLayout(false);

        }

        private void AddIncomeForm_Load(object sender, EventArgs e)
        {

        }
    }
}