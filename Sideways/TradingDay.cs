namespace Sideways
{
    using System;

    public static class TradingDay
    {
        public static DateTimeOffset LastComplete
        {
            get
            {
                var date = DateTimeOffset.UtcNow;
                if (date.Hour < 21)
                {
                    date -= TimeSpan.FromDays(1);
                }

                while (!IsTradingDay(date))
                {
                    date -= TimeSpan.FromDays(1);
                }

                return date.Date;

                static bool IsTradingDay(DateTimeOffset candidate)
                {
                    return candidate switch
                    {
                        { Month: 1, Day: 1 } => false,
                        //// Presidents day third Monday in February
                        { Month: 2, DayOfWeek: DayOfWeek.Monday } => candidate.Day / 7 == 3,
                        { Month: 7, Day: 5 } => false,
                        //// Memorial Day 4th Monday in May
                        { Month: 5, DayOfWeek: DayOfWeek.Monday } => candidate.Day / 7 == 4,
                        //// Labor Day 1st Monday in September
                        { Month: 9, DayOfWeek: DayOfWeek.Monday } => candidate.Day < 7,
                        //// Thanksgiving Day 4th Thursday in November
                        { Month: 11, DayOfWeek: DayOfWeek.Thursday } => candidate.Day / 7 == 4,
                        { Month: 12, Day: 24 } => false,
                        { DayOfWeek: DayOfWeek.Saturday } => false,
                        { DayOfWeek: DayOfWeek.Sunday } => false,
                        _ => true,
                    };
                }
            }
        }
    }
}
