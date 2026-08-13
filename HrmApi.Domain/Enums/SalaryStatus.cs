namespace HrmApi.Domain.Enums
{
    public static class SalaryStatus
    {
        public const string Draft = "DRAFT";
        public const string Processing = "PROCESSING";
        public const string Approved = "APPROVED";
        public const string Paid = "PAID";
        public const string Cancelled = "CANCELLED";
    }

    public static class SalaryItemType
    {
        public const string Income = "INCOME";
        public const string Deduction = "DEDUCTION";
    }

    /// <summary>Loại phiếu điều chỉnh lương (khấu trừ / thu nhập thêm).</summary>
    public static class SlipKind
    {
        public const string Deduction = "DEDUCTION";
        public const string Addition = "ADDITION";
    }

    /// <summary>Giá trị DeductionSlipEntity.DeductionType.</summary>
    public static class DeductionSlipType
    {
        public const string Fine = "FINE";
        public const string Compensation = "COMPENSATION";
        public const string Other = "OTHER";
    }

    /// <summary>Giá trị CashAdditionSlipEntity.AdditionType.</summary>
    public static class AdditionSlipType
    {
        public const string Bonus = "BONUS";
        public const string Support = "SUPPORT";
        public const string Other = "OTHER";
    }

    public static class SalaryItemCode
    {
        public const string Basic = "BASIC";
        public const string Lunch = "LUNCH";
        public const string Transport = "TRANSPORT";
        public const string Kpi = "KPI";
        public const string Overtime = "OVERTIME";
        public const string Bonus = "BONUS";
        public const string OtherIncome = "OTHER_INCOME";
        public const string Bhxh = "BHXH";
        public const string Bhyt = "BHYT";
        public const string Bhtn = "BHTN";
        public const string Pit = "PIT";
        public const string Advance = "ADVANCE";
        public const string OtherDeduction = "OTHER_DEDUCTION";
    }

    public static class SlipStatus
    {
        public const string Draft = "DRAFT";
        public const string Pending = "PENDING";
        public const string Approved = "APPROVED";
        public const string Applied = "APPLIED";
        public const string Rejected = "REJECTED";
        public const string Cancelled = "CANCELLED";
    }

    public static class CurrencyCode
    {
        public const string Vnd = "VND";
        public const string Usd = "USD";
    }
}
