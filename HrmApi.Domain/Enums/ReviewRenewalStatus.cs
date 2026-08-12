namespace HrmApi.Domain.Enums
{

    public static class ReviewRenewalStatus
    {
        public const string PendingReview = "PENDING_REVIEW";
        public const string PendingApproval = "PENDING_APPROVAL";
        public const string Approved = "APPROVED";
        public const string Rejected = "REJECTED";
        public const string Applied = "APPLIED";
    }


    public static class ReviewRecommendation
    {
        public const string Renew = "RENEW";
        public const string Convert = "CONVERT";
        public const string IncreaseSalary = "INCREASE_SALARY";
        public const string Terminate = "TERMINATE";
        public const string NoChange = "NO_CHANGE";
    }
}
