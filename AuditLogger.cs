using System.Data.SQLite;
using System;

namespace BarangayManagementSystem
{
    /// <summary>
    /// Call AuditLogger.Log(...) anywhere in the app to record an action.
    /// </summary>
    public static class AuditLogger
    {
        public static void Log(string action, string module, string description)
        {
            try
            {
                var conn = DatabaseConnection.GetConnection();
                if (conn == null) return;
                using (conn)
                {
                    var cmd = new SQLiteCommand(@"
                        INSERT INTO auditlog
                            (UserID, Username, Role, Action, Module, Description, IPAddress)
                        VALUES
                            (@uid, @uname, @role, @action, @module, @desc, @ip)", conn);
                    cmd.Parameters.AddWithValue("@uid", CurrentUser.IsLoggedIn ? (object)CurrentUser.UserId : DBNull.Value);
                    cmd.Parameters.AddWithValue("@uname", CurrentUser.IsLoggedIn ? CurrentUser.Username : "System");
                    cmd.Parameters.AddWithValue("@role", CurrentUser.IsLoggedIn ? CurrentUser.Role : "System");
                    cmd.Parameters.AddWithValue("@action", action);
                    cmd.Parameters.AddWithValue("@module", module);
                    cmd.Parameters.AddWithValue("@desc", description);
                    cmd.Parameters.AddWithValue("@ip", GetLocalIP());
                    cmd.ExecuteNonQuery();
                }
            }
            catch { /* never crash the app because of a log failure */ }
        }

        private static string GetLocalIP()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        return ip.ToString();
            }
            catch { }
            return "unknown";
        }
    }
}