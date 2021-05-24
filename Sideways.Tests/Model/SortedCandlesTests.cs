namespace Sideways.Tests.Model
{
    using System;
    using System.Collections.Generic;

    using NUnit.Framework;

    public static class SortedCandlesTests
    {
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
