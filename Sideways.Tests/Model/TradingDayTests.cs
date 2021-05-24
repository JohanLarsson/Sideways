namespace Sideways.Tests.Model
{
    using System;
    using System.Linq;
    using NUnit.Framework;

    public static class TradingDayTests
    {
        private static readonly DateTimeOffset[] RealDates = Database.ReadDays("MSFT").Select(x => x.Time).ToArray();

        [TestCase(2020, 01, 01, false, Description = "New Year's Day")]
        [TestCase(2020, 01, 20, false, Description = "Martin Luther King, Jr. Day")]
        [TestCase(2020, 02, 17, false, Description = "Presidents' Day")]
        //// [TestCase(2020, 04, 10, false, Description = "Good Friday")]
        [TestCase(2020, 05, 25, false, Description = "Memorial Day")]
        [TestCase(2019, 07, 04, false, Description = "Independence Day")]
        [TestCase(2020, 09, 07, false, Description = "Labor Day")]
        [TestCase(2020, 11, 26, false, Description = "Thanksgiving Day")]
        [TestCase(2020, 12, 25, false, Description = "Christmas Day")]
        [TestCase(2021, 05, 08, false)]
        [TestCase(2021, 05, 09, false)]
        [TestCase(2021, 05, 10, true)]
        [TestCase(2021, 05, 11, true)]
        [TestCase(2021, 05, 12, true)]
        [TestCase(2021, 05, 13, true)]
        [TestCase(2021, 05, 14, true)]
        public static void IsMatch(int year, int month, int day, bool expected)
        {
            Assert.AreEqual(expected, TradingDay.IsMatch(new DateTimeOffset(year, month, day, 0, 0, 0, 0, TimeSpan.Zero)));
        }

        [TestCaseSource(nameof(RealDates))]
        public static void RealDays(DateTimeOffset date)
        {
            Assert.AreEqual(true, TradingDay.IsMatch(date));
        }
    }
}
