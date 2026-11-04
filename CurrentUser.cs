using System;
using System.Data.SQLite;

namespace BarangayManagementSystem
{
    public static class CurrentUser
    {
        public static int UserId { get; set; }
        public static string Username { get; set; }
        public static string Role { get; set; }
        public static string DisplayName { get; set; }
        public static byte[] ProfilePhoto { get; set; }
        public static bool IsLoggedIn { get; set; }

        public static void Login(int userId, string username, string role)
        {
            UserId = userId;
            Username = username;
            Role = role;
            DisplayName = username;
            ProfilePhoto = null;
            IsLoggedIn = true;
        }

        public static void LoadProfile()
        {
            try
            {
            using (var connection = DatabaseConnection.GetConnection())
                {
                    EnsureProfileColumns(connection);
            using (var command = new SQLiteCommand("SELECT DisplayName, Photo FROM users WHERE idUsers=@id", connection))
                {
                    command.Parameters.AddWithValue("@id", UserId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read()) return;
                        if (reader["DisplayName"] != DBNull.Value && !string.IsNullOrWhiteSpace(reader["DisplayName"].ToString()))
                            DisplayName = reader["DisplayName"].ToString();
                        object photo = reader["Photo"];
                        ProfilePhoto = photo == DBNull.Value ? null : photo as byte[];
                    }
                }
                    }
            }
            catch
            {
                // Older databases may not have profile columns yet.
            }
        }

        private static void EnsureProfileColumns(SQLiteConnection connection)
        {
            bool hasDisplayName = false;
            bool hasPhoto = false;
            using (var check = new SQLiteCommand("PRAGMA table_info(users)", connection))
            using (var reader = check.ExecuteReader())
            {
                while (reader.Read())
                {
                    string name = reader["name"].ToString();
                    hasDisplayName |= name.Equals("DisplayName", StringComparison.OrdinalIgnoreCase);
                    hasPhoto |= name.Equals("Photo", StringComparison.OrdinalIgnoreCase);
                }
            }
            if (!hasDisplayName) new SQLiteCommand("ALTER TABLE users ADD COLUMN DisplayName TEXT", connection).ExecuteNonQuery();
            if (!hasPhoto) new SQLiteCommand("ALTER TABLE users ADD COLUMN Photo BLOB", connection).ExecuteNonQuery();
        }

        public static void Logout()
        {
            UserId = 0;
            Username = string.Empty;
            Role = string.Empty;
            DisplayName = string.Empty;
            ProfilePhoto = null;
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