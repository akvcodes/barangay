using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace BarangayManagementSystem.Integration
{
    public static class IntegrationSchema
    {
        // Call once at app startup, right after DatabaseConnection is ready.
        // Uses CREATE TABLE IF NOT EXISTS / defensive column checks since
        // this app has no EF migrations — safe to call every launch.
        public static void EnsureSchema(SQLiteConnection conn)
        {
            using (var cmd = new SQLiteCommand(@"
                CREATE TABLE IF NOT EXISTS ggms_pending_transactions (
                    idPending           INTEGER PRIMARY KEY AUTOINCREMENT,
                    BeneficiaryId       TEXT,
                    CivilRegistryId     TEXT,
                    ProjectCode         TEXT,
                    ProjectDetailsId    TEXT,
                    ProjectName         TEXT,
                    OfficeId            TEXT,
                    OfficeName          TEXT,
                    FullName            TEXT,
                    FirstName           TEXT,
                    MiddleName          TEXT,
                    LastName            TEXT,
                    Barangay            TEXT,
                    HouseholdNo         TEXT,
                    TransactionType     TEXT,
                    Amount              DECIMAL(20,4) NOT NULL,
                    TransactionDate     TEXT NOT NULL,
                    Status              TEXT,
                    IsPushed            INTEGER NOT NULL DEFAULT 0,
                    PushAttempts        INTEGER NOT NULL DEFAULT 0,
                    LastPushError       TEXT,
                    CreatedAt           TEXT NOT NULL,
                    PushedAt            TEXT
                )", conn))
            {
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new SQLiteCommand(@"
                CREATE TABLE IF NOT EXISTS crs_resident_cache (
                    BeneficiaryId   TEXT PRIMARY KEY,
                    FirstName       TEXT,
                    LastName        TEXT,
                    MiddleName      TEXT,
                    FullName        TEXT,
                    Sex             TEXT,
                    Address         TEXT,
                    DateOfBirth     TEXT,
                    MaritalStatus   TEXT,
                    IsPwd           INTEGER,
                    IsSenior        INTEGER,
                    CachedAt        TEXT
                )", conn))
            {
                cmd.ExecuteNonQuery();
            }

            // Add the CRS beneficiary_id link to your existing resident table
            // if it isn't there yet. SQLite has no "ADD COLUMN IF NOT EXISTS",
            // so check pragma first.
            bool hasColumn = false;
            using (var cmd = new SQLiteCommand("PRAGMA table_info(resident)", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (string.Equals(reader["name"] as string, "CRSBeneficiaryId", StringComparison.OrdinalIgnoreCase))
                    {
                        hasColumn = true;
                        break;
                    }
                }
            }
            if (!hasColumn)
            {
                using var cmd = new SQLiteCommand(
                    "ALTER TABLE resident ADD COLUMN CRSBeneficiaryId TEXT", conn);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public static class GgmsPendingQueue
    {
        public static long Enqueue(SQLiteConnection conn, SQLiteTransaction txn, GgmsTransaction t)
        {
            using var cmd = new SQLiteCommand(@"
                INSERT INTO ggms_pending_transactions
                    (BeneficiaryId, CivilRegistryId, ProjectCode, ProjectDetailsId, ProjectName,
                     OfficeId, OfficeName, FullName, FirstName, MiddleName, LastName,
                     Barangay, HouseholdNo, TransactionType, Amount, TransactionDate, Status,
                     IsPushed, PushAttempts, CreatedAt)
                VALUES
                    (@bid,@crid,@pcode,@pdid,@pname,@oid,@oname,@full,@first,@mid,@last,
                     @brgy,@hh,@ttype,@amt,@tdate,@status, 0, 0, @created)", conn, txn);

            cmd.Parameters.AddWithValue("@bid", (object)t.BeneficiaryId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@crid", (object)t.CivilRegistryId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@pcode", (object)t.ProjectCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@pdid", (object)t.ProjectDetailsId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@pname", (object)t.ProjectName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@oid", (object)t.OfficeId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@oname", (object)t.OfficeName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@full", (object)t.FullName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@first", (object)t.FirstName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@mid", (object)t.MiddleName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@last", (object)t.LastName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@brgy", (object)t.Barangay ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@hh", (object)t.HouseholdNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ttype", (object)t.TransactionType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@amt", t.Amount);
            cmd.Parameters.AddWithValue("@tdate", t.TransactionDate.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@status", (object)t.Status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@created", DateTime.UtcNow.ToString("s"));
            cmd.ExecuteNonQuery();

            return conn.LastInsertRowId;
        }

        // "Spent so far" is always computed live from OUR OWN rows here —
        // never from GGMS's officeallocations.SpentAmount.
        public static decimal GetLocalSpentTotal(SQLiteConnection conn, string officeId, string projectDetailsId)
        {
            string sql = projectDetailsId == null
                ? "SELECT IFNULL(SUM(Amount),0) FROM ggms_pending_transactions WHERE OfficeId=@oid AND (ProjectDetailsId IS NULL OR ProjectDetailsId='')"
                : "SELECT IFNULL(SUM(Amount),0) FROM ggms_pending_transactions WHERE ProjectDetailsId=@pdid";

            using var cmd = new SQLiteCommand(sql, conn);
            if (projectDetailsId == null)
                cmd.Parameters.AddWithValue("@oid", officeId);
            else
                cmd.Parameters.AddWithValue("@pdid", projectDetailsId);

            return Convert.ToDecimal(cmd.ExecuteScalar());
        }

        public static List<(long Id, GgmsTransaction Txn)> GetUnpushed(SQLiteConnection conn, int max = 50)
        {
            var list = new List<(long, GgmsTransaction)>();
            using var cmd = new SQLiteCommand(
                "SELECT * FROM ggms_pending_transactions WHERE IsPushed=0 ORDER BY idPending ASC LIMIT @max", conn);
            cmd.Parameters.AddWithValue("@max", max);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                long pendingId = Convert.ToInt64(reader["idPending"]);
                var t = new GgmsTransaction
                {
                    BeneficiaryId = reader["BeneficiaryId"] as string,
                    CivilRegistryId = reader["CivilRegistryId"] as string,
                    ProjectCode = reader["ProjectCode"] as string,
                    ProjectDetailsId = reader["ProjectDetailsId"] as string,
                    ProjectName = reader["ProjectName"] as string,
                    OfficeId = reader["OfficeId"] as string,
                    OfficeName = reader["OfficeName"] as string,
                    FullName = reader["FullName"] as string,
                    FirstName = reader["FirstName"] as string,
                    MiddleName = reader["MiddleName"] as string,
                    LastName = reader["LastName"] as string,
                    Barangay = reader["Barangay"] as string,
                    HouseholdNo = reader["HouseholdNo"] as string,
                    TransactionType = reader["TransactionType"] as string,
                    Amount = Convert.ToDecimal(reader["Amount"]),
                    TransactionDate = DateTime.Parse(reader["TransactionDate"].ToString()),
                    Status = reader["Status"] as string,
                    SyncId = new Guid(pendingId.ToString("D32")).ToString()
                };
                list.Add((pendingId, t));
            }
            return list;
        }

        public static void MarkPushed(SQLiteConnection conn, long id)
        {
            using var cmd = new SQLiteCommand(
                "UPDATE ggms_pending_transactions SET IsPushed=1, PushedAt=@now WHERE idPending=@id", conn);
            cmd.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("s"));
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public static void RecordFailure(SQLiteConnection conn, long id, string error)
        {
            using var cmd = new SQLiteCommand(@"
                UPDATE ggms_pending_transactions
                SET PushAttempts = PushAttempts + 1, LastPushError = @err
                WHERE idPending=@id", conn);
            cmd.Parameters.AddWithValue("@err", error ?? "");
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }

    // Background push + budget-cap check. Never let a GGMS failure surface
    // as an error to the user — by the time this runs, the release has
    // already succeeded on the local database.
    public class GgmsSyncService
    {
        private readonly ExternalDbConfig _config;
        private static bool _pushInProgress; // simple re-entrancy guard

        public GgmsSyncService(ExternalDbConfig config)
        {
            _config = config;
        }

        // Call this fire-and-forget right after a release succeeds locally,
        // and again on periodic/full sync, and on a manual "Push Now" button.
        public async Task PushPendingAsync()
        {
            if (_pushInProgress) return;
            if (_config?.ActiveGgms == null || !_config.ActiveGgms.IsFilledIn()) return;

            _pushInProgress = true;
            try
            {
                var ggms = new GgmsClient(_config.ActiveGgms);
                using var conn = DatabaseConnection.GetConnection();
                if (conn == null) return;

                var unpushed = GgmsPendingQueue.GetUnpushed(conn);
                foreach (var (id, txn) in unpushed)
                {
                    try
                    {
                        await ggms.InsertTransactionAsync(txn);
                        GgmsPendingQueue.MarkPushed(conn, id);
                    }
                    catch (Exception ex)
                    {
                        // Leave it unpushed, log why, move to the next row —
                        // one bad row must never block the rest of the queue.
                        GgmsPendingQueue.RecordFailure(conn, id, ex.Message);
                    }
                }
            }
            finally
            {
                _pushInProgress = false;
            }
        }

        // Returns (allowed, warningMessage). Checks live GGMS if reachable,
        // otherwise falls back to a local view of the last-known allocation.
        // A CONFIRMED over-budget release blocks regardless of
        // HardBlockOverBudget; that flag only governs what happens when GGMS
        // can't be reached to confirm at all.
        public async Task<(bool Allowed, string Message)> CheckBudgetCapAsync(
            string officeId, string projectDetailsId, decimal proposedAmount)
        {
            using var conn = DatabaseConnection.GetConnection();
            decimal alreadyCommitted = conn != null
                ? GgmsPendingQueue.GetLocalSpentTotal(conn, officeId, projectDetailsId)
                : 0;

            decimal? remoteAllocated = null;
            bool ggmsReachable = false;

            if (_config?.ActiveGgms != null && _config.ActiveGgms.IsFilledIn())
            {
                try
                {
                    var ggms = new GgmsClient(_config.ActiveGgms);
                    remoteAllocated = string.IsNullOrEmpty(projectDetailsId)
                        ? await ggms.GetAllocatedAmountAsync(officeId)
                        : (await ggms.GetActiveProjectsAsync(officeId))
                            .Find(p => p.ProjectDetailsId == projectDetailsId)?.TotalBudget;
                    ggmsReachable = true;
                }
                catch
                {
                    ggmsReachable = false;
                }
            }

            if (remoteAllocated == null)
            {
                // Can't verify at all — warn but don't hard-block, since we
                // have no confirmed number to block against.
                return (true, "Could not verify remaining GGMS budget (offline or not configured). Proceeding without a live check.");
            }

            decimal remaining = remoteAllocated.Value - alreadyCommitted;
            if (proposedAmount > remaining)
            {
                if (ggmsReachable)
                {
                    // Confirmed violation — blocks regardless of the
                    // HardBlockOverBudget setting.
                    return (false, $"This release (₱{proposedAmount:N2}) would exceed the remaining GGMS-tracked budget (₱{remaining:N2}).");
                }
                return (!_config.HardBlockOverBudget,
                    $"Using a cached budget figure — this release may exceed the remaining amount (₱{remaining:N2}). Confirm before proceeding.");
            }

            return (true, null);
        }
    }
}