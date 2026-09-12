using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BarangayManagementSystem
{
    /// <summary>
    /// Welcome popup shown once after login.
    /// Displays user greeting and role-based pending notifications.
    /// </summary>
    public partial class WelcomeNotificationForm : Form
    {
        // Dot colors matching main menu theme
        private static readonly Color DotRed = Color.FromArgb(239, 68, 68);
        private static readonly Color DotAmber = Color.FromArgb(245, 158, 11);
        private static readonly Color DotBlue = Color.FromArgb(37, 99, 235);
        private static readonly Color DotGreen = Color.FromArgb(16, 185, 129);
        private static readonly Color DotPurple = Color.FromArgb(109, 40, 217);

        public WelcomeNotificationForm()
        {
            InitializeComponent();
            this.Load += WelcomeNotificationForm_Load;
        }

        private void WelcomeNotificationForm_Load(object sender, EventArgs e)
        {
            // Apply rounded corners to the form
            ApplyRoundedCorners(this, 16);

            // Populate header
            lblWelcomeName.Text = $"Welcome back, {CurrentUser.Username}!";
            lblRoleDate.Text = $"{CurrentUser.Role}  ·  {DateTime.Now:dddd, MMMM d, yyyy}";

            // Draw avatar with initials
            string initials = CurrentUser.Username.Length > 0
                ? CurrentUser.Username.Substring(0, 1).ToUpper() : "?";

            picAvatar.Paint += (s, pe) =>
            {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                pe.Graphics.Clear(Color.FromArgb(37, 99, 235));

                // Draw inner circle highlight
                using (var pen = new Pen(Color.FromArgb(80, 255, 255, 255), 2f))
                    pe.Graphics.DrawEllipse(pen, 3, 3, picAvatar.Width - 7, picAvatar.Height - 7);

                using (var f = new Font("Segoe UI", 18f, FontStyle.Bold))
                using (var b = new SolidBrush(Color.White))
                {
                    var sz = pe.Graphics.MeasureString(initials, f);
                    pe.Graphics.DrawString(initials, f, b,
                        (picAvatar.Width - sz.Width) / 2,
                        (picAvatar.Height - sz.Height) / 2);
                }
            };
            picAvatar.Invalidate();

            // Build and display notifications
            var items = BuildNotifications();
            foreach (var item in items)
                flowNotifications.Controls.Add(CreateNotifItem(item));

            // Adjust form size based on notification count
            int itemHeight = 62;
            int notificationsHeight = items.Count * itemHeight;
            int minBodyHeight = 100;
            int maxBodyHeight = 340;
            int bodyHeight = Math.Min(maxBodyHeight, Math.Max(minBodyHeight, notificationsHeight + 70));

            panelBody.Height = bodyHeight;
            this.Height = panelHeader.Height + bodyHeight + panelFooter.Height + 5;

            // Re-apply rounded corners after resize
            ApplyRoundedCorners(this, 16);
        }

        // Notification item structure
        private class NotifItem
        {
            public Color DotColor { get; set; }
            public string Title { get; set; }
            public string Meta { get; set; }
            public string Icon { get; set; }
            public string BadgeText { get; set; }
        }

        // Build notification list based on user role
        private List<NotifItem> BuildNotifications()
        {
            var list = new List<NotifItem>();

            // Birthday notifications for all roles
            var birthdays = GetTodaysBirthdays();
            foreach (var name in birthdays)
            {
                list.Add(new NotifItem
                {
                    DotColor = DotGreen,
                    Title = $"{name}'s birthday today!",
                    Meta = "Resident record - Send greetings",
                    Icon = "🎂",
                    BadgeText = "BIRTHDAY"
                });
            }

            if (CurrentUser.IsSuperAdmin() || CurrentUser.IsAdmin())
            {
                // Pending blotter reports
                int blotterCount = GetPendingCount("blotter");
                if (blotterCount > 0)
                {
                    list.Add(new NotifItem
                    {
                        DotColor = DotRed,
                        Title = $"{blotterCount} blotter report{(blotterCount > 1 ? "s" : "")} pending action",
                        Meta = "Requires immediate review and resolution",
                        Icon = "📋",
                        BadgeText = "URGENT"
                    });
                }

                // Pending clearance requests
                int clearanceCount = GetPendingCount("clearance");
                if (clearanceCount > 0)
                {
                    list.Add(new NotifItem
                    {
                        DotColor = DotAmber,
                        Title = $"{clearanceCount} clearance request{(clearanceCount > 1 ? "s" : "")} awaiting approval",
                        Meta = "Submitted by residents for verification",
                        Icon = "📄",
                        BadgeText = "PENDING"
                    });
                }

                // Pending certificate requests
                int certCount = GetPendingCount("certificate");
                if (certCount > 0)
                {
                    list.Add(new NotifItem
                    {
                        DotColor = DotAmber,
                        Title = $"{certCount} certificate request{(certCount > 1 ? "s" : "")} pending issuance",
                        Meta = "Awaiting officer signature and release",
                        Icon = "📜",
                        BadgeText = "PENDING"
                    });
                }

                // Backup reminder (SuperAdmin only)
                if (CurrentUser.IsSuperAdmin())
                {
                    int daysSinceBackup = GetDaysSinceLastBackup();
                    if (daysSinceBackup >= 3)
                    {
                        list.Add(new NotifItem
                        {
                            DotColor = DotBlue,
                            Title = $"Last database backup was {daysSinceBackup} day{(daysSinceBackup > 1 ? "s" : "")} ago",
                            Meta = "Consider running a database backup today",
                            Icon = "💾",
                            BadgeText = "REMINDER"
                        });
                    }
                }

                // New user registration requests (Admin only)
                int pendingUsers = GetPendingUserRegistrations();
                if (pendingUsers > 0 && !CurrentUser.IsSuperAdmin())
                {
                    list.Add(new NotifItem
                    {
                        DotColor = DotPurple,
                        Title = $"{pendingUsers} new user registration request{(pendingUsers > 1 ? "s" : "")}",
                        Meta = "Review and approve pending account requests",
                        Icon = "👥",
                        BadgeText = "NEW"
                    });
                }
            }
            else if (CurrentUser.IsOfficial())
            {
                int blotterCount = GetPendingCount("blotter");
                if (blotterCount > 0)
                {
                    list.Add(new NotifItem
                    {
                        DotColor = DotRed,
                        Title = $"{blotterCount} blotter report{(blotterCount > 1 ? "s" : "")} assigned for review",
                        Meta = "Please review and take appropriate action",
                        Icon = "⚠️",
                        BadgeText = "ACTION NEEDED"
                    });
                }
            }
            else
            {
                // Regular resident - show their pending requests
                int myCount = GetMyPendingDocuments();
                if (myCount > 0)
                {
                    list.Add(new NotifItem
                    {
                        DotColor = DotBlue,
                        Title = $"You have {myCount} pending document request{(myCount > 1 ? "s" : "")}",
                        Meta = "Check the status in your documents section",
                        Icon = "📬",
                        BadgeText = "PENDING"
                    });
                }
            }

            // Add system tip if no notifications
            if (list.Count == 0)
            {
                list.Add(new NotifItem
                {
                    DotColor = DotGreen,
                    Title = "All caught up!",
                    Meta = "No pending notifications at this time",
                    Icon = "✅",
                    BadgeText = "GOOD"
                });
            }

            return list;
        }

        // Create a styled notification card
        private Panel CreateNotifItem(NotifItem item)
        {
            var panel = new Panel
            {
                Width = 460,
                Height = 58,
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 8),
                Padding = new Padding(0, 0, 0, 0),
                Cursor = Cursors.Default
            };

            // Add shadow and border effect
            panel.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                // Draw border with subtle color
                using (var pen = new Pen(Color.FromArgb(220, 230, 240), 1))
                    e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);

                // Draw subtle shadow on right and bottom
                using (var pen = new Pen(Color.FromArgb(30, 0, 0, 0), 1))
                {
                    e.Graphics.DrawLine(pen, panel.Width - 1, 2, panel.Width - 1, panel.Height - 1);
                    e.Graphics.DrawLine(pen, 2, panel.Height - 1, panel.Width - 1, panel.Height - 1);
                }
            };

            ApplyRoundedCorners(panel, 10);

            // Icon (emoji)
            var iconLabel = new Label
            {
                Text = item.Icon ?? "📌",
                Font = new Font("Segoe UI", 14f),
                Location = new Point(12, 16),
                Size = new Size(32, 32),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter
            };
            panel.Controls.Add(iconLabel);

            // Status badge
            if (!string.IsNullOrEmpty(item.BadgeText))
            {
                var badge = new Label
                {
                    Text = item.BadgeText,
                    Font = new Font("Segoe UI", 7f, FontStyle.Bold),
                    ForeColor = GetBadgeColor(item.BadgeText),
                    Location = new Point(400, 8),
                    Size = new Size(48, 16),
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleRight
                };
                panel.Controls.Add(badge);
            }

            // Colored dot indicator
            var dot = new Panel
            {
                Width = 8,
                Height = 8,
                BackColor = item.DotColor,
                Location = new Point(48, 25),
            };
            MakeCircle(dot);
            panel.Controls.Add(dot);

            // Title text
            var lblTitle = new Label
            {
                Text = item.Title,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(64, 10),
                Size = new Size(340, 22),
                BackColor = Color.Transparent
            };
            panel.Controls.Add(lblTitle);

            // Meta/description text
            var lblMeta = new Label
            {
                Text = item.Meta,
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(64, 32),
                Size = new Size(380, 18),
                BackColor = Color.Transparent
            };
            panel.Controls.Add(lblMeta);

            return panel;
        }

        private Color GetBadgeColor(string badgeText)
        {
            switch (badgeText.ToUpper())
            {
                case "URGENT":
                case "ACTION NEEDED":
                    return Color.FromArgb(220, 38, 38);
                case "PENDING":
                    return Color.FromArgb(245, 158, 11);
                case "NEW":
                    return Color.FromArgb(16, 185, 129);
                case "REMINDER":
                    return Color.FromArgb(37, 99, 235);
                case "BIRTHDAY":
                    return Color.FromArgb(139, 92, 246);
                default:
                    return Color.FromArgb(100, 116, 139);
            }
        }

        private void MakeCircle(Panel p)
        {
            if (p.Width > 0 && p.Height > 0)
            {
                p.Region = Region.FromHrgn(
                    CreateRoundRectRgn(0, 0, p.Width, p.Height, p.Width, p.Height));
            }
        }

        private void ApplyRoundedCorners(Control control, int radius)
        {
            if (control.Width > 0 && control.Height > 0)
            {
                control.Region = Region.FromHrgn(
                    CreateRoundRectRgn(0, 0, control.Width, control.Height, radius, radius));
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // WinAPI for rounded corners
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);

        // =========================================================================
        // DATABASE QUERY HELPERS - Replace these with your actual database methods
        // =========================================================================

        /// <summary>
        /// Returns names of residents whose birthday is today.
        /// </summary>
        private List<string> GetTodaysBirthdays()
        {
            var names = new List<string>();
            // TODO: Implement actual database query
            // Example:
            // string sql = @"SELECT CONCAT(FirstName, ' ', LastName) as FullName 
            //                FROM Residents 
            //                WHERE MONTH(Birthdate) = MONTH(GETDATE()) 
            //                AND DAY(Birthdate) = DAY(GETDATE())";
            // using (var conn = DatabaseConnection.GetConnection())
            // using (var cmd = new SqlCommand(sql, conn))
            // using (var reader = cmd.ExecuteReader())
            // {
            //     while (reader.Read()) names.Add(reader["FullName"].ToString());
            // }
            return names;
        }

        /// <summary>
        /// Returns count of pending items for the given module.
        /// </summary>
        private int GetPendingCount(string module)
        {
            // TODO: Implement actual database query
            // Example:
            // string tableName = module == "blotter" ? "Blotters" : 
            //                    module == "clearance" ? "ClearanceRequests" : "CertificateRequests";
            // string sql = $"SELECT COUNT(*) FROM {tableName} WHERE Status = 'Pending'";
            // using (var conn = DatabaseConnection.GetConnection())
            // using (var cmd = new SqlCommand(sql, conn))
            // {
            //     return Convert.ToInt32(cmd.ExecuteScalar());
            // }
            return 0;
        }

        /// <summary>
        /// Returns how many days since the last backup was performed.
        /// </summary>
        private int GetDaysSinceLastBackup()
        {
            // TODO: Implement actual database query
            // Example:
            // string sql = "SELECT TOP 1 DATEDIFF(DAY, BackupDate, GETDATE()) FROM BackupLog ORDER BY BackupDate DESC";
            // using (var conn = DatabaseConnection.GetConnection())
            // using (var cmd = new SqlCommand(sql, conn))
            // {
            //     var result = cmd.ExecuteScalar();
            //     return result != DBNull.Value ? Convert.ToInt32(result) : 7;
            // }
            return 0;
        }

        /// <summary>
        /// Returns count of pending user registration requests.
        /// </summary>
        private int GetPendingUserRegistrations()
        {
            // TODO: Implement actual database query
            // string sql = "SELECT COUNT(*) FROM Users WHERE Status = 'Pending' AND Role = 'Resident'";
            // using (var conn = DatabaseConnection.GetConnection())
            // using (var cmd = new SqlCommand(sql, conn))
            // {
            //     return Convert.ToInt32(cmd.ExecuteScalar());
            // }
            return 0;
        }

        /// <summary>
        /// Returns pending document requests for the current resident user.
        /// </summary>
        private int GetMyPendingDocuments()
        {
            // TODO: Implement actual database query
            // string sql = @"SELECT COUNT(*) FROM DocumentRequests 
            //                WHERE UserId = @UserId AND Status = 'Pending'";
            // using (var conn = DatabaseConnection.GetConnection())
            // using (var cmd = new SqlCommand(sql, conn))
            // {
            //     cmd.Parameters.AddWithValue("@UserId", CurrentUser.UserId);
            //     return Convert.ToInt32(cmd.ExecuteScalar());
            // }
            return 0;
        }
    }
}