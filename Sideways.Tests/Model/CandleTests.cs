namespace Sideways.Tests.Model
{
    using System;

    using NUnit.Framework;

    public static class CandleTests
    {
        [Test]
        public static void Merge()
        {
            var c1 = new Candle(
                new DateTimeOffset(2021, 05, 22, 09, 30, 00, 0, TimeSpan.Zero),
                open: 1.3f,
                high: 1.4f,
                low: 1.1f,
                close: 1.2f,
                volume: 1);

            var c2 = new Candle(
                new DateTimeOffset(2021, 05, 22, 09, 31, 00, 0, TimeSpan.Zero),
                open: 2.3f,
                high: 2.4f,
                low: 2.1f,
                close: 2.2f,
                volume: 2);

            var expected = new Candle(
                new DateTimeOffset(2021, 05, 22, 09, 31, 00, 0, TimeSpan.Zero),
                open: 1.3f,
                high: 2.4f,
                low: 1.1f,
                close: 2.2f,
                volume: 3);

            Assert.AreEqual(expected, c1.Merge(c2));
            Assert.AreEqual(expected, c2.Merge(c1));
        }

        [Test]
        public static void Adjust()
        {
            var candle = new Candle(
                new DateTimeOffset(2021, 05, 22, 09, 30, 00, 0, TimeSpan.Zero),
                open: 1.3f,
                high: 1.4f,
                low: 1.1f,
                close: 1.2f,
                volume: 1);

            var expected = new Candle(
                new DateTimeOffset(2021, 05, 22, 09, 30, 00, 0, TimeSpan.Zero),
                open: 2.6f,
                high: 2.8f,
                low: 2.2f,
                close: 2.4f,
                volume: 1);

            Assert.AreEqual(expected, candle.Adjust(2));
        }

        [TestCase(06, 00, true)]
        [TestCase(09, 29, true)]
        [TestCase(09, 30, false)]
        [TestCase(10, 00, false)]
        [TestCase(16, 00, false)]
        public static void IsPreMarket(int hour, int minute, bool expected)
        {
            Assert.AreEqual(expected, TradingDay.IsPreMarket(new DateTimeOffset(2021, 06, 01, hour, minute, 0, TimeSpan.Zero)));
        }

        [TestCase(06, 00, false)]
        [TestCase(09, 29, false)]
        [TestCase(09, 30, false)]
        [TestCase(10, 00, false)]
        [TestCase(16, 00, true)]
        [TestCase(20, 00, true)]
        public static void IsPostMarket(int hour, int minute, bool expected)
        {
            Assert.AreEqual(expected, TradingDay.IsPostMarket(new DateTimeOffset(2021, 06, 01, hour, minute, 0, TimeSpan.Zero)));
        }

        [TestCase(06, 00, false)]
        [TestCase(09, 29, false)]
        [TestCase(09, 30, true)]
        [TestCase(10, 00, true)]
        [TestCase(15, 59, true)]
        [TestCase(16, 00, false)]
        [TestCase(20, 00, false)]
        public static void IsOrdinaryHours(int hour, int minute, bool expected)
        {
            Assert.AreEqual(expected, TradingDay.IsOrdinaryHours(new DateTimeOffset(2021, 06, 01, hour, minute, 0, TimeSpan.Zero)));
        }
    }
}
