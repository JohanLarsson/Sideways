namespace Sideways.Tests
{
    using System;
    using System.Globalization;
    using NUnit.Framework;

    [Explicit]
    public static class Dump
    {
        [Test]
        public static void DaysCompact()
        {
            foreach (var day in Database.ReadDays("DPLS", new DateTimeOffset(2020, 12, 01, 00, 00, 00, TimeSpan.Zero), new DateTimeOffset(2021, 03, 01, 00, 00, 00, TimeSpan.Zero)))
            {
                Console.WriteLine($"{day.Open} {day.Low} {day.High} {day.Close}");
            }
        }

        [Test]
        public static void DaysCreate()
        {
            foreach (var day in Database.ReadDays("EH", new DateTimeOffset(2021, 05, 19, 00, 00, 00, TimeSpan.Zero), new DateTimeOffset(2021, 06, 09, 00, 00, 00, TimeSpan.Zero)))
            {
                Console.WriteLine("new Candle(");
                Console.WriteLine($"    time: new DateTimeOffset({day.Time.Year}, {day.Time.Month}, {day.Time.Day}, {day.Time.Hour}, {day.Time.Minute}, {day.Time.Second}, 0, TimeSpan.Zero),");
                Console.WriteLine($"    open: {day.Open.ToString(CultureInfo.InvariantCulture)}f,");
                Console.WriteLine($"    high: {day.High.ToString(CultureInfo.InvariantCulture)}f,");
                Console.WriteLine($"    low: {day.Low.ToString(CultureInfo.InvariantCulture)}f,");
                Console.WriteLine($"    close: {day.Close.ToString(CultureInfo.InvariantCulture)}f,");
                Console.WriteLine($"    volume: {day.Volume}),");
            }
        }
    }
}
