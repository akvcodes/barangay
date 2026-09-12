using System;
using System.Drawing;
using System.Windows.Forms;

namespace BarangayManagementSystem
{
    public class CustomTextBox : TextBox
    {
        private Color _borderColor = Color.DarkBlue;
        private int _borderSize = 2;
        private bool _isFocused = false;

        public CustomTextBox()
        {
            this.BorderStyle = BorderStyle.None;
            this.BackColor = Color.White;
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            // Handle focus events for optional focus effect
            this.Enter += (s, e) => { _isFocused = true; this.Invalidate(); };
            this.Leave += (s, e) => { _isFocused = false; this.Invalidate(); };
        }

        public Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; this.Invalidate(); }
        }

        public int BorderSize
        {
            get { return _borderSize; }
            set { _borderSize = value; this.Invalidate(); }
        }

        public Color FocusBorderColor { get; set; } = Color.FromArgb(52, 152, 219);
        public bool ChangeBorderOnFocus { get; set; } = true;

        protected override void OnPaint(PaintEventArgs e)
        {
            // Draw the textbox background
            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillRectangle(brush, 0, 0, this.Width, this.Height);
            }

            // Draw the text
            using (SolidBrush textBrush = new SolidBrush(this.ForeColor))
            {
                Rectangle textRect = new Rectangle(4, 2, this.Width - 8, this.Height - 4);
                TextRenderer.DrawText(e.Graphics, this.Text, this.Font, textRect, this.ForeColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            }

            // Determine border color based on focus
            Color currentBorderColor = _borderColor;
            if (ChangeBorderOnFocus && _isFocused)
            {
                currentBorderColor = FocusBorderColor;
            }

            // Draw custom border
            using (Pen pen = new Pen(currentBorderColor, _borderSize))
            {
                Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            this.Invalidate();
        }
    }
}