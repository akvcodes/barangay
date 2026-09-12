using System.Drawing;
using System.Windows.Forms;

namespace BarangayManagementSystem
{
    public static class DataGridViewStyler
    {
        // Custom color scheme matching your main menu
        private static class Colors
        {
            // Dark blue shades (main menu colors)
            public static readonly Color PrimaryDark = Color.FromArgb(0, 33, 71);      // Dark navy blue
            public static readonly Color PrimaryMedium = Color.FromArgb(0, 51, 102);    // Medium navy
            public static readonly Color PrimaryLight = Color.FromArgb(25, 75, 125);    // Lighter navy
            public static readonly Color AccentBlue = Color.FromArgb(0, 102, 204);      // Bright accent blue

            // Neutral colors
            public static readonly Color White = Color.White;
            public static readonly Color OffWhite = Color.FromArgb(248, 249, 250);
            public static readonly Color LightGray = Color.FromArgb(240, 242, 245);
            public static readonly Color MediumGray = Color.FromArgb(220, 225, 230);
            public static readonly Color DarkGray = Color.FromArgb(128, 128, 128);
            public static readonly Color TextDark = Color.FromArgb(51, 51, 51);
        }

        // MAIN STYLE: Dark Blue Theme (matching your main menu)
        public static void ApplyMainMenuTheme(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.BackgroundColor = Colors.LightGray;
            dgv.BorderStyle = BorderStyle.None;

            // Column headers - Dark blue like main menu
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Colors.PrimaryDark;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Colors.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            dgv.ColumnHeadersHeight = 50;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Default cell style
            dgv.DefaultCellStyle.BackColor = Colors.White;
            dgv.DefaultCellStyle.ForeColor = Colors.TextDark;
            dgv.DefaultCellStyle.SelectionBackColor = Colors.AccentBlue;
            dgv.DefaultCellStyle.SelectionForeColor = Colors.White;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 10.5f);
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.DefaultCellStyle.Padding = new Padding(5);

            // Alternating rows for better readability
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Colors.OffWhite;

            // Row styling
            dgv.RowTemplate.Height = 45;
            dgv.RowHeadersVisible = false;

            // Grid styling
            dgv.GridColor = Colors.MediumGray;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            // Selection and interaction
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;

            // Layout
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.ReadOnly = true;

            // Additional styling for modern look
            dgv.RowsDefaultCellStyle.Padding = new Padding(5);
        }

        // VARIANT 1: Dark Blue with Gradient Header
        public static void ApplyGradientHeaderTheme(DataGridView dgv)
        {
            ApplyMainMenuTheme(dgv);

            // Enhanced header with gradient-like effect using darker shade
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Colors.PrimaryMedium;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Colors.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Colors.PrimaryMedium;

            // Slightly different alternating row color
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(252, 253, 255);
        }

        // VARIANT 2: Dark Blue with Light Header (Inverse)
        public static void ApplyLightHeaderTheme(DataGridView dgv)
        {
            ApplyMainMenuTheme(dgv);

            // Light header with dark text
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Colors.White;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Colors.PrimaryDark;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);

            // Add bottom border to header
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Colors.White;
        }

        // VARIANT 3: Compact Dark Blue (for dense data)
        public static void ApplyCompactTheme(DataGridView dgv)
        {
            ApplyMainMenuTheme(dgv);

            // Smaller padding for compact view
            dgv.ColumnHeadersHeight = 40;
            dgv.RowTemplate.Height = 35;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            dgv.DefaultCellStyle.Padding = new Padding(3);
        }

        // VARIANT 4: Card-Style Dark Blue (with rounded corners effect)
        public static void ApplyCardStyleTheme(DataGridView dgv)
        {
            ApplyMainMenuTheme(dgv);

            // Card-like appearance
            dgv.BackgroundColor = Colors.White;
            dgv.BorderStyle = BorderStyle.FixedSingle;
            dgv.GridColor = Colors.LightGray;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.None;

            // Add subtle cell spacing effect
            dgv.DefaultCellStyle.Padding = new Padding(8, 5, 8, 5);
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 5, 8, 5);

            // Slight shadow effect via color
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Colors.OffWhite;
        }

        // HELPER METHOD: Apply consistent font across all themes
        public static void ApplyFont(DataGridView dgv, Font font)
        {
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font(font, FontStyle.Bold);
            dgv.DefaultCellStyle.Font = font;
            dgv.RowsDefaultCellStyle.Font = font;
        }

        // HELPER METHOD: Toggle alternating row colors
        public static void ToggleAlternatingRows(DataGridView dgv, bool enable)
        {
            if (enable)
            {
                dgv.AlternatingRowsDefaultCellStyle.BackColor = Colors.OffWhite;
            }
            else
            {
                dgv.AlternatingRowsDefaultCellStyle.BackColor = Colors.White;
            }
        }

        // HELPER METHOD: Set column header alignment
        public static void SetHeaderAlignment(DataGridView dgv, DataGridViewContentAlignment alignment)
        {
            dgv.ColumnHeadersDefaultCellStyle.Alignment = alignment;
        }
    }
}