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

        public static bool operator ==(TradingDay left, TradingDay right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TradingDay left, TradingDay right)
        {
            return !left.Equals(right);
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

        public bool Equals(TradingDay other)
        {
            return this.Year == other.Year && this.Month == other.Month && this.Day == other.Day;
        }

        public override bool Equals(object? obj)
        {
            return obj is TradingDay other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Year, this.Month, this.Day);
        }
    }
}
