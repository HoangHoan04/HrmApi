using System;
using HrmApi.Application.Common.Helpers;
using HrmApi.Application.Common.Interfaces;

namespace HrmApi.Application.Common.Helpers
{
    public static class WorkedMinutesCalculator
    {
        public static int Compute(
            DateOnly workDate,
            DateTime checkInAt,
            DateTime checkOutAt,
            WorkWindowResult? window)
        {
            DateTime inAt = NormalizeUtc(checkInAt);
            DateTime outAt = NormalizeUtc(checkOutAt);
            if (outAt < inAt)
            {
                return 0;
            }

            double grossSeconds = (outAt - inAt).TotalSeconds;
            int grossMinutes = Math.Max(0, (int)Math.Floor(grossSeconds / 60.0));
            if (grossMinutes <= 0 && grossSeconds >= 30)
            {
                grossMinutes = 1;
            }

            if (window == null || grossMinutes <= 0)
            {
                return grossMinutes;
            }

            int breakMinutes = ResolveBreakOverlapMinutes(workDate, inAt, outAt, window);
            return Math.Max(0, grossMinutes - Math.Max(0, breakMinutes));
        }

        public static int ResolveBreakOverlapMinutes(
            DateOnly workDate,
            DateTime checkInAt,
            DateTime checkOutAt,
            WorkWindowResult window)
        {
            DateTime inAt = NormalizeUtc(checkInAt);
            DateTime outAt = NormalizeUtc(checkOutAt);

            if (window.BreakStartTime.HasValue && window.BreakEndTime.HasValue)
            {
                DateTime breakStart = BusinessDateHelper.ToUtc(workDate, window.BreakStartTime.Value);
                DateTime breakEnd = BusinessDateHelper.ToUtc(workDate, window.BreakEndTime.Value);
                if (breakEnd <= breakStart)
                {
                    breakEnd = breakEnd.AddDays(1);
                }

                DateTime overlapStart = inAt > breakStart ? inAt : breakStart;
                DateTime overlapEnd = outAt < breakEnd ? outAt : breakEnd;
                if (overlapEnd > overlapStart)
                {
                    return Math.Max(0, (int)Math.Floor((overlapEnd - overlapStart).TotalSeconds / 60.0));
                }

                return 0;
            }

            if (window.BreakMinutes > 0)
            {
                int gross = Math.Max(0, (int)Math.Floor((outAt - inAt).TotalSeconds / 60.0));
                return gross > window.BreakMinutes ? window.BreakMinutes : 0;
            }

            return 0;
        }

        private static DateTime NormalizeUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            };
        }
    }
}
