namespace HrmApi.Domain.Enums
{
    public enum AttendanceComplaintStatus
    {
        PENDING,
        APPROVED,
        REJECTED,
        CANCELLED
    }

    public enum AttendanceComplaintType
    {
        FORGOT_CHECK_IN,
        FORGOT_CHECK_OUT,
        FORGOT_BOTH,
        WRONG_TIME,
        OTHER
    }
}
