using System.Data.SQLite;
using System;
using System.IO;
using System.Windows.Forms;

namespace BarangayManagementSystem
{
    public class DatabaseConnection
    {
        // ── Local SQLite database file (sits next to the .exe) ────────────
        private static readonly string DB_FILE = Path.Combine(
            Application.StartupPath,
            "BarangayDB.sqlite"
        );

        private static readonly string connectionString =
            $"Data Source={DB_FILE};Version=3;";

        // ─────────────────────────────────────────────────────────────────
        //  GetConnection — returns an OPEN SQLite connection, or null.
        // ─────────────────────────────────────────────────────────────────
        public static SQLiteConnection GetConnection()
        {
            try
            {
                var conn = new SQLiteConnection(connectionString);
                conn.Open();
                return conn;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Cannot open the local database.\n\n" +
                    $"File  : {DB_FILE}\n\n" +
                    $"Error : {ex.Message}\n\n" +
                    "Make sure 'BarangayDB.sqlite' is in the same folder as the application.",
                    "Database Connection Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        // ─────────────────────────────────────────────────────────────────
        //  TestConnection — call from your Connection Setup button
        // ─────────────────────────────────────────────────────────────────
        public static void TestConnection()
        {
            using (var conn = GetConnection())
            {
                if (conn != null)
                {
                    MessageBox.Show(
                        "Connection Successful!\n\n" +
                        "Database : BarangayDB.sqlite (local)\n" +
                        $"Version  : {conn.ServerVersion}",
                        "Test Result",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────
        //  Kept for backward compatibility — no longer used
        // ─────────────────────────────────────────────────────────────────
        public static void UpdateSettings(string newServer, string newDatabase,
            string newUsername, string newPassword, string newPort = "3306")
        {
            MessageBox.Show(
                "This application now uses a local SQLite database and no longer " +
                "connects to the online MySQL server.\n\n" +
                "There is nothing to configure.",
                "Not Needed",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static string CurrentServer => "Local SQLite File";
        public static string CurrentDatabase => DB_FILE;
        public static string CurrentUsername => "";
        public static string CurrentPort => "";
    }
}