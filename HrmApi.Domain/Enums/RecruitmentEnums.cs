namespace HrmApi.Domain.Enums
{
    public static class RecruitmentRequestLevel
    {
        public const string Company = "COMPANY";
        public const string Branch = "BRANCH";
        public const string Department = "DEPARTMENT";
        public const string Part = "PART";
    }

    public static class RecruitmentRequestStatus
    {
        public const string Draft = "DRAFT";
        public const string Pending = "PENDING";
        public const string Approved = "APPROVED";
        public const string Rejected = "REJECTED";
        public const string Closed = "CLOSED";
    }

    public static class HiringPlanStatus
    {
        public const string Draft = "DRAFT";
        public const string Open = "OPEN";
        public const string Closed = "CLOSED";
        public const string Cancelled = "CANCELLED";
    }

    public static class CandidateStatus
    {
        public const string New = "NEW";
        public const string Screening = "SCREENING";
        public const string Interview = "INTERVIEW";
        public const string Waitlist = "WAITLIST";
        public const string Offer = "OFFER";
        public const string Hired = "HIRED";
        public const string Rejected = "REJECTED";
        public const string Withdrawn = "WITHDRAWN";
    }

    public static class InterviewStatus
    {
        public const string Scheduled = "SCHEDULED";
        public const string Completed = "COMPLETED";
        public const string Cancelled = "CANCELLED";
        public const string NoShow = "NO_SHOW";
    }

    public static class HeadcountNodeType
    {
        public const string Company = "COMPANY";
        public const string Branch = "BRANCH";
        public const string Department = "DEPARTMENT";
        public const string Part = "PART";
        public const string Position = "POSITION";
    }

    /// <summary>Loại kênh nguồn tuyển — dùng phân loại / báo cáo.</summary>
    public static class HiringSourceChannel
    {
        public const string Referral = "REFERRAL";
        public const string Email = "EMAIL";
        public const string CareersSite = "CAREERS_SITE";
        public const string JobBoard = "JOBBOARD";
        public const string Social = "SOCIAL";
        public const string Agency = "AGENCY";
        public const string WalkIn = "WALK_IN";
        public const string Other = "OTHER";
    }
}

