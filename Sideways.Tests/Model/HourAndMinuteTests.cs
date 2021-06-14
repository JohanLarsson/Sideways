namespace Sideways.Tests.Model
{
    using System;
    using NUnit.Framework;

    public static class HourAndMinuteTests
    {
        [TestCase(09, 01, 09, 30)]
        [TestCase(09, 02, 09, 30)]
        [TestCase(09, 29, 09, 30)]
        [TestCase(09, 30, 09, 30)]
        [TestCase(09, 31, 10, 00)]
        [TestCase(09, 59, 10, 00)]
        [TestCase(10, 00, 10, 00)]
        [TestCase(11, 01, 12, 00)]
        [TestCase(11, 02, 12, 00)]
        [TestCase(11, 59, 12, 00)]
        [TestCase(12, 00, 12, 00)]
        public static void EndOfHourCandle(int h, int m, int expectedHour, int expectedMinute)
        {
            Assert.AreEqual(new HourAndMinute(expectedHour, expectedMinute), HourAndMinute.EndOfHourCandle(new DateTimeOffset(2021, 06, 01, h, m, 0, TimeSpan.Zero)));
        }

        [TestCase(09, 01, 09, 15)]
        [TestCase(09, 02, 09, 15)]
        [TestCase(09, 14, 09, 15)]
        [TestCase(09, 15, 09, 15)]
        [TestCase(09, 16, 09, 30)]
        [TestCase(09, 29, 09, 30)]
        [TestCase(09, 30, 09, 30)]
        [TestCase(09, 31, 09, 45)]
        [TestCase(09, 59, 10, 00)]
        [TestCase(10, 00, 10, 00)]
        [TestCase(11, 01, 11, 15)]
        [TestCase(11, 02, 11, 15)]
        [TestCase(11, 15, 11, 15)]
        [TestCase(11, 16, 11, 30)]
        [TestCase(11, 29, 11, 30)]
        [TestCase(11, 30, 11, 30)]
        [TestCase(11, 44, 11, 45)]
        [TestCase(11, 45, 11, 45)]
        [TestCase(11, 46, 12, 00)]
        [TestCase(11, 59, 12, 00)]
        [TestCase(12, 00, 12, 00)]
        public static void EndOfFifteenMinutesCandle(int h, int m, int expectedHour, int expectedMinute)
        {
            Assert.AreEqual(new HourAndMinute(expectedHour, expectedMinute), HourAndMinute.EndOfFifteenMinutesCandle(new DateTimeOffset(2021, 06, 01, h, m, 0, TimeSpan.Zero)));
        }
    }
}
