namespace Sideways.Tests.Chart
{
    using System;
    using System.Collections.Generic;

    using NUnit.Framework;

    public static class CandlesTests
    {
        [TestCaseSource(nameof(SkipWeeksSource))]
        public static void SkipWeeks(Candles candles, DateTimeOffset time, int count, DateTimeOffset expected)
        {
            Assert.AreEqual(expected, candles.Skip(time, CandleInterval.Week, count));
        }

        [TestCaseSource(nameof(SkipDaysSource))]
        public static void SkipDays(Candles candles, DateTimeOffset time, int count, DateTimeOffset expected)
        {
            Assert.AreEqual(expected, candles.Skip(time, CandleInterval.Day, count));
        }

        [TestCaseSource(nameof(SkipHoursSource))]
        public static void SkipHours(Candles candles, DateTimeOffset time, int count, DateTimeOffset expected)
        {
            Assert.AreEqual(expected, candles.Skip(time, CandleInterval.Hour, count));
        }

        [TestCaseSource(nameof(SkipMinutesSource))]
        public static void SkipMinutes(Candles candles, DateTimeOffset time, int count, DateTimeOffset expected)
        {
            Assert.AreEqual(expected, candles.Skip(time, CandleInterval.Minute, count));
        }

        [TestCaseSource(nameof(DescendingHoursSource))]
        public static void DescendingHours(Candles candles, DateTimeOffset time, Candle[] expected)
        {
            CollectionAssert.AreEqual(expected, candles.DescendingHours(time));
        }

        [TestCaseSource(nameof(DescendingDaysSource))]
        public static void DescendingDays(Candles candles, DateTimeOffset time, Candle[] expected)
        {
            CollectionAssert.AreEqual(expected, candles.DescendingDays(time));
        }

        [TestCaseSource(nameof(DescendingVWapsSource))]
        public static void DescendingVWaps(Candles candles, DateTimeOffset time, CandleInterval interval, float[] expected)
        {
            CollectionAssert.AreEqual(expected, candles.DescendingVWaps(time, interval));
        }

