namespace HrmApi.Domain.Enums
{
    /// <summary>Trạng thái làm việc nhân viên (Employee.Status).</summary>
    public static class EmployeeWorkStatus
    {
        public const string Working = "WORKING";
        public const string Probation = "PROBATION";
        public const string Official = "OFFICIAL";
        public const string OnLeave = "ON_LEAVE";
        public const string Suspended = "SUSPENDED";
        public const string Resigned = "RESIGNED";
        public const string Retired = "RETIRED";
    }

    /// <summary>Loại hợp đồng trên hồ sơ NV (Employee.ContractType) — đồng bộ form Admin.</summary>
    public static class EmployeeContractType
    {
        public const string Probation = "PROBATION";
        public const string Indefinite = "INDEFINITE";
        public const string Definite = "DEFINITE";
        public const string Seasonal = "SEASONAL";
        public const string Collaborator = "COLLABORATOR";
        public const string Internship = "INTERNSHIP";
    }
}
