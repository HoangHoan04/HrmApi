using HrmApi.Domain.Entities.Timekeeping;

namespace HrmApi.Application.Common.Helpers
{
    public static class WorkPatternHelper
    {
        public static bool IsEffectiveOn(EmployeeWorkPatternEntity pattern, DateOnly date)
        {
            if (!pattern.IsActive || pattern.IsDeleted)
            {
                return false;
            }

            if (date < pattern.EffectiveFrom)
            {
                return false;
            }

            if (pattern.EffectiveTo.HasValue && date > pattern.EffectiveTo.Value)
            {
                return false;
            }

            return true;
        }

        public static bool IsWorkDay(EmployeeWorkPatternEntity pattern, DateOnly date)
        {
            if (!IsEffectiveOn(pattern, date))
            {
                return false;
            }

            return date.DayOfWeek switch
            {
                DayOfWeek.Monday => pattern.WorkOnMonday,
                DayOfWeek.Tuesday => pattern.WorkOnTuesday,
                DayOfWeek.Wednesday => pattern.WorkOnWednesday,
                DayOfWeek.Thursday => pattern.WorkOnThursday,
                DayOfWeek.Friday => pattern.WorkOnFriday,
                DayOfWeek.Saturday => pattern.WorkOnSaturday,
                DayOfWeek.Sunday => pattern.WorkOnSunday,
                _ => false,
            };
        }

        public static bool RangesOverlap(
            DateOnly fromA,
            DateOnly? toA,
            DateOnly fromB,
            DateOnly? toB)
        {
            DateOnly endA = toA ?? DateOnly.MaxValue;
            DateOnly endB = toB ?? DateOnly.MaxValue;
            return fromA <= endB && fromB <= endA;
        }
    }
}
