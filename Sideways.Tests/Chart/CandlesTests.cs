namespace Sideways.Tests.Chart
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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

        [TestCaseSource(nameof(SkipMinutesSource))]
        public static void SkipMinutes(Candles candles, DateTimeOffset time, int count, DateTimeOffset expected)
        {
            Assert.AreEqual(expected, candles.Skip(time, CandleInterval.Minute, count));
        }

        [TestCaseSource(nameof(HoursSource))]
        public static void Hours(Candles candles, DateTimeOffset time, Candle[] expected)
        {
            CollectionAssert.AreEqual(expected, candles.Hours(time).ToArray());
        }

        [TestCaseSource(nameof(DaysSource))]
        public static void Days(Candles candles, DateTimeOffset time, Candle[] expected)
        {
            CollectionAssert.AreEqual(expected, candles.Days(time).ToArray());
        }

        private static IEnumerable<TestCaseData> SkipWeeksSource()
        {
            var friday1 = new Candle(
                new DateTimeOffset(2021, 05, 14, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var thursday1 = new Candle(
                new DateTimeOffset(2021, 05, 13, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var monday1 = new Candle(
                new DateTimeOffset(2021, 05, 10, 00, 00, 00, 0, TimeSpan.Zero),
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

            var thursday2 = new Candle(
                new DateTimeOffset(2021, 05, 6, 00, 00, 00, 0, TimeSpan.Zero),
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

            var friday3 = new Candle(
                new DateTimeOffset(2021, 04, 30, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var thursday3 = new Candle(
                new DateTimeOffset(2021, 04, 29, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var monday3 = new Candle(
                new DateTimeOffset(2021, 04, 26, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var builder = DescendingCandles.CreateBuilder();
            builder.Add(friday1);
            builder.Add(thursday1);
            builder.Add(monday1);
            builder.Add(friday2);
            builder.Add(thursday2);
            builder.Add(monday2);
            builder.Add(friday3);
            builder.Add(thursday3);
            builder.Add(monday3);
            var candles = new Candles(builder.Create(), default);

            yield return new TestCaseData(candles, friday1.Time, -1, TradingDay.EndOfDay(friday2.Time));
            yield return new TestCaseData(candles, friday1.Time, 1, TradingDay.EndOfDay(friday1.Time));

            yield return new TestCaseData(candles, thursday1.Time, -1, TradingDay.EndOfDay(friday2.Time));
            yield return new TestCaseData(candles, thursday1.Time, 1, TradingDay.EndOfDay(friday1.Time));

            yield return new TestCaseData(candles, monday1.Time, -1, TradingDay.EndOfDay(friday2.Time));
            yield return new TestCaseData(candles, monday1.Time, 1, TradingDay.EndOfDay(friday1.Time));

            yield return new TestCaseData(candles, friday2.Time, -1, TradingDay.EndOfDay(friday3.Time));
            yield return new TestCaseData(candles, friday2.Time, 1, TradingDay.EndOfDay(friday1.Time));

            yield return new TestCaseData(candles, thursday2.Time, -1, TradingDay.EndOfDay(friday3.Time));
            yield return new TestCaseData(candles, thursday2.Time, 1, TradingDay.EndOfDay(friday2.Time));

            yield return new TestCaseData(candles, monday2.Time, -1, TradingDay.EndOfDay(friday3.Time));
            yield return new TestCaseData(candles, monday2.Time, 1, TradingDay.EndOfDay(friday2.Time));

            yield return new TestCaseData(candles, friday3.Time, -1, TradingDay.EndOfDay(friday3.Time));
            yield return new TestCaseData(candles, friday3.Time, 1, TradingDay.EndOfDay(friday2.Time));

            yield return new TestCaseData(candles, thursday3.Time, -1, TradingDay.EndOfDay(friday3.Time));
            yield return new TestCaseData(candles, thursday3.Time, 1, TradingDay.EndOfDay(friday3.Time));

            yield return new TestCaseData(candles, monday3.Time, -1, TradingDay.EndOfDay(friday3.Time));
            yield return new TestCaseData(candles, monday3.Time, 1, TradingDay.EndOfDay(friday3.Time));
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

            yield return new TestCaseData(candles, c1.Time, -1, TradingDay.EndOfDay(c2.Time));
            yield return new TestCaseData(candles, c1.Time, 1, TradingDay.EndOfDay(c1.Time));

            yield return new TestCaseData(candles, c2.Time, 1, TradingDay.EndOfDay(c1.Time));
            yield return new TestCaseData(candles, c2.Time.AddHours(12), 1, TradingDay.EndOfDay(c1.Time));
            yield return new TestCaseData(candles, c2.Time, -1, TradingDay.EndOfDay(c3.Time));
            yield return new TestCaseData(candles, c2.Time.AddHours(12), -1, TradingDay.EndOfDay(c3.Time));

            yield return new TestCaseData(candles, c4.Time, -1, TradingDay.EndOfDay(c4.Time));
            yield return new TestCaseData(candles, c4.Time, 1, TradingDay.EndOfDay(c3.Time));
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

            yield return new TestCaseData(candles, c1.Time, -1, c2.Time);
            yield return new TestCaseData(candles, c1.Time, 1, c1.Time);

            yield return new TestCaseData(candles, c2.Time, -1, c3.Time);
            yield return new TestCaseData(candles, c2.Time, 1, c1.Time);

            yield return new TestCaseData(candles, c3.Time, -1, c4.Time);
            yield return new TestCaseData(candles, c3.Time, 1, c2.Time);

            yield return new TestCaseData(candles, c4.Time, -1, c4.Time);
            yield return new TestCaseData(candles, c4.Time, 1, c3.Time);
        }

        private static IEnumerable<TestCaseData> HoursSource()
        {
            var candles = new Candles(
                default,
                Create(
                    new Candle(
                        new DateTimeOffset(2021, 05, 22, 10, 01, 00, 0, TimeSpan.Zero),
                        open: 5.3f,
                        high: 5.4f,
                        low: 5.1f,
                        close: 5.2f,
                        volume: 5),
                    new Candle(
                        new DateTimeOffset(2021, 05, 22, 10, 00, 00, 0, TimeSpan.Zero),
                        open: 4.3f,
                        high: 4.4f,
                        low: 4.1f,
                        close: 4.2f,
                        volume: 4),
                    new Candle(
                        new DateTimeOffset(2021, 05, 22, 09, 32, 00, 0, TimeSpan.Zero),
                        open: 3.3f,
                        high: 3.4f,
                        low: 3.1f,
                        close: 3.2f,
                        volume: 3),
                    new Candle(
                        new DateTimeOffset(2021, 05, 22, 09, 31, 00, 0, TimeSpan.Zero),
                        open: 2.3f,
                        high: 2.4f,
                        low: 2.1f,
                        close: 2.2f,
                        volume: 2),
                    new Candle(
                        new DateTimeOffset(2021, 05, 22, 09, 30, 00, 0, TimeSpan.Zero),
                        open: 1.3f,
                        high: 1.4f,
                        low: 1.1f,
                        close: 1.2f,
                        volume: 1),
                    new Candle(
                        new DateTimeOffset(2021, 05, 22, 09, 29, 00, 0, TimeSpan.Zero),
                        open: 0.3f,
                        high: 0.4f,
                        low: 0.1f,
                        close: 0.2f,
                        volume: 0)));

            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 22, 09, 28, 00, 0, TimeSpan.Zero), Array.Empty<Candle>());

            var expected = new[]
            {
                new Candle(new DateTimeOffset(2021, 05, 22, 09, 29, 00, 0, TimeSpan.Zero), open: 0.3f, high: 0.4f, low: 0.1f, close: 0.2f, volume: 0),
            };
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 22, 09, 29, 00, 0, TimeSpan.Zero), expected);

            expected = new[]
            {
                new Candle(new DateTimeOffset(2021, 05, 22, 09, 30, 00, 0, TimeSpan.Zero), open: 1.3f, high: 1.4f, low: 1.1f, close: 1.2f, volume: 1),
                new Candle(new DateTimeOffset(2021, 05, 22, 09, 29, 00, 0, TimeSpan.Zero), open: 0.3f, high: 0.4f, low: 0.1f, close: 0.2f, volume: 0),
            };

            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 22, 09, 30, 00, 0, TimeSpan.Zero), expected);

            expected = new[]
            {
                new Candle(new DateTimeOffset(2021, 05, 22, 09, 31, 00, 0, TimeSpan.Zero), open: 1.3f, high: 2.4f, low: 1.1f, close: 2.2f, volume: 3),
                new Candle(new DateTimeOffset(2021, 05, 22, 09, 29, 00, 0, TimeSpan.Zero), open: 0.3f, high: 0.4f, low: 0.1f, close: 0.2f, volume: 0),
            };
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 22, 09, 31, 00, 0, TimeSpan.Zero), expected);

            expected = new[]
            {
                new Candle(new DateTimeOffset(2021, 05, 22, 09, 32, 00, 0, TimeSpan.Zero), open: 1.3f, high: 3.4f, low: 1.1f, close: 3.2f, volume: 6),
                new Candle(new DateTimeOffset(2021, 05, 22, 09, 29, 00, 0, TimeSpan.Zero), open: 0.3f, high: 0.4f, low: 0.1f, close: 0.2f, volume: 0),
            };
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 22, 09, 32, 00, 0, TimeSpan.Zero), expected);

            expected = new[]
            {
                new Candle(new DateTimeOffset(2021, 05, 22, 10, 00, 00, 0, TimeSpan.Zero), open: 4.3f, high: 4.4f, low: 4.1f, close: 4.2f, volume: 4),
                new Candle(new DateTimeOffset(2021, 05, 22, 09, 32, 00, 0, TimeSpan.Zero), open: 1.3f, high: 3.4f, low: 1.1f, close: 3.2f, volume: 6),
                new Candle(new DateTimeOffset(2021, 05, 22, 09, 29, 00, 0, TimeSpan.Zero), open: 0.3f, high: 0.4f, low: 0.1f, close: 0.2f, volume: 0),
            };
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 22, 10, 00, 00, 0, TimeSpan.Zero), expected);

            expected = new[]
            {
                new Candle(new DateTimeOffset(2021, 05, 22, 10, 01, 00, 0, TimeSpan.Zero), open: 4.3f, high: 5.4f, low: 4.1f, close: 5.2f, volume: 9),
                new Candle(new DateTimeOffset(2021, 05, 22, 09, 32, 00, 0, TimeSpan.Zero), open: 1.3f, high: 3.4f, low: 1.1f, close: 3.2f, volume: 6),
                new Candle(new DateTimeOffset(2021, 05, 22, 09, 29, 00, 0, TimeSpan.Zero), open: 0.3f, high: 0.4f, low: 0.1f, close: 0.2f, volume: 0),
            };
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 22, 10, 01, 00, 0, TimeSpan.Zero), expected);
        }

        private static IEnumerable<TestCaseData> DaysSource()
        {
            var candles = new Candles(
                default,
                Create(
                    new Candle(
                        new DateTimeOffset(2021, 05, 22, 16, 00, 00, 0, TimeSpan.Zero),
                        open: 7.3f,
                        high: 7.4f,
                        low: 7.1f,
                        close: 7.2f,
                        volume: 7),
                    new Candle(
                        new DateTimeOffset(2021, 05, 22, 15, 59, 00, 0, TimeSpan.Zero),
                        open: 6.3f,
                        high: 6.4f,
                        low: 6.1f,
                        close: 6.2f,
                        volume: 6),
                    new Candle(
                        new DateTimeOffset(2021, 05, 22, 10, 01, 00, 0, TimeSpan.Zero),
                        open: 5.3f,
                        high: 5.4f,
                        low: 5.1f,
                        close: 5.2f,
                        volume: 5),
                    new Candle(
                        new DateTimeOffset(2021, 05, 22, 10, 00, 00, 0, TimeSpan.Zero),
                        open: 4.3f,
                        high: 4.4f,
                        low: 4.1f,
                        close: 4.2f,
                        volume: 4),
                    new Candle(
                        new DateTimeOffset(2021, 05, 22, 09, 32, 00, 0, TimeSpan.Zero),
                        open: 3.3f,
                        high: 3.4f,
                        low: 3.1f,
                        close: 3.2f,
                        volume: 3),
                    new Candle(
                        new DateTimeOffset(2021, 05, 22, 09, 31, 00, 0, TimeSpan.Zero),
                        open: 2.3f,
                        high: 2.4f,
                        low: 2.1f,
                        close: 2.2f,
                        volume: 2),
                    new Candle(
                        new DateTimeOffset(2021, 05, 22, 09, 30, 00, 0, TimeSpan.Zero),
                        open: 1.3f,
                        high: 1.4f,
                        low: 1.1f,
                        close: 1.2f,
                        volume: 1)));

            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 22, 09, 29, 00, 0, TimeSpan.Zero), Array.Empty<Candle>());
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 22, 09, 30, 00, 0, TimeSpan.Zero), Array.Empty<Candle>());
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 22, 09, 31, 00, 0, TimeSpan.Zero), new[] { new Candle(new DateTimeOffset(2021, 05, 22, 09, 31, 00, 0, TimeSpan.Zero), open: 2.3f, high: 2.4f, low: 2.1f, close: 2.2f, volume: 2), });
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 22, 09, 32, 00, 0, TimeSpan.Zero), new[] { new Candle(new DateTimeOffset(2021, 05, 22, 09, 32, 00, 0, TimeSpan.Zero), open: 2.3f, high: 3.4f, low: 2.1f, close: 3.2f, volume: 5), });
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 22, 15, 59, 00, 0, TimeSpan.Zero), new[] { new Candle(new DateTimeOffset(2021, 05, 22, 15, 59, 00, 0, TimeSpan.Zero), open: 2.3f, high: 6.4f, low: 2.1f, close: 6.2f, volume: 20), });
            yield return new TestCaseData(candles, new DateTimeOffset(2021, 05, 22, 16, 00, 00, 0, TimeSpan.Zero), new[] { new Candle(new DateTimeOffset(2021, 05, 22, 16, 00, 00, 0, TimeSpan.Zero), open: 2.3f, high: 7.4f, low: 2.1f, close: 7.2f, volume: 27), });
        }

        private static DescendingCandles Create(params Candle[] candles)
        {
            var builder = DescendingCandles.CreateBuilder();
            foreach (var candle in candles)
            {
                builder.Add(candle);
            }

            return builder.Create();
        }
    }
}
