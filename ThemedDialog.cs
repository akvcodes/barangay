using System;
using System.Drawing;
using System.Windows.Forms;

namespace BarangayManagementSystem
{
    internal static class ThemedDialog
    {
        private static readonly Color Navy = Color.FromArgb(26, 35, 126);
        private static readonly Color Page = Color.FromArgb(240, 242, 248);

        public static void Show(string message, string title, bool confirm)
        {
            using (var dialog = Build(message, title, confirm))
                dialog.ShowDialog();
        }

        public static bool Confirm(string message, string title)
        {
            using (var dialog = Build(message, title, true))
                return dialog.ShowDialog() == DialogResult.Yes;
        }

        private static Form Build(string message, string title, bool confirm)
        {
            var dialog = new Form
            {
                Text = title,
                ClientSize = new Size(410, confirm ? 190 : 170),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Page,
                Font = new Font("Segoe UI", 9f)
            };

            var header = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = Navy };
            header.Controls.Add(new Label
            {
                Text = title,
                AutoSize = true,
                Location = new Point(18, 11),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            });
            dialog.Controls.Add(header);
            dialog.Controls.Add(new Label
            {
                Text = message,
                Location = new Point(24, 64),
                Size = new Size(362, 54),
                ForeColor = Color.FromArgb(45, 49, 58),
                TextAlign = ContentAlignment.MiddleLeft
            });

            var ok = new Button { Text = confirm ? "YES" : "OK", Size = new Size(90, 34), Location = new Point(confirm ? 210 : 296, confirm ? 135 : 120), BackColor = Navy, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, DialogResult = confirm ? DialogResult.Yes : DialogResult.OK };
            ok.FlatAppearance.BorderSize = 0;
            dialog.Controls.Add(ok);
            dialog.AcceptButton = ok;

            if (confirm)
            {
                var cancel = new Button { Text = "CANCEL", Size = new Size(90, 34), Location = new Point(308, 135), BackColor = Color.White, ForeColor = Navy, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.No };
                cancel.FlatAppearance.BorderColor = Color.FromArgb(210, 218, 235);
                dialog.Controls.Add(cancel);
                dialog.CancelButton = cancel;
            }
            return dialog;
        }
    }
}