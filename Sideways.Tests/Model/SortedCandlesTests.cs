namespace Sideways.Tests.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    public static class SortedCandlesTests
    {
        [TestCase(0, 0, false)]
        [TestCase(0, 1, true)]
        [TestCase(0, 2, true)]
        [TestCase(0, 3, true)]
        [TestCase(0, 4, true)]
        [TestCase(0, 5, false)]
        [TestCase(1, 2, true)]
        [TestCase(2, 2, true)]
        [TestCase(2, 3, false)]
        [TestCase(3, 1, true)]
        [TestCase(3, 2, false)]
        [TestCase(1, -2, true)]
        [TestCase(2, -3, true)]
        [TestCase(3, -4, true)]
        [TestCase(2, -2, true)]
        [TestCase(3, -2, true)]
        [TestCase(3, -4, true)]
        [TestCase(3, -5, false)]
        public static void CanSlice(int index, int length, bool expected)
        {
            var candles = SortedCandles.Create(
                new Candle(
                    new DateTimeOffset(2021, 04, 1, 00, 00, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default),
                new Candle(
                    new DateTimeOffset(2021, 04, 2, 00, 00, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default),
                new Candle(
                    new DateTimeOffset(2021, 04, 3, 00, 00, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default),
                new Candle(
                    new DateTimeOffset(2021, 04, 4, 00, 00, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default));
            Assert.AreEqual(expected, candles.CanSlice(index, length));
        }

        [TestCase(0, 0, "")]
        [TestCase(0, 1, "1")]
        [TestCase(0, 2, "1, 2")]
        [TestCase(0, 3, "1, 2, 3")]
        [TestCase(0, 4, "1, 2, 3, 4")]
        [TestCase(1, 2, "2, 3")]
        [TestCase(2, 2, "3, 4")]
        [TestCase(3, 1, "4")]
        [TestCase(1, -2, "1, 2")]
        [TestCase(2, -3, "1, 2, 3")]
        [TestCase(3, -4, "1, 2, 3, 4")]
        [TestCase(2, -2, "2, 3")]
        [TestCase(3, -2, "3, 4")]
        [TestCase(3, -4, "1, 2, 3, 4")]
        public static void Slice(int index, int length, string expected)
        {
            var candles = SortedCandles.Create(
                new Candle(
                    new DateTimeOffset(2021, 04, 1, 00, 00, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default),
                new Candle(
                    new DateTimeOffset(2021, 04, 2, 00, 00, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default),
                new Candle(
                    new DateTimeOffset(2021, 04, 3, 00, 00, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default),
                new Candle(
                    new DateTimeOffset(2021, 04, 4, 00, 00, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default));
            Assert.AreEqual(expected, string.Join(", ", candles.Slice(index, length).ToArray().Select(x => x.Time.Day)));
        }

        [TestCaseSource(nameof(IndexOfCases))]
        public static void IndexOf(SortedCandles candles, DateTimeOffset time, int startAt, int index)
        {
            Assert.AreEqual(index, candles.IndexOf(time, startAt));
        }

        private static IEnumerable<TestCaseData> IndexOfCases()
        {
            var candles = SortedCandles.Create(
                new Candle(
                    new DateTimeOffset(2021, 04, 1, 00, 00, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default),
                new Candle(
                    new DateTimeOffset(2021, 04, 2, 00, 00, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default),
                new Candle(
                    new DateTimeOffset(2021, 04, 3, 00, 00, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default),
                new Candle(
                    new DateTimeOffset(2021, 04, 4, 00, 00, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default));

            for (var startAt = 0; startAt < candles.Count; startAt++)
            {
                yield return new TestCaseData(candles, candles[0].Time.AddDays(-1), startAt, -1);

                for (var i = 0; i < candles.Count; i++)
                {
                    yield return new TestCaseData(candles, candles[i].Time, startAt, i);
                    yield return new TestCaseData(candles, candles[i].Time.AddHours(12), startAt, i);
                }
            }
        }
    }
}
