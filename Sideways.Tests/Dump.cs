namespace Sideways.Tests
{
    using System;
    using NUnit.Framework;

    public static class Dump
    {
        [Explicit]
        [Test]
        public static void Days()
        {
            foreach (var day in Database.ReadDays("DPLS", new DateTimeOffset(2020, 12, 01, 00, 00, 00, TimeSpan.Zero), new DateTimeOffset(2021, 03, 01, 00, 00, 00, TimeSpan.Zero)))
            {
                Console.WriteLine($"{day.Open} {day.Low} {day.High} {day.Close}");
            }
        }
    }
}
