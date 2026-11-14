using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace BarangayManagementSystem
{
    public sealed class LoadingOverlay : Panel
    {
        private static readonly ConditionalWeakTable<Control, LoadingOverlay> Overlays =
            new ConditionalWeakTable<Control, LoadingOverlay>();

        private readonly Label _label;
        private readonly ProgressBar _progress;

        private LoadingOverlay()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.FromArgb(190, 18, 30, 75);
            Visible = false;
            Cursor = Cursors.WaitCursor;

            _label = new Label
            {
                AutoSize = true,
                Text = "Loading...",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            _progress = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30,
                Width = 220,
                Height = 18
            };
            Controls.Add(_label);
            Controls.Add(_progress);
            Resize += (s, e) => PositionContent();
        }

        public static void ShowLoading(Control parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (parent.InvokeRequired)
                throw new InvalidOperationException("ShowLoading must be called on the UI thread.");

            if (!Overlays.TryGetValue(parent, out LoadingOverlay overlay))
            {
                overlay = new LoadingOverlay { Dock = DockStyle.Fill };
                Overlays.Add(parent, overlay);
                parent.Controls.Add(overlay);
            }

            overlay.Visible = true;
            overlay.BringToFront();
            overlay.PositionContent();
        }

        public static void HideLoading(Control parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (parent.InvokeRequired)
                throw new InvalidOperationException("HideLoading must be called on the UI thread.");

            if (Overlays.TryGetValue(parent, out LoadingOverlay overlay))
                overlay.Visible = false;
        }

        private void PositionContent()
        {
            _label.Left = (ClientSize.Width - _label.Width) / 2;
            _label.Top = (ClientSize.Height - _progress.Height - _label.Height - 12) / 2;
            _progress.Left = (ClientSize.Width - _progress.Width) / 2;
            _progress.Top = _label.Bottom + 12;
        }
    }
}
