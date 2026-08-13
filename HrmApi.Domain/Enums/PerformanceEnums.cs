namespace HrmApi.Domain.Enums
{
    public static class ReviewCycleStatus
    {
        public const string Draft = "DRAFT";
        public const string Open = "OPEN";
        public const string Closed = "CLOSED";
    }

    public static class Performance360ReviewerType
    {
        public const string Self = "SELF";
        public const string Peer = "PEER";
        public const string Manager = "MANAGER";
    }

    public static class Performance360Status
    {
        public const string Draft = "DRAFT";
        public const string Submitted = "SUBMITTED";
    }
}
