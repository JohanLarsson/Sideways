namespace Sideways
{
    using System;

    public readonly struct TradingDay : IEquatable<TradingDay>, IComparable<TradingDay>, IComparable
    {
        public readonly int Year;
        public readonly int Month;
        public readonly int Day;

        public TradingDay(int year, int month, int day)
        {
            this.Year = year;
            this.Month = month;
            this.Day = day;
        }

        public static bool operator ==(TradingDay left, TradingDay right) => left.Equals(right);

        public static bool operator !=(TradingDay left, TradingDay right) => !left.Equals(right);

        public static bool operator <(TradingDay left, TradingDay right) => left.CompareTo(right) < 0;

        public static bool operator >(TradingDay left, TradingDay right) => left.CompareTo(right) > 0;

        public static bool operator <=(TradingDay left, TradingDay right) => left.CompareTo(right) <= 0;

        public static bool operator >=(TradingDay left, TradingDay right) => left.CompareTo(right) >= 0;

        public static DateTimeOffset StartOfDay(DateTimeOffset t) => new(t.Year, t.Month, t.Day, 09, 30, 00, t.Offset);

        public static DateTimeOffset EndOfDay(DateTimeOffset t) => new(t.Year, t.Month, t.Day, 20, 00, 00, t.Offset);

        public static bool IsPreMarket(DateTimeOffset t) => t.TimeOfDay <= new TimeSpan(09, 30, 00);

        public static bool IsPostMarket(DateTimeOffset t) => t.TimeOfDay > new TimeSpan(16, 00, 00);

        public static bool IsOrdinaryHours(DateTimeOffset t) => t.TimeOfDay > new TimeSpan(09, 30, 00) && t.TimeOfDay <= new TimeSpan(16, 00, 00);

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

            return From(date);
        }

        public static TradingDay From(DateTimeOffset date) => new(date.Year, date.Month, date.Day);

        public static TradingDay Min(TradingDay x, TradingDay y) => x < y ? x : y;

        public static TradingDay Max(TradingDay x, TradingDay y) => x > y ? x : y;

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

        public override string ToString() => $"{this.Year}-{this.Month}-{this.Day}";

        public int CompareTo(TradingDay other)
        {
            var yearComparison = this.Year.CompareTo(other.Year);
            if (yearComparison != 0)
            {
                return yearComparison;
            }

            var monthComparison = this.Month.CompareTo(other.Month);
            if (monthComparison != 0)
            {
                return monthComparison;
            }

            return this.Day.CompareTo(other.Day);
        }

        public int CompareTo(object? obj)
        {
            if (obj is null)
            {
                return 1;
            }

            return obj is TradingDay other ? this.CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(TradingDay)}");
        }
    }
}
