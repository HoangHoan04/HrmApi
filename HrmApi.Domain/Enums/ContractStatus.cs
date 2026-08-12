namespace HrmApi.Domain.Enums
{
    public static class ContractStatus
    {
        public const string Draft = "DRAFT";
        public const string PendingSign = "PENDING_SIGN";
        public const string Active = "ACTIVE";
        public const string ExpiringSoon = "EXPIRING_SOON";
        public const string Expired = "EXPIRED";
        public const string Terminated = "TERMINATED";
        public const string Liquidated = "LIQUIDATED";
    }
}
