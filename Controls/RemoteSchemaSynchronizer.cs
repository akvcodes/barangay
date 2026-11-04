using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using MySqlConnector;

namespace BarangayManagementSystem.Integration
{
    public sealed class RemoteSchemaSyncResult
    {
        public int TablesCreated { get; set; }
        public int ColumnsAdded { get; set; }
        public List<string> Details { get; } = new List<string>();

        public string ToMessage()
        {
            var message = $"Tables created: {TablesCreated}\nColumns added: {ColumnsAdded}";
            if (Details.Count > 0)
                message += "\n\n" + string.Join("\n", Details);
            return message;
        }
    }

    public static class RemoteSchemaSynchronizer
    {
        public static RemoteSchemaSyncResult Synchronize(DbCredential credential)
        {
            if (credential == null || !credential.IsFilledIn())
                throw new InvalidOperationException("The online database connection is incomplete.");

            using (var local = DatabaseConnection.GetConnection())
            {
                if (local == null)
                    throw new InvalidOperationException("The local SQLite database could not be opened.");

                using (var remote = new MySqlConnection(credential.ToConnectionString()))
                {
                    remote.Open();
                    var result = new RemoteSchemaSyncResult();

                    foreach (var table in GetLocalTables(local))
                    {
                        var columns = GetLocalColumns(local, table);
                        if (columns.Count == 0) continue;

                        if (!RemoteTableExists(remote, table))
                        {
                            CreateRemoteTable(remote, table, columns);
                            result.TablesCreated++;
                            result.Details.Add("Created table: " + table);
                            continue;
                        }

                        var remoteColumns = GetRemoteColumns(remote, table);
                        foreach (var column in columns)
                        {
                            if (remoteColumns.Contains(column.Name)) continue;
                            AddRemoteColumn(remote, table, column);
                            result.ColumnsAdded++;
                            result.Details.Add("Added column: " + table + "." + column.Name);
                        }
                    }

                    return result;
                }
            }
        }

        private static List<string> GetLocalTables(SQLiteConnection connection)
        {
            var tables = new List<string>();
            using (var command = new SQLiteCommand(
                "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read()) tables.Add(reader.GetString(0));
            }
            return tables;
        }

        private static List<LocalColumn> GetLocalColumns(SQLiteConnection connection, string table)
        {
            var columns = new List<LocalColumn>();
            var escapedTable = table.Replace("'", "''");
            using (var command = new SQLiteCommand("PRAGMA table_info('" + escapedTable + "')", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    columns.Add(new LocalColumn
                    {
                        Name = reader["name"].ToString(),
                        SqliteType = reader["type"].ToString(),
                        NotNull = Convert.ToInt32(reader["notnull"]) == 1,
                        PrimaryKeyOrder = Convert.ToInt32(reader["pk"])
                    });
                }
            }
            return columns;
        }

        private static bool RemoteTableExists(MySqlConnection connection, string table)
        {
            using (var command = new MySqlCommand(
                "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = @table", connection))
            {
                command.Parameters.AddWithValue("@table", table);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private static HashSet<string> GetRemoteColumns(MySqlConnection connection, string table)
        {
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var command = new MySqlCommand(
                "SELECT column_name FROM information_schema.columns WHERE table_schema = DATABASE() AND table_name = @table", connection))
            {
                command.Parameters.AddWithValue("@table", table);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read()) columns.Add(reader.GetString(0));
                }
            }
            return columns;
        }

        private static void CreateRemoteTable(MySqlConnection connection, string table, List<LocalColumn> columns)
        {
            var definitions = new List<string>();
            foreach (var column in columns)
                definitions.Add(ColumnDefinition(column, false));

            var primaryKeys = columns.FindAll(c => c.PrimaryKeyOrder > 0);
            if (primaryKeys.Count > 0)
                definitions.Add("PRIMARY KEY (" + string.Join(", ", primaryKeys.ConvertAll(c => Quote(c.Name))) + ")");

            var sql = "CREATE TABLE " + Quote(table) + " (" + string.Join(", ", definitions) + ") ENGINE=InnoDB";
            using (var command = new MySqlCommand(sql, connection)) command.ExecuteNonQuery();
        }

        private static void AddRemoteColumn(MySqlConnection connection, string table, LocalColumn column)
        {
            // Existing remote tables may already contain rows. A newly added
            // column must therefore start nullable until it has been populated.
            var sql = "ALTER TABLE " + Quote(table) + " ADD COLUMN " + ColumnDefinition(column, true);
            using (var command = new MySqlCommand(sql, connection)) command.ExecuteNonQuery();
        }

        private static string ColumnDefinition(LocalColumn column, bool allowNull)
        {
            var type = ToMySqlType(column.SqliteType);
            var definition = Quote(column.Name) + " " + type;
            if (!allowNull && column.NotNull && column.PrimaryKeyOrder == 0) definition += " NOT NULL";
            if (column.PrimaryKeyOrder > 0 && type == "INT") definition += " AUTO_INCREMENT";
            return definition;
        }

        private static string ToMySqlType(string sqliteType)
        {
            var type = (sqliteType ?? "").ToUpperInvariant();
            if (type.Contains("INT")) return "INT";
            if (type.Contains("CHAR") || type.Contains("CLOB") || type.Contains("TEXT")) return "TEXT";
            if (type.Contains("REAL") || type.Contains("FLOA") || type.Contains("DOUB")) return "DOUBLE";
            if (type.Contains("BLOB")) return "LONGBLOB";
            if (type.Contains("DEC") || type.Contains("NUM")) return "DECIMAL(18,2)";
            return "TEXT";
        }

        private static string Quote(string identifier)
        {
            return "`" + identifier.Replace("`", "``") + "`";
        }

        private sealed class LocalColumn
        {
            public string Name { get; set; }
            public string SqliteType { get; set; }
            public bool NotNull { get; set; }
            public int PrimaryKeyOrder { get; set; }
        }
    }
}