        private static IEnumerable<TestCaseData> SkipWeeksSource()
        {
            var monday1 = new Candle(
                new DateTimeOffset(2021, 04, 26, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);
            var thursday1 = new Candle(
                new DateTimeOffset(2021, 04, 29, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);
            var friday1 = new Candle(
                new DateTimeOffset(2021, 04, 30, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var monday2 = new Candle(
                new DateTimeOffset(2021, 05, 3, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);
            var thursday2 = new Candle(
                new DateTimeOffset(2021, 05, 6, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);
            var friday2 = new Candle(
                new DateTimeOffset(2021, 05, 7, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var monday3 = new Candle(
                new DateTimeOffset(2021, 05, 10, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var thursday3 = new Candle(
                new DateTimeOffset(2021, 05, 13, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var friday3 = new Candle(
                new DateTimeOffset(2021, 05, 14, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var candles = new Candles(
                SortedCandles.Create(monday1, thursday1, friday1, monday2, thursday2, friday2, monday3, thursday3, friday3),
                default);

            yield return Identity(monday1.Time, -1);
            yield return Identity(thursday1.Time, -1);
            yield return Identity(friday1.Time, -1);
            yield return new TestCaseData(candles, monday1.Time, 1, TradingDay.EndOfDay(friday1.Time));
            yield return new TestCaseData(candles, thursday1.Time, 1, TradingDay.EndOfDay(friday1.Time));
            yield return new TestCaseData(candles, friday1.Time, 1, TradingDay.EndOfDay(friday1.Time));
            yield return new TestCaseData(candles, TradingDay.EndOfDay(friday1.Time), 1, TradingDay.EndOfDay(friday2.Time));

            yield return new TestCaseData(candles, monday2.Time, -1, TradingDay.EndOfDay(friday1.Time));
            yield return new TestCaseData(candles, monday2.Time, 1, TradingDay.EndOfDay(friday2.Time));
            yield return new TestCaseData(candles, thursday2.Time, -1, TradingDay.EndOfDay(friday1.Time));
            yield return new TestCaseData(candles, thursday2.Time, 1, TradingDay.EndOfDay(friday2.Time));
            yield return new TestCaseData(candles, friday2.Time, -1, TradingDay.EndOfDay(friday1.Time));
            yield return new TestCaseData(candles, friday2.Time, 1, TradingDay.EndOfDay(friday2.Time));
            yield return new TestCaseData(candles, TradingDay.EndOfDay(friday2.Time), 1, TradingDay.EndOfDay(friday3.Time));

            yield return new TestCaseData(candles, monday3.Time, -1, TradingDay.EndOfDay(friday2.Time));
            yield return new TestCaseData(candles, monday3.Time, 1, TradingDay.EndOfDay(friday3.Time));
            yield return new TestCaseData(candles, thursday3.Time, -1, TradingDay.EndOfDay(friday2.Time));
            yield return new TestCaseData(candles, thursday3.Time, 1, TradingDay.EndOfDay(friday3.Time));
            yield return new TestCaseData(candles, friday3.Time, -1, TradingDay.EndOfDay(friday2.Time));
            yield return new TestCaseData(candles, friday3.Time, 1, TradingDay.EndOfDay(friday3.Time));
            yield return Identity(TradingDay.EndOfDay(friday3.Time), 1);

            TestCaseData Identity(DateTimeOffset time, int count) => new(candles, time, count, time);
        }

        private static IEnumerable<TestCaseData> SkipDaysSource()
        {
            var days = SortedCandles.Create(
                new Candle(
                    new DateTimeOffset(2021, 04, 04, 00, 00, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default),
                new Candle(
                    new DateTimeOffset(2021, 04, 05, 00, 00, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default),
                new Candle(
                    new DateTimeOffset(2021, 04, 06, 00, 00, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default),
                new Candle(
                    new DateTimeOffset(2021, 04, 07, 00, 00, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default));
            var candles = new Candles(days, default);

            yield return Identity(days[0].Time.AddHours(-12), -1);
            yield return Identity(days[0].Time, -1);
            yield return Identity(days[0].Time.AddHours(-1), -1);
            yield return new TestCaseData(candles, days[0].Time, 1, TradingDay.EndOfDay(days[1].Time));
            yield return new TestCaseData(candles, days[0].Time.AddHours(12), 1, TradingDay.EndOfDay(days[1].Time));

            yield return new TestCaseData(candles, days[1].Time, 1, TradingDay.EndOfDay(days[2].Time));
            yield return new TestCaseData(candles, days[1].Time.AddHours(12), 1, TradingDay.EndOfDay(days[2].Time));
            yield return new TestCaseData(candles, days[1].Time, -1, TradingDay.EndOfDay(days[0].Time));
            yield return new TestCaseData(candles, days[1].Time.AddHours(12), -1, TradingDay.EndOfDay(days[0].Time));

            yield return new TestCaseData(candles, days[2].Time, -1, TradingDay.EndOfDay(days[1].Time));
            yield return new TestCaseData(candles, days[2].Time, 1, TradingDay.EndOfDay(days[3].Time));

            yield return new TestCaseData(candles, days[3].Time, -1, TradingDay.EndOfDay(days[2].Time));
            yield return new TestCaseData(candles, days[3].Time, 1, TradingDay.EndOfDay(days[3].Time));
            yield return Identity(days[3].Time.AddDays(1), 1);

            TestCaseData Identity(DateTimeOffset time, int count) => new(candles, time, count, time);
        }

        private static IEnumerable<TestCaseData> SkipHoursSource()
        {
            var minutes = SortedCandles.Create(
                new Candle(
                    new DateTimeOffset(2021, 04, 07, 09, 00, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default),
                new Candle(
                    new DateTimeOffset(2021, 04, 07, 09, 29, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default),
                new Candle(
                    new DateTimeOffset(2021, 04, 07, 09, 30, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default),
                new Candle(
                    new DateTimeOffset(2021, 04, 07, 09, 31, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default),
                new Candle(
                    new DateTimeOffset(2021, 04, 07, 09, 59, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default),
                new Candle(
                    new DateTimeOffset(2021, 04, 07, 10, 01, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default),
                new Candle(
                    new DateTimeOffset(2021, 04, 07, 11, 00, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default));
            var candles = new Candles(default, minutes);

            yield return Identity(minutes[0].Time.AddMinutes(-5), -1);
            yield return Identity(minutes[0].Time, -1);
            yield return new TestCaseData(candles, minutes[0].Time, 1, new DateTimeOffset(2021, 04, 07, 09, 30, 00, 0, TimeSpan.Zero));

            yield return new TestCaseData(candles, minutes[1].Time, -1, new DateTimeOffset(2021, 04, 07, 09, 00, 00, 0, TimeSpan.Zero));
            yield return new TestCaseData(candles, minutes[1].Time, 1, new DateTimeOffset(2021, 04, 07, 09, 30, 00, 0, TimeSpan.Zero));

            yield return new TestCaseData(candles, minutes[2].Time, -1, new DateTimeOffset(2021, 04, 07, 09, 00, 00, 0, TimeSpan.Zero));
            yield return new TestCaseData(candles, minutes[2].Time, 1, new DateTimeOffset(2021, 04, 07, 10, 00, 00, 0, TimeSpan.Zero));

            yield return new TestCaseData(candles, minutes[3].Time, -1, new DateTimeOffset(2021, 04, 07, 09, 30, 00, 0, TimeSpan.Zero));
            yield return new TestCaseData(candles, minutes[3].Time, 1, new DateTimeOffset(2021, 04, 07, 10, 00, 00, 0, TimeSpan.Zero));

            yield return new TestCaseData(candles, minutes[4].Time, -1, new DateTimeOffset(2021, 04, 07, 09, 30, 00, 0, TimeSpan.Zero));
            yield return new TestCaseData(candles, minutes[4].Time, 1, new DateTimeOffset(2021, 04, 07, 10, 00, 00, 0, TimeSpan.Zero));

            yield return new TestCaseData(candles, minutes[5].Time, -1, new DateTimeOffset(2021, 04, 07, 10, 00, 00, 0, TimeSpan.Zero));
            yield return new TestCaseData(candles, minutes[5].Time, 1, new DateTimeOffset(2021, 04, 07, 11, 00, 00, 0, TimeSpan.Zero));

            yield return new TestCaseData(candles, minutes[6].Time, -1, new DateTimeOffset(2021, 04, 07, 10, 00, 00, 0, TimeSpan.Zero));
            yield return new TestCaseData(candles, minutes[6].Time, 1, new DateTimeOffset(2021, 04, 07, 11, 00, 00, 0, TimeSpan.Zero));

            yield return Identity(minutes[6].Time.AddMinutes(5), 1);

            TestCaseData Identity(DateTimeOffset time, int count) => new(candles, time, count, time);
        }

        private static IEnumerable<TestCaseData> SkipMinutesSource()
        {
            var minutes = SortedCandles.Create(
                new Candle(
                    new DateTimeOffset(2021, 04, 07, 09, 32, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default),
                new Candle(
                    new DateTimeOffset(2021, 04, 07, 09, 33, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default),
                new Candle(
                    new DateTimeOffset(2021, 04, 07, 09, 34, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default),
                new Candle(
                    new DateTimeOffset(2021, 04, 07, 09, 35, 00, 0, TimeSpan.Zero),
                    default,
                    default,
                    default,
                    default,
                    default));
            var candles = new Candles(default, minutes);

            yield return Identity(minutes[0].Time.AddMinutes(-5), -1);
            yield return new TestCaseData(candles, minutes[0].Time, -1, minutes[0].Time);
            yield return new TestCaseData(candles, minutes[0].Time, 1, minutes[1].Time);

            yield return new TestCaseData(candles, minutes[1].Time, -1, minutes[0].Time);
            yield return new TestCaseData(candles, minutes[1].Time, 1, minutes[2].Time);

            yield return new TestCaseData(candles, minutes[2].Time, -1, minutes[1].Time);
            yield return new TestCaseData(candles, minutes[2].Time, 1, minutes[3].Time);

            yield return new TestCaseData(candles, minutes[3].Time, -1, minutes[2].Time);
            yield return new TestCaseData(candles, minutes[3].Time, 1, minutes[3].Time);
            yield return Identity(minutes[3].Time.AddMinutes(5), 1);

            TestCaseData Identity(DateTimeOffset time, int count) => new(candles, time, count, time);
        }

        private static IEnumerable<TestCaseData> DescendingHoursSource()
        {
            var candles = new Candles(
                default,
                SortedCandles.Create(
                    new Candle(
                        new DateTimeOffset(2021, 05, 22, 09, 29, 00, 0, TimeSpan.Zero),
                        open: 1.3f,
                        high: 1.4f,
                        low: 1.1f,
                        close: 1.2f,
                        volume: 1),
                    new Candle(
                        new DateTimeOffset(2021, 05, 22, 09, 30, 00, 0, TimeSpan.Zero),
                        open: 2.3f,
                        high: 2.4f,
                        low: 2.1f,
                        close: 2.2f,
                        volume: 2),
                    new Candle(
                        new DateTimeOffset(2021, 05, 22, 09, 31, 00, 0, TimeSpan.Zero),
                        open: 3.3f,
                        high: 3.4f,
                        low: 3.1f,
                        close: 3.2f,
                        volume: 3),
                    new Candle(
                        new DateTimeOffset(2021, 05, 22, 09, 32, 00, 0, TimeSpan.Zero),
                        open: 4.3f,
                        high: 4.4f,
                        low: 4.1f,
                        close: 4.2f,
                        volume: 4),
                    new Candle(
                        new DateTimeOffset(2021, 05, 22, 10, 00, 00, 0, TimeSpan.Zero),
                        open: 5.3f,
                        high: 5.4f,
                        low: 5.1f,
                        close: 5.2f,
                        volume: 5),
                    new Candle(
                        new DateTimeOffset(2021, 05, 22, 10, 01, 00, 0, TimeSpan.Zero),
                        open: 6.3f,
                        high: 6.4f,
                        low: 6.1f,
                        close: 6.2f,
                        volume: 6),
                    new Candle(
                        new DateTimeOffset(2021, 05, 22, 11, 00, 00, 0, TimeSpan.Zero),
                        open: 7.3f,
                        high: 7.4f,
                        low: 7.1f,
                        close: 7.2f,
                        volume: 7)));

            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 22, 09, 28, 00, 0, TimeSpan.Zero), Array.Empty<Candle>());

            var expected = new[]
            {
                new Candle(new DateTimeOffset(2021, 05, 22, 09, 29, 00, 0, TimeSpan.Zero), open: 1.3f, high: 1.4f, low: 1.1f, close: 1.2f, volume: 1),
            };
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 22, 09, 29, 00, 0, TimeSpan.Zero), expected);

            expected = new[]
            {
                new Candle(new DateTimeOffset(2021, 05, 22, 09, 30, 00, 0, TimeSpan.Zero), open: 1.3f, high: 2.4f, low: 1.1f, close: 2.2f, volume: 3),
            };

            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 22, 09, 30, 00, 0, TimeSpan.Zero), expected);

            expected = new[]
            {
                new Candle(new DateTimeOffset(2021, 05, 22, 09, 31, 00, 0, TimeSpan.Zero), open: 3.3f, high: 3.4f, low: 3.1f, close: 3.2f, volume: 3),
                new Candle(new DateTimeOffset(2021, 05, 22, 09, 30, 00, 0, TimeSpan.Zero), open: 1.3f, high: 2.4f, low: 1.1f, close: 2.2f, volume: 3),
            };
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 22, 09, 31, 00, 0, TimeSpan.Zero), expected);

            expected = new[]
            {
                new Candle(new DateTimeOffset(2021, 05, 22, 09, 32, 00, 0, TimeSpan.Zero), open: 3.3f, high: 4.4f, low: 3.1f, close: 4.2f, volume: 7),
                new Candle(new DateTimeOffset(2021, 05, 22, 09, 30, 00, 0, TimeSpan.Zero), open: 1.3f, high: 2.4f, low: 1.1f, close: 2.2f, volume: 3),
            };
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 22, 09, 32, 00, 0, TimeSpan.Zero), expected);

            expected = new[]
            {
                new Candle(new DateTimeOffset(2021, 05, 22, 10, 00, 00, 0, TimeSpan.Zero), open: 3.3f, high: 5.4f, low: 3.1f, close: 5.2f, volume: 12),
                new Candle(new DateTimeOffset(2021, 05, 22, 09, 30, 00, 0, TimeSpan.Zero), open: 1.3f, high: 2.4f, low: 1.1f, close: 2.2f, volume: 3),
            };
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 22, 10, 00, 00, 0, TimeSpan.Zero), expected);

            expected = new[]
            {
                new Candle(new DateTimeOffset(2021, 05, 22, 10, 01, 00, 0, TimeSpan.Zero), open: 6.3f, high: 6.4f, low: 6.1f, close: 6.2f, volume: 6),
                new Candle(new DateTimeOffset(2021, 05, 22, 10, 00, 00, 0, TimeSpan.Zero), open: 3.3f, high: 5.4f, low: 3.1f, close: 5.2f, volume: 12),
                new Candle(new DateTimeOffset(2021, 05, 22, 09, 30, 00, 0, TimeSpan.Zero), open: 1.3f, high: 2.4f, low: 1.1f, close: 2.2f, volume: 3),
            };
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 22, 10, 01, 00, 0, TimeSpan.Zero), expected);

            expected = new[]
            {
                new Candle(new DateTimeOffset(2021, 05, 22, 10, 01, 00, 0, TimeSpan.Zero), open: 6.3f, high: 6.4f, low: 6.1f, close: 6.2f, volume: 6),
                new Candle(new DateTimeOffset(2021, 05, 22, 10, 00, 00, 0, TimeSpan.Zero), open: 3.3f, high: 5.4f, low: 3.1f, close: 5.2f, volume: 12),
                new Candle(new DateTimeOffset(2021, 05, 22, 09, 30, 00, 0, TimeSpan.Zero), open: 1.3f, high: 2.4f, low: 1.1f, close: 2.2f, volume: 3),
            };
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 22, 10, 15, 00, 0, TimeSpan.Zero), expected);

            expected = new[]
            {
                new Candle(new DateTimeOffset(2021, 05, 22, 11, 00, 00, 0, TimeSpan.Zero), open: 6.3f, high: 7.4f, low: 6.1f, close: 7.2f, volume: 13),
                new Candle(new DateTimeOffset(2021, 05, 22, 10, 00, 00, 0, TimeSpan.Zero), open: 3.3f, high: 5.4f, low: 3.1f, close: 5.2f, volume: 12),
                new Candle(new DateTimeOffset(2021, 05, 22, 09, 30, 00, 0, TimeSpan.Zero), open: 1.3f, high: 2.4f, low: 1.1f, close: 2.2f, volume: 3),
            };
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 22, 11, 00, 00, 0, TimeSpan.Zero), expected);
        }

        private static IEnumerable<TestCaseData> DescendingDaysSource()
        {
            var day1 = new Candle(new DateTimeOffset(2021, 05, 20, 00, 00, 00, 0, TimeSpan.Zero), open: 20.3f, high: 20.4f, low: 20.1f, close: 20.2f, volume: 20);
            var day2 = new Candle(new DateTimeOffset(2021, 05, 21, 00, 00, 00, 0, TimeSpan.Zero), open: 21.3f, high: 21.4f, low: 21.1f, close: 21.2f, volume: 21);
            var candles = new Candles(
                SortedCandles.Create(day1, day2),
                SortedCandles.Create(
                    new Candle(
                        new DateTimeOffset(2021, 05, 21, 09, 30, 00, 0, TimeSpan.Zero),
                        open: 1.3f,
                        high: 1.4f,
                        low: 1.1f,
                        close: 1.2f,
                        volume: 1),
                    new Candle(
                        new DateTimeOffset(2021, 05, 21, 09, 31, 00, 0, TimeSpan.Zero),
                        open: 2.3f,
                        high: 2.4f,
                        low: 2.1f,
                        close: 2.2f,
                        volume: 2),
                    new Candle(
                        new DateTimeOffset(2021, 05, 21, 09, 32, 00, 0, TimeSpan.Zero),
                        open: 3.3f,
                        high: 3.4f,
                        low: 3.1f,
                        close: 3.2f,
                        volume: 3),
                    new Candle(
                        new DateTimeOffset(2021, 05, 21, 10, 00, 00, 0, TimeSpan.Zero),
                        open: 4.3f,
                        high: 4.4f,
                        low: 4.1f,
                        close: 4.2f,
                        volume: 4),
                    new Candle(
                        new DateTimeOffset(2021, 05, 21, 10, 01, 00, 0, TimeSpan.Zero),
                        open: 5.3f,
                        high: 5.4f,
                        low: 5.1f,
                        close: 5.2f,
                        volume: 5),
                    new Candle(
                        new DateTimeOffset(2021, 05, 21, 15, 59, 00, 0, TimeSpan.Zero),
                        open: 6.3f,
                        high: 6.4f,
                        low: 6.1f,
                        close: 6.2f,
                        volume: 6),
                    new Candle(
                        new DateTimeOffset(2021, 05, 21, 16, 00, 00, 0, TimeSpan.Zero),
                        open: 7.3f,
                        high: 7.4f,
                        low: 7.1f,
                        close: 7.2f,
                        volume: 7),
                    new Candle(
                        new DateTimeOffset(2021, 05, 21, 16, 01, 00, 0, TimeSpan.Zero),
                        open: 8.3f,
                        high: 8.4f,
                        low: 8.1f,
                        close: 8.2f,
                        volume: 8)));

            day1 = day1.WithTime(TradingDay.EndOfDay(day1.Time));
            day2 = day2.WithTime(TradingDay.EndOfDay(day2.Time));
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 19, 09, 30, 00, 0, TimeSpan.Zero), Array.Empty<Candle>());
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 20, 16, 00, 00, 0, TimeSpan.Zero), new[] { day1 });
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 21, 09, 30, 00, 0, TimeSpan.Zero), new[] { day1 });
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 21, 09, 31, 00, 0, TimeSpan.Zero), new[] { new Candle(new DateTimeOffset(2021, 05, 21, 09, 31, 00, 0, TimeSpan.Zero), open: 2.3f, high: 2.4f, low: 2.1f, close: 2.2f, volume: 2), day1 });
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 21, 09, 32, 00, 0, TimeSpan.Zero), new[] { new Candle(new DateTimeOffset(2021, 05, 21, 09, 32, 00, 0, TimeSpan.Zero), open: 2.3f, high: 3.4f, low: 2.1f, close: 3.2f, volume: 5), day1 });
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 21, 15, 59, 00, 0, TimeSpan.Zero), new[] { new Candle(new DateTimeOffset(2021, 05, 21, 15, 59, 00, 0, TimeSpan.Zero), open: 2.3f, high: 6.4f, low: 2.1f, close: 6.2f, volume: 20), day1 });
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 21, 16, 00, 00, 0, TimeSpan.Zero), new[] { new Candle(new DateTimeOffset(2021, 05, 21, 16, 00, 00, 0, TimeSpan.Zero), open: 2.3f, high: 7.4f, low: 2.1f, close: 7.2f, volume: 27), day1 });
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 21, 16, 01, 00, 0, TimeSpan.Zero), new[] { day2, day1 });
        }

        private static IEnumerable<TestCaseData> DescendingVWapsSource()
        {
            var candles = new Candles(
                default,
                SortedCandles.Create(
                    new Candle(
                        new DateTimeOffset(2021, 06, 14, 09, 29, 00, 0, TimeSpan.Zero),
                        open: 1.3f,
                        high: 1.4f,
                        low: 1.1f,
                        close: 1.2f,
                        volume: 1),
                    new Candle(
                        new DateTimeOffset(2021, 06, 14, 09, 30, 00, 0, TimeSpan.Zero),
                        open: 2.3f,
                        high: 2.4f,
                        low: 2.1f,
                        close: 2.2f,
                        volume: 2),
                    new Candle(
                        new DateTimeOffset(2021, 06, 14, 09, 31, 00, 0, TimeSpan.Zero),
                        open: 3.3f,
                        high: 3.4f,
                        low: 3.1f,
                        close: 3.2f,
                        volume: 3),
                    new Candle(
                        new DateTimeOffset(2021, 06, 14, 09, 32, 00, 0, TimeSpan.Zero),
                        open: 4.3f,
                        high: 4.4f,
                        low: 4.1f,
                        close: 4.2f,
                        volume: 4),
                    new Candle(
                        new DateTimeOffset(2021, 06, 14, 10, 00, 00, 0, TimeSpan.Zero),
                        open: 5.3f,
                        high: 5.4f,
                        low: 5.1f,
                        close: 5.2f,
                        volume: 5),
                    new Candle(
                        new DateTimeOffset(2021, 06, 14, 10, 01, 00, 0, TimeSpan.Zero),
                        open: 6.3f,
                        high: 6.4f,
                        low: 6.1f,
                        close: 6.2f,
                        volume: 6),
                    new Candle(
                        new DateTimeOffset(2021, 06, 14, 11, 00, 00, 0, TimeSpan.Zero),
                        open: 7.3f,
                        high: 7.4f,
                        low: 7.1f,
                        close: 7.2f,
                        volume: 7)));

            yield return new TestCaseData(candles, new DateTimeOffset(2021, 06, 14, 09, 28, 00, 0, TimeSpan.Zero), CandleInterval.Minute, Array.Empty<float>());
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 06, 14, 09, 28, 00, 0, TimeSpan.Zero), CandleInterval.FiveMinutes, Array.Empty<float>());
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 06, 14, 09, 28, 00, 0, TimeSpan.Zero), CandleInterval.FifteenMinutes, Array.Empty<float>());
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 06, 14, 09, 28, 00, 0, TimeSpan.Zero), CandleInterval.Hour, Array.Empty<float>());

            yield return new TestCaseData(candles, new DateTimeOffset(2021, 06, 14, 09, 29, 00, 0, TimeSpan.Zero), CandleInterval.Minute, new[] { 1.25f });
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 06, 14, 09, 29, 00, 0, TimeSpan.Zero), CandleInterval.FiveMinutes, new[] { 1.25f });
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 06, 14, 09, 29, 00, 0, TimeSpan.Zero), CandleInterval.FifteenMinutes, new[] { 1.25f });
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 06, 14, 09, 29, 00, 0, TimeSpan.Zero), CandleInterval.Hour, new[] { 1.25f });

            yield return new TestCaseData(candles, new DateTimeOffset(2021, 06, 14, 09, 30, 00, 0, TimeSpan.Zero), CandleInterval.Minute, new[] { 1.91666663f, 1.25f });
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 06, 14, 09, 30, 00, 0, TimeSpan.Zero), CandleInterval.FiveMinutes, new[] { 1.91666663f });
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 06, 14, 09, 30, 00, 0, TimeSpan.Zero), CandleInterval.FifteenMinutes, new[] { 1.91666663f });
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 06, 14, 09, 30, 00, 0, TimeSpan.Zero), CandleInterval.Hour, new[] { 1.91666663f });

            yield return new TestCaseData(candles, new DateTimeOffset(2021, 06, 14, 09, 31, 00, 0, TimeSpan.Zero), CandleInterval.Minute, new[] { 2.58333325f, 1.91666663f, 1.25f });
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 06, 14, 09, 31, 00, 0, TimeSpan.Zero), CandleInterval.FiveMinutes, new[] { 2.58333325f, 1.91666663f });
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 06, 14, 09, 31, 00, 0, TimeSpan.Zero), CandleInterval.FifteenMinutes, new[] { 2.58333325f, 1.91666663f });
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 06, 14, 09, 31, 00, 0, TimeSpan.Zero), CandleInterval.Hour, new[] { 2.58333325f, 1.91666663f });

            yield return new TestCaseData(candles, new DateTimeOffset(2021, 06, 14, 09, 32, 00, 0, TimeSpan.Zero), CandleInterval.Minute, new[] { 3.25f, 2.58333325f, 1.91666663f, 1.25f });
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 06, 14, 09, 32, 00, 0, TimeSpan.Zero), CandleInterval.FiveMinutes, new[] { 3.25f, 1.91666663f });
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 06, 14, 09, 32, 00, 0, TimeSpan.Zero), CandleInterval.FifteenMinutes, new[] { 3.25f, 1.91666663f });
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 06, 14, 09, 32, 00, 0, TimeSpan.Zero), CandleInterval.Hour, new[] { 3.25f, 1.91666663f });
        }
    }
}
