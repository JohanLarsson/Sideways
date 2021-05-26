namespace Sideways
{
    using System;
    using System.Globalization;

    public static class DateTimeOffsetExtensions
    {
        public static bool IsSameWeek(this DateTimeOffset x, DateTimeOffset y) => x.Year == y.Year && Week(x) == Week(y);

        public static bool IsSameDay(this DateTimeOffset x, DateTimeOffset y) => x.Year == y.Year && x.Month == y.Month && x.Day == y.Day;

        public static bool IsSameHour(this DateTimeOffset x, DateTimeOffset y) => IsSameDay(x, y) && x.Hour == y.Hour;

        public static int Week(this DateTimeOffset date) => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date.Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

        public static DateTimeOffset Min(DateTimeOffset x, DateTimeOffset y) => x < y ? x : y;

        public static DateTimeOffset Max(DateTimeOffset x, DateTimeOffset y) => x < y ? y : x;
    }
}
