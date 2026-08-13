namespace HrmApi.Domain.Enums
{
    public static class ViolationSeverity
    {
        public const string Low = "LOW";
        public const string Medium = "MEDIUM";
        public const string High = "HIGH";
        public const string Critical = "CRITICAL";
    }

    public static class ViolationStatus
    {
        public const string Draft = "DRAFT";
        public const string Confirmed = "CONFIRMED";
        public const string Cancelled = "CANCELLED";
    }

    public static class PenaltyType
    {
        public const string Warning = "WARNING";
        public const string WrittenWarning = "WRITTEN_WARNING";
        public const string Fine = "FINE";
        public const string Suspension = "SUSPENSION";
        public const string Termination = "TERMINATION";
        public const string None = "NONE";
    }
}
