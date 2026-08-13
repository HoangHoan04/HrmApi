namespace HrmApi.Domain.Enums
{
    public static class WorkflowEntityType
    {
        public const string Leave = "LEAVE";
        public const string Ot = "OT";
        public const string Transfer = "TRANSFER";
        public const string Discipline = "DISCIPLINE";
        public const string RecruitmentRequest = "RECRUITMENT_REQUEST";
        public const string Complaint = "COMPLAINT";
    }

    public static class WorkflowApproverResolver
    {
        public const string Manager = "MANAGER";
        public const string Hr = "HR";
        public const string Role = "ROLE";
    }

    public static class WorkflowInstanceStatus
    {
        public const string Running = "RUNNING";
        public const string Approved = "APPROVED";
        public const string Rejected = "REJECTED";
        public const string Cancelled = "CANCELLED";
    }

    public static class WorkflowTaskStatus
    {
        public const string Pending = "PENDING";
        public const string Done = "DONE";
        public const string Skipped = "SKIPPED";
    }

    public static class WorkflowTaskAction
    {
        public const string Approve = "APPROVE";
        public const string Reject = "REJECT";
    }
}
