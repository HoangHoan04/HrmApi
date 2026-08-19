namespace HrmApi.Domain.Enums
{
    public static class AssetStatus
    {
        public const string Available = "AVAILABLE";
        public const string Assigned = "ASSIGNED";
        public const string Maintenance = "MAINTENANCE";
        public const string Retired = "RETIRED";
        public const string Lost = "LOST";
        public const string Disposed = "DISPOSED";
    }

    public static class AssetTicketType
    {
        public const string Issue = "ISSUE";
        public const string Return = "RETURN";
        public const string Repair = "REPAIR";
        public const string Transfer = "TRANSFER";
    }

    public static class AssetTicketStatus
    {
        public const string Draft = "DRAFT";
        public const string Done = "DONE";
        public const string Cancelled = "CANCELLED";
    }
}
