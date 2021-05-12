namespace Sideways.Tests
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;

    public static class CandlesTests
    {
        [TestCaseSource(nameof(SkipDaysSource))]
        public static void SkipDays(Candles candles, DateTimeOffset time, int count, DateTimeOffset expected)
        {
            Assert.AreEqual(expected, candles.Skip(time, CandleInterval.Day, count));
        }

        [TestCaseSource(nameof(SkipMinutesSource))]
        public static void SkipMinutes(Candles candles, DateTimeOffset time, int count, DateTimeOffset expected)
        {
            Assert.AreEqual(expected, candles.Skip(time, CandleInterval.Minute, count));
        }

        private static IEnumerable<TestCaseData> SkipDaysSource()
        {
            var c1 = new Candle(
                new DateTimeOffset(2021, 04, 07, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c2 = new Candle(
                new DateTimeOffset(2021, 04, 06, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c3 = new Candle(
                new DateTimeOffset(2021, 04, 05, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c4 = new Candle(
                new DateTimeOffset(2021, 04, 04, 00, 00, 00, 0, TimeSpan.Zero),
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
            var candles = new Candles(builder.Create(), default);

            yield return new TestCaseData(candles, c1.Time, -1, new DateTimeOffset(2021, 04, 07, 20, 00, 00, 0, TimeSpan.Zero));
            yield return new TestCaseData(candles, c1.Time, 1, new DateTimeOffset(2021, 04, 06, 20, 00, 00, 0, TimeSpan.Zero));
            yield return new TestCaseData(candles, c2.Time, -1, new DateTimeOffset(2021, 04, 07, 20, 00, 00, 0, TimeSpan.Zero));
            yield return new TestCaseData(candles, c2.Time.AddHours(12), -1, new DateTimeOffset(2021, 04, 07, 20, 00, 00, 0, TimeSpan.Zero));
            yield return new TestCaseData(candles, c2.Time, 1, new DateTimeOffset(2021, 04, 05, 20, 00, 00, 0, TimeSpan.Zero));
            yield return new TestCaseData(candles, c2.Time.AddHours(12), 1, new DateTimeOffset(2021, 04, 05, 20, 00, 00, 0, TimeSpan.Zero));
            yield return new TestCaseData(candles, c4.Time, -1, new DateTimeOffset(2021, 04, 05, 20, 00, 00, 0, TimeSpan.Zero));
            yield return new TestCaseData(candles, c4.Time, 1, new DateTimeOffset(2021, 04, 04, 20, 00, 00, 0, TimeSpan.Zero));
        }

        private static IEnumerable<TestCaseData> SkipMinutesSource()
        {
            var c1 = new Candle(
                new DateTimeOffset(2021, 04, 07, 09, 35, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c2 = new Candle(
                new DateTimeOffset(2021, 04, 07, 09, 34, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c3 = new Candle(
                new DateTimeOffset(2021, 04, 07, 09, 33, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c4 = new Candle(
                new DateTimeOffset(2021, 04, 07, 09, 32, 00, 0, TimeSpan.Zero),
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
            var candles = new Candles(default, builder.Create());

            yield return new TestCaseData(candles, c1.Time, -1, new DateTimeOffset(2021, 04, 07, 09, 35, 00, 0, TimeSpan.Zero));
            yield return new TestCaseData(candles, c1.Time, 1, new DateTimeOffset(2021, 04, 07, 09, 34, 00, 0, TimeSpan.Zero));
            yield return new TestCaseData(candles, c2.Time, -1, new DateTimeOffset(2021, 04, 07, 09, 35, 00, 0, TimeSpan.Zero));
            yield return new TestCaseData(candles, c2.Time, 1, new DateTimeOffset(2021, 04, 07, 09, 33, 00, 0, TimeSpan.Zero));
            yield return new TestCaseData(candles, c4.Time, -1, new DateTimeOffset(2021, 04, 07, 09, 33, 00, 0, TimeSpan.Zero));
            yield return new TestCaseData(candles, c4.Time, 1, new DateTimeOffset(2021, 04, 07, 09, 32, 00, 0, TimeSpan.Zero));
        }
    }
}
