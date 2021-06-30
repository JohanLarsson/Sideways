namespace Sideways
{
    using System;
    using System.Globalization;

    public static class DateTimeOffsetExtensions
    {
        public static DateTimeOffset WithHourAndMinute(this DateTimeOffset t, HourAndMinute hm) => new(t.Year, t.Month, t.Day, hm.Hour, hm.Minute, t.Second, t.Millisecond, t.Offset);

        public static bool IsSameWeek(this DateTimeOffset x, DateTimeOffset y) => x.Year == y.Year && Week(x) == Week(y);

        public static bool IsSameDay(this DateTimeOffset x, DateTimeOffset y) => x.Year == y.Year && x.Month == y.Month && x.Day == y.Day;

        public static bool IsSameHour(this DateTimeOffset x, DateTimeOffset y) => IsSameDay(x, y) && x.Hour == y.Hour;

        public static bool IsBetween(this DateTimeOffset time, DateTimeOffset min, DateTimeOffset max) => time >= min && time <= max;

        public static int Week(this DateTimeOffset date) => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date.Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

        public static DateTimeOffset Min(DateTimeOffset x, DateTimeOffset y) => x < y ? x : y;

        public static DateTimeOffset Max(DateTimeOffset x, DateTimeOffset y) => x < y ? y : x;
    }
}
