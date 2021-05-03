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

                bool IsTradingDay(DateTimeOffset candidate)
                {
                    return candidate switch
                    {
                        { Month: 1, Day: 1 } => false,
                        { Month: 2, Day: 15 } => false,
                        { Month: 7, Day: 5 } => false,
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
