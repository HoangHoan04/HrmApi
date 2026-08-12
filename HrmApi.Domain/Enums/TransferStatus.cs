namespace HrmApi.Domain.Enums
{

    public static class TransferStatus
    {
        public const string Pending = "PENDING";
        public const string Approved = "APPROVED";
        public const string Rejected = "REJECTED";
        public const string Cancelled = "CANCELLED";
        public const string Applied = "APPLIED";
    }


    public static class TransferType
    {
        public const string InternalTransfer = "INTERNAL_TRANSFER";
        public const string Secondment = "SECONDMENT";
        public const string Rotation = "ROTATION";
        public const string CompanyTransfer = "COMPANY_TRANSFER";
        public const string BranchTransfer = "BRANCH_TRANSFER";
        public const string Promotion = "PROMOTION";
        public const string Demotion = "DEMOTION";
        public const string Dismissal = "DISMISSAL";
    }

    public static class TransferChangeType
    {
        public const string Company = "COMPANY";
        public const string Branch = "BRANCH";
        public const string Department = "DEPARTMENT";
        public const string Part = "PART";
        public const string Position = "POSITION";
        public const string Mixed = "MIXED";
    }
}
