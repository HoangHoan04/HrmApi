using HrmApi.Domain.Enums;

namespace HrmApi.Application.Common.Helpers
{

    public static class VietnamPitCalculator
    {
        public const decimal PersonalDeduction = 11_000_000m;
        public const decimal DependentDeduction = 4_400_000m;

        private static readonly (decimal Ceiling, decimal Rate)[] Brackets =
        [
            (5_000_000m, 0.05m),
            (10_000_000m, 0.10m),
            (18_000_000m, 0.15m),
            (32_000_000m, 0.20m),
            (52_000_000m, 0.25m),
            (80_000_000m, 0.30m),
            (decimal.MaxValue, 0.35m),
        ];

        public static decimal ComputeMonthlyTax(decimal taxableIncome)
        {
            if (taxableIncome <= 0)
            {
                return 0;
            }

            decimal tax = 0;
            decimal prev = 0;
            foreach ((decimal ceiling, decimal rate) in Brackets)
            {
                if (taxableIncome <= prev)
                {
                    break;
                }

                decimal slice = Math.Min(taxableIncome, ceiling) - prev;
                if (slice > 0)
                {
                    tax += slice * rate;
                }

                prev = ceiling;
                if (taxableIncome <= ceiling)
                {
                    break;
                }
            }

            return Math.Round(tax, 0, MidpointRounding.AwayFromZero);
        }

        public static decimal ComputeTaxableIncome(
            decimal grossIncome,
            decimal insuranceEmployeeAmount,
            int dependentCount)
        {
            decimal dependents = Math.Max(0, dependentCount) * DependentDeduction;
            return Math.Max(0, grossIncome - insuranceEmployeeAmount - PersonalDeduction - dependents);
        }
    }

    public static class PayrollLineFactory
    {
        public static Domain.Entities.Payroll.SalaryLineItemEntity Income(
            string code, string name, decimal amount, int order, string? note = null)
        {
            return Line(SalaryItemType.Income, code, name, amount, order, note);
        }

        public static Domain.Entities.Payroll.SalaryLineItemEntity Deduction(
            string code, string name, decimal amount, int order, string? note = null)
        {
            return Line(SalaryItemType.Deduction, code, name, amount, order, note);
        }

        private static Domain.Entities.Payroll.SalaryLineItemEntity Line(
            string type, string code, string name, decimal amount, int order, string? note)
        {
            return new Domain.Entities.Payroll.SalaryLineItemEntity
            {
                ItemType = type,
                ItemCode = code,
                ItemName = name,
                Amount = Math.Round(Math.Abs(amount), 0, MidpointRounding.AwayFromZero),
                DisplayOrder = order,
                Note = note,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
            };
        }
    }
}
