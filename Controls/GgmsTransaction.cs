using System;

namespace BarangayManagementSystem.Integration
{
    public sealed class GgmsTransaction
    {
        public string BeneficiaryId { get; set; }
        public string CivilRegistryId { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectDetailsId { get; set; }
        public string ProjectName { get; set; }
        public string OfficeId { get; set; }
        public string OfficeName { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Barangay { get; set; }
        public string HouseholdNo { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; }
        public string SyncId { get; set; } = Guid.NewGuid().ToString();
    }
}
