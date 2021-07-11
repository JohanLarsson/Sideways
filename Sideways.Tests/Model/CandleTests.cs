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
        public static void MergeWhenZeroVolume()
        {
            var c1 = new Candle(
                new DateTimeOffset(2021, 05, 22, 09, 30, 00, 0, TimeSpan.Zero),
                open: 0f,
                high: 0f,
                low: 0f,
                close: 0f,
                volume: 0);

            var c2 = new Candle(
                new DateTimeOffset(2021, 05, 22, 09, 31, 00, 0, TimeSpan.Zero),
                open: 1.3f,
                high: 1.4f,
                low: 1.1f,
                close: 1.2f,
                volume: 1);

            var expected = new Candle(
                new DateTimeOffset(2021, 05, 22, 09, 31, 00, 0, TimeSpan.Zero),
                open: 1.3f,
                high: 1.4f,
                low: 1.1f,
                close: 1.2f,
                volume: 1);

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

        [TestCase(09, 01, 09, 02, true)]
        [TestCase(09, 01, 09, 30, true)]
        [TestCase(09, 01, 09, 31, false)]
        [TestCase(09, 31, 09, 32, true)]
        [TestCase(09, 31, 10, 00, true)]
        [TestCase(09, 00, 10, 00, false)]
        [TestCase(09, 01, 10, 00, false)]
        [TestCase(09, 30, 10, 00, false)]
        [TestCase(10, 01, 10, 02, true)]
        [TestCase(10, 01, 11, 00, true)]
        [TestCase(10, 02, 11, 00, true)]
        [TestCase(11, 00, 12, 00, false)]
        [TestCase(10, 02, 12, 00, false)]
        [TestCase(19, 59, 20, 00, true)]
        public static void ShouldMergeHour(int hour1, int minute1, int hour2, int minute2, bool expected)
        {
            Assert.AreEqual(expected, Candle.ShouldMergeHour(new DateTimeOffset(2021, 06, 01, hour1, minute1, 0, TimeSpan.Zero), new DateTimeOffset(2021, 06, 01, hour2, minute2, 0, TimeSpan.Zero)));
        }
    }
}
