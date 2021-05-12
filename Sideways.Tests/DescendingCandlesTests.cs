namespace Sideways.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;

    public static class DescendingCandlesTests
    {
        private static readonly TestCaseData[] IndexOfCases = CreateIndexOfCases().ToArray();

        [TestCaseSource(nameof(IndexOfCases))]
        public static void IndexOf(DescendingCandles candles, DateTimeOffset time, int startAt, int index)
        {
            Assert.AreEqual(index, candles.IndexOf(time, startAt));
        }

        private static IEnumerable<TestCaseData> CreateIndexOfCases()
        {
            var c1 = new Candle(
                new DateTimeOffset(2021, 04, 4, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c2 = new Candle(
                new DateTimeOffset(2021, 04, 3, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c3 = new Candle(
                new DateTimeOffset(2021, 04, 2, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c4 = new Candle(
                new DateTimeOffset(2021, 04, 1, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);
            var builder = DescendingCandles.CreateBuilder();
            builder.Add(c1);
            builder.Add(c2);
            builder.Add(c3);
            builder.Add(c4);
            var candles = builder.Create();

            for (var i = 0; i < 4; i++)
            {
                for (var startAt = 0; startAt < 4; startAt++)
                {
                    yield return new TestCaseData(candles, candles[i].Time, startAt, i);
                    yield return new TestCaseData(candles, candles[i].Time.AddHours(12), startAt, i);
                }
            }
        }
    }
}
