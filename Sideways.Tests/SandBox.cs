namespace Sideways.Tests
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    [Explicit]
    public static class SandBox
    {
        [TestCase(06, 14)]
        [TestCase(06, 15)]
        [TestCase(06, 16)]
        [TestCase(06, 17)]
        [TestCase(06, 18)]
        [TestCase(06, 21)]
        [TestCase(06, 22)]
        [TestCase(06, 23)]
        [TestCase(06, 24)]
        [TestCase(06, 25)]
        public static void MergeMinutes(int month, int day)
        {
            var start = new DateTimeOffset(2021, month, day, 00, 00, 00, TimeSpan.Zero);
            var end = start.WithHourAndMinute(21, 00);
            var days = Database.ReadDays("TSLA", start, end);
            var minutes = Database.ReadMinutes("TSLA", start, end);
            var merged = minutes.Where(x => TradingDay.IsRegularHours(x.Time)).MergeBy((x, y) => x.Time.IsSameDay(y.Time)).Select(x => x.WithTime(x.Time.WithHourAndMinute(0, 0)));
            Console.WriteLine($"Actual c: {days[0].Close}");
            Console.WriteLine($"15:59 c: {minutes.SingleOrDefault(x => x.Time is { Hour: 15, Minute: 59 }).Close}");
            Console.WriteLine($"16:00 c: {minutes.SingleOrDefault(x => x.Time is { Hour: 16, Minute: 00 }).Close}");
            Console.WriteLine($"16:01 c: {minutes.SingleOrDefault(x => x.Time is { Hour: 16, Minute: 01 }).Close}");
            Console.WriteLine($"16:02 c: {minutes.SingleOrDefault(x => x.Time is { Hour: 16, Minute: 02 }).Close}");
            Console.WriteLine($"Calculated volume: {minutes.Sum(x => x.Volume)} actual {days[0].Volume}");
            CollectionAssert.AreEqual(days, merged);
        }
    }
}
