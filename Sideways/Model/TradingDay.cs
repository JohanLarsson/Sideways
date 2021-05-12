namespace Sideways
{
    using System;

    public readonly struct TradingDay : IEquatable<TradingDay>
    {
        public readonly int Year;
        public readonly int Month;
        public readonly int Day;

        private TradingDay(int year, int month, int day)
        {
            this.Year = year;
            this.Month = month;
            this.Day = day;
        }

        public static bool operator ==(TradingDay left, TradingDay right) => left.Equals(right);

        public static bool operator !=(TradingDay left, TradingDay right) => !left.Equals(right);

        public static DateTimeOffset EndOfDay(DateTimeOffset t) => new(t.Year, t.Month, t.Day, 20, 00, 00, t.Offset);

        public static bool IsPreMarket(DateTimeOffset t)
        {
            return t.Hour is >= 4 and < 9 ||
                   t is { Hour: 9, Minute: < 30 };
        }

        public static bool IsPostMarket(DateTimeOffset t)
        {
            return t.Hour > 16;
        }

        public static bool IsOrdinaryHours(DateTimeOffset t)
        {
            return !IsPreMarket(t) &&
                   !IsPostMarket(t);
        }

        public static TradingDay LastComplete()
        {
            var date = DateTimeOffset.UtcNow;
            if (date.Hour < 21)
            {
                date -= TimeSpan.FromDays(1);
            }

            while (!IsMatch(date))
            {
                date -= TimeSpan.FromDays(1);
            }

            return Create(date);
        }

        public static TradingDay Create(DateTime date) => new(date.Year, date.Month, date.Day);

        public static TradingDay Create(DateTimeOffset date) => new(date.Year, date.Month, date.Day);

        public static bool IsMatch(DateTimeOffset candidate)
        {
            return candidate switch
            {
                { Month: 1, Day: 1 } => false,
                //// Martin Luther King, Jr. Day third Monday in January
                { Month: 1, DayOfWeek: DayOfWeek.Monday, Day: > 14 and < 22 } => false,
                //// Presidents day third Monday in February
                { Month: 2, DayOfWeek: DayOfWeek.Monday, Day: > 14 and < 22 } => false,
                //// Memorial Day Last Monday in May
                { Month: 5, DayOfWeek: DayOfWeek.Monday, Day: > 24 } => false,
                //// Independence day
                { Month: 7, Day: 4 } => false,
                //// Labor Day 1st Monday in September
                { Month: 9, DayOfWeek: DayOfWeek.Monday, Day: < 8 } => false,
                //// Thanksgiving Day 4th Thursday in November
                { Month: 11, DayOfWeek: DayOfWeek.Thursday, Day: > 21 and < 29 } => false,
                { Month: 12, Day: 25 } => false,
                { DayOfWeek: DayOfWeek.Saturday } => false,
                { DayOfWeek: DayOfWeek.Sunday } => false,
                _ => true,
            };
        }

        public bool Equals(TradingDay other) => this.Year == other.Year && this.Month == other.Month && this.Day == other.Day;

        public override bool Equals(object? obj) => obj is TradingDay other && this.Equals(other);

        public override int GetHashCode() => HashCode.Combine(this.Year, this.Month, this.Day);
    }
}
