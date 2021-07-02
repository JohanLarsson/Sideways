namespace Sideways.Tests.Scan
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using Sideways.Scan;

    public static class YieldCriteriaTest
    {
        [TestCaseSource(nameof(IsSatisfiedSource))]
        public static void IsSatisfied(int days, Percent? min, Percent? max, SortedCandles candles, int index, bool expected)
        {
            var criteria = new YieldCriteria
            {
                Days = days,
                IsActive = true,
                Min = min,
                Max = max,
            };

            Assert.AreEqual(expected, criteria.IsSatisfied(candles, index));
        }

        private static IEnumerable<TestCaseData> IsSatisfiedSource()
        {
            var candles = SortedCandles.Create(
                new Candle(
                    time: new DateTimeOffset(2021, 5, 19, 0, 0, 0, 0, TimeSpan.Zero),
                    open: 22.14f,
                    high: 22.65f,
                    low: 21.1f,
                    close: 21.96f,
                    volume: 2576531),
                new Candle(
                    time: new DateTimeOffset(2021, 5, 20, 0, 0, 0, 0, TimeSpan.Zero),
                    open: 22.3f,
                    high: 22.99f,
                    low: 21.51f,
                    close: 22.81f,
                    volume: 1320736),
                new Candle(
                    time: new DateTimeOffset(2021, 5, 21, 0, 0, 0, 0, TimeSpan.Zero),
                    open: 23.2f,
                    high: 23.81f,
                    low: 21.61f,
                    close: 21.74f,
                    volume: 1653603),
                new Candle(
                    time: new DateTimeOffset(2021, 5, 24, 0, 0, 0, 0, TimeSpan.Zero),
                    open: 22f,
                    high: 22.17f,
                    low: 20.85f,
                    close: 21.6f,
                    volume: 1550110),
                new Candle(
                    time: new DateTimeOffset(2021, 5, 25, 0, 0, 0, 0, TimeSpan.Zero),
                    open: 21.6f,
                    high: 24.89f,
                    low: 21.17f,
                    close: 22.53f,
                    volume: 6794783),
                new Candle(
                    time: new DateTimeOffset(2021, 5, 26, 0, 0, 0, 0, TimeSpan.Zero),
                    open: 23f,
                    high: 26.39f,
                    low: 22.3f,
                    close: 25.47f,
                    volume: 6576354),
                new Candle(
                    time: new DateTimeOffset(2021, 5, 27, 0, 0, 0, 0, TimeSpan.Zero),
                    open: 25.85f,
                    high: 26.4f,
                    low: 24.33f,
                    close: 25.86f,
                    volume: 3550413),
                new Candle(
                    time: new DateTimeOffset(2021, 5, 28, 0, 0, 0, 0, TimeSpan.Zero),
                    open: 26.41f,
                    high: 28.38f,
                    low: 25.8f,
                    close: 27.01f,
                    volume: 2495548),
                new Candle(
                    time: new DateTimeOffset(2021, 6, 1, 0, 0, 0, 0, TimeSpan.Zero),
                    open: 27.95f,
                    high: 29.85f,
                    low: 27.5f,
                    close: 28.54f,
                    volume: 2857094),
                new Candle(
                    time: new DateTimeOffset(2021, 6, 2, 0, 0, 0, 0, TimeSpan.Zero),
                    open: 29.2f,
                    high: 29.25f,
                    low: 27f,
                    close: 28.83f,
                    volume: 2402372),
                new Candle(
                    time: new DateTimeOffset(2021, 6, 3, 0, 0, 0, 0, TimeSpan.Zero),
                    open: 27.85f,
                    high: 29.11f,
                    low: 27.07f,
                    close: 27.22f,
                    volume: 1633669),
                new Candle(
                    time: new DateTimeOffset(2021, 6, 4, 0, 0, 0, 0, TimeSpan.Zero),
                    open: 27.77f,
                    high: 36.8f,
                    low: 27f,
                    close: 32.6f,
                    volume: 10280961),
                new Candle(
                    time: new DateTimeOffset(2021, 6, 7, 0, 0, 0, 0, TimeSpan.Zero),
                    open: 33.82f,
                    high: 35.4f,
                    low: 32.36f,
                    close: 34.25f,
                    volume: 4023581),
                new Candle(
                    time: new DateTimeOffset(2021, 6, 8, 0, 0, 0, 0, TimeSpan.Zero),
                    open: 35.03f,
                    high: 40.5f,
                    low: 34.72f,
                    close: 40.41f,
                    volume: 6295034),
                new Candle(
                    time: new DateTimeOffset(2021, 6, 9, 0, 0, 0, 0, TimeSpan.Zero),
                    open: 42f,
                    high: 42.59f,
                    low: 37.35f,
                    close: 38.64f,
                    volume: 4324465));
            yield return new TestCaseData(5, new Percent(25), null, candles, 5, false);
            yield return new TestCaseData(5, new Percent(25), null, candles, 6, false);
            yield return new TestCaseData(5, new Percent(25), null, candles, 7, false);
            yield return new TestCaseData(5, new Percent(25), null, candles, 8, true);
            yield return new TestCaseData(5, new Percent(25), null, candles, 9, true);
            yield return new TestCaseData(5, new Percent(25), null, candles, 10, true);
            yield return new TestCaseData(5, new Percent(25), null, candles, 11, false);
            yield return new TestCaseData(5, new Percent(25), null, candles, 12, true);
            yield return new TestCaseData(5, new Percent(25), null, candles, 13, true);
            yield return new TestCaseData(5, new Percent(25), null, candles, 14, true);
            yield return new TestCaseData(5, new Percent(25), null, candles, 15, true);
        }
    }
}
