namespace Sideways.Tests
{
    using System;

    using NUnit.Framework;

    public static class SortedCandlesTests
    {
        [Test]
        public static void Descending()
        {
            var c1 = new Candle(
                new DateTimeOffset(2021, 04, 1, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c2 = new Candle(
                new DateTimeOffset(2021, 04, 2, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c3 = new Candle(
                new DateTimeOffset(2021, 04, 3, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);
            var sorted = new SortedCandles(new[] { c1, c2, c3 });
            CollectionAssert.AreEqual(new[] { c3, c2, c1 }, sorted);
        }

        [Test]
        public static void Previous()
        {
            var c1 = new Candle(
                new DateTimeOffset(2021, 04, 1, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c2 = new Candle(
                new DateTimeOffset(2021, 04, 2, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c3 = new Candle(
                new DateTimeOffset(2021, 04, 3, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);
            var sorted = new SortedCandles(new[] { c1, c2, c3 });
            Assert.AreEqual(c1, sorted.Previous(c2.Time));
        }

        [Test]
        public static void Next()
        {
            var c1 = new Candle(
                new DateTimeOffset(2021, 04, 1, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c2 = new Candle(
                new DateTimeOffset(2021, 04, 2, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c3 = new Candle(
                new DateTimeOffset(2021, 04, 3, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);
            var sorted = new SortedCandles(new[] { c1, c2, c3 });
            Assert.AreEqual(c3, sorted.Next(c2.Time));
        }
    }
}
