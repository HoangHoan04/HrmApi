using System;

namespace HrmApi.Application.Common.Helpers
{
    public static class BusinessDateHelper
    {
        private static readonly Lazy<TimeZoneInfo> VietnamTimeZone = new(ResolveVietnamTimeZone);

        public static TimeZoneInfo TimeZone => VietnamTimeZone.Value;

        public static DateTime NowLocal => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZone);

        public static DateOnly Today() => DateOnly.FromDateTime(NowLocal);

        public static DateTime ToUtc(DateOnly workDate, TimeSpan timeOfDay)
        {
            DateTime local = workDate.ToDateTime(TimeOnly.FromTimeSpan(timeOfDay));
            DateTime unspecified = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(unspecified, TimeZone);
        }

        private static TimeZoneInfo ResolveVietnamTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch (InvalidTimeZoneException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
        }
    }
}
