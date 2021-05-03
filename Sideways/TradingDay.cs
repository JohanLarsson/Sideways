namespace Sideways
{
    using System;

    public static class TradingDay
    {
        public static DateTimeOffset LastComplete
        {
            get
            {
                var today = DateTimeOffset.UtcNow.Date;
                return today switch
                {
                    { Month: 1, Day: 1 } => today.AddDays(-1),
                    { Month: 2, Day: 15 } => today.AddDays(-1),
                    { Month: 7, Day: 5 } => today.AddDays(-1),
                    { Month: 12, Day: 24 } => today.AddDays(-1),
                    { DayOfWeek: DayOfWeek.Saturday } => today.AddDays(-1),
                    { DayOfWeek: DayOfWeek.Sunday } => today.AddDays(-2),
                    { DayOfWeek: DayOfWeek.Monday, Hour: < 21 } => today.AddDays(-3),
                    { Hour: < 21 } => today.AddDays(-1),
                    _ => today,
                };
            }
        }
    }
}
