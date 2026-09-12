using System;
using System.IO;
using System.Text.Json;

namespace BarangayManagementSystem.Integration
{
    // One block of five fields — one database, one network path (LAN or Remote).
    public class DbCredential
    {
        public string Host { get; set; } = "";
        public string Port { get; set; } = "3306";
        public string Database { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";

        public bool IsFilledIn() =>
            !string.IsNullOrWhiteSpace(Host) &&
            !string.IsNullOrWhiteSpace(Database) &&
            !string.IsNullOrWhiteSpace(Username);

        // Raw MySqlConnector connection string. GuidFormat=None avoids
        // CRS's CHAR(36) audit/sync columns being mis-mapped to System.Guid.
        public string ToConnectionString() =>
            $"Server={Host};Port={Port};Database={Database};" +
            $"User={Username};Password={Password};GuidFormat=None;" +
            $"ConnectionTimeout=8;DefaultCommandTimeout=15;";
    }

    // A database has two possible paths: over the local barangay-hall
    // network, or over the public internet. Same database, two routes to it.
    public class DbCredentialProfile
    {
        public DbCredential Lan { get; set; } = new DbCredential();
        public DbCredential Remote { get; set; } = new DbCredential();
    }

    public enum ActiveNetworkProfile { Lan, Remote }

    public class ExternalDbConfig
    {
        public DbCredentialProfile OwnDb { get; set; } = new DbCredentialProfile();
        public DbCredentialProfile Crs { get; set; } = new DbCredentialProfile();   // read-only
        public DbCredentialProfile Ggms { get; set; } = new DbCredentialProfile();  // read + write

        // Which profile is currently live for ALL three databases at once —
        // matches the "one preset configures everything" pattern other
        // systems in this deployment already use.
        public ActiveNetworkProfile ActiveProfile { get; set; } = ActiveNetworkProfile.Remote;

        // Resolved credentials for whichever profile is currently active.
        // Everything that actually opens a connection (CrsClient, GgmsClient,
        // GgmsSyncService) should read these, never .Crs/.Ggms/.OwnDb directly.
        public DbCredential ActiveOwnDb => ActiveProfile == ActiveNetworkProfile.Lan ? OwnDb.Lan : OwnDb.Remote;
        public DbCredential ActiveCrs => ActiveProfile == ActiveNetworkProfile.Lan ? Crs.Lan : Crs.Remote;
        public DbCredential ActiveGgms => ActiveProfile == ActiveNetworkProfile.Lan ? Ggms.Lan : Ggms.Remote;

        // TODO(confirm with GGMS/other-system teams before go-live):
        //  - OfficeCode: your barangay's office_code as registered in GGMS.
        //  - ProjectCodePrefix: "BMS" is proposed in the task doc but not
        //    yet confirmed collision-free against other systems.
        //  - TransactionType / Status: exact strings GGMS expects. Inspect
        //    existing consolidated_transactions rows from another live
        //    system, or ask the GGMS team, before shipping this to production.
        public string OfficeCode { get; set; } = "";
        public string ProjectCodePrefix { get; set; } = "BMS";
        public string TransactionType { get; set; } = "BMS_FUND_RELEASE"; // CONFIRM
        public string TransactionStatus { get; set; } = "Completed";       // CONFIRM

        // Hard block vs warn-and-confirm when a release would exceed the
        // remaining GGMS budget and GGMS itself is reachable to verify.
        // Per the task doc, warn-and-confirm only applies when GGMS can't
        // be reached — a *confirmed* over-budget release should still block.
        public bool HardBlockOverBudget { get; set; } = true;

        private static readonly string ConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BarangaySystem");

        private static readonly string ConfigFile =
            Path.Combine(ConfigDir, "integration_config.json");

        // Optional dev-machine-only file for the "Load Preset" button on the
        // EDIT form (inside the system, not login). Never committed — see
        // .gitignore — and ships only as integration_presets.example.json
        // with fake values. If it's not present, the button just says so.
        private static readonly string PresetsFile =
            Path.Combine(ConfigDir, "integration_presets.local.json");

        public static ExternalDbConfig Load()
        {
            try
            {
                if (!File.Exists(ConfigFile)) return new ExternalDbConfig();
                var json = File.ReadAllText(ConfigFile);
                return JsonSerializer.Deserialize<ExternalDbConfig>(json) ?? new ExternalDbConfig();
            }
            catch
            {
                // Corrupt/unreadable config should never crash the app —
                // fall back to blank fields, same spirit as the offline
                // fallback rules for CRS/GGMS themselves.
                return new ExternalDbConfig();
            }
        }

        public void Save()
        {
            Directory.CreateDirectory(ConfigDir);
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFile, json);
        }

        public static ExternalDbConfig LoadPresetsOrNull()
        {
            try
            {
                if (!File.Exists(PresetsFile)) return null;
                var json = File.ReadAllText(PresetsFile);
                return JsonSerializer.Deserialize<ExternalDbConfig>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}