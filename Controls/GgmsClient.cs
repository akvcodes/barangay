using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;

namespace BarangayManagementSystem.Integration
{
    public sealed class GgmsClient
    {
        private readonly DbCredential _credential;

        public GgmsClient(DbCredential credential)
        {
            _credential = credential ?? throw new ArgumentNullException(nameof(credential));
        }

        public async Task InsertTransactionAsync(GgmsTransaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            using var connection = new MySqlConnection(_credential.ToConnectionString());
            await connection.OpenAsync();
            using var dbTransaction = await connection.BeginTransactionAsync();

            long nextId;
            using (var idCommand = new MySqlCommand(
                "SELECT COALESCE(MAX(id), 0) + 1 FROM consolidated_transactions FOR UPDATE",
                connection, dbTransaction))
            {
                nextId = Convert.ToInt64(await idCommand.ExecuteScalarAsync());
            }

            const string sql = @"
                INSERT INTO consolidated_transactions
                    (id, beneficiary_id, civil_registry_id, project_code, project_details_id,
                     project_name, office_id, full_name, first_name, middle_name, last_name,
                     office_name, transaction_type, amount, transaction_date, status,
                     SyncId, barangay, household_no)
                VALUES
                    (@id, @beneficiary_id, @civil_registry_id, @project_code, @project_details_id,
                     @project_name, @office_id, @full_name, @first_name, @middle_name, @last_name,
                     @office_name, @transaction_type, @amount, @transaction_date, @status,
                     @sync_id, @barangay, @household_no)";

            using (var command = new MySqlCommand(sql, connection, dbTransaction))
            {
                command.Parameters.AddWithValue("@id", nextId);
                command.Parameters.AddWithValue("@beneficiary_id", DbValue(transaction.BeneficiaryId));
                command.Parameters.AddWithValue("@civil_registry_id", DbValue(transaction.CivilRegistryId));
                command.Parameters.AddWithValue("@project_code", DbValue(transaction.ProjectCode));
                command.Parameters.AddWithValue("@project_details_id", DbValue(transaction.ProjectDetailsId));
                command.Parameters.AddWithValue("@project_name", DbValue(transaction.ProjectName));
                command.Parameters.AddWithValue("@office_id", DbValue(transaction.OfficeId));
                command.Parameters.AddWithValue("@full_name", DbValue(transaction.FullName));
                command.Parameters.AddWithValue("@first_name", DbValue(transaction.FirstName));
                command.Parameters.AddWithValue("@middle_name", DbValue(transaction.MiddleName));
                command.Parameters.AddWithValue("@last_name", DbValue(transaction.LastName));
                command.Parameters.AddWithValue("@office_name", DbValue(transaction.OfficeName));
                command.Parameters.AddWithValue("@transaction_type", DbValue(transaction.TransactionType));
                command.Parameters.AddWithValue("@amount", transaction.Amount);
                command.Parameters.AddWithValue("@transaction_date", transaction.TransactionDate.Date);
                command.Parameters.AddWithValue("@status", DbValue(transaction.Status));
                command.Parameters.AddWithValue("@sync_id", transaction.SyncId);
                command.Parameters.AddWithValue("@barangay", DbValue(transaction.Barangay));
                command.Parameters.AddWithValue("@household_no", DbValue(transaction.HouseholdNo));
                await command.ExecuteNonQueryAsync();
            }

            await dbTransaction.CommitAsync();
        }

        public async Task<decimal> GetAllocatedAmountAsync(string officeCode)
        {
            const string sql = @"
                SELECT COALESCE(SUM(AllocatedAmount), 0)
                FROM officeallocations
                WHERE office_code = @office_code";

            using var connection = await OpenConnectionAsync();
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@office_code", officeCode);
            return Convert.ToDecimal(await command.ExecuteScalarAsync());
        }

        public async Task<List<GgmsProject>> GetActiveProjectsAsync(string officeCode)
        {
            const string sql = @"
                SELECT project_details_id, project, total_budget
                FROM project_details
                WHERE office_code = @office_code AND status = 'active'
                ORDER BY project_details_id";

            using var connection = await OpenConnectionAsync();
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@office_code", officeCode);

            var projects = new List<GgmsProject>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                projects.Add(new GgmsProject
                {
                    ProjectDetailsId = reader["project_details_id"] as string,
                    ProjectName = reader["project"] as string,
                    TotalBudget = reader["total_budget"] == DBNull.Value
                        ? 0m
                        : Convert.ToDecimal(reader["total_budget"])
                });
            }
            return projects;
        }

        private async Task<MySqlConnection> OpenConnectionAsync()
        {
            var connection = new MySqlConnection(_credential.ToConnectionString());
            try
            {
                await connection.OpenAsync();
                return connection;
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        private static object DbValue(string value) => (object)value ?? DBNull.Value;
    }

    public sealed class GgmsProject
    {
        public string ProjectDetailsId { get; set; }
        public string ProjectName { get; set; }
        public decimal TotalBudget { get; set; }
    }
}
