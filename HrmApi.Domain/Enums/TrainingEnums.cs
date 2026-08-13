namespace HrmApi.Domain.Enums
{
    public static class TrainingCourseStatus
    {
        public const string Draft = "DRAFT";
        public const string Open = "OPEN";
        public const string Closed = "CLOSED";
    }

    public static class TrainingEnrollmentStatus
    {
        public const string Enrolled = "ENROLLED";
        public const string Completed = "COMPLETED";
        public const string Dropped = "DROPPED";
    }
}
