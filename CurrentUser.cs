using System;

namespace BarangayManagementSystem
{
    public static class CurrentUser
    {
        public static int UserId { get; set; }
        public static string Username { get; set; }
        public static string Role { get; set; }
        public static bool IsLoggedIn { get; set; }

        public static void Login(int userId, string username, string role)
        {
            UserId = userId;
            Username = username;
            Role = role;
            IsLoggedIn = true;
        }

        public static void Logout()
        {
            UserId = 0;
            Username = string.Empty;
            Role = string.Empty;
            IsLoggedIn = false;
        }

        // ── Role checks ───────────────────────────────────────────────────────

        public static bool IsSuperAdmin()
        {
            return Role != null &&
                   Role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsAdmin()
        {
            return Role != null &&
                   Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsOfficial()
        {
            return Role != null &&
                   Role.Equals("Official", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsUser()
        {
            return Role != null &&
                   Role.Equals("User", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsTreasurer()
        {
            return Role != null &&
                   Role.Equals("Treasurer", StringComparison.OrdinalIgnoreCase);
        }

        // ── Convenience helpers ───────────────────────────────────────────────

        /// <summary>
        /// Returns true for both SuperAdmin AND Admin.
        /// Use this when a feature is available to all admins.
        /// </summary>
        public static bool IsAdminOrAbove()
        {
            return IsSuperAdmin() || IsAdmin();
        }

        /// <summary>
        /// Returns true for SuperAdmin, Admin, and Official.
        /// Use this when a feature is available to all staff.
        /// </summary>
        public static bool IsStaff()
        {
            return IsSuperAdmin() || IsAdmin() || IsOfficial();
        }
    }
}