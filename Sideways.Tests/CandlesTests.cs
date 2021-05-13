﻿namespace Sideways.Tests
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

        [TestCaseSource(nameof(SkipMinutesSource))]
        public static void SkipMinutes(Candles candles, DateTimeOffset time, int count, DateTimeOffset expected)
        {
            Assert.AreEqual(expected, candles.Skip(time, CandleInterval.Minute, count));
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
    }
}