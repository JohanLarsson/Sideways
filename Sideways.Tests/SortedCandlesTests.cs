namespace Sideways.Tests
{
    using System;
    using System.Collections.Immutable;

    using NUnit.Framework;

    public static class SortedCandlesTests
    {
        [Test]
        public static void Descending()
        {
            var c1 = new Candle(
                new DateTimeOffset(2021, 04, 3, 00, 00, 00, 0, TimeSpan.Zero),
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
                new DateTimeOffset(2021, 04, 1, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);
            var sorted = new SortedCandles(ImmutableArray.Create(c1, c2, c3), ImmutableArray<Split>.Empty);
            CollectionAssert.AreEqual(new[] { c1, c2, c3 }, sorted);
        }

        [Test]
        public static void Previous()
        {
            var c1 = new Candle(
                new DateTimeOffset(2021, 04, 3, 00, 00, 00, 0, TimeSpan.Zero),
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
                new DateTimeOffset(2021, 04, 1, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);
            var sorted = new SortedCandles(ImmutableArray.Create(c1, c2, c3), ImmutableArray<Split>.Empty);
            Assert.AreEqual(c3, sorted.Previous(c2.Time));
        }

        [Test]
        public static void Next()
        {
            var c1 = new Candle(
                new DateTimeOffset(2021, 04, 3, 00, 00, 00, 0, TimeSpan.Zero),
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
                new DateTimeOffset(2021, 04, 1, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);
            var sorted = new SortedCandles(ImmutableArray.Create(c1, c2, c3), ImmutableArray<Split>.Empty);
            Assert.AreEqual(c1, sorted.Next(c2.Time));
        }

        [TestCase(3, 2, 1)]
        [TestCase(30, 2, 1)]
        [TestCase(30, 29, 1)]
        public static void IndexOfFirst(int d1, int d2, int d3)
        {
            var c1 = new Candle(
                new DateTimeOffset(2021, 04, d1, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c2 = new Candle(
                new DateTimeOffset(2021, 04, d2, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c3 = new Candle(
                new DateTimeOffset(2021, 04, d3, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);
            var sorted = new SortedCandles(ImmutableArray.Create(c1, c2, c3), ImmutableArray<Split>.Empty);
            Assert.AreEqual(0, sorted.IndexOf(c1.Time));
        }

        [TestCase(3, 2, 1)]
        [TestCase(30, 2, 1)]
        [TestCase(30, 29, 1)]
        public static void IndexOfMiddle(int d1, int d2, int d3)
        {
            var c1 = new Candle(
                new DateTimeOffset(2021, 04, d1, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c2 = new Candle(
                new DateTimeOffset(2021, 04, d2, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c3 = new Candle(
                new DateTimeOffset(2021, 04, d3, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);
            var sorted = new SortedCandles(ImmutableArray.Create(c1, c2, c3), ImmutableArray<Split>.Empty);
            Assert.AreEqual(1, sorted.IndexOf(c2.Time));
        }

        [TestCase(3, 2, 1)]
        [TestCase(30, 2, 1)]
        [TestCase(30, 29, 1)]
        public static void IndexOfLast(int d1, int d2, int d3)
        {
            var c1 = new Candle(
                new DateTimeOffset(2021, 04, d1, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c2 = new Candle(
                new DateTimeOffset(2021, 04, d2, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c3 = new Candle(
                new DateTimeOffset(2021, 04, d3, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);
            var sorted = new SortedCandles(ImmutableArray.Create(c1, c2, c3), ImmutableArray<Split>.Empty);
            Assert.AreEqual(2, sorted.IndexOf(c3.Time));
        }
    }
}
