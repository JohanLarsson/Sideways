namespace Sideways
{
    using System;
    using System.Globalization;

    public static class DateExtensions
    {
        public static bool IsSameWeek(this DateTimeOffset x, DateTimeOffset y) => x.Year == y.Year && Week(x) == Week(y);

        public static int Week(this DateTimeOffset date) => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date.Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }
}